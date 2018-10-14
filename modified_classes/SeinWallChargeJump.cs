using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200033A RID: 826
public class SeinWallChargeJump : CharacterState, ISeinReceiver
{
	// Token: 0x06001231 RID: 4657 RVA: 0x0000FFC8 File Offset: 0x0000E1C8
	public SeinWallChargeJump()
	{
	}

	// Token: 0x1700031B RID: 795
	// (get) Token: 0x06001232 RID: 4658 RVA: 0x0000FFF9 File Offset: 0x0000E1F9
	public PlayerAbilities PlayerAbilities
	{
		get
		{
			return this.m_sein.PlayerAbilities;
		}
	}

	// Token: 0x1700031C RID: 796
	// (get) Token: 0x06001233 RID: 4659 RVA: 0x00010006 File Offset: 0x0000E206
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.m_sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x06001234 RID: 4660 RVA: 0x00010018 File Offset: 0x0000E218
	public void OnDoubleJump()
	{
		this.ChangeState(SeinWallChargeJump.State.Normal);
	}

	// Token: 0x06001235 RID: 4661 RVA: 0x00010021 File Offset: 0x0000E221
	public override void UpdateCharacterState()
	{
		if (this.m_sein.IsSuspended)
		{
			return;
		}
		this.UpdateState();
	}

	// Token: 0x1700031D RID: 797
	// (get) Token: 0x06001236 RID: 4662 RVA: 0x0001003A File Offset: 0x0000E23A
	public float AngularElevation
	{
		get
		{
			return this.m_angularElevation;
		}
	}

	// Token: 0x06001237 RID: 4663 RVA: 0x00010042 File Offset: 0x0000E242
	public override void OnExit()
	{
		base.OnExit();
		this.ChangeState(SeinWallChargeJump.State.Normal);
	}

	// Token: 0x06001238 RID: 4664 RVA: 0x00010051 File Offset: 0x0000E251
	public void Start()
	{
		this.m_sein.PlatformBehaviour.Gravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyGravityPlatformMovementSettings;
	}

	// Token: 0x06001239 RID: 4665 RVA: 0x00010074 File Offset: 0x0000E274
	public override void OnDestroy()
	{
		base.OnDestroy();
		this.m_sein.PlatformBehaviour.Gravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyGravityPlatformMovementSettings;
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x0600123A RID: 4666 RVA: 0x000100B3 File Offset: 0x0000E2B3
	public void OnAnimationEnd()
	{
		this.SpriteMirrorLock = false;
	}

	// Token: 0x0600123B RID: 4667 RVA: 0x000100BC File Offset: 0x0000E2BC
	public void OnAnimationStart()
	{
		this.SpriteMirrorLock = true;
	}

	// Token: 0x0600123C RID: 4668 RVA: 0x000100C5 File Offset: 0x0000E2C5
	public void ModifyGravityPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (this.m_currentState == SeinWallChargeJump.State.Jumping)
		{
			settings.GravityStrength = 0f;
		}
	}

	// Token: 0x0600123D RID: 4669 RVA: 0x000694D4 File Offset: 0x000676D4
	public void ChangeState(SeinWallChargeJump.State state)
	{
		this.m_attackablesIgnore.Clear();
		SeinWallChargeJump.State currentState = this.m_currentState;
		if (currentState == SeinWallChargeJump.State.Aiming)
		{
			if (this.Arrow)
			{
				this.Arrow.AnimatorDriver.ContinueBackwards();
			}
		}
		this.m_currentState = state;
		this.m_stateCurrentTime = 0f;
		currentState = this.m_currentState;
		if (currentState != SeinWallChargeJump.State.Normal)
		{
			if (currentState == SeinWallChargeJump.State.Aiming)
			{
				if (this.m_sein.Abilities.GrabWall)
				{
					this.m_sein.Abilities.GrabWall.LockVerticalMovement = true;
				}
				if (this.Arrow)
				{
					this.Arrow.AnimatorDriver.ContinueForward();
				}
			}
		}
		else if (this.m_sein.Abilities.GrabWall)
		{
			this.m_sein.Abilities.GrabWall.LockVerticalMovement = false;
		}
	}

	// Token: 0x1700031E RID: 798
	// (get) Token: 0x0600123E RID: 4670 RVA: 0x000695DC File Offset: 0x000677DC
	public bool IsCharged
	{
		get
		{
			return this.m_sein.Controller.IsGrabbingWall && this.m_sein.Abilities.GrabWall.IsGrabbingAway && Characters.Sein.Controller.CanMove && this.m_sein.Abilities.ChargeJumpCharging.IsCharged;
		}
	}

	// Token: 0x1700031F RID: 799
	// (get) Token: 0x0600123F RID: 4671 RVA: 0x00069644 File Offset: 0x00067844
	public bool IsCharging
	{
		get
		{
			return this.m_sein.Controller.IsGrabbingWall && this.m_sein.Abilities.GrabWall.IsGrabbingAway && Characters.Sein.Controller.CanMove && this.m_sein.Abilities.ChargeJumpCharging.IsCharging;
		}
	}

	// Token: 0x06001240 RID: 4672 RVA: 0x000696AC File Offset: 0x000678AC
	public void UpdateState()
	{
		switch (this.m_currentState)
		{
		case SeinWallChargeJump.State.Normal:
			this.UpdateNormalState();
			break;
		case SeinWallChargeJump.State.Aiming:
			this.UpdateAimingState();
			break;
		case SeinWallChargeJump.State.Jumping:
			this.UpdateJumpingState();
			break;
		}
		this.m_stateCurrentTime += Time.deltaTime;
	}

	// Token: 0x06001241 RID: 4673 RVA: 0x0006970C File Offset: 0x0006790C
	public void UpdateNormalState()
	{
		if (this.PlayerAbilities.ChargeJump.HasAbility)
		{
			if (this.IsCharged)
			{
				this.ChangeState(SeinWallChargeJump.State.Aiming);
			}
			else if (this.IsCharging)
			{
				this.UpdateAimElevation();
			}
			else
			{
				this.m_angularElevation = 0f;
			}
		}
	}

	// Token: 0x06001242 RID: 4674 RVA: 0x00069768 File Offset: 0x00067968
	public void UpdateJumpingState()
	{
		float adjustedDrag = this.HorizontalDrag-this.HorizontalDrag*.10f*RandomizerBonus.Velocity();
		this.PlatformMovement.LocalSpeedX = this.PlatformMovement.LocalSpeedX * (1f - adjustedDrag);
		this.PlatformMovement.LocalSpeedY = this.PlatformMovement.LocalSpeedY * (1f - adjustedDrag);
		if (this.m_stateCurrentTime > (this.AntiGravityDuration+this.AntiGravityDuration*.10f*RandomizerBonus.Velocity()))
		{
			this.ChangeState(SeinWallChargeJump.State.Normal);
			return;
		}
		this.m_sein.PlatformBehaviour.Visuals.SpriteRotater.CenterAngle = this.m_angleDirection;
		this.m_sein.PlatformBehaviour.Visuals.SpriteRotater.UpdateRotation();
		for (int i = 0; i < Targets.Attackables.Count; i++)
		{
			IAttackable attackable = Targets.Attackables[i];
			if (!this.m_attackablesIgnore.Contains(attackable))
			{
				if (attackable.CanBeStomped())
				{
					Vector3 vector = attackable.Position - this.m_sein.PlatformBehaviour.PlatformMovement.Position;
					float magnitude = vector.magnitude;
					if (magnitude < 4f && Vector2.Dot(vector.normalized, this.PlatformMovement.LocalSpeed.normalized) > 0f)
					{
						this.m_attackablesIgnore.Add(attackable);
						Damage damage = new Damage((float)this.Damage, this.PlatformMovement.WorldSpeed.normalized * 3f, this.m_sein.Position, DamageType.Stomp, base.gameObject);
						damage.DealToComponents(((Component)attackable).gameObject);
						if (this.ExplosionEffect)
						{
							InstantiateUtility.Instantiate(this.ExplosionEffect, Vector3.Lerp(base.transform.position, attackable.Position, 0.5f), Quaternion.identity);
						}
						break;
					}
				}
			}
		}
	}

	// Token: 0x06001243 RID: 4675 RVA: 0x00069964 File Offset: 0x00067B64
	public void UpdateAimElevation()
	{
		bool hasWallLeft = this.PlatformMovement.HasWallLeft;
		Vector2 analogAxisLeft = Core.Input.AnalogAxisLeft;
		if (analogAxisLeft.magnitude > 0.2f)
		{
			this.m_angularElevation = Mathf.Atan2(analogAxisLeft.y, analogAxisLeft.x * (float)((!hasWallLeft) ? -1 : 1)) * 57.29578f;
		}
		else if (Core.Input.Up.Pressed && !Core.Input.Down.Pressed)
		{
			this.m_angularElevationSpeed = Mathf.Clamp(this.m_angularElevationSpeed + Time.deltaTime * 500f, 0f, 200f);
		}
		else if (Core.Input.Down.Pressed)
		{
			this.m_angularElevationSpeed = Mathf.Clamp(this.m_angularElevationSpeed - Time.deltaTime * 500f, -200f, 0f);
		}
		else
		{
			this.m_angularElevationSpeed = 0f;
		}
	}

	// Token: 0x06001244 RID: 4676 RVA: 0x00069A58 File Offset: 0x00067C58
	public void UpdateAimingState()
	{
		if (!this.IsCharged)
		{
			this.ChangeState(SeinWallChargeJump.State.Normal);
		}
		if (this.Arrow)
		{
			this.UpdateAimElevation();
			bool hasWallLeft = this.PlatformMovement.HasWallLeft;
			this.m_angularElevation = Mathf.Clamp(this.m_angularElevation + this.m_angularElevationSpeed * Time.deltaTime, -45f, 45f);
			this.Arrow.transform.eulerAngles = new Vector3(0f, 0f, (!hasWallLeft) ? (180f - this.m_angularElevation) : this.m_angularElevation);
		}
	}

	// Token: 0x17000320 RID: 800
	// (get) Token: 0x06001245 RID: 4677 RVA: 0x00069B00 File Offset: 0x00067D00
	public bool CanChargeJump
	{
		get
		{
			return this.m_sein.Abilities.GrabWall.IsGrabbing && this.m_sein.Abilities.ChargeJumpCharging.IsCharged && this.m_currentState == SeinWallChargeJump.State.Aiming;
		}
	}

	// Token: 0x06001246 RID: 4678 RVA: 0x000100DE File Offset: 0x0000E2DE
	public void OnRestoreCheckpoint()
	{
		this.m_spriteMirrorLock = false;
	}

	// Token: 0x06001247 RID: 4679 RVA: 0x000100E7 File Offset: 0x0000E2E7
	public override void Awake()
	{
		base.Awake();
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x17000321 RID: 801
	// (get) Token: 0x06001248 RID: 4680 RVA: 0x00010105 File Offset: 0x0000E305
	public CharacterSpriteMirror CharacterSpriteMirror
	{
		get
		{
			return this.m_sein.PlatformBehaviour.Visuals.SpriteMirror;
		}
	}

	// Token: 0x17000322 RID: 802
	// (get) Token: 0x06001249 RID: 4681 RVA: 0x0001011C File Offset: 0x0000E31C
	// (set) Token: 0x0600124A RID: 4682 RVA: 0x00069B50 File Offset: 0x00067D50
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
				if (value)
				{
					this.CharacterSpriteMirror.Lock++;
				}
				else
				{
					this.CharacterSpriteMirror.Lock--;
				}
			}
		}
	}

	// Token: 0x0600124B RID: 4683 RVA: 0x00069BA4 File Offset: 0x00067DA4
	public void PerformChargeJump()
	{
		float chargedJumpStrength = this.ChargedJumpStrength +  this.ChargedJumpStrength*.10f*RandomizerBonus.Velocity();
		this.PlatformMovement.LocalSpeedX = chargedJumpStrength * this.Arrow.transform.right.x;
		this.PlatformMovement.LocalSpeedY = chargedJumpStrength * this.Arrow.transform.right.y;
		Vector2 normalized = this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeed.normalized;
		this.m_angleDirection = Mathf.Atan2(normalized.y, Mathf.Abs(normalized.x)) * 57.29578f * (float)((normalized.x >= 0f) ? 1 : -1);
		Sound.Play(this.JumpSound.GetSound(null), this.m_sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.m_sein.Mortality.DamageReciever.MakeInvincibleToEnemies(this.AntiGravityDuration+this.AntiGravityDuration*.10f*RandomizerBonus.Velocity());
		this.ChangeState(SeinWallChargeJump.State.Jumping);
		this.m_sein.FaceLeft = (this.PlatformMovement.LocalSpeedX < 0f);
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.m_sein.PlatformBehaviour.Visuals.Animation.Play(this.JumpAnimation, 10, new Func<bool>(this.ShouldChargeJumpAnimationKeepPlaying));
		characterAnimationState.OnStartPlaying = new Action(this.OnAnimationStart);
		characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
		this.m_sein.PlatformBehaviour.Visuals.SpriteRotater.BeginTiltUpDownInAir(1.5f);
		if (this.m_sein.Abilities.Glide)
		{
			this.m_sein.Abilities.Glide.NeedsRightTriggerReleased = true;
		}
		JumpFlipPlatform.OnSeinChargeJumpEvent();
		this.m_sein.Abilities.ChargeJumpCharging.EndCharge();
	}

	// Token: 0x0600124C RID: 4684 RVA: 0x00010124 File Offset: 0x0000E324
	public bool ShouldChargeJumpAnimationKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnWall && !this.PlatformMovement.IsOnCeiling;
	}

	// Token: 0x0600124D RID: 4685 RVA: 0x00010157 File Offset: 0x0000E357
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.Abilities.WallChargeJump = this;
	}

	// Token: 0x0400110E RID: 4366
	public TextureAnimationWithTransitions ChargeAnimation;

	// Token: 0x0400110F RID: 4367
	public TextureAnimationWithTransitions JumpAnimation;

	// Token: 0x04001110 RID: 4368
	public SoundProvider JumpSound;

	// Token: 0x04001111 RID: 4369
	public float AntiGravityDuration = 0.2f;

	// Token: 0x04001112 RID: 4370
	public float HorizontalDrag = 30f;

	// Token: 0x04001113 RID: 4371
	public BaseAnimator Arrow;

	// Token: 0x04001114 RID: 4372
	public int Damage = 50;

	// Token: 0x04001115 RID: 4373
	public float ChargedJumpStrength;

	// Token: 0x04001116 RID: 4374
	public SeinWallChargeJump.State m_currentState;

	// Token: 0x04001117 RID: 4375
	public float m_angularElevation;

	// Token: 0x04001118 RID: 4376
	public float m_angularElevationSpeed;

	// Token: 0x04001119 RID: 4377
	public float m_stateCurrentTime;

	// Token: 0x0400111A RID: 4378
	public float m_angleDirection;

	// Token: 0x0400111B RID: 4379
	public bool m_spriteMirrorLock;

	// Token: 0x0400111C RID: 4380
	public SeinCharacter m_sein;

	// Token: 0x0400111D RID: 4381
	public HashSet<IAttackable> m_attackablesIgnore = new HashSet<IAttackable>();

	// Token: 0x0400111E RID: 4382
	public GameObject ExplosionEffect;

	// Token: 0x0200033B RID: 827
	public enum State
	{
		// Token: 0x04001120 RID: 4384
		Normal,
		// Token: 0x04001121 RID: 4385
		Aiming,
		// Token: 0x04001122 RID: 4386
		Jumping
	}
}
