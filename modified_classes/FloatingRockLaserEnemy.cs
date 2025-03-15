using System;
using fsm;
using fsm.triggers;
using Game;
using UnityEngine;

public class FloatingRockLaserEnemy : Enemy
{
	public void PlayAnimationOnce(CharacterAnimationSystem animationSystem, TextureAnimationWithTransitions anim, int layer = 0)
	{
		if (anim && animationSystem)
		{
			animationSystem.Play(anim, layer, null);
		}
	}

	public void RestartAnimationLoop(CharacterAnimationSystem animationSystem, TextureAnimationWithTransitions anim, int layer = 0)
	{
		if (anim && animationSystem)
		{
			animationSystem.RestartLoop(anim, layer, null);
		}
	}

	public void PlayAnimationLoop(CharacterAnimationSystem animationSystem, TextureAnimationWithTransitions anim, int layer = 0)
	{
		if (anim && animationSystem)
		{
			animationSystem.PlayLoop(anim, layer, null, false);
		}
	}

	public new void Awake()
	{
		base.Awake();
		EntityDamageReciever damageReciever = this.DamageReciever;
		damageReciever.OnModifyDamage = (EntityDamageReciever.ModifyDamageDelegate)Delegate.Combine(damageReciever.OnModifyDamage, new EntityDamageReciever.ModifyDamageDelegate(this.OnModifyDamage));
	}

	public new void OnDestroy()
	{
		base.OnDestroy();
		EntityDamageReciever damageReciever = this.DamageReciever;
		damageReciever.OnModifyDamage = (EntityDamageReciever.ModifyDamageDelegate)Delegate.Remove(damageReciever.OnModifyDamage, new EntityDamageReciever.ModifyDamageDelegate(this.OnModifyDamage));
	}

	public virtual void OnModifyDamage(Damage damage)
	{
		if (damage.Type == DamageType.SpiritFlame)
		{
			damage.SetAmount(0f);
		}
	}

	public void OnEnterIdle()
	{
		this.RestartAnimationLoop(this.Animation, this.Animations.Idle, 0);
		this.RestartAnimationLoop(this.AnimationB, this.AnimationsB.Idle, 0);
		this.RestartAnimationLoop(this.AnimationC, this.AnimationsC.Idle, 0);
		if (this.IdleSound)
		{
			this.IdleSound.Play();
		}
	}

	public void OnExitIdle()
	{
		if (this.IdleSound)
		{
			this.IdleSound.StopAndFadeOut(0.2f);
		}
	}

	public void OnEnterCharge()
	{
		this.RestartAnimationLoop(this.Animation, this.Animations.Charging, 0);
		this.RestartAnimationLoop(this.AnimationB, this.AnimationsB.Charging, 0);
		this.RestartAnimationLoop(this.AnimationC, this.AnimationsC.Charging, 0);
		base.PlaySound(this.ChargingSound);
		base.SpawnPrefab(this.ChargingEffect);
	}

	public void OnEnterLaser()
	{
		this.RestartAnimationLoop(this.Animation, this.Animations.Laser, 0);
		this.RestartAnimationLoop(this.AnimationB, this.AnimationsB.Laser, 0);
		this.RestartAnimationLoop(this.AnimationC, this.AnimationsC.Laser, 0);
		this.AimLaserAtPlayer();
		this.ActivateLaser();
	}

	public void OnExitLaser()
	{
		this.DeactivateLaser();
	}

	public void OnEnterShooting()
	{
		this.RestartAnimationLoop(this.Animation, this.Animations.Shooting, 0);
		this.RestartAnimationLoop(this.AnimationB, this.AnimationsB.Shooting, 0);
		this.RestartAnimationLoop(this.AnimationC, this.AnimationsC.Shooting, 0);
		base.PlaySound(this.ShootingSound);
		base.SpawnPrefab(this.ShootingEffect);
		this.ProjectileSpawner.AimAt(Characters.Sein.Controller.Transform);
		Projectile projectile = this.ProjectileSpawner.SpawnProjectile();
		projectile.GetComponent<DamageDealer>().Damage = this.Settings.ProjectileDamage;
		this.Movement.ApplyImpulseForce(this.Settings.ShootingForce * this.ProjectileSpawner.Direction * -1f);
	}

	public void UpdateLaserState()
	{
		this.Movement.ApplyForce(-this.Settings.LaserForce * this.m_laserDirection);
		this.UpdateLaserDirection();
		this.UpdateLaser();
	}

	public new void Start()
	{
		base.Start();
		this.State.Idle = new State();
		this.State.Charge = new State();
		this.State.Laser = new State();
		this.State.Shooting = new State();
		State idle = this.State.Idle;
		idle.OnEnterEvent = (Action)Delegate.Combine(idle.OnEnterEvent, new Action(this.OnEnterIdle));
		State idle2 = this.State.Idle;
		idle2.OnExitEvent = (Action)Delegate.Combine(idle2.OnExitEvent, new Action(this.OnExitIdle));
		State charge = this.State.Charge;
		charge.OnEnterEvent = (Action)Delegate.Combine(charge.OnEnterEvent, new Action(this.OnEnterCharge));
		State laser = this.State.Laser;
		laser.OnEnterEvent = (Action)Delegate.Combine(laser.OnEnterEvent, new Action(this.OnEnterLaser));
		State laser2 = this.State.Laser;
		laser2.OnExitEvent = (Action)Delegate.Combine(laser2.OnExitEvent, new Action(this.OnExitLaser));
		State shooting = this.State.Shooting;
		shooting.OnEnterEvent = (Action)Delegate.Combine(shooting.OnEnterEvent, new Action(this.OnEnterShooting));
		State laser3 = this.State.Laser;
		laser3.UpdateStateEvent = (Action)Delegate.Combine(laser3.UpdateStateEvent, new Action(this.UpdateLaserState));
		this.Controller.StateMachine.RegisterStates(new IState[]
		{
			this.State.Idle,
			this.State.Charge,
			this.State.Shooting
		});
		this.Controller.StateMachine.Configure(this.State.Idle).AddTransition<OnFixedUpdate>(this.State.Charge, new Func<bool>(this.ShouldCharge), null);
		this.Controller.StateMachine.Configure(this.State.Charge).AddTransition<OnFixedUpdate>(this.State.Laser, () => base.AfterTime(this.Settings.ChargeDuration), null).AddTransition<OnFixedUpdate>(this.State.Idle, () => this.InCloseDistance(), null);
		this.Controller.StateMachine.Configure(this.State.Laser).AddTransition<OnFixedUpdate>(this.State.Shooting, () => base.AfterTime(this.Settings.LaserDuration), null);
		this.Controller.StateMachine.Configure(this.State.Shooting).AddTransition<OnFixedUpdate>(this.State.Idle, () => base.AfterTime(this.Settings.ShootingDuration), null).AddTransition<OnFixedUpdate>(this.State.Idle, () => this.InCloseDistance(), null);
		this.Controller.StateMachine.ChangeState(this.State.Idle);
		this.ProjectileSpawner.Projectile = this.Settings.Projectile;
		this.ProjectileSpawner.Speed = this.Settings.ProjectileSpeed;
		this.Laser.Activated = false;
		this.Laser.gameObject.SetActive(false);
	}

	public void UpdateLaser()
	{
	}

	public float DesiredLaserRotationDirection
	{
		get
		{
			bool flag = Vector3.Dot(base.PositionToPlayerPosition.normalized, Vector3.Cross(this.m_laserDirection, Vector3.back)) > 0f;
			return (float)((!flag) ? -1 : 1);
		}
	}

	public void UpdateLaserDirection()
	{
		if (this.Controller.NearSein)
		{
			this.m_laserRotationSpeed = Mathf.MoveTowards(this.m_laserRotationSpeed, this.DesiredLaserRotationDirection, Time.deltaTime * 4f);
		}
		float num = this.LaserAngleOverTimeCurve.Evaluate(this.Controller.StateMachine.CurrentStateTime / this.Settings.LaserDuration);
		float num2 = this.Laser.CurrentLaserLength / this.Settings.LaserChaseSpeedDistance;
		float num3 = (!Mathf.Approximately(num2, 0f)) ? (num * this.Settings.LaserChaseSpeed / num2) : 0f;
		float num4 = MoonMath.Angle.AngleFromVector(this.m_laserDirection) + this.m_laserRotationSpeed * RandomizerBonusSkill.TimeScale(Time.deltaTime) * num3;
		this.m_laserDirection = MoonMath.Angle.VectorFromAngle(num4);
		this.Laser.transform.eulerAngles = new Vector3(0f, 0f, num4 - 90f);
	}

	public void ActivateLaser()
	{
		this.UpdateLaserDirection();
		this.UpdateLaser();
		this.Laser.gameObject.SetActive(true);
		this.Laser.Activated = true;
	}

	public void DeactivateLaser()
	{
		this.Laser.Activated = false;
	}

	public void AimLaserAtPlayer()
	{
		Vector3 vector = Characters.Sein.PlatformBehaviour.PlatformMovement.WorldSpeed;
		this.m_laserDirection = base.PositionToPlayerPosition.normalized;
		bool flag = Vector3.Dot(vector.normalized, Vector3.Cross(this.m_laserDirection, Vector3.forward)) > 0f;
		float num = MoonMath.Angle.AngleFromVector(this.m_laserDirection);
		num += (float)((!flag) ? -1 : 1) * this.Settings.LaserAngularOffset;
		this.m_laserDirection = MoonMath.Angle.VectorFromAngle(num);
		this.m_laserRotationSpeed = this.DesiredLaserRotationDirection;
	}

	public new void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		this.Movement.ApplySpringForce(this.Settings.SpringForce, base.StartPosition);
		this.Movement.ApplyDrag(this.Settings.Drag);
	}

	public bool ShouldCharge()
	{
		return this.Controller.NearSein && !this.InCloseDistance();
	}

	public bool InCloseDistance()
	{
		return base.PositionToPlayerPosition.magnitude < this.Settings.CloseDistance;
	}

	public FloatingRockLaserEnemy.States State = new FloatingRockLaserEnemy.States();

	public FloatingRockLaserEnemySettings Settings;

	public FloatingRockLaserEnemyAnimations Animations;

	public FloatingRockLaserEnemyAnimations AnimationsB;

	public FloatingRockLaserEnemyAnimations AnimationsC;

	public CharacterAnimationSystem AnimationB;

	public CharacterAnimationSystem AnimationC;

	public PrefabSpawner ChargingEffect;

	public PrefabSpawner ShootingEffect;

	public ProjectileSpawner ProjectileSpawner;

	public RigidbodyMovement Movement;

	public SoundSource ChargingSound;

	public SoundSource ShootingSound;

	public SoundSource LaserSound;

	public SoundSource IdleSound;

	public SoundSource LaserHitSound;

	public AnimationCurve LaserThicknessCurve;

	public AnimationCurve LaserAngleOverTimeCurve;

	public BlockableLaser Laser;

	public LayerMask LaserLayerMask;

	private float m_laserSpeed;

	private Vector3 m_laserDirection;

	private float m_laserRotationSpeed;

	private Vector3 m_laserStartPosition;

	public class States
	{
		public State Idle;

		public State Charge;

		public State Laser;

		public State Shooting;
	}
}
