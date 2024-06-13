using System;
using Game;
using UnityEngine;

public class SpiritGrenade : MonoBehaviour, IDamageReciever, IAttackable, IBashAttackable, ISuspendable
{
	public bool IsInsideSpiritTorch
	{
		get
		{
			return this.m_ignitableTorch != null;
		}
	}

	public void Awake()
	{
		DamageDealer damageDealer = this.DamageDealer;
		damageDealer.OnDamageDealtEvent = (Action<GameObject, Damage>)Delegate.Combine(damageDealer.OnDamageDealtEvent, new Action<GameObject, Damage>(this.OnDamageDealt));
		DamageDealer damageDealer2 = this.DamageDealer;
		damageDealer2.ShouldDealDamage = (Func<GameObject, bool>)Delegate.Combine(damageDealer2.ShouldDealDamage, new Func<GameObject, bool>(this.ShouldDealDamage));
		SuspensionManager.Register(this);
		Targets.Attackables.Add(this);
		this.m_rigidbody = base.GetComponent<Rigidbody>();
	}

	public void Start()
	{
		this.m_time = 0f;
	}

	public void OnDestroy()
	{
		DamageDealer damageDealer = this.DamageDealer;
		damageDealer.OnDamageDealtEvent = (Action<GameObject, Damage>)Delegate.Remove(damageDealer.OnDamageDealtEvent, new Action<GameObject, Damage>(this.OnDamageDealt));
		DamageDealer damageDealer2 = this.DamageDealer;
		damageDealer2.ShouldDealDamage = (Func<GameObject, bool>)Delegate.Remove(damageDealer2.ShouldDealDamage, new Func<GameObject, bool>(this.ShouldDealDamage));
		SuspensionManager.Unregister(this);
		Targets.Attackables.Remove(this);
	}

	public bool ShouldDealDamage(GameObject target)
	{
		if (this.IsInsideSpiritTorch)
		{
			return false;
		}
		IAttackable attackable = target.FindComponent<IAttackable>();
		return attackable as Component && attackable.CanBeGrenaded();
	}

	public void OnDamageDealt(GameObject go, Damage damage)
	{
		if (!this.IsInsideSpiritTorch && !go.GetComponent<Projectile>())
		{
			this.Explode();
		}
	}

	public void Explode()
	{
		InstantiateUtility.Destroy(base.gameObject);
		InstantiateUtility.Instantiate(this.Explosion, base.transform.position, Quaternion.identity);
	}

	public void SetTrajectory(Vector2 speed)
	{
		Rigidbody component = base.GetComponent<Rigidbody>();
		component.velocity = speed;
	}

	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		this.m_time += Time.deltaTime;
		if (this.IsInsideSpiritTorch)
		{
			this.m_rigidbody.velocity = (this.m_ignitableTorch.Position - this.m_rigidbody.position + new Vector3(0.2f, 0.4f)) * 8f;
			if (this.m_time > 0.8f)
			{
				InstantiateUtility.Destroy(base.gameObject);
			}
		}
		else
		{
			this.m_rigidbody.velocity += Vector3.down * this.Gravity * Time.deltaTime;
			IgnitableSpiritTorch ignitableSpiritTorch = IgnitableSpiritTorch.IgniteAnyTorchesNearPosition(base.transform.position);
			if (ignitableSpiritTorch)
			{
				this.m_ignitableTorch = ignitableSpiritTorch;
				this.m_time = 0f;
				return;
			}
			if (this.m_time > this.Duration)
			{
				this.Explode();
			}
		}
		if (WaterZone.PositionInWater(this.Position))
		{
			this.m_rigidbody.velocity *= 0.9f;
		}
	}

	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
	}

	public bool IsDead()
	{
		return base.gameObject.activeSelf;
	}

	public bool CanBeChargeFlamed()
	{
		return false;
	}

	public bool CanBeChargeDashed()
	{
		return false;
	}

	public bool CanBeGrenaded()
	{
		return false;
	}

	public bool CanBeStomped()
	{
		return false;
	}

	public bool CanBeBashed()
	{
		return !this.IsInsideSpiritTorch && this.Bashable;
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
			return 100;
		}
	}

	public bool IsSuspended { get; set; }

	public void OnSpring(float height, Vector2 direction)
	{
		this.m_rigidbody.velocity = direction * MoonMath.Physics.SpeedFromHeightAndGravity(this.Gravity, height);
	}

	public void OnRecieveDamage(Damage damage)
	{
		if (damage.Type == DamageType.Spikes || damage.Type == DamageType.Lava || damage.Type == DamageType.Laser || damage.Type == DamageType.Bash)
		{
			this.Explode();
		}
	}

	public float Gravity;

	public DamageDealer DamageDealer;

	public GameObject Explosion;

	public float Duration = 4f;

	private float m_time;

	private Rigidbody m_rigidbody;

	private IgnitableSpiritTorch m_ignitableTorch;

	public bool Bashable = true;
}
