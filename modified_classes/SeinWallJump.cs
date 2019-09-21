using System;
using System.Collections;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000345 RID: 837
public class SeinWallJump : CharacterState, ISeinReceiver
{
	// Token: 0x1400001B RID: 27
	// (add) Token: 0x0600126A RID: 4714 RVA: 0x00010999 File Offset: 0x0000EB99
	// (remove) Token: 0x0600126B RID: 4715 RVA: 0x000109B2 File Offset: 0x0000EBB2
	public event Action<Vector2> OnWallJumpEvent = delegate(Vector2 A_0)
	{
	};

	// Token: 0x17000325 RID: 805
	// (get) Token: 0x0600126C RID: 4716 RVA: 0x000109CB File Offset: 0x0000EBCB
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x17000326 RID: 806
	// (get) Token: 0x0600126D RID: 4717 RVA: 0x000109DD File Offset: 0x0000EBDD
	public SeinDoubleJump DoubleJump
	{
		get
		{
			return this.Sein.Abilities.DoubleJump;
		}
	}

	// Token: 0x17000327 RID: 807
	// (get) Token: 0x0600126E RID: 4718 RVA: 0x000109EF File Offset: 0x0000EBEF
	public CharacterLeftRightMovement LeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	// Token: 0x17000328 RID: 808
	// (get) Token: 0x0600126F RID: 4719 RVA: 0x00010A01 File Offset: 0x0000EC01
	public CharacterSpriteMirror CharacterSpriteMirror
	{
		get
		{
			return this.Sein.PlatformBehaviour.Visuals.SpriteMirror;
		}
	}

	// Token: 0x17000329 RID: 809
	// (get) Token: 0x06001270 RID: 4720 RVA: 0x0006C330 File Offset: 0x0006A530
	public bool CanPerformWallJump
	{
		get
		{
			return base.enabled && this.Sein.Abilities.WallSlide.IsOnWall && !this.PlatformMovement.IsOnGround && this.Sein.PlayerAbilities.WallJump.HasAbility;
		}
	}

	// Token: 0x1700032A RID: 810
	// (get) Token: 0x06001271 RID: 4721 RVA: 0x00010A18 File Offset: 0x0000EC18
	// (set) Token: 0x06001272 RID: 4722 RVA: 0x0006C38C File Offset: 0x0006A58C
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

	// Token: 0x06001273 RID: 4723 RVA: 0x00010A20 File Offset: 0x0000EC20
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.WallJump = this;
	}

	// Token: 0x06001274 RID: 4724 RVA: 0x00010A3A File Offset: 0x0000EC3A
	public void PerformWallJump()
	{
		if (this.PlatformMovement.HasWallLeft)
		{
			this.PerformWallJumpRight();
		}
		if (this.PlatformMovement.HasWallRight)
		{
			this.PerformWallJumpLeft();
		}
	}

	// Token: 0x06001275 RID: 4725 RVA: 0x0006C3E0 File Offset: 0x0006A5E0
	public void PerformWallJumpLeft()
	{
		if (this.m_hasWallJumpedLeft)
		{
			return;
		}
		if (this.DontAllowJumpingTowardsWall && this.LeftRightMovement.BaseHorizontalInput > 0f)
		{
			return;
		}
		if (this.LeftRightMovement.BaseHorizontalInput > 0f && this.DoubleJump)
		{
			this.DoubleJump.LockForDuration(this.LockDoubleJumpTowardsDuration);
		}
		if (this.LimitWallJumping)
		{
			this.m_hasWallJumpedLeft = true;
		}
		this.m_hasWallJumpedRight = false;
		this.PlatformMovement.LocalSpeedX = -this.JumpStrength.x;
		this.PlatformMovement.LocalSpeedY = this.JumpStrength.y;
		Vector2 localSpeed = this.PlatformMovement.LocalSpeed;
		this.ApplyImpulseToWall(localSpeed);
		if (this.Sein.Input.NormalizedHorizontal < 0)
		{
			this.CharacterSpriteMirror.FaceLeft = true;
			CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.AwayAnimation, 10, new Func<bool>(this.ShouldKeepPlayingWallJumpLeftAwayAnimation));
			characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
			characterAnimationState.OnStartPlaying = new Action(this.OnAnimationStart);
		}
		else if (this.Sein.Input.NormalizedHorizontal > 0)
		{
			Vector3 origin = this.PlatformMovement.Position2D + this.PlatformMovement.LocalToWorld(Vector3.up * 2f);
			float maxDistance = this.PlatformMovement.CapsuleCollider.radius + 2f;
			Ray ray = new Ray(origin, this.PlatformMovement.LocalToWorld(Vector3.right));
			if (Physics.Raycast(ray, maxDistance))
			{
				CharacterAnimationSystem.CharacterAnimationState characterAnimationState2 = this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.TowardsAnimation, 10, new Func<bool>(this.ShouldKeepPlayingWallJumpLeftTowardsAnimation));
				characterAnimationState2.OnStopPlaying = new Action(this.OnAnimationEnd);
				base.StartCoroutine(this.RoutineForMegWhoPlaysMarioAndSucksAtWallJumping());
			}
			else
			{
				CharacterAnimationSystem.CharacterAnimationState characterAnimationState3 = this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.EdgeJumpAnimation, 10, new Func<bool>(this.ShouldKeepPlayingWallJumpLeftTowardsAnimation));
				characterAnimationState3.OnStopPlaying = new Action(this.OnAnimationEnd);
				localSpeed.y = 0f;
			}
		}
		else
		{
			CharacterAnimationSystem.CharacterAnimationState characterAnimationState4 = this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.RegularAnimation, 10, new Func<bool>(this.ShouldKeepPlayingWallJumpLeftRegularAnimation));
			characterAnimationState4.OnStopPlaying = new Action(this.OnAnimationEnd);
			characterAnimationState4.OnStartPlaying = new Action(this.OnAnimationStart);
		}
		Sound.Play(this.WallJumpSound.GetSoundForMaterial(this.Sein.PlatformBehaviour.WallSurfaceMaterialType, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.OnWallJumpEvent(localSpeed);
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(localSpeed.y, 1f);
		}
		this.Sein.PlatformBehaviour.AirNoDeceleration.NoDeceleration = true;
		this.Sein.ResetAirLimits();
		JumpFlipPlatform.OnSeinWallJumpEvent();
	}

	// Token: 0x06001276 RID: 4726 RVA: 0x0006C760 File Offset: 0x0006A960
	public IEnumerator RoutineForMegWhoPlaysMarioAndSucksAtWallJumping()
	{
		float i = (float)this.Sein.Input.NormalizedHorizontal;
		bool left = i < 0f;
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		for (float t = 0f; t < 0.2f; t += Time.deltaTime)
		{
			if (Core.Input.Jump.OnPressed)
			{
				break;
			}
			if (this.PlatformMovement.IsOnWall)
			{
				break;
			}
			if ((float)this.Sein.Input.NormalizedHorizontal == -i)
			{
				this.PlatformMovement.LocalSpeedX = this.JumpStrength.x * (float)((!left) ? -1 : 1);
				this.CharacterSpriteMirror.FaceLeft = !left;
				CharacterAnimationSystem.CharacterAnimationState state = this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.AwayAnimation, 10, new Func<bool>(this.ShouldKeepPlayingWallJumpLeftAwayAnimation));
				state.OnStopPlaying = new Action(this.OnAnimationEnd);
				state.OnStartPlaying = new Action(this.OnAnimationStart);
				if (this.DoubleJump)
				{
					this.DoubleJump.ResetLock();
				}
				break;
			}
			yield return new WaitForFixedUpdate();
		}
		yield break;
	}

	// Token: 0x06001277 RID: 4727 RVA: 0x00010A68 File Offset: 0x0000EC68
	public void OnAnimationEnd()
	{
		this.SpriteMirrorLock = false;
	}

	// Token: 0x06001278 RID: 4728 RVA: 0x00010A71 File Offset: 0x0000EC71
	public void OnAnimationStart()
	{
		this.SpriteMirrorLock = true;
	}

	// Token: 0x06001279 RID: 4729 RVA: 0x0006C77C File Offset: 0x0006A97C
	public bool ShouldKeepPlayingWallJumpLeftTowardsAnimation()
	{
		return this.LeftRightMovement.HorizontalInput >= 0f && this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnCeiling && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	// Token: 0x0600127A RID: 4730 RVA: 0x0006C7E4 File Offset: 0x0006A9E4
	public bool ShouldKeepPlayingWallJumpLeftAwayAnimation()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnCeiling && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	// Token: 0x0600127B RID: 4731 RVA: 0x0006C7E4 File Offset: 0x0006A9E4
	public bool ShouldKeepPlayingWallJumpLeftRegularAnimation()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnCeiling && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	// Token: 0x0600127C RID: 4732 RVA: 0x00010A7A File Offset: 0x0000EC7A
	public override void Awake()
	{
		base.Awake();
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x0600127D RID: 4733 RVA: 0x00010A98 File Offset: 0x0000EC98
	public override void OnDestroy()
	{
		base.OnDestroy();
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x0600127E RID: 4734 RVA: 0x0006C838 File Offset: 0x0006AA38
	public void PerformWallJumpRight()
	{
		if (this.m_hasWallJumpedRight)
		{
			return;
		}
		if (this.DontAllowJumpingTowardsWall && this.LeftRightMovement.BaseHorizontalInput < 0f)
		{
			return;
		}
		if (this.LeftRightMovement.BaseHorizontalInput < 0f && this.DoubleJump)
		{
			this.DoubleJump.LockForDuration(this.LockDoubleJumpTowardsDuration);
		}
		if (this.LimitWallJumping)
		{
			this.m_hasWallJumpedRight = true;
		}
		this.m_hasWallJumpedLeft = false;
		this.PlatformMovement.LocalSpeedX = this.JumpStrength.x;
		this.PlatformMovement.LocalSpeedY = this.JumpStrength.y;
		Vector2 localSpeed = this.PlatformMovement.LocalSpeed;
		this.ApplyImpulseToWall(localSpeed);
		if (this.Sein.Input.NormalizedHorizontal > 0)
		{
			this.CharacterSpriteMirror.FaceLeft = false;
			CharacterAnimationSystem.CharacterAnimationState characterAnimationState = this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.AwayAnimation, 10, new Func<bool>(this.ShouldKeepPlayingWallJumpRightAwayAnimation));
			characterAnimationState.OnStopPlaying = new Action(this.OnAnimationEnd);
			characterAnimationState.OnStartPlaying = new Action(this.OnAnimationStart);
		}
		else if (this.Sein.Input.NormalizedHorizontal < 0)
		{
			Vector3 origin = this.PlatformMovement.Position + Vector3.up * 2f;
			float maxDistance = this.PlatformMovement.CapsuleCollider.radius + 2f;
			Ray ray = new Ray(origin, this.PlatformMovement.LocalToWorld(Vector3.left));
			if (Physics.Raycast(ray, maxDistance))
			{
				CharacterAnimationSystem.CharacterAnimationState characterAnimationState2 = this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.TowardsAnimation, 10, new Func<bool>(this.ShouldKeepPlayingWallJumpRightTowardsAnimation));
				characterAnimationState2.OnStopPlaying = new Action(this.OnAnimationEnd);
				base.StartCoroutine(this.RoutineForMegWhoPlaysMarioAndSucksAtWallJumping());
			}
			else
			{
				CharacterAnimationSystem.CharacterAnimationState characterAnimationState3 = this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.EdgeJumpAnimation, 10, new Func<bool>(this.ShouldKeepPlayingWallJumpRightTowardsAnimation));
				characterAnimationState3.OnStopPlaying = new Action(this.OnAnimationEnd);
				localSpeed.y = 0f;
			}
		}
		else
		{
			CharacterAnimationSystem.CharacterAnimationState characterAnimationState4 = this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.RegularAnimation, 10, new Func<bool>(this.ShouldKeepPlayingWallJumpRightRegularAnimation));
			characterAnimationState4.OnStopPlaying = new Action(this.OnAnimationEnd);
			characterAnimationState4.OnStartPlaying = new Action(this.OnAnimationStart);
		}
		Sound.Play(this.WallJumpSound.GetSoundForMaterial(this.Sein.PlatformBehaviour.WallSurfaceMaterialType, null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.OnWallJumpEvent(localSpeed);
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(localSpeed.y, 1f);
		}
		this.Sein.PlatformBehaviour.AirNoDeceleration.NoDeceleration = true;
		this.Sein.ResetAirLimits();
		JumpFlipPlatform.OnSeinWallJumpEvent();
	}

	// Token: 0x0600127F RID: 4735 RVA: 0x0006CBA0 File Offset: 0x0006ADA0
	public bool ShouldKeepPlayingWallJumpRightTowardsAnimation()
	{
		return this.LeftRightMovement.HorizontalInput <= 0f && this.PlatformMovement.IsInAir && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	// Token: 0x06001280 RID: 4736 RVA: 0x00010AB6 File Offset: 0x0000ECB6
	public bool ShouldKeepPlayingWallJumpRightAwayAnimation()
	{
		return this.PlatformMovement.IsInAir && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	// Token: 0x06001281 RID: 4737 RVA: 0x00010AB6 File Offset: 0x0000ECB6
	public bool ShouldKeepPlayingWallJumpRightRegularAnimation()
	{
		return this.PlatformMovement.IsInAir && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	// Token: 0x06001282 RID: 4738 RVA: 0x0006CBF8 File Offset: 0x0006ADF8
	public void ApplyImpulseToWall(Vector2 speed)
	{
		PlatformMovementListOfColliders platformMovementListOfColliders = this.Sein.PlatformBehaviour.PlatformMovementListOfColliders;
		for (int i = 0; i < platformMovementListOfColliders.WallLeftColliders.Count; i++)
		{
			Collider collider = platformMovementListOfColliders.WallLeftColliders[i];
			if (collider)
			{
				Rigidbody attachedRigidbody = collider.attachedRigidbody;
				if (attachedRigidbody)
				{
					Vector3 force = this.PlatformMovement.LocalToWorld(-speed.normalized * this.WallJumpImpulse);
					attachedRigidbody.AddForceAtPosition(force, this.PlatformMovement.Position, ForceMode.Impulse);
				}
			}
		}
		for (int j = 0; j < platformMovementListOfColliders.WallRightColliders.Count; j++)
		{
			Collider collider2 = platformMovementListOfColliders.WallRightColliders[j];
			if (collider2)
			{
				Rigidbody attachedRigidbody2 = collider2.attachedRigidbody;
				if (attachedRigidbody2)
				{
					Vector3 force2 = this.PlatformMovement.LocalToWorld(-speed.normalized * this.WallJumpImpulse);
					attachedRigidbody2.AddForceAtPosition(force2, this.PlatformMovement.Position, ForceMode.Impulse);
				}
			}
		}
	}

	// Token: 0x06001283 RID: 4739 RVA: 0x00010AEC File Offset: 0x0000ECEC
	public override void UpdateCharacterState()
	{
		if (this.PlatformMovement.IsOnGround)
		{
			this.m_hasWallJumpedLeft = false;
			this.m_hasWallJumpedRight = false;
		}
	}

	// Token: 0x06001284 RID: 4740 RVA: 0x00010B0C File Offset: 0x0000ED0C
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_hasWallJumpedLeft);
		ar.Serialize(ref this.m_hasWallJumpedRight);
		ar.Serialize(ref this.m_lockInputTimeRemaining);
		ar.Serialize(ref this.m_spriteMirrorLock);
	}

	// Token: 0x06001285 RID: 4741 RVA: 0x00010B3E File Offset: 0x0000ED3E
	public void OnRestoreCheckpoint()
	{
		this.m_spriteMirrorLock = false;
	}

	// Token: 0x04001164 RID: 4452
	public TextureAnimationWithTransitions[] AwayAnimation;

	// Token: 0x04001165 RID: 4453
	public bool DontAllowJumpingTowardsWall;

	// Token: 0x04001166 RID: 4454
	public TextureAnimationWithTransitions[] EdgeJumpAnimation;

	// Token: 0x04001167 RID: 4455
	public Vector2 JumpStrength;

	// Token: 0x04001168 RID: 4456
	public bool LimitWallJumping;

	// Token: 0x04001169 RID: 4457
	public float LockDoubleJumpTowardsDuration = 1.5f;

	// Token: 0x0400116A RID: 4458
	public HorizontalPlatformMovementSettings.SpeedMultiplierSet MoveSpeed;

	// Token: 0x0400116B RID: 4459
	public TextureAnimationWithTransitions[] RegularAnimation;

	// Token: 0x0400116C RID: 4460
	public SeinCharacter Sein;

	// Token: 0x0400116D RID: 4461
	public TextureAnimationWithTransitions[] TowardsAnimation;

	// Token: 0x0400116E RID: 4462
	public float WallJumpImpulse = 20f;

	// Token: 0x0400116F RID: 4463
	public SurfaceToSoundProviderMap WallJumpSound;

	// Token: 0x04001170 RID: 4464
	private bool m_hasWallJumpedLeft;

	// Token: 0x04001171 RID: 4465
	private bool m_hasWallJumpedRight;

	// Token: 0x04001172 RID: 4466
	private float m_lockInputTimeRemaining;

	// Token: 0x04001173 RID: 4467
	private bool m_spriteMirrorLock;
}
