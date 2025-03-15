using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class SeinChargeJump : CharacterState, ISeinReceiver
{
	public event Action<float> OnJumpEvent = delegate
	{
	};

	public PlayerAbilities PlayerAbilities
	{
		get
		{
			return this.Sein.PlayerAbilities;
		}
	}

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public SeinChargeJump ChargeJump
	{
		get
		{
			return this.Sein.Abilities.ChargeJump;
		}
	}

	public CharacterUpwardsDeceleration UpwardsDeceleration
	{
		get
		{
			return this.Sein.PlatformBehaviour.UpwardsDeceleration;
		}
	}

	public void OnDoubleJump()
	{
		this.UpwardsDeceleration.Reset();
		this.ChangeState(SeinChargeJump.State.Normal);
	}

	public override void UpdateCharacterState()
	{
		if (this.Sein.IsSuspended)
		{
			return;
		}
		this.UpdateState();
	}

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

	public bool CanChargeJump
	{
		get
		{
			return this.Sein.Abilities.ChargeJumpCharging.IsCharged && this.PlatformMovement.IsOnGround;
		}
	}

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

	public bool ShouldChargeJumpAnimationKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && !this.PlatformMovement.IsOnWall && !this.PlatformMovement.IsOnCeiling;
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.ChargeJump = this;
	}

	public override void Serialize(Archive ar)
	{
		base.Serialize(ar);
		ar.Serialize(ref this.m_superJumpedEnemies);
	}

	public SeinCharacter Sein;

	public TextureAnimationWithTransitions JumpAnimation;

	public SoundProvider JumpSound;

	public float JumpDuration = 0.5f;

	public SeinChargeJump.State CurrentState;

	public float m_stateCurrentTime;

	public HashSet<IAttackable> m_attackablesIgnore = new HashSet<IAttackable>();

	public GameObject ExplosionEffect;

	public int Damage = 50;

	public float ChargingTime;

	public float ChargedJumpStrength;

	public float Deceleration = 20f;

	public int m_superJumpedEnemies;

	public enum State
	{
		Normal,
		Jumping
	}
}
