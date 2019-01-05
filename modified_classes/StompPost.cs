using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200024F RID: 591
public class StompPost : SaveSerialize, IDamageReciever, IAttackable, IStompAttackable, ISuspendable, IDynamicGraphicHierarchy
{
	// Token: 0x06000AF7 RID: 2807 RVA: 0x0000A919 File Offset: 0x00008B19
	public new void Awake()
	{
		base.Awake();
		SuspensionManager.Register(this);
		this.m_transform = base.transform;
	}

	// Token: 0x06000AF8 RID: 2808 RVA: 0x00005374 File Offset: 0x00003574
	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	// Token: 0x06000AF9 RID: 2809 RVA: 0x0000A933 File Offset: 0x00008B33
	public void Start()
	{
		this.m_distanceStompedIntoGround = 0f;
		this.m_startLocalPosition = base.transform.localPosition;
	}

	// Token: 0x06000AFA RID: 2810 RVA: 0x0004F628 File Offset: 0x0004D828
	public void OnRecieveDamage(Damage damage)
	{
		if (damage.Type == DamageType.Stomp && Vector3.Dot(base.transform.rotation * Vector3.down, Characters.Sein.PlatformBehaviour.PlatformMovement.GravityDirection) > Mathf.Cos(0.17453292f) && !this.m_activated)
		{
			this.m_distanceStompedIntoGround = Mathf.Min(this.StompIntoGroundAmount, this.m_distanceStompedIntoGround + this.StompIntoGroundAmount / (float)this.NumberOfStomps);
			this.m_remainingRiseDelayTime = this.RisingDelay;
			if (Mathf.Approximately(this.m_distanceStompedIntoGround, this.StompIntoGroundAmount))
			{
				BingoController.OnStompPost(this.MoonGuid);
				this.m_activated = true;
				if (this.AllTheWayInAction)
				{
					this.AllTheWayInAction.Perform(null);
				}
				if (this.AllTheWayInSound)
				{
					Sound.Play(this.AllTheWayInSound.GetSound(null), this.m_transform.position, null);
					return;
				}
			}
			else if (this.StompSound)
			{
				Sound.Play(this.StompSound.GetSound(null), this.m_transform.position, null);
			}
		}
	}

	// Token: 0x06000AFB RID: 2811 RVA: 0x0004F754 File Offset: 0x0004D954
	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		if (this.m_remainingRiseDelayTime > 0f)
		{
			this.m_remainingRiseDelayTime -= Time.deltaTime;
			if (this.m_remainingRiseDelayTime < 0f)
			{
				this.m_remainingRiseDelayTime = 0f;
			}
		}
		if (!this.m_activated && this.m_remainingRiseDelayTime < 0f)
		{
			this.m_distanceStompedIntoGround -= Time.deltaTime * this.RiseSpeed;
			if (this.m_distanceStompedIntoGround < 0f)
			{
				this.m_distanceStompedIntoGround = 0f;
			}
		}
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, this.m_startLocalPosition + Vector3.down * this.m_distanceStompedIntoGround, 0.3f);
	}

	// Token: 0x06000AFC RID: 2812 RVA: 0x0000A951 File Offset: 0x00008B51
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_activated);
		ar.Serialize(ref this.m_distanceStompedIntoGround);
		ar.Serialize(ref this.m_remainingRiseDelayTime);
	}

	// Token: 0x170001AC RID: 428
	// (get) Token: 0x06000AFD RID: 2813 RVA: 0x0000A977 File Offset: 0x00008B77
	// (set) Token: 0x06000AFE RID: 2814 RVA: 0x0000A97F File Offset: 0x00008B7F
	public bool IsSuspended { get; set; }

	// Token: 0x170001AD RID: 429
	// (get) Token: 0x06000AFF RID: 2815 RVA: 0x0000A988 File Offset: 0x00008B88
	public Vector3 Position
	{
		get
		{
			return this.m_transform.position;
		}
	}

	// Token: 0x06000B00 RID: 2816 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CanBeChargeFlamed()
	{
		return false;
	}

	// Token: 0x06000B01 RID: 2817 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CanBeChargeDashed()
	{
		return false;
	}

	// Token: 0x06000B02 RID: 2818 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CanBeGrenaded()
	{
		return false;
	}

	// Token: 0x06000B03 RID: 2819 RVA: 0x00004AE6 File Offset: 0x00002CE6
	public bool CanBeStomped()
	{
		return true;
	}

	// Token: 0x06000B04 RID: 2820 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CanBeBashed()
	{
		return false;
	}

	// Token: 0x06000B05 RID: 2821 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CanBeSpiritFlamed()
	{
		return false;
	}

	// Token: 0x06000B06 RID: 2822 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool IsStompBouncable()
	{
		return false;
	}

	// Token: 0x06000B07 RID: 2823 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CanBeLevelUpBlasted()
	{
		return false;
	}

	// Token: 0x06000B08 RID: 2824 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CountsTowardsSuperJumpAchievement()
	{
		return false;
	}

	// Token: 0x06000B09 RID: 2825 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool IsDead()
	{
		return false;
	}

	// Token: 0x04000A6B RID: 2667
	public int NumberOfStomps = 3;

	// Token: 0x04000A6C RID: 2668
	public float StompIntoGroundAmount = 0.1f;

	// Token: 0x04000A6D RID: 2669
	public float RisingDelay = 8f;

	// Token: 0x04000A6E RID: 2670
	public float RiseSpeed = 1f;

	// Token: 0x04000A6F RID: 2671
	public SoundProvider StompSound;

	// Token: 0x04000A70 RID: 2672
	public SoundProvider AllTheWayInSound;

	// Token: 0x04000A71 RID: 2673
	public ActionMethod AllTheWayInAction;

	// Token: 0x04000A72 RID: 2674
	private Vector3 m_startLocalPosition;

	// Token: 0x04000A73 RID: 2675
	private Transform m_transform;

	// Token: 0x04000A74 RID: 2676
	private float m_distanceStompedIntoGround;

	// Token: 0x04000A75 RID: 2677
	private float m_remainingRiseDelayTime;

	// Token: 0x04000A76 RID: 2678
	private bool m_activated;
}
