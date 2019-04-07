using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

// Token: 0x02000021 RID: 33
public class ChargeFlameBurst : MonoBehaviour, IPooled, ISuspendable
{
	// Token: 0x060000C0 RID: 192 RVA: 0x00002BDB File Offset: 0x00000DDB
	public void OnPoolSpawned()
	{
		this.m_suspended = false;
		this.m_simultaneousEnemies = 0;
		this.m_time = 0f;
		this.m_waitDelay = 0f;
	}

	// Token: 0x060000C1 RID: 193 RVA: 0x00002C01 File Offset: 0x00000E01
	public static void IgnoreOnLastInstance(IAttackable attackable)
	{
		if (ChargeFlameBurst.m_lastInstance)
		{
			ChargeFlameBurst.m_lastInstance.m_damageAttackables.Add(attackable);
		}
	}

	// Token: 0x060000C2 RID: 194 RVA: 0x00002AE2 File Offset: 0x00000CE2
	public void Awake()
	{
		SuspensionManager.Register(this);
	}

	// Token: 0x060000C3 RID: 195 RVA: 0x000023E2 File Offset: 0x000005E2
	public void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	// Token: 0x060000C4 RID: 196 RVA: 0x00002C23 File Offset: 0x00000E23
	public void OnEnable()
	{
		ChargeFlameBurst.m_lastInstance = this;
	}

	// Token: 0x060000C5 RID: 197 RVA: 0x00002C2B File Offset: 0x00000E2B
	public void OnDisable()
	{
		this.m_damageAttackables.Clear();
		if (ChargeFlameBurst.m_lastInstance == this)
		{
			ChargeFlameBurst.m_lastInstance = null;
		}
	}

	// Token: 0x060000C6 RID: 198 RVA: 0x00002C4E File Offset: 0x00000E4E
	public void Start()
	{
		this.DealDamage();
		this.m_time = 0f;
		this.m_simultaneousEnemies = 0;
		this.m_waitDelay = 0f;
	}

	// Token: 0x060000C7 RID: 199
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

	// Token: 0x060000C8 RID: 200 RVA: 0x0002DE54 File Offset: 0x0002C054
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

	// Token: 0x17000021 RID: 33
	// (get) Token: 0x060000C9 RID: 201 RVA: 0x00002C73 File Offset: 0x00000E73
	// (set) Token: 0x060000CA RID: 202 RVA: 0x00002C7B File Offset: 0x00000E7B
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

	// Token: 0x040000F5 RID: 245
	public float BurstRadius = 5f;

	// Token: 0x040000F6 RID: 246
	public float DamageAmount = 10f;

	// Token: 0x040000F7 RID: 247
	public GameObject BurstImpactEffectPrefab;

	// Token: 0x040000F8 RID: 248
	public float DealDamageDuration = 0.5f;

	// Token: 0x040000F9 RID: 249
	private float m_time;

	// Token: 0x040000FA RID: 250
	private float m_waitDelay;

	// Token: 0x040000FB RID: 251
	private readonly HashSet<IAttackable> m_damageAttackables = new HashSet<IAttackable>();

	// Token: 0x040000FC RID: 252
	private int m_simultaneousEnemies;

	// Token: 0x040000FD RID: 253
	private static ChargeFlameBurst m_lastInstance;

	// Token: 0x040000FE RID: 254
	private bool m_suspended;
}
