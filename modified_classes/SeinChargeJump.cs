using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000310 RID: 784
public class SeinChargeJump : CharacterState, ISeinReceiver
{
	// Token: 0x06000FF2 RID: 4082 RVA: 0x00060798 File Offset: 0x0005E998
	public SeinChargeJump()
	{
	}

	// Token: 0x14000011 RID: 17
	// (add) Token: 0x06000FF3 RID: 4083 RVA: 0x0000DE7B File Offset: 0x0000C07B
	// (remove) Token: 0x06000FF4 RID: 4084 RVA: 0x0000DE94 File Offset: 0x0000C094
	public event Action<float> OnJumpEvent = delegate
	{
	};

	// Token: 0x1700028B RID: 651
	// (get) Token: 0x06000FF5 RID: 4085 RVA: 0x0000DEAD File Offset: 0x0000C0AD
	public PlayerAbilities PlayerAbilities
	{
		get
		{
			return this.Sein.PlayerAbilities;
		}
	}

	// Token: 0x1700028C RID: 652
	// (get) Token: 0x06000FF6 RID: 4086 RVA: 0x0000DEBA File Offset: 0x0000C0BA
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x1700028D RID: 653
	// (get) Token: 0x06000FF7 RID: 4087 RVA: 0x0000DECC File Offset: 0x0000C0CC
	public SeinChargeJump ChargeJump
	{
		get
		{
			return this.Sein.Abilities.ChargeJump;
		}
	}

	// Token: 0x1700028E RID: 654
	// (get) Token: 0x06000FF8 RID: 4088 RVA: 0x0000DEDE File Offset: 0x0000C0DE
	public CharacterUpwardsDeceleration UpwardsDeceleration
	{
		get
		{
			return this.Sein.PlatformBehaviour.UpwardsDeceleration;
		}
	}

	// Token: 0x06000FF9 RID: 4089 RVA: 0x0000DEF0 File Offset: 0x0000C0F0
	public void OnDoubleJump()
	{
		this.UpwardsDeceleration.Reset();
		this.ChangeState(SeinChargeJump.State.Normal);
	}

	// Token: 0x06000FFA RID: 4090 RVA: 0x0000DF04 File Offset: 0x0000C104
	public override void UpdateCharacterState()
	{
		if (this.Sein.IsSuspended)
		{
			return;
		}
		this.UpdateState();
	}

	// Token: 0x06000FFB RID: 4091 RVA: 0x000607F8 File Offset: 0x0005E9F8
	public void ChangeState(SeinChargeJump.State state)
	{
		this.CurrentState = state;
		this.m_stateCurrentTime = 0f;
		this.m_attackablesIgnore.Clear();
		SeinChargeJump.State currentState = this.CurrentState;
		if (currentState != SeinChargeJump.State.Normal)
		{
			if (currentState != SeinChargeJump.State.Jumping)
			{
			}
		}
	}

	// Token: 0x06000FFC RID: 4092 RVA: 0x00060848 File Offset: 0x0005EA48
	public void UpdateState()
	{
		SeinChargeJump.State currentState = this.CurrentState;
		if (currentState != SeinChargeJump.State.Normal)
		{
			if (currentState == SeinChargeJump.State.Jumping)
			{
				if (this.m_stateCurrentTime > this.JumpDuration)
				{
					this.ChangeState(SeinChargeJump.State.Normal);
				}
				for (int i = 0; i < Targets.Attackables.Count; i++)
				{
					IAttackable attackable = Targets.Attackables[i];
					if (!InstantiateUtility.IsDestroyed(attackable as Component))
					{
						if (!this.m_attackablesIgnore.Contains(attackable))
						{
							if (attackable.CanBeStomped())
							{
								Vector3 vector = attackable.Position - this.Sein.PlatformBehaviour.PlatformMovement.HeadPosition;
								float magnitude = vector.magnitude;
								if (magnitude < 3f && Vector2.Dot(vector.normalized, this.PlatformMovement.LocalSpeed.normalized) > 0f)
								{
									this.m_attackablesIgnore.Add(attackable);
									Damage damage = new Damage((float)this.Damage, this.PlatformMovement.WorldSpeed.normalized * 3f, this.Sein.Position, DamageType.Stomp, base.gameObject);
									damage.DealToComponents(((Component)attackable).gameObject);
									if (attackable.IsDead() && attackable is IStompAttackable && ((IStompAttackable)attackable).CountsTowardsSuperJumpAchievement())
									{
										AchievementsLogic.Instance.OnSuperJumpedThroughEnemy();
									}
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
			}
		}
		this.m_stateCurrentTime += Time.deltaTime;
	}

	// Token: 0x1700028F RID: 655
	// (get) Token: 0x06000FFD RID: 4093 RVA: 0x0000DF1D File Offset: 0x0000C11D
	public bool CanChargeJump
	{
		get
		{
			return this.Sein.Abilities.ChargeJumpCharging.IsCharged && this.PlatformMovement.IsOnGround;
		}
	}

	// Token: 0x06000FFE RID: 4094 RVA: 0x00060A40 File Offset: 0x0005EC40
	public void PerformChargeJump()
	{
		float chargedJumpStrength = this.ChargedJumpStrength + this.ChargedJumpStrength * 0.08f * (float)(RandomizerBonus.Velocity() + RandomizerBonus.Jumpgrades());
		this.PlatformMovement.LocalSpeedY = chargedJumpStrength;
		this.OnJumpEvent(chargedJumpStrength);
		Sound.Play(this.JumpSound.GetSound(null), this.Sein.PlatformBehaviour.PlatformMovement.Position, null);
		this.UpwardsDeceleration.Deceleration = this.Deceleration;
		this.Sein.Mortality.DamageReciever.MakeInvincibleToEnemies(this.JumpDuration);
		this.ChangeState(SeinChargeJump.State.Jumping);
		this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.JumpAnimation, 10, new Func<bool>(this.ShouldChargeJumpAnimationKeepPlaying));
		this.Sein.PlatformBehaviour.Visuals.SpriteRotater.BeginTiltLeftRightInAir(1.5f);
		if (this.Sein.PlatformBehaviour.JumpSustain)
		{
			this.Sein.PlatformBehaviour.JumpSustain.SetAmountOfSpeedToLose(this.PlatformMovement.LocalSpeedY, 1f);
		}
		this.Sein.Abilities.ChargeJumpCharging.EndCharge();
		JumpFlipPlatform.OnSeinChargeJumpEvent();
	}

	// Token: 0x06000FFF RID: 4095 RVA: 0x0000DF47 File Offset: 0x0000C147
	public bool ShouldChargeJumpAnimationKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnWall && !this.PlatformMovement.IsOnCeiling;
	}

	// Token: 0x06001000 RID: 4096 RVA: 0x0000DF7A File Offset: 0x0000C17A
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.ChargeJump = this;
	}

	// Token: 0x06001001 RID: 4097 RVA: 0x0000DF94 File Offset: 0x0000C194
	public override void Serialize(Archive ar)
	{
		base.Serialize(ar);
		ar.Serialize(ref this.m_superJumpedEnemies);
	}

	// Token: 0x04000F3B RID: 3899
	public SeinCharacter Sein;

	// Token: 0x04000F3C RID: 3900
	public TextureAnimationWithTransitions JumpAnimation;

	// Token: 0x04000F3D RID: 3901
	public SoundProvider JumpSound;

	// Token: 0x04000F3E RID: 3902
	public float JumpDuration = 0.5f;

	// Token: 0x04000F3F RID: 3903
	public SeinChargeJump.State CurrentState;

	// Token: 0x04000F40 RID: 3904
	public float m_stateCurrentTime;

	// Token: 0x04000F41 RID: 3905
	public HashSet<IAttackable> m_attackablesIgnore = new HashSet<IAttackable>();

	// Token: 0x04000F42 RID: 3906
	public GameObject ExplosionEffect;

	// Token: 0x04000F43 RID: 3907
	public int Damage = 50;

	// Token: 0x04000F44 RID: 3908
	public float ChargingTime;

	// Token: 0x04000F45 RID: 3909
	public float ChargedJumpStrength;

	// Token: 0x04000F46 RID: 3910
	public float Deceleration = 20f;

	// Token: 0x04000F47 RID: 3911
	public int m_superJumpedEnemies;

	// Token: 0x02000311 RID: 785
	public enum State
	{
		// Token: 0x04000F4B RID: 3915
		Normal,
		// Token: 0x04000F4C RID: 3916
		Jumping
	}
}
