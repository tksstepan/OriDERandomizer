using System;
using System.Collections.Generic;
using Core;
using fsm;
using Game;
using UnityEngine;

public class SeinStomp : CharacterState, ISeinReceiver
{
	static SeinStomp()
	{
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

	public static event Action OnStompIdleEvent;

	public static event Action OnStompLandEvent;

	public static event Action OnStompDownEvent;

	public CharacterLeftRightMovement LeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	public SeinDoubleJump DoubleJump
	{
		get
		{
			return this.Sein.Abilities.DoubleJump;
		}
	}

	public CharacterUpwardsDeceleration UpwardsDeceleration
	{
		get
		{
			return this.Sein.PlatformBehaviour.UpwardsDeceleration;
		}
	}

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public bool Finished
	{
		get
		{
			return this.Logic.CurrentState == this.State.Inactive;
		}
	}

	public bool IsStomping
	{
		get
		{
			return this.Logic.CurrentState != this.State.Inactive;
		}
	}

	public void OnRestoreCheckpoint()
	{
		this.Logic.ChangeState(this.State.Inactive);
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
		this.Sein.Abilities.Stomp = this;
	}

	public void Start()
	{
		this.Sein.PlatformBehaviour.Gravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyVerticalPlatformMovementSettings;
		this.PlatformMovement.OnCollisionGroundEvent += this.OnCollisionGround;
	}

	public override void OnExit()
	{
		this.Logic.ChangeState(this.State.Inactive);
	}

	public override void UpdateCharacterState()
	{
		this.Logic.UpdateState(Time.deltaTime);
	}

	public void ModifyVerticalPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (this.Logic.CurrentState != this.State.Inactive)
		{
			settings.GravityStrength = 0f;
		}
	}

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

	public override void OnDestroy()
	{
		this.Sein.PlatformBehaviour.Gravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyVerticalPlatformMovementSettings;
		this.PlatformMovement.OnCollisionGroundEvent -= this.OnCollisionGround;
		this.Logic.ChangeState(this.State.Inactive);
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
		base.OnDestroy();
	}

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

	public void UpdateStompFinishedState()
	{
		if (this.Logic.CurrentStateTime > 0.05f)
		{
			this.EndStomp();
		}
	}

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

	public void OnEnterInactive()
	{
	}

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

	public bool CanStomp()
	{
		return this.Sein.Controller.CanMove && this.Sein.PlatformBehaviour.PlatformMovement.IsInAir && !this.Sein.Controller.IsGliding && !this.Sein.Controller.InputLocked && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities);
	}

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

	public void EndStomp()
	{
		this.Logic.ChangeState(this.State.Inactive);
	}

	public bool ShouldStompAnimationKeepPlaying()
	{
		return this.Logic.CurrentState != this.State.Inactive;
	}

	public bool ShouldStompLandAnimationKeepPlaying()
	{
		return this.PlatformMovement.LocalSpeedX == 0f && this.PlatformMovement.LocalSpeedY <= 0f;
	}

	public override void Serialize(Archive ar)
	{
		this.Logic.Serialize(ar);
		base.Serialize(ar);
	}

	public float IdleDuration;

	public StateMachine Logic = new StateMachine();

	public SeinCharacter Sein;

	public SeinStomp.States State = new SeinStomp.States();

	public float StompBlashRadius = 10f;

	public float Damage = 15f;

	public float UpgradedDamage = 25f;

	public AnimationCurve StompBlastFalloutCurve;

	public TextureAnimationWithTransitions StompBounceAnimation;

	public TextureAnimationWithTransitions StompDownAnimation;

	public float StompDownDuration;

	public SoundSource StompFallSound;

	public SoundSource StompFallSoundUpgraded;

	public TextureAnimationWithTransitions StompIdleAnimation;

	public TextureAnimationWithTransitions StompLandAnimation;

	public float StompLandDuration;

	public GameObject StompLandEffect;

	public GameObject StompLandEffectUpgraded;

	public SoundSource StompLandSound;

	public SoundSource StompLandSoundUpgraded;

	public float StompSpeed;

	public SoundSource StompStartSound;

	public SoundSource StompStartSoundUpgraded;

	public float UpwardDeceleration;

	public List<IAttackable> m_stompBlastAttackables = new List<IAttackable>();

	public class States
	{
		public IState Inactive;

		public IState StompDown;

		public IState StompIdle;

		public IState StompFinished;
	}
}
