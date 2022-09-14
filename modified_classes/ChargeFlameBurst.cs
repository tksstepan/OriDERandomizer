using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class ChargeFlameBurst : MonoBehaviour, IPooled, ISuspendable
{
	public void OnPoolSpawned()
	{
		this.m_suspended = false;
		this.m_simultaneousEnemies = 0;
		this.m_time = 0f;
		this.m_waitDelay = 0f;
	}

	public static void IgnoreOnLastInstance(IAttackable attackable)
	{
		if (ChargeFlameBurst.m_lastInstance)
		{
			ChargeFlameBurst.m_lastInstance.m_damageAttackables.Add(attackable);
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
		ChargeFlameBurst.m_lastInstance = this;
	}

	public void OnDisable()
	{
		this.m_damageAttackables.Clear();
		if (ChargeFlameBurst.m_lastInstance == this)
		{
			ChargeFlameBurst.m_lastInstance = null;
		}
	}

	public void Start()
	{
		this.DealDamage();
		this.m_time = 0f;
		this.m_simultaneousEnemies = 0;
		this.m_waitDelay = 0f;
	}

	public void DealDamage()
	{
		Vector3 position = base.transform.position;
		IAttackable[] array = Targets.Attackables.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			IAttackable attackable = array[i];
			if (!InstantiateUtility.IsDestroyed(attackable as Component) && !this.m_damageAttackables.Contains(attackable) && attackable.CanBeChargeFlamed())
			{
				Vector3 position2 = attackable.Position;
				Vector3 vector = position2 - position;
				if (vector.magnitude <= this.BurstRadius)
				{
					this.m_damageAttackables.Add(attackable);
					GameObject gameObject = ((Component)attackable).gameObject;
					new Damage(this.DamageAmount + (float)(6 * RandomizerBonus.SpiritFlameLevel()), vector.normalized * 3f, position, DamageType.ChargeFlame, base.gameObject).DealToComponents(gameObject);
					bool expr_D8 = attackable.IsDead();
					if (!expr_D8)
					{
						GameObject expr_F2 = (GameObject)InstantiateUtility.Instantiate(this.BurstImpactEffectPrefab, position2, Quaternion.identity);
						expr_F2.transform.eulerAngles = new Vector3(0f, 0f, MoonMath.Angle.AngleFromVector(vector.normalized));
						expr_F2.GetComponent<FollowPositionRotation>().SetTarget(gameObject.transform);
					}
					if (expr_D8 && attackable is IChargeFlameAttackable && ((IChargeFlameAttackable)attackable).CountsTowardsPowerOfLightAchievement())
					{
						this.m_simultaneousEnemies++;
					}
				}
			}
		}
		if (this.m_simultaneousEnemies >= 4)
		{
			AchievementsController.AwardAchievement(Characters.Sein.Abilities.ChargeFlame.KillEnemiesSimultaneouslyAchievement);
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

	private int m_simultaneousEnemies;

	private static ChargeFlameBurst m_lastInstance;

	private bool m_suspended;
}
