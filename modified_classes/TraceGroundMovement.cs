using System;
using UnityEngine;

// Token: 0x0200045B RID: 1115
public class TraceGroundMovement : SaveSerialize, IDamageReciever, ISuspendable
{
	// Token: 0x1700042E RID: 1070
	// (get) Token: 0x060018DC RID: 6364
	// (set) Token: 0x060018DD RID: 6365
	public float Speed { get; set; }

	// Token: 0x060018DE RID: 6366
	public override void Awake()
	{
		this.m_rigidbody = base.GetComponent<Rigidbody>();
		SuspensionManager.Register(this);
		base.Awake();
	}

	// Token: 0x060018DF RID: 6367
	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	// Token: 0x1700042F RID: 1071
	// (get) Token: 0x060018E0 RID: 6368
	public Vector3 Right
	{
		get
		{
			return Vector3.Cross(Vector3.back, this.m_floorNormal);
		}
	}

	// Token: 0x17000430 RID: 1072
	// (get) Token: 0x060018E1 RID: 6369
	public Vector3 Left
	{
		get
		{
			return -this.Right;
		}
	}

	// Token: 0x17000431 RID: 1073
	// (get) Token: 0x060018E2 RID: 6370
	public Vector3 Up
	{
		get
		{
			return this.m_floorNormal;
		}
	}

	// Token: 0x17000432 RID: 1074
	// (get) Token: 0x060018E3 RID: 6371
	public Vector3 Down
	{
		get
		{
			return -this.Up;
		}
	}

	// Token: 0x060018E4 RID: 6372
	public void OnCollisionEnter(Collision collision)
	{
		this.OnCollision(collision);
	}

	// Token: 0x060018E5 RID: 6373
	public void OnCollisionStay(Collision collision)
	{
		this.OnCollision(collision);
	}

	// Token: 0x060018E6 RID: 6374
	public void OnCollision(Collision collision)
	{
		this.m_floorNormal = PhysicsHelper.CalculateAverageNormalFromContactPoints(collision.contacts);
		this.m_movingGround.SetGround(collision.transform);
		this.Surface = SurfaceToSoundProviderMap.ColliderMaterialToSurfaceMaterialType(collision.collider);
	}

	// Token: 0x060018E7 RID: 6375
	public void FixedUpdate()
	{
		this.m_movingGround.Update();
		this.Kickback.AdvanceTime();
		if (this.IsSuspended)
		{
			this.m_rigidbody.velocity = Vector3.zero;
			return;
		}
		float num = this.Speed;
		num += this.Kickback.CurrentKickbackSpeed;
		this.m_rigidbody.velocity = RandomizerBonusSkill.TimeScale(this.Right * num);
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles = new Vector3(0f, 0f, Mathf.LerpAngle(eulerAngles.z, MoonMath.Angle.AngleFromVector(this.Right), 0.2f));
		base.transform.eulerAngles = eulerAngles;
		Vector3 vector = base.transform.position;
		Vector2 vector2 = this.m_movingGround.CalculateDelta(base.transform);
		vector.x += RandomizerBonusSkill.TimeScale(vector2.x);
		vector.y += RandomizerBonusSkill.TimeScale(vector2.y);
		float z = eulerAngles.z;
		float b = Mathf.DeltaAngle(z, this.m_lastAngle) / Time.deltaTime;
		this.m_lastAngle = z;
		this.CurrentAngularVelocity = Mathf.Lerp(this.CurrentAngularVelocity, b, 0.5f);
		if (Vector3.Distance(this.m_lastPosition, vector) > 0.03f)
		{
			this.m_lastPosition = vector;
			vector -= this.Down * 0.05f;
			base.transform.position = vector;
			RaycastHit raycastHit;
			if (this.m_rigidbody.SweepTest(this.Down, out raycastHit, 1f))
			{
				vector += RandomizerBonusSkill.TimeScale(this.Down * raycastHit.distance);
			}
		}
		base.transform.position = vector;
	}

	// Token: 0x060018E8 RID: 6376
	public void ApplyKickback(float kickbackMultiplier)
	{
		this.Kickback.ApplyKickback(kickbackMultiplier);
	}

	// Token: 0x060018E9 RID: 6377
	public void OnRecieveDamage(Damage damage)
	{
		if (damage.Type == DamageType.Acid)
		{
			return;
		}
		if (Vector3.Dot(this.Right, damage.Force) > 0f)
		{
			this.Kickback.ApplyKickback(damage.Force.magnitude);
			return;
		}
		this.Kickback.ApplyKickback(-damage.Force.magnitude);
	}

	// Token: 0x060018EA RID: 6378
	public override void Serialize(Archive ar)
	{
		base.transform.position = ar.Serialize(base.transform.position);
		this.Speed = ar.Serialize(this.Speed);
		ar.Serialize(ref this.m_floorNormal);
	}

	// Token: 0x17000433 RID: 1075
	// (get) Token: 0x060018EB RID: 6379
	// (set) Token: 0x060018EC RID: 6380
	public bool IsSuspended { get; set; }

	// Token: 0x04001585 RID: 5509
	public Kickback Kickback = new Kickback();

	// Token: 0x04001586 RID: 5510
	private Vector3 m_floorNormal = Vector3.up;

	// Token: 0x04001587 RID: 5511
	private Rigidbody m_rigidbody;

	// Token: 0x04001588 RID: 5512
	private readonly MovingGroundHelper m_movingGround = new MovingGroundHelper();

	// Token: 0x04001589 RID: 5513
	public SurfaceMaterialType Surface;

	// Token: 0x0400158A RID: 5514
	private Vector3 m_lastPosition;

	// Token: 0x0400158B RID: 5515
	private float m_lastAngle;

	// Token: 0x0400158C RID: 5516
	public float CurrentAngularVelocity;
}
