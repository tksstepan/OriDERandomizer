using System;
using UnityEngine;

public class TraceGroundMovement : SaveSerialize, IDamageReciever, ISuspendable
{
	public float Speed { get; set; }

	public override void Awake()
	{
		this.m_rigidbody = base.GetComponent<Rigidbody>();
		SuspensionManager.Register(this);
		base.Awake();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	public Vector3 Right
	{
		get
		{
			return Vector3.Cross(Vector3.back, this.m_floorNormal);
		}
	}

	public Vector3 Left
	{
		get
		{
			return -this.Right;
		}
	}

	public Vector3 Up
	{
		get
		{
			return this.m_floorNormal;
		}
	}

	public Vector3 Down
	{
		get
		{
			return -this.Up;
		}
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
		this.m_floorNormal = PhysicsHelper.CalculateAverageNormalFromContactPoints(collision.contacts);
		this.m_movingGround.SetGround(collision.transform);
		this.Surface = SurfaceToSoundProviderMap.ColliderMaterialToSurfaceMaterialType(collision.collider);
	}

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

	public void ApplyKickback(float kickbackMultiplier)
	{
		this.Kickback.ApplyKickback(kickbackMultiplier);
	}

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

	public override void Serialize(Archive ar)
	{
		base.transform.position = ar.Serialize(base.transform.position);
		this.Speed = ar.Serialize(this.Speed);
		ar.Serialize(ref this.m_floorNormal);
	}

	public bool IsSuspended { get; set; }

	public Kickback Kickback = new Kickback();

	private Vector3 m_floorNormal = Vector3.up;

	private Rigidbody m_rigidbody;

	private readonly MovingGroundHelper m_movingGround = new MovingGroundHelper();

	public SurfaceMaterialType Surface;

	private Vector3 m_lastPosition;

	private float m_lastAngle;

	public float CurrentAngularVelocity;
}
