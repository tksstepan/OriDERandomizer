using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000337 RID: 823
public class SeinJump : CharacterState, ISeinReceiver
{
	// Token: 0x14000017 RID: 23
	// (add) Token: 0x060011B8 RID: 4536 RVA: 0x00010117 File Offset: 0x0000E317
	// (remove) Token: 0x060011B9 RID: 4537 RVA: 0x00010130 File Offset: 0x0000E330
	public event Action<float> OnJumpEvent = delegate(float A_0)
	{
	};

	// Token: 0x170002FA RID: 762
	// (get) Token: 0x060011BA RID: 4538 RVA: 0x0006939C File Offset: 0x0006759C
	public bool CanJump
	{
		get
		{
			return base.enabled && this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedY <= 0.0001f && this.m_timeWeCanJumpRemaining > 0f && !this.Sein.PlatformBehaviour.PlatformMovement.Ceiling.IsOn && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities);
		}
	}

	// Token: 0x170002FB RID: 763
	// (get) Token: 0x060011BB RID: 4539 RVA: 0x00010149 File Offset: 0x0000E349
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x170002FC RID: 764
	// (get) Token: 0x060011BC RID: 4540 RVA: 0x0001015B File Offset: 0x0000E35B
	// (set) Token: 0x060011BD RID: 4541 RVA: 0x00069410 File Offset: 0x00067610
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

	// Token: 0x170002FD RID: 765
	// (get) Token: 0x060011BE RID: 4542 RVA: 0x00010163 File Offset: 0x0000E363
	public CharacterSpriteMirror CharacterSpriteMirror
	{
		get
		{
			return this.Sein.PlatformBehaviour.Visuals.SpriteMirror;
		}
	}

	// Token: 0x170002FE RID: 766
	// (get) Token: 0x060011BF RID: 4543 RVA: 0x00069464 File Offset: 0x00067664
	public bool HasSharplyTurnedAround
	{
		get
		{
			return (this.m_timeSinceMovingRight > 0f && this.m_timeSinceMovingRight < 0.2f && this.PlatformMovement.LocalSpeedX < 0f) || (this.m_timeSinceMovingLeft > 0f && this.m_timeSinceMovingLeft < 0.2f && this.PlatformMovement.LocalSpeedX > 0f);
		}
	}

	// Token: 0x060011C0 RID: 4544 RVA: 0x0001017A File Offset: 0x0000E37A
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.Jump = this;
	}

	// Token: 0x060011C1 RID: 4545 RVA: 0x000694E0 File Offset: 0x000676E0
	public override void UpdateCharacterState()
	{
		if (this.m_timeWeCanJumpRemaining > 0f)
		{
			this.m_timeWeCanJumpRemaining -= Time.deltaTime;
		}
		if (this.Sein.PlatformBehaviour.PlatformMovement.Ground.IsOn)
		{
			this.m_timeWeCanJumpRemaining = this.DurationSinceLastOnGroundThatWeCanStillJump;
		}
		else
		{
			this.m_bunnyHopTimeRemaining = 0.2f;
		}
		if (this.m_bunnyHopTimeRemaining > 0f)
		{
			this.m_bunnyHopTimeRemaining -= Time.deltaTime;
			if (this.m_bunnyHopTimeRemaining < 0f)
			{
				this.ResetRunningJumpCount();
			}
		}
		if (!this.PlatformMovement.MovingHorizontally && this.PlatformMovement.IsOnGround)
		{
			this.ResetRunningJumpCount();
		}
		if (this.PlatformMovement.MovingHorizontally && this.PlatformMovement.IsOnGround)
		{
			this.ResetJumpIdleCount();
		}
		this.UpdateTimeSinceFacing();
	}

	// Token: 0x060011C2 RID: 4546 RVA: 0x00010194 File Offset: 0x0000E394
	public void ResetRunningJumpCount()
	{
		this.m_runningJumpNumber = 0;
	}

	// Token: 0x060011C3 RID: 4547 RVA: 0x0001019D File Offset: 0x0000E39D
	public void ResetJumpIdleCount()
	{
		this.m_jumpIdleNumber = 0;
	}

	// Token: 0x060011C4 RID: 4548 RVA: 0x000101A6 File Offset: 0x0000E3A6
	public float CalculateSpeedFromHeight(float height)
	{
		return PhysicsHelper.CalculateSpeedFromHeight(height * RandomizerBonus.Jumpscale, this.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityStrength);
	}

	// Token: 0x060011C5 RID: 4549 RVA: 0x000695D4 File Offset: 0x000677D4
	public void PerformTurnAroundBackFlipJump()
	{
		this.PlatformMovement.LocalSpeedY = this.CalculateSpeedFromHeight(this.BackflipJumpHeight);
		this.Sein.PlatformBehaviour.AirNoDeceleration.NoDeceleration = true;
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(this.PlatformMovement.LocalSpeedY * 0.5f, 1f);
		}
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.BackflipAnimation, 10, new Func<bool>(this.ShouldBackflipAnimationKeepPlaying));
		characterAnimationState.OnStartPlaying = new Action(this.OnAnimationStart);
		characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
	}

	// Token: 0x060011C6 RID: 4550 RVA: 0x000696A8 File Offset: 0x000678A8
	public void PerformJump()
	{
		this.m_currentJumpingMaterial = SurfaceToSoundProviderMap.ColliderMaterialToSurfaceMaterialType(this.Sein.PlatformBehaviour.PlatformMovementListOfColliders.GroundCollider);
		if (this.Sein.Controller.IsCrouching)
		{
			this.PerformCrouchJump();
			Sound.Play(this.JumpSoundProvider.GetSoundForMaterial(this.m_currentJumpingMaterial, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		}
		else if (this.HasSharplyTurnedAround)
		{
			this.PerformTurnAroundBackFlipJump();
			Sound.Play(this.FlipJumpSoundProvider.GetSoundForMaterial(this.m_currentJumpingMaterial, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		}
		else if (this.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput == 0f || this.PlatformMovement.IsOnWall)
		{
			if (this.PlatformMovement.IsOnWall && this.Sein.PlayerAbilities.WallJump.HasAbility && this.Sein.Abilities.WallSlide.IsOnWall)
			{
				this.PerformWallSlideJump();
				Sound.Play(this.JumpSoundProvider.GetSoundForMaterial(this.m_currentJumpingMaterial, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
			}
			else
			{
				this.PerformIdleJump();
			}
		}
		else
		{
			this.PerformRunningJump();
		}
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.JumpParticleEffect, this.Sein.PlatformBehaviour.PlatformMovement.FeetPosition, Quaternion.identity);
		gameObject.transform.eulerAngles = new Vector3(0f, 0f, MoonMath.Angle.AngleFromVector(-this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeed));
		this.Sein.PlatformBehaviour.Force.ApplyGroundForce(Vector3.down * this.JumpImpulse, 1);
		this.OnJumpEvent(this.PlatformMovement.LocalSpeedY);
		JumpFlipPlatform.OnSeinJumpEvent();
		this.m_timeWeCanJumpRemaining = 0f;
	}

	// Token: 0x060011C7 RID: 4551 RVA: 0x000698DC File Offset: 0x00067ADC
	public void PerformRunningJump()
	{
		switch (this.m_runningJumpNumber)
		{
		case 0:
			this.PerformFirstRunningJump();
			break;
		case 1:
			this.PerformSecondRunningJump();
			break;
		case 2:
			this.PerformThirdRunningJump();
			break;
		}
	}

	// Token: 0x060011C8 RID: 4552 RVA: 0x000101C8 File Offset: 0x0000E3C8
	private void CacheDelegates()
	{
		if (this.m_shouldJumpMoving == null)
		{
			this.m_shouldJumpMoving = new Func<bool>(this.ShouldJumpMovingAnimationKeepPlaying);
		}
		if (this.onAnimationEnd == null)
		{
			this.onAnimationEnd = new Action(this.OnAnimationEnd);
		}
	}

	// Token: 0x060011C9 RID: 4553 RVA: 0x00069928 File Offset: 0x00067B28
	public void PerformFirstRunningJump()
	{
		Vector2 localSpeed = this.PlatformMovement.LocalSpeed;
		localSpeed.y = this.CalculateSpeedFromHeight(this.FirstJumpHeight);
		this.PlatformMovement.LocalSpeed = localSpeed;
		this.CacheDelegates();
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.JumpAnimation[0], 10, this.m_shouldJumpMoving);
		characterAnimationState.OnStopPlaying = this.onAnimationEnd;
		characterAnimationState.OnStartPlaying = null;
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(this.PlatformMovement.LocalSpeedY, 1f);
		}
		Sound.Play(this.JumpSoundProvider.GetSoundForMaterial(this.m_currentJumpingMaterial, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.m_runningJumpNumber++;
	}

	// Token: 0x060011CA RID: 4554 RVA: 0x00069A20 File Offset: 0x00067C20
	public void PerformSecondRunningJump()
	{
		Vector2 localSpeed = this.PlatformMovement.LocalSpeed;
		localSpeed.y = this.CalculateSpeedFromHeight((this.m_runningJumpNumber != 0) ? this.SecondJumpHeight : this.FirstJumpHeight);
		this.PlatformMovement.LocalSpeed = localSpeed;
		this.CacheDelegates();
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.JumpAnimation[1], 10, this.m_shouldJumpMoving);
		characterAnimationState.OnStopPlaying = this.onAnimationEnd;
		characterAnimationState.OnStartPlaying = null;
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(this.PlatformMovement.LocalSpeedY, 1f);
		}
		Sound.Play(this.JumpSoundProvider.GetSoundForMaterial(this.m_currentJumpingMaterial, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.m_runningJumpNumber++;
	}

	// Token: 0x060011CB RID: 4555 RVA: 0x00069B30 File Offset: 0x00067D30
	public void PerformThirdRunningJump()
	{
		Vector2 localSpeed = this.PlatformMovement.LocalSpeed;
		localSpeed.y = this.CalculateSpeedFromHeight(this.ThirdJumpHeight);
		this.PlatformMovement.LocalSpeed = localSpeed;
		this.CacheDelegates();
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.JumpAnimation[2], 10, this.m_shouldJumpMoving);
		characterAnimationState.OnStartPlaying = null;
		characterAnimationState.OnStopPlaying = this.onAnimationEnd;
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(this.PlatformMovement.LocalSpeedY * 0.5f, 1f);
		}
		Sound.Play(this.SpinJumpSoundProvider.GetSoundForMaterial(this.m_currentJumpingMaterial, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.m_runningJumpNumber = 0;
	}

	// Token: 0x060011CC RID: 4556 RVA: 0x00069C28 File Offset: 0x00067E28
	private void PerformIdleJump()
	{
		switch (this.m_jumpIdleNumber)
		{
		case 0:
			this.PerformFirstIdleJump();
			break;
		case 1:
			this.PerformSecondIdleJump();
			break;
		case 2:
			this.PerformThirdIldleJump();
			break;
		}
	}

	// Token: 0x060011CD RID: 4557 RVA: 0x00069C74 File Offset: 0x00067E74
	public void PerformFirstIdleJump()
	{
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.JumpIdleAnimation[0], 10, new Func<bool>(this.ShouldJumpIdleAnimationKeepPlaying));
		characterAnimationState.OnStartPlaying = null;
		characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
		this.PlatformMovement.LocalSpeedY = this.CalculateSpeedFromHeight(this.FirstJumpHeight);
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(this.PlatformMovement.LocalSpeedY, 1f);
		}
		Sound.Play(this.JumpSoundProvider.GetSoundForMaterial(this.m_currentJumpingMaterial, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.m_jumpIdleNumber++;
	}

	// Token: 0x060011CE RID: 4558 RVA: 0x00069D60 File Offset: 0x00067F60
	public void PerformSecondIdleJump()
	{
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.JumpIdleAnimation[1], 10, new Func<bool>(this.ShouldJumpIdleAnimationKeepPlaying));
		characterAnimationState.OnStartPlaying = null;
		characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
		this.PlatformMovement.LocalSpeedY = this.CalculateSpeedFromHeight(this.SecondJumpHeight);
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(this.PlatformMovement.LocalSpeedY, 1f);
		}
		Sound.Play(this.JumpSoundProvider.GetSoundForMaterial(this.m_currentJumpingMaterial, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.m_jumpIdleNumber++;
	}

	// Token: 0x060011CF RID: 4559 RVA: 0x00069E4C File Offset: 0x0006804C
	private void PerformThirdIldleJump()
	{
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.JumpIdleAnimation[2], 10, new Func<bool>(this.ShouldJumpIdleAnimationKeepPlaying));
		characterAnimationState.OnStartPlaying = null;
		characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
		this.PlatformMovement.LocalSpeedY = this.CalculateSpeedFromHeight(this.ThirdJumpHeight);
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(this.PlatformMovement.LocalSpeedY, 1f);
		}
		Sound.Play(this.SpinJumpSoundProvider.GetSoundForMaterial(this.m_currentJumpingMaterial, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.m_jumpIdleNumber = 0;
	}

	// Token: 0x060011D0 RID: 4560 RVA: 0x00069F30 File Offset: 0x00068130
	private void PerformWallSlideJump()
	{
		CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.WallSlideJumpAnimation, 24, new Func<bool>(this.ShouldWallSlideJumpAnimationKeepPlaying));
		characterAnimationState.OnStartPlaying = null;
		characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
		this.PlatformMovement.LocalSpeedY = this.CalculateSpeedFromHeight(this.FirstJumpHeight);
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(this.PlatformMovement.LocalSpeedY, 1f);
		}
	}

	// Token: 0x060011D1 RID: 4561 RVA: 0x00069FDC File Offset: 0x000681DC
	private void PerformCrouchJump()
	{
		bool flag = false;
		List<Collider> groundColliders = this.Sein.PlatformBehaviour.PlatformMovementListOfColliders.GroundColliders;
		for (int i = 0; i < groundColliders.Count; i++)
		{
			Collider component = groundColliders[i];
			if (component.GetComponentInParents<GoThroughPlatform>() && this.Sein.GetComponent<GoThroughPlatformHandler>().FallThroughPlatform())
			{
				this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedX = 0f;
				this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedY = 0f;
				this.Sein.PlatformBehaviour.PlatformMovement.Ground.FutureOn = false;
				this.Sein.PlatformBehaviour.PlatformMovement.Ground.IsOn = false;
				this.Sein.PlatformBehaviour.PlatformMovement.Ground.WasOn = false;
				flag = true;
			}
		}
		if (!flag)
		{
			this.PlatformMovement.LocalSpeedY = this.CalculateSpeedFromHeight(this.CrouchJumpHeight);
			this.PlatformMovement.LocalSpeedX = (float)((!this.CharacterSpriteMirror.FaceLeft) ? -3 : 3);
			this.Sein.PlatformBehaviour.AirNoDeceleration.NoDeceleration = true;
			CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.CrouchJumpAnimation, 10, new Func<bool>(this.ShouldBackflipAnimationKeepPlaying));
			characterAnimationState.OnStartPlaying = new Action(this.OnAnimationStart);
			characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
		}
	}

	// Token: 0x060011D2 RID: 4562 RVA: 0x00010204 File Offset: 0x0000E404
	public bool ShouldBackflipAnimationKeepPlaying()
	{
		return this.Sein.PlatformBehaviour.PlatformMovement.IsInAir;
	}

	// Token: 0x060011D3 RID: 4563 RVA: 0x0006A17C File Offset: 0x0006837C
	public bool ShouldJumpIdleAnimationKeepPlaying()
	{
		return this.Sein.PlatformBehaviour.PlatformMovement.IsInAir && (!Characters.Sein.Controller.CanMove || Input.NormalizedHorizontal == 0 || this.PlatformMovement.IsOnWall);
	}

	// Token: 0x060011D4 RID: 4564 RVA: 0x0006A1D8 File Offset: 0x000683D8
	public bool ShouldWallSlideJumpAnimationKeepPlaying()
	{
		return this.PlatformMovement.IsOnWall && this.PlatformMovement.IsInAir && this.PlatformMovement.Jumping && this.PlatformMovement.HeadAgainstWall && this.PlatformMovement.FeetAgainstWall;
	}

	// Token: 0x060011D5 RID: 4565 RVA: 0x0006A234 File Offset: 0x00068434
	public bool ShouldJumpMovingAnimationKeepPlaying()
	{
		return this.Sein.PlatformBehaviour.PlatformMovement.IsInAir && (!Characters.Sein.Controller.CanMove || (this.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput != 0f && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAgainstWall)));
	}

	// Token: 0x060011D6 RID: 4566 RVA: 0x00010204 File Offset: 0x0000E404
	public bool ShouldThirdJumpMovingAnimationKeepPlaying()
	{
		return this.Sein.PlatformBehaviour.PlatformMovement.IsInAir;
	}

	// Token: 0x060011D7 RID: 4567 RVA: 0x0006A2B4 File Offset: 0x000684B4
	public void UpdateTimeSinceFacing()
	{
		this.m_timeSinceMovingLeft += Time.deltaTime;
		this.m_timeSinceMovingRight += Time.deltaTime;
		if (this.PlatformMovement.LocalSpeedX < 0f)
		{
			this.m_timeSinceMovingLeft = 0f;
		}
		if (this.PlatformMovement.LocalSpeedX > 0f)
		{
			this.m_timeSinceMovingRight = 0f;
		}
	}

	// Token: 0x060011D8 RID: 4568 RVA: 0x00010223 File Offset: 0x0000E423
	public void OnAnimationEnd()
	{
		this.SpriteMirrorLock = false;
	}

	// Token: 0x060011D9 RID: 4569 RVA: 0x0001022C File Offset: 0x0000E42C
	public void OnAnimationStart()
	{
		this.SpriteMirrorLock = true;
	}

	// Token: 0x060011DA RID: 4570 RVA: 0x0006A328 File Offset: 0x00068528
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_bunnyHopTimeRemaining);
		ar.Serialize(ref this.m_jumpIdleNumber);
		ar.Serialize(ref this.m_runningJumpNumber);
		ar.Serialize(ref this.m_spriteMirrorLock);
		ar.Serialize(ref this.m_timeSinceMovingLeft);
		ar.Serialize(ref this.m_timeSinceMovingRight);
		ar.Serialize(ref this.m_timeWeCanJumpRemaining);
	}

	// Token: 0x060011DB RID: 4571 RVA: 0x00010235 File Offset: 0x0000E435
	public override void Awake()
	{
		base.Awake();
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x060011DC RID: 4572 RVA: 0x00010253 File Offset: 0x0000E453
	public override void OnDestroy()
	{
		base.OnDestroy();
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x060011DD RID: 4573 RVA: 0x00010271 File Offset: 0x0000E471
	public void OnRestoreCheckpoint()
	{
		this.m_spriteMirrorLock = false;
	}

	// Token: 0x040010E2 RID: 4322
	public TextureAnimationWithTransitions BackflipAnimation;

	// Token: 0x040010E3 RID: 4323
	public float BackflipJumpHeight = 3f;

	// Token: 0x040010E4 RID: 4324
	public TextureAnimationWithTransitions CrouchJumpAnimation;

	// Token: 0x040010E5 RID: 4325
	public float CrouchJumpHeight = 4.5f;

	// Token: 0x040010E6 RID: 4326
	public float DurationSinceLastOnGroundThatWeCanStillJump = 0.2f;

	// Token: 0x040010E7 RID: 4327
	public float FirstJumpHeight = 3f;

	// Token: 0x040010E8 RID: 4328
	public TextureAnimationWithTransitions[] JumpAnimation;

	// Token: 0x040010E9 RID: 4329
	public TextureAnimationWithTransitions[] JumpIdleAnimation;

	// Token: 0x040010EA RID: 4330
	public float JumpIdleHeight = 3f;

	// Token: 0x040010EB RID: 4331
	public float JumpImpulse;

	// Token: 0x040010EC RID: 4332
	public GameObject JumpParticleEffect;

	// Token: 0x040010ED RID: 4333
	public SurfaceToSoundProviderMap JumpSoundProvider;

	// Token: 0x040010EE RID: 4334
	public SurfaceToSoundProviderMap FlipJumpSoundProvider;

	// Token: 0x040010EF RID: 4335
	public SurfaceToSoundProviderMap SpinJumpSoundProvider;

	// Token: 0x040010F0 RID: 4336
	private SurfaceMaterialType m_currentJumpingMaterial;

	// Token: 0x040010F1 RID: 4337
	public float SecondJumpHeight = 3.75f;

	// Token: 0x040010F2 RID: 4338
	public SeinCharacter Sein;

	// Token: 0x040010F3 RID: 4339
	public float ThirdJumpHeight = 4.5f;

	// Token: 0x040010F4 RID: 4340
	public TextureAnimationWithTransitions WallSlideJumpAnimation;

	// Token: 0x040010F5 RID: 4341
	private float m_bunnyHopTimeRemaining;

	// Token: 0x040010F6 RID: 4342
	private int m_jumpIdleNumber;

	// Token: 0x040010F7 RID: 4343
	private int m_runningJumpNumber;

	// Token: 0x040010F8 RID: 4344
	private bool m_spriteMirrorLock;

	// Token: 0x040010F9 RID: 4345
	private float m_timeSinceMovingLeft;

	// Token: 0x040010FA RID: 4346
	private float m_timeSinceMovingRight;

	// Token: 0x040010FB RID: 4347
	private float m_timeWeCanJumpRemaining;

	// Token: 0x040010FC RID: 4348
	private Func<bool> m_shouldJumpMoving;

	// Token: 0x040010FD RID: 4349
	private Action onAnimationEnd;
}
