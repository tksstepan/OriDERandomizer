using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;
using UnityEngine.Audio;

public class MixerManager : MonoBehaviour
{
	public void RegisterSnapshotZone(MixerSnapshotZone mixerSnapshotZone)
	{
		this.m_snapshotZones.Add(mixerSnapshotZone);
	}

	public void DeregisterSnapshotZone(MixerSnapshotZone mixerSnapshotZone)
	{
		this.m_snapshotZones.Remove(mixerSnapshotZone);
	}

	public void Awake()
	{
		MixerManager.s_manager = this;
	}

	public static MixerManager Instance
	{
		get
		{
			return MixerManager.s_manager;
		}
	}

	public void RegisterActiveSnapshot(MixerSnapshot snapshot)
	{
		if (!this.m_currentlyActiveSnapshots.Contains(snapshot))
		{
			this.m_currentlyActiveSnapshots.Add(snapshot);
		}
	}

	public void FixedUpdate()
	{
		bool flag = UI.MainMenuVisible || ResumeGameController.IsGameSuspended;
		if (flag != this.m_wasInUI)
		{
			if (flag)
			{
				this.UISnapshot.FadeIn();
			}
			else
			{
				this.UISnapshot.FadeOut();
			}
		}
		this.m_wasInUI = flag;
		this.UpdateMixerSnapshotZones();
		this.UpdateMixerSettingsBasedOnActiveSnapshots();
	}

	private void UpdateMixerSettingsBasedOnActiveSnapshots()
	{
		this.m_settings.Reset();
		for (int i = 0; i < this.m_currentlyActiveSnapshots.Count; i++)
		{
			MixerSnapshot mixerSnapshot = this.m_currentlyActiveSnapshots[i];
			mixerSnapshot.UpdateMixerSnapshotState(Time.fixedDeltaTime);
			this.m_settings.MultiplyBlendWith(mixerSnapshot.SnapshotSettings, mixerSnapshot.Weight);
		}
		this.m_settings.MultiplyBlendWith(this.ModulatingSnapshot.SnapshotSettings, 1f);
		this.m_currentlyActiveSnapshots.RemoveAll(MixerManager.CachedIsSnapshotInactivePredicate);
		this.m_settings.Music = this.m_settings.Music * Mathf.Log10(GameSettings.Instance.MusicVolume * 9f + 1f);
		this.m_settings.SoundEffects = this.m_settings.SoundEffects * Mathf.Log10(GameSettings.Instance.SoundEffectsVolume * 9f + 1f);
		this.ApplySoundCompression();
		AudioMixer masterMixer = MixerManager.GetMasterMixer();
		this.m_settings.ApplyGroupSettingsToMixer(masterMixer);
	}

	private void UpdateMixerSnapshotZones()
	{
		Vector3 cameraPositionForSampling = UI.Cameras.Current.CameraPositionForSampling;
		SceneRoot sceneRoot = Scenes.Manager.FindLoadedSceneRootFromPosition(cameraPositionForSampling);
		MixerSnapshot mixerSnapshot = null;
		if (sceneRoot != null)
		{
			mixerSnapshot = sceneRoot.SceneSettings.DefaultMixerSnapshot;
		}
		if (mixerSnapshot == null)
		{
			mixerSnapshot = this.DefaultSceneSnapshot;
		}
		if (mixerSnapshot != this.m_currentSceneMixerSnapshot)
		{
			if (this.m_currentSceneMixerSnapshot != null)
			{
				this.m_currentSceneMixerSnapshot.FadeOut();
			}
			if (mixerSnapshot != null)
			{
				mixerSnapshot.FadeIn();
			}
		}
		this.m_currentSceneMixerSnapshot = mixerSnapshot;
		for (int i = 0; i < this.m_snapshotZones.Count; i++)
		{
			MixerSnapshotZone mixerSnapshotZone = this.m_snapshotZones[i];
			mixerSnapshotZone.UpdateSnapshotZoneState(mixerSnapshotZone.Bounds.Contains(cameraPositionForSampling));
		}
	}

	public static AudioMixer GetMasterMixer()
	{
		if (MixerManager.s_cachedMasterMixer == null)
		{
			MixerManager.s_cachedMasterMixer = (AudioMixer)Resources.Load("masterMixer", typeof(AudioMixer));
		}
		return MixerManager.s_cachedMasterMixer;
	}

	public static AudioMixerGroup GetMixerGroup(MixerGroupType group)
	{
		AudioMixerGroup audioMixerGroup;
		if (!MixerManager.s_typeToGroup.TryGetValue(group, out audioMixerGroup))
		{
			switch (group)
			{
			case MixerGroupType.Foley:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("foley")[0];
				break;
			case MixerGroupType.Footsteps:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("footsteps")[0];
				break;
			case MixerGroupType.EnemiesAttack:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("enemiesAttack")[0];
				break;
			case MixerGroupType.EnemiesFoley:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("enemiesFoley")[0];
				break;
			case MixerGroupType.AmbienceQuad:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("ambienceQuad")[0];
				break;
			case MixerGroupType.AmbiencePoint:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("ambiencePoint")[0];
				break;
			case MixerGroupType.Attacks:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("attacks")[0];
				break;
			case MixerGroupType.Destruction:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("destruction")[0];
				break;
			case MixerGroupType.UI:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("ui")[0];
				break;
			case MixerGroupType.SpiritTree:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("spiritTree")[0];
				break;
			case MixerGroupType.Sein:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("sein")[0];
				break;
			case MixerGroupType.Doors:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("doors")[0];
				break;
			case MixerGroupType.Cutscenes:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("cutscenes")[0];
				break;
			case MixerGroupType.Props:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("props")[0];
				break;
			case MixerGroupType.Collectibles:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("collectibles")[0];
				break;
			case MixerGroupType.MusicStingers:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("stingers")[0];
				break;
			case MixerGroupType.MusicLoops:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("loops")[0];
				break;
			default:
				audioMixerGroup = MixerManager.GetMasterMixer().FindMatchingGroups("Master")[0];
				break;
			}

			MixerManager.s_typeToGroup.Add(group, audioMixerGroup);
		}
		return audioMixerGroup;
	}

	public static void WarmUpResource()
	{
		MixerManager.GetMasterMixer();
	}

	public void ApplySoundCompression()
	{
		if (RandomizerSettings.Accessibility.ApplySoundCompression)
		{
			float multiplier = 1f - RandomizerSettings.Accessibility.SoundCompressionFactor;
			this.m_settings.MusicLoops = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.MusicLoops));
			this.m_settings.MusicStingers = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.MusicStingers));
			this.m_settings.AmbienceQuad = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.AmbienceQuad));
			this.m_settings.AmbiencePoint = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.AmbiencePoint));
			this.m_settings.EnemiesAttack = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.EnemiesAttack));
			this.m_settings.EnemiesFoley = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.EnemiesFoley));
			this.m_settings.Foley = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.Foley));
			this.m_settings.Footsteps = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.Footsteps));
			this.m_settings.Attacks = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.Attacks));
			this.m_settings.Destruction = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.Destruction));
			this.m_settings.UI = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.UI));
			this.m_settings.SpiritTree = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.SpiritTree));
			this.m_settings.Sein = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.Sein));
			this.m_settings.Doors = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.Doors));
			this.m_settings.Cutscenes = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.Cutscenes));
			this.m_settings.Props = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.Props));
			this.m_settings.Collectibles = Mathf.Pow(10f, multiplier * Mathf.Log10(this.m_settings.Collectibles));
		}
	}

	public MixerSnapshot DefaultSceneSnapshot;

	public MixerSnapshot UISnapshot;

	public MixerSnapshot ModulatingSnapshot;

	private MixerGroupSettings m_currentMixerGroupSettings;

	private bool m_wasInUI;

	private static readonly Predicate<MixerSnapshot> CachedIsSnapshotInactivePredicate = (MixerSnapshot snapshot) => snapshot.State == MixerSnapshot.MixerSnapshotState.Inactive;

	private static Dictionary<MixerGroupType, AudioMixerGroup> s_typeToGroup = new Dictionary<MixerGroupType, AudioMixerGroup>();

	private List<MixerSnapshot> m_currentlyActiveSnapshots = new List<MixerSnapshot>(10);

	private static AudioMixer s_cachedMasterMixer = null;

	private static MixerManager s_manager;

	private MixerGroupSettings m_settings = default(MixerGroupSettings);

	private List<MixerSnapshotZone> m_snapshotZones = new List<MixerSnapshotZone>(5);

	private MixerSnapshot m_currentSceneMixerSnapshot;
}
