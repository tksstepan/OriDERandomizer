using System;
using UnityEngine;

public class PlatformingMovement : PlatformMovement
{
	public override bool IsSuspended { get; set; }

	public new void Awake()
	{
		base.Awake();
		this.m_rigidbody = base.GetComponent<Rigidbody>();
		this.m_rigidbody.sleepThreshold = 0f;
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
		for (int i = 0; i < collision.contacts.Length; i++)
		{
			ContactPoint contactPoint = collision.contacts[i];
			Vector2 vector = base.WorldToLocal(contactPoint.normal);
			if (PlatformMovement.IsWallLeft(vector, contactPoint.otherCollider, 30f))
			{
				base.OnCollisionWallLeft(vector, contactPoint.otherCollider);
			}
			if (PlatformMovement.IsWallRight(vector, contactPoint.otherCollider, 30f))
			{
				base.OnCollisionWallRight(vector, contactPoint.otherCollider);
			}
			if (PlatformMovement.IsGround(vector, contactPoint.otherCollider, 60f))
			{
				this.m_groundContactNormal += vector;
				base.OnCollisionGround(vector, contactPoint.otherCollider);
			}
			if (PlatformMovement.IsCeiling(vector, contactPoint.otherCollider, 60f))
			{
				base.OnCollisionCeiling(vector, contactPoint.otherCollider);
			}
		}
	}

	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			this.m_rigidbody.velocity = Vector3.zero;
			if (this.m_rigidbody.detectCollisions)
			{
				this.m_rigidbody.detectCollisions = false;
				return;
			}
		}
		else
		{
			if (!this.m_rigidbody.detectCollisions)
			{
				this.m_rigidbody.detectCollisions = true;
			}
			base.PreFixedUpdate();
			if (this.m_groundContactNormal.magnitude == 0f)
			{
				this.GroundNormal = Vector3.up;
			}
			else
			{
				this.GroundNormal = this.m_groundContactNormal.normalized;
			}
			this.m_groundContactNormal = Vector3.zero;
			if (base.IsOnGround && !Physics.Raycast(new Ray(base.Position + base.WorldOffsetToBottomSphereOfCapsuleCollider, base.GravityDirection), base.CapsuleCollider.radius * base.transform.lossyScale.y + 0.5f))
			{
				this.Ground.IsOn = false;
			}
			if (base.IsOnGround)
			{
				base.LocalSpeedY = 0f;
				Vector3 position = base.transform.position;
				base.transform.position += base.GroundBinormal * base.LocalSpeedX * Time.deltaTime;
				base.transform.position += this.GroundNormal * 0.02f;
				Vector3 vector = (0.04f + Mathf.Abs(base.LocalSpeedX) * Time.deltaTime) * -this.GroundNormal;
				RaycastHit raycastHit;
				if (this.m_rigidbody.SweepTest(vector.normalized, out raycastHit, vector.magnitude))
				{
					base.transform.position += vector.normalized * (raycastHit.distance + 0.02f);
				}
				else
				{
					base.transform.position -= this.GroundNormal * 0.02f;
				}
				if (Time.deltaTime == 0f)
				{
					this.m_rigidbody.velocity = Vector3.zero;
				}
				else
				{
					this.m_rigidbody.velocity = (base.transform.position - position) / Time.deltaTime;
				}
				this.m_rigidbody.position = position;
			}
			else
			{
				this.m_rigidbody.velocity = RandomizerBonusSkill.TimeScale(this.WorldSpeed);
			}
			base.PostFixedUpdate();
		}
	}

	public override void PlaceOnGround(float lift = 0.5f, float distance = 0f)
	{
		this.Position += base.LocalToWorld(Vector3.up * lift);
		if (distance == 0f)
		{
			distance = 50f;
		}
		else
		{
			distance += lift;
		}
		Vector3 vector = base.LocalToWorld(Vector3.down * distance);
		RaycastHit raycastHit;
		if (this.m_rigidbody.SweepTest(vector.normalized, out raycastHit, vector.magnitude))
		{
			this.Position += raycastHit.distance * vector.normalized;
		}
		else
		{
			this.Position += base.LocalToWorld(Vector3.down * 0.5f);
		}
	}

	private Rigidbody m_rigidbody;

	private Vector2 m_groundContactNormal;
}
