using System;
using System.Collections.Generic;
using Core;
using fsm;
using Game;
using UnityEngine;

// Token: 0x02000338 RID: 824
public class SeinStomp : CharacterState, ISeinReceiver
{
	// Token: 0x06001206 RID: 4614 RVA: 0x00068A40 File Offset: 0x00066C40
	public SeinStomp()
	{
	}

	// Token: 0x06001207 RID: 4615 RVA: 0x00068A98 File Offset: 0x00066C98
	static SeinStomp()
	{
		// Note: this type is marked as 'beforefieldinit'.
		SeinStomp.OnStompIdleEvent = delegate
		{
		};
		SeinStomp.OnStompLandEvent = delegate
		{
		};
		SeinStomp.OnStompDownEvent = delegate
		{
		};
	}

	// Token: 0x14000018 RID: 24
	// (add) Token: 0x06001208 RID: 4616 RVA: 0x0000FD8F File Offset: 0x0000DF8F
	// (remove) Token: 0x06001209 RID: 4617 RVA: 0x0000FDA6 File Offset: 0x0000DFA6
	public static event Action OnStompIdleEvent;

	// Token: 0x14000019 RID: 25
	// (add) Token: 0x0600120A RID: 4618 RVA: 0x0000FDBD File Offset: 0x0000DFBD
	// (remove) Token: 0x0600120B RID: 4619 RVA: 0x0000FDD4 File Offset: 0x0000DFD4
	public static event Action OnStompLandEvent;

	// Token: 0x1400001A RID: 26
	// (add) Token: 0x0600120C RID: 4620 RVA: 0x0000FDEB File Offset: 0x0000DFEB
	// (remove) Token: 0x0600120D RID: 4621 RVA: 0x0000FE02 File Offset: 0x0000E002
	public static event Action OnStompDownEvent;

	// Token: 0x17000314 RID: 788
	// (get) Token: 0x0600120E RID: 4622 RVA: 0x0000FE19 File Offset: 0x0000E019
	public CharacterLeftRightMovement LeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	// Token: 0x17000315 RID: 789
	// (get) Token: 0x0600120F RID: 4623 RVA: 0x0000FE2B File Offset: 0x0000E02B
	public SeinDoubleJump DoubleJump
	{
		get
		{
			return this.Sein.Abilities.DoubleJump;
		}
	}

	// Token: 0x17000316 RID: 790
	// (get) Token: 0x06001210 RID: 4624 RVA: 0x0000FE3D File Offset: 0x0000E03D
	public CharacterUpwardsDeceleration UpwardsDeceleration
	{
		get
		{
			return this.Sein.PlatformBehaviour.UpwardsDeceleration;
		}
	}

	// Token: 0x17000317 RID: 791
	// (get) Token: 0x06001211 RID: 4625 RVA: 0x0000FE4F File Offset: 0x0000E04F
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x17000318 RID: 792
	// (get) Token: 0x06001212 RID: 4626 RVA: 0x0000FE61 File Offset: 0x0000E061
	public bool Finished
	{
		get
		{
			return this.Logic.CurrentState == this.State.Inactive;
		}
	}

	// Token: 0x17000319 RID: 793
	// (get) Token: 0x06001213 RID: 4627 RVA: 0x0000FE7B File Offset: 0x0000E07B
	public bool IsStomping
	{
		get
		{
			return this.Logic.CurrentState != this.State.Inactive;
		}
	}

	// Token: 0x06001214 RID: 4628 RVA: 0x0000FE98 File Offset: 0x0000E098
	public void OnRestoreCheckpoint()
	{
		this.Logic.ChangeState(this.State.Inactive);
	}

	// Token: 0x06001215 RID: 4629 RVA: 0x0000FEB0 File Offset: 0x0000E0B0
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.Stomp = this;
	}

	// Token: 0x06001216 RID: 4630 RVA: 0x0000FECA File Offset: 0x0000E0CA
	public void Start()
	{
		this.Sein.PlatformBehaviour.Gravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyVerticalPlatformMovementSettings;
		this.PlatformMovement.OnCollisionGroundEvent += this.OnCollisionGround;
	}

	// Token: 0x06001217 RID: 4631 RVA: 0x0000FE98 File Offset: 0x0000E098
	public override void OnExit()
	{
		this.Logic.ChangeState(this.State.Inactive);
	}

	// Token: 0x06001218 RID: 4632 RVA: 0x0000FF04 File Offset: 0x0000E104
	public override void UpdateCharacterState()
	{
		this.Logic.UpdateState(Time.deltaTime);
	}

	// Token: 0x06001219 RID: 4633 RVA: 0x0000FF16 File Offset: 0x0000E116
	public void ModifyVerticalPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (this.Logic.CurrentState != this.State.Inactive)
		{
			settings.GravityStrength = 0f;
		}
	}

	// Token: 0x1700031A RID: 794
	// (get) Token: 0x0600121A RID: 4634 RVA: 0x0000FF3E File Offset: 0x0000E13E
	public float StompDamage
	{
		get
		{
			if (this.Sein.PlayerAbilities.StompUpgrade.HasAbility)
			{
				return RandomizerBonusSkill.AbilityDamage(this.UpgradedDamage);
			}
			return RandomizerBonusSkill.AbilityDamage(this.Damage);
		}
	}

	// Token: 0x0600121B RID: 4635 RVA: 0x00068B0C File Offset: 0x00066D0C
	public void OnCollisionGround(Vector3 normal, Collider collider)
	{
		if (this.Logic.CurrentState == this.State.StompDown)
		{
			this.LandStomp();
			if (!this.Sein.Controller.IsSwimming)
			{
				IAttackable attackable = collider.gameObject.FindComponent<IAttackable>();
				if (attackable != null && attackable.CanBeStomped())
				{
					Damage damage = new Damage(this.StompDamage, Vector3.down * 3f, Characters.Sein.Position, DamageType.Stomp, base.gameObject);
					damage.DealToComponents(collider.gameObject);
				}
				this.DoBlastRadius(attackable);
			}
			this.Logic.ChangeState(this.State.StompFinished);
		}
	}

	// Token: 0x0600121C RID: 4636 RVA: 0x00068BC8 File Offset: 0x00066DC8
	public void DoBlastRadius(IAttackable landedStompAttackable)
	{
		this.m_stompBlastAttackables.Clear();
		this.m_stompBlastAttackables.AddRange(Targets.Attackables);
		for (int i = 0; i < this.m_stompBlastAttackables.Count; i++)
		{
			IAttackable attackable = this.m_stompBlastAttackables[i];
			if (!InstantiateUtility.IsDestroyed(attackable as Component))
			{
				if (attackable != landedStompAttackable)
				{
					if (attackable.CanBeStomped())
					{
						Vector3 vector = attackable.Position - this.Sein.Position;
						float magnitude = vector.magnitude;
						if (magnitude < this.StompBlashRadius)
						{
							Vector3 normalized = (vector.normalized + Vector3.up * 2f).normalized;
							GameObject gameObject = ((Component)attackable).gameObject;
							float stompDamage = this.StompDamage;
							Damage damage = new Damage(stompDamage, normalized * 3f, attackable.Position, DamageType.StompBlast, gameObject);
							damage.DealToComponents(gameObject);
						}
					}
				}
			}
		}
		this.m_stompBlastAttackables.Clear();
	}

	// Token: 0x0600121D RID: 4637 RVA: 0x00068CE4 File Offset: 0x00066EE4
	public override void Awake()
	{
		this.State.Inactive = new State
		{
			OnEnterEvent = new Action(this.OnEnterInactive),
			UpdateStateEvent = new Action(this.UpdateStompInactiveState)
		};
		this.State.StompDown = new State
		{
			OnEnterEvent = new Action(this.OnEnterStompDownState),
			UpdateStateEvent = new Action(this.UpdateStompDownState)
		};
		this.State.StompIdle = new State
		{
			OnEnterEvent = new Action(this.OnEnterStompIdleState),
			UpdateStateEvent = new Action(this.UpdateStompIdleState)
		};
		this.State.StompFinished = new State
		{
			UpdateStateEvent = new Action(this.UpdateStompFinishedState)
		};
		this.Logic.RegisterStates(new IState[]
		{
			this.State.Inactive,
			this.State.StompDown,
			this.State.StompIdle
		});
		this.Logic.ChangeState(this.State.Inactive);
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x0600121E RID: 4638 RVA: 0x00068E20 File Offset: 0x00067020
	public override void OnDestroy()
	{
		this.Sein.PlatformBehaviour.Gravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyVerticalPlatformMovementSettings;
		this.PlatformMovement.OnCollisionGroundEvent -= this.OnCollisionGround;
		this.Logic.ChangeState(this.State.Inactive);
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
		base.OnDestroy();
	}

	// Token: 0x0600121F RID: 4639 RVA: 0x00068E98 File Offset: 0x00067098
	public void OnEnterStompIdleState()
	{
		SeinStomp.OnStompIdleEvent();
		if (!this.Sein.PlayerAbilities.StompUpgrade.HasAbility)
		{
			this.StompStartSound.Play();
		}
		else
		{
			this.StompStartSoundUpgraded.Play();
		}
		this.Sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.StompIdleAnimation, 111, new Func<bool>(this.ShouldStompAnimationKeepPlaying), false);
	}

	// Token: 0x06001220 RID: 4640 RVA: 0x00068F14 File Offset: 0x00067114
	public void OnEnterStompDownState()
	{
		SeinStomp.OnStompDownEvent();
		this.PlatformMovement.LocalSpeedX *= 0.5f;
		if (!this.Sein.PlayerAbilities.StompUpgrade.HasAbility)
		{
			this.StompFallSound.Play();
		}
		else
		{
			this.StompFallSoundUpgraded.Play();
		}
		this.Sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.StompDownAnimation, 111, new Func<bool>(this.ShouldStompAnimationKeepPlaying), false);
	}

	// Token: 0x06001221 RID: 4641 RVA: 0x0000FF67 File Offset: 0x0000E167
	public void UpdateStompFinishedState()
	{
		if (this.Logic.CurrentStateTime > 0.05f)
		{
			this.EndStomp();
		}
	}

	// Token: 0x06001222 RID: 4642 RVA: 0x00068FA8 File Offset: 0x000671A8
	public void LandStomp()
	{
		this.PlatformMovement.LocalSpeedX = 0f;
		this.PlatformMovement.LocalSpeedY = 0f;
		SeinStomp.OnStompLandEvent();
		if (!this.Sein.PlayerAbilities.StompUpgrade.HasAbility)
		{
			this.StompLandSound.Play();
		}
		else
		{
			this.StompLandSoundUpgraded.Play();
		}
		this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.StompLandAnimation, 111, new Func<bool>(this.ShouldStompLandAnimationKeepPlaying));
		if (this.Sein.Controller.IsSwimming)
		{
			return;
		}
		this.EndStomp();
		this.DoStompBlastEffect();
	}

	// Token: 0x06001223 RID: 4643 RVA: 0x00069068 File Offset: 0x00067268
	public void DoStompBlastEffect()
	{
		if (this.StompLandEffect != null)
		{
			if (this.Sein.PlayerAbilities.StompUpgrade.HasAbility)
			{
				InstantiateUtility.Instantiate(this.StompLandEffectUpgraded, this.Sein.PlatformBehaviour.PlatformMovement.FeetPosition, Quaternion.identity);
			}
			else
			{
				InstantiateUtility.Instantiate(this.StompLandEffect, this.Sein.PlatformBehaviour.PlatformMovement.FeetPosition, Quaternion.identity);
			}
		}
	}

	// Token: 0x06001224 RID: 4644 RVA: 0x000028E7 File Offset: 0x00000AE7
	public void OnEnterInactive()
	{
	}

	// Token: 0x06001225 RID: 4645 RVA: 0x000690F4 File Offset: 0x000672F4
	public void UpdateStompIdleState()
	{
		if (this.DoubleJump && this.DoubleJump.CanDoubleJump && Core.Input.Jump.OnPressed)
		{
			this.EndStomp();
			this.DoubleJump.PerformDoubleJump();
			return;
		}
		if (this.Logic.CurrentStateTime > this.IdleDuration)
		{
			this.Logic.ChangeState(this.State.StompDown);
		}
		this.PlatformMovement.LocalSpeedX = 0f;
		this.PlatformMovement.LocalSpeedY = 0f;
	}

	// Token: 0x06001226 RID: 4646 RVA: 0x00069190 File Offset: 0x00067390
	public void UpdateStompInactiveState()
	{
		if (this.Sein.Controller.IsSwimming)
		{
			return;
		}
		bool flag = Core.Input.Stomp.OnPressed & this.Sein.Input.NormalizedHorizontal == 0;
		bool flag2 = Core.Input.Stomp.OnPressed && Core.Input.DigiPadAxis.y < 0f;
		bool flag3 = flag || flag2;
		if (flag3 && !Core.Input.Stomp.Used && this.CanStomp())
		{
			this.Logic.ChangeState(this.State.StompIdle);
		}
	}

	// Token: 0x06001227 RID: 4647 RVA: 0x0006925C File Offset: 0x0006745C
	public bool CanStomp()
	{
		return this.Sein.Controller.CanMove && this.Sein.PlatformBehaviour.PlatformMovement.IsInAir && !this.Sein.Controller.IsGliding && !this.Sein.Controller.InputLocked && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities);
	}

	// Token: 0x06001228 RID: 4648 RVA: 0x000692D0 File Offset: 0x000674D0
	public void UpdateStompDownState()
	{
		if (Core.Input.Jump.OnPressed)
		{
			this.EndStomp();
			return;
		}
		if (this.Logic.CurrentStateTime > this.StompDownDuration && !Core.Input.Stomp.Pressed)
		{
			this.EndStomp();
			return;
		}
		this.PlatformMovement.LocalSpeed = new Vector2(0f, -( this.StompSpeed + this.StompSpeed * 0.2f * RandomizerBonus.Velocity()));
		this.Sein.Mortality.DamageReciever.MakeInvincibleToEnemies(0.2f);
		if (this.Sein.Controller.IsSwimming)
		{
			this.EndStomp();
		}
		for (int i = 0; i < Targets.Attackables.Count; i++)
		{
			IAttackable attackable = Targets.Attackables[i];
			if (!attackable.IsDead())
			{
				if (!InstantiateUtility.IsDestroyed(attackable as Component))
				{
					if (attackable.IsStompBouncable())
					{
						Vector3 a = Characters.Sein.Position + Vector3.down;
						if (Vector3.Distance(a, attackable.Position) < 1.5f && this.Logic.CurrentState == this.State.StompDown)
						{
							GameObject gameObject = ((Component)attackable).gameObject;
							Damage damage = new Damage(this.StompDamage, Vector3.down * 3f, Characters.Sein.Position, DamageType.Stomp, base.gameObject);
							damage.DealToComponents(gameObject);
							if (attackable.IsDead())
							{
								return;
							}
							this.EndStomp();
							this.PlatformMovement.LocalSpeedY = 17f;
							this.Sein.PlatformBehaviour.UpwardsDeceleration.Deceleration = 20f;
							this.Sein.Animation.Play(this.StompBounceAnimation, 111, null);
							this.Sein.ResetAirLimits();
							this.StompLandSound.Play();
							this.DoBlastRadius(attackable);
							this.DoStompBlastEffect();
							return;
						}
					}
				}
			}
		}
	}

	// Token: 0x06001229 RID: 4649 RVA: 0x0000FE98 File Offset: 0x0000E098
	public void EndStomp()
	{
		this.Logic.ChangeState(this.State.Inactive);
	}

	// Token: 0x0600122A RID: 4650 RVA: 0x0000FE7B File Offset: 0x0000E07B
	public bool ShouldStompAnimationKeepPlaying()
	{
		return this.Logic.CurrentState != this.State.Inactive;
	}

	// Token: 0x0600122B RID: 4651 RVA: 0x0000FF84 File Offset: 0x0000E184
	public bool ShouldStompLandAnimationKeepPlaying()
	{
		return this.PlatformMovement.LocalSpeedX == 0f && this.PlatformMovement.LocalSpeedY <= 0f;
	}

	// Token: 0x0600122C RID: 4652 RVA: 0x0000FFB3 File Offset: 0x0000E1B3
	public override void Serialize(Archive ar)
	{
		this.Logic.Serialize(ar);
		base.Serialize(ar);
	}

	// Token: 0x040010EB RID: 4331
	public float IdleDuration;

	// Token: 0x040010EC RID: 4332
	public StateMachine Logic = new StateMachine();

	// Token: 0x040010ED RID: 4333
	public SeinCharacter Sein;

	// Token: 0x040010EE RID: 4334
	public SeinStomp.States State = new SeinStomp.States();

	// Token: 0x040010EF RID: 4335
	public float StompBlashRadius = 10f;

	// Token: 0x040010F0 RID: 4336
	public float Damage = 15f;

	// Token: 0x040010F1 RID: 4337
	public float UpgradedDamage = 25f;

	// Token: 0x040010F2 RID: 4338
	public AnimationCurve StompBlastFalloutCurve;

	// Token: 0x040010F3 RID: 4339
	public TextureAnimationWithTransitions StompBounceAnimation;

	// Token: 0x040010F4 RID: 4340
	public TextureAnimationWithTransitions StompDownAnimation;

	// Token: 0x040010F5 RID: 4341
	public float StompDownDuration;

	// Token: 0x040010F6 RID: 4342
	public SoundSource StompFallSound;

	// Token: 0x040010F7 RID: 4343
	public SoundSource StompFallSoundUpgraded;

	// Token: 0x040010F8 RID: 4344
	public TextureAnimationWithTransitions StompIdleAnimation;

	// Token: 0x040010F9 RID: 4345
	public TextureAnimationWithTransitions StompLandAnimation;

	// Token: 0x040010FA RID: 4346
	public float StompLandDuration;

	// Token: 0x040010FB RID: 4347
	public GameObject StompLandEffect;

	// Token: 0x040010FC RID: 4348
	public GameObject StompLandEffectUpgraded;

	// Token: 0x040010FD RID: 4349
	public SoundSource StompLandSound;

	// Token: 0x040010FE RID: 4350
	public SoundSource StompLandSoundUpgraded;

	// Token: 0x040010FF RID: 4351
	public float StompSpeed;

	// Token: 0x04001100 RID: 4352
	public SoundSource StompStartSound;

	// Token: 0x04001101 RID: 4353
	public SoundSource StompStartSoundUpgraded;

	// Token: 0x04001102 RID: 4354
	public float UpwardDeceleration;

	// Token: 0x04001103 RID: 4355
	public List<IAttackable> m_stompBlastAttackables = new List<IAttackable>();

	// Token: 0x02000339 RID: 825
	public class States
	{
		// Token: 0x06001230 RID: 4656 RVA: 0x000024FF File Offset: 0x000006FF
		public States()
		{
		}

		// Token: 0x0400110A RID: 4362
		public IState Inactive;

		// Token: 0x0400110B RID: 4363
		public IState StompDown;

		// Token: 0x0400110C RID: 4364
		public IState StompIdle;

		// Token: 0x0400110D RID: 4365
		public IState StompFinished;
	}
}
