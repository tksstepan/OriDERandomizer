using System;
using fsm;
using fsm.triggers;
using Game;
using UnityEngine;

// Token: 0x02000492 RID: 1170
public class FloatingRockLaserEnemy : Enemy
{
	// Token: 0x060019D9 RID: 6617 RVA: 0x00016ADA File Offset: 0x00014CDA
	public void PlayAnimationOnce(CharacterAnimationSystem animationSystem, TextureAnimationWithTransitions anim, int layer = 0)
	{
		if (anim && animationSystem)
		{
			animationSystem.Play(anim, layer, null);
		}
	}

	// Token: 0x060019DA RID: 6618 RVA: 0x00016AFC File Offset: 0x00014CFC
	public void RestartAnimationLoop(CharacterAnimationSystem animationSystem, TextureAnimationWithTransitions anim, int layer = 0)
	{
		if (anim && animationSystem)
		{
			animationSystem.RestartLoop(anim, layer, null);
		}
	}

	// Token: 0x060019DB RID: 6619 RVA: 0x00016B1E File Offset: 0x00014D1E
	public void PlayAnimationLoop(CharacterAnimationSystem animationSystem, TextureAnimationWithTransitions anim, int layer = 0)
	{
		if (anim && animationSystem)
		{
			animationSystem.PlayLoop(anim, layer, null, false);
		}
	}

	// Token: 0x060019DC RID: 6620 RVA: 0x00016B41 File Offset: 0x00014D41
	public new void Awake()
	{
		base.Awake();
		EntityDamageReciever damageReciever = this.DamageReciever;
		damageReciever.OnModifyDamage = (EntityDamageReciever.ModifyDamageDelegate)Delegate.Combine(damageReciever.OnModifyDamage, new EntityDamageReciever.ModifyDamageDelegate(this.OnModifyDamage));
	}

	// Token: 0x060019DD RID: 6621 RVA: 0x00016B71 File Offset: 0x00014D71
	public new void OnDestroy()
	{
		base.OnDestroy();
		EntityDamageReciever damageReciever = this.DamageReciever;
		damageReciever.OnModifyDamage = (EntityDamageReciever.ModifyDamageDelegate)Delegate.Remove(damageReciever.OnModifyDamage, new EntityDamageReciever.ModifyDamageDelegate(this.OnModifyDamage));
	}

	// Token: 0x060019DE RID: 6622 RVA: 0x00016BA1 File Offset: 0x00014DA1
	public virtual void OnModifyDamage(Damage damage)
	{
		if (damage.Type == DamageType.SpiritFlame)
		{
			damage.SetAmount(0f);
		}
	}

	// Token: 0x060019DF RID: 6623 RVA: 0x000812A8 File Offset: 0x0007F4A8
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

	// Token: 0x060019E0 RID: 6624 RVA: 0x00016BBB File Offset: 0x00014DBB
	public void OnExitIdle()
	{
		if (this.IdleSound)
		{
			this.IdleSound.StopAndFadeOut(0.2f);
		}
	}

	// Token: 0x060019E1 RID: 6625 RVA: 0x00081318 File Offset: 0x0007F518
	public void OnEnterCharge()
	{
		this.RestartAnimationLoop(this.Animation, this.Animations.Charging, 0);
		this.RestartAnimationLoop(this.AnimationB, this.AnimationsB.Charging, 0);
		this.RestartAnimationLoop(this.AnimationC, this.AnimationsC.Charging, 0);
		base.PlaySound(this.ChargingSound);
		base.SpawnPrefab(this.ChargingEffect);
	}

	// Token: 0x060019E2 RID: 6626 RVA: 0x00081388 File Offset: 0x0007F588
	public void OnEnterLaser()
	{
		this.RestartAnimationLoop(this.Animation, this.Animations.Laser, 0);
		this.RestartAnimationLoop(this.AnimationB, this.AnimationsB.Laser, 0);
		this.RestartAnimationLoop(this.AnimationC, this.AnimationsC.Laser, 0);
		this.AimLaserAtPlayer();
		this.ActivateLaser();
	}

	// Token: 0x060019E3 RID: 6627 RVA: 0x00016BDD File Offset: 0x00014DDD
	public void OnExitLaser()
	{
		this.DeactivateLaser();
	}

	// Token: 0x060019E4 RID: 6628 RVA: 0x000813EC File Offset: 0x0007F5EC
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

	// Token: 0x060019E5 RID: 6629 RVA: 0x00016BE5 File Offset: 0x00014DE5
	public void UpdateLaserState()
	{
		this.Movement.ApplyForce(-this.Settings.LaserForce * this.m_laserDirection);
		this.UpdateLaserDirection();
		this.UpdateLaser();
	}

	// Token: 0x060019E6 RID: 6630 RVA: 0x000814C8 File Offset: 0x0007F6C8
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

	// Token: 0x060019E7 RID: 6631 RVA: 0x000030E7 File Offset: 0x000012E7
	public void UpdateLaser()
	{
	}

	// Token: 0x17000439 RID: 1081
	// (get) Token: 0x060019E8 RID: 6632 RVA: 0x00081814 File Offset: 0x0007FA14
	public float DesiredLaserRotationDirection
	{
		get
		{
			bool flag = Vector3.Dot(base.PositionToPlayerPosition.normalized, Vector3.Cross(this.m_laserDirection, Vector3.back)) > 0f;
			return (float)((!flag) ? -1 : 1);
		}
	}

	// Token: 0x060019E9 RID: 6633
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

	// Token: 0x060019EA RID: 6634 RVA: 0x00016C15 File Offset: 0x00014E15
	public void ActivateLaser()
	{
		this.UpdateLaserDirection();
		this.UpdateLaser();
		this.Laser.gameObject.SetActive(true);
		this.Laser.Activated = true;
	}

	// Token: 0x060019EB RID: 6635 RVA: 0x00016C40 File Offset: 0x00014E40
	public void DeactivateLaser()
	{
		this.Laser.Activated = false;
	}

	// Token: 0x060019EC RID: 6636 RVA: 0x0008195C File Offset: 0x0007FB5C
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

	// Token: 0x060019ED RID: 6637 RVA: 0x00016C4E File Offset: 0x00014E4E
	public new void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		this.Movement.ApplySpringForce(this.Settings.SpringForce, base.StartPosition);
		this.Movement.ApplyDrag(this.Settings.Drag);
	}

	// Token: 0x060019EE RID: 6638 RVA: 0x00016C8E File Offset: 0x00014E8E
	public bool ShouldCharge()
	{
		return this.Controller.NearSein && !this.InCloseDistance();
	}

	// Token: 0x060019EF RID: 6639 RVA: 0x00081A08 File Offset: 0x0007FC08
	public bool InCloseDistance()
	{
		return base.PositionToPlayerPosition.magnitude < this.Settings.CloseDistance;
	}

	// Token: 0x0400169F RID: 5791
	public FloatingRockLaserEnemy.States State = new FloatingRockLaserEnemy.States();

	// Token: 0x040016A0 RID: 5792
	public FloatingRockLaserEnemySettings Settings;

	// Token: 0x040016A1 RID: 5793
	public FloatingRockLaserEnemyAnimations Animations;

	// Token: 0x040016A2 RID: 5794
	public FloatingRockLaserEnemyAnimations AnimationsB;

	// Token: 0x040016A3 RID: 5795
	public FloatingRockLaserEnemyAnimations AnimationsC;

	// Token: 0x040016A4 RID: 5796
	public CharacterAnimationSystem AnimationB;

	// Token: 0x040016A5 RID: 5797
	public CharacterAnimationSystem AnimationC;

	// Token: 0x040016A6 RID: 5798
	public PrefabSpawner ChargingEffect;

	// Token: 0x040016A7 RID: 5799
	public PrefabSpawner ShootingEffect;

	// Token: 0x040016A8 RID: 5800
	public ProjectileSpawner ProjectileSpawner;

	// Token: 0x040016A9 RID: 5801
	public RigidbodyMovement Movement;

	// Token: 0x040016AA RID: 5802
	public SoundSource ChargingSound;

	// Token: 0x040016AB RID: 5803
	public SoundSource ShootingSound;

	// Token: 0x040016AC RID: 5804
	public SoundSource LaserSound;

	// Token: 0x040016AD RID: 5805
	public SoundSource IdleSound;

	// Token: 0x040016AE RID: 5806
	public SoundSource LaserHitSound;

	// Token: 0x040016AF RID: 5807
	public AnimationCurve LaserThicknessCurve;

	// Token: 0x040016B0 RID: 5808
	public AnimationCurve LaserAngleOverTimeCurve;

	// Token: 0x040016B1 RID: 5809
	public BlockableLaser Laser;

	// Token: 0x040016B2 RID: 5810
	public LayerMask LaserLayerMask;

	// Token: 0x040016B3 RID: 5811
	private float m_laserSpeed;

	// Token: 0x040016B4 RID: 5812
	private Vector3 m_laserDirection;

	// Token: 0x040016B5 RID: 5813
	private float m_laserRotationSpeed;

	// Token: 0x040016B6 RID: 5814
	private Vector3 m_laserStartPosition;

	// Token: 0x02000493 RID: 1171
	public class States
	{
		// Token: 0x040016B7 RID: 5815
		public State Idle;

		// Token: 0x040016B8 RID: 5816
		public State Charge;

		// Token: 0x040016B9 RID: 5817
		public State Laser;

		// Token: 0x040016BA RID: 5818
		public State Shooting;
	}
}
