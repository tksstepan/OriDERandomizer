using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class GrenadeBurst : MonoBehaviour, IPooled, ISuspendable
{
	public void OnPoolSpawned()
	{
		this.m_suspended = false;
		this.m_time = 0f;
		this.m_waitDelay = 0f;
	}

	public static void IgnoreOnLastInstance(IAttackable attackable)
	{
		if (GrenadeBurst.m_lastInstance)
		{
			GrenadeBurst.m_lastInstance.m_damageAttackables.Add(attackable);
		}
	}

	public void Awake()
	{
		SuspensionManager.Register(this);
	}

	public void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	public void OnEnable()
	{
		GrenadeBurst.m_lastInstance = this;
	}

	public void OnDisable()
	{
		this.m_damageAttackables.Clear();
		if (GrenadeBurst.m_lastInstance == this)
		{
			GrenadeBurst.m_lastInstance = null;
		}
	}

	public void Start()
	{
		this.DealDamage();
		this.m_time = 0f;
		this.m_waitDelay = 0f;
	}

	public void DealDamage()
	{
		Vector3 position = base.transform.position;
		foreach (IAttackable attackable in Targets.Attackables.ToArray())
		{
			if (!InstantiateUtility.IsDestroyed(attackable as Component) && !this.m_damageAttackables.Contains(attackable) && attackable.CanBeGrenaded())
			{
				Vector3 position2 = attackable.Position;
				Vector3 vector = position2 - position;
				if (vector.magnitude <= this.BurstRadius + (float)RandomizerBonus.SpiritFlameLevel())
				{
					this.m_damageAttackables.Add(attackable);
					GameObject gameObject = ((Component)attackable).gameObject;
					new Damage(this.DamageAmount + (float)(3 * RandomizerBonus.SpiritFlameLevel()), vector.normalized * 3f, position, DamageType.Grenade, base.gameObject).DealToComponents(gameObject);
					if (!attackable.IsDead())
					{
						GameObject gameObject2 = (GameObject)InstantiateUtility.Instantiate(this.BurstImpactEffectPrefab, position2, Quaternion.identity);
						gameObject2.transform.eulerAngles = new Vector3(0f, 0f, MoonMath.Angle.AngleFromVector(vector.normalized));
						gameObject2.GetComponent<FollowPositionRotation>().SetTarget(gameObject.transform);
					}
				}
			}
		}
		this.m_waitDelay = 0.1f;
	}

	public void FixedUpdate()
	{
		if (this.m_suspended)
		{
			return;
		}
		this.m_time += Time.deltaTime;
		this.m_waitDelay -= Time.deltaTime;
		if (this.m_time < this.DealDamageDuration && this.m_waitDelay <= 0f)
		{
			this.DealDamage();
		}
	}

	public bool IsSuspended
	{
		get
		{
			return this.m_suspended;
		}
		set
		{
			this.m_suspended = value;
		}
	}

	public float BurstRadius = 5f;

	public float DamageAmount = 10f;

	public GameObject BurstImpactEffectPrefab;

	public float DealDamageDuration = 0.5f;

	private float m_time;

	private float m_waitDelay;

	private readonly HashSet<IAttackable> m_damageAttackables = new HashSet<IAttackable>();

	private static GrenadeBurst m_lastInstance;

	private bool m_suspended;
}
