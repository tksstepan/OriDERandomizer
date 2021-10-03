using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class SeinJump : CharacterState, ISeinReceiver
{
	public event Action<float> OnJumpEvent = delegate(float A_0)
	{
	};

	public bool CanJump
	{
		get
		{
			return base.enabled && this.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedY <= 0.0001f && this.m_timeWeCanJumpRemaining > 0f && !this.Sein.PlatformBehaviour.PlatformMovement.Ceiling.IsOn && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities);
		}
	}

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
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

	public CharacterSpriteMirror CharacterSpriteMirror
	{
		get
		{
			return this.Sein.PlatformBehaviour.Visuals.SpriteMirror;
		}
	}

	public bool HasSharplyTurnedAround
	{
		get
		{
			return (this.m_timeSinceMovingRight > 0f && this.m_timeSinceMovingRight < 0.2f && this.PlatformMovement.LocalSpeedX < 0f) || (this.m_timeSinceMovingLeft > 0f && this.m_timeSinceMovingLeft < 0.2f && this.PlatformMovement.LocalSpeedX > 0f);
		}
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.Jump = this;
	}

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

	public void ResetRunningJumpCount()
	{
		this.m_runningJumpNumber = 0;
	}

	public void ResetJumpIdleCount()
	{
		this.m_jumpIdleNumber = 0;
	}

	public float CalculateSpeedFromHeight(float height)
	{
		return PhysicsHelper.CalculateSpeedFromHeight(height * RandomizerBonus.Jumpscale, this.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityStrength);
	}

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

	public bool ShouldBackflipAnimationKeepPlaying()
	{
		return this.Sein.PlatformBehaviour.PlatformMovement.IsInAir;
	}

	public bool ShouldJumpIdleAnimationKeepPlaying()
	{
		return this.Sein.PlatformBehaviour.PlatformMovement.IsInAir && (!Characters.Sein.Controller.CanMove || Input.NormalizedHorizontal == 0 || this.PlatformMovement.IsOnWall);
	}

	public bool ShouldWallSlideJumpAnimationKeepPlaying()
	{
		return this.PlatformMovement.IsOnWall && this.PlatformMovement.IsInAir && this.PlatformMovement.Jumping && this.PlatformMovement.HeadAgainstWall && this.PlatformMovement.FeetAgainstWall;
	}

	public bool ShouldJumpMovingAnimationKeepPlaying()
	{
		return this.Sein.PlatformBehaviour.PlatformMovement.IsInAir && (!Characters.Sein.Controller.CanMove || (this.Sein.PlatformBehaviour.LeftRightMovement.HorizontalInput != 0f && (!this.PlatformMovement.IsOnWall || !this.PlatformMovement.HeadAgainstWall)));
	}

	public bool ShouldThirdJumpMovingAnimationKeepPlaying()
	{
		return this.Sein.PlatformBehaviour.PlatformMovement.IsInAir;
	}

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

	public void OnAnimationEnd()
	{
		this.SpriteMirrorLock = false;
	}

	public void OnAnimationStart()
	{
		this.SpriteMirrorLock = true;
	}

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

	public void OnRestoreCheckpoint()
	{
		this.m_spriteMirrorLock = false;
	}

	public TextureAnimationWithTransitions BackflipAnimation;

	public float BackflipJumpHeight = 3f;

	public TextureAnimationWithTransitions CrouchJumpAnimation;

	public float CrouchJumpHeight = 4.5f;

	public float DurationSinceLastOnGroundThatWeCanStillJump = 0.2f;

	public float FirstJumpHeight = 3f;

	public TextureAnimationWithTransitions[] JumpAnimation;

	public TextureAnimationWithTransitions[] JumpIdleAnimation;

	public float JumpIdleHeight = 3f;

	public float JumpImpulse;

	public GameObject JumpParticleEffect;

	public SurfaceToSoundProviderMap JumpSoundProvider;

	public SurfaceToSoundProviderMap FlipJumpSoundProvider;

	public SurfaceToSoundProviderMap SpinJumpSoundProvider;

	private SurfaceMaterialType m_currentJumpingMaterial;

	public float SecondJumpHeight = 3.75f;

	public SeinCharacter Sein;

	public float ThirdJumpHeight = 4.5f;

	public TextureAnimationWithTransitions WallSlideJumpAnimation;

	private float m_bunnyHopTimeRemaining;

	private int m_jumpIdleNumber;

	private int m_runningJumpNumber;

	private bool m_spriteMirrorLock;

	private float m_timeSinceMovingLeft;

	private float m_timeSinceMovingRight;

	private float m_timeWeCanJumpRemaining;

	private Func<bool> m_shouldJumpMoving;

	private Action onAnimationEnd;
}
