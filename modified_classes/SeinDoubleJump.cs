using System;
using System.Runtime.CompilerServices;
using Core;
using UnityEngine;

// Token: 0x0200031C RID: 796
public class SeinDoubleJump : CharacterState, ISeinReceiver
{
	// Token: 0x06001090 RID: 4240 RVA: 0x0000E7C9 File Offset: 0x0000C9C9
	static SeinDoubleJump()
	{
		SeinDoubleJump.OnDoubleJumpEvent = delegate
		{
		};
	}

	// Token: 0x14000014 RID: 20
	// (add) Token: 0x06001091 RID: 4241 RVA: 0x0000E7ED File Offset: 0x0000C9ED
	// (remove) Token: 0x06001092 RID: 4242 RVA: 0x0000E804 File Offset: 0x0000CA04
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

	// Token: 0x170002AE RID: 686
	// (get) Token: 0x06001093 RID: 4243
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

	// Token: 0x170002AF RID: 687
	// (get) Token: 0x06001094 RID: 4244 RVA: 0x0000E852 File Offset: 0x0000CA52
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x170002B0 RID: 688
	// (get) Token: 0x06001095 RID: 4245 RVA: 0x0000E864 File Offset: 0x0000CA64
	public SeinJump Jump
	{
		get
		{
			return this.Sein.Abilities.Jump;
		}
	}

	// Token: 0x170002B1 RID: 689
	// (get) Token: 0x06001096 RID: 4246 RVA: 0x00062CE4 File Offset: 0x00060EE4
	public bool CanDoubleJump
	{
		get
		{
			return base.enabled && !this.PlatformMovement.IsOnGround && this.m_numberOfJumpsAvailable != 0 && this.m_remainingLockTime <= 0f && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities);
		}
	}

	// Token: 0x06001097 RID: 4247 RVA: 0x0000E876 File Offset: 0x0000CA76
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.DoubleJump = this;
	}

	// Token: 0x06001098 RID: 4248 RVA: 0x0000E890 File Offset: 0x0000CA90
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_doubleJumpTime);
		ar.Serialize(ref this.m_numberOfJumpsAvailable);
		ar.Serialize(ref this.m_remainingLockTime);
	}

	// Token: 0x06001099 RID: 4249 RVA: 0x00062D34 File Offset: 0x00060F34
	public void PerformDoubleJump()
	{
		if (this.Sein.Abilities.ChargeJump)
		{
			this.Sein.Abilities.ChargeJump.OnDoubleJump();
		}
		this.PlatformMovement.LocalSpeedY = this.JumpStrength;
		this.m_numberOfJumpsAvailable--;
		this.Sein.PlatformBehaviour.Visuals.Animation.PlayRandom(this.DoubleJumpAnimation, 10, new Func<bool>(this.ShouldDoubleJumpAnimationKeepPlaying));
		this.m_doubleJumpSound = Sound.Play(this.DoubleJumpSound.GetSound(null), this.Sein.PlatformBehaviour.PlatformMovement.Position, delegate
		{
			this.m_doubleJumpSound = null;
		});
		SeinDoubleJump.OnDoubleJumpEvent(this.JumpStrength);
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

	// Token: 0x0600109A RID: 4250 RVA: 0x0000E8B6 File Offset: 0x0000CAB6
	public bool ShouldDoubleJumpAnimationKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnCeiling;
	}

	// Token: 0x0600109B RID: 4251 RVA: 0x00062E80 File Offset: 0x00061080
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

	// Token: 0x0600109C RID: 4252 RVA: 0x0000E8D9 File Offset: 0x0000CAD9
	public void ResetDoubleJump()
	{
		this.m_numberOfJumpsAvailable = this.ExtraJumpsAvailable;
	}

	// Token: 0x0600109D RID: 4253 RVA: 0x0000E8E7 File Offset: 0x0000CAE7
	public void LockForDuration(float duration)
	{
		this.m_remainingLockTime = Mathf.Max(this.m_remainingLockTime, duration);
	}

	// Token: 0x0600109E RID: 4254 RVA: 0x0000E8FB File Offset: 0x0000CAFB
	public void ResetLock()
	{
		this.m_remainingLockTime = 0f;
	}

	// Token: 0x04000FC9 RID: 4041
	public TextureAnimationWithTransitions[] DoubleJumpAnimation;

	// Token: 0x04000FCA RID: 4042
	public GameObject DoubleJumpAfterShock;

	// Token: 0x04000FCB RID: 4043
	public GameObject TrippleJumpAfterShock;

	// Token: 0x04000FCC RID: 4044
	public SoundProvider DoubleJumpSound;

	// Token: 0x04000FCD RID: 4045
	public float JumpStrength;

	// Token: 0x04000FCE RID: 4046
	public SeinCharacter Sein;

	// Token: 0x04000FCF RID: 4047
	private SoundPlayer m_doubleJumpSound;

	// Token: 0x04000FD0 RID: 4048
	private float m_doubleJumpTime;

	// Token: 0x04000FD1 RID: 4049
	private int m_numberOfJumpsAvailable;

	// Token: 0x04000FD2 RID: 4050
	private float m_remainingLockTime;
}
