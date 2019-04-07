using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

// Token: 0x0200001F RID: 31
public class GrenadeBurst : MonoBehaviour, IPooled, ISuspendable
{
	// Token: 0x060000AC RID: 172 RVA: 0x00002AA1 File Offset: 0x00000CA1
	public void OnPoolSpawned()
	{
		this.m_suspended = false;
		this.m_time = 0f;
		this.m_waitDelay = 0f;
	}

	// Token: 0x060000AD RID: 173 RVA: 0x00002AC0 File Offset: 0x00000CC0
	public static void IgnoreOnLastInstance(IAttackable attackable)
	{
		if (GrenadeBurst.m_lastInstance)
		{
			GrenadeBurst.m_lastInstance.m_damageAttackables.Add(attackable);
		}
	}

	// Token: 0x060000AE RID: 174 RVA: 0x00002AE2 File Offset: 0x00000CE2
	public void Awake()
	{
		SuspensionManager.Register(this);
	}

	// Token: 0x060000AF RID: 175 RVA: 0x000023E2 File Offset: 0x000005E2
	public void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	// Token: 0x060000B0 RID: 176 RVA: 0x00002AEA File Offset: 0x00000CEA
	public void OnEnable()
	{
		GrenadeBurst.m_lastInstance = this;
	}

	// Token: 0x060000B1 RID: 177 RVA: 0x00002AF2 File Offset: 0x00000CF2
	public void OnDisable()
	{
		this.m_damageAttackables.Clear();
		if (GrenadeBurst.m_lastInstance == this)
		{
			GrenadeBurst.m_lastInstance = null;
		}
	}

	// Token: 0x060000B2 RID: 178 RVA: 0x00002B15 File Offset: 0x00000D15
	public void Start()
	{
		this.DealDamage();
		this.m_time = 0f;
		this.m_waitDelay = 0f;
	}

	// Token: 0x060000B3 RID: 179
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

	// Token: 0x060000B4 RID: 180 RVA: 0x0002DAC0 File Offset: 0x0002BCC0
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

	// Token: 0x17000020 RID: 32
	// (get) Token: 0x060000B5 RID: 181 RVA: 0x00002B33 File Offset: 0x00000D33
	// (set) Token: 0x060000B6 RID: 182 RVA: 0x00002B3B File Offset: 0x00000D3B
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

	// Token: 0x040000E4 RID: 228
	public float BurstRadius = 5f;

	// Token: 0x040000E5 RID: 229
	public float DamageAmount = 10f;

	// Token: 0x040000E6 RID: 230
	public GameObject BurstImpactEffectPrefab;

	// Token: 0x040000E7 RID: 231
	public float DealDamageDuration = 0.5f;

	// Token: 0x040000E8 RID: 232
	private float m_time;

	// Token: 0x040000E9 RID: 233
	private float m_waitDelay;

	// Token: 0x040000EA RID: 234
	private readonly HashSet<IAttackable> m_damageAttackables = new HashSet<IAttackable>();

	// Token: 0x040000EB RID: 235
	private static GrenadeBurst m_lastInstance;

	// Token: 0x040000EC RID: 236
	private bool m_suspended;
}
