using System;
using UnityEngine;

// Token: 0x020005EC RID: 1516
public class PlatformingMovement : PlatformMovement
{
	// Token: 0x17000520 RID: 1312
	// (get) Token: 0x06002078 RID: 8312 RVA: 0x0001C24F File Offset: 0x0001A44F
	// (set) Token: 0x06002079 RID: 8313 RVA: 0x0001C257 File Offset: 0x0001A457
	public override bool IsSuspended { get; set; }

	// Token: 0x0600207A RID: 8314 RVA: 0x0001C260 File Offset: 0x0001A460
	public new void Awake()
	{
		base.Awake();
		this.m_rigidbody = base.GetComponent<Rigidbody>();
		this.m_rigidbody.sleepThreshold = 0f;
	}

	// Token: 0x0600207B RID: 8315 RVA: 0x0001C284 File Offset: 0x0001A484
	public void OnCollisionEnter(Collision collision)
	{
		this.OnCollision(collision);
	}

	// Token: 0x0600207C RID: 8316 RVA: 0x0001C284 File Offset: 0x0001A484
	public void OnCollisionStay(Collision collision)
	{
		this.OnCollision(collision);
	}

	// Token: 0x0600207D RID: 8317 RVA: 0x00094A78 File Offset: 0x00092C78
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

	// Token: 0x0600207E RID: 8318
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

	// Token: 0x0600207F RID: 8319 RVA: 0x00094E30 File Offset: 0x00093030
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

	// Token: 0x04001CCA RID: 7370
	private Rigidbody m_rigidbody;

	// Token: 0x04001CCB RID: 7371
	private Vector2 m_groundContactNormal;
}
