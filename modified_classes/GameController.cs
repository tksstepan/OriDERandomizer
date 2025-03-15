using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core;
using Frameworks;
using Game;
using UnityEngine;

public class GameController : SaveSerialize, ISuspendable
{
	public bool MainMenuCanBeOpened { get; set; }

	public int GameTimeInSeconds
	{
		get
		{
			return Mathf.RoundToInt(this.Timer.CurrentTime);
		}
	}

	public void PerformSaveGameSequence()
	{
		RandomizerStatsManager.OnSave(false);
		if (this.GameSaveSequence)
		{
			this.GameSaveSequence.Perform(null);
		}
	}

	public bool IsPackageFullyInstalled
	{
		get
		{
			return !DebugMenuB.IsFullyInstalledDebugOverride;
		}
	}

	public bool IsTrial
	{
		get
		{
			return this.PCTrialValue;
		}
	}

	public bool IsDemo
	{
		get
		{
			WorldEventsRuntime worldEventsRuntime = World.Events.Find(this.DebugWorldEvents);
			return worldEventsRuntime.Value == this.DebugWorldEvents.GetIDFromName("Demo");
		}
	}

	public void ExitGame()
	{
		if (this.IsTrial)
		{
			GameController.Instance.GoToEndTrialScreen();
		}
		else
		{
			GameController.Instance.QuitApplication();
		}
	}

	public void ExitTrial()
	{
		GameController.Instance.RestartGame();
	}

	public void QuitApplication()
	{
		Application.Quit();
	}

	public void GoToEndTrialScreen()
	{
		this.MainMenuCanBeOpened = false;
		GameStateMachine.Instance.SetToTrialEnd();
		RuntimeSceneMetaData sceneInformation = Scenes.Manager.GetSceneInformation("trialEndScreen");
		GoToSceneController.Instance.GoToScene(sceneInformation, new Action(this.OnFinishedLoadingTrialEndScene), false);
	}

	public void OnFinishedLoadingTrialEndScene()
	{
		this.RemoveGameplayObjects();
	}

	public void OnGameReset()
	{
		SaveSlotsManager.BackupIndex = -1;
		TriggerByString.OnGameReset();
		SeinLevel.HasSpentSkillPoint = false;
		WorldEventsManager.Instance.OnGameReset();
		SoundPlayer.DestroyAll();
	}

	public void RemoveGameplayObjects()
	{
		CharacterFactory.Instance.DestroyCharacter();
		if (Characters.Sein)
		{
			InstantiateUtility.Destroy(Characters.Sein.gameObject);
		}
		if (Characters.Naru)
		{
			InstantiateUtility.Destroy(Characters.Naru.gameObject);
		}
		if (Characters.BabySein)
		{
			InstantiateUtility.Destroy(Characters.BabySein.gameObject);
		}
		if (Characters.Ori)
		{
			InstantiateUtility.Destroy(Characters.Ori.gameObject);
		}
		if (UI.SeinUI)
		{
			InstantiateUtility.Destroy(UI.SeinUI.gameObject);
		}
		Core.SoundComposition.Manager.StopMusic();
		UI.Cameras.Current.Target = null;
		if (UI.MainMenuVisible)
		{
			UI.Menu.HideMenuScreen(false);
		}
		UI.Menu.RemoveGameplayObjects();
		WorldMapUI.CancelLoading();
	}

	public void ResetStateForDebugMenuGoToScene()
	{
		this.RemoveGameplayObjects();
		this.RequireInitialValues = true;
	}

	public void RestartGame()
	{
		if (this.m_isRestartingGame)
		{
			return;
		}
		RuntimeSceneMetaData sceneInformation = Scenes.Manager.GetSceneInformation("titleScreenSwallowsNest");
		if (sceneInformation == null)
		{
			return;
		}
		this.Timer.Reset();
		this.MainMenuCanBeOpened = false;
		this.RequireInitialValues = true;
		GameController.Instance.IsLoadingGame = false;
		InstantLoadScenesController.Instance.OnGameReset();
		GoToSceneController.Instance.GoToScene(sceneInformation, new Action(this.OnFinishedRestarting), false);
	}

	private void OnFinishedRestarting()
	{
		base.StartCoroutine(this.RestartingCleanupNextFrame());
	}

	public IEnumerator RestartingCleanupNextFrame()
	{
		this.RemoveGameplayObjects();
		this.ResetInputLocks();
		if (UI.Fader.IsFadingInOrStay() || UI.Fader.IsTimelineFading())
		{
			UI.Fader.FadeOut(2f);
		}
		XboxLiveController.Instance.Reset();
		XboxOneController.ResetCurrentGamepad();
		XboxOneFlow.Engage = false;
		XboxOneSession.EndSession();
		yield return new WaitForFixedUpdate();
		this.m_isRestartingGame = false;
		this.ActiveObjectives.Clear();
		Game.Checkpoint.SaveGameData = new SaveGameData();
		Events.Scheduler.OnGameSerializeLoad.Call();
		Events.Scheduler.OnGameReset.Call();
		if (UI.Fader.IsFadingInOrStay() || UI.Fader.IsTimelineFading())
		{
			UI.Fader.FadeOut(2f);
		}
		TitleScreenManager.OnReturnToTitleScreen();
		this.CreateCheckpoint();
		yield break;
	}

	public bool GameplaySuspended { get; set; }

	public bool GameplaySuspendedForUI { get; set; }

	public bool InputLocked
	{
		get
		{
			return this.LockInput || this.LockInputByAction;
		}
	}

	public bool LockInputByAction { get; set; }

	public bool LockInput { get; set; }

	[ContextMenu("Print out sizes of SaveSlot")]
	public void PrintOutSizesOfSaveSlot()
	{
		int num = 0;
		foreach (KeyValuePair<MoonGuid, SaveScene> keyValuePair in Game.Checkpoint.SaveGameData.Scenes)
		{
			foreach (SaveObject saveObject in keyValuePair.Value.SaveObjects)
			{
				num += saveObject.Data.MemoryStream.Capacity;
			}
			num += 16;
		}
	}

	public override void Awake()
	{
		if (GameController.Instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		GameController.Instance = this;
		this.HandleTrialData();
		Randomizer.InitializeOnce();
		this.WarmUpResources();
		base.Awake();
		if (LoadingBootstrap.Instance)
		{
			UnityEngine.Object.Destroy(LoadingBootstrap.Instance.gameObject);
		}
		this.GameScheduler.OnGameAwake.Add(new Action(this.OnGameAwake));
		this.GameScheduler.OnGameAwake.Call();
		this.GameScheduler.OnGameReset.Add(new Action(this.OnGameReset));
		UberGCManager.OnGameStart();
		this.m_systemsGameObject = new GameObject("systems");
		Utility.DontAssociateWithAnyScene(this.m_systemsGameObject);
		base.transform.parent = this.m_systemsGameObject.transform;
		foreach (GameObject gameObject in this.Systems)
		{
			try
			{
				if (gameObject)
				{
					GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
					gameObject2.name = gameObject.name;
					gameObject2.transform.SetParentMaintainingLocalTransform(this.m_systemsGameObject.transform);
				}
			}
			catch (Exception ex)
			{
			}
		}
		new Telemetry();
		UI.LoadMessageController();
		this.Systems.Clear();
		Application.targetFrameRate = 60;
		UberGCManager.CollectProactiveFull();
	}

	private void OnGameAwake()
	{
		this.m_restoreCheckpointController = new RestoreCheckpointController();
		Frameworks.Shader.Globals.FogGradientRange = 100f;
		Frameworks.Shader.Globals.FogGradientTexture = Frameworks.Shader.DefaultTextures.Transparent;
		FixedRandom.UpdateValues();
		if (ScenesToSkip.Instance == null)
		{
			new ScenesToSkip();
		}
		SaveSceneManager.Master = base.GetComponent<SaveSceneManager>();
	}

	public IEnumerator Start()
	{
		GameplayCamera currentCamera = UI.Cameras.Current;
		currentCamera.ChangeTargetToCurrentCharacter();
		Scenes.Manager.EnableDisabledScenesAtPosition(false);
		currentCamera.UpdateTargetHelperPosition();
		currentCamera.MoveCameraToTargetPosition();
		currentCamera.OffsetController.UpdateOffset(true);
		currentCamera.MoveCameraToTargetInstantly(true);
		yield return new WaitForFixedUpdate();
		GameSettings.Instance.LoadSettings();
		this.CreateCheckpoint();
		SaveSceneManager.Master.RegisterGameObject(this.m_systemsGameObject);
		SuspensionManager.Register(this);
		if (!this.IsTrial)
		{
			WaitForSaveGameLogic.OnCompletedStatic = (Action)Delegate.Combine(WaitForSaveGameLogic.OnCompletedStatic, new Action(AchievementsLogic.Instance.HandleTrialAchievements));
		}
		yield break;
	}

	private void OnApplicationFocus(bool focusStatus)
	{
		if (focusStatus)
		{
			this.m_setRunInBackgroundToFalse = false;
			Application.runInBackground = true;
			if (this.CurVsyncValue != 0)
			{
				QualitySettings.vSyncCount = this.CurVsyncValue;
				this.CurVsyncValue = 0;
			}
		}
		else if (QualitySettings.vSyncCount != 0)
		{
			this.CurVsyncValue = QualitySettings.vSyncCount;
			QualitySettings.vSyncCount = 0;
		}
	}
	
	private IEnumerator SetRunInBackgroundToTrue()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		if (this.m_setRunInBackgroundToFalse && !this.PreventFocusPause)
		{
			this.m_setRunInBackgroundToFalse = false;
			Application.runInBackground = false;
		}
		yield break;
	}

	private IEnumerator LoadAssets(List<string> assetsToLoad)
	{
		foreach (string assetToLoad in assetsToLoad)
		{
			WWW www = new WWW(assetToLoad);
			yield return www;
			UnityEngine.Object.Instantiate(www.assetBundle.mainAsset);
		}
		yield break;
	}

	public override void OnDestroy()
	{
		InstantiateUtility.Destroy(this.m_systemsGameObject);
		SuspensionManager.Unregister(this);
		base.OnDestroy();
	}

	public void ResetInputLocks()
	{
		this.LockInputByAction = false;
		this.LockInput = false;
	}

	public override void Serialize(Archive ar)
	{
		if (ar.Reading)
		{
			this.ResetInputLocks();
		}
		WorldEventsManager.Instance.Serialize(ar);
		TriggerByString.SerializeStringTriggers(ar);
		ar.Serialize(0f);
		ar.Serialize(ref this.GameTime);
		ar.Serialize(0);
		ar.Serialize(0);
		ar.Serialize(ref this.RequireInitialValues);
		if (ar.Reading)
		{
			this.RequireInitialValues = false;
		}
		Game.Objectives.Serialize(ar);
	}

	public void WarmUpResources()
	{
		Timer timer = new Timer();
		UI.LoadMessageController();
		Orbs.OrbDisplayText.LoadOrbText();
		Attacking.DamageDisplayText.LoadDamageText();
		Sound.LoadAudioParent();
		UberGhostTrail.WarmUpResource();
		MixerManager.WarmUpResource();
		InteractionRotationModifier.WarmUpResource();
		Randomizer.initialize();
		timer.Report("Warming resources");
		this.Resources.Clear();
	}

	public void SetupGameplay(SceneRoot sceneRoot, WorldEventsOnAwake worldEventsOnAwake)
	{
		sceneRoot.MetaData.InitialValues.ApplyInitialValues();
		this.WarmUpResources();
		if (worldEventsOnAwake != null)
		{
			worldEventsOnAwake.Apply();
		}
		Randomizer.SetupNewGame();
		LateStartHook.AddLateStartMethod(new Action(this.CreateCheckpoint));
	}

	public void OnApplicationQuit()
	{
		GameController.IsClosing = true;
		if (this.m_logCallbackHandler != null)
		{
			this.m_logCallbackHandler.FlushEntriesToFile(this.m_logOutputFile);
		}
		MoonDebug.OnApplicationQuit();
		Randomizer.OnApplicationQuit();
	}

	public void Update()
	{
		Randomizer.Update();
		if ((MoonInput.GetKey(KeyCode.LeftAlt) || MoonInput.GetKey(KeyCode.RightAlt)) && MoonInput.GetKeyDown(KeyCode.U))
		{
			UI.SeinUI.ShowUI = true;
			SeinUI.DebugHideUI = !SeinUI.DebugHideUI;
		}
	}

	private static void CheckPackageFullyInstalled()
	{
	}

	public void FixedUpdate()
	{
		if (Scenes.Manager)
		{
			RandomizerBootstrap.FixedUpdate();
			Scenes.Manager.CheckForScenesFinishedLoading();
		}
		if (!GameController.FreezeFixedUpdate)
		{
			FixedRandom.FixedUpdateIndex++;
			FixedRandom.UpdateValues();
		}
		Music.UpdateMusic();
		Ambience.UpdateAmbience();
		this.GameScheduler.OnGameFixedUpdate.Call();
		Respawner.UpdateRespawners();
		if (!GameStateMachine.Instance.IsInExtendedTitleScreen() && !UI.MainMenuVisible && (Screen.width != this.m_previousScreenWidth || Screen.height != this.m_previousScreenHeight))
		{
			UI.Menu.ShowResumeScreen();
		}
		this.m_previousScreenWidth = Screen.width;
		this.m_previousScreenHeight = Screen.height;
		if (this.m_lastDebugControlsEnabledValue != DebugMenuB.DebugControlsEnabled)
		{
			this.m_lastDebugControlsEnabledValue = DebugMenuB.DebugControlsEnabled;
		}
		if (!this.IsSuspended)
		{
			this.GameTime += Time.deltaTime;
		}
	}

	public Objective GetObjectiveFromIndex(int index)
	{
		if (this.Objectives.Count > index && index >= 0)
		{
			return this.Objectives[index];
		}
		return null;
	}

	public int GetObjectiveIndex(Objective objective)
	{
		return this.Objectives.IndexOf(objective);
	}

	public void SuspendGameplay()
	{
		if (!this.GameplaySuspended)
		{
			Component[] suspendables = Characters.Sein.Controller.Suspendables;
			this.m_suspendablesToIgnoreForGameplay = new HashSet<ISuspendable>(suspendables.Cast<ISuspendable>());
			SuspensionManager.SuspendExcluding(this.m_suspendablesToIgnoreForGameplay);
			this.GameplaySuspended = true;
		}
	}

	public void ResumeGameplay()
	{
		if (this.GameplaySuspended)
		{
			SuspensionManager.ResumeExcluding(this.m_suspendablesToIgnoreForGameplay);
			this.m_suspendablesToIgnoreForGameplay.Clear();
			this.GameplaySuspended = false;
		}
	}

	public void SuspendGameplayForUI()
	{
		if (!this.GameplaySuspendedForUI)
		{
			SuspensionManager.SuspendAll();
			this.GameplaySuspendedForUI = true;
		}
	}

	public void ResumeGameplayForUI()
	{
		if (this.GameplaySuspendedForUI)
		{
			SuspensionManager.ResumeAll();
			this.GameplaySuspendedForUI = false;
		}
	}

	public void CreateCheckpoint()
	{
		SaveGameData saveGameData = Game.Checkpoint.SaveGameData;
		SaveSceneManager.Master.SaveWithoutClearing(saveGameData.Master);
		saveGameData.ApplyPendingScenes();
		if (Scenes.Manager)
		{
			foreach (SceneManagerScene sceneManagerScene in Scenes.Manager.ActiveScenes)
			{
				if (sceneManagerScene.IsVisible && sceneManagerScene.HasStartBeenCalled && sceneManagerScene.SceneRoot.SaveSceneManager)
				{
					sceneManagerScene.SceneRoot.SaveSceneManager.Save(saveGameData.InsertScene(sceneManagerScene.MetaData.SceneMoonGuid));
				}
			}
		}
		Game.Checkpoint.Events.OnPostCreate.Call();
	}

	public void ClearCheckpointData()
	{
		Game.Checkpoint.SaveGameData.ClearAllData();
	}

	public void RestoreCheckpoint(Action onFinished = null)
	{
		this.IsLoadingGame = true;
		this.m_onRestoreCheckpointFinished = onFinished;
		LateStartHook.AddLateStartMethod(new Action(this.RestoreCheckpointImmediate));
	}

	public void RestoreCheckpointImmediate()
	{
		this.m_restoreCheckpointController.RestoreCheckpoint();
		if (this.m_onRestoreCheckpointFinished != null)
		{
			this.m_onRestoreCheckpointFinished();
			this.m_onRestoreCheckpointFinished = null;
		}
	}

	private void HandleTrialData()
	{
		if (this.IsTrial)
		{
			return;
		}
		if (OutputFolder.PlayerTrialDataFolderPath == OutputFolder.PlayerDataFolderPath)
		{
			return;
		}
		if (!Directory.Exists(OutputFolder.PlayerTrialDataFolderPath))
		{
			return;
		}
		string[] files = Directory.GetFiles(OutputFolder.PlayerTrialDataFolderPath);
		for (int i = 0; i < files.Length; i++)
		{
			string fileName = Path.GetFileName(files[i]);
			string path = Path.Combine(OutputFolder.PlayerDataFolderPath, fileName);
			if (!File.Exists(path))
			{
				File.Move(files[i], Path.Combine(OutputFolder.PlayerDataFolderPath, fileName));
			}
		}
		if (Directory.GetFiles(OutputFolder.PlayerTrialDataFolderPath).Length == 0)
		{
			Directory.Delete(OutputFolder.PlayerTrialDataFolderPath);
		}
	}

	[Conditional("NOT_FINAL_BUILD")]
	private void HandleBuildName()
	{
	}

	[Conditional("NOT_FINAL_BUILD")]
	private void HandleCommands()
	{
	}

	[Conditional("NOT_FINAL_BUILD")]
	private void HandleBuildIDString()
	{
	}

	public bool GameInTitleScreen
	{
		get
		{
			return GameStateMachine.Instance.CurrentState == GameStateMachine.State.TitleScreen || GameStateMachine.Instance.CurrentState == GameStateMachine.State.StartScreen;
		}
	}

	public bool IsSuspended { get; set; }

	public bool PreventFocusPause { get; set; }

	public const string TitleScreenSceneName = "titleScreenSwallowsNest";

	public const string TrialEndScreenSceneName = "trialEndScreen";

	public const string IntroLogosSceneName = "introLogos";

	public const string TrailerSceneName = "trailerScene";

	public const string WorldMapSceneName = "worldMapScene";

	public const string EmptyTestSceneName = "emptyTestScene";

	public const string BootLoadSceneName = "loadBootstrap";

	public const string GameStartScene = "sunkenGladesRunaway";

	public GameTimer Timer;

	public static GameController Instance;

	public static bool FreezeFixedUpdate;

	public static bool IsClosing;

	public SaveGameController SaveGameController = new SaveGameController();

	public List<GameObject> Systems = new List<GameObject>();

	public GameScheduler GameScheduler = new GameScheduler();

	public AllContainer<Objective> ActiveObjectives = new AllContainer<Objective>();

	public List<Objective> Objectives = new List<Objective>();

	public string BuildIDString = string.Empty;

	public string BuildName = string.Empty;

	public UberAtlassingPlatform AtlasPlatform;

	private HashSet<ISuspendable> m_suspendablesToIgnoreForGameplay = new HashSet<ISuspendable>();

	private GameObject m_systemsGameObject;

	private LogCallbackHandler m_logCallbackHandler;

	private RestoreCheckpointController m_restoreCheckpointController = new RestoreCheckpointController();

	public int VSyncCount = 1;

	private string m_logOutputFile = string.Empty;

	public float GameTime;

	public ActionSequence GameSaveSequence;

	public static bool IsFocused = true;

	private static volatile bool m_isPackageFullyInstalled;

	public bool PCTrialValue;

	public bool EditorTrialValue;

	public WorldEvents DebugWorldEvents;

	private bool m_isRestartingGame;

	private bool m_setRunInBackgroundToFalse;

	public bool RequireInitialValues = true;

	public bool IsLoadingGame;

	public List<UnityEngine.Object> Resources;

	private bool m_lastDebugControlsEnabledValue;

	private int m_previousScreenWidth;

	private int m_previousScreenHeight;

	private float m_isPackageFullyInstalledTimer;

	private Action m_onRestoreCheckpointFinished;
	
	private int CurVsyncValue;
}
