using System;
using System.Collections;
using Core;
using Game;
using UnityEngine;

public class SeinWallJump : CharacterState, ISeinReceiver
{
	public event Action<Vector2> OnWallJumpEvent = delegate(Vector2 A_0)
	{
	};

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public SeinDoubleJump DoubleJump
	{
		get
		{
			return this.Sein.Abilities.DoubleJump;
		}
	}

	public CharacterLeftRightMovement LeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	public CharacterSpriteMirror CharacterSpriteMirror
	{
		get
		{
			return this.Sein.PlatformBehaviour.Visuals.SpriteMirror;
		}
	}

	public bool CanPerformWallJump
	{
		get
		{
			return base.enabled && this.Sein.Abilities.WallSlide.IsOnWall && !this.PlatformMovement.IsOnGround && this.Sein.PlayerAbilities.WallJump.HasAbility;
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

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.WallJump = this;
	}

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
		this.PlatformMovement.LocalSpeedX = -this.JumpStrength.x * RandomizerBonus.Jumpscale;
		this.PlatformMovement.LocalSpeedY = this.JumpStrength.y * RandomizerBonus.Jumpscale;
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
				this.PlatformMovement.LocalSpeedX = this.JumpStrength.x  * RandomizerBonus.Jumpscale * (float)((!left) ? -1 : 1);
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

	public void OnAnimationEnd()
	{
		this.SpriteMirrorLock = false;
	}

	public void OnAnimationStart()
	{
		this.SpriteMirrorLock = true;
	}

	public bool ShouldKeepPlayingWallJumpLeftTowardsAnimation()
	{
		return this.LeftRightMovement.HorizontalInput >= 0f && this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnCeiling && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	public bool ShouldKeepPlayingWallJumpLeftAwayAnimation()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnCeiling && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	public bool ShouldKeepPlayingWallJumpLeftRegularAnimation()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnCeiling && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	public override void Awake()
	{
		base.Awake();
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

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
		this.PlatformMovement.LocalSpeedX = this.JumpStrength.x  * RandomizerBonus.Jumpscale;
		this.PlatformMovement.LocalSpeedY = this.JumpStrength.y  * RandomizerBonus.Jumpscale;
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

	public bool ShouldKeepPlayingWallJumpRightTowardsAnimation()
	{
		return this.LeftRightMovement.HorizontalInput <= 0f && this.PlatformMovement.IsInAir && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	public bool ShouldKeepPlayingWallJumpRightAwayAnimation()
	{
		return this.PlatformMovement.IsInAir && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

	public bool ShouldKeepPlayingWallJumpRightRegularAnimation()
	{
		return this.PlatformMovement.IsInAir && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAndFeetAgainstWall);
	}

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

	public override void UpdateCharacterState()
	{
		if (this.PlatformMovement.IsOnGround)
		{
			this.m_hasWallJumpedLeft = false;
			this.m_hasWallJumpedRight = false;
		}
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_hasWallJumpedLeft);
		ar.Serialize(ref this.m_hasWallJumpedRight);
		ar.Serialize(ref this.m_lockInputTimeRemaining);
		ar.Serialize(ref this.m_spriteMirrorLock);
	}

	public void OnRestoreCheckpoint()
	{
		this.m_spriteMirrorLock = false;
	}

	public TextureAnimationWithTransitions[] AwayAnimation;

	public bool DontAllowJumpingTowardsWall;

	public TextureAnimationWithTransitions[] EdgeJumpAnimation;

	public Vector2 JumpStrength;

	public bool LimitWallJumping;

	public float LockDoubleJumpTowardsDuration = 1.5f;

	public HorizontalPlatformMovementSettings.SpeedMultiplierSet MoveSpeed;

	public TextureAnimationWithTransitions[] RegularAnimation;

	public SeinCharacter Sein;

	public TextureAnimationWithTransitions[] TowardsAnimation;

	public float WallJumpImpulse = 20f;

	public SurfaceToSoundProviderMap WallJumpSound;

	private bool m_hasWallJumpedLeft;

	private bool m_hasWallJumpedRight;

	private float m_lockInputTimeRemaining;

	private bool m_spriteMirrorLock;
}
