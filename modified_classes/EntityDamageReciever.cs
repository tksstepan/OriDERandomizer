using System;
using Game;
using UnityEngine;
using Core;
// Token: 0x02000422 RID: 1058
public class EntityDamageReciever : DamageReciever, IDynamicGraphicHierarchy, IProjectileDetonatable
{
	// Token: 0x060017A7 RID: 6055 RVA: 0x000147C6 File Offset: 0x000129C6
	public new void OnValidate()
	{
		this.Entity = base.transform.FindComponentUpwards<Entity>();
		this.Entity.DamageReciever = this;
		base.OnValidate();
	}

	// Token: 0x060017A8 RID: 6056 RVA: 0x000147EB File Offset: 0x000129EB
	public new void Awake()
	{
		base.Awake();
		if (this.Entity == null)
		{
			this.OnValidate();
		}
	}

	// Token: 0x1700040B RID: 1035
	// (get) Token: 0x060017A9 RID: 6057 RVA: 0x0001480A File Offset: 0x00012A0A
	public override GameObject DisableTarget
	{
		get
		{
			return this.Entity.gameObject;
		}
	}

	// Token: 0x060017AA RID: 6058 RVA: 0x0007AA4C File Offset: 0x00078C4C
	public override void OnPoolSpawned()
	{
		this.OnModifyDamage = delegate
		{
		};
		EntityDamageReciever.OnEntityDeathEvent = delegate
		{
		};
		base.OnPoolSpawned();
	}

	// Token: 0x060017AB RID: 6059 RVA: 0x0007AAA4 File Offset: 0x00078CA4
	public void OnTriggerEnter(Collider collider)
	{
		if (this.CanBeCrushed && collider.GetComponent<CrushPlayer>())
		{
			Damage damage = new Damage(10000f, Vector2.zero, this.Entity.Position, DamageType.Crush, base.gameObject);
			damage.DealToComponents(base.gameObject);
		}
	}

	// Token: 0x060017AC RID: 6060 RVA: 0x0007AAFC File Offset: 0x00078CFC
	public override void OnRecieveDamage(Damage damage)
	{
		bool terrain = (damage.Type == DamageType.Crush || damage.Type == DamageType.Spikes || damage.Type == DamageType.Lava || damage.Type == DamageType.Laser);
		if (this.Entity is Enemy && !(terrain || damage.Type == DamageType.Projectile || damage.Type == DamageType.Enemy))
		{
			RandomizerBonus.DamageDealt(damage.Amount);
		}
		this.OnModifyDamage(damage);
		if (damage.Type == DamageType.Enemy)
		{
			return;
		}
		if (damage.Type == DamageType.Projectile)
		{
			damage.SetAmount(damage.Amount * 4f);
		}
		if (damage.Type == DamageType.Spikes || damage.Type == DamageType.Lava)
		{
			damage.SetAmount(1000f);
		}
		if (this.Entity.gameObject != base.gameObject)
		{
			damage.DealToComponents(this.Entity.gameObject);
		}
		base.OnRecieveDamage(damage);
		if (base.NoHealthLeft)
		{
			EntityDamageReciever.OnEntityDeathEvent(this.Entity);
			if (damage.Type == DamageType.Projectile && this.Entity is Enemy)
			{
				Projectile component = damage.Sender.GetComponent<Projectile>();
				if (component != null && component.HasBeenBashedByOri)
				{
					AchievementsLogic.Instance.OnProjectileKilledEnemy();
				}
				if (component != null && !component.HasBeenBashedByOri)
				{
					AchievementsLogic.Instance.OnEnemyKilledAnotherEnemy();
				}
			}
			if (terrain)
			{
				Type type = this.Entity.GetType();
				if (type != typeof(DropSlugEnemy) && type != typeof(KamikazeSootEnemy) && !base.gameObject.name.ToLower().Contains("wall"))
				{
					AchievementsLogic.Instance.OnEnemyKilledItself();
				}
			}
			if (this.Entity is Enemy)
			{
				BingoController.OnDestroyEntity(this.Entity, damage);
				RandomizerStatsManager.OnKill(damage.Type);
				if (damage.Type == DamageType.ChargeFlame)
				{
					if (Characters.Sein && Characters.Sein.Abilities.Dash)
					{
						if (Characters.Sein.Abilities.Dash.CurrentState == SeinDashAttack.State.ChargeDashing)
						{
							AchievementsLogic.Instance.OnChargeDashKilledEnemy();
						}
						else
						{
							AchievementsLogic.Instance.OnChargeFlameKilledEnemy();
						}
					}
					else
					{
						AchievementsLogic.Instance.OnChargeFlameKilledEnemy();
					}
				}
				else if ((damage.Type == DamageType.Stomp && damage.Force.y < 0f) || damage.Type == DamageType.StompBlast)
				{
					AchievementsLogic.Instance.OnStompKilledEnemy();
				}
				else if (damage.Type == DamageType.SpiritFlameSplatter || damage.Type == DamageType.SpiritFlame)
				{
					AchievementsLogic.Instance.OnSpiritFlameKilledEnemy();
				}
				else if (damage.Type == DamageType.Grenade)
				{
					AchievementsLogic.Instance.OnGrenaedKilledEnemy();
				}
			}
			if (this.Entity is PetrifiedPlant)
			{
				Randomizer.getPickup(this.Entity.Position);
				RandomizerPlantManager.DestroyPlant(this.Entity.MoonGuid);
			}
		}
	}

	// Token: 0x060017AD RID: 6061 RVA: 0x00014817 File Offset: 0x00012A17
	public bool CanDetonateProjectiles()
	{
		return this.IgnoreDamageCondition == null || !this.IgnoreDamageCondition(null);
	}

	// Token: 0x040014FE RID: 5374
	public Entity Entity;

	// Token: 0x040014FF RID: 5375
	public EntityDamageReciever.ModifyDamageDelegate OnModifyDamage = delegate
	{
	};

	// Token: 0x04001500 RID: 5376
	public static Action<Entity> OnEntityDeathEvent = delegate
	{
	};

	// Token: 0x04001501 RID: 5377
	public bool CanBeCrushed = true;

	// Token: 0x02000423 RID: 1059
	// (Invoke) Token: 0x060017B3 RID: 6067
	public delegate void ModifyDamageDelegate(Damage d);
}
