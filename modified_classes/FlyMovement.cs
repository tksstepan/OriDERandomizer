using System;
using UnityEngine;

public class FlyMovement : SaveSerialize, IDamageReciever, ISuspendable
{
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

	public Rigidbody Rigidbody
	{
		get
		{
			return this.m_rigidbody;
		}
	}

	public override void Awake()
	{
		base.Awake();
		this.m_rigidbody = base.GetComponent<Rigidbody>();
		SuspensionManager.Register(this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	public void Start()
	{
	}

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

	public void OnRecieveDamage(Damage damage)
	{
		if (this.HasKickback)
		{
			this.Kickback.ApplyKickback(damage.Force.magnitude, damage.Force);
		}
	}

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

	public override void Serialize(Archive ar)
	{
		this.Velocity = ar.Serialize(this.Velocity);
		this.m_rigidbody.velocity = ar.Serialize(this.m_rigidbody.velocity);
		base.transform.position = ar.Serialize(base.transform.position);
	}

	public bool IsSuspended { get; set; }

	public Kickback Kickback;

	public bool HasKickback = true;

	public Vector2 Velocity;

	private Rigidbody m_rigidbody;
}
