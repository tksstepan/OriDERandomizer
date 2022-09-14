using System;
using Core;
using Game;
using Sein.World;
using UnityEngine;

public class SeinSoulFlame : CharacterState, ISeinReceiver
{
	static SeinSoulFlame()
	{
		SeinSoulFlame.OnSoulFlameCast = delegate()
		{
		};
	}

	public static event Action OnSoulFlameCast;

	public bool SoulFlameExists
	{
		get
		{
			return this.m_checkpointMarkerGameObject;
		}
	}

	public Vector3 SoulFlamePosition
	{
		get
		{
			return this.m_checkpointMarkerGameObject.transform.position;
		}
	}

	public new void Awake()
	{
		base.Awake();
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
		Game.Events.Scheduler.OnGameReset.Add(new Action(this.OnGameReset));
	}

	public void OnGameReset()
	{
		this.m_numberOfSoulFlamesCast = 0;
	}

	public void OnRestoreCheckpoint()
	{
		if (this.CanAffordSoulFlame)
		{
			this.m_cooldownRemaining = 0f;
		}
		this.LockSoulFlame = false;
		this.m_nagTimer = this.NagDuration;
	}

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

	public void FillSoulFlameBar()
	{
		this.m_cooldownRemaining = 0f;
		this.m_nagTimer = 0f;
	}

	public bool InsideCheckpointMarker
	{
		get
		{
			return this.m_soulFlame && this.m_soulFlame.IsInside;
		}
	}

	public SeinSoulFlame.SoulFlamePlacementSafety IsSafeToCastSoulFlame
	{
		get
		{
			Vector3 position = this.m_sein.Position;
			for (int i = 0; i < NoSoulFlameZone.All.Count; i++)
			{
				if (NoSoulFlameZone.All[i].BoundingRect.Contains(position))
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
				if (SavePedestal.All[j].IsInside)
				{
					return SeinSoulFlame.SoulFlamePlacementSafety.SavePedestal;
				}
			}
			for (int k = 0; k < this.m_sein.Abilities.SpiritFlameTargetting.ClosestAttackables.Count; k++)
			{
				EntityTargetting entityTargetting = this.m_sein.Abilities.SpiritFlameTargetting.ClosestAttackables[k] as EntityTargetting;
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
			if (Physics.SphereCast(new Ray(position, Vector3.right), 0.5f, 0.7f, this.UnsafeMask) | Physics.SphereCast(new Ray(position, -Vector3.right), 0.5f, 0.7f, this.UnsafeMask))
			{
				return SeinSoulFlame.SoulFlamePlacementSafety.UnsafeGround;
			}
			return SeinSoulFlame.SoulFlamePlacementSafety.Safe;
		}
	}

	public float BarValue
	{
		get
		{
			return (1f - this.CooldownRemaining) * (1f - this.m_holdDownTime);
		}
	}

	public float CooldownRemaining
	{
		get
		{
			return this.m_cooldownRemaining;
		}
	}

	public bool ShowFlameOnUI
	{
		get
		{
			return Mathf.Approximately(this.BarValue, 1f);
		}
	}

	public float SoulFlameCost
	{
		get
		{
			if (this.m_sein.PlayerAbilities.SoulFlameEfficiency.HasAbility)
			{
				return 0.5f;
			}
			return 1f;
		}
	}

	public bool CanAffordSoulFlame
	{
		get
		{
			return this.m_sein.Energy.CanAfford(this.SoulFlameCost);
		}
	}

	public bool AllowedToAccessSkillTree
	{
		get
		{
			return this.m_sein.Level.Current > 0 && this.IsSafeToCastSoulFlame == SeinSoulFlame.SoulFlamePlacementSafety.Safe;
		}
	}

	public bool PlayerCouldSoulFlame
	{
		get
		{
			return Characters.Sein.Controller.CanMove && !this.m_sein.Controller.IsSwimming && !UI.Fader.IsFadingInOrStay() && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && !this.LockSoulFlame;
		}
	}

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
					return;
				}
				this.m_nagTimer = 2f;
			}
		}
	}

	private void HandleDelayOnGround()
	{
		if (!this.m_sein.IsOnGround)
		{
			this.m_delayOnGround = 0.1f;
			return;
		}
		this.m_delayOnGround = Mathf.Max(0f, this.m_delayOnGround - Time.deltaTime);
	}

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

	private void CastSoulFlame()
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
		RandomizerBonusSkill.LastSoulLink = Characters.Sein.Position;
		RandomizerStatsManager.OnSave();
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

	private void HandleCharging()
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
			return;
		}
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

	private void HandleCooldown()
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

	private void HandleCheckpointMarkerVisibility()
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
					return;
				}
			}
			else if (flag)
			{
				this.m_checkpointMarkerGameObject.SetActive(true);
			}
		}
	}

	private void HandleSkillTreeHint()
	{
		if (this.AllowedToAccessSkillTree)
		{
			if (this.InsideCheckpointMarker && this.SkillTreeMessage && this.SkillTreeRekindleMessage && this.PlayerCouldSoulFlame)
			{
				if (this.m_skillTreeHint == null)
				{
					MessageProvider messageProvider = (!Characters.Sein.PlayerAbilities.Rekindle.HasAbility || this.IsSafeToCastSoulFlame != SeinSoulFlame.SoulFlamePlacementSafety.Safe) ? this.SkillTreeMessage : this.SkillTreeRekindleMessage;
					this.m_skillTreeHint = UI.Hints.Show(messageProvider, HintLayer.SoulFlame, float.PositiveInfinity);
					return;
				}
			}
			else if (this.m_skillTreeHint)
			{
				this.m_skillTreeHint.HideMessageScreen();
			}
		}
	}

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

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.SoulFlame = this;
	}

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
				return;
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
					return;
				}
				this.SpawnSoulFlame(zero);
				return;
			}
			else
			{
				this.DestroySoulFlame();
			}
		}
	}

	public void SpawnSoulFlame(Vector3 position)
	{
		this.m_checkpointMarkerGameObject = (GameObject)InstantiateUtility.Instantiate(this.CheckpointMarker, position, Quaternion.identity);
		this.m_soulFlame = this.m_checkpointMarkerGameObject.GetComponent<SoulFlame>();
	}

	public void DestroySoulFlame()
	{
		if (this.m_soulFlame)
		{
			InstantiateUtility.Destroy(this.m_soulFlame.gameObject);
			this.m_soulFlame = null;
			this.m_checkpointMarkerGameObject = null;
		}
	}

	public BaseAnimator ChargeEffectAnimator;

	public GameObject CheckpointMarker;

	public ActionMethod CheckpointSequence;

	public AnimationCurve ParticleRateOverSpeed;

	public AchievementAsset CreateManySoulLinkAchievement;

	public MessageProvider SkillTreeRekindleMessage;

	public MessageProvider SkillTreeMessage;

	public MessageProvider NotSafeZoneMessage;

	public MessageProvider NotSafeEnemiesMessage;

	public MessageProvider NotSafeGroundMessage;

	public MessageProvider SavePedestalZoneMessage;

	public MessageProvider NotReadyMessage;

	public LayerMask UnsafeMask;

	private MessageBox m_notSafeHint;

	private MessageBox m_notReadyHint;

	private MessageBox m_skillTreeHint;

	private GameObject m_checkpointMarkerGameObject;

	private SoulFlame m_soulFlame;

	private SeinCharacter m_sein;

	private int m_numberOfSoulFlamesCast;

	private float m_holdDownTime;

	public float HoldDownDuration = 0.7f;

	private float m_nagTimer;

	public float NagDuration = 120f;

	public bool LockSoulFlame;

	public SoundProvider NotSafeSound;

	public SoundProvider NotReadySound;

	public SoundSource ChargingSound;

	public SoundSource AbortChargingSound;

	public SoundProvider FullyAbortedSound;

	public SoundProvider SoulFlameReadySoundProvider;

	public GameObject SoulFlameReadyEffect;

	public GameObject SoulFlameReadyText;

	public float CooldownDuration = 60f;

	public float RekindleCooldownDuration = 10f;

	private float m_cooldownRemaining;

	private bool m_readyForReadySequence;

	private float m_tapRemainingTime;

	private MoonGuid m_sceneCheckpoint = new MoonGuid(0, 0, 0, 0);

	private bool m_isCasting;

	private float m_delayOnGround;

	public enum SoulFlamePlacementSafety
	{
		Safe,
		UnsafeEnemies,
		UnsafeGround,
		UnsafeZone,
		SavePedestal
	}
}
