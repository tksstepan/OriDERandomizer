using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x020008E4 RID: 2276
public class Projectile : MonoBehaviour, IDamageReciever, IAttackable, IChargeFlameAttackable, IStompAttackable, IBashAttackable, IPooled, ISuspendable, IPortalVisitor, IReflectable
{
	// Token: 0x060031AC RID: 12716 RVA: 0x00027F56 File Offset: 0x00026156
	public Projectile()
	{
	}

	// Token: 0x170007CE RID: 1998
	// (get) Token: 0x060031AD RID: 12717 RVA: 0x00027F91 File Offset: 0x00026191
	// (set) Token: 0x060031AE RID: 12718 RVA: 0x00027F99 File Offset: 0x00026199
	Vector3 IPortalVisitor.Speed
	{
		get
		{
			return this.Direction;
		}
		set
		{
			this.Direction = value;
		}
	}

	// Token: 0x170007CF RID: 1999
	// (get) Token: 0x060031AF RID: 12719 RVA: 0x00027FA2 File Offset: 0x000261A2
	// (set) Token: 0x060031B0 RID: 12720 RVA: 0x00027FAA File Offset: 0x000261AA
	public Vector3 Direction { get; set; }

	// Token: 0x170007D0 RID: 2000
	// (get) Token: 0x060031B1 RID: 12721 RVA: 0x00027FB3 File Offset: 0x000261B3
	// (set) Token: 0x060031B2 RID: 12722 RVA: 0x00027FBB File Offset: 0x000261BB
	public float Speed { get; set; }

	// Token: 0x170007D1 RID: 2001
	// (get) Token: 0x060031B3 RID: 12723 RVA: 0x00027FC4 File Offset: 0x000261C4
	// (set) Token: 0x060031B4 RID: 12724 RVA: 0x00027FCC File Offset: 0x000261CC
	public GameObject LastReflector { get; set; }

	// Token: 0x170007D2 RID: 2002
	// (get) Token: 0x060031B5 RID: 12725 RVA: 0x00027FD5 File Offset: 0x000261D5
	// (set) Token: 0x060031B6 RID: 12726 RVA: 0x00027FDD File Offset: 0x000261DD
	public Vector3 Displacement { get; set; }

	// Token: 0x170007D3 RID: 2003
	// (get) Token: 0x060031B7 RID: 12727 RVA: 0x00027FE6 File Offset: 0x000261E6
	// (set) Token: 0x060031B8 RID: 12728 RVA: 0x00027FEE File Offset: 0x000261EE
	public bool IsSuspended { get; set; }

	// Token: 0x060031B9 RID: 12729 RVA: 0x00027FF7 File Offset: 0x000261F7
	public void OnValidate()
	{
		this.m_onKillRecievers = base.GetComponentsInChildren(typeof(IKillReciever));
	}

	// Token: 0x060031BA RID: 12730 RVA: 0x000D00E8 File Offset: 0x000CE2E8
	public void OnPoolSpawned()
	{
		this.HasBeenBashedByOri = false;
		this.CurrentTime = 0f;
		this.Gravity = this.m_originalGravity;
		this.Direction = Vector3.left;
		this.Speed = 0f;
		this.m_explode = false;
		this.m_explodeLater = false;
		this.m_lastLoop = null;
		this.LastReflector = null;
		this.Displacement = Vector3.zero;
		this.IsSuspended = false;
		this.Owner = null;
	}

	// Token: 0x060031BB RID: 12731 RVA: 0x000D0160 File Offset: 0x000CE360
	public void Start()
	{
		if (this.RotateSpriteToDirection)
		{
			base.transform.eulerAngles = new Vector3(0f, 0f, MoonMath.Angle.AngleFromVector(this.Direction));
		}
		if (this.EnableCollisionGracePeriod)
		{
			this.m_collider.enabled = false;
		}
	}

	// Token: 0x060031BC RID: 12732 RVA: 0x000D01BC File Offset: 0x000CE3BC
	public void Awake()
	{
		this.m_nullify = delegate()
		{
			this.m_lastLoop = null;
		};
		SuspensionManager.Register(this);
		this.Direction = Vector3.left;
		this.Speed = 0f;
		this.m_collider = base.GetComponent<Collider>();
		this.Rigidbody = base.GetComponent<Rigidbody>();
		DamageDealer component = base.GetComponent<DamageDealer>();
		if (component)
		{
			DamageDealer damageDealer = component;
			damageDealer.OnDamageDealtEvent = (Action<GameObject, Damage>)Delegate.Combine(damageDealer.OnDamageDealtEvent, new Action<GameObject, Damage>(this.OnDamageDealt));
		}
		if (this.ProjectileLoop)
		{
			this.m_lastLoop = Sound.Play(this.ProjectileLoop.GetSound(null), base.transform.position, this.m_nullify);
			if (this.m_lastLoop)
			{
				this.m_lastLoop.AttachTo = base.transform;
			}
		}
		this.m_originalGravity = this.Gravity;
	}

	// Token: 0x060031BD RID: 12733 RVA: 0x0002800F File Offset: 0x0002620F
	public void OnEnable()
	{
		Targets.Attackables.Add(this);
		PortalVistor.All.Add(this);
		this.CurrentTime = 0f;
		this.Rigidbody.velocity = Vector3.zero;
	}

	// Token: 0x060031BE RID: 12734 RVA: 0x00028042 File Offset: 0x00026242
	public void OnDisable()
	{
		Targets.Attackables.Remove(this);
		PortalVistor.All.Remove(this);
	}

	// Token: 0x060031BF RID: 12735 RVA: 0x00002BE2 File Offset: 0x00000DE2
	public void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	// Token: 0x060031C0 RID: 12736 RVA: 0x0002805B File Offset: 0x0002625B
	public virtual bool CanBeBashed()
	{
		return this.CanProjectileBeBashed;
	}

	// Token: 0x060031C1 RID: 12737 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CanBeSpiritFlamed()
	{
		return false;
	}

	// Token: 0x060031C2 RID: 12738 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool IsStompBouncable()
	{
		return false;
	}

	// Token: 0x060031C3 RID: 12739 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CanBeLevelUpBlasted()
	{
		return false;
	}

	// Token: 0x060031C4 RID: 12740 RVA: 0x00028063 File Offset: 0x00026263
	public void OnEnterBash()
	{
		if (this.m_lastLoop)
		{
			this.m_lastLoop.FadeOut(0.3f, true);
		}
	}

	// Token: 0x060031C5 RID: 12741 RVA: 0x000030E7 File Offset: 0x000012E7
	public void OnBashHighlight()
	{
	}

	// Token: 0x060031C6 RID: 12742 RVA: 0x000030E7 File Offset: 0x000012E7
	public void OnBashDehighlight()
	{
	}

	// Token: 0x170007D4 RID: 2004
	// (get) Token: 0x060031C7 RID: 12743 RVA: 0x00028086 File Offset: 0x00026286
	public int BashPriority
	{
		get
		{
			return 40;
		}
	}

	// Token: 0x060031C8 RID: 12744 RVA: 0x000D02A8 File Offset: 0x000CE4A8
	public void OnRecieveDamage(Damage damage)
	{
		DamageType type = damage.Type;
		switch (type)
		{
		case DamageType.Bash:
			this.HasBeenBashedByOri = true;
			this.Direction = damage.Force.normalized;
			if (this.UseBashSpeed)
			{
				this.Speed = this.BashSpeed;
			}
			if (this.CancelGravityOnBash)
			{
				this.Gravity = 0f;
			}
			this.Owner = null;
			return;
		case DamageType.Grenade:
			break;
		default:
			if (type != DamageType.ChargeFlame)
			{
				return;
			}
			break;
		case DamageType.StompBlast:
			this.Direction = damage.Force.normalized;
			this.Owner = null;
			return;
		}
		this.Direction = damage.Force.normalized;
		this.Owner = null;
	}

	// Token: 0x170007D5 RID: 2005
	// (get) Token: 0x060031C9 RID: 12745 RVA: 0x00002AF6 File Offset: 0x00000CF6
	// (set) Token: 0x060031CA RID: 12746 RVA: 0x00002B03 File Offset: 0x00000D03
	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	// Token: 0x060031CB RID: 12747 RVA: 0x00004AE6 File Offset: 0x00002CE6
	public bool CanBeStomped()
	{
		return true;
	}

	// Token: 0x060031CC RID: 12748 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CountsTowardsSuperJumpAchievement()
	{
		return false;
	}

	// Token: 0x060031CD RID: 12749 RVA: 0x00004AE6 File Offset: 0x00002CE6
	public bool CanBeChargeFlamed()
	{
		return true;
	}

	// Token: 0x060031CE RID: 12750 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CanBeChargeDashed()
	{
		return false;
	}

	// Token: 0x060031CF RID: 12751 RVA: 0x00004AE6 File Offset: 0x00002CE6
	public bool CanBeGrenaded()
	{
		return true;
	}

	// Token: 0x060031D0 RID: 12752 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool CountsTowardsPowerOfLightAchievement()
	{
		return false;
	}

	// Token: 0x060031D1 RID: 12753 RVA: 0x00002C3A File Offset: 0x00000E3A
	public bool IsDead()
	{
		return false;
	}

	// Token: 0x060031D2 RID: 12754 RVA: 0x000030E7 File Offset: 0x000012E7
	public void OnGoThroughPortal()
	{
	}

	// Token: 0x060031D3 RID: 12755 RVA: 0x000030E7 File Offset: 0x000012E7
	public void OnPortalOverlapEnter()
	{
	}

	// Token: 0x060031D4 RID: 12756 RVA: 0x000030E7 File Offset: 0x000012E7
	public void OnPortalOverlapExit()
	{
	}

	// Token: 0x060031D5 RID: 12757 RVA: 0x00004AE6 File Offset: 0x00002CE6
	public bool CanBeReflected(float maximumReflectableDamage)
	{
		return true;
	}

	// Token: 0x060031D6 RID: 12758 RVA: 0x000030E7 File Offset: 0x000012E7
	public void OnGrabbed()
	{
	}

	// Token: 0x060031D7 RID: 12759 RVA: 0x000030E7 File Offset: 0x000012E7
	public void OnReleased(float speed, Vector3 direction)
	{
	}

	// Token: 0x060031D8 RID: 12760 RVA: 0x000D038C File Offset: 0x000CE58C
	public void OnDamageDealt(GameObject go, Damage damage)
	{
		if (go == this.Owner)
		{
			return;
		}
		IProjectileDetonatable projectileDetonatable = go.FindComponent<IProjectileDetonatable>();
		if (projectileDetonatable != null && projectileDetonatable.CanDetonateProjectiles())
		{
			this.ExplodeProjectile();
		}
	}

	// Token: 0x060031D9 RID: 12761 RVA: 0x000D03CC File Offset: 0x000CE5CC
	public virtual void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			this.Rigidbody.velocity = Vector3.zero;
			return;
		}
		if (this.EnableCollisionGracePeriod && this.CurrentTime > this.CollisionGracePeriod)
		{
			this.m_collider.enabled = true;
		}
		if (this.m_lastLoop == null && this.ProjectileLoop != null)
		{
			this.m_lastLoop = Sound.Play(this.ProjectileLoop.GetSound(null), base.transform.position, this.m_nullify);
			if (this.m_lastLoop)
			{
				this.m_lastLoop.AttachTo = base.transform;
			}
		}
		this.CurrentTime += Time.deltaTime;
		if (this.CurrentTime > this.MaximumLiveTime)
		{
			this.m_explode = true;
		}
		if (WaterZone.PositionInWater(this.Position))
		{
			this.m_explode = true;
		}
		if (this.Gravity > 0f)
		{
			this.SpeedVector += RandomizerBonusSkill.TimeScale(Vector3.down * this.Gravity * Time.fixedDeltaTime);
		}
		this.UpdateVelocity();
		if (this.RotateSpriteToDirection)
		{
			float num = base.transform.eulerAngles.z;
			num = Mathf.MoveTowardsAngle(num, MoonMath.Angle.AngleFromDirection(this.Direction), this.SpriteTurnSpeed * Time.deltaTime);
			base.transform.eulerAngles = new Vector3(0f, 0f, num);
		}
		if (this.m_explode)
		{
			this.ExplodeProjectile();
		}
		if (this.m_explodeLater)
		{
			this.m_explode = true;
			this.m_explodeLater = false;
		}
	}

	// Token: 0x060031DA RID: 12762 RVA: 0x000D0594 File Offset: 0x000CE794
	public void ExplodeProjectile()
	{
		if (this.m_lastLoop)
		{
			this.m_lastLoop.FadeOut(0.3f, true);
		}
		for (int i = 0; i < this.m_onKillRecievers.Length; i++)
		{
			if (this.m_onKillRecievers[i])
			{
				((IKillReciever)this.m_onKillRecievers[i]).OnKill();
			}
		}
		InstantiateUtility.Destroy(base.gameObject);
	}

	// Token: 0x060031DB RID: 12763 RVA: 0x0002808A File Offset: 0x0002628A
	public void OnCollisionEnter(Collision collision)
	{
		this.m_explodeLater = true;
	}

	// Token: 0x060031DC RID: 12764 RVA: 0x00028093 File Offset: 0x00026293
	public void OnCollisionStay(Collision collision)
	{
		this.m_explode = true;
	}

	// Token: 0x060031DD RID: 12765 RVA: 0x000D060C File Offset: 0x000CE80C
	public void UpdateVelocity()
	{
		Vector3 vector = -Vector3.ClampMagnitude(this.Displacement / Time.deltaTime, 10f);
		this.Displacement += vector * Time.deltaTime;
		this.Rigidbody.velocity = RandomizerBonusSkill.TimeScale(this.Direction * this.Speed + vector);
	}

	// Token: 0x170007D6 RID: 2006
	// (get) Token: 0x060031DE RID: 12766 RVA: 0x0002809C File Offset: 0x0002629C
	// (set) Token: 0x060031DF RID: 12767 RVA: 0x000280AF File Offset: 0x000262AF
	public Vector3 SpeedVector
	{
		get
		{
			return this.Speed * this.Direction;
		}
		set
		{
			this.Speed = value.magnitude;
			this.Direction = value.normalized;
		}
	}

	// Token: 0x060031E0 RID: 12768 RVA: 0x000D0678 File Offset: 0x000CE878
	public void UpdateSpeedAndDirection()
	{
		this.Direction = this.Rigidbody.velocity.normalized;
		this.Speed = RandomizerBonusSkill.TimeScale(this.Rigidbody.velocity.magnitude);
	}

	// Token: 0x04002D0E RID: 11534
	public GameObject Owner;

	// Token: 0x04002D0F RID: 11535
	public bool CanProjectileBeBashed = true;

	// Token: 0x04002D10 RID: 11536
	public float CollisionGracePeriod = 0.5f;

	// Token: 0x04002D11 RID: 11537
	public bool EnableCollisionGracePeriod;

	// Token: 0x04002D12 RID: 11538
	public float Gravity;

	// Token: 0x04002D13 RID: 11539
	public float MaximumLiveTime = 5f;

	// Token: 0x04002D14 RID: 11540
	public SoundProvider ProjectileLoop;

	// Token: 0x04002D15 RID: 11541
	public float BashSpeed = 20f;

	// Token: 0x04002D16 RID: 11542
	public bool UseBashSpeed;

	// Token: 0x04002D17 RID: 11543
	public bool CancelGravityOnBash;

	// Token: 0x04002D18 RID: 11544
	public bool RotateSpriteToDirection;

	// Token: 0x04002D19 RID: 11545
	public float SpriteTurnSpeed = 360f;

	// Token: 0x04002D1A RID: 11546
	[NonSerialized]
	public bool HasBeenBashedByOri;

	// Token: 0x04002D1B RID: 11547
	protected float CurrentTime;

	// Token: 0x04002D1C RID: 11548
	private float m_originalGravity;

	// Token: 0x04002D1D RID: 11549
	private SoundPlayer m_lastLoop;

	// Token: 0x04002D1E RID: 11550
	private bool m_explode;

	// Token: 0x04002D1F RID: 11551
	private bool m_explodeLater;

	// Token: 0x04002D20 RID: 11552
	private Action m_nullify;

	// Token: 0x04002D21 RID: 11553
	protected Rigidbody Rigidbody;

	// Token: 0x04002D22 RID: 11554
	private Collider m_collider;

	// Token: 0x04002D23 RID: 11555
	[SerializeField]
	[HideInInspector]
	private Component[] m_onKillRecievers;
}
