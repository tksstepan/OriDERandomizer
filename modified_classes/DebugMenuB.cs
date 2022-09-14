using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core;
using Game;
using Sein.World;
using UnityEngine;

public class DebugMenuB : SaveSerialize
{
	public static void MakeDebugMenuExist()
	{
		if (DebugMenuB.Instance == null)
		{
			GameObject gameObject = Resources.Load<GameObject>("debugMenu");
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
			Utility.DontAssociateWithAnyScene(gameObject2);
			gameObject2.name = gameObject.name;
		}
	}

	public static void ToggleDebugMenu()
	{
		DebugMenuB.MakeDebugMenuExist();
		if (DebugMenuB.Active)
		{
			if (DebugMenuB.Instance)
			{
				DebugMenuB.Instance.HideDebugMenu();
			}
		}
		else if (DebugMenuB.Instance)
		{
			DebugMenuB.Instance.ShowDebugMenu();
		}
	}

	public void ShowDebugMenu()
	{
		if (DebugMenuB.Active)
		{
			return;
		}
		DebugMenuB.Active = true;
		DebugMenuB.SuspendGameplay();
	}

	public void HideDebugMenu()
	{
		if (!DebugMenuB.Active)
		{
			return;
		}
		DebugMenuB.Active = false;
		DebugMenuB.ResumeGameplay();
	}

	public void Start()
	{
	}

	private static void SuspendGameplay()
	{
		SuspensionManager.GetSuspendables(DebugMenuB.SuspendablesToIgnoreForGameplay, UI.Cameras.Current.GameObject);
		SuspensionManager.SuspendExcluding(DebugMenuB.SuspendablesToIgnoreForGameplay);
	}

	private static void ResumeGameplay()
	{
		SuspensionManager.ResumeExcluding(DebugMenuB.SuspendablesToIgnoreForGameplay);
		DebugMenuB.SuspendablesToIgnoreForGameplay.Clear();
	}

	public override void Awake()
	{
		DebugMenuB.Instance = this;
		if (this.ImportantLevels.Count > 0)
		{
		}
		base.Awake();
		DebugMenuB.Style = this.Skin.FindStyle("debugMenuItem");
		DebugMenuB.SelectedStyle = this.Skin.FindStyle("selectedDebugMenuItem");
		DebugMenuB.PressedStyle = this.Skin.FindStyle("pressedDebugMenuItem");
		DebugMenuB.DebugMenuStyle = this.Skin.FindStyle("debugMenu");
		DebugMenuB.StyleEnabled = this.Skin.FindStyle("debugMenuItemEnabled");
		DebugMenuB.StyleDisabled = this.Skin.FindStyle("debugMenuItemDisabled");
	}

	public bool ReinstantiateOri()
	{
		Vector3 position = Characters.Current.Position;
		CharacterFactory.Instance.DestroyCharacter();
		CharacterFactory.Instance.SpawnCharacter(CharacterFactory.Characters.Sein, null, position, null);
		LateStartHook.AddLateStartMethod(delegate
		{
			Characters.Sein.Position = position;
		});
		return true;
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_cursorIndex);
		ar.Serialize(ref this.m_showGumoSequences);
		ar.Serialize(ref this.m_gumoSequencesCursorIndex);
		ar.Serialize(ref DebugMenuB.DebugControlsEnabled);
		ar.Serialize(ref DebugMenuB.MuteMusic);
		ar.Serialize(ref DebugMenuB.MuteAmbience);
		ar.Serialize(ref DebugMenuB.MuteSoundEffects);
		if (ar.Reading)
		{
			bool flag = false;
			ar.Serialize(ref flag);
			if (flag == DebugMenuB.Active)
			{
				return;
			}
			DebugMenuB.Active = flag;
			if (DebugMenuB.Active)
			{
				this.ShowDebugMenu();
			}
			else
			{
				this.HideDebugMenu();
			}
		}
		else
		{
			ar.Serialize(ref DebugMenuB.Active);
		}
	}

	public bool SendLeaderboard()
	{
		LeaderboardsController.UploadScores();
		return true;
	}

	public bool LoadTestScene()
	{
		GoToSceneController.Instance.GoToScene(this.TestScene, null, false);
		return true;
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public void SendOneSteamTelemetry()
	{
		this.SendSteamTelemetry(1);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	public void SendTenSteamTelemetry()
	{
		this.SendSteamTelemetry(10);
	}

	public void SendSteamTelemetry(int repetition)
	{
		for (int i = 0; i < repetition; i++)
		{
			SteamTelemetry.StringData stringData = new SteamTelemetry.StringData("test #" + i);
			SteamTelemetry.Instance.Send(TelemetryEvent.Test, stringData.ToString());
		}
	}

	public bool DisableArt()
	{
		if (this.m_art == null)
		{
			this.m_art = (from a in UnityEngine.Object.FindObjectsOfType<GameObject>()
			where a.name == "art"
			select a).ToArray<GameObject>();
			foreach (GameObject gameObject in this.m_art)
			{
				if (gameObject)
				{
					gameObject.SetActive(false);
				}
			}
		}
		else
		{
			foreach (GameObject gameObject2 in this.m_art)
			{
				if (gameObject2)
				{
					gameObject2.SetActive(true);
				}
			}
			this.m_art = null;
		}
		return true;
	}

	public bool PrintReadableTextures()
	{
		using (StreamWriter streamWriter = new StreamWriter("texturesYouCanWriteTo.txt"))
		{
			foreach (Texture2D texture2D in Resources.FindObjectsOfTypeAll(typeof(Texture2D)))
			{
				try
				{
					texture2D.GetPixel(0, 0);
					streamWriter.WriteLine(texture2D.name);
				}
				catch (Exception ex)
				{
				}
			}
		}
		return true;
	}

	public bool DisableEnemies()
	{
		if (this.m_enemies == null)
		{
			this.m_enemies = (from a in UnityEngine.Object.FindObjectsOfType<GameObject>()
			where a.name == "enemies"
			select a).ToArray<GameObject>();
			foreach (GameObject gameObject in this.m_enemies)
			{
				if (gameObject)
				{
					gameObject.SetActive(false);
				}
			}
		}
		else
		{
			foreach (GameObject gameObject2 in this.m_enemies)
			{
				if (gameObject2)
				{
					gameObject2.SetActive(true);
				}
			}
			this.m_enemies = null;
		}
		return true;
	}

	public bool DisableAllParticles()
	{
		if (this.m_particleSystems == null)
		{
			this.m_particleSystems = new List<GameObject>();
			ParticleSystem[] array = UnityEngine.Object.FindObjectsOfType<ParticleSystem>();
			foreach (ParticleSystem particleSystem in array)
			{
				if (particleSystem)
				{
					particleSystem.gameObject.SetActive(false);
					this.m_particleSystems.Add(particleSystem.gameObject);
				}
			}
			ParticleEmitter[] array3 = UnityEngine.Object.FindObjectsOfType<ParticleEmitter>();
			foreach (ParticleEmitter particleEmitter in array3)
			{
				if (particleEmitter)
				{
					particleEmitter.gameObject.SetActive(false);
					this.m_particleSystems.Add(particleEmitter.gameObject);
				}
			}
			InstantiateUtility.DisableParticles = true;
		}
		else
		{
			ParticleSystem[] array5 = UnityEngine.Object.FindObjectsOfType<ParticleSystem>();
			foreach (ParticleSystem particleSystem2 in array5)
			{
				if (particleSystem2)
				{
					particleSystem2.gameObject.SetActive(true);
					this.m_particleSystems.Add(particleSystem2.gameObject);
				}
			}
			ParticleEmitter[] array7 = UnityEngine.Object.FindObjectsOfType<ParticleEmitter>();
			foreach (ParticleEmitter particleEmitter2 in array7)
			{
				if (particleEmitter2)
				{
					particleEmitter2.gameObject.SetActive(true);
					this.m_particleSystems.Add(particleEmitter2.gameObject);
				}
			}
			this.m_particleSystems = null;
			InstantiateUtility.DisableParticles = false;
		}
		return true;
	}

	private void BuildMenu()
	{
		this.MenuWidth = (float)Screen.width - this.MenuTopLeftX * 2f;
		this.MenuHeight = (float)Screen.height - this.MenuTopLeftY * 2f - this.VerticalSpace - 30f;
		DebugMenuB.ShouldShowOnlySelectedItem = false;
		this.m_menuList.Clear();
		List<IDebugMenuItem> list = new List<IDebugMenuItem>();
		list.Add(new ActionDebugMenuItem("Save", new Func<bool>(this.SaveGame)));
		list.Add(new ActionDebugMenuItem("Load", new Func<bool>(this.LoadGame)));
		list.Add(new ActionDebugMenuItem("Restore Checkpoint", new Func<bool>(this.RestoreCheckpoint)));
		list.Add(new ActionDebugMenuItem("Instantiate Ori", new Func<bool>(this.ReinstantiateOri)));
		list.Add(new ActionDebugMenuItem("Activate teleporters", new Func<bool>(TeleporterController.ActivateAll)));
		list.Add(new ActionDebugMenuItem("Unlock Difficulties", new Func<bool>(this.UnlockDifficulties)));
		if (SkipCutsceneController.Instance.SkippingAvailable)
		{
			list.Add(new ActionDebugMenuItem("Skipping Available", new Func<bool>(this.SkipAction)));
		}
		list.Add(new BoolDebugMenuItem("Cheats", new Func<bool>(this.CheatsGetter), new Action<bool>(this.CheatsSetter)));
		list.Add(new BoolDebugMenuItem("Disable Sound", () => Sound.AllSoundsDisabled, delegate(bool val)
		{
			Sound.AllSoundsDisabled = val;
		}));
		list.Add(new BoolDebugMenuItem("Unlock Cutscenes", () => DebugMenuB.UnlockAllCutscenes, delegate(bool value)
		{
			DebugMenuB.UnlockAllCutscenes = value;
		}));
		list.Add(new BoolDebugMenuItem("Frame Performance Monitor", () => FramePerformanceMonitor.Enabled, delegate(bool val)
		{
			SceneFrameworkPerformanceMonitor.Enabled = val;
			FramePerformanceMonitor.Enabled = val;
		}));
		list.Add(new BoolDebugMenuItem("Binary Profiler Log", () => BinaryProfilerLogMaker.Enabled, delegate(bool val)
		{
			BinaryProfilerLogMaker.Enabled = val;
		}));
		list.Add(new BoolDebugMenuItem("Leaked Objects Detector", () => LeakedSceneObjectDetector.Enabled, delegate(bool val)
		{
			LeakedSceneObjectDetector.Enabled = val;
		}));
		list.Add(new BoolDebugMenuItem("UberShader Detector", () => UberShaderDetector.Enabled, delegate(bool val)
		{
			UberShaderDetector.Enabled = val;
		}));
		list.Add(new BoolDebugMenuItem("Debug Controls", new Func<bool>(this.DebugControlsGetter), new Action<bool>(this.DebugControlsSetter)));
		list.Add(new BoolDebugMenuItem("Debug text", new Func<bool>(this.DebugTextGetter), new Action<bool>(this.DebugTextSetter)));
		list.Add(new BoolDebugMenuItem("Scene Framework", new Func<bool>(this.DebugSceneFrameworkGetter), new Action<bool>(this.DebugSceneFrameworkSetter)));
		list.Add(new BoolDebugMenuItem("Xbox Controller", new Func<bool>(this.DebugXboxControllerGetter), new Action<bool>(this.DebugXboxControllerSetter)));
		list.Add(new BoolDebugMenuItem("Visual Log", new Func<bool>(this.VisualLogGetter), new Action<bool>(this.VisualLogSetter)));
		list.Add(new BoolDebugMenuItem("Log Callback Hook", new Func<bool>(this.LogCallbackHookGetter), new Action<bool>(this.LogCallbackHookSetter)));
		list.Add(new BoolDebugMenuItem("Fixed Update Sync Debug", new Func<bool>(this.FixedUpdateSyncGetter), new Action<bool>(this.FixedUpdateSyncSetter)));
		list.Add(new ActionDebugMenuItem("Print Object report", new Func<bool>(YouCanLeaveYourHatOn.DebugMenuPrintReport)));
		list.Add(new ActionDebugMenuItem("Disable particles", new Func<bool>(this.DisableAllParticles)));
		list.Add(new ActionDebugMenuItem("Disable art", new Func<bool>(this.DisableArt)));
		list.Add(new ActionDebugMenuItem("Disable enemies", new Func<bool>(this.DisableEnemies)));
		list.Add(new ActionDebugMenuItem("Print readable textures", new Func<bool>(this.PrintReadableTextures)));
		list.Add(new GarbageRunner());
		list.Add(new ActionDebugMenuItem("Reset Steam Stats", new Func<bool>(this.ResetSteamStats)));
		list.Add(new ActionDebugMenuItem("Reset Input Lock", new Func<bool>(this.ResetInputLock)));
		if (Characters.Sein)
		{
			list.Add(new ActionDebugMenuItem("Reset berry position", new Func<bool>(this.ResetNightBerryPosition)));
			list.Add(new ActionDebugMenuItem("Teleport Nightberry", new Func<bool>(this.TeleportNightberry)));
			list.Add(new ActionDebugMenuItem("Visit all spots in current area", new Func<bool>(this.VisitAllAreas)));
		}
		list.Add(new BoolDebugMenuItem("See Achievement Hint", new Func<bool>(this.AchievementHintGetter), new Action<bool>(this.AchievementHintSetter)));
		list.Add(new ActionDebugMenuItem("Gumo Sequences", new Func<bool>(this.GumoSequencesAction)));
		list.Add(new ActionDebugMenuItem("Quit", new Func<bool>(this.Quit)));
		List<IDebugMenuItem> list2 = new List<IDebugMenuItem>();
		list2.Add(new TimeScaleDebugMenuItem("Time Scale"));
		list2.Add(new ZoomDebugMenuItem("Zoom"));
		list2.Add(new GlobalDebugQuadScaleMenuItem("Quad scale"));
		list2.Add(new BoolDebugMenuItem("Super Slow Motion", () => this.m_superSlowMotion, delegate(bool val)
		{
			this.m_superSlowMotion = val;
			Time.timeScale = ((!val) ? 1f : 0.25f);
		}));
		list2.Add(new BoolDebugMenuItem("Sync fixed update", () => SyncFramesTest.EnableSync, delegate(bool val)
		{
			SyncFramesTest.EnableSync = val;
		}));
		list2.Add(new BoolDebugMenuItem("force fixed update", () => SyncFramesTest.EnabledForceFixedUpdate, delegate(bool val)
		{
			SyncFramesTest.EnabledForceFixedUpdate = val;
		}));
		if (Characters.Sein)
		{
			list2.Add(new SeinLevelUpDownDebugMenuItem("Level"));
			list2.Add(new SeinSkillUpDownDebugMenuItem("Skill Points"));
			list2.Add(new LeafsDebugMenuItem("Door Leafs"));
			list2.Add(new MapStonesDebugMenuItem("Map Stones"));
			list2.Add(new HealthDebugMenuItem("Health"));
			list2.Add(new MaxHealthDebugMenuItem("Max Health"));
			list2.Add(new EnergyDebugMenuItem("Energy"));
			list2.Add(new MaxEnergyDebugMenuItem("Max Energy"));
		}
		MonoBehaviour[] array = (MonoBehaviour[])UnityEngine.Object.FindObjectsOfType(typeof(MonoBehaviour));
		foreach (MonoBehaviour monoBehaviour in array)
		{
			IDebugMenuToggleable debugMenuToggleable = monoBehaviour as IDebugMenuToggleable;
			if (debugMenuToggleable != null)
			{
				list2.Add(new DebugMenuTogglerItem(debugMenuToggleable));
			}
		}
		list2.Add(new BoolDebugMenuItem("Deactivate Darkness", new Func<bool>(this.DeactivateDarknessGetter), new Action<bool>(this.DeactivateDarknessSetter)));
		list2.Add(new BoolDebugMenuItem("Camera", new Func<bool>(this.CameraEnabledGetter), new Action<bool>(this.CameraEnabledSetter)));
		list2.Add(new BoolDebugMenuItem("Music", new Func<bool>(this.DebugMuteMusicGetter), new Action<bool>(this.DebugMuteMusicSetter)));
		list2.Add(new BoolDebugMenuItem("Ambience", new Func<bool>(this.DebugMuteAmbienceGetter), new Action<bool>(this.DebugMuteAmbienceSetter)));
		list2.Add(new BoolDebugMenuItem("Sound Effects", new Func<bool>(this.DebugMuteSoundEffectsGetter), new Action<bool>(this.DebugMuteSoundEffectsSetter)));
		list2.Add(new BoolDebugMenuItem("Sound Log", new Func<bool>(this.ShowSoundLogGetter), new Action<bool>(this.ShowSoundLogSetter)));
		list2.Add(new BoolDebugMenuItem("Pink Boxes", new Func<bool>(this.ShowPinkBoxesGetter), new Action<bool>(this.ShowPinkBoxesSetter)));
		list2.Add(new BoolDebugMenuItem("UI", new Func<bool>(this.SeinUIGetter), new Action<bool>(this.SeinUISetter)));
		list2.Add(new BoolDebugMenuItem("Damage Text", new Func<bool>(this.SeinDamageTextGetter), new Action<bool>(this.SeinDamageTextSetter)));
		list2.Add(new BoolDebugMenuItem("120fps Physics", new Func<bool>(this.HighFPSPhysicsGetter), new Action<bool>(this.HighFPSPhysicsSetter)));
		list2.Add(new BoolDebugMenuItem("Invincibility", new Func<bool>(this.SeinInvincibilityGetter), new Action<bool>(this.SeinInvincibilitySetter)));
		list2.Add(new BoolDebugMenuItem("Replay Engine", new Func<bool>(this.ReplayEngineActiveGetter), new Action<bool>(this.ReplayEngineActiveSetter)));
		list2.Add(new ActionDebugMenuItem("Send leaderboard", new Func<bool>(this.SendLeaderboard)));
		list2.Add(new ActionDebugMenuItem("Load Test Scene", new Func<bool>(this.LoadTestScene)));
		list2.Add(new BoolDebugMenuItem("Auto send leaderboard", () => LeaderboardsController.AutoUpload, delegate(bool v)
		{
			LeaderboardsController.AutoUpload = v;
		}));
		List<IDebugMenuItem> list3 = new List<IDebugMenuItem>();
		foreach (string text in this.ImportantLevelsNames)
		{
			bool flag = false;
			foreach (RuntimeSceneMetaData runtimeSceneMetaData in Scenes.Manager.AllScenes)
			{
				if (runtimeSceneMetaData.Scene == text)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list3.Add(new ActionDebugMenuItem(text, new Func<bool>(this.GoToScene))
				{
					HelpText = "Press X or A to invoke teleport"
				});
			}
		}
		List<IDebugMenuItem> list4 = new List<IDebugMenuItem>();
		foreach (WorldEvents worldEvent in this.m_worldEvents)
		{
			list4.Add(new DebugMenuWorldEventActionMenuItem(worldEvent));
		}
		List<IDebugMenuItem> list5 = new List<IDebugMenuItem>();
		List<IDebugMenuItem> list6 = new List<IDebugMenuItem>();
		if (Characters.Sein)
		{
			list5.Add(new ActionDebugMenuItem("Remove all skills/abilities", new Func<bool>(AbilityDebugMenuItems.RemoveAllSkillsAndAbilities)));
			list5.Add(new ActionDebugMenuItem("Remove all skills", new Func<bool>(AbilityDebugMenuItems.RemoveAllSkills)));
			list5.Add(new BoolDebugMenuItem("Spirit Flame", new Func<bool>(AbilityDebugMenuItems.SpiritFlameGetter), new Action<bool>(AbilityDebugMenuItems.SpiritFlameSetter)));
			list5.Add(new BoolDebugMenuItem("Wall Jump", new Func<bool>(AbilityDebugMenuItems.WallJumpGetter), new Action<bool>(AbilityDebugMenuItems.WallJumpSetter)));
			list5.Add(new BoolDebugMenuItem("Charge Flame", new Func<bool>(AbilityDebugMenuItems.ChargeFlameGetter), new Action<bool>(AbilityDebugMenuItems.ChargeFlameSetter)));
			list5.Add(new BoolDebugMenuItem("Double Jump", new Func<bool>(AbilityDebugMenuItems.DoubleJumpGetter), new Action<bool>(AbilityDebugMenuItems.DoubleJumpSetter)));
			list5.Add(new BoolDebugMenuItem("Bash", new Func<bool>(AbilityDebugMenuItems.BashGetter), new Action<bool>(AbilityDebugMenuItems.BashSetter)));
			list5.Add(new BoolDebugMenuItem("Stomp", new Func<bool>(AbilityDebugMenuItems.StompGetter), new Action<bool>(AbilityDebugMenuItems.StompSetter)));
			list5.Add(new BoolDebugMenuItem("Glide", new Func<bool>(AbilityDebugMenuItems.GlideGetter), new Action<bool>(AbilityDebugMenuItems.GlideSetter)));
			list5.Add(new BoolDebugMenuItem("Climb", new Func<bool>(AbilityDebugMenuItems.ClimbGetter), new Action<bool>(AbilityDebugMenuItems.ClimbSetter)));
			list5.Add(new BoolDebugMenuItem("Charge Jump", new Func<bool>(AbilityDebugMenuItems.ChargeJumpGetter), new Action<bool>(AbilityDebugMenuItems.ChargeJumpSetter)));
			list5.Add(new BoolDebugMenuItem("Dash", new Func<bool>(AbilityDebugMenuItems.DashGetter), new Action<bool>(AbilityDebugMenuItems.DashSetter)));
			list5.Add(new BoolDebugMenuItem("Grenade", new Func<bool>(AbilityDebugMenuItems.GrenadeGetter), new Action<bool>(AbilityDebugMenuItems.GrenadeSetter)));
			list6.Add(new ActionDebugMenuItem("Remove all abilities", new Func<bool>(AbilityDebugMenuItems.RemoveAllAbilities)));
			list6.Add(new ActionDebugMenuItem("Remove all blue abilities", new Func<bool>(AbilityDebugMenuItems.RemoveAllBlueAbilities)));
			list6.Add(new BoolDebugMenuItem("Rekindle", new Func<bool>(AbilityDebugMenuItems.RekindleGetter), new Action<bool>(AbilityDebugMenuItems.RekindleSetter)));
			list6.Add(new BoolDebugMenuItem("Regroup", new Func<bool>(AbilityDebugMenuItems.RegroupGetter), new Action<bool>(AbilityDebugMenuItems.RegroupSetter)));
			list6.Add(new BoolDebugMenuItem("Charge Flame Efficiency", new Func<bool>(AbilityDebugMenuItems.ChargeFlameEfficiencyGetter), new Action<bool>(AbilityDebugMenuItems.ChargeFlameEfficiencySetter)));
			list6.Add(new BoolDebugMenuItem("Air Dash", new Func<bool>(AbilityDebugMenuItems.AirDashGetter), new Action<bool>(AbilityDebugMenuItems.AirDashSetter)));
			list6.Add(new BoolDebugMenuItem("Ultra Soul Link", new Func<bool>(AbilityDebugMenuItems.UltraSoulFlameGetter), new Action<bool>(AbilityDebugMenuItems.UltraSoulFlameSetter)));
			list6.Add(new BoolDebugMenuItem("Charge Dash", new Func<bool>(AbilityDebugMenuItems.ChargeDashGetter), new Action<bool>(AbilityDebugMenuItems.ChargeDashSetter)));
			list6.Add(new BoolDebugMenuItem("Water Breath", new Func<bool>(AbilityDebugMenuItems.WaterBreathGetter), new Action<bool>(AbilityDebugMenuItems.WaterBreathSetter)));
			list6.Add(new BoolDebugMenuItem("Soul Link Efficiency", new Func<bool>(AbilityDebugMenuItems.SoulFlameEfficiencyGetter), new Action<bool>(AbilityDebugMenuItems.SoulFlameEfficiencySetter)));
			list6.Add(new BoolDebugMenuItem("Triple Jump", new Func<bool>(AbilityDebugMenuItems.DoubleJumpUpgradeGetter), new Action<bool>(AbilityDebugMenuItems.DoubleJumpUpgradeSetter)));
			list6.Add(new BoolDebugMenuItem("Ultra Defense", new Func<bool>(AbilityDebugMenuItems.UltraDefenseGetter), new Action<bool>(AbilityDebugMenuItems.UltraDefenseSetter)));
			list6.Add(new ActionDebugMenuItem("Remove all purple abilities", new Func<bool>(AbilityDebugMenuItems.RemoveAllPurpleAbilities)));
			list6.Add(new BoolDebugMenuItem("Magnet", new Func<bool>(AbilityDebugMenuItems.MagnetGetter), new Action<bool>(AbilityDebugMenuItems.MagnetSetter)));
			list6.Add(new BoolDebugMenuItem("Map Markers", new Func<bool>(AbilityDebugMenuItems.MapMarkersGetter), new Action<bool>(AbilityDebugMenuItems.MapMarkersSetter)));
			list6.Add(new BoolDebugMenuItem("Health Efficiency", new Func<bool>(AbilityDebugMenuItems.HealthEfficiencyGetter), new Action<bool>(AbilityDebugMenuItems.HealthEfficiencySetter)));
			list6.Add(new BoolDebugMenuItem("Ultra Magnet", new Func<bool>(AbilityDebugMenuItems.UltraMagnetGetter), new Action<bool>(AbilityDebugMenuItems.UltraMagnetSetter)));
			list6.Add(new BoolDebugMenuItem("Energy Efficiency", new Func<bool>(AbilityDebugMenuItems.EnergyEfficiencyGetter), new Action<bool>(AbilityDebugMenuItems.EnergyEfficiencySetter)));
			list6.Add(new BoolDebugMenuItem("Spirit Efficiency", new Func<bool>(AbilityDebugMenuItems.AbilityMarkersGetter), new Action<bool>(AbilityDebugMenuItems.AbilityMarkersSetter)));
			list6.Add(new BoolDebugMenuItem("Spirit Potency", new Func<bool>(AbilityDebugMenuItems.SoulEfficiencyGetter), new Action<bool>(AbilityDebugMenuItems.SoulEfficiencySetter)));
			list6.Add(new BoolDebugMenuItem("Health Recovery", new Func<bool>(AbilityDebugMenuItems.HealthMarkersGetter), new Action<bool>(AbilityDebugMenuItems.HealthMarkersSetter)));
			list6.Add(new BoolDebugMenuItem("Energy Recovery", new Func<bool>(AbilityDebugMenuItems.EnergyMarkersGetter), new Action<bool>(AbilityDebugMenuItems.EnergyMarkersSetter)));
			list6.Add(new BoolDebugMenuItem("Sense Items", new Func<bool>(AbilityDebugMenuItems.SenseGetter), new Action<bool>(AbilityDebugMenuItems.SenseSetter)));
			list6.Add(new ActionDebugMenuItem("Remove all red abilities", new Func<bool>(AbilityDebugMenuItems.RemoveAllRedAbilities)));
			list6.Add(new BoolDebugMenuItem("Quick Flame", new Func<bool>(AbilityDebugMenuItems.QuickFlameGetter), new Action<bool>(AbilityDebugMenuItems.QuickFlameSetter)));
			list6.Add(new BoolDebugMenuItem("Spark Flame", new Func<bool>(AbilityDebugMenuItems.SparkFlameGetter), new Action<bool>(AbilityDebugMenuItems.SparkFlameSetter)));
			list6.Add(new BoolDebugMenuItem("Charge Flame Burn", new Func<bool>(AbilityDebugMenuItems.ChargeFlameBurnGetter), new Action<bool>(AbilityDebugMenuItems.ChargeFlameBurnSetter)));
			list6.Add(new BoolDebugMenuItem("Split Flame", new Func<bool>(AbilityDebugMenuItems.SplitFlameUpgradeGetter), new Action<bool>(AbilityDebugMenuItems.SplitFlameUpgradeSetter)));
			list6.Add(new BoolDebugMenuItem("Ultra Light Burst", new Func<bool>(AbilityDebugMenuItems.GrenadeUpgradeGetter), new Action<bool>(AbilityDebugMenuItems.GrenadeUpgradeSetter)));
			list6.Add(new BoolDebugMenuItem("Cinder Flame", new Func<bool>(AbilityDebugMenuItems.CinderFlameGetter), new Action<bool>(AbilityDebugMenuItems.CinderFlameSetter)));
			list6.Add(new BoolDebugMenuItem("Ultra Stomp", new Func<bool>(AbilityDebugMenuItems.StompUpgradeGetter), new Action<bool>(AbilityDebugMenuItems.StompUpgradeSetter)));
			list6.Add(new BoolDebugMenuItem("Rapid Flame", new Func<bool>(AbilityDebugMenuItems.RapidFireGetter), new Action<bool>(AbilityDebugMenuItems.RapidFireSetter)));
			list6.Add(new BoolDebugMenuItem("Charge Flame Blast", new Func<bool>(AbilityDebugMenuItems.ChargeFlameBlastGetter), new Action<bool>(AbilityDebugMenuItems.ChargeFlameBlastSetter)));
			list6.Add(new BoolDebugMenuItem("Ultra Split Flame", new Func<bool>(AbilityDebugMenuItems.UltraSplitFlameGetter), new Action<bool>(AbilityDebugMenuItems.UltraSplitFlameSetter)));
		}
		List<IDebugMenuItem> list7 = new List<IDebugMenuItem>();
		if (!XboxLiveController.IsContentPackage)
		{
			list7.Add(new ActionDebugMenuItem("Start FPS Test 0", new Func<bool>(this.StartFPSTest0)));
			list7.Add(new ActionDebugMenuItem("Start FPS Test 60", new Func<bool>(this.StartFPSTest60)));
			list7.Add(new ActionDebugMenuItem("Start FPS Test 120", new Func<bool>(this.StartFPSTest120)));
			list7.Add(new ActionDebugMenuItem("Start FPS Test 180", new Func<bool>(this.StartFPSTest180)));
			list7.Add(new ActionDebugMenuItem("Start FPS Test 240", new Func<bool>(this.StartFPSTest240)));
			list7.Add(new BoolDebugMenuItem("Override Misty Woods Conditions", () => SceneFPSTest.OVERRIDE_MISTYWOODS_CONDITION, delegate(bool val)
			{
				SceneFPSTest.OVERRIDE_MISTYWOODS_CONDITION = val;
			}));
			list7.Add(new BoolDebugMenuItem("FPS Test Reverse IsCutscene", () => SceneFPSTest.HACK_REVERSE_ISCUTSCENE, delegate(bool val)
			{
				SceneFPSTest.HACK_REVERSE_ISCUTSCENE = val;
			}));
			list7.Add(new BoolDebugMenuItem("Screenshot", () => SceneFPSTest.SHOULD_CREATE_SCREENSHOT, delegate(bool val)
			{
				SceneFPSTest.SHOULD_CREATE_SCREENSHOT = val;
			}));
			list7.Add(new BoolDebugMenuItem("Memory Report", () => SceneFPSTest.SHOULD_CREATE_MEMORY_REPORT, delegate(bool val)
			{
				SceneFPSTest.SHOULD_CREATE_MEMORY_REPORT = val;
			}));
			list7.Add(new BoolDebugMenuItem("Basic Sample", () => SceneFPSTest.SHOULD_RUN_SAMPLE, delegate(bool val)
			{
				SceneFPSTest.SHOULD_RUN_SAMPLE = val;
			}));
			list7.Add(new BoolDebugMenuItem("No Camera", () => SceneFPSTest.SHOULD_RUN_CPU_SAMPLE, delegate(bool val)
			{
				SceneFPSTest.SHOULD_RUN_CPU_SAMPLE = val;
			}));
			list7.Add(new BoolDebugMenuItem("Quad Scale 0", () => SceneFPSTest.SHOULD_RUN_CPU_B_SAMPLE, delegate(bool val)
			{
				SceneFPSTest.SHOULD_RUN_CPU_B_SAMPLE = val;
			}));
			list7.Add(new BoolDebugMenuItem("Draw Debug UI", () => SceneFPSTest.DRAW_DEBUG_UI, delegate(bool val)
			{
				SceneFPSTest.DRAW_DEBUG_UI = val;
			}));
		}
		list7.Add(new BoolDebugMenuItem("Streaming Install Debug Override", new Func<bool>(this.StreamingInstallDebugGetter), new Action<bool>(this.StreamingInstallDebugSetter)));
		list7.Add(new ActionDebugMenuItem("Break Telem URL", new Func<bool>(this.BreakSteamTelemetryURL)));
		list7.Add(new ActionDebugMenuItem("Set Telemetry to UPF", new Func<bool>(this.SetSteamTelemetryURLToUPF)));
		List<IDebugMenuItem> list8 = new List<IDebugMenuItem>();
		list8.Add(new BoolDebugMenuItem("Clean Water", new Func<bool>(this.CleanWaterGetter), new Action<bool>(this.CleanWaterSetter)));
		list8.Add(new BoolDebugMenuItem("Wind Released", new Func<bool>(this.WindReleasedGetter), new Action<bool>(this.WindReleasedSetter)));
		list8.Add(new BoolDebugMenuItem("Gumo Free", new Func<bool>(this.GumoFreeGetter), new Action<bool>(this.GumFreeSetter)));
		list8.Add(new BoolDebugMenuItem("Forlorn Energy Restored", new Func<bool>(this.ForlornEnergyRestoredGetter), new Action<bool>(this.ForlornEnergyRestoredSetter)));
		list8.Add(new BoolDebugMenuItem("Mist Lifted", new Func<bool>(this.MistLiftedGetter), new Action<bool>(this.MistLiftedSetter)));
		list8.Add(new BoolDebugMenuItem("Ginso Key", new Func<bool>(this.GinsoKeyGetter), new Action<bool>(this.GinsoKeySetter)));
		list8.Add(new BoolDebugMenuItem("Forlorn Ruins Key", new Func<bool>(this.ForlornRuinsKeyGetter), new Action<bool>(this.ForlornRuinsKeySetter)));
		list8.Add(new BoolDebugMenuItem("Horu Key", new Func<bool>(this.HoruKeyGetter), new Action<bool>(this.HoruKeySetter)));
		list8.Add(new BoolDebugMenuItem("Darkness Lifted", new Func<bool>(this.DarknessLiftedGetter), new Action<bool>(this.DarknessLiftedSetter)));
		if (GameController.Instance.IsTrial)
		{
			list5.Clear();
			list6.Clear();
			list7.Clear();
			list8.Clear();
		}
		if (list.Count > 0)
		{
			this.m_menuList.Add(list);
		}
		if (list2.Count > 0)
		{
			this.m_menuList.Add(list2);
		}
		if (list3.Count > 0)
		{
			this.m_menuList.Add(list3);
		}
		if (list4.Count > 0)
		{
			this.m_menuList.Add(list4);
		}
		if (list5.Count > 0)
		{
			this.m_menuList.Add(list5);
		}
		if (list6.Count > 0)
		{
			this.m_menuList.Add(list6);
		}
		if (list7.Count > 0)
		{
			this.m_menuList.Add(list7);
		}
		if (list8.Count > 0)
		{
			this.m_menuList.Add(list8);
		}
		this.m_showGumoSequences = false;
		int num = 8;
		this.m_gumoSequencesMenuList.Clear();
		List<IDebugMenuItem> list9 = new List<IDebugMenuItem>();
		List<IDebugMenuItem> list10 = new List<IDebugMenuItem>();
		List<IDebugMenuItem> list11 = new List<IDebugMenuItem>();
		List<IDebugMenuItem> list12 = new List<IDebugMenuItem>();
		List<IDebugMenuItem> list13 = new List<IDebugMenuItem>();
		foreach (GoToSequenceData goToSequenceData in this.GumoSequence)
		{
			if (goToSequenceData.Scene)
			{
			}
			if (list9.Count < num)
			{
				list9.Add(new GoToSequenceMenuItem(goToSequenceData));
			}
			else if (list10.Count < num)
			{
				list10.Add(new GoToSequenceMenuItem(goToSequenceData));
			}
			else if (list11.Count < num)
			{
				list11.Add(new GoToSequenceMenuItem(goToSequenceData));
			}
			else if (list12.Count < num)
			{
				list12.Add(new GoToSequenceMenuItem(goToSequenceData));
			}
			else
			{
				list13.Add(new GoToSequenceMenuItem(goToSequenceData));
			}
		}
		if (list9.Count > 0)
		{
			this.m_gumoSequencesMenuList.Add(list9);
		}
		if (list10.Count > 0)
		{
			this.m_gumoSequencesMenuList.Add(list10);
		}
		if (list11.Count > 0)
		{
			this.m_gumoSequencesMenuList.Add(list11);
		}
		if (list12.Count > 0)
		{
			this.m_gumoSequencesMenuList.Add(list12);
		}
		if (list13.Count > 0)
		{
			this.m_gumoSequencesMenuList.Add(list13);
		}
	}

	private bool UnlockDifficulties()
	{
		GameSettings.Instance.OneLifeModeUnlocked = true;
		GameSettings.Instance.SaveSettings();
		return true;
	}

	private bool BreakSteamTelemetryURL()
	{
		SteamTelemetry.URL = "http://www.ssodifjsoifj.com";
		return true;
	}

	private bool SetSteamTelemetryURLToUPF()
	{
		SteamTelemetry.URL = "http://www.upf.co.il/steamTelemetryTest.php";
		return true;
	}

	private bool VisitAllAreas()
	{
		World.CurrentArea.VisitAllAreas();
		World.CurrentArea.UpdateCompletionAmount();
		return true;
	}

	private bool StreamingInstallDebugGetter()
	{
		return DebugMenuB.IsFullyInstalledDebugOverride;
	}

	private void StreamingInstallDebugSetter(bool value)
	{
		DebugMenuB.IsFullyInstalledDebugOverride = value;
	}

	private bool FixedUpdateSyncGetter()
	{
		return FixedUpdateSyncTracker.Enable;
	}

	private void FixedUpdateSyncSetter(bool value)
	{
		FixedUpdateSyncTracker.Enable = value;
	}

	private bool HighFPSPhysicsGetter()
	{
		return this.m_highFPSPhysics;
	}

	private void HighFPSPhysicsSetter(bool value)
	{
		this.m_highFPSPhysics = value;
		if (value)
		{
			Time.fixedDeltaTime = 0.008333334f;
		}
		else
		{
			Time.fixedDeltaTime = 0.016666668f;
		}
	}

	private bool LimitPhysicsIterationGetter()
	{
		return Mathf.Round(Time.maximumDeltaTime * 100f) == Mathf.Round(1.6666667f);
	}

	private void LimitPhysicsIterationSetter(bool obj)
	{
		Time.maximumDeltaTime = ((!obj) ? 0.033333335f : 0.016666668f);
	}

	private bool StartFPSTest0()
	{
		SceneFPSTest.SetupTheTest();
		return true;
	}

	private bool StartFPSTest60()
	{
		SceneFPSTest.CurrentSceneMetaDataIndex = 60;
		SceneFPSTest.SetupTheTest();
		return true;
	}

	private bool StartFPSTest120()
	{
		SceneFPSTest.CurrentSceneMetaDataIndex = 120;
		SceneFPSTest.SetupTheTest();
		return true;
	}

	private bool StartFPSTest180()
	{
		SceneFPSTest.CurrentSceneMetaDataIndex = 180;
		SceneFPSTest.SetupTheTest();
		return true;
	}

	private bool StartFPSTest240()
	{
		SceneFPSTest.CurrentSceneMetaDataIndex = 240;
		SceneFPSTest.SetupTheTest();
		return true;
	}

	private bool ResetSteamStats()
	{
		if (Steamworks.Ready)
		{
			Steamworks.SteamInterface.Stats.ResetAllStats(true);
		}
		return true;
	}

	private void ForlornRuinsKeySetter(bool obj)
	{
		Keys.ForlornRuins = obj;
	}

	private bool ForlornRuinsKeyGetter()
	{
		return Keys.ForlornRuins;
	}

	private void GinsoKeySetter(bool obj)
	{
		Keys.GinsoTree = obj;
	}

	private bool GinsoKeyGetter()
	{
		return Keys.GinsoTree;
	}

	private void HoruKeySetter(bool obj)
	{
		Keys.MountHoru = obj;
	}

	private bool HoruKeyGetter()
	{
		return Keys.MountHoru;
	}

	public void AddWorldEvent(WorldEvents worldEvent)
	{
		if (this.m_worldEvents.Contains(worldEvent))
		{
			return;
		}
		this.m_worldEvents.Add(worldEvent);
	}

	private bool WindReleasedGetter()
	{
		return Sein.World.Events.WindRestored;
	}

	private void WindReleasedSetter(bool released)
	{
		Sein.World.Events.WindRestored = released;
	}

	private bool DarknessLiftedGetter()
	{
		return Sein.World.Events.DarknessLifted;
	}

	private void DarknessLiftedSetter(bool isDarknessLifted)
	{
		Sein.World.Events.DarknessLifted = isDarknessLifted;
	}

	private bool LoadGame()
	{
		if (!GameController.Instance.SaveGameController.PerformLoad())
		{
		}
		return true;
	}

	private bool SkipAction()
	{
		SkipCutsceneController.Instance.SkipCutscene();
		return true;
	}

	private bool SaveGame()
	{
		this.HideDebugMenu();
		GameController.Instance.CreateCheckpoint();
		GameController.Instance.SaveGameController.PerformSave();
		return true;
	}

	private void GumFreeSetter(bool obj)
	{
		Sein.World.Events.GumoFree = obj;
	}

	private bool GumoFreeGetter()
	{
		return Sein.World.Events.GumoFree;
	}

	private void ForlornEnergyRestoredSetter(bool obj)
	{
		Sein.World.Events.GravityActivated = obj;
	}

	private bool ForlornEnergyRestoredGetter()
	{
		return Sein.World.Events.GravityActivated;
	}

	private void MistLiftedSetter(bool value)
	{
		Sein.World.Events.MistLifted = value;
	}

	private bool MistLiftedGetter()
	{
		return Sein.World.Events.MistLifted;
	}

	private void SeinUISetter(bool obj)
	{
		SeinUI.DebugHideUI = obj;
	}

	private void SeinDamageTextSetter(bool obj)
	{
		GameSettings.Instance.DamageTextEnabled = obj;
	}

	private bool CameraEnabledGetter()
	{
		return UI.Cameras.Current.Camera.enabled;
	}

	private void CameraEnabledSetter(bool obj)
	{
		UI.Cameras.Current.Camera.enabled = obj;
		if (obj)
		{
			return;
		}
		this.HideDebugMenu();
		Graphics.SetRenderTarget(UI.Cameras.Current.GetComponent<Camera>().targetTexture);
		GL.Clear(true, true, Color.black);
		Graphics.SetRenderTarget(null);
	}

	private bool DebugMuteMusicGetter()
	{
		return DebugMenuB.MuteMusic;
	}

	private void DebugMuteMusicSetter(bool value)
	{
		DebugMenuB.MuteMusic = value;
	}

	private bool DebugMuteAmbienceGetter()
	{
		return DebugMenuB.MuteAmbience;
	}

	private void DebugMuteAmbienceSetter(bool value)
	{
		DebugMenuB.MuteAmbience = value;
	}

	private bool DebugMuteSoundEffectsGetter()
	{
		return DebugMenuB.MuteSoundEffects;
	}

	private void DebugMuteSoundEffectsSetter(bool value)
	{
		DebugMenuB.MuteSoundEffects = value;
	}

	private bool SeinUIGetter()
	{
		return SeinUI.DebugHideUI;
	}

	private bool SeinDamageTextGetter()
	{
		return GameSettings.Instance.DamageTextEnabled;
	}

	private bool SeinInvincibilityGetter()
	{
		return Characters.Sein && Characters.Sein.Mortality.DamageReciever && Characters.Sein.Mortality.DamageReciever.IsImmortal;
	}

	private void SeinInvincibilitySetter(bool newValue)
	{
		if (Characters.Sein)
		{
			Characters.Sein.Mortality.DamageReciever.IsImmortal = newValue;
		}
	}

	private bool ReplayEngineActiveGetter()
	{
		return Recorder.Instance && Recorder.Instance.Active;
	}

	private void ReplayEngineActiveSetter(bool newValue)
	{
		if (Recorder.Instance)
		{
			Recorder.Instance.Active = newValue;
		}
	}

	private bool GumoSequencesAction()
	{
		this.m_showGumoSequences = true;
		return false;
	}

	private bool AchievementHintGetter()
	{
		return DebugMenuB.ShowAchievementHint;
	}

	private void AchievementHintSetter(bool newValue)
	{
		DebugMenuB.ShowAchievementHint = newValue;
	}

	private bool CleanWaterGetter()
	{
		return Sein.World.Events.WaterPurified;
	}

	private void CleanWaterSetter(bool newValue)
	{
		Sein.World.Events.WaterPurified = newValue;
	}

	private void CheatsSetter(bool arg)
	{
		CheatsHandler.Instance.DebugEnabled = arg;
		if (!CheatsHandler.Instance.DebugEnabled)
		{
			DebugMenuB.ToggleDebugMenu();
		}
	}

	private bool CheatsGetter()
	{
		return CheatsHandler.Instance.DebugEnabled;
	}

	private void DebugControlsSetter(bool arg)
	{
		DebugMenuB.DebugControlsEnabled = arg;
	}

	private bool DebugControlsGetter()
	{
		return DebugMenuB.DebugControlsEnabled;
	}

	private void DebugTextSetter(bool arg)
	{
		DebugGUIText.Enabled = arg;
	}

	private bool DebugTextGetter()
	{
		return DebugGUIText.Enabled;
	}

	private void DebugSceneFrameworkSetter(bool arg)
	{
		this.m_showSceneFrameworkDebug = arg;
	}

	private bool DebugSceneFrameworkGetter()
	{
		return this.m_showSceneFrameworkDebug;
	}

	private bool VisualLogGetter()
	{
		return VisualLog.Instance != null;
	}

	private void VisualLogSetter(bool arg)
	{
		if (arg == (VisualLog.Instance != null))
		{
			return;
		}
		if (arg)
		{
			base.gameObject.AddComponent<VisualLog>();
		}
		else
		{
			VisualLog.Disable();
		}
	}

	private bool LogCallbackHookGetter()
	{
		return LogCallbackHandler.Instance != null;
	}

	private void LogCallbackHookSetter(bool arg)
	{
		if (LogCallbackHandler.Instance == null)
		{
			LogCallbackHandler.Instance = new LogCallbackHandler();
		}
		else
		{
			LogCallbackHandler.Instance.RemoveHandler();
		}
	}

	private void DebugXboxControllerSetter(bool arg)
	{
		if (XboxLiveController.Instance)
		{
			XboxLiveController.Instance.IsDebugEnabled = arg;
		}
	}

	private bool DebugXboxControllerGetter()
	{
		return XboxLiveController.Instance && XboxLiveController.Instance.IsDebugEnabled;
	}

	private void UnloadUnusedSetter(bool arg)
	{
		Resources.UnloadUnusedAssets();
	}

	private bool UnloadUnusedGetter()
	{
		return true;
	}

	private bool ShowSoundLogGetter()
	{
		return Sound.IsSoundLogEnabled;
	}

	private void ShowSoundLogSetter(bool arg)
	{
		Sound.IsSoundLogEnabled = arg;
	}

	private bool ShowPinkBoxesGetter()
	{
		return Sound.IsPinkBoxesEnabled;
	}

	private void ShowPinkBoxesSetter(bool arg)
	{
		Sound.IsPinkBoxesEnabled = arg;
	}

	private bool DeactivateDarknessGetter()
	{
		return SpiritLightVisualAffectorManager.DeactivateLightMechanics;
	}

	private void DeactivateDarknessSetter(bool arg)
	{
		SpiritLightVisualAffectorManager.DeactivateLightMechanics = arg;
	}

	public bool ResetInputLock()
	{
		this.HideDebugMenu();
		GameController.Instance.ResetInputLocks();
		SuspensionManager.ResumeAll();
		return true;
	}

	public bool TeleportNightberry()
	{
		if (Items.NightBerry)
		{
			Items.NightBerry.transform.position = Characters.Sein.Position;
			Items.NightBerry.SetToDropMode();
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(this.NightberryPlaceholder, Characters.Sein.Position, Quaternion.identity) as GameObject;
			InstantiateUtility.Destroy(gameObject);
		}
		return true;
	}

	private void Initialize()
	{
		this.BuildMenu();
	}

	public void HandleQuickQuit()
	{
		if (DebugMenuB.DebugControlsEnabled && Core.Input.ChargeJump.IsPressed && Core.Input.Bash.IsPressed && Core.Input.SoulFlame.IsPressed && !TestSetManager.IsPerformingTests)
		{
			try
			{
				InstantLoadScenesController.Instance.LogState();
			}
			catch (Exception ex)
			{
			}
			Application.Quit();
		}
	}

	public void Update()
	{
		if (CheatsHandler.Instance && !CheatsHandler.Instance.DebugEnabled && !CheatsHandler.DebugAlwaysEnabled)
		{
			return;
		}
		this.HandleQuickQuit();
		if (DebugMenuB.Active)
		{
			if (!this.m_lastDebugMenuActiveState)
			{
				this.Initialize();
			}
			this.m_menuList[(int)this.m_cursorIndex.x][(int)this.m_cursorIndex.y].OnSelectedUpdate();
		}
	}

	private void ResetHold()
	{
		this.m_holdRemainingTime = 0.4f;
		this.m_holdDelayDuration = 0.04f;
	}

	public void FixedUpdate()
	{
		if (CheatsHandler.Instance && !CheatsHandler.Instance.DebugEnabled && !CheatsHandler.DebugAlwaysEnabled)
		{
			return;
		}
		if (GameController.FreezeFixedUpdate)
		{
			return;
		}
		if (MoonInput.GetKeyDown(KeyCode.N))
		{
			this.DisableEnemies();
		}
		if (!DebugMenuB.Active && !Recorder.IsPlaying && !DebugMenu.DashOrGrenadeEnabled)
		{
			if (Core.Input.LeftShoulder.IsPressed && DebugMenuB.DebugControlsEnabled)
			{
				Time.timeScale = this.FastForwardTimeScale;
			}
			else if (Core.Input.LeftShoulder.WasPressed)
			{
				Time.timeScale = 1f;
			}
		}
		if (DebugMenuB.Active)
		{
			if (this.m_showGumoSequences)
			{
				if (Core.Input.SoulFlame.OnPressed)
				{
					this.m_showGumoSequences = false;
				}
				if (Core.Input.Down.OnPressed)
				{
					this.m_gumoSequencesCursorIndex.y = this.m_gumoSequencesCursorIndex.y + 1f;
				}
				if (Core.Input.Up.OnPressed)
				{
					this.m_gumoSequencesCursorIndex.y = this.m_gumoSequencesCursorIndex.y - 1f;
				}
				if (Core.Input.Left.OnPressed)
				{
					this.m_gumoSequencesCursorIndex.x = this.m_gumoSequencesCursorIndex.x - 1f;
				}
				if (Core.Input.Right.OnPressed)
				{
					this.m_gumoSequencesCursorIndex.x = this.m_gumoSequencesCursorIndex.x + 1f;
				}
				if (this.m_gumoSequencesCursorIndex.x == -1f)
				{
					this.m_gumoSequencesCursorIndex.x = (float)(this.m_gumoSequencesMenuList.Count - 1);
				}
				if (this.m_gumoSequencesCursorIndex.y == -1f)
				{
					this.m_gumoSequencesCursorIndex.y = (float)(this.m_gumoSequencesMenuList[(int)this.m_gumoSequencesCursorIndex.x].Count - 1);
				}
				if (this.m_gumoSequencesCursorIndex.x == (float)this.m_gumoSequencesMenuList.Count)
				{
					this.m_gumoSequencesCursorIndex.x = 0f;
				}
				if (this.m_gumoSequencesCursorIndex.y == (float)this.m_gumoSequencesMenuList[(int)this.m_gumoSequencesCursorIndex.x].Count)
				{
					this.m_gumoSequencesCursorIndex.y = 0f;
				}
				if (this.m_gumoSequencesCursorIndex != this.m_lastGumoSequencesIndex)
				{
					this.m_gumoSequencesMenuList[(int)this.m_gumoSequencesCursorIndex.x][(int)this.m_gumoSequencesCursorIndex.y].OnSelected();
					this.m_lastGumoSequencesIndex = this.m_gumoSequencesCursorIndex;
				}
				this.m_gumoSequencesMenuList[(int)this.m_gumoSequencesCursorIndex.x][(int)this.m_gumoSequencesCursorIndex.y].OnSelectedFixedUpdate();
			}
			else
			{
				if (!this.m_lastDebugMenuActiveState)
				{
					this.Initialize();
				}
				if (Core.Input.Down.OnPressed)
				{
					this.ResetHold();
					this.m_cursorIndex.y = this.m_cursorIndex.y + 1f;
				}
				if (Core.Input.Up.OnPressed)
				{
					this.ResetHold();
					this.m_cursorIndex.y = this.m_cursorIndex.y - 1f;
				}
				if (Core.Input.Left.OnPressed)
				{
					this.ResetHold();
					this.m_cursorIndex.x = this.m_cursorIndex.x - 1f;
				}
				if (Core.Input.Right.OnPressed)
				{
					this.ResetHold();
					this.m_cursorIndex.x = this.m_cursorIndex.x + 1f;
				}
				if (Core.Input.Left.Pressed || Core.Input.Right.Pressed || Core.Input.Up.Pressed || Core.Input.Down.Pressed)
				{
					this.m_holdRemainingTime -= Time.deltaTime;
					if (this.m_holdRemainingTime < 0f)
					{
						this.m_holdRemainingTime = this.m_holdDelayDuration;
						if (Core.Input.Left.Pressed)
						{
							this.m_cursorIndex.x = this.m_cursorIndex.x - 1f;
						}
						if (Core.Input.Right.Pressed)
						{
							this.m_cursorIndex.x = this.m_cursorIndex.x + 1f;
						}
						if (Core.Input.Down.Pressed)
						{
							this.m_cursorIndex.y = this.m_cursorIndex.y + 1f;
						}
						if (Core.Input.Up.Pressed)
						{
							this.m_cursorIndex.y = this.m_cursorIndex.y - 1f;
						}
					}
				}
				if (this.m_cursorIndex.x < 0f)
				{
					this.m_cursorIndex.x = (float)(this.m_menuList.Count - 1);
				}
				if (this.m_cursorIndex.x > (float)(this.m_menuList.Count - 1))
				{
					this.m_cursorIndex.x = 0f;
				}
				if (this.m_cursorIndex.y < 0f)
				{
					this.m_cursorIndex.y = (float)(this.m_menuList[(int)this.m_cursorIndex.x].Count - 1);
				}
				if (Core.Input.Left.OnPressed || Core.Input.Right.OnPressed)
				{
					if (this.m_cursorIndex.y > (float)(this.m_menuList[(int)this.m_cursorIndex.x].Count - 1))
					{
						this.m_cursorIndex.y = (float)(this.m_menuList[(int)this.m_cursorIndex.x].Count - 1);
					}
				}
				else if (this.m_cursorIndex.y > (float)(this.m_menuList[(int)this.m_cursorIndex.x].Count - 1))
				{
					this.m_cursorIndex.y = 0f;
				}
				if (this.m_cursorIndex != this.m_lastIndex)
				{
					this.m_menuList[(int)this.m_cursorIndex.x][(int)this.m_cursorIndex.y].OnSelected();
					this.m_lastIndex = this.m_cursorIndex;
					DebugMenuB.ShouldShowOnlySelectedItem = false;
				}
				this.m_menuList[(int)this.m_cursorIndex.x][(int)this.m_cursorIndex.y].OnSelectedFixedUpdate();
			}
		}
		this.m_lastDebugMenuActiveState = DebugMenuB.Active;
	}

	public void OnGUI()
	{
		if (DebugMenuB.Active)
		{
			GUILayout.BeginArea(new Rect((float)(Screen.width - 150), (float)(Screen.height - 50), 150f, 50f));
			GUILayout.Label("BuildID: " + this.BuildID, new GUILayoutOption[0]);
			GUILayout.EndArea();
			if (this.m_showGumoSequences)
			{
				GUILayout.BeginArea(new Rect(this.MenuTopLeftX, this.MenuTopLeftY, this.MenuWidth, this.MenuHeight), GUIContent.none, DebugMenuB.DebugMenuStyle);
				int num = 0;
				foreach (List<IDebugMenuItem> list in this.m_gumoSequencesMenuList)
				{
					int num2 = 0;
					foreach (IDebugMenuItem debugMenuItem in list)
					{
						Vector2 vector = new Vector2(this.HorizontalSpace * (float)num, this.VerticalSpace * (float)num2);
						bool b = new Vector2((float)num, (float)num2) == this.m_gumoSequencesCursorIndex;
						debugMenuItem.Draw(new Rect(vector.x, vector.y, this.HorizontalSpace, this.VerticalSpace), b);
						num2++;
					}
					num++;
				}
				GUILayout.EndArea();
				GUI.Label(new Rect(this.MenuTopLeftX, this.MenuTopLeftY + this.MenuHeight, this.MenuWidth, 30f), this.m_gumoSequencesMenuList[(int)this.m_gumoSequencesCursorIndex.x][(int)this.m_gumoSequencesCursorIndex.y].HelpText, DebugMenuB.DebugMenuStyle);
			}
			else
			{
				if (this.m_menuList.Count == 0)
				{
					return;
				}
				if (!DebugMenuB.ShouldShowOnlySelectedItem)
				{
					GUILayout.BeginArea(new Rect(this.MenuTopLeftX, this.MenuTopLeftY, this.MenuWidth, this.MenuHeight), GUIContent.none, DebugMenuB.DebugMenuStyle);
				}
				else
				{
					GUILayout.BeginArea(new Rect(this.MenuTopLeftX, this.MenuTopLeftY, this.MenuWidth, this.MenuHeight), GUIContent.none);
				}
				int num3 = 0;
				foreach (List<IDebugMenuItem> list2 in this.m_menuList)
				{
					int num4 = 0;
					foreach (IDebugMenuItem debugMenuItem2 in list2)
					{
						Vector2 vector2 = new Vector2((float)this.GetColPosition(num3), this.VerticalSpace * (float)num4);
						bool flag = new Vector2((float)num3, (float)num4) == this.m_cursorIndex;
						if (!DebugMenuB.ShouldShowOnlySelectedItem || flag)
						{
							debugMenuItem2.Draw(new Rect(vector2.x, vector2.y, (float)this.ColumnsWidth[num3], this.VerticalSpace), flag);
						}
						num4++;
					}
					num3++;
				}
				GUILayout.EndArea();
				if (!DebugMenuB.ShouldShowOnlySelectedItem)
				{
					GUI.Label(new Rect(this.MenuTopLeftX, this.MenuTopLeftY + this.MenuHeight, this.MenuWidth, 30f), this.m_menuList[(int)this.m_cursorIndex.x][(int)this.m_cursorIndex.y].HelpText, DebugMenuB.DebugMenuStyle);
				}
			}
		}
		else if (this.m_showSceneFrameworkDebug)
		{
			Scenes.Manager.DrawScenesManagerDebugData();
		}
	}

	private int GetColPosition(int index)
	{
		int num = 0;
		for (int i = 0; i < index; i++)
		{
			num += this.ColumnsWidth[i];
		}
		return num;
	}

	private bool CreateCheckpoint()
	{
		this.HideDebugMenu();
		GameController.Instance.CreateCheckpoint();
		return true;
	}

	private bool RestoreCheckpoint()
	{
		GameController.Instance.RestoreCheckpoint(null);
		return true;
	}

	private bool GoToScene()
	{
		base.StartCoroutine(this.GoToScene(this.m_menuList[(int)this.m_cursorIndex.x][(int)this.m_cursorIndex.y].Text));
		return true;
	}

	private bool FaderBAction()
	{
		UI.Fader.Fade(0.5f, 0.5f, 0.5f, null, null);
		return true;
	}

	public IEnumerator GoToScene(string sceneName)
	{
		RuntimeSceneMetaData sceneInformation = Scenes.Manager.GetSceneInformation(sceneName);
		Scenes.Manager.AutoLoadingUnloading = false;
		Scenes.Manager.UnloadAllScenes();
		Scenes.Manager.DestroyManager.DestroyAll();
		SuspensionManager.SuspendAll();
		while (Scenes.Manager.ResourcesNeedUnloading)
		{
			yield return new WaitForFixedUpdate();
		}
		SuspensionManager.ResumeAll();
		GameController.Instance.ResetStateForDebugMenuGoToScene();
		GoToSceneController.Instance.GoToScene(sceneInformation, null, true);
		DebugMenuB.ToggleDebugMenu();
		yield break;
	}

	private bool ResetNightBerryPosition()
	{
		if (Items.NightBerry == null)
		{
			return false;
		}
		Items.NightBerry.transform.position = Characters.Sein.PlatformBehaviour.PlatformMovement.Position;
		return true;
	}

	private bool Quit()
	{
		Application.Quit();
		return true;
	}

	public const float HOLD_DELAY = 0.4f;

	public const float HOLD_FAST_DELAY = 0.04f;

	public static DebugMenuB Instance = null;

	private readonly List<WorldEvents> m_worldEvents = new List<WorldEvents>();

	private readonly List<List<IDebugMenuItem>> m_menuList = new List<List<IDebugMenuItem>>();

	private readonly List<List<IDebugMenuItem>> m_gumoSequencesMenuList = new List<List<IDebugMenuItem>>();

	public List<SceneMetaData> ImportantLevels = new List<SceneMetaData>();

	public List<string> ImportantLevelsNames = new List<string>();

	public GUISkin Skin;

	public static GUIStyle SelectedStyle;

	public static GUIStyle Style;

	public static GUIStyle PressedStyle;

	public static GUIStyle DebugMenuStyle;

	public static GUIStyle StyleEnabled;

	public static GUIStyle StyleDisabled;

	public SceneMetaData TestScene;

	public static bool UnlockAllCutscenes = false;

	public static bool MuteMusic = false;

	public static bool MuteAmbience = false;

	public static bool MuteSoundEffects = false;

	public float VerticalSpace = 25f;

	public float HorizontalSpace = 150f;

	private Vector2 m_cursorIndex;

	private Vector2 m_gumoSequencesCursorIndex;

	public MessageProvider ReplayGotResetMessageProvider;

	public int BuildID;

	public float MenuTopLeftX = 200f;

	public float MenuTopLeftY = 70f;

	public float MenuWidth = 900f;

	public float MenuHeight = 600f;

	private bool m_showSceneFrameworkDebug;

	public static bool ShowAchievementHint = false;

	private bool m_showGumoSequences;

	private bool m_superSlowMotion;

	public List<int> ColumnsWidth = new List<int>();

	public List<GoToSequenceData> GumoSequence = new List<GoToSequenceData>();

	public static bool IsFullyInstalledDebugOverride = false;

	private static readonly HashSet<ISuspendable> SuspendablesToIgnoreForGameplay = new HashSet<ISuspendable>();

	private List<GameObject> m_particleSystems;

	private GameObject[] m_art;

	private GameObject[] m_enemies;

	private bool m_highFPSPhysics;

	public GameObject NightberryPlaceholder;

	private bool m_lastDebugMenuActiveState;

	private Vector2 m_lastIndex;

	private Vector2 m_lastGumoSequencesIndex;

	public static bool Active = false;

	public static bool DebugControlsEnabled = false;

	public float FastForwardTimeScale = 3f;

	private float m_holdDelayDuration;

	private float m_holdRemainingTime;

	public static bool ShouldShowOnlySelectedItem = false;
}
