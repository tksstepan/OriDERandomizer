using System;
using fsm;
using fsm.triggers;
using UnityEngine;

// Token: 0x020004CD RID: 1229
public class SpitterEnemy : GroundEnemy
{
	// Token: 0x1700043B RID: 1083
	// (get) Token: 0x06001A9C RID: 6812 RVA: 0x0001759F File Offset: 0x0001579F
	// (set) Token: 0x06001A9D RID: 6813 RVA: 0x000175A7 File Offset: 0x000157A7
	public Vector2 ThrownDirection { get; set; }

	// Token: 0x06001A9E RID: 6814 RVA: 0x000170C8 File Offset: 0x000152C8
	public override void Awake()
	{
		base.Awake();
	}

	// Token: 0x06001A9F RID: 6815 RVA: 0x0008408C File Offset: 0x0008228C
	public override bool CanBeOptimized()
	{
		IState currentState = this.Controller.StateMachine.CurrentState;
		return currentState == this.State.Idle || currentState == this.State.Walk;
	}

	// Token: 0x06001AA0 RID: 6816 RVA: 0x000840CC File Offset: 0x000822CC
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

	// Token: 0x06001AA1 RID: 6817 RVA: 0x00084150 File Offset: 0x00082350
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

	// Token: 0x06001AA2 RID: 6818 RVA: 0x0001625E File Offset: 0x0001445E
	public bool IsOnGround()
	{
		return this.PlatformMovement.IsOnGround;
	}

	// Token: 0x06001AA3 RID: 6819 RVA: 0x000175B0 File Offset: 0x000157B0
	public bool HasHitWall()
	{
		return this.PlatformMovement.IsOnWall;
	}

	// Token: 0x06001AA4 RID: 6820 RVA: 0x000175BD File Offset: 0x000157BD
	public void TurnAround()
	{
		base.FaceLeft = !base.FaceLeft;
	}

	// Token: 0x06001AA5 RID: 6821 RVA: 0x00084898 File Offset: 0x00082A98
	public bool CanSeePlayer()
	{
		return this.Controller.NearSein && base.PositionToPlayerPosition.magnitude < this.Settings.SeePlayerDistance;
	}

	// Token: 0x06001AA6 RID: 6822 RVA: 0x000848D4 File Offset: 0x00082AD4
	public bool FurtherThanMinChargeDistance()
	{
		return base.PositionToPlayerPosition.magnitude > this.Settings.MinChargeDistance;
	}

	// Token: 0x06001AA7 RID: 6823 RVA: 0x000848FC File Offset: 0x00082AFC
	public bool CloserThanMinChargeDistance()
	{
		return base.PositionToPlayerPosition.magnitude < this.Settings.MinChargeDistance;
	}

	// Token: 0x06001AA8 RID: 6824
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

	// Token: 0x06001AA9 RID: 6825 RVA: 0x00084A50 File Offset: 0x00082C50
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

	// Token: 0x06001AAA RID: 6826 RVA: 0x00079B78 File Offset: 0x00077D78
	public bool ShouldThrow()
	{
		OnReceiveDamage onReceiveDamage = (OnReceiveDamage)this.Controller.StateMachine.CurrentTrigger;
		return onReceiveDamage.Damage.Type == DamageType.Bash;
	}

	// Token: 0x06001AAB RID: 6827 RVA: 0x0007E6F4 File Offset: 0x0007C8F4
	public bool ShouldStomped()
	{
		OnReceiveDamage onReceiveDamage = (OnReceiveDamage)this.Controller.StateMachine.CurrentTrigger;
		return onReceiveDamage.Damage.Type == DamageType.StompBlast;
	}

	// Token: 0x040017B5 RID: 6069
	public PrefabSpawner SpitEffect;

	// Token: 0x040017B6 RID: 6070
	public PrefabSpawner ProjectileSpawner;

	// Token: 0x040017B7 RID: 6071
	public ChargingSootEnemyAnimations Animations;

	// Token: 0x040017B8 RID: 6072
	public ChargingSootEnemySettings Settings;

	// Token: 0x040017B9 RID: 6073
	public SoundSource IdleSound;

	// Token: 0x040017BA RID: 6074
	public SoundSource WalkSound;

	// Token: 0x040017BB RID: 6075
	public SoundSource RunAwaySound;

	// Token: 0x040017BC RID: 6076
	public SoundSource AttackSound;

	// Token: 0x040017BD RID: 6077
	public SoundSource LandSound;

	// Token: 0x040017BE RID: 6078
	public ActionMethod EnterZoneAction;

	// Token: 0x040017BF RID: 6079
	public Transform[] ActionZones;

	// Token: 0x040017C0 RID: 6080
	private bool m_hasEnteredZone;

	// Token: 0x040017C1 RID: 6081
	public SpitterEnemy.States State = new SpitterEnemy.States();

	// Token: 0x020004CE RID: 1230
	public class States
	{
		// Token: 0x040017C3 RID: 6083
		public SpitterEnemyIdleState Idle;

		// Token: 0x040017C4 RID: 6084
		public SpitterEnemyWalkState Walk;

		// Token: 0x040017C5 RID: 6085
		public SpitterEnemyRunBackState RunBack;

		// Token: 0x040017C6 RID: 6086
		public SpitterEnemyChargingState SpitterEnemyCharging;

		// Token: 0x040017C7 RID: 6087
		public SpitterEnemyShootingState Shooting;

		// Token: 0x040017C8 RID: 6088
		public SpitterEnemyThrownState Thrown;

		// Token: 0x040017C9 RID: 6089
		public SpitterEnemyStompedState Stomped;

		// Token: 0x040017CA RID: 6090
		public SpitterEnemyStunnedState Stunned;
	}
}
