using System;
using Core;
using Game;
using UnityEngine;

public class Projectile : MonoBehaviour, IDamageReciever, IAttackable, IChargeFlameAttackable, IStompAttackable, IBashAttackable, IPooled, ISuspendable, IPortalVisitor, IReflectable
{
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

	public Vector3 Direction { get; set; }

	public float Speed { get; set; }

	public GameObject LastReflector { get; set; }

	public Vector3 Displacement { get; set; }

	public bool IsSuspended { get; set; }

	public void OnValidate()
	{
		this.m_onKillRecievers = base.GetComponentsInChildren(typeof(IKillReciever));
	}

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

	public void OnEnable()
	{
		Targets.Attackables.Add(this);
		PortalVistor.All.Add(this);
		this.CurrentTime = 0f;
		this.Rigidbody.velocity = Vector3.zero;
	}

	public void OnDisable()
	{
		Targets.Attackables.Remove(this);
		PortalVistor.All.Remove(this);
	}

	public void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	public virtual bool CanBeBashed()
	{
		return this.CanProjectileBeBashed;
	}

	public bool CanBeSpiritFlamed()
	{
		return false;
	}

	public bool IsStompBouncable()
	{
		return false;
	}

	public bool CanBeLevelUpBlasted()
	{
		return false;
	}

	public void OnEnterBash()
	{
		if (this.m_lastLoop)
		{
			this.m_lastLoop.FadeOut(0.3f, true);
		}
	}

	public void OnBashHighlight()
	{
	}

	public void OnBashDehighlight()
	{
	}

	public int BashPriority
	{
		get
		{
			return 40;
		}
	}

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

	public bool CanBeStomped()
	{
		return true;
	}

	public bool CountsTowardsSuperJumpAchievement()
	{
		return false;
	}

	public bool CanBeChargeFlamed()
	{
		return true;
	}

	public bool CanBeChargeDashed()
	{
		return false;
	}

	public bool CanBeGrenaded()
	{
		return true;
	}

	public bool CountsTowardsPowerOfLightAchievement()
	{
		return false;
	}

	public bool IsDead()
	{
		return false;
	}

	public void OnGoThroughPortal()
	{
	}

	public void OnPortalOverlapEnter()
	{
	}

	public void OnPortalOverlapExit()
	{
	}

	public bool CanBeReflected(float maximumReflectableDamage)
	{
		return true;
	}

	public void OnGrabbed()
	{
	}

	public void OnReleased(float speed, Vector3 direction)
	{
	}

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

	public void OnCollisionEnter(Collision collision)
	{
		this.m_explodeLater = true;
	}

	public void OnCollisionStay(Collision collision)
	{
		this.m_explode = true;
	}

	public void UpdateVelocity()
	{
		Vector3 vector = -Vector3.ClampMagnitude(this.Displacement / Time.deltaTime, 10f);
		this.Displacement += vector * Time.deltaTime;
		this.Rigidbody.velocity = RandomizerBonusSkill.TimeScale(this.Direction * this.Speed + vector);
	}

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

	public void UpdateSpeedAndDirection()
	{
		this.Direction = this.Rigidbody.velocity.normalized;
		this.Speed = RandomizerBonusSkill.TimeScale(this.Rigidbody.velocity.magnitude);
	}

	public GameObject Owner;

	public bool CanProjectileBeBashed = true;

	public float CollisionGracePeriod = 0.5f;

	public bool EnableCollisionGracePeriod;

	public float Gravity;

	public float MaximumLiveTime = 5f;

	public SoundProvider ProjectileLoop;

	public float BashSpeed = 20f;

	public bool UseBashSpeed;

	public bool CancelGravityOnBash;

	public bool RotateSpriteToDirection;

	public float SpriteTurnSpeed = 360f;

	[NonSerialized]
	public bool HasBeenBashedByOri;

	protected float CurrentTime;

	private float m_originalGravity;

	private SoundPlayer m_lastLoop;

	private bool m_explode;

	private bool m_explodeLater;

	private Action m_nullify;

	protected Rigidbody Rigidbody;

	private Collider m_collider;

	[SerializeField]
	[HideInInspector]
	private Component[] m_onKillRecievers;
}
