using System;
using Core;
using Game;
using Sein.World;
using UnityEngine;

// Token: 0x02000367 RID: 871
public class SeinSoulFlame : CharacterState, ISeinReceiver
{
	// Token: 0x0600136B RID: 4971 RVA: 0x0006DFB8 File Offset: 0x0006C1B8
	public SeinSoulFlame()
	{
	}

	// Token: 0x0600136C RID: 4972 RVA: 0x0001141D File Offset: 0x0000F61D
	static SeinSoulFlame()
	{
		SeinSoulFlame.OnSoulFlameCast = delegate
		{
		};
	}

	// Token: 0x1400001C RID: 28
	// (add) Token: 0x0600136D RID: 4973 RVA: 0x00011441 File Offset: 0x0000F641
	// (remove) Token: 0x0600136E RID: 4974 RVA: 0x00011458 File Offset: 0x0000F658
	public static event Action OnSoulFlameCast;

	// Token: 0x17000389 RID: 905
	// (get) Token: 0x0600136F RID: 4975 RVA: 0x0001146F File Offset: 0x0000F66F
	public bool SoulFlameExists
	{
		get
		{
			return this.m_checkpointMarkerGameObject;
		}
	}

	// Token: 0x1700038A RID: 906
	// (get) Token: 0x06001370 RID: 4976 RVA: 0x0001147C File Offset: 0x0000F67C
	public Vector3 SoulFlamePosition
	{
		get
		{
			return this.m_checkpointMarkerGameObject.transform.position;
		}
	}

	// Token: 0x06001371 RID: 4977 RVA: 0x0001148E File Offset: 0x0000F68E
	public new void Awake()
	{
		base.Awake();
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
		Game.Events.Scheduler.OnGameReset.Add(new Action(this.OnGameReset));
	}

	// Token: 0x06001372 RID: 4978 RVA: 0x000114C7 File Offset: 0x0000F6C7
	public void OnGameReset()
	{
		this.m_numberOfSoulFlamesCast = 0;
	}

	// Token: 0x06001373 RID: 4979 RVA: 0x000114D0 File Offset: 0x0000F6D0
	public void OnRestoreCheckpoint()
	{
		if (this.CanAffordSoulFlame)
		{
			this.m_cooldownRemaining = 0f;
		}
		this.LockSoulFlame = false;
		this.m_nagTimer = this.NagDuration;
	}

	// Token: 0x06001374 RID: 4980 RVA: 0x0006E008 File Offset: 0x0006C208
	public override void OnDestroy()
	{
		base.OnDestroy();
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
		Game.Events.Scheduler.OnGameReset.Remove(new Action(this.OnGameReset));
		if (this.m_checkpointMarkerGameObject)
		{
			InstantiateUtility.Destroy(this.m_checkpointMarkerGameObject);
			this.m_soulFlame = null;
			this.m_checkpointMarkerGameObject = null;
		}
	}

	// Token: 0x06001375 RID: 4981 RVA: 0x000114FB File Offset: 0x0000F6FB
	public void FillSoulFlameBar()
	{
		this.m_cooldownRemaining = 0f;
		this.m_nagTimer = 0f;
	}

	// Token: 0x1700038B RID: 907
	// (get) Token: 0x06001376 RID: 4982 RVA: 0x00011513 File Offset: 0x0000F713
	public bool InsideCheckpointMarker
	{
		get
		{
			return this.m_soulFlame && this.m_soulFlame.IsInside;
		}
	}

	// Token: 0x1700038C RID: 908
	// (get) Token: 0x06001377 RID: 4983 RVA: 0x0006E078 File Offset: 0x0006C278
	public SeinSoulFlame.SoulFlamePlacementSafety IsSafeToCastSoulFlame
	{
		get
		{
			Vector3 position = this.m_sein.Position;
			for (int i = 0; i < NoSoulFlameZone.All.Count; i++)
			{
				NoSoulFlameZone noSoulFlameZone = NoSoulFlameZone.All[i];
				if (noSoulFlameZone.BoundingRect.Contains(position))
				{
					return SeinSoulFlame.SoulFlamePlacementSafety.UnsafeZone;
				}
			}
			if (!Sein.World.Events.DarknessLifted && SpiritLightDarknessZone.IsInsideDarknessZone(position) && !SaveInTheDarkZone.IsInside(position) && !LightSource.TestPosition(position, 0f))
			{
				return SeinSoulFlame.SoulFlamePlacementSafety.UnsafeZone;
			}
			for (int j = 0; j < SavePedestal.All.Count; j++)
			{
				SavePedestal savePedestal = SavePedestal.All[j];
				if (savePedestal.IsInside)
				{
					return SeinSoulFlame.SoulFlamePlacementSafety.SavePedestal;
				}
			}
			for (int k = 0; k < this.m_sein.Abilities.SpiritFlameTargetting.ClosestAttackables.Count; k++)
			{
				ISpiritFlameAttackable spiritFlameAttackable = this.m_sein.Abilities.SpiritFlameTargetting.ClosestAttackables[k];
				EntityTargetting entityTargetting = spiritFlameAttackable as EntityTargetting;
				if (entityTargetting && entityTargetting.Entity is Enemy)
				{
					return SeinSoulFlame.SoulFlamePlacementSafety.UnsafeEnemies;
				}
			}
			for (int l = 0; l < RespawningPlaceholder.All.Count; l++)
			{
				RespawningPlaceholder respawningPlaceholder = RespawningPlaceholder.All[l];
				if (!respawningPlaceholder.EntityIsDead && Vector3.Distance(position, respawningPlaceholder.Position) < 10f)
				{
					return SeinSoulFlame.SoulFlamePlacementSafety.UnsafeEnemies;
				}
			}
			if (this.m_sein.Mortality.DamageReciever.IsInvinsible)
			{
				return SeinSoulFlame.SoulFlamePlacementSafety.UnsafeZone;
			}
			Collider groundCollider = this.m_sein.PlatformBehaviour.PlatformMovementListOfColliders.GroundCollider;
			if (groundCollider)
			{
				if (groundCollider.attachedRigidbody)
				{
					return SeinSoulFlame.SoulFlamePlacementSafety.UnsafeGround;
				}
				if (groundCollider.GetComponent<HeatUpPlatform>())
				{
					return SeinSoulFlame.SoulFlamePlacementSafety.UnsafeGround;
				}
			}
			bool flag = Physics.SphereCast(new Ray(position, Vector3.right), 0.5f, 0.7f, this.UnsafeMask);
			flag |= Physics.SphereCast(new Ray(position, -Vector3.right), 0.5f, 0.7f, this.UnsafeMask);
			if (flag)
			{
				return SeinSoulFlame.SoulFlamePlacementSafety.UnsafeGround;
			}
			return SeinSoulFlame.SoulFlamePlacementSafety.Safe;
		}
	}

	// Token: 0x1700038D RID: 909
	// (get) Token: 0x06001378 RID: 4984 RVA: 0x00011533 File Offset: 0x0000F733
	public float BarValue
	{
		get
		{
			return (1f - this.CooldownRemaining) * (1f - this.m_holdDownTime);
		}
	}

	// Token: 0x1700038E RID: 910
	// (get) Token: 0x06001379 RID: 4985 RVA: 0x0001154E File Offset: 0x0000F74E
	public float CooldownRemaining
	{
		get
		{
			return this.m_cooldownRemaining;
		}
	}

	// Token: 0x1700038F RID: 911
	// (get) Token: 0x0600137A RID: 4986 RVA: 0x00011556 File Offset: 0x0000F756
	public bool ShowFlameOnUI
	{
		get
		{
			return Mathf.Approximately(this.BarValue, 1f);
		}
	}

	// Token: 0x17000390 RID: 912
	// (get) Token: 0x0600137B RID: 4987 RVA: 0x00011568 File Offset: 0x0000F768
	public float SoulFlameCost
	{
		get
		{
			return (!this.m_sein.PlayerAbilities.SoulFlameEfficiency.HasAbility) ? 1f : 0.5f;
		}
	}

	// Token: 0x17000391 RID: 913
	// (get) Token: 0x0600137C RID: 4988 RVA: 0x00011593 File Offset: 0x0000F793
	public bool CanAffordSoulFlame
	{
		get
		{
			return this.m_sein.Energy.CanAfford(this.SoulFlameCost);
		}
	}

	// Token: 0x17000392 RID: 914
	// (get) Token: 0x0600137D RID: 4989 RVA: 0x000115AB File Offset: 0x0000F7AB
	public bool AllowedToAccessSkillTree
	{
		get
		{
			return this.m_sein.Level.Current % 128 > 0 && this.IsSafeToCastSoulFlame == SeinSoulFlame.SoulFlamePlacementSafety.Safe;
		}
	}

	// Token: 0x17000393 RID: 915
	// (get) Token: 0x0600137E RID: 4990 RVA: 0x0006E2C8 File Offset: 0x0006C4C8
	public bool PlayerCouldSoulFlame
	{
		get
		{
			return Characters.Sein.Controller.CanMove && !this.m_sein.Controller.IsSwimming && !UI.Fader.IsFadingInOrStay() && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && !this.LockSoulFlame;
		}
	}

	// Token: 0x0600137F RID: 4991 RVA: 0x0006E324 File Offset: 0x0006C524
	public void HandleNagging()
	{
		if (this.m_readyForReadySequence && this.PlayerCouldSoulFlame && this.IsSafeToCastSoulFlame == SeinSoulFlame.SoulFlamePlacementSafety.Safe && this.CanAffordSoulFlame)
		{
			this.m_readyForReadySequence = false;
			InstantiateUtility.Instantiate(this.SoulFlameReadyText, Characters.Ori.transform.position, Quaternion.identity);
			UI.SeinUI.OnSoulFlameReady();
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.SoulFlameReadyEffect);
			gameObject.transform.parent = Characters.Ori.transform;
			gameObject.transform.localPosition = Vector3.zero;
			Sound.Play(this.SoulFlameReadySoundProvider.GetSound(null), Characters.Sein.Position, null);
			this.m_nagTimer = this.NagDuration;
		}
		if (this.m_nagTimer > 0f)
		{
			this.m_nagTimer -= Time.deltaTime;
			if (this.m_nagTimer <= 0f)
			{
				if (this.PlayerCouldSoulFlame && this.CanAffordSoulFlame && this.IsSafeToCastSoulFlame == SeinSoulFlame.SoulFlamePlacementSafety.Safe)
				{
					this.m_nagTimer = 0f;
					InstantiateUtility.Instantiate(this.SoulFlameReadyText, Characters.Ori.transform.position, Quaternion.identity);
					UI.SeinUI.OnSoulFlameReady();
					Sound.Play(this.SoulFlameReadySoundProvider.GetSound(null), Characters.Sein.Position, null);
					this.m_nagTimer = this.NagDuration;
				}
				else
				{
					this.m_nagTimer = 2f;
				}
			}
		}
	}

	// Token: 0x06001380 RID: 4992 RVA: 0x000115D1 File Offset: 0x0000F7D1
	public void HandleDelayOnGround()
	{
		if (!this.m_sein.IsOnGround)
		{
			this.m_delayOnGround = 0.1f;
		}
		else
		{
			this.m_delayOnGround = Mathf.Max(0f, this.m_delayOnGround - Time.deltaTime);
		}
	}

	// Token: 0x06001381 RID: 4993 RVA: 0x0006E4AC File Offset: 0x0006C6AC
	public override void UpdateCharacterState()
	{
		if (this.m_sein.Controller.IsBashing)
		{
			return;
		}
		this.HandleDelayOnGround();
		this.HandleCooldown();
		this.HandleCheckpointMarkerVisibility();
		this.HandleNagging();
		this.HandleSkillTreeHint();
		this.HandleCharging();
		if (this.m_sein.Energy.Max == 0f)
		{
			this.m_cooldownRemaining = 1f;
		}
		if (!UI.Fader.IsFadingInOrStay())
		{
			if (Core.Input.SoulFlame.OnPressed && !Core.Input.SoulFlame.Used && !Core.Input.Cancel.Used)
			{
				this.m_isCasting = true;
				if (this.InsideCheckpointMarker)
				{
					this.m_tapRemainingTime = 0.3f;
				}
				else if (!this.CanAffordSoulFlame)
				{
					this.HideOtherMessages();
					UI.SeinUI.ShakeEnergyOrbBar();
					this.m_sein.Energy.NotifyOutOfEnergy();
				}
				else if (this.m_cooldownRemaining != 0f)
				{
					this.HideOtherMessages();
					this.m_notReadyHint = UI.Hints.Show(this.NotReadyMessage, HintLayer.SoulFlame, 1f);
					Sound.Play(this.NotReadySound.GetSound(null), base.transform.position, null);
				}
				else if (this.IsSafeToCastSoulFlame != SeinSoulFlame.SoulFlamePlacementSafety.Safe)
				{
					this.HideOtherMessages();
					switch (this.IsSafeToCastSoulFlame)
					{
					case SeinSoulFlame.SoulFlamePlacementSafety.UnsafeEnemies:
						this.m_notSafeHint = UI.Hints.Show(this.NotSafeEnemiesMessage, HintLayer.SoulFlame, 1f);
						break;
					case SeinSoulFlame.SoulFlamePlacementSafety.UnsafeGround:
						this.m_notSafeHint = UI.Hints.Show(this.NotSafeGroundMessage, HintLayer.SoulFlame, 1f);
						break;
					case SeinSoulFlame.SoulFlamePlacementSafety.UnsafeZone:
						this.m_notSafeHint = UI.Hints.Show(this.NotSafeZoneMessage, HintLayer.SoulFlame, 1f);
						break;
					case SeinSoulFlame.SoulFlamePlacementSafety.SavePedestal:
						this.m_notSafeHint = UI.Hints.Show(this.SavePedestalZoneMessage, HintLayer.SoulFlame, 1f);
						break;
					}
					Sound.Play(this.NotSafeSound.GetSound(null), base.transform.position, null);
				}
			}
			if (this.m_isCasting && this.m_sein.IsOnGround && this.m_delayOnGround == 0f && this.m_tapRemainingTime > 0f)
			{
				this.m_tapRemainingTime -= Time.deltaTime;
				if (this.m_tapRemainingTime < 0f && this.InsideCheckpointMarker && Characters.Sein.PlayerAbilities.Rekindle.HasAbility && this.IsSafeToCastSoulFlame == SeinSoulFlame.SoulFlamePlacementSafety.Safe)
				{
					SeinSoulFlame.OnSoulFlameCast();
					Vector3 position = Characters.Sein.Position;
					Characters.Sein.Position = this.m_soulFlame.Position;
					SaveSlotBackupsManager.CreateCurrentBackup();
					GameController.Instance.CreateCheckpoint();
					Characters.Sein.Position = position;
					GameController.Instance.SaveGameController.PerformSave();
					this.m_soulFlame.OnRekindle();
					GameController.Instance.PerformSaveGameSequence();
				}
			}
			if (Core.Input.SoulFlame.Released)
			{
				this.m_isCasting = false;
				if (this.m_tapRemainingTime > 0f)
				{
					this.m_tapRemainingTime = 0f;
					if (this.AllowedToAccessSkillTree && this.InsideCheckpointMarker)
					{
						if (this.m_skillTreeHint)
						{
							this.m_skillTreeHint.Visibility.HideImmediately();
						}
						Core.Input.Start.Used = true;
						UI.Menu.ShowSkillTree();
					}
				}
			}
		}
		else
		{
			this.m_tapRemainingTime = 0f;
		}
		if (this.m_holdDownTime == 1f && this.m_sein.IsOnGround && this.m_delayOnGround == 0f)
		{
			this.CastSoulFlame();
		}
	}

	// Token: 0x06001382 RID: 4994 RVA: 0x0006E870 File Offset: 0x0006CA70
	public void CastSoulFlame()
	{
		if (this.ChargingSound)
		{
			this.ChargingSound.StopAndFadeOut(0.1f);
		}
		this.m_sein.Energy.Spend(this.SoulFlameCost);
		this.m_cooldownRemaining = 1f;
		this.m_holdDownTime = 0f;
		if (this.m_sein.PlayerAbilities.Regroup.HasAbility)
		{
			this.m_sein.Mortality.Health.GainHealth(4);
		}
		if (this.m_sein.PlayerAbilities.UltraSoulFlame.HasAbility)
		{
			this.m_sein.Mortality.Health.GainHealth(4);
		}
		this.m_sceneCheckpoint = new MoonGuid(Scenes.Manager.CurrentScene.SceneMoonGuid);
		if (this.m_checkpointMarkerGameObject)
		{
			this.m_checkpointMarkerGameObject.GetComponent<SoulFlame>().Disappear();
		}
		this.SpawnSoulFlame(Characters.Sein.Position);
		Randomizer.LastSoulLink = Characters.Sein.Position;
		SeinSoulFlame.OnSoulFlameCast();
		SaveSlotBackupsManager.CreateCurrentBackup();
		GameController.Instance.CreateCheckpoint();
		GameController.Instance.SaveGameController.PerformSave();
		this.m_numberOfSoulFlamesCast++;
		if (this.m_numberOfSoulFlamesCast == 50)
		{
			AchievementsController.AwardAchievement(AchievementsLogic.Instance.SoulLinkManyTimesAchievementAsset);
		}
		if (this.CheckpointSequence)
		{
			this.CheckpointSequence.Perform(null);
		}
	}

	// Token: 0x06001383 RID: 4995 RVA: 0x0006E9E4 File Offset: 0x0006CBE4
	public void HandleCharging()
	{
		if (this.m_isCasting && this.CanAffordSoulFlame && this.IsSafeToCastSoulFlame == SeinSoulFlame.SoulFlamePlacementSafety.Safe && this.m_cooldownRemaining == 0f && !this.InsideCheckpointMarker && this.PlayerCouldSoulFlame)
		{
			if (this.m_holdDownTime == 0f && this.ChargingSound)
			{
				this.ChargingSound.Play();
			}
			this.m_holdDownTime += Time.deltaTime / this.HoldDownDuration;
			if (this.m_holdDownTime > 1f)
			{
				this.m_holdDownTime = 1f;
			}
			this.ChargeEffectAnimator.AnimatorDriver.ContinueForward();
		}
		else
		{
			this.ChargeEffectAnimator.AnimatorDriver.ContinueBackwards();
			if (this.ChargingSound && this.ChargingSound.IsPlaying)
			{
				this.ChargingSound.StopAndFadeOut(0.1f);
			}
			if (this.m_holdDownTime > 0f)
			{
				if (this.AbortChargingSound && !this.AbortChargingSound.IsPlaying)
				{
					this.AbortChargingSound.Play();
				}
				this.m_holdDownTime -= Time.deltaTime / this.HoldDownDuration;
				if (this.m_holdDownTime <= 0f)
				{
					this.m_holdDownTime = 0f;
					if (this.AbortChargingSound)
					{
						this.AbortChargingSound.StopAndFadeOut(0.1f);
					}
					if (this.FullyAbortedSound)
					{
						Sound.Play(this.FullyAbortedSound.GetSound(null), base.transform.position, null);
					}
				}
			}
		}
	}

	// Token: 0x06001384 RID: 4996 RVA: 0x0006EBAC File Offset: 0x0006CDAC
	public void HandleCooldown()
	{
		if (this.m_cooldownRemaining > 0f)
		{
			this.m_nagTimer = 0f;
			if (this.m_sein.PlayerAbilities.Rekindle.HasAbility)
			{
				this.m_cooldownRemaining -= Time.deltaTime / this.RekindleCooldownDuration;
			}
			else
			{
				this.m_cooldownRemaining -= Time.deltaTime / this.CooldownDuration;
			}
			if (this.m_cooldownRemaining <= 0f)
			{
				this.m_cooldownRemaining = 0f;
				this.m_readyForReadySequence = true;
			}
		}
	}

	// Token: 0x06001385 RID: 4997 RVA: 0x0006EC48 File Offset: 0x0006CE48
	public void HandleCheckpointMarkerVisibility()
	{
		if (this.m_checkpointMarkerGameObject)
		{
			bool flag = Scenes.Manager.SceneIsEnabled(this.m_sceneCheckpoint);
			bool flag2 = UI.Cameras.Current.IsOnScreenPadded(this.m_soulFlame.Position, 5f);
			if (this.m_checkpointMarkerGameObject.activeSelf)
			{
				if (!flag && !flag2)
				{
					this.m_checkpointMarkerGameObject.SetActive(false);
				}
			}
			else if (flag)
			{
				this.m_checkpointMarkerGameObject.SetActive(true);
			}
		}
	}

	// Token: 0x06001386 RID: 4998 RVA: 0x0006ECD0 File Offset: 0x0006CED0
	public void HandleSkillTreeHint()
	{
		if (this.AllowedToAccessSkillTree)
		{
			if (this.InsideCheckpointMarker && this.SkillTreeMessage && this.SkillTreeRekindleMessage && this.PlayerCouldSoulFlame)
			{
				if (this.m_skillTreeHint == null)
				{
					MessageProvider messageProvider = (!Characters.Sein.PlayerAbilities.Rekindle.HasAbility || this.IsSafeToCastSoulFlame != SeinSoulFlame.SoulFlamePlacementSafety.Safe) ? this.SkillTreeMessage : this.SkillTreeRekindleMessage;
					this.m_skillTreeHint = UI.Hints.Show(messageProvider, HintLayer.SoulFlame, float.PositiveInfinity);
				}
			}
			else if (this.m_skillTreeHint)
			{
				this.m_skillTreeHint.HideMessageScreen();
			}
		}
	}

	// Token: 0x06001387 RID: 4999 RVA: 0x0001160F File Offset: 0x0000F80F
	public void HideOtherMessages()
	{
		if (this.m_notReadyHint)
		{
			this.m_notReadyHint.HideMessageScreen();
		}
		if (this.m_notSafeHint)
		{
			this.m_notSafeHint.HideMessageScreen();
		}
	}

	// Token: 0x06001388 RID: 5000 RVA: 0x00011647 File Offset: 0x0000F847
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.SoulFlame = this;
	}

	// Token: 0x06001389 RID: 5001 RVA: 0x0006ED98 File Offset: 0x0006CF98
	public override void Serialize(Archive ar)
	{
		base.Serialize(ar);
		ar.Serialize(ref this.m_cooldownRemaining);
		ar.Serialize(ref this.m_readyForReadySequence);
		ar.Serialize(ref this.m_nagTimer);
		this.m_sceneCheckpoint.Serialize(ar);
		ar.Serialize(ref this.m_numberOfSoulFlamesCast);
		if (ar.Writing)
		{
			ar.Serialize(this.m_soulFlame != null);
			if (this.m_soulFlame)
			{
				ar.Serialize(this.m_soulFlame.Position);
			}
		}
		else
		{
			bool flag = false;
			ar.Serialize(ref flag);
			if (flag)
			{
				Vector3 zero = Vector3.zero;
				ar.Serialize(ref zero);
				if (this.m_soulFlame)
				{
					this.m_soulFlame.Position = zero;
				}
				else
				{
					this.SpawnSoulFlame(zero);
				}
			}
			else
			{
				this.DestroySoulFlame();
			}
		}
	}

	// Token: 0x0600138A RID: 5002 RVA: 0x0001165C File Offset: 0x0000F85C
	public void SpawnSoulFlame(Vector3 position)
	{
		this.m_checkpointMarkerGameObject = (GameObject)InstantiateUtility.Instantiate(this.CheckpointMarker, position, Quaternion.identity);
		this.m_soulFlame = this.m_checkpointMarkerGameObject.GetComponent<SoulFlame>();
	}

	// Token: 0x0600138B RID: 5003 RVA: 0x0001168B File Offset: 0x0000F88B
	public void DestroySoulFlame()
	{
		if (this.m_soulFlame)
		{
			InstantiateUtility.Destroy(this.m_soulFlame.gameObject);
			this.m_soulFlame = null;
			this.m_checkpointMarkerGameObject = null;
		}
	}

	// Token: 0x04001236 RID: 4662
	public BaseAnimator ChargeEffectAnimator;

	// Token: 0x04001237 RID: 4663
	public GameObject CheckpointMarker;

	// Token: 0x04001238 RID: 4664
	public ActionMethod CheckpointSequence;

	// Token: 0x04001239 RID: 4665
	public AnimationCurve ParticleRateOverSpeed;

	// Token: 0x0400123A RID: 4666
	public AchievementAsset CreateManySoulLinkAchievement;

	// Token: 0x0400123B RID: 4667
	public MessageProvider SkillTreeRekindleMessage;

	// Token: 0x0400123C RID: 4668
	public MessageProvider SkillTreeMessage;

	// Token: 0x0400123D RID: 4669
	public MessageProvider NotSafeZoneMessage;

	// Token: 0x0400123E RID: 4670
	public MessageProvider NotSafeEnemiesMessage;

	// Token: 0x0400123F RID: 4671
	public MessageProvider NotSafeGroundMessage;

	// Token: 0x04001240 RID: 4672
	public MessageProvider SavePedestalZoneMessage;

	// Token: 0x04001241 RID: 4673
	public MessageProvider NotReadyMessage;

	// Token: 0x04001242 RID: 4674
	public LayerMask UnsafeMask;

	// Token: 0x04001243 RID: 4675
	public MessageBox m_notSafeHint;

	// Token: 0x04001244 RID: 4676
	public MessageBox m_notReadyHint;

	// Token: 0x04001245 RID: 4677
	public MessageBox m_skillTreeHint;

	// Token: 0x04001246 RID: 4678
	public GameObject m_checkpointMarkerGameObject;

	// Token: 0x04001247 RID: 4679
	public SoulFlame m_soulFlame;

	// Token: 0x04001248 RID: 4680
	public SeinCharacter m_sein;

	// Token: 0x04001249 RID: 4681
	public int m_numberOfSoulFlamesCast;

	// Token: 0x0400124A RID: 4682
	public float m_holdDownTime;

	// Token: 0x0400124B RID: 4683
	public float HoldDownDuration = 0.7f;

	// Token: 0x0400124C RID: 4684
	public float m_nagTimer;

	// Token: 0x0400124D RID: 4685
	public float NagDuration = 120f;

	// Token: 0x0400124E RID: 4686
	public bool LockSoulFlame;

	// Token: 0x0400124F RID: 4687
	public SoundProvider NotSafeSound;

	// Token: 0x04001250 RID: 4688
	public SoundProvider NotReadySound;

	// Token: 0x04001251 RID: 4689
	public SoundSource ChargingSound;

	// Token: 0x04001252 RID: 4690
	public SoundSource AbortChargingSound;

	// Token: 0x04001253 RID: 4691
	public SoundProvider FullyAbortedSound;

	// Token: 0x04001254 RID: 4692
	public SoundProvider SoulFlameReadySoundProvider;

	// Token: 0x04001255 RID: 4693
	public GameObject SoulFlameReadyEffect;

	// Token: 0x04001256 RID: 4694
	public GameObject SoulFlameReadyText;

	// Token: 0x04001257 RID: 4695
	public float CooldownDuration = 60f;

	// Token: 0x04001258 RID: 4696
	public float RekindleCooldownDuration = 10f;

	// Token: 0x04001259 RID: 4697
	public float m_cooldownRemaining;

	// Token: 0x0400125A RID: 4698
	public bool m_readyForReadySequence;

	// Token: 0x0400125B RID: 4699
	public float m_tapRemainingTime;

	// Token: 0x0400125C RID: 4700
	public MoonGuid m_sceneCheckpoint = new MoonGuid(0, 0, 0, 0);

	// Token: 0x0400125D RID: 4701
	public bool m_isCasting;

	// Token: 0x0400125E RID: 4702
	public float m_delayOnGround;

	// Token: 0x02000368 RID: 872
	public enum SoulFlamePlacementSafety
	{
		// Token: 0x04001262 RID: 4706
		Safe,
		// Token: 0x04001263 RID: 4707
		UnsafeEnemies,
		// Token: 0x04001264 RID: 4708
		UnsafeGround,
		// Token: 0x04001265 RID: 4709
		UnsafeZone,
		// Token: 0x04001266 RID: 4710
		SavePedestal
	}
}
