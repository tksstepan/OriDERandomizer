using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class SeinWallChargeJump : CharacterState, ISeinReceiver
{
	public PlayerAbilities PlayerAbilities
	{
		get
		{
			return this.m_sein.PlayerAbilities;
		}
	}

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.m_sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public void OnDoubleJump()
	{
		this.ChangeState(SeinWallChargeJump.State.Normal);
	}

	public override void UpdateCharacterState()
	{
		if (this.m_sein.IsSuspended)
		{
			return;
		}
		this.UpdateState();
	}

	public float AngularElevation
	{
		get
		{
			return this.m_angularElevation;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		this.ChangeState(SeinWallChargeJump.State.Normal);
	}

	public void Start()
	{
		this.m_sein.PlatformBehaviour.Gravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyGravityPlatformMovementSettings;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		this.m_sein.PlatformBehaviour.Gravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyGravityPlatformMovementSettings;
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

	public void OnAnimationEnd()
	{
		this.SpriteMirrorLock = false;
	}

	public void OnAnimationStart()
	{
		this.SpriteMirrorLock = true;
	}

	public void ModifyGravityPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (this.m_currentState == SeinWallChargeJump.State.Jumping)
		{
			settings.GravityStrength = 0f;
		}
	}

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

	public bool IsCharged
	{
		get
		{
			return this.m_sein.Controller.IsGrabbingWall && this.m_sein.Abilities.GrabWall.IsGrabbingAway && Characters.Sein.Controller.CanMove && this.m_sein.Abilities.ChargeJumpCharging.IsCharged;
		}
	}

	public bool IsCharging
	{
		get
		{
			return this.m_sein.Controller.IsGrabbingWall && this.m_sein.Abilities.GrabWall.IsGrabbingAway && Characters.Sein.Controller.CanMove && this.m_sein.Abilities.ChargeJumpCharging.IsCharging;
		}
	}

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

	public void UpdateJumpingState()
	{
		float adjustedDrag = this.HorizontalDrag-this.HorizontalDrag* 0.08f * (float)(RandomizerBonus.Velocity() + RandomizerBonus.Jumpgrades());
		this.PlatformMovement.LocalSpeedX = this.PlatformMovement.LocalSpeedX * (1f - adjustedDrag);
		this.PlatformMovement.LocalSpeedY = this.PlatformMovement.LocalSpeedY * (1f - adjustedDrag);
		if (this.m_stateCurrentTime > (this.AntiGravityDuration+this.AntiGravityDuration* 0.08f * (float)(RandomizerBonus.Velocity() + RandomizerBonus.Jumpgrades())))
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

	public void UpdateAimElevation()
	{
		float normalizedFacing = this.PlatformMovement.HasWallLeft ? 1 : -1;
		Vector2 analogAxisLeft = Core.Input.AnalogAxisLeft;

		if (analogAxisLeft.magnitude > 0.2f)
		{
			this.m_angularElevationSpeed = 0f;
			this.m_angularElevation = Mathf.Atan2(analogAxisLeft.y, analogAxisLeft.x * normalizedFacing) * 57.29578f;
			return;
		}
		else if (Core.Input.Up.Pressed && !Core.Input.Down.Pressed)
		{
			this.m_angularElevationSpeed = Mathf.Clamp(this.m_angularElevationSpeed + Time.deltaTime * 500f, 0f, 200f);
			return;
		}
		else if (Core.Input.Down.Pressed)
		{
			this.m_angularElevationSpeed = Mathf.Clamp(this.m_angularElevationSpeed - Time.deltaTime * 500f, -200f, 0f);
			return;
		}

		this.m_angularElevationSpeed = 0f;

		if (RandomizerSettings.Controls.WallChargeMouseAim)
		{
			Vector2 arrowScreenPos = UI.Cameras.Current.Camera.WorldToScreenPoint(this.Arrow.transform.position);
			Vector2 arrowWorldPos = UI.Cameras.System.GUICamera.Camera.ScreenToWorldPoint(arrowScreenPos);
			Vector2 cursorAxis = Core.Input.CursorPositionUI - arrowWorldPos;

			if (Core.Input.CursorMoved && cursorAxis.magnitude > 1f && MoonMath.Float.Normalize(cursorAxis.x) == normalizedFacing)
			{
				float axisElevation = Mathf.Atan2(cursorAxis.y, cursorAxis.x * normalizedFacing) * 57.29578f;
				if (Mathf.Abs(axisElevation) <= 60f)
				{
					this.m_angularElevation = axisElevation;
				}
			}
		}
	}

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

	public bool CanChargeJump
	{
		get
		{
			return this.m_sein.Abilities.GrabWall.IsGrabbing && this.m_sein.Abilities.ChargeJumpCharging.IsCharged && this.m_currentState == SeinWallChargeJump.State.Aiming;
		}
	}

	public void OnRestoreCheckpoint()
	{
		this.m_spriteMirrorLock = false;
	}

	public override void Awake()
	{
		base.Awake();
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	public CharacterSpriteMirror CharacterSpriteMirror
	{
		get
		{
			return this.m_sein.PlatformBehaviour.Visuals.SpriteMirror;
		}
	}

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

	public void PerformChargeJump()
	{
		float chargedJumpStrength = this.ChargedJumpStrength +  this.ChargedJumpStrength* 0.08f * (float)(RandomizerBonus.Velocity() + RandomizerBonus.Jumpgrades());
		this.PlatformMovement.LocalSpeedX = chargedJumpStrength * this.Arrow.transform.right.x;
		this.PlatformMovement.LocalSpeedY = chargedJumpStrength * this.Arrow.transform.right.y;
		Vector2 normalized = this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeed.normalized;
		this.m_angleDirection = Mathf.Atan2(normalized.y, Mathf.Abs(normalized.x)) * 57.29578f * (float)((normalized.x >= 0f) ? 1 : -1);
		Sound.Play(this.JumpSound.GetSound(null), this.m_sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.m_sein.Mortality.DamageReciever.MakeInvincibleToEnemies(this.AntiGravityDuration);
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

	public bool ShouldChargeJumpAnimationKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnWall && !this.PlatformMovement.IsOnCeiling;
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.Abilities.WallChargeJump = this;
	}

	public TextureAnimationWithTransitions ChargeAnimation;

	public TextureAnimationWithTransitions JumpAnimation;

	public SoundProvider JumpSound;

	public float AntiGravityDuration = 0.2f;

	public float HorizontalDrag = 30f;

	public BaseAnimator Arrow;

	public int Damage = 50;

	public float ChargedJumpStrength;

	public SeinWallChargeJump.State m_currentState;

	public float m_angularElevation;

	public float m_angularElevationSpeed;

	public float m_stateCurrentTime;

	public float m_angleDirection;

	public bool m_spriteMirrorLock;

	public SeinCharacter m_sein;

	public HashSet<IAttackable> m_attackablesIgnore = new HashSet<IAttackable>();

	public GameObject ExplosionEffect;

	public enum State
	{
		Normal,
		Aiming,
		Jumping
	}
}
