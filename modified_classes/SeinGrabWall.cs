using System;
using Core;
using Game;
using UnityEngine;

public class SeinGrabWall : CharacterState, ISeinReceiver
{
	public CharacterGravity CharacterGravity
	{
		get
		{
			return this.Sein.PlatformBehaviour.Gravity;
		}
	}

	public CharacterLeftRightMovement CharacterLeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	public PlatformMovementListOfColliders ListOfCollidedObjects
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovementListOfColliders;
		}
	}

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public TextureAnimationWithTransitions PickAwayAnimation()
	{
		if (this.Sein.PlayerAbilities.ChargeJump.HasAbility)
		{
			TextureAnimationWithTransitions[] away = this.GrabWallAnimation.Away;
			float angularElevation = this.Sein.Abilities.WallChargeJump.AngularElevation;
			int num = (int)Mathf.Clamp(Mathf.InverseLerp(-45f, 45f, angularElevation) * (float)away.Length, 0f, (float)(away.Length - 1));
			return away[num];
		}
		return this.GrabWallAnimation.GrabAway;
	}

	public void Start()
	{
		this.CharacterGravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyGravityPlatformMovementSettings;
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += this.ModifyHorizontalPlatformMovementSettings;
	}

	public new void OnDestroy()
	{
		base.OnDestroy();
		this.CharacterGravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyGravityPlatformMovementSettings;
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= this.ModifyHorizontalPlatformMovementSettings;
	}

	public void ModifyGravityPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (this.IsGrabbing)
		{
			settings.GravityStrength = 0f;
		}
	}

	public bool IsNotMoving
	{
		get
		{
			return this.PlatformMovement.LocalSpeedY == 0f;
		}
	}

	public void ModifyHorizontalPlatformMovementSettings(HorizontalPlatformMovementSettings settings)
	{
		if (this.IsGrabbing && !this.PlatformMovement.IsOnGround && !this.PlatformMovement.IsOnCeiling)
		{
			settings.LockInput = true;
		}
	}

	public override void OnExit()
	{
		this.IsGrabbing = false;
		if (this.m_climbDownSoundPlayer)
		{
			this.m_climbDownSoundPlayer.FadeOut(0.3f, true);
			UberPoolManager.Instance.RemoveOnDestroyed(this.m_climbDownSoundPlayer.gameObject);
			this.m_climbDownSoundPlayer = null;
		}
		base.OnExit();
	}

	public void OnGrabWall()
	{
		if (GameController.Instance.GameTime > this.m_lastWallGrabEnterSoundTime + this.m_minimumSoundDelay)
		{
			Sound.Play(this.WallGrabEnterSound.GetSoundForMaterial(this.Sein.PlatformBehaviour.WallSurfaceMaterialType, null), this.PlatformMovement.Position, null);
			this.m_lastWallGrabEnterSoundTime = GameController.Instance.GameTime;
		}
	}

	public void OnReleaseWall()
	{
		this.Sein.Abilities.WallSlide.ResetMovingOffWallLockTimer();
		if (GameController.Instance.GameTime > this.m_lastWallGrabExitSoundTime + this.m_minimumSoundDelay)
		{
			Sound.Play(this.WallGrabExitSound.GetSoundForMaterial(this.Sein.PlatformBehaviour.WallSurfaceMaterialType, null), this.PlatformMovement.Position, null);
			this.m_lastWallGrabExitSoundTime = GameController.Instance.GameTime;
		}
		if (this.m_climbDownSoundPlayer)
		{
			this.m_climbDownSoundPlayer.FadeOut(0f, true);
			UberPoolManager.Instance.RemoveOnDestroyed(this.m_climbDownSoundPlayer.gameObject);
			this.m_climbDownSoundPlayer = null;
		}
	}

	public bool IsGrabbing
	{
		get
		{
			return this.m_isGrabbing;
		}
		set
		{
			if (this.m_isGrabbing != value)
			{
				this.m_isGrabbing = value;
				if (this.m_isGrabbing)
				{
					this.OnGrabWall();
				}
				else
				{
					this.OnReleaseWall();
				}
			}
		}
	}

	public void UpdateGrabbing()
	{
		if (this.IsGrabbing && this.Sein.Controller.CanMove && this.Sein.Input.Up.Pressed && !this.PlatformMovement.HeadAgainstWall)
		{
			if (this.Sein.Abilities.Glide)
			{
				this.Sein.Abilities.Glide.NeedsRightTriggerReleased = true;
			}
			float climbClamberFactor = RandomizerSettings.SlowClimbVault ? 0.225f : 0.65f;
			this.Sein.Abilities.EdgeClamber.PerformEdgeClamber(climbClamberFactor);
		}
		if (!this.CanGrab)
		{
			if (Randomizer.DoesGrabForgivenessExpire(Time.deltaTime))
			{
				this.IsGrabbing = false;
				return;
			}
		}
		if (!this.WantToGrab)
		{
			this.IsGrabbing = false;
			return;
		}
		if (ForceGrabReleaseZone.InsideZone(this.Sein.Position))
		{
			this.m_requiresRelease = true;
		}
		Vector2 localSpeed = this.PlatformMovement.LocalSpeed;
		if (Characters.Sein.Controller.CanMove)
		{
			if (this.LockVerticalMovement || this.Sein.Controller.IsChargingJump)
			{
				localSpeed.y = 0f;
			}
			else if (this.Sein.Input.Up.Pressed)
			{
				localSpeed.y = Mathf.Clamp(localSpeed.y + this.Acceleration * Time.deltaTime, 0f, this.ClimbSpeedUp * RandomizerBonus.Veloscale);
			}
			else if (this.Sein.Input.Down.Pressed)
			{
				localSpeed.y = Mathf.Clamp(localSpeed.y - this.Acceleration * Time.deltaTime, -this.ClimbSpeedDown * RandomizerBonus.Veloscale, 0f);
			}
			else
			{
				localSpeed.y = 0f;
			}
		}
		this.HandleWallClimbUpSteps();
		this.HandleWallClimbDownSteps();
		this.PlatformMovement.LocalSpeed = localSpeed;
		if (this.ShouldGrabWallUpAnimationPlay)
		{
			this.Sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.GrabWallAnimation.ClimbUp, 25, new Func<bool>(this.ShouldGrabWallUpAnimationKeepPlaying), false);
		}
		if (this.ShouldGrabWallDownAnimationPlay)
		{
			this.Sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.GrabWallAnimation.ClimbDown, 25, new Func<bool>(this.ShouldGrabWallDownAnimationKeepPlaying), false);
		}
		if (this.ShouldGrabWallAwayAnimationPlay)
		{
			this.Sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.PickAwayAnimation(), 25, new Func<bool>(this.ShouldGrabWallAwayAnimationKeepPlaying), true);
		}
		if (this.ShouldGrabWallIdleAnimationPlay)
		{
			this.Sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.GrabWallAnimation.Idle, 25, new Func<bool>(this.ShouldGrabWallIdleAnimationKeepPlaying), false);
		}
		this.m_currentTime += Time.deltaTime;
	}

	public override void UpdateCharacterState()
	{
		if (this.IsGrabbing)
		{
			this.UpdateGrabbing();
		}
		else if (this.WantToGrab)
		{
			if (!this.Sein.Abilities.WallSlide.IsWallSliding)
			{
				this.m_requiresRelease = false;
			}
			if (this.CanGrab)
			{
				Randomizer.ApplyGrabForgiveness();
				this.IsGrabbing = true;
			}
		}
		else
		{
			this.m_requiresRelease = false;
		}
	}

	public bool WantToGrab
	{
		get
		{
			return RandomizerSettings.InvertClimb ^ Core.Input.Glide.Pressed;
		}
	}

	public bool CanGrab
	{
		get
		{
			return this.Sein.Abilities.WallSlide.IsOnWall && (this.Sein.PlatformBehaviour.PlatformMovement.HasWallLeft || !this.Sein.Controller.FaceLeft) && (this.Sein.PlatformBehaviour.PlatformMovement.HasWallRight || this.Sein.Controller.FaceLeft) && !this.m_requiresRelease && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && this.PlatformMovement.HeadAgainstWall;
		}
	}

	public bool ShouldGrabWallUpAnimationPlay
	{
		get
		{
			return this.ShouldGrabWallUpAnimationKeepPlaying();
		}
	}

	public bool ShouldGrabWallDownAnimationPlay
	{
		get
		{
			return this.ShouldGrabWallDownAnimationKeepPlaying();
		}
	}

	public bool ShouldGrabWallAwayAnimationPlay
	{
		get
		{
			return this.ShouldGrabWallAwayAnimationKeepPlaying();
		}
	}

	public bool ShouldGrabWallIdleAnimationPlay
	{
		get
		{
			return this.ShouldGrabWallIdleAnimationKeepPlaying();
		}
	}

	public bool ShouldGrabWallUpAnimationKeepPlaying()
	{
		return this.IsGrabbing && this.PlatformMovement.LocalSpeedY > 0f;
	}

	public bool ShouldGrabWallDownAnimationKeepPlaying()
	{
		return this.IsGrabbing && this.PlatformMovement.LocalSpeedY < 0f;
	}

	public bool ShouldGrabWallAwayAnimationKeepPlaying()
	{
		return this.IsGrabbing && this.PlatformMovement.LocalSpeedY == 0f && this.IsGrabbingAway;
	}

	public bool ShouldGrabWallIdleAnimationKeepPlaying()
	{
		return this.IsGrabbing && this.PlatformMovement.LocalSpeedY == 0f && !this.IsGrabbingAway;
	}

	public bool IsGrabbingAway
	{
		get
		{
			return (this.Sein.Input.NormalizedHorizontal == -1 && this.PlatformMovement.HasWallRight) || (this.Sein.Input.NormalizedHorizontal == 1 && this.PlatformMovement.HasWallLeft);
		}
	}

	public void HandleWallClimbUpSteps()
	{
		if (this.PlatformMovement.LocalSpeedY > 0f && this.m_nextWallClimbUpTime < this.m_currentTime)
		{
			Sound.Play(this.WallGrabStepUpSound.GetSoundForMaterial(this.Sein.PlatformBehaviour.WallSurfaceMaterialType, null), this.PlatformMovement.Position, null);
			this.m_nextWallClimbUpTime = this.m_currentTime + 1f / this.WallClimbUpStepsPerSecond;
		}
	}

	public void HandleWallClimbDownSteps()
	{
		if (this.PlatformMovement.LocalSpeedY < 0f)
		{
			if (InstantiateUtility.IsDestroyed(this.m_climbDownSoundPlayer) && GameController.Instance.GameTime > this.m_lastWallGrabStepDownSoundTime + this.m_minimumSoundDelay)
			{
				this.m_climbDownSoundPlayer = Sound.PlayLooping(this.WallGrabStepDownSound.GetSoundForMaterial(this.Sein.PlatformBehaviour.WallSurfaceMaterialType, null), this.PlatformMovement.Position, delegate()
				{
					this.m_climbDownSoundPlayer = null;
				});
				this.m_lastWallGrabStepDownSoundTime = GameController.Instance.GameTime;
			}
		}
		else if (!InstantiateUtility.IsDestroyed(this.m_climbDownSoundPlayer))
		{
			this.m_climbDownSoundPlayer.FadeOut(0.3f, true);
			UberPoolManager.Instance.RemoveOnDestroyed(this.m_climbDownSoundPlayer.gameObject);
			this.m_climbDownSoundPlayer = null;
		}
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.GrabWall = this;
	}

	public SeinCharacter Sein;

	public float WallClimbUpStepsPerSecond;

	public float WallClimbDownStepsPerSecond;

	public SurfaceToSoundProviderMap WallGrabEnterSound;

	public SurfaceToSoundProviderMap WallGrabExitSound;

	public SurfaceToSoundProviderMap WallGrabStepUpSound;

	public SurfaceToSoundProviderMap WallGrabStepDownSound;

	private float m_minimumSoundDelay = 0.4f;

	private float m_lastWallGrabEnterSoundTime;

	private float m_lastWallGrabExitSoundTime;

	private float m_lastWallGrabStepDownSoundTime = -10f;

	public bool LockVerticalMovement;

	public SeinGrabWall.GrabWallAnimationSet GrabWallAnimation;

	public TextureAnimationWithTransitions EdgeClimbAnimation;

	public float ClimbSpeedUp;

	public float ClimbSpeedDown;

	public float Acceleration = 60f;

	private float m_currentTime;

	private bool m_isGrabbing;

	private float m_nextWallClimbUpTime;

	private bool m_requiresRelease;

	private SoundPlayer m_climbDownSoundPlayer;

	[Serializable]
	public class GrabWallAnimationSet
	{
		public TextureAnimationWithTransitions Idle;

		public TextureAnimationWithTransitions ClimbUp;

		public TextureAnimationWithTransitions ClimbDown;

		public TextureAnimationWithTransitions[] Away;

		public TextureAnimationWithTransitions GrabAway;
	}
}
