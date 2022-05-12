using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class TeleporterController : SaveSerialize, ISuspendable
{
	private void Nullify()
	{
		this.m_teleportingStartSound = null;
	}

	public override void Serialize(Archive ar)
	{
		// By default we just serialize 12 booleans, one for each default teleporter.
		// So if we only get 12 bytes of information or only have default teleporters then
		// we stick to that.
		// If there are more than 12 teleporters immediately after the 12 default teleporters
		// we serialize the number of extra teleporters, and then for each teleporter
		// serialise the name, location, and activation status.
		if (ar.Reading)
		{
			long readLength = ar.MemoryStream.Length;
			if (readLength < 12) {
				return;
			}
			// Read default teleporters.
			for (int i = 0; i < 12; i++)
			{
				GameMapTeleporter gameMapTeleporter = this.Teleporters[i];
				ar.Serialize(ref gameMapTeleporter.Activated);
			}
			// Determine extra teleporter count.
			int requiredCustomTeleporterCount = 0;
			if (readLength > 12) {
				ar.Serialize(ref requiredCustomTeleporterCount);
			}
			// Remove excess teleporters.
			while (this.Teleporters.Count > 12 + requiredCustomTeleporterCount)
			{	
				this.Teleporters.RemoveAt(this.Teleporters.Count - 1);
			}
			// Create or modify teleporters.
			for (int i = 0; i < requiredCustomTeleporterCount; i++)
			{
				string name = "???";
				Vector3 position = new Vector3(0,0,0);
				bool activated = false;
				ar.Serialize(ref name);
				ar.Serialize(ref position);
				ar.Serialize(ref activated);
				int currentTeleporterIndex = 12 + i;
				if (currentTeleporterIndex < this.Teleporters.Count) {
					// Alter the existing teleporter.
					this.Teleporters[currentTeleporterIndex].SetInfo(name, position, activated);
				} else {
					// Create a new teleporter.
					GameMapTeleporter gameMapTeleporter = new GameMapTeleporter(name, position, activated);
					this.Teleporters.Add(gameMapTeleporter);
				}
			}
		} else {
			// Writing.
			if (this.Teleporters.Count < 12) {
				return;
			}
			// Default teleporters.
			for (int i = 0; i < 12; i++)
			{
				GameMapTeleporter gameMapTeleporter = this.Teleporters[i];
				ar.Serialize(ref gameMapTeleporter.Activated);
			}
			// Extra teleporters.
			int customTeleporterCount = this.Teleporters.Count - 12;
			if (customTeleporterCount > 0) {
				ar.Serialize(ref customTeleporterCount);
			}
			for (int i = 12; i < this.Teleporters.Count; i++)
			{
				GameMapTeleporter gameMapTeleporter = this.Teleporters[i];
				ar.Serialize(ref gameMapTeleporter.Identifier);
				ar.Serialize(ref gameMapTeleporter.WorldPosition);
				ar.Serialize(ref gameMapTeleporter.Activated);
			}
		}
	}

	public static bool CanTeleport(string ignoreIdentifier)
	{
		if (TeleporterController.Instance)
		{
			for (int i = 0; i < TeleporterController.Instance.Teleporters.Count; i++)
			{
				GameMapTeleporter gameMapTeleporter = TeleporterController.Instance.Teleporters[i];
				if (!(gameMapTeleporter.Identifier == ignoreIdentifier))
				{
					if (gameMapTeleporter.Activated)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public override void Awake()
	{
		base.Awake();
		TeleporterController.Instance = this;
		SuspensionManager.Register(this);
		Events.Scheduler.OnGameReset.Add(new Action(this.OnGameReset));
		this.DontTeleportForAnimationTesting = false;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		TeleporterController.Instance = null;
		SuspensionManager.Unregister(this);
		Events.Scheduler.OnGameReset.Remove(new Action(this.OnGameReset));
	}

	public void OnGameReset()
	{
		for (int i = 0; i < TeleporterController.Instance.Teleporters.Count; i++)
		{
			TeleporterController.Instance.Teleporters[i].Activated = false;
		}
		this.CancelTeleport();
	}

	public void CancelTeleport()
	{
		Randomizer.IsUsingRandomizerTeleportAnywhere = false;
		this.m_isTeleporting = false;
		this.m_isBlooming = false;
		if (!InstantiateUtility.IsDestroyed(this.m_teleportingStartSound))
		{
			this.m_teleportingStartSound.FadeOut(0.1f, true);
			this.m_teleportingStartSound = null;
		}
	}

	public static void Show(string identifier)
	{
		UI.Menu.ShowWorldMap(false);
		GameMapUI.Instance.SetShowingTeleporters();
		GameMapUI.Instance.Teleporters.Select(identifier);
		AreaMapUI.Instance.Navigation.ScrollPosition = GameMapUI.Instance.Teleporters.SelectedTeleporter.WorldPosition;
		WorldMapUI.Instance.HideAreaSelection();
		if (GameMapUI.Instance.Teleporters.OpenWindowSound)
		{
			Sound.Play(GameMapUI.Instance.Teleporters.OpenWindowSound.GetSound(null), Vector3.zero, null);
		}
	}

	public static void OnClose()
	{
		GameMapUI.Instance.SetNormal();
	}

	public static bool ActivateAll()
	{
		foreach (GameMapTeleporter gameMapTeleporter in TeleporterController.Instance.Teleporters)
		{
			gameMapTeleporter.Activated = true;
		}
		return true;
	}

	public static void Activate(string identifier, bool natural)
	{
		if(natural)
			RandomizerSyncManager.FoundTP(identifier);
		BingoController.OnActivateTeleporter(identifier);
		foreach (GameMapTeleporter gameMapTeleporter in TeleporterController.Instance.Teleporters)
		{
			if (gameMapTeleporter.Identifier == identifier)
			{
				gameMapTeleporter.Activated = true;
			}
		}
	}

	public static void Activate(string identifier)
	{
		Activate(identifier, true);
	}

	public static void BeginTeleportation(GameMapTeleporter selectedTeleporter)
	{
		if (Vector3.Distance(selectedTeleporter.WorldPosition, Characters.Sein.Position) < 10f)
		{
			return;
		}
		if (selectedTeleporter.Area.Area.AreaNameString == "Forlorn Ruins")
		{
			Randomizer.NightBerryWarpPosition = selectedTeleporter.WorldPosition;
			Characters.Sein.Inventory.SetRandomizerItem(82, 1);
		}
		if (!TeleporterController.Instance.DontTeleportForAnimationTesting)
		{
			Scenes.Manager.AdditivelyLoadScenesAtPosition(selectedTeleporter.WorldPosition, true, false, true);
			TeleporterController.Instance.m_teleporterTargetPosition = selectedTeleporter.WorldPosition;
		}
		TeleporterController.Instance.m_isTeleporting = true;
		Characters.Sein.Controller.PlayAnimation(TeleporterController.Instance.TeleportingStartAnimation);
		if (GameMapUI.Instance.Teleporters.StartTeleportingSound)
		{
			Sound.Play(GameMapUI.Instance.Teleporters.StartTeleportingSound.GetSound(null), Vector3.zero, null);
		}
		if (Characters.Sein.Abilities.Carry && Characters.Sein.Abilities.Carry.CurrentCarryable != null)
		{
			Characters.Sein.Abilities.Carry.CurrentCarryable.Drop();
		}
		if (TeleporterController.Instance.TeleportingStartSound != null)
		{
			TeleporterController.Instance.m_teleportingStartSound = Sound.Play(TeleporterController.Instance.TeleportingStartSound.GetSound(null), Characters.Sein.Position, new Action(TeleporterController.Instance.Nullify));
		}
		Characters.Sein.Controller.OnTriggeredAnimationFinished += TeleporterController.OnFinishedTeleportingStartAnimation;
		TeleporterController.Instance.m_startTime = Time.time;
		foreach (SavePedestal savePedestal in SavePedestal.All)
		{
			savePedestal.OnBeginTeleporting();
		}
	}

	public static void OnFinishedTeleportingStartAnimation()
	{
		Characters.Sein.Controller.OnTriggeredAnimationFinished -= TeleporterController.OnFinishedTeleportingStartAnimation;
		if (TeleporterController.Instance.m_isTeleporting)
		{
			Characters.Sein.Controller.PlayAnimation(TeleporterController.Instance.TeleportingLoopAnimation);
			TeleporterController.Instance.TeleportingTwirlAnimationSound.Play();
		}
	}

	public void FixedUpdate()
	{
		if (this.m_isTeleporting)
		{
			float time = Time.time;
			float num = 7f;
			if (this.DontTeleportForAnimationTesting)
			{
				if (time > this.m_startTime + this.NoTeleportAnimationTime)
				{
					Characters.Sein.Controller.StopAnimation();
					Characters.Sein.Controller.PlayAnimation(TeleporterController.Instance.TeleportingFinishAnimation);
					TeleporterController.Instance.TeleportingTwirlAnimationSound.Stop();
					this.m_isTeleporting = false;
				}
			}
			else if (!Scenes.Manager.IsLoadingScenes && time > this.m_startTime + num)
			{
				this.m_isTeleporting = false;
				if (this.BloomFade)
				{
					InstantiateUtility.Instantiate(this.BloomFade);
					this.m_bloomCurrentTime = 0f;
					this.m_isBlooming = true;
					if (this.TeleportingBloomSound)
					{
						Sound.Play(this.TeleportingBloomSound.GetSound(null), Characters.Sein.Position, null);
					}
				}
				else
				{
					UI.Fader.Fade(0.5f, 0.05f, 0.2f, new Action(this.OnFadedToBlack), null);
				}
			}
		}
		if (this.m_isBlooming)
		{
			this.m_bloomCurrentTime += ((!this.IsSuspended) ? Time.deltaTime : 0f);
			if (this.m_bloomCurrentTime > this.BloomFadeDuration)
			{
				this.OnFadedToBlack();
				this.m_isBlooming = false;
			}
		}
	}

	public void OnFadedToBlack()
	{
		foreach (SavePedestal savePedestal in SavePedestal.All)
		{
			savePedestal.OnFinishedTeleporting();
		}
		if (!InstantiateUtility.IsDestroyed(this.m_teleportingStartSound))
		{
			this.m_teleportingStartSound.FadeOut(0.5f, true);
			this.m_teleportingStartSound = null;
		}
		if (this.BloomFade)
		{
			UberGCManager.CollectResourcesIfNeeded();
		}
		if (Randomizer.IsUsingRandomizerTeleportAnywhere)
		    RandomizerBonusSkill.LastAltR = Characters.Sein.Position;		
		Characters.Sein.Position = this.m_teleporterTargetPosition + Vector3.up * 1.6f;
		CameraPivotZone.InstantUpdate();
		Scenes.Manager.UpdatePosition();
		Scenes.Manager.UnloadScenesAtPosition(true);
		Scenes.Manager.EnableDisabledScenesAtPosition(false);
		Characters.Sein.Controller.StopAnimation();
		UI.Cameras.Current.MoveCameraToTargetInstantly(true);
		if (Characters.Ori)
		{
			Characters.Ori.BackToPlayerController();
		}
		GameController.Instance.CreateCheckpoint();
		GameController.Instance.PerformSaveGameSequence();
		RandomizerStatsManager.UsedTeleporter();

		if (Randomizer.IsUsingRandomizerTeleportAnywhere)
		{
		    int value = World.Events.Find(Randomizer.MistySim).Value;
		    if (value != 1 && value != 8)
		    {
		        World.Events.Find(Randomizer.MistySim).Value = 10;
		    }
		}

		LateStartHook.AddLateStartMethod(new Action(this.OnFinishedTeleporting));
	}

	public void OnFinishedTeleporting()
	{
		Randomizer.IsUsingRandomizerTeleportAnywhere = false;
		CameraFrustumOptimizer.ForceUpdate();
		Characters.Sein.Controller.PlayAnimation(TeleporterController.Instance.TeleportingFinishAnimation);
		if (GameMapUI.Instance.Teleporters.ReachDestinationTeleporterSound)
		{
			Sound.Play(GameMapUI.Instance.Teleporters.ReachDestinationTeleporterSound.GetSound(null), base.transform.position, null);
		}
		this.TeleportingTwirlAnimationSound.Stop();
		if (this.TeleporterFinishEffect)
		{
			InstantiateUtility.Instantiate(this.TeleporterFinishEffect, this.m_teleporterTargetPosition, Quaternion.identity);
		}
		if (this.TeleportingEndSound)
		{
			Sound.Play(this.TeleportingEndSound.GetSound(null), Characters.Sein.Position, null);
		}
		// Disable any sein locks that we got from teleporting from a physical savePedestal.
		Characters.Ori.ChangeState(Ori.State.Hovering);
		Characters.Ori.EnableHoverWobbling = true;
		if (Characters.Sein.Abilities.SpiritFlame)
		{
			Characters.Sein.Abilities.SpiritFlame.RemoveLock("savePedestal");
		}
	}

	public static void RemoveCustomTeleporters()
	{
		if (TeleporterController.Instance != null)
		{
			TeleporterController.Instance.Teleporters.RemoveAll((GameMapTeleporter teleporter) => teleporter.Name.GetType() == typeof(RandomizerMessageProvider));
		}
	}

	public static void AddCustomTeleporter(string name, float warpX, float warpY)
	{
		if (TeleporterController.Instance == null) {
			return;
		}
		// If we already have that teleporter don't add it.
		for (int i = 0; i < TeleporterController.Instance.Teleporters.Count; i++) 
		{
			if (TeleporterController.Instance.Teleporters[i].Identifier == name)
			{
				return;
			}
		}
		GameMapTeleporter teleporter = new GameMapTeleporter(name, warpX, warpY);
		TeleporterController.Instance.Teleporters.Add(teleporter);
    }

	public bool IsSuspended { get; set; }

	public static TeleporterController Instance;

	public TextureAnimationWithTransitions TeleportingStartAnimation;

	public TextureAnimationWithTransitions TeleportingLoopAnimation;

	public TextureAnimationWithTransitions TeleportingFinishAnimation;

	public SoundSource TeleportingTwirlAnimationSound;

	public SoundProvider TeleportingStartSound;

	public SoundProvider TeleportingBloomSound;

	public SoundProvider TeleportingEndSound;

	private SoundPlayer m_teleportingStartSound;

	private float m_startTime;

	public bool DontTeleportForAnimationTesting;

	public float NoTeleportAnimationTime = 6f;

	public List<GameMapTeleporter> Teleporters = new List<GameMapTeleporter>();

	public GameObject BloomFade;

	public float BloomFadeDuration;

	public GameObject TeleporterFinishEffect;

	private bool m_isTeleporting;

	private bool m_isBlooming;

	private float m_bloomCurrentTime;

	private Vector3 m_teleporterTargetPosition;
}
