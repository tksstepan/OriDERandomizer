using System;
using fsm;
using fsm.triggers;
using Game;
using UnityEngine;

public class JumperEnemy : GroundEnemy
{
	public override bool CanBeOptimized()
	{
		return this.Controller.StateMachine.CurrentState == this.State.Idle;
	}

	public void ForceAttackPlayer()
	{
		this.Controller.StateMachine.ChangeState(this.State.JumpCharge);
	}

	public new void Start()
	{
		base.Start();
		this.State.Idle = new JumperEnemyIdleState(this);
		this.State.JumpCharge = new JumperEnemyChargingState(this);
		this.State.Fall = new JumperEnemyFallState(this);
		this.State.Thrown = new JumperEnemyThrownState(this);
		this.State.Stomped = new JumperEnemyStompedState(this);
		this.State.Stunned = new JumperEnemyStunnedState(this);
		this.State.Respawn = new State();
		this.State.Respawn.OnEnterEvent = delegate()
		{
			base.PlayAnimationOnce(this.Animations.Respawn, 0);
			base.FacePlayer();
			base.SpawnPrefab(this.Settings.RespawnEffect);
		};
		this.Controller.StateMachine.Configure(this.State.Idle).AddTransition<OnFixedUpdate>(this.State.JumpCharge, new Func<bool>(this.PlayerInRange), null).AddTransition<OnFixedUpdate>(this.State.JumpCharge, new Func<bool>(this.OutOfJumpingZone), null).AddTransition<AttackTriggered>(this.State.JumpCharge, null, null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.OnStomped)).AddTransition<OnReceiveDamage>(this.State.JumpCharge, null, null);
		this.Controller.StateMachine.Configure(this.State.JumpCharge).AddTransition<OnFixedUpdate>(this.State.Fall, () => base.AfterTime(this.Settings.ChargingDuration), new Action(this.DoJump)).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.OnStomped));
		this.Controller.StateMachine.Configure(this.State.Fall).AddTransition<OnFixedUpdate>(this.State.JumpCharge, () => this.LandedOnGround() && this.PlayerInRange(), new Action(this.OnLanded)).AddTransition<OnFixedUpdate>(this.State.Idle, () => this.LandedOnGround() && !this.PlayerInRange(), new Action(this.OnLanded)).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.OnStomped));
		this.Controller.StateMachine.Configure(this.State.Thrown).AddTransition<OnFixedUpdate>(this.State.Stunned, new Func<bool>(this.IsOnGround), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.OnStomped));
		this.Controller.StateMachine.Configure(this.State.Stomped).AddTransition<OnFixedUpdate>(this.State.Stunned, new Func<bool>(this.IsOnGround), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.OnStomped));
		this.Controller.StateMachine.Configure(this.State.Stunned).AddTransition<OnFixedUpdate>(this.State.Idle, () => base.AfterTime(this.Settings.StunnedDuration), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.OnStomped));
		this.Controller.StateMachine.Configure(this.State.Respawn).AddTransition<OnAnimationEnded>(this.State.Idle, null, null);
		this.Controller.StateMachine.RegisterStates(new IState[]
		{
			this.State.Idle,
			this.State.JumpCharge,
			this.State.Fall,
			this.State.Stunned,
			this.State.Thrown,
			this.State.Stomped,
			this.State.Respawn
		});
		if (this.m_timedRespawn)
		{
			this.Controller.StateMachine.ChangeState(this.State.Respawn);
			this.m_timedRespawn = false;
		}
		else
		{
			this.Controller.StateMachine.ChangeState(this.State.Idle);
		}
	}

	public new void OnTimedRespawn()
	{
		this.m_timedRespawn = true;
	}

	public bool OutOfJumpingZone()
	{
		return !(this.JumpingZone == null) && !new Rect
		{
			width = this.JumpingZone.lossyScale.x,
			height = this.JumpingZone.lossyScale.y,
			center = this.JumpingZone.position
		}.Contains(base.Position);
	}

	public bool IsOnGround()
	{
		return this.PlatformMovement.IsOnGround;
	}

	public bool ShouldThrow()
	{
		OnReceiveDamage onReceiveDamage = (OnReceiveDamage)this.Controller.StateMachine.CurrentTrigger;
		return onReceiveDamage.Damage.Type == DamageType.Bash;
	}

	public bool ShouldStomped()
	{
		OnReceiveDamage onReceiveDamage = (OnReceiveDamage)this.Controller.StateMachine.CurrentTrigger;
		return onReceiveDamage.Damage.Type == DamageType.StompBlast;
	}

	public void OnThrow()
	{
		OnReceiveDamage onReceiveDamage = (OnReceiveDamage)this.Controller.StateMachine.CurrentTrigger;
		this.PlatformMovement.WorldSpeed = onReceiveDamage.Damage.Force * 10f;
		if (this.PlatformMovement.IsOnGround && this.PlatformMovement.LocalSpeedY < 0f)
		{
			this.PlatformMovement.LocalSpeedY *= -0.5f;
		}
		this.m_thrownDirection = onReceiveDamage.Damage.Force.normalized;
		base.FaceLeft = (this.PlatformMovement.LocalSpeedX < 0f);
	}

	public void OnStomped()
	{
		OnReceiveDamage onReceiveDamage = (OnReceiveDamage)this.Controller.StateMachine.CurrentTrigger;
		this.PlatformMovement.WorldSpeed = onReceiveDamage.Damage.Force * 8f;
		if (this.PlatformMovement.IsOnGround && this.PlatformMovement.LocalSpeedY < 0f)
		{
			this.PlatformMovement.LocalSpeedY *= -0.5f;
		}
		this.m_thrownDirection = onReceiveDamage.Damage.Force.normalized;
		base.FaceLeft = (this.PlatformMovement.LocalSpeedX < 0f);
	}

	public void DoJump()
	{
		Vector2 localSpeed;
		localSpeed.y = PhysicsHelper.CalculateSpeedFromHeight(this.Settings.JumpHeight, this.Settings.Gravity);
		float num = 2f * localSpeed.y / this.Settings.Gravity;
		localSpeed.x = this.Settings.JumpDistance / num;
		if (this.OutOfJumpingZone())
		{
			localSpeed.x = Mathf.Clamp((this.JumpingZone.position.x - base.Position.x) / num, -localSpeed.x, localSpeed.x);
			this.m_shouldStomp = false;
		}
		else
		{
			localSpeed.x = Mathf.Clamp(base.PositionToPlayerPosition.x / num, -localSpeed.x, localSpeed.x);
			this.m_shouldStomp = (Mathf.Abs((base.PlayerPosition + this.m_playerSmoothSpeed - base.Position).x) < this.Settings.StompAttackDistance);
		}
		Vector3 vector = new Vector3(localSpeed.x * num * 0.5f, this.Settings.JumpHeight);
		bool flag = Mathf.Sign(localSpeed.x) != (float)base.FaceLeftSign;
		if (Physics.Raycast(new Ray(base.Position, vector.normalized), vector.magnitude, this.RaycastLayerMask) || !this.m_shouldStomp)
		{
			localSpeed.y = PhysicsHelper.CalculateSpeedFromHeight(this.Settings.ShortJumpHeight, this.Settings.Gravity);
			this.Animation.Play((!flag) ? this.Animations.ShortJump : this.Animations.JumpFlip, 1, () => !this.PlatformMovement.IsOnGround);
			this.m_shouldStomp = false;
		}
		else
		{
			this.Animation.Play((!flag) ? this.Animations.Jump : this.Animations.JumpFlip, 1, () => !this.PlatformMovement.IsOnGround);
		}
		base.PlaySound(this.Sounds.Jump);
		if (flag)
		{
			base.FaceLeft = !base.FaceLeft;
		}
		this.PlatformMovement.LocalSpeed = localSpeed;
	}

	public bool PlayerInRange()
	{
		return base.PositionToPlayerPosition.magnitude < this.Settings.ChargeRange && this.Controller.NearSein;
	}

	public new void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.IsSuspended)
		{
			return;
		}
		this.PlatformMovement.LocalSpeedY -= RandomizerBonusSkill.TimeScale(Time.deltaTime) * this.Settings.Gravity;
		if (this.PlatformMovement.LocalSpeedY < -this.Settings.MaxFallSpeed)
		{
			this.PlatformMovement.LocalSpeedY = -this.Settings.MaxFallSpeed;
		}
		if (this.PlatformMovement.IsOnCeiling)
		{
			this.PlatformMovement.LocalSpeedY = Mathf.Min(0f, this.PlatformMovement.LocalSpeedY);
		}
		this.UpdateRotation();
		if (Characters.Sein)
		{
			this.m_playerSmoothSpeed = Vector3.Lerp(this.m_playerSmoothSpeed, Characters.Sein.Speed, 0.1f);
		}
		if (base.IsInWater)
		{
			base.Drown();
		}
	}

	public void UpdateRotation()
	{
		IState currentState = this.Controller.StateMachine.CurrentState;
		float currentStateTime = this.Controller.StateMachine.CurrentStateTime;
		if (currentState == this.State.Thrown)
		{
			float num = 1f - Mathf.InverseLerp(0.3f, 0.6f, currentStateTime);
			this.FeetTransform.eulerAngles = new Vector3(0f, 0f, (MoonMath.Angle.AngleFromVector(this.m_thrownDirection) - 90f) * num);
		}
		else
		{
			float b = (!this.PlatformMovement.IsOnGround) ? 0f : this.PlatformMovement.GroundAngle;
			this.FeetTransform.eulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(this.FeetTransform.eulerAngles.z, b, 0.2f));
		}
	}

	public bool LandedOnGround()
	{
		return this.PlatformMovement.IsOnGround && this.PlatformMovement.LocalSpeedY <= 0f;
	}

	public void OnLanded()
	{
		if (this.m_shouldStomp && this.Settings.HasStompExplosion)
		{
			if (this.StompEffect)
			{
				GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.StompEffect, base.Position, Quaternion.identity);
				gameObject.GetComponentInChildren<DamageDealer>().Damage = this.Settings.ExplosionDamage;
			}
			base.PlaySound(this.Sounds.Impact);
		}
		else
		{
			if (this.LandEffect)
			{
				InstantiateUtility.Instantiate(this.LandEffect, base.Position, Quaternion.identity);
			}
			base.PlaySound(this.Sounds.Impact);
		}
		Collider groundCollider = this.PlatformMovementListOfColliders.GroundCollider;
		Damage damage = new Damage((float)this.Settings.GroundStompDamage, Vector3.down * 3f, base.transform.position, DamageType.Stomp, base.gameObject);
		if (groundCollider)
		{
			damage.DealToComponents(groundCollider.gameObject);
		}
		this.PlatformMovement.LocalSpeed = Vector3.zero;
	}

	public JumpingSootEnemyAnimations Animations;

	public JumpingSootEnemySettings Settings;

	public JumpingSootEnemySounds Sounds;

	public JumperEnemy.States State = new JumperEnemy.States();

	public Transform JumpingZone;

	public LayerMask RaycastLayerMask;

	private Vector3 m_playerSmoothSpeed;

	private bool m_shouldStomp;

	private Vector3 m_thrownDirection;

	private bool m_timedRespawn;

	public GameObject StompEffect;

	public GameObject LandEffect;

	public class States
	{
		public State Respawn;

		public JumperEnemyIdleState Idle;

		public JumperEnemyChargingState JumpCharge;

		public JumperEnemyFallState Fall;

		public JumperEnemyThrownState Thrown;

		public JumperEnemyStompedState Stomped;

		public JumperEnemyStunnedState Stunned;
	}
}
