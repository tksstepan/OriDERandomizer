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

// Token: 0x02000953 RID: 2387
public class GameController : SaveSerialize, ISuspendable
{
	// Token: 0x0600340F RID: 13327 RVA: 0x000D67D0 File Offset: 0x000D49D0
	public GameController()
	{
	}

	// Token: 0x06003410 RID: 13328 RVA: 0x000295FD File Offset: 0x000277FD
	static GameController()
	{
	}

	// Token: 0x17000812 RID: 2066
	// (get) Token: 0x06003411 RID: 13329 RVA: 0x00029605 File Offset: 0x00027805
	// (set) Token: 0x06003412 RID: 13330 RVA: 0x0002960D File Offset: 0x0002780D
	public bool MainMenuCanBeOpened { get; set; }

	// Token: 0x17000813 RID: 2067
	// (get) Token: 0x06003413 RID: 13331 RVA: 0x00029616 File Offset: 0x00027816
	public int GameTimeInSeconds
	{
		get
		{
			return Mathf.RoundToInt(this.Timer.CurrentTime);
		}
	}

	// Token: 0x06003414 RID: 13332
	public void PerformSaveGameSequence()
	{
		RandomizerStatsManager.OnSave(false);
		if (this.GameSaveSequence)
		{
			this.GameSaveSequence.Perform(null);
		}
	}

	// Token: 0x17000814 RID: 2068
	// (get) Token: 0x06003415 RID: 13333 RVA: 0x00029643 File Offset: 0x00027843
	public bool IsPackageFullyInstalled
	{
		get
		{
			return !DebugMenuB.IsFullyInstalledDebugOverride;
		}
	}

	// Token: 0x17000815 RID: 2069
	// (get) Token: 0x06003416 RID: 13334 RVA: 0x00029652 File Offset: 0x00027852
	public bool IsTrial
	{
		get
		{
			return this.PCTrialValue;
		}
	}

	// Token: 0x17000816 RID: 2070
	// (get) Token: 0x06003417 RID: 13335 RVA: 0x000D6860 File Offset: 0x000D4A60
	public bool IsDemo
	{
		get
		{
			WorldEventsRuntime worldEventsRuntime = World.Events.Find(this.DebugWorldEvents);
			return worldEventsRuntime.Value == this.DebugWorldEvents.GetIDFromName("Demo");
		}
	}

	// Token: 0x06003418 RID: 13336 RVA: 0x0002965A File Offset: 0x0002785A
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

	// Token: 0x06003419 RID: 13337 RVA: 0x000259D7 File Offset: 0x00023BD7
	public void ExitTrial()
	{
		GameController.Instance.RestartGame();
	}

	// Token: 0x0600341A RID: 13338 RVA: 0x0001E678 File Offset: 0x0001C878
	public void QuitApplication()
	{
		Application.Quit();
	}

	// Token: 0x0600341B RID: 13339 RVA: 0x000D6894 File Offset: 0x000D4A94
	public void GoToEndTrialScreen()
	{
		this.MainMenuCanBeOpened = false;
		GameStateMachine.Instance.SetToTrialEnd();
		RuntimeSceneMetaData sceneInformation = Scenes.Manager.GetSceneInformation("trialEndScreen");
		GoToSceneController.Instance.GoToScene(sceneInformation, new Action(this.OnFinishedLoadingTrialEndScene), false);
	}

	// Token: 0x0600341C RID: 13340 RVA: 0x00029680 File Offset: 0x00027880
	public void OnFinishedLoadingTrialEndScene()
	{
		this.RemoveGameplayObjects();
	}

	// Token: 0x0600341D RID: 13341 RVA: 0x00029688 File Offset: 0x00027888
	public void OnGameReset()
	{
		SaveSlotsManager.BackupIndex = -1;
		TriggerByString.OnGameReset();
		SeinLevel.HasSpentSkillPoint = false;
		WorldEventsManager.Instance.OnGameReset();
		SoundPlayer.DestroyAll();
	}

	// Token: 0x0600341E RID: 13342 RVA: 0x000D68DC File Offset: 0x000D4ADC
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

	// Token: 0x0600341F RID: 13343 RVA: 0x000296AA File Offset: 0x000278AA
	public void ResetStateForDebugMenuGoToScene()
	{
		this.RemoveGameplayObjects();
		this.RequireInitialValues = true;
	}

	// Token: 0x06003420 RID: 13344 RVA: 0x000D69C4 File Offset: 0x000D4BC4
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

	// Token: 0x06003421 RID: 13345 RVA: 0x000296B9 File Offset: 0x000278B9
	private void OnFinishedRestarting()
	{
		base.StartCoroutine(this.RestartingCleanupNextFrame());
	}

	// Token: 0x06003422 RID: 13346 RVA: 0x000D6A3C File Offset: 0x000D4C3C
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

	// Token: 0x17000817 RID: 2071
	// (get) Token: 0x06003423 RID: 13347 RVA: 0x000296C8 File Offset: 0x000278C8
	// (set) Token: 0x06003424 RID: 13348 RVA: 0x000296D0 File Offset: 0x000278D0
	public bool GameplaySuspended { get; set; }

	// Token: 0x17000818 RID: 2072
	// (get) Token: 0x06003425 RID: 13349 RVA: 0x000296D9 File Offset: 0x000278D9
	// (set) Token: 0x06003426 RID: 13350 RVA: 0x000296E1 File Offset: 0x000278E1
	public bool GameplaySuspendedForUI { get; set; }

	// Token: 0x17000819 RID: 2073
	// (get) Token: 0x06003427 RID: 13351 RVA: 0x000296EA File Offset: 0x000278EA
	public bool InputLocked
	{
		get
		{
			return this.LockInput || this.LockInputByAction;
		}
	}

	// Token: 0x1700081A RID: 2074
	// (get) Token: 0x06003428 RID: 13352 RVA: 0x00029700 File Offset: 0x00027900
	// (set) Token: 0x06003429 RID: 13353 RVA: 0x00029708 File Offset: 0x00027908
	public bool LockInputByAction { get; set; }

	// Token: 0x1700081B RID: 2075
	// (get) Token: 0x0600342A RID: 13354 RVA: 0x00029711 File Offset: 0x00027911
	// (set) Token: 0x0600342B RID: 13355 RVA: 0x00029719 File Offset: 0x00027919
	public bool LockInput { get; set; }

	// Token: 0x0600342C RID: 13356 RVA: 0x000D6A58 File Offset: 0x000D4C58
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

	// Token: 0x0600342D RID: 13357 RVA: 0x000D6B14 File Offset: 0x000D4D14
	public override void Awake()
	{
		if (GameController.Instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		GameController.Instance = this;
		this.HandleTrialData();
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

	// Token: 0x0600342E RID: 13358 RVA: 0x000D6CAC File Offset: 0x000D4EAC
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

	// Token: 0x0600342F RID: 13359 RVA: 0x000D6CF8 File Offset: 0x000D4EF8
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

	// Token: 0x06003430 RID: 13360 RVA: 0x000D6D14 File Offset: 0x000D4F14
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
		GameController.IsFocused = focusStatus;
	}
	
	// Token: 0x06003431 RID: 13361 RVA: 0x000D6D78 File Offset: 0x000D4F78
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

	// Token: 0x06003432 RID: 13362 RVA: 0x000D6D94 File Offset: 0x000D4F94
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

	// Token: 0x06003433 RID: 13363 RVA: 0x00029722 File Offset: 0x00027922
	public override void OnDestroy()
	{
		InstantiateUtility.Destroy(this.m_systemsGameObject);
		SuspensionManager.Unregister(this);
		base.OnDestroy();
	}

	// Token: 0x06003434 RID: 13364 RVA: 0x0002973B File Offset: 0x0002793B
	public void ResetInputLocks()
	{
		this.LockInputByAction = false;
		this.LockInput = false;
	}

	// Token: 0x06003435 RID: 13365 RVA: 0x000D6DB8 File Offset: 0x000D4FB8
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

	// Token: 0x06003436 RID: 13366 RVA: 0x000D6E34 File Offset: 0x000D5034
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

	// Token: 0x06003437 RID: 13367 RVA: 0x0002974B File Offset: 0x0002794B
	public void SetupGameplay(SceneRoot sceneRoot, WorldEventsOnAwake worldEventsOnAwake)
	{
		sceneRoot.MetaData.InitialValues.ApplyInitialValues();
		this.WarmUpResources();
		if (worldEventsOnAwake != null)
		{
			worldEventsOnAwake.Apply();
		}
		LateStartHook.AddLateStartMethod(new Action(this.CreateCheckpoint));
	}

	// Token: 0x06003438 RID: 13368 RVA: 0x00029786 File Offset: 0x00027986
	public void OnApplicationQuit()
	{
		GameController.IsClosing = true;
		if (this.m_logCallbackHandler != null)
		{
			this.m_logCallbackHandler.FlushEntriesToFile(this.m_logOutputFile);
		}
		MoonDebug.OnApplicationQuit();
	}

	// Token: 0x06003439 RID: 13369 RVA: 0x000297AF File Offset: 0x000279AF
	public void Update()
	{
		Randomizer.Update();
		if ((MoonInput.GetKey(KeyCode.LeftAlt) || MoonInput.GetKey(KeyCode.RightAlt)) && MoonInput.GetKeyDown(KeyCode.U))
		{
			UI.SeinUI.ShowUI = true;
			SeinUI.DebugHideUI = !SeinUI.DebugHideUI;
		}
	}

	// Token: 0x0600343A RID: 13370 RVA: 0x000030E7 File Offset: 0x000012E7
	private static void CheckPackageFullyInstalled()
	{
	}

	// Token: 0x0600343B RID: 13371 RVA: 0x000D6E84 File Offset: 0x000D5084
	public void FixedUpdate()
	{
		if (Scenes.Manager)
		{
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

	// Token: 0x0600343C RID: 13372 RVA: 0x000297EF File Offset: 0x000279EF
	public Objective GetObjectiveFromIndex(int index)
	{
		if (this.Objectives.Count > index && index >= 0)
		{
			return this.Objectives[index];
		}
		return null;
	}

	// Token: 0x0600343D RID: 13373 RVA: 0x00029817 File Offset: 0x00027A17
	public int GetObjectiveIndex(Objective objective)
	{
		return this.Objectives.IndexOf(objective);
	}

	// Token: 0x0600343E RID: 13374 RVA: 0x000D6F78 File Offset: 0x000D5178
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

	// Token: 0x0600343F RID: 13375 RVA: 0x00029825 File Offset: 0x00027A25
	public void ResumeGameplay()
	{
		if (this.GameplaySuspended)
		{
			SuspensionManager.ResumeExcluding(this.m_suspendablesToIgnoreForGameplay);
			this.m_suspendablesToIgnoreForGameplay.Clear();
			this.GameplaySuspended = false;
		}
	}

	// Token: 0x06003440 RID: 13376 RVA: 0x0002984F File Offset: 0x00027A4F
	public void SuspendGameplayForUI()
	{
		if (!this.GameplaySuspendedForUI)
		{
			SuspensionManager.SuspendAll();
			this.GameplaySuspendedForUI = true;
		}
	}

	// Token: 0x06003441 RID: 13377 RVA: 0x00029868 File Offset: 0x00027A68
	public void ResumeGameplayForUI()
	{
		if (this.GameplaySuspendedForUI)
		{
			SuspensionManager.ResumeAll();
			this.GameplaySuspendedForUI = false;
		}
	}

	// Token: 0x06003442 RID: 13378 RVA: 0x000D6FC4 File Offset: 0x000D51C4
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

	// Token: 0x06003443 RID: 13379 RVA: 0x00029881 File Offset: 0x00027A81
	public void ClearCheckpointData()
	{
		Game.Checkpoint.SaveGameData.ClearAllData();
	}

	// Token: 0x06003444 RID: 13380 RVA: 0x0002988D File Offset: 0x00027A8D
	public void RestoreCheckpoint(Action onFinished = null)
	{
		this.IsLoadingGame = true;
		this.m_onRestoreCheckpointFinished = onFinished;
		LateStartHook.AddLateStartMethod(new Action(this.RestoreCheckpointImmediate));
	}

	// Token: 0x06003445 RID: 13381 RVA: 0x000298AE File Offset: 0x00027AAE
	public void RestoreCheckpointImmediate()
	{
		this.m_restoreCheckpointController.RestoreCheckpoint();
		if (this.m_onRestoreCheckpointFinished != null)
		{
			this.m_onRestoreCheckpointFinished();
			this.m_onRestoreCheckpointFinished = null;
		}
	}

	// Token: 0x06003446 RID: 13382 RVA: 0x000D708C File Offset: 0x000D528C
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

	// Token: 0x06003447 RID: 13383 RVA: 0x000030E7 File Offset: 0x000012E7
	[Conditional("NOT_FINAL_BUILD")]
	private void HandleBuildName()
	{
	}

	// Token: 0x06003448 RID: 13384 RVA: 0x000030E7 File Offset: 0x000012E7
	[Conditional("NOT_FINAL_BUILD")]
	private void HandleCommands()
	{
	}

	// Token: 0x06003449 RID: 13385 RVA: 0x000030E7 File Offset: 0x000012E7
	[Conditional("NOT_FINAL_BUILD")]
	private void HandleBuildIDString()
	{
	}

	// Token: 0x1700081C RID: 2076
	// (get) Token: 0x0600344A RID: 13386 RVA: 0x000298D8 File Offset: 0x00027AD8
	public bool GameInTitleScreen
	{
		get
		{
			return GameStateMachine.Instance.CurrentState == GameStateMachine.State.TitleScreen || GameStateMachine.Instance.CurrentState == GameStateMachine.State.StartScreen;
		}
	}

	// Token: 0x1700081D RID: 2077
	// (get) Token: 0x0600344B RID: 13387 RVA: 0x000298FA File Offset: 0x00027AFA
	// (set) Token: 0x0600344C RID: 13388 RVA: 0x00029902 File Offset: 0x00027B02
	public bool IsSuspended { get; set; }

	// Token: 0x1700081E RID: 2078
	// (get) Token: 0x0600344D RID: 13389 RVA: 0x0002990B File Offset: 0x00027B0B
	// (set) Token: 0x0600344E RID: 13390 RVA: 0x00029913 File Offset: 0x00027B13
	public bool PreventFocusPause { get; set; }

	// Token: 0x04002ECA RID: 11978
	public const string TitleScreenSceneName = "titleScreenSwallowsNest";

	// Token: 0x04002ECB RID: 11979
	public const string TrialEndScreenSceneName = "trialEndScreen";

	// Token: 0x04002ECC RID: 11980
	public const string IntroLogosSceneName = "introLogos";

	// Token: 0x04002ECD RID: 11981
	public const string TrailerSceneName = "trailerScene";

	// Token: 0x04002ECE RID: 11982
	public const string WorldMapSceneName = "worldMapScene";

	// Token: 0x04002ECF RID: 11983
	public const string EmptyTestSceneName = "emptyTestScene";

	// Token: 0x04002ED0 RID: 11984
	public const string BootLoadSceneName = "loadBootstrap";

	// Token: 0x04002ED1 RID: 11985
	public const string GameStartScene = "sunkenGladesRunaway";

	// Token: 0x04002ED2 RID: 11986
	public GameTimer Timer;

	// Token: 0x04002ED3 RID: 11987
	public static GameController Instance;

	// Token: 0x04002ED4 RID: 11988
	public static bool FreezeFixedUpdate;

	// Token: 0x04002ED5 RID: 11989
	public static bool IsClosing;

	// Token: 0x04002ED6 RID: 11990
	public SaveGameController SaveGameController = new SaveGameController();

	// Token: 0x04002ED7 RID: 11991
	public List<GameObject> Systems = new List<GameObject>();

	// Token: 0x04002ED8 RID: 11992
	public GameScheduler GameScheduler = new GameScheduler();

	// Token: 0x04002ED9 RID: 11993
	public AllContainer<Objective> ActiveObjectives = new AllContainer<Objective>();

	// Token: 0x04002EDA RID: 11994
	public List<Objective> Objectives = new List<Objective>();

	// Token: 0x04002EDB RID: 11995
	public string BuildIDString = string.Empty;

	// Token: 0x04002EDC RID: 11996
	public string BuildName = string.Empty;

	// Token: 0x04002EDD RID: 11997
	public UberAtlassingPlatform AtlasPlatform;

	// Token: 0x04002EDE RID: 11998
	private HashSet<ISuspendable> m_suspendablesToIgnoreForGameplay = new HashSet<ISuspendable>();

	// Token: 0x04002EDF RID: 11999
	private GameObject m_systemsGameObject;

	// Token: 0x04002EE0 RID: 12000
	private LogCallbackHandler m_logCallbackHandler;

	// Token: 0x04002EE1 RID: 12001
	private RestoreCheckpointController m_restoreCheckpointController = new RestoreCheckpointController();

	// Token: 0x04002EE2 RID: 12002
	public int VSyncCount = 1;

	// Token: 0x04002EE3 RID: 12003
	private string m_logOutputFile = string.Empty;

	// Token: 0x04002EE4 RID: 12004
	public float GameTime;

	// Token: 0x04002EE5 RID: 12005
	public ActionSequence GameSaveSequence;

	// Token: 0x04002EE6 RID: 12006
	public static bool IsFocused = true;

	// Token: 0x04002EE7 RID: 12007
	private static volatile bool m_isPackageFullyInstalled;

	// Token: 0x04002EE8 RID: 12008
	public bool PCTrialValue;

	// Token: 0x04002EE9 RID: 12009
	public bool EditorTrialValue;

	// Token: 0x04002EEA RID: 12010
	public WorldEvents DebugWorldEvents;

	// Token: 0x04002EEB RID: 12011
	private bool m_isRestartingGame;

	// Token: 0x04002EEC RID: 12012
	private bool m_setRunInBackgroundToFalse;

	// Token: 0x04002EED RID: 12013
	public bool RequireInitialValues = true;

	// Token: 0x04002EEE RID: 12014
	public bool IsLoadingGame;

	// Token: 0x04002EEF RID: 12015
	public List<UnityEngine.Object> Resources;

	// Token: 0x04002EF0 RID: 12016
	private bool m_lastDebugControlsEnabledValue;

	// Token: 0x04002EF1 RID: 12017
	private int m_previousScreenWidth;

	// Token: 0x04002EF2 RID: 12018
	private int m_previousScreenHeight;

	// Token: 0x04002EF3 RID: 12019
	private float m_isPackageFullyInstalledTimer;

	// Token: 0x04002EF4 RID: 12020
	private Action m_onRestoreCheckpointFinished;
	
	// Token: 0x04003522 RID: 13602
	private int CurVsyncValue;
}
