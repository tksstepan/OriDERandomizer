using System;
using UnityEngine;

// Token: 0x02000459 RID: 1113
public class FlyMovement : SaveSerialize, IDamageReciever, ISuspendable
{
	// Token: 0x17000426 RID: 1062
	// (get) Token: 0x060018BF RID: 6335 RVA: 0x00015D57 File Offset: 0x00013F57
	// (set) Token: 0x060018C0 RID: 6336 RVA: 0x00015D64 File Offset: 0x00013F64
	public float Speed
	{
		get
		{
			return this.Velocity.magnitude;
		}
		set
		{
			this.Velocity = this.Velocity.normalized * this.Speed;
		}
	}

	// Token: 0x17000427 RID: 1063
	// (get) Token: 0x060018C1 RID: 6337 RVA: 0x00015D82 File Offset: 0x00013F82
	// (set) Token: 0x060018C2 RID: 6338 RVA: 0x00015D8F File Offset: 0x00013F8F
	public float Angle
	{
		get
		{
			return MoonMath.Angle.AngleFromVector(this.Velocity);
		}
		set
		{
			this.Velocity = this.Velocity.magnitude * MoonMath.Angle.VectorFromAngle(value);
		}
	}

	// Token: 0x17000428 RID: 1064
	// (get) Token: 0x060018C3 RID: 6339 RVA: 0x00015DAD File Offset: 0x00013FAD
	// (set) Token: 0x060018C4 RID: 6340 RVA: 0x00015DBF File Offset: 0x00013FBF
	public Vector2 VelocityAsDelta
	{
		get
		{
			return this.Velocity * Time.deltaTime;
		}
		set
		{
			this.Velocity = ((Time.deltaTime != 0f) ? (value / Time.deltaTime) : Vector2.zero);
		}
	}

	// Token: 0x17000429 RID: 1065
	// (get) Token: 0x060018C5 RID: 6341 RVA: 0x00015DEB File Offset: 0x00013FEB
	public Rigidbody Rigidbody
	{
		get
		{
			return this.m_rigidbody;
		}
	}

	// Token: 0x060018C6 RID: 6342 RVA: 0x00015DF3 File Offset: 0x00013FF3
	public override void Awake()
	{
		base.Awake();
		this.m_rigidbody = base.GetComponent<Rigidbody>();
		SuspensionManager.Register(this);
	}

	// Token: 0x060018C7 RID: 6343 RVA: 0x00005374 File Offset: 0x00003574
	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	// Token: 0x060018C8 RID: 6344 RVA: 0x000030E7 File Offset: 0x000012E7
	public void Start()
	{
	}

	// Token: 0x060018C9 RID: 6345
	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			this.m_rigidbody.velocity = Vector3.zero;
			return;
		}
		this.Kickback.AdvanceTime();
		this.m_rigidbody.velocity = RandomizerBonusSkill.TimeScale(this.Velocity + ((!this.HasKickback) ? Vector2.zero : this.Kickback.KickbackVector));
	}

	// Token: 0x060018CA RID: 6346 RVA: 0x0007D38C File Offset: 0x0007B58C
	public void OnRecieveDamage(Damage damage)
	{
		if (this.HasKickback)
		{
			this.Kickback.ApplyKickback(damage.Force.magnitude, damage.Force);
		}
	}

	// Token: 0x1700042A RID: 1066
	// (get) Token: 0x060018CB RID: 6347 RVA: 0x00015E0D File Offset: 0x0001400D
	// (set) Token: 0x060018CC RID: 6348 RVA: 0x0007D3C4 File Offset: 0x0007B5C4
	public float VelocityX
	{
		get
		{
			return this.Velocity.x;
		}
		set
		{
			Vector2 velocity = this.Velocity;
			velocity.x = value;
			this.Velocity = velocity;
		}
	}

	// Token: 0x1700042B RID: 1067
	// (get) Token: 0x060018CD RID: 6349 RVA: 0x00015E1A File Offset: 0x0001401A
	// (set) Token: 0x060018CE RID: 6350 RVA: 0x0007D3E8 File Offset: 0x0007B5E8
	public float VelocityY
	{
		get
		{
			return this.Velocity.y;
		}
		set
		{
			Vector2 velocity = this.Velocity;
			velocity.y = value;
			this.Velocity = velocity;
		}
	}

	// Token: 0x060018CF RID: 6351 RVA: 0x0007D40C File Offset: 0x0007B60C
	public override void Serialize(Archive ar)
	{
		this.Velocity = ar.Serialize(this.Velocity);
		this.m_rigidbody.velocity = ar.Serialize(this.m_rigidbody.velocity);
		base.transform.position = ar.Serialize(base.transform.position);
	}

	// Token: 0x1700042C RID: 1068
	// (get) Token: 0x060018D0 RID: 6352 RVA: 0x00015E27 File Offset: 0x00014027
	// (set) Token: 0x060018D1 RID: 6353 RVA: 0x00015E2F File Offset: 0x0001402F
	public bool IsSuspended { get; set; }

	// Token: 0x0400157F RID: 5503
	public Kickback Kickback;

	// Token: 0x04001580 RID: 5504
	public bool HasKickback = true;

	// Token: 0x04001581 RID: 5505
	public Vector2 Velocity;

	// Token: 0x04001582 RID: 5506
	private Rigidbody m_rigidbody;
}
