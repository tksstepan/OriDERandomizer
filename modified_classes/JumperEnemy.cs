using System;
using fsm;
using fsm.triggers;
using Game;
using UnityEngine;

// Token: 0x02000464 RID: 1124
public class JumperEnemy : GroundEnemy
{
	// Token: 0x06001916 RID: 6422 RVA: 0x00016219 File Offset: 0x00014419
	public override bool CanBeOptimized()
	{
		return this.Controller.StateMachine.CurrentState == this.State.Idle;
	}

	// Token: 0x06001917 RID: 6423 RVA: 0x00016238 File Offset: 0x00014438
	public void ForceAttackPlayer()
	{
		this.Controller.StateMachine.ChangeState(this.State.JumpCharge);
	}

	// Token: 0x06001918 RID: 6424 RVA: 0x0007E11C File Offset: 0x0007C31C
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

	// Token: 0x06001919 RID: 6425 RVA: 0x00016255 File Offset: 0x00014455
	public new void OnTimedRespawn()
	{
		this.m_timedRespawn = true;
	}

	// Token: 0x0600191A RID: 6426 RVA: 0x0007E66C File Offset: 0x0007C86C
	public bool OutOfJumpingZone()
	{
		return !(this.JumpingZone == null) && !new Rect
		{
			width = this.JumpingZone.lossyScale.x,
			height = this.JumpingZone.lossyScale.y,
			center = this.JumpingZone.position
		}.Contains(base.Position);
	}

	// Token: 0x0600191B RID: 6427 RVA: 0x0001625E File Offset: 0x0001445E
	public bool IsOnGround()
	{
		return this.PlatformMovement.IsOnGround;
	}

	// Token: 0x0600191C RID: 6428 RVA: 0x00079B78 File Offset: 0x00077D78
	public bool ShouldThrow()
	{
		OnReceiveDamage onReceiveDamage = (OnReceiveDamage)this.Controller.StateMachine.CurrentTrigger;
		return onReceiveDamage.Damage.Type == DamageType.Bash;
	}

	// Token: 0x0600191D RID: 6429 RVA: 0x0007E6F4 File Offset: 0x0007C8F4
	public bool ShouldStomped()
	{
		OnReceiveDamage onReceiveDamage = (OnReceiveDamage)this.Controller.StateMachine.CurrentTrigger;
		return onReceiveDamage.Damage.Type == DamageType.StompBlast;
	}

	// Token: 0x0600191E RID: 6430 RVA: 0x0007E728 File Offset: 0x0007C928
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

	// Token: 0x0600191F RID: 6431 RVA: 0x0007E7E0 File Offset: 0x0007C9E0
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

	// Token: 0x06001920 RID: 6432 RVA: 0x0007E898 File Offset: 0x0007CA98
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

	// Token: 0x06001921 RID: 6433 RVA: 0x0007EB00 File Offset: 0x0007CD00
	public bool PlayerInRange()
	{
		return base.PositionToPlayerPosition.magnitude < this.Settings.ChargeRange && this.Controller.NearSein;
	}

	// Token: 0x06001922 RID: 6434
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

	// Token: 0x06001923 RID: 6435 RVA: 0x0007EC28 File Offset: 0x0007CE28
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

	// Token: 0x06001924 RID: 6436 RVA: 0x0001626B File Offset: 0x0001446B
	public bool LandedOnGround()
	{
		return this.PlatformMovement.IsOnGround && this.PlatformMovement.LocalSpeedY <= 0f;
	}

	// Token: 0x06001925 RID: 6437 RVA: 0x0007ED18 File Offset: 0x0007CF18
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

	// Token: 0x040015D2 RID: 5586
	public JumpingSootEnemyAnimations Animations;

	// Token: 0x040015D3 RID: 5587
	public JumpingSootEnemySettings Settings;

	// Token: 0x040015D4 RID: 5588
	public JumpingSootEnemySounds Sounds;

	// Token: 0x040015D5 RID: 5589
	public JumperEnemy.States State = new JumperEnemy.States();

	// Token: 0x040015D6 RID: 5590
	public Transform JumpingZone;

	// Token: 0x040015D7 RID: 5591
	public LayerMask RaycastLayerMask;

	// Token: 0x040015D8 RID: 5592
	private Vector3 m_playerSmoothSpeed;

	// Token: 0x040015D9 RID: 5593
	private bool m_shouldStomp;

	// Token: 0x040015DA RID: 5594
	private Vector3 m_thrownDirection;

	// Token: 0x040015DB RID: 5595
	private bool m_timedRespawn;

	// Token: 0x040015DC RID: 5596
	public GameObject StompEffect;

	// Token: 0x040015DD RID: 5597
	public GameObject LandEffect;

	// Token: 0x02000465 RID: 1125
	public class States
	{
		// Token: 0x040015DE RID: 5598
		public State Respawn;

		// Token: 0x040015DF RID: 5599
		public JumperEnemyIdleState Idle;

		// Token: 0x040015E0 RID: 5600
		public JumperEnemyChargingState JumpCharge;

		// Token: 0x040015E1 RID: 5601
		public JumperEnemyFallState Fall;

		// Token: 0x040015E2 RID: 5602
		public JumperEnemyThrownState Thrown;

		// Token: 0x040015E3 RID: 5603
		public JumperEnemyStompedState Stomped;

		// Token: 0x040015E4 RID: 5604
		public JumperEnemyStunnedState Stunned;
	}
}
