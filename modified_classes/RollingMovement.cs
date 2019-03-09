using System;
using UnityEngine;

// Token: 0x020005ED RID: 1517
public class RollingMovement : SaveSerialize, ISuspendable
{
	// Token: 0x1400002D RID: 45
	// (add) Token: 0x06002081 RID: 8321 RVA: 0x0001C28D File Offset: 0x0001A48D
	// (remove) Token: 0x06002082 RID: 8322 RVA: 0x0001C2A6 File Offset: 0x0001A4A6
	public event Action<Vector3, float, Collider> OnCollisionGroundEvent = delegate(Vector3 A_0, float A_1, Collider A_2)
	{
	};

	// Token: 0x1400002E RID: 46
	// (add) Token: 0x06002083 RID: 8323 RVA: 0x0001C2BF File Offset: 0x0001A4BF
	// (remove) Token: 0x06002084 RID: 8324 RVA: 0x0001C2D8 File Offset: 0x0001A4D8
	public event Action<Vector3, float, Collider> OnCollisionWallLeftEvent = delegate(Vector3 A_0, float A_1, Collider A_2)
	{
	};

	// Token: 0x1400002F RID: 47
	// (add) Token: 0x06002085 RID: 8325 RVA: 0x0001C2F1 File Offset: 0x0001A4F1
	// (remove) Token: 0x06002086 RID: 8326 RVA: 0x0001C30A File Offset: 0x0001A50A
	public event Action<Vector3, float, Collider> OnCollisionWallRightEvent = delegate(Vector3 A_0, float A_1, Collider A_2)
	{
	};

	// Token: 0x17000521 RID: 1313
	// (get) Token: 0x06002087 RID: 8327 RVA: 0x0001C323 File Offset: 0x0001A523
	// (set) Token: 0x06002088 RID: 8328 RVA: 0x0001C330 File Offset: 0x0001A530
	public float SpeedY
	{
		get
		{
			return this.Speed.y;
		}
		set
		{
			this.Speed.y = value;
		}
	}

	// Token: 0x17000522 RID: 1314
	// (get) Token: 0x06002089 RID: 8329 RVA: 0x0001C33E File Offset: 0x0001A53E
	// (set) Token: 0x0600208A RID: 8330 RVA: 0x0001C34B File Offset: 0x0001A54B
	public float SpeedX
	{
		get
		{
			return this.Speed.x;
		}
		set
		{
			this.Speed.x = value;
		}
	}

	// Token: 0x17000523 RID: 1315
	// (get) Token: 0x0600208B RID: 8331 RVA: 0x0001C359 File Offset: 0x0001A559
	public float GroundAngle
	{
		get
		{
			return 57.29578f * Mathf.Atan2(-this.GroundNormal.x, this.GroundNormal.y);
		}
	}

	// Token: 0x0600208C RID: 8332 RVA: 0x0001C37D File Offset: 0x0001A57D
	public Vector2 WorldToGround(Vector2 world)
	{
		return MoonMath.Angle.Unrotate(world, this.GroundAngle);
	}

	// Token: 0x0600208D RID: 8333 RVA: 0x0001C38B File Offset: 0x0001A58B
	public Vector2 GroundToWorld(Vector2 local)
	{
		return MoonMath.Angle.Rotate(local, this.GroundAngle);
	}

	// Token: 0x17000524 RID: 1316
	// (get) Token: 0x0600208E RID: 8334 RVA: 0x0001C399 File Offset: 0x0001A599
	// (set) Token: 0x0600208F RID: 8335 RVA: 0x0001C3A1 File Offset: 0x0001A5A1
	public bool IsSuspended { get; set; }

	// Token: 0x06002090 RID: 8336 RVA: 0x0001C3AA File Offset: 0x0001A5AA
	public new void Awake()
	{
		base.Awake();
		this.m_rigidbody = base.GetComponent<Rigidbody>();
		this.m_rigidbody.sleepThreshold = 0f;
		SuspensionManager.Register(this);
	}

	// Token: 0x06002091 RID: 8337 RVA: 0x00002BE2 File Offset: 0x00000DE2
	public override void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	// Token: 0x06002092 RID: 8338 RVA: 0x0001C3D4 File Offset: 0x0001A5D4
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.Speed);
	}

	// Token: 0x06002093 RID: 8339 RVA: 0x0001C3E2 File Offset: 0x0001A5E2
	public void OnCollisionEnter(Collision collision)
	{
		this.OnCollision(collision);
	}

	// Token: 0x06002094 RID: 8340 RVA: 0x0001C3E2 File Offset: 0x0001A5E2
	public void OnCollisionStay(Collision collision)
	{
		this.OnCollision(collision);
	}

	// Token: 0x06002095 RID: 8341 RVA: 0x00094FBC File Offset: 0x000931BC
	public void OnCollision(Collision collision)
	{
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			this.Speed -= Vector3.Dot(this.Speed.normalized, contactPoint.normal) * contactPoint.normal;
			if (Vector3.Dot(contactPoint.normal, Vector3.up) > Mathf.Cos(0.7853982f))
			{
				this.m_groundNormal += contactPoint.normal;
				this.Ground.FutureOn = true;
				this.OnCollisionGroundEvent(contactPoint.normal, Vector3.Dot(collision.relativeVelocity, contactPoint.normal), collision.collider);
			}
			if (Vector3.Dot(contactPoint.normal, Vector3.right) > Mathf.Cos(0.34906584f))
			{
				this.WallLeft.FutureOn = true;
				this.OnCollisionWallLeftEvent(contactPoint.normal, Vector3.Dot(collision.relativeVelocity, contactPoint.normal), collision.collider);
			}
			if (Vector3.Dot(contactPoint.normal, Vector3.left) > Mathf.Cos(0.34906584f))
			{
				this.WallRight.FutureOn = true;
				this.OnCollisionWallRightEvent(contactPoint.normal, Vector3.Dot(collision.relativeVelocity, contactPoint.normal), collision.collider);
			}
		}
	}

	// Token: 0x17000525 RID: 1317
	// (get) Token: 0x06002096 RID: 8342 RVA: 0x0001C3EB File Offset: 0x0001A5EB
	public Vector3 GroundBinormal
	{
		get
		{
			return Vector3.Cross(this.GroundNormal, Vector3.forward);
		}
	}

	// Token: 0x06002097 RID: 8343
	public void FixedUpdate()
	{
		this.Ground.Update();
		this.WallLeft.Update();
		this.WallRight.Update();
		this.GroundNormal = ((this.m_groundNormal.magnitude != 0f) ? this.m_groundNormal.normalized : Vector3.up);
		this.IsOnGround = (this.m_groundNormal.magnitude != 0f);
		this.m_groundNormal = Vector3.zero;
		this.Speed.z = 0f;
		this.m_rigidbody.velocity = ((!this.IsSuspended) ? RandomizerBonusSkill.TimeScale(this.Speed) : Vector3.zero);
		this.m_rigidbody.detectCollisions = true;
	}

	// Token: 0x04001CCD RID: 7373
	private Rigidbody m_rigidbody;

	// Token: 0x04001CCE RID: 7374
	public Vector3 Speed;

	// Token: 0x04001CCF RID: 7375
	private Vector3 m_groundNormal;

	// Token: 0x04001CD0 RID: 7376
	public Vector3 GroundNormal;

	// Token: 0x04001CD1 RID: 7377
	public bool IsOnGround;

	// Token: 0x04001CD2 RID: 7378
	public IsOnCollisionState WallLeft = new IsOnCollisionState();

	// Token: 0x04001CD3 RID: 7379
	public IsOnCollisionState WallRight = new IsOnCollisionState();

	// Token: 0x04001CD4 RID: 7380
	public IsOnCollisionState Ground = new IsOnCollisionState();
}
