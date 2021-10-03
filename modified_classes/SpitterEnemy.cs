using System;
using fsm;
using fsm.triggers;
using UnityEngine;

public class SpitterEnemy : GroundEnemy
{
	public Vector2 ThrownDirection { get; set; }

	public override void Awake()
	{
		base.Awake();
	}

	public override bool CanBeOptimized()
	{
		IState currentState = this.Controller.StateMachine.CurrentState;
		return currentState == this.State.Idle || currentState == this.State.Walk;
	}

	public bool WilhelmScreamZoneRectanglesContain(Vector2 position)
	{
		for (int i = 0; i < this.ActionZones.Length; i++)
		{
			Transform transform = this.ActionZones[i];
			Rect rect = default(Rect);
			rect.width = transform.lossyScale.x;
			rect.height = transform.lossyScale.y;
			rect.center = transform.position;
			if (rect.Contains(position))
			{
				return true;
			}
		}
		return false;
	}

	public new void Start()
	{
		base.Start();
		this.State.Idle = new SpitterEnemyIdleState(this);
		this.State.Walk = new SpitterEnemyWalkState(this);
		this.State.RunBack = new SpitterEnemyRunBackState(this);
		this.State.SpitterEnemyCharging = new SpitterEnemyChargingState(this);
		this.State.Shooting = new SpitterEnemyShootingState(this);
		this.State.Thrown = new SpitterEnemyThrownState(this);
		this.State.Stomped = new SpitterEnemyStompedState(this);
		this.State.Stunned = new SpitterEnemyStunnedState(this);
		this.Controller.StateMachine.RegisterStates(new IState[]
		{
			this.State.Idle,
			this.State.Walk,
			this.State.RunBack,
			this.State.SpitterEnemyCharging,
			this.State.Shooting,
			this.State.Stunned,
			this.State.Thrown,
			this.State.Stomped
		});
		this.Controller.StateMachine.Configure(this.State.Idle).AddTransition<AttackTriggered>(this.State.SpitterEnemyCharging, null, null).AddTransition<OnFixedUpdate>(this.State.Walk, () => base.AfterTime(this.Settings.IdleDuration) && base.IsOnScreen(), null).AddTransition<OnFixedUpdate>(this.State.RunBack, new Func<bool>(this.CanSeePlayer), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.State.Thrown.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.State.Stomped.OnStomped)).AddTransition<OnReceiveDamage>(this.State.SpitterEnemyCharging, null, null);
		this.Controller.StateMachine.Configure(this.State.Walk).AddTransition<AttackTriggered>(this.State.SpitterEnemyCharging, null, null).AddTransition<OnFixedUpdate>(this.State.Idle, () => base.AfterTime(this.Settings.WalkDuration) && base.IsOnScreen(), null).AddTransition<OnFixedUpdate>(this.State.Idle, () => !base.IsOnScreen(), null).AddTransition<OnFixedUpdate>(this.State.Idle, new Func<bool>(this.HasHitWall), new Action(this.TurnAround)).AddTransition<OnFixedUpdate>(this.State.RunBack, new Func<bool>(this.CanSeePlayer), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.State.Thrown.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.State.Stomped.OnStomped)).AddTransition<OnReceiveDamage>(this.State.SpitterEnemyCharging, null, null);
		this.Controller.StateMachine.Configure(this.State.RunBack).AddTransition<AttackTriggered>(this.State.SpitterEnemyCharging, null, null).AddTransition<OnFixedUpdate>(this.State.Idle, () => !this.CanSeePlayer(), null).AddTransition<OnFixedUpdate>(this.State.SpitterEnemyCharging, new Func<bool>(this.FurtherThanMinChargeDistance), null).AddTransition<OnFixedUpdate>(this.State.SpitterEnemyCharging, new Func<bool>(this.HasHitWall), new Action(this.TurnAround)).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.State.Thrown.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.State.Stomped.OnStomped)).AddTransition<OnReceiveDamage>(this.State.SpitterEnemyCharging, null, null);
		this.Controller.StateMachine.Configure(this.State.SpitterEnemyCharging).AddTransition<OnFixedUpdate>(this.State.Shooting, () => base.AfterTime(this.Settings.ChargeDuration), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.State.Thrown.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.State.Stomped.OnStomped));
		this.Controller.StateMachine.Configure(this.State.Shooting).AddTransition<OnFixedUpdate>(this.State.Idle, () => base.AfterTime(this.Settings.ShootingDuration) && !this.CanSeePlayer(), null).AddTransition<OnFixedUpdate>(this.State.RunBack, () => base.AfterTime(this.Settings.ShootingDuration) && this.CloserThanMinChargeDistance(), null).AddTransition<OnFixedUpdate>(this.State.SpitterEnemyCharging, () => base.AfterTime(this.Settings.ShootingDuration) && this.FurtherThanMinChargeDistance(), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.State.Thrown.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.State.Stomped.OnStomped));
		this.Controller.StateMachine.Configure(this.State.Thrown).AddTransition<OnFixedUpdate>(this.State.Stunned, new Func<bool>(this.IsOnGround), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.State.Thrown.OnThrow));
		this.Controller.StateMachine.Configure(this.State.Stomped).AddTransition<OnFixedUpdate>(this.State.Stunned, new Func<bool>(this.IsOnGround), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.State.Thrown.OnThrow)).AddTransition<OnReceiveDamage>(this.State.Stomped, new Func<bool>(this.ShouldStomped), new Action(this.State.Stomped.OnStomped));
		this.Controller.StateMachine.Configure(this.State.Stunned).AddTransition<OnFixedUpdate>(this.State.Idle, () => base.AfterTime(this.Settings.StunnedDuration) && !this.CanSeePlayer(), null).AddTransition<OnFixedUpdate>(this.State.RunBack, () => base.AfterTime(this.Settings.StunnedDuration) && this.CanSeePlayer(), null).AddTransition<OnReceiveDamage>(this.State.Thrown, new Func<bool>(this.ShouldThrow), new Action(this.State.Thrown.OnThrow));
		this.Controller.StateMachine.ChangeState(this.State.Idle);
	}

	public bool IsOnGround()
	{
		return this.PlatformMovement.IsOnGround;
	}

	public bool HasHitWall()
	{
		return this.PlatformMovement.IsOnWall;
	}

	public void TurnAround()
	{
		base.FaceLeft = !base.FaceLeft;
	}

	public bool CanSeePlayer()
	{
		return this.Controller.NearSein && base.PositionToPlayerPosition.magnitude < this.Settings.SeePlayerDistance;
	}

	public bool FurtherThanMinChargeDistance()
	{
		return base.PositionToPlayerPosition.magnitude > this.Settings.MinChargeDistance;
	}

	public bool CloserThanMinChargeDistance()
	{
		return base.PositionToPlayerPosition.magnitude < this.Settings.MinChargeDistance;
	}

	public new void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.IsSuspended)
		{
			return;
		}
		bool flag;
		if (this.PlatformMovement.MovingHorizontally && EnemyStopper.InsideEnemyStopper(base.Position, (!base.FaceLeft) ? Vector3.right : Vector3.left, out flag))
		{
			base.FaceLeft = !base.FaceLeft;
			if (this.Controller.StateMachine.CurrentState == this.State.RunBack)
			{
				this.Controller.StateMachine.ChangeState(this.State.SpitterEnemyCharging);
			}
		}
		if (!this.PlatformMovement.IsSuspended && this.PlatformMovement.IsInAir)
		{
			this.PlatformMovement.LocalSpeedY -= this.Settings.Gravity * RandomizerBonusSkill.TimeScale(Time.deltaTime);
		}
		this.UpdateRotation();
		if (base.IsInWater)
		{
			base.Drown();
		}
		if (this.WilhelmScreamZoneRectanglesContain(base.transform.position) && !this.m_hasEnteredZone && this.EnterZoneAction)
		{
			this.m_hasEnteredZone = true;
			BingoController.OnScream();
			this.EnterZoneAction.Perform(null);
		}
	}

	public void UpdateRotation()
	{
		IState currentState = this.Controller.StateMachine.CurrentState;
		float currentStateTime = this.Controller.StateMachine.CurrentStateTime;
		if (currentState == this.State.Thrown)
		{
			float num = 1f - Mathf.InverseLerp(0.3f, 0.6f, currentStateTime);
			this.FeetTransform.eulerAngles = new Vector3(0f, 0f, (MoonMath.Angle.AngleFromVector(this.ThrownDirection) - 90f) * num);
		}
		else
		{
			float b = (!this.PlatformMovement.IsOnGround) ? 0f : this.PlatformMovement.GroundAngle;
			this.FeetTransform.eulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(this.FeetTransform.eulerAngles.z, b, 0.2f));
		}
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

	public PrefabSpawner SpitEffect;

	public PrefabSpawner ProjectileSpawner;

	public ChargingSootEnemyAnimations Animations;

	public ChargingSootEnemySettings Settings;

	public SoundSource IdleSound;

	public SoundSource WalkSound;

	public SoundSource RunAwaySound;

	public SoundSource AttackSound;

	public SoundSource LandSound;

	public ActionMethod EnterZoneAction;

	public Transform[] ActionZones;

	private bool m_hasEnteredZone;

	public SpitterEnemy.States State = new SpitterEnemy.States();

	public class States
	{
		public SpitterEnemyIdleState Idle;

		public SpitterEnemyWalkState Walk;

		public SpitterEnemyRunBackState RunBack;

		public SpitterEnemyChargingState SpitterEnemyCharging;

		public SpitterEnemyShootingState Shooting;

		public SpitterEnemyThrownState Thrown;

		public SpitterEnemyStompedState Stomped;

		public SpitterEnemyStunnedState Stunned;
	}
}
