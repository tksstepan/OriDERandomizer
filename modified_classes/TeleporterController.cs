using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200081F RID: 2079
public class TeleporterController : SaveSerialize, ISuspendable
{
	// Token: 0x06002CDF RID: 11487 RVA: 0x00024B81 File Offset: 0x00022D81
	public TeleporterController()
	{
	}

	// Token: 0x06002CE0 RID: 11488 RVA: 0x00024B9F File Offset: 0x00022D9F
	private void Nullify()
	{
		this.m_teleportingStartSound = null;
	}

	// Token: 0x06002CE1 RID: 11489 RVA: 0x000C39F4 File Offset: 0x000C1BF4
	public override void Serialize(Archive ar)
	{
		foreach (GameMapTeleporter gameMapTeleporter in this.Teleporters)
		{
			ar.Serialize(ref gameMapTeleporter.Activated);
		}
	}

	// Token: 0x06002CE2 RID: 11490 RVA: 0x000C3A54 File Offset: 0x000C1C54
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

	// Token: 0x06002CE3 RID: 11491 RVA: 0x00024BA8 File Offset: 0x00022DA8
	public override void Awake()
	{
		base.Awake();
		TeleporterController.Instance = this;
		SuspensionManager.Register(this);
		Events.Scheduler.OnGameReset.Add(new Action(this.OnGameReset));
		this.DontTeleportForAnimationTesting = false;
	}

	// Token: 0x06002CE4 RID: 11492 RVA: 0x00024BDE File Offset: 0x00022DDE
	public override void OnDestroy()
	{
		base.OnDestroy();
		TeleporterController.Instance = null;
		SuspensionManager.Unregister(this);
		Events.Scheduler.OnGameReset.Remove(new Action(this.OnGameReset));
	}

	// Token: 0x06002CE5 RID: 11493 RVA: 0x000C3AC8 File Offset: 0x000C1CC8
	public void OnGameReset()
	{
		for (int i = 0; i < TeleporterController.Instance.Teleporters.Count; i++)
		{
			TeleporterController.Instance.Teleporters[i].Activated = false;
		}
		this.m_isTeleporting = false;
		this.m_isBlooming = false;
		if (!InstantiateUtility.IsDestroyed(this.m_teleportingStartSound))
		{
			this.m_teleportingStartSound.FadeOut(0.1f, true);
			this.m_teleportingStartSound = null;
		}
	}

	// Token: 0x06002CE6 RID: 11494 RVA: 0x000C3B44 File Offset: 0x000C1D44
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

	// Token: 0x06002CE7 RID: 11495 RVA: 0x00024C0D File Offset: 0x00022E0D
	public static void OnClose()
	{
		GameMapUI.Instance.SetNormal();
	}

	// Token: 0x06002CE8 RID: 11496 RVA: 0x000C3BE4 File Offset: 0x000C1DE4
	public static bool ActivateAll()
	{
		foreach (GameMapTeleporter gameMapTeleporter in TeleporterController.Instance.Teleporters)
		{
			gameMapTeleporter.Activated = true;
		}
		return true;
	}

	// Token: 0x06002CE9 RID: 11497 RVA: 0x000C3C3C File Offset: 0x000C1E3C
	public static void Activate(string identifier)
	{
		BingoController.OnActivateTeleporter(identifier);
		foreach (GameMapTeleporter gameMapTeleporter in TeleporterController.Instance.Teleporters)
		{
			if (gameMapTeleporter.Identifier == identifier)
			{
				gameMapTeleporter.Activated = true;
			}
		}
	}

	// Token: 0x06002CEA RID: 11498
	public static void BeginTeleportation(GameMapTeleporter selectedTeleporter)
	{
		if (Vector3.Distance(selectedTeleporter.WorldPosition, Characters.Sein.Position) < 10f)
		{
			return;
		}
		if (selectedTeleporter.Identifier == "forlorn")
		{
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

	// Token: 0x06002CEB RID: 11499 RVA: 0x000C3E70 File Offset: 0x000C2070
	public static void OnFinishedTeleportingStartAnimation()
	{
		Characters.Sein.Controller.OnTriggeredAnimationFinished -= TeleporterController.OnFinishedTeleportingStartAnimation;
		if (TeleporterController.Instance.m_isTeleporting)
		{
			Characters.Sein.Controller.PlayAnimation(TeleporterController.Instance.TeleportingLoopAnimation);
			TeleporterController.Instance.TeleportingTwirlAnimationSound.Play();
		}
	}

	// Token: 0x06002CEC RID: 11500 RVA: 0x000C3ED0 File Offset: 0x000C20D0
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

	// Token: 0x06002CED RID: 11501 RVA: 0x000C4050 File Offset: 0x000C2250
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
		LateStartHook.AddLateStartMethod(new Action(this.OnFinishedTeleporting));
	}

	// Token: 0x06002CEE RID: 11502 RVA: 0x000C4188 File Offset: 0x000C2388
	public void OnFinishedTeleporting()
	{
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
	}

	// Token: 0x17000706 RID: 1798
	// (get) Token: 0x06002CEF RID: 11503 RVA: 0x00024C19 File Offset: 0x00022E19
	// (set) Token: 0x06002CF0 RID: 11504 RVA: 0x00024C21 File Offset: 0x00022E21
	public bool IsSuspended { get; set; }

	// Token: 0x04002860 RID: 10336
	public static TeleporterController Instance;

	// Token: 0x04002861 RID: 10337
	public TextureAnimationWithTransitions TeleportingStartAnimation;

	// Token: 0x04002862 RID: 10338
	public TextureAnimationWithTransitions TeleportingLoopAnimation;

	// Token: 0x04002863 RID: 10339
	public TextureAnimationWithTransitions TeleportingFinishAnimation;

	// Token: 0x04002864 RID: 10340
	public SoundSource TeleportingTwirlAnimationSound;

	// Token: 0x04002865 RID: 10341
	public SoundProvider TeleportingStartSound;

	// Token: 0x04002866 RID: 10342
	public SoundProvider TeleportingBloomSound;

	// Token: 0x04002867 RID: 10343
	public SoundProvider TeleportingEndSound;

	// Token: 0x04002868 RID: 10344
	private SoundPlayer m_teleportingStartSound;

	// Token: 0x04002869 RID: 10345
	private float m_startTime;

	// Token: 0x0400286A RID: 10346
	public bool DontTeleportForAnimationTesting;

	// Token: 0x0400286B RID: 10347
	public float NoTeleportAnimationTime = 6f;

	// Token: 0x0400286C RID: 10348
	public List<GameMapTeleporter> Teleporters = new List<GameMapTeleporter>();

	// Token: 0x0400286D RID: 10349
	public GameObject BloomFade;

	// Token: 0x0400286E RID: 10350
	public float BloomFadeDuration;

	// Token: 0x0400286F RID: 10351
	public GameObject TeleporterFinishEffect;

	// Token: 0x04002870 RID: 10352
	private bool m_isTeleporting;

	// Token: 0x04002871 RID: 10353
	private bool m_isBlooming;

	// Token: 0x04002872 RID: 10354
	private float m_bloomCurrentTime;

	// Token: 0x04002873 RID: 10355
	private Vector3 m_teleporterTargetPosition;
}
