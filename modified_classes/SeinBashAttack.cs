using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000309 RID: 777
public class SeinBashAttack : CharacterState, ISeinReceiver
{
	// Token: 0x06000F88 RID: 3976 RVA: 0x0005EC30 File Offset: 0x0005CE30
	public SeinBashAttack()
	{
	}

	// Token: 0x06000F89 RID: 3977 RVA: 0x0005ECA8 File Offset: 0x0005CEA8
	static SeinBashAttack()
	{
		SeinBashAttack.OnBashAttackEvent = delegate(Vector2 A_0)
		{
		};
		SeinBashAttack.OnBashBegin = delegate
		{
		};
		SeinBashAttack.OnBashEnemy = delegate(EntityTargetting A_0)
		{
		};
	}

	// Token: 0x1400000E RID: 14
	// (add) Token: 0x06000F8A RID: 3978 RVA: 0x0005ECF4 File Offset: 0x0005CEF4
	// (remove) Token: 0x06000F8B RID: 3979 RVA: 0x0005ED28 File Offset: 0x0005CF28
	public static event Action<Vector2> OnBashAttackEvent;

	// Token: 0x1400000F RID: 15
	// (add) Token: 0x06000F8C RID: 3980 RVA: 0x0005ED5C File Offset: 0x0005CF5C
	// (remove) Token: 0x06000F8D RID: 3981 RVA: 0x0005ED90 File Offset: 0x0005CF90
	public static event Action OnBashBegin;

	// Token: 0x14000010 RID: 16
	// (add) Token: 0x06000F8E RID: 3982 RVA: 0x0005EDC4 File Offset: 0x0005CFC4
	// (remove) Token: 0x06000F8F RID: 3983 RVA: 0x0005EDF8 File Offset: 0x0005CFF8
	public static event Action<EntityTargetting> OnBashEnemy;

	// Token: 0x17000271 RID: 625
	// (get) Token: 0x06000F90 RID: 3984 RVA: 0x0000D9E1 File Offset: 0x0000BBE1
	public Component TargetAsComponent
	{
		get
		{
			return this.Target as Component;
		}
	}

	// Token: 0x17000272 RID: 626
	// (get) Token: 0x06000F91 RID: 3985 RVA: 0x0000D9EE File Offset: 0x0000BBEE
	public CharacterAirNoDeceleration AirNoDeceleration
	{
		get
		{
			return this.Sein.PlatformBehaviour.AirNoDeceleration;
		}
	}

	// Token: 0x17000273 RID: 627
	// (get) Token: 0x06000F92 RID: 3986 RVA: 0x0000DA00 File Offset: 0x0000BC00
	public SeinDoubleJump DoubleJump
	{
		get
		{
			return this.Sein.Abilities.DoubleJump;
		}
	}

	// Token: 0x17000274 RID: 628
	// (get) Token: 0x06000F93 RID: 3987 RVA: 0x0000DA12 File Offset: 0x0000BC12
	public CharacterApplyFrictionToSpeed ApplyFrictionToSpeed
	{
		get
		{
			return this.Sein.PlatformBehaviour.ApplyFrictionToSpeed;
		}
	}

	// Token: 0x17000275 RID: 629
	// (get) Token: 0x06000F94 RID: 3988 RVA: 0x0000DA24 File Offset: 0x0000BC24
	public CharacterGravity Gravity
	{
		get
		{
			return this.Sein.PlatformBehaviour.Gravity;
		}
	}

	// Token: 0x17000276 RID: 630
	// (get) Token: 0x06000F95 RID: 3989 RVA: 0x0000DA36 File Offset: 0x0000BC36
	public CharacterLeftRightMovement CharacterLeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	// Token: 0x17000277 RID: 631
	// (get) Token: 0x06000F96 RID: 3990 RVA: 0x0000DA48 File Offset: 0x0000BC48
	public PlayerAbilities PlayerAbilities
	{
		get
		{
			return this.Sein.PlayerAbilities;
		}
	}

	// Token: 0x17000278 RID: 632
	// (get) Token: 0x06000F97 RID: 3991 RVA: 0x0000DA55 File Offset: 0x0000BC55
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x17000279 RID: 633
	// (get) Token: 0x06000F98 RID: 3992 RVA: 0x0000DA67 File Offset: 0x0000BC67
	public SeinController SeinController
	{
		get
		{
			return this.Sein.Controller;
		}
	}

	// Token: 0x1700027A RID: 634
	// (get) Token: 0x06000F99 RID: 3993 RVA: 0x0005EE2C File Offset: 0x0005D02C
	public TextureAnimationWithTransitions BashChargeAnimation
	{
		get
		{
			Vector2 vector = this.m_directionToTarget;
			float num = Mathf.Cos(0.3926991f);
			SeinBashAttack.DirectionalAnimationSet directionalAnimationSet = (!this.Sein.Controller.IsSwimming) ? this.BashChargeAnimationSet : this.SwimBashChargeAnimationSet;
			vector.x = Mathf.Abs(vector.x);
			if (Vector3.Dot(Vector3.up, vector) > num)
			{
				return directionalAnimationSet.Up;
			}
			Vector3 vector2 = new Vector3(1f, 1f);
			if (Vector3.Dot(vector2.normalized, vector) > num)
			{
				return directionalAnimationSet.UpDiagonal;
			}
			if (Vector3.Dot(Vector3.right, vector) > num)
			{
				return directionalAnimationSet.Horizontal;
			}
			Vector3 vector3 = new Vector3(1f, -1f);
			if (Vector3.Dot(vector3.normalized, vector) > num)
			{
				return directionalAnimationSet.DownDiagonal;
			}
			if (Vector3.Dot(Vector3.down, vector) > num)
			{
				return directionalAnimationSet.Down;
			}
			return directionalAnimationSet.Up;
		}
	}

	// Token: 0x1700027B RID: 635
	// (get) Token: 0x06000F9A RID: 3994 RVA: 0x0005EF34 File Offset: 0x0005D134
	public TextureAnimationWithTransitions BashJumpAnimation
	{
		get
		{
			Vector2 vector = MoonMath.Angle.VectorFromAngle(this.m_bashAngle + 90f);
			float num = Mathf.Cos(0.3926991f);
			SeinBashAttack.DirectionalAnimationSet directionalAnimationSet = (!this.Sein.Controller.IsSwimming) ? this.BashJumpAnimationSet : this.SwimBashJumpAnimationSet;
			vector.x = Mathf.Abs(vector.x);
			if (Vector3.Dot(Vector3.up, vector) > num)
			{
				return directionalAnimationSet.Up;
			}
			Vector3 vector2 = new Vector3(1f, 1f);
			if (Vector3.Dot(vector2.normalized, vector) > num)
			{
				return directionalAnimationSet.UpDiagonal;
			}
			if (Vector3.Dot(Vector3.right, vector) > num)
			{
				return directionalAnimationSet.Horizontal;
			}
			Vector3 vector3 = new Vector3(1f, -1f);
			if (Vector3.Dot(vector3.normalized, vector) > num)
			{
				return directionalAnimationSet.DownDiagonal;
			}
			if (Vector3.Dot(Vector3.down, vector) > num)
			{
				return directionalAnimationSet.Down;
			}
			return directionalAnimationSet.Up;
		}
	}

	// Token: 0x1700027C RID: 636
	// (get) Token: 0x06000F9B RID: 3995 RVA: 0x0000DA74 File Offset: 0x0000BC74
	// (set) Token: 0x06000F9C RID: 3996 RVA: 0x0005F040 File Offset: 0x0005D240
	public bool SpriteMirrorLock
	{
		get
		{
			return this.m_spriteMirrorLock;
		}
		set
		{
			if (this.m_spriteMirrorLock != value)
			{
				this.m_spriteMirrorLock = value;
				int @lock;
				if (value)
				{
					CharacterSpriteMirror spriteMirror = this.Sein.PlatformBehaviour.Visuals.SpriteMirror;
					@lock = spriteMirror.Lock;
					spriteMirror.Lock = @lock + 1;
					return;
				}
				CharacterSpriteMirror spriteMirror2 = this.Sein.PlatformBehaviour.Visuals.SpriteMirror;
				@lock = spriteMirror2.Lock;
				spriteMirror2.Lock = @lock - 1;
			}
		}
	}

	// Token: 0x1700027D RID: 637
	// (get) Token: 0x06000F9D RID: 3997 RVA: 0x0005F0AC File Offset: 0x0005D2AC
	public bool CanBash
	{
		get
		{
			return this.PlayerAbilities.Bash.HasAbility && !(this.TargetAsComponent == null) && this.TargetAsComponent.gameObject.activeInHierarchy && (!(this.Sein != null) || this.Sein.Active) && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities);
		}
	}

	// Token: 0x06000F9E RID: 3998 RVA: 0x0000DA7C File Offset: 0x0000BC7C
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.m_seinTransform = this.Sein.transform;
		this.Sein.Abilities.Bash = this;
	}

	// Token: 0x06000F9F RID: 3999 RVA: 0x0005F114 File Offset: 0x0005D314
	public void Start()
	{
		this.m_hasStarted = true;
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += this.ModifyHorizontalPlatformMovementSettings;
		this.Gravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyGravityPlatformMovementSettings;
	}

	// Token: 0x06000FA0 RID: 4000 RVA: 0x0005F16C File Offset: 0x0005D36C
	public new void OnDestroy()
	{
		base.OnDestroy();
		if (this.m_hasStarted)
		{
			Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
			this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= this.ModifyHorizontalPlatformMovementSettings;
			this.Gravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyGravityPlatformMovementSettings;
		}
	}

	// Token: 0x06000FA1 RID: 4001 RVA: 0x0000DAA7 File Offset: 0x0000BCA7
	public void ModifyGravityPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (this.IsBashing)
		{
			settings.GravityStrength = 0f;
		}
	}

	// Token: 0x06000FA2 RID: 4002 RVA: 0x0000DABC File Offset: 0x0000BCBC
	public void ModifyHorizontalPlatformMovementSettings(HorizontalPlatformMovementSettings settings)
	{
		if (this.IsBashing)
		{
			settings.LockInput = true;
		}
	}

	// Token: 0x06000FA3 RID: 4003 RVA: 0x0000DACD File Offset: 0x0000BCCD
	public void OnRestoreCheckpoint()
	{
		if (this.IsBashing)
		{
			this.ExitBash();
		}
		this.ApplyFrictionToSpeed.SpeedFactor = 0f;
		this.m_spriteMirrorLock = false;
	}

	// Token: 0x06000FA4 RID: 4004 RVA: 0x0000DAF4 File Offset: 0x0000BCF4
	public void OnDisable()
	{
		if (this.IsBashing)
		{
			this.ExitBash();
		}
	}

	// Token: 0x06000FA5 RID: 4005 RVA: 0x0000DB04 File Offset: 0x0000BD04
	public void ExitBash()
	{
		if (GameController.Instance)
		{
			GameController.Instance.ResumeGameplay();
		}
		this.ApplyFrictionToSpeed.SpeedFactor = 0f;
		this.IsBashing = false;
	}

	// Token: 0x06000FA6 RID: 4006 RVA: 0x0005F1CC File Offset: 0x0005D3CC
	public void MovePlayerToTargetAndCreateEffect()
	{
		Component component = this.Target as Component;
		Vector3 vector = (!InstantiateUtility.IsDestroyed(component)) ? component.transform.position : this.PlatformMovement.Position;
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.BashFromFx);
		gameObject.transform.position = vector;
		Vector3 localScale = gameObject.transform.localScale;
		localScale.x = (vector - this.PlatformMovement.Position).magnitude;
		gameObject.transform.localScale = localScale;
		gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, MoonMath.Angle.AngleFromVector(this.PlatformMovement.Position - vector));
		if (!this.PlatformMovement.IsOnGround)
		{
			this.PlatformMovement.Position2D = vector;
		}
	}

	// Token: 0x06000FA7 RID: 4007 RVA: 0x0005F2AC File Offset: 0x0005D4AC
	public void BeginBash()
	{
		this.m_timeRemainingOfBashButtonPress = 0f;
		this.IsBashing = true;
		this.Target.OnEnterBash();
		Transform transform = this.TargetAsComponent.transform;
		Sound.Play((!this.Sein.PlayerAbilities.BashBuff.HasAbility) ? this.BashStartSound.GetSound(null) : this.UpgradedBashStartSound.GetSound(null), this.m_seinTransform.position, null);
		if (GameController.Instance)
		{
			GameController.Instance.SuspendGameplay();
		}
		if (UI.Cameras.Current != null)
		{
			SuspensionManager.GetSuspendables(this.m_bashSuspendables, UI.Cameras.Current.GameObject);
			SuspensionManager.Resume(this.m_bashSuspendables);
			this.m_bashSuspendables.Clear();
		}
		this.PlatformMovement.LocalSpeed = Vector2.zero;
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.BashAttackGamePrefab);
		this.m_bashAttackGame = gameObject.GetComponent<BashAttackGame>();
		this.m_bashAttackGame.SendDirection(transform.position - this.PlatformMovement.Position);
		this.m_bashAttackGame.BashGameComplete += this.BashGameComplete;
		this.m_bashAttackGame.transform.position = transform.position;
		Vector3 b = Vector3.ClampMagnitude(transform.position - this.PlatformMovement.Position, 2f);
		this.m_playerTargetPosition = transform.position - b;
		this.m_directionToTarget = b.normalized;
		SeinBashAttack.OnBashBegin();
		this.Sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.BashChargeAnimation, 10, new Func<bool>(this.ShouldBashChargeAnimationKeepPlaying), false);
	}

	// Token: 0x06000FA8 RID: 4008 RVA: 0x0000DB33 File Offset: 0x0000BD33
	public void BashGameComplete(float angle)
	{
		this.JumpOffTarget(angle);
		this.AttackTarget();
		this.ExitBash();
	}

	// Token: 0x06000FA9 RID: 4009 RVA: 0x0005F470 File Offset: 0x0005D670
	public void JumpOffTarget(float angle)
	{
		if (GameController.Instance)
		{
			GameController.Instance.ResumeGameplay();
		}
		Vector2 vector = Quaternion.Euler(0f, 0f, angle) * Vector2.up;
		Vector2 vector2 = vector * (this.BashVelocity  + this.BashVelocity * .10f * RandomizerBonus.Velocity());
		this.PlatformMovement.WorldSpeed = vector2;
		this.AirNoDeceleration.NoDeceleration = true;
		this.Sein.ResetAirLimits();
		this.m_frictionTimeRemaining = this.FrictionDuration;
		this.ApplyFrictionToSpeed.SpeedToSlowDown = this.PlatformMovement.LocalSpeed;
		this.MovePlayerToTargetAndCreateEffect();
		Component component = this.Target as Component;
		Vector3 position = (!InstantiateUtility.IsDestroyed(component)) ? component.transform.position : this.Sein.Position;
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.BashOffFx);
		gameObject.transform.position = position;
		Vector3 localScale = gameObject.transform.localScale;
		localScale.x = vector2.magnitude * 0.1f;
		gameObject.transform.localScale = localScale;
		gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, MoonMath.Angle.AngleFromVector(vector));
		if (this.BashReleaseEffect)
		{
			((GameObject)InstantiateUtility.Instantiate(this.BashReleaseEffect)).transform.position = position;
		}
		SeinBashAttack.OnBashAttackEvent(vector2);
		this.m_timeRemainingTillNextBash = this.DelayTillNextBash;
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.BashJumpAnimation, 10, new Func<bool>(this.ShouldBashJumpAnimationKeepPlaying));
		characterAnimationState.OnStartPlaying = new Action(this.OnAnimationStart);
		characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
		this.Sein.PlatformBehaviour.Visuals.SpriteMirror.FaceLeft = (vector2.x > 0f);
		if (this.Sein.Abilities.Swimming)
		{
			this.Sein.Abilities.Swimming.OnBash(angle);
		}
	}

	// Token: 0x06000FAA RID: 4010 RVA: 0x0000DB48 File Offset: 0x0000BD48
	public void OnAnimationStart()
	{
		this.SpriteMirrorLock = true;
	}

	// Token: 0x06000FAB RID: 4011 RVA: 0x0005F690 File Offset: 0x0005D890
	public void AttackTarget()
	{
		Component component = this.Target as Component;
		if (!InstantiateUtility.IsDestroyed(component))
		{
			Vector2 force = -MoonMath.Angle.VectorFromAngle(this.m_bashAngle + 90f) * (4f + (float)RandomizerBonus.Velocity());
			new Damage(RandomizerBonusSkill.AbilityDamage((!this.Sein.PlayerAbilities.BashBuff.HasAbility) ? this.Damage : this.UpgradedDamage), force, Characters.Sein.Position, DamageType.Bash, base.gameObject).DealToComponents(component.gameObject);			EntityTargetting component2 = component.gameObject.GetComponent<EntityTargetting>();
			if (component2 && component2.Entity is Enemy)
			{
				SeinBashAttack.OnBashEnemy(component2);
			}
			if (this.Sein.PlayerAbilities.BashBuff.HasAbility)
			{
				this.BeginBashThroughEnemies();
			}
		}
	}

	// Token: 0x06000FAC RID: 4012 RVA: 0x0000DB51 File Offset: 0x0000BD51
	public void BeginBashThroughEnemies()
	{
		this.m_bashThroughEnemiesRemainingTime = 0.5f;
		this.Sein.Mortality.DamageReciever.MakeInvincibleToEnemies(this.m_bashThroughEnemiesRemainingTime);
		this.m_enemiesBashedThrough.Clear();
	}

	// Token: 0x06000FAD RID: 4013 RVA: 0x0005F768 File Offset: 0x0005D968
	public void UpdateBashThroughEnemies()
	{
		if (this.m_bashThroughEnemiesRemainingTime > 0f)
		{
			this.m_bashThroughEnemiesRemainingTime -= Time.deltaTime;
			for (int i = 0; i < Targets.Attackables.Count; i++)
			{
				IAttackable attackable = Targets.Attackables[i];
				if (attackable.CanBeSpiritFlamed() && !this.m_enemiesBashedThrough.Contains(attackable))
				{
					Vector3 vector = attackable.Position - this.Sein.PlatformBehaviour.PlatformMovement.Position;
					if (vector.magnitude < 3f && Vector2.Dot(vector.normalized, this.PlatformMovement.LocalSpeed.normalized) > 0f)
					{
						Damage damage = new Damage(this.UpgradedDamage, this.PlatformMovement.WorldSpeed.normalized, this.Sein.Position, DamageType.SpiritFlame, base.gameObject);
						GameObject gameObject = ((Component)attackable).gameObject;
						damage.DealToComponents(gameObject);
						this.m_enemiesBashedThrough.Add(attackable);
						break;
					}
				}
			}
			if (this.m_bashThroughEnemiesRemainingTime <= 0f)
			{
				this.m_bashThroughEnemiesRemainingTime = 0f;
				this.FinishBashThroughEnemies();
			}
		}
	}

	// Token: 0x06000FAE RID: 4014 RVA: 0x0000DB84 File Offset: 0x0000BD84
	public void FinishBashThroughEnemies()
	{
		this.m_enemiesBashedThrough.Clear();
	}

	// Token: 0x06000FAF RID: 4015 RVA: 0x0005F8A8 File Offset: 0x0005DAA8
	public void UpdateBashingState()
	{
		this.HandleBashAngle();
		this.Sein.Mortality.DamageReciever.MakeInvincibleToEnemies(0.2f);
		this.HandleMovingTowardsBashTarget();
		this.Sein.PlatformBehaviour.Visuals.SpriteMirror.FaceLeft = (this.m_directionToTarget.x < 0f);
	}

	// Token: 0x06000FB0 RID: 4016 RVA: 0x0000DB91 File Offset: 0x0000BD91
	public void BashFailed()
	{
		if (this.NoBashTargetEffect)
		{
			((GameObject)InstantiateUtility.Instantiate(this.NoBashTargetEffect, base.transform.position, Quaternion.identity)).transform.parent = this.m_seinTransform;
		}
	}

	// Token: 0x06000FB1 RID: 4017 RVA: 0x0005F908 File Offset: 0x0005DB08
	public void UpdateNormalState()
	{
		Randomizer.BashWasQueued = Randomizer.QueueBash;
		if (Core.Input.Bash.OnPressed || Randomizer.QueueBash)
		{
			Randomizer.QueueBash = false;
			this.m_timeRemainingOfBashButtonPress = 0.5f;
			if (this.Sein.IsOnGround && this.Sein.Speed.x == 0f && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && !this.Sein.Abilities.Carry.IsCarrying)
			{
				this.Sein.Animation.Play(this.BackFlipAnimation, 10, null);
				this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedY = this.BackFlipSpeed;
				if ((!this.Sein.PlayerAbilities.BashBuff.HasAbility) ? this.StationaryBashSound : this.UpgradedStationaryBashSound)
				{
					Sound.Play((!this.Sein.PlayerAbilities.BashBuff.HasAbility) ? this.StationaryBashSound.GetSound(null) : this.UpgradedStationaryBashSound.GetSound(null), base.transform.position, null);
				}
			}
		}
		if (this.m_timeRemainingOfBashButtonPress > 0f)
		{
			this.m_timeRemainingOfBashButtonPress -= Time.deltaTime;
			if ((Core.Input.Bash.OnReleased || ((double)this.m_timeRemainingOfBashButtonPress <= 0.4 && (double)this.m_timeRemainingOfBashButtonPress >= 0.4 - (double)Time.deltaTime)) && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && !this.Sein.Abilities.Carry.IsCarrying)
			{
				this.BashFailed();
			}
			if (Core.Input.Bash.Released || this.m_timeRemainingOfBashButtonPress <= 0f)
			{
				this.m_timeRemainingOfBashButtonPress = 0f;
			}
		}
		if ((this.m_timeRemainingOfBashButtonPress > 0f || Randomizer.BashWasQueued) && this.CanBash)
		{
			this.BeginBash();
		}
		this.HandleFindingTarget();
		this.UpdateTargetHighlight(this.Target);
	}

	// Token: 0x06000FB2 RID: 4018 RVA: 0x0005FB10 File Offset: 0x0005DD10
	public override void UpdateCharacterState()
	{
		if (this.Sein.IsSuspended)
		{
			return;
		}
		if (!this.Sein.PlayerAbilities.Bash.HasAbility)
		{
			return;
		}
		if (!this.Sein.Active)
		{
			this.ExitBash();
			return;
		}
		if (this.m_timeRemainingTillNextBash > 0f)
		{
			this.m_timeRemainingTillNextBash -= Time.deltaTime;
		}
		this.UpdateBashThroughEnemies();
		if (this.m_frictionTimeRemaining > 0f)
		{
			this.m_frictionTimeRemaining -= Time.deltaTime;
			float time = this.FrictionDuration - this.m_frictionTimeRemaining;
			this.ApplyFrictionToSpeed.SpeedFactor = this.FrictionCurve.Evaluate(time);
		}
		if (this.m_frictionTimeRemaining + this.NoAirDecelerationDuration - this.FrictionDuration > 0f)
		{
			this.AirNoDeceleration.NoDeceleration = true;
		}
		if (this.IsBashing)
		{
			this.UpdateBashingState();
			return;
		}
		this.UpdateNormalState();
	}

	// Token: 0x06000FB3 RID: 4019 RVA: 0x0005FBFC File Offset: 0x0005DDFC
	public void HandleMovingTowardsBashTarget()
	{
		Vector3 a = this.m_playerTargetPosition - this.PlatformMovement.Position;
		this.PlatformMovement.WorldSpeed = a / Time.deltaTime * 0.1f;
	}

	// Token: 0x06000FB4 RID: 4020 RVA: 0x0000DBD0 File Offset: 0x0000BDD0
	public void HandleBashAngle()
	{
		if (!InstantiateUtility.IsDestroyed(this.m_bashAttackGame))
		{
			this.m_bashAngle = this.m_bashAttackGame.Angle;
		}
	}

	// Token: 0x06000FB5 RID: 4021 RVA: 0x0005FC48 File Offset: 0x0005DE48
	public void HandleFindingTarget()
	{
		if (this.Sein.Controller.IsCarrying)
		{
			this.Target = null;
			return;
		}
		if (this.m_timeRemainingTillNextBash > 0f)
		{
			this.Target = null;
			return;
		}
		if (this.PlayerAbilities.Bash.HasAbility)
		{
			this.Target = this.FindClosestAttackHandler();
			return;
		}
		this.Target = null;
	}

	// Token: 0x06000FB6 RID: 4022 RVA: 0x0005FCAC File Offset: 0x0005DEAC
	public void UpdateTargetHighlight(IBashAttackable target)
	{
		if (this.m_lastTarget == target)
		{
			return;
		}
		if (!InstantiateUtility.IsDestroyed(this.m_lastTarget as Component))
		{
			this.m_lastTarget.OnBashDehighlight();
		}
		this.m_lastTarget = target;
		if (!InstantiateUtility.IsDestroyed(this.m_lastTarget as Component))
		{
			this.m_lastTarget.OnBashHighlight();
		}
	}

	// Token: 0x06000FB7 RID: 4023 RVA: 0x0005FD04 File Offset: 0x0005DF04
	public IBashAttackable FindClosestAttackHandler()
	{
		IBashAttackable result = null;
		float num = float.MaxValue;
		int num2 = int.MinValue;
		Vector3 position = this.Sein.Position;
		for (int i = 0; i < Targets.Attackables.Count; i++)
		{
			IAttackable attackable = Targets.Attackables[i];
			if (attackable.CanBeBashed())
			{
				float magnitude = (attackable.Position - position).magnitude;
				if (magnitude <= this.Range)
				{
					IBashAttackable bashAttackable = attackable as IBashAttackable;
					if (bashAttackable != null)
					{
						int bashPriority = bashAttackable.BashPriority;
						if ((bashPriority > num2 || (magnitude <= num && bashPriority == num2)) && this.Sein.Controller.RayTest(((Component)bashAttackable).gameObject))
						{
							num = magnitude;
							num2 = bashPriority;
							result = bashAttackable;
						}
					}
				}
			}
		}
		return result;
	}

	// Token: 0x06000FB8 RID: 4024 RVA: 0x0000DBF0 File Offset: 0x0000BDF0
	public bool ShouldBashChargeAnimationKeepPlaying()
	{
		return this.IsBashing;
	}

	// Token: 0x06000FB9 RID: 4025 RVA: 0x0000DBF8 File Offset: 0x0000BDF8
	public bool ShouldBashJumpAnimationKeepPlaying()
	{
		return !this.PlatformMovement.IsOnGround;
	}

	// Token: 0x06000FBA RID: 4026 RVA: 0x0000DC08 File Offset: 0x0000BE08
	public void OnAnimationEnd()
	{
		this.SpriteMirrorLock = false;
	}

	// Token: 0x06000FBB RID: 4027 RVA: 0x0005FDD0 File Offset: 0x0005DFD0
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_timeRemainingOfBashButtonPress);
		ar.Serialize(ref this.m_frictionTimeRemaining);
		ar.Serialize(ref this.m_timeRemainingTillNextBash);
		ar.Serialize(ref this.m_spriteMirrorLock);
		base.Serialize(ar);
		if (ar.Reading && !InstantiateUtility.IsDestroyed(this.m_bashAttackGame))
		{
			InstantiateUtility.Destroy(this.m_bashAttackGame.gameObject);
		}
	}

	// Token: 0x04000EE7 RID: 3815
	public SeinBashAttack.DirectionalAnimationSet BashChargeAnimationSet;

	// Token: 0x04000EE8 RID: 3816
	public SeinBashAttack.DirectionalAnimationSet BashJumpAnimationSet;

	// Token: 0x04000EE9 RID: 3817
	public SeinBashAttack.DirectionalAnimationSet SwimBashChargeAnimationSet;

	// Token: 0x04000EEA RID: 3818
	public SeinBashAttack.DirectionalAnimationSet SwimBashJumpAnimationSet;

	// Token: 0x04000EEB RID: 3819
	public TextureAnimationWithTransitions BackFlipAnimation;

	// Token: 0x04000EEC RID: 3820
	public GameObject BashAttackGamePrefab;

	// Token: 0x04000EED RID: 3821
	public SoundProvider BashEndSound;

	// Token: 0x04000EEE RID: 3822
	public SoundProvider BashLoopSound;

	// Token: 0x04000EEF RID: 3823
	public SoundProvider BashStartSound;

	// Token: 0x04000EF0 RID: 3824
	public SoundProvider StationaryBashSound;

	// Token: 0x04000EF1 RID: 3825
	public SoundProvider UpgradedBashEndSound;

	// Token: 0x04000EF2 RID: 3826
	public SoundProvider UpgradedBashLoopSound;

	// Token: 0x04000EF3 RID: 3827
	public SoundProvider UpgradedBashStartSound;

	// Token: 0x04000EF4 RID: 3828
	public SoundProvider UpgradedStationaryBashSound;

	// Token: 0x04000EF5 RID: 3829
	public GameObject BashFromFx;

	// Token: 0x04000EF6 RID: 3830
	public GameObject BashOffFx;

	// Token: 0x04000EF7 RID: 3831
	public GameObject BashReleaseEffect;

	// Token: 0x04000EF8 RID: 3832
	public float BashVelocity = 56.568f;

	// Token: 0x04000EF9 RID: 3833
	public float Damage = 2f;

	// Token: 0x04000EFA RID: 3834
	public float UpgradedDamage = 5f;

	// Token: 0x04000EFB RID: 3835
	public float DelayTillNextBash  = 0.2f;

	// Token: 0x04000EFC RID: 3836
	public AnimationCurve FrictionCurve;

	// Token: 0x04000EFD RID: 3837
	public float FrictionDuration;

	// Token: 0x04000EFE RID: 3838
	public float NoAirDecelerationDuration = 0.2f;

	// Token: 0x04000EFF RID: 3839
	public float Range = 4f;

	// Token: 0x04000F00 RID: 3840
	public SeinCharacter Sein;

	// Token: 0x04000F01 RID: 3841
	public IBashAttackable Target;

	// Token: 0x04000F02 RID: 3842
	public Vector3 m_directionToTarget;

	// Token: 0x04000F03 RID: 3843
	public float m_bashAngle;

	// Token: 0x04000F04 RID: 3844
	public Vector3 m_playerTargetPosition;

	// Token: 0x04000F05 RID: 3845
	public BashAttackGame m_bashAttackGame;

	// Token: 0x04000F06 RID: 3846
	public float m_frictionTimeRemaining;

	// Token: 0x04000F07 RID: 3847
	public IBashAttackable m_lastTarget;

	// Token: 0x04000F08 RID: 3848
	public Transform m_seinTransform;

	// Token: 0x04000F09 RID: 3849
	public bool m_spriteMirrorLock;

	// Token: 0x04000F0A RID: 3850
	public float m_timeRemainingTillNextBash;

	// Token: 0x04000F0B RID: 3851
	public float m_timeRemainingOfBashButtonPress;

	// Token: 0x04000F0C RID: 3852
	public readonly HashSet<ISuspendable> m_bashSuspendables = new HashSet<ISuspendable>();

	// Token: 0x04000F0D RID: 3853
	public GameObject NoBashTargetEffect;

	// Token: 0x04000F0E RID: 3854
	public bool IsBashing;

	// Token: 0x04000F0F RID: 3855
	public float m_bashThroughEnemiesRemainingTime;

	// Token: 0x04000F10 RID: 3856
	public HashSet<IAttackable> m_enemiesBashedThrough = new HashSet<IAttackable>();

	// Token: 0x04000F11 RID: 3857
	public bool m_hasStarted;

	// Token: 0x04000F12 RID: 3858
	public float BackFlipSpeed = 5f;

	// Token: 0x0200030A RID: 778
	[Serializable]
	public class DirectionalAnimationSet
	{
		// Token: 0x06000FBC RID: 4028 RVA: 0x000024FF File Offset: 0x000006FF
		public DirectionalAnimationSet()
		{
		}

		// Token: 0x04000F16 RID: 3862
		public TextureAnimationWithTransitions Down;

		// Token: 0x04000F17 RID: 3863
		public TextureAnimationWithTransitions DownDiagonal;

		// Token: 0x04000F18 RID: 3864
		public TextureAnimationWithTransitions Horizontal;

		// Token: 0x04000F19 RID: 3865
		public TextureAnimationWithTransitions Up;

		// Token: 0x04000F1A RID: 3866
		public TextureAnimationWithTransitions UpDiagonal;
	}
}
