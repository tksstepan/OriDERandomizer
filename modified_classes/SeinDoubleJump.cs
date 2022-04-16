using System;
using System.Runtime.CompilerServices;
using Core;
using UnityEngine;

public class SeinDoubleJump : CharacterState, ISeinReceiver
{
	static SeinDoubleJump()
	{
		SeinDoubleJump.OnDoubleJumpEvent = delegate
		{
		};
	}

	public static event Action<float> OnDoubleJumpEvent
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			SeinDoubleJump.OnDoubleJumpEvent = (Action<float>)Delegate.Combine(SeinDoubleJump.OnDoubleJumpEvent, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			SeinDoubleJump.OnDoubleJumpEvent = (Action<float>)Delegate.Remove(SeinDoubleJump.OnDoubleJumpEvent, value);
		}
	}

	public int ExtraJumpsAvailable
	{
		get
		{
			int bonus = RandomizerBonus.DoubleJumpUpgrades();
			if (CheatsHandler.InfiniteDoubleJumps)
			{
				return 999999;
			}
			if (this.Sein.PlayerAbilities.DoubleJumpUpgrade.HasAbility)
			{
				return 2 + bonus;
			}
			return 1 + bonus;
		}
	}

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public SeinJump Jump
	{
		get
		{
			return this.Sein.Abilities.Jump;
		}
	}

	public bool CanDoubleJump
	{
		get
		{
			return base.enabled && !this.PlatformMovement.IsOnGround && this.m_numberOfJumpsAvailable != 0 && this.m_remainingLockTime <= 0f && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities);
		}
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.DoubleJump = this;
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_doubleJumpTime);
		ar.Serialize(ref this.m_numberOfJumpsAvailable);
		ar.Serialize(ref this.m_remainingLockTime);
	}

	public void PerformDoubleJump()
	{
		if (this.Sein.Abilities.ChargeJump)
		{
			this.Sein.Abilities.ChargeJump.OnDoubleJump();
		}
		this.PlatformMovement.LocalSpeedY = this.JumpStrength * RandomizerBonus.DoubleJumpscale;
		this.m_numberOfJumpsAvailable--;
		this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.DoubleJumpAnimation, 10, new Func<bool>(this.ShouldDoubleJumpAnimationKeepPlaying));
		this.m_doubleJumpSound = Sound.Play(this.DoubleJumpSound.GetSound(null), this.Sein.PlatformBehaviour.PlatformMovement.Position, delegate
		{
			this.m_doubleJumpSound = null;
		});
		SeinDoubleJump.OnDoubleJumpEvent(this.JumpStrength * RandomizerBonus.DoubleJumpscale);
		GameObject original = this.DoubleJumpAfterShock;
		if (this.m_numberOfJumpsAvailable == 0 && this.ExtraJumpsAvailable == 2)
		{
			original = this.TrippleJumpAfterShock;
		}
		Vector2 worldSpeed = this.PlatformMovement.WorldSpeed;
		float num = Mathf.Atan2(worldSpeed.x, worldSpeed.y) * 57.29578f;
		InstantiateUtility.Instantiate(original, this.Sein.Position, Quaternion.Euler(0f, 0f, -num));
		JumpFlipPlatform.OnSeinDoubleJumpEvent();
	}

	public bool ShouldDoubleJumpAnimationKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnCeiling;
	}

	public override void UpdateCharacterState()
	{
		if (this.Sein.IsSuspended)
		{
			return;
		}
		if (this.PlatformMovement.IsOnGround && this.m_numberOfJumpsAvailable != this.ExtraJumpsAvailable)
		{
			this.ResetDoubleJump();
		}
		if (this.m_doubleJumpSound && (this.PlatformMovement.IsOnWall || this.PlatformMovement.IsOnCeiling))
		{
			this.m_doubleJumpSound.FadeOut(0.5f, true);
			UberPoolManager.Instance.RemoveOnDestroyed(this.m_doubleJumpSound.gameObject);
			this.m_doubleJumpSound = null;
		}
		if (this.m_remainingLockTime > 0f)
		{
			this.m_remainingLockTime -= Time.deltaTime;
		}
		if (this.m_doubleJumpTime > 0f)
		{
			if (this.PlatformMovement.LocalSpeedY <= 0f)
			{
				this.m_doubleJumpTime = 0f;
			}
			this.m_doubleJumpTime -= Time.deltaTime;
		}
	}

	public void ResetDoubleJump()
	{
		this.m_numberOfJumpsAvailable = this.ExtraJumpsAvailable;
	}

	public void LockForDuration(float duration)
	{
		this.m_remainingLockTime = Mathf.Max(this.m_remainingLockTime, duration);
	}

	public void ResetLock()
	{
		this.m_remainingLockTime = 0f;
	}

	public TextureAnimationWithTransitions[] DoubleJumpAnimation;

	public GameObject DoubleJumpAfterShock;

	public GameObject TrippleJumpAfterShock;

	public SoundProvider DoubleJumpSound;

	public float JumpStrength;

	public SeinCharacter Sein;

	private SoundPlayer m_doubleJumpSound;

	private float m_doubleJumpTime;

	private int m_numberOfJumpsAvailable;

	private float m_remainingLockTime;
}
