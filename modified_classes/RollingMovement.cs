using System;
using UnityEngine;

public class RollingMovement : SaveSerialize, ISuspendable
{
	public event Action<Vector3, float, Collider> OnCollisionGroundEvent = delegate(Vector3 A_0, float A_1, Collider A_2)
	{
	};

	public event Action<Vector3, float, Collider> OnCollisionWallLeftEvent = delegate(Vector3 A_0, float A_1, Collider A_2)
	{
	};

	public event Action<Vector3, float, Collider> OnCollisionWallRightEvent = delegate(Vector3 A_0, float A_1, Collider A_2)
	{
	};

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

	public float GroundAngle
	{
		get
		{
			return 57.29578f * Mathf.Atan2(-this.GroundNormal.x, this.GroundNormal.y);
		}
	}

	public Vector2 WorldToGround(Vector2 world)
	{
		return MoonMath.Angle.Unrotate(world, this.GroundAngle);
	}

	public Vector2 GroundToWorld(Vector2 local)
	{
		return MoonMath.Angle.Rotate(local, this.GroundAngle);
	}

	public bool IsSuspended { get; set; }

	public new void Awake()
	{
		base.Awake();
		this.m_rigidbody = base.GetComponent<Rigidbody>();
		this.m_rigidbody.sleepThreshold = 0f;
		SuspensionManager.Register(this);
	}

	public override void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.Speed);
	}

	public void OnCollisionEnter(Collision collision)
	{
		this.OnCollision(collision);
	}

	public void OnCollisionStay(Collision collision)
	{
		this.OnCollision(collision);
	}

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

	public Vector3 GroundBinormal
	{
		get
		{
			return Vector3.Cross(this.GroundNormal, Vector3.forward);
		}
	}

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

	private Rigidbody m_rigidbody;

	public Vector3 Speed;

	private Vector3 m_groundNormal;

	public Vector3 GroundNormal;

	public bool IsOnGround;

	public IsOnCollisionState WallLeft = new IsOnCollisionState();

	public IsOnCollisionState WallRight = new IsOnCollisionState();

	public IsOnCollisionState Ground = new IsOnCollisionState();
}
