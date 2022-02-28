using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200032B RID: 811
public class SeinGrabWall : CharacterState, ISeinReceiver
{
	// Token: 0x170002D4 RID: 724
	// (get) Token: 0x0600112A RID: 4394 RVA: 0x0000F842 File Offset: 0x0000DA42
	public CharacterGravity CharacterGravity
	{
		get
		{
			return this.Sein.PlatformBehaviour.Gravity;
		}
	}

	// Token: 0x170002D5 RID: 725
	// (get) Token: 0x0600112B RID: 4395 RVA: 0x0000F854 File Offset: 0x0000DA54
	public CharacterLeftRightMovement CharacterLeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	// Token: 0x170002D6 RID: 726
	// (get) Token: 0x0600112C RID: 4396 RVA: 0x0000F866 File Offset: 0x0000DA66
	public PlatformMovementListOfColliders ListOfCollidedObjects
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovementListOfColliders;
		}
	}

	// Token: 0x170002D7 RID: 727
	// (get) Token: 0x0600112D RID: 4397 RVA: 0x0000F878 File Offset: 0x0000DA78
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x0600112E RID: 4398 RVA: 0x00067234 File Offset: 0x00065434
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

	// Token: 0x0600112F RID: 4399 RVA: 0x0000F88A File Offset: 0x0000DA8A
	public void Start()
	{
		this.CharacterGravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyGravityPlatformMovementSettings;
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += this.ModifyHorizontalPlatformMovementSettings;
	}

	// Token: 0x06001130 RID: 4400 RVA: 0x0000F8BA File Offset: 0x0000DABA
	public new void OnDestroy()
	{
		base.OnDestroy();
		this.CharacterGravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyGravityPlatformMovementSettings;
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= this.ModifyHorizontalPlatformMovementSettings;
	}

	// Token: 0x06001131 RID: 4401 RVA: 0x0000F8F0 File Offset: 0x0000DAF0
	public void ModifyGravityPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (this.IsGrabbing)
		{
			settings.GravityStrength = 0f;
		}
	}

	// Token: 0x170002D8 RID: 728
	// (get) Token: 0x06001132 RID: 4402 RVA: 0x0000F908 File Offset: 0x0000DB08
	public bool IsNotMoving
	{
		get
		{
			return this.PlatformMovement.LocalSpeedY == 0f;
		}
	}

	// Token: 0x06001133 RID: 4403 RVA: 0x0000F91C File Offset: 0x0000DB1C
	public void ModifyHorizontalPlatformMovementSettings(HorizontalPlatformMovementSettings settings)
	{
		if (this.IsGrabbing && !this.PlatformMovement.IsOnGround && !this.PlatformMovement.IsOnCeiling)
		{
			settings.LockInput = true;
		}
	}

	// Token: 0x06001134 RID: 4404 RVA: 0x000672B4 File Offset: 0x000654B4
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

	// Token: 0x06001135 RID: 4405 RVA: 0x0006730C File Offset: 0x0006550C
	public void OnGrabWall()
	{
		if (GameController.Instance.GameTime > this.m_lastWallGrabEnterSoundTime + this.m_minimumSoundDelay)
		{
			Sound.Play(this.WallGrabEnterSound.GetSoundForMaterial(this.Sein.PlatformBehaviour.WallSurfaceMaterialType, null), this.PlatformMovement.Position, null);
			this.m_lastWallGrabEnterSoundTime = GameController.Instance.GameTime;
		}
	}

	// Token: 0x06001136 RID: 4406 RVA: 0x00067374 File Offset: 0x00065574
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

	// Token: 0x170002D9 RID: 729
	// (get) Token: 0x06001137 RID: 4407 RVA: 0x0000F950 File Offset: 0x0000DB50
	// (set) Token: 0x06001138 RID: 4408 RVA: 0x0000F958 File Offset: 0x0000DB58
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

	// Token: 0x06001139 RID: 4409 RVA: 0x00067430 File Offset: 0x00065630
	public void UpdateGrabbing()
	{
		if (this.IsGrabbing && this.Sein.Controller.CanMove && this.Sein.Input.Up.Pressed && !this.PlatformMovement.HeadAgainstWall)
		{
			if (this.Sein.Abilities.Glide)
			{
				this.Sein.Abilities.Glide.NeedsRightTriggerReleased = true;
			}
			this.Sein.Abilities.EdgeClamber.PerformEdgeClamber();
		}
		if (!this.CanGrab)
		{
			this.IsGrabbing = false;
			return;
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

	// Token: 0x0600113A RID: 4410 RVA: 0x00067728 File Offset: 0x00065928
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
				this.IsGrabbing = true;
			}
		}
		else
		{
			this.m_requiresRelease = false;
		}
	}

	// Token: 0x170002DA RID: 730
	// (get) Token: 0x0600113B RID: 4411 RVA: 0x0000F989 File Offset: 0x0000DB89
	public bool WantToGrab
	{
		get
		{
			return Core.Input.Glide.IsPressed;
		}
	}

	// Token: 0x170002DB RID: 731
	// (get) Token: 0x0600113C RID: 4412 RVA: 0x00067798 File Offset: 0x00065998
	public bool CanGrab
	{
		get
		{
			return this.Sein.Abilities.WallSlide.IsOnWall && (this.Sein.PlatformBehaviour.PlatformMovement.HasWallLeft || !this.Sein.Controller.FaceLeft) && (this.Sein.PlatformBehaviour.PlatformMovement.HasWallRight || this.Sein.Controller.FaceLeft) && !this.m_requiresRelease && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && this.PlatformMovement.HeadAgainstWall;
		}
	}

	// Token: 0x170002DC RID: 732
	// (get) Token: 0x0600113D RID: 4413 RVA: 0x0000F995 File Offset: 0x0000DB95
	public bool ShouldGrabWallUpAnimationPlay
	{
		get
		{
			return this.ShouldGrabWallUpAnimationKeepPlaying();
		}
	}

	// Token: 0x170002DD RID: 733
	// (get) Token: 0x0600113E RID: 4414 RVA: 0x0000F99D File Offset: 0x0000DB9D
	public bool ShouldGrabWallDownAnimationPlay
	{
		get
		{
			return this.ShouldGrabWallDownAnimationKeepPlaying();
		}
	}

	// Token: 0x170002DE RID: 734
	// (get) Token: 0x0600113F RID: 4415 RVA: 0x0000F9A5 File Offset: 0x0000DBA5
	public bool ShouldGrabWallAwayAnimationPlay
	{
		get
		{
			return this.ShouldGrabWallAwayAnimationKeepPlaying();
		}
	}

	// Token: 0x170002DF RID: 735
	// (get) Token: 0x06001140 RID: 4416 RVA: 0x0000F9AD File Offset: 0x0000DBAD
	public bool ShouldGrabWallIdleAnimationPlay
	{
		get
		{
			return this.ShouldGrabWallIdleAnimationKeepPlaying();
		}
	}

	// Token: 0x06001141 RID: 4417 RVA: 0x0000F9B5 File Offset: 0x0000DBB5
	public bool ShouldGrabWallUpAnimationKeepPlaying()
	{
		return this.IsGrabbing && this.PlatformMovement.LocalSpeedY > 0f;
	}

	// Token: 0x06001142 RID: 4418 RVA: 0x0000F9D7 File Offset: 0x0000DBD7
	public bool ShouldGrabWallDownAnimationKeepPlaying()
	{
		return this.IsGrabbing && this.PlatformMovement.LocalSpeedY < 0f;
	}

	// Token: 0x06001143 RID: 4419 RVA: 0x0000F9F9 File Offset: 0x0000DBF9
	public bool ShouldGrabWallAwayAnimationKeepPlaying()
	{
		return this.IsGrabbing && this.PlatformMovement.LocalSpeedY == 0f && this.IsGrabbingAway;
	}

	// Token: 0x06001144 RID: 4420 RVA: 0x0000FA24 File Offset: 0x0000DC24
	public bool ShouldGrabWallIdleAnimationKeepPlaying()
	{
		return this.IsGrabbing && this.PlatformMovement.LocalSpeedY == 0f && !this.IsGrabbingAway;
	}

	// Token: 0x170002E0 RID: 736
	// (get) Token: 0x06001145 RID: 4421 RVA: 0x00067848 File Offset: 0x00065A48
	public bool IsGrabbingAway
	{
		get
		{
			return (this.Sein.Input.NormalizedHorizontal == -1 && this.PlatformMovement.HasWallRight) || (this.Sein.Input.NormalizedHorizontal == 1 && this.PlatformMovement.HasWallLeft);
		}
	}

	// Token: 0x06001146 RID: 4422 RVA: 0x000678A4 File Offset: 0x00065AA4
	public void HandleWallClimbUpSteps()
	{
		if (this.PlatformMovement.LocalSpeedY > 0f && this.m_nextWallClimbUpTime < this.m_currentTime)
		{
			Sound.Play(this.WallGrabStepUpSound.GetSoundForMaterial(this.Sein.PlatformBehaviour.WallSurfaceMaterialType, null), this.PlatformMovement.Position, null);
			this.m_nextWallClimbUpTime = this.m_currentTime + 1f / this.WallClimbUpStepsPerSecond;
		}
	}

	// Token: 0x06001147 RID: 4423 RVA: 0x00067920 File Offset: 0x00065B20
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

	// Token: 0x06001148 RID: 4424 RVA: 0x0000FA52 File Offset: 0x0000DC52
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.GrabWall = this;
	}

	// Token: 0x04001060 RID: 4192
	public SeinCharacter Sein;

	// Token: 0x04001061 RID: 4193
	public float WallClimbUpStepsPerSecond;

	// Token: 0x04001062 RID: 4194
	public float WallClimbDownStepsPerSecond;

	// Token: 0x04001063 RID: 4195
	public SurfaceToSoundProviderMap WallGrabEnterSound;

	// Token: 0x04001064 RID: 4196
	public SurfaceToSoundProviderMap WallGrabExitSound;

	// Token: 0x04001065 RID: 4197
	public SurfaceToSoundProviderMap WallGrabStepUpSound;

	// Token: 0x04001066 RID: 4198
	public SurfaceToSoundProviderMap WallGrabStepDownSound;

	// Token: 0x04001067 RID: 4199
	private float m_minimumSoundDelay = 0.4f;

	// Token: 0x04001068 RID: 4200
	private float m_lastWallGrabEnterSoundTime;

	// Token: 0x04001069 RID: 4201
	private float m_lastWallGrabExitSoundTime;

	// Token: 0x0400106A RID: 4202
	private float m_lastWallGrabStepDownSoundTime = -10f;

	// Token: 0x0400106B RID: 4203
	public bool LockVerticalMovement;

	// Token: 0x0400106C RID: 4204
	public SeinGrabWall.GrabWallAnimationSet GrabWallAnimation;

	// Token: 0x0400106D RID: 4205
	public TextureAnimationWithTransitions EdgeClimbAnimation;

	// Token: 0x0400106E RID: 4206
	public float ClimbSpeedUp;

	// Token: 0x0400106F RID: 4207
	public float ClimbSpeedDown;

	// Token: 0x04001070 RID: 4208
	public float Acceleration = 60f;

	// Token: 0x04001071 RID: 4209
	private float m_currentTime;

	// Token: 0x04001072 RID: 4210
	private bool m_isGrabbing;

	// Token: 0x04001073 RID: 4211
	private float m_nextWallClimbUpTime;

	// Token: 0x04001074 RID: 4212
	private bool m_requiresRelease;

	// Token: 0x04001075 RID: 4213
	private SoundPlayer m_climbDownSoundPlayer;

	// Token: 0x0200032C RID: 812
	[Serializable]
	public class GrabWallAnimationSet
	{
		// Token: 0x04001076 RID: 4214
		public TextureAnimationWithTransitions Idle;

		// Token: 0x04001077 RID: 4215
		public TextureAnimationWithTransitions ClimbUp;

		// Token: 0x04001078 RID: 4216
		public TextureAnimationWithTransitions ClimbDown;

		// Token: 0x04001079 RID: 4217
		public TextureAnimationWithTransitions[] Away;

		// Token: 0x0400107A RID: 4218
		public TextureAnimationWithTransitions GrabAway;
	}
}
