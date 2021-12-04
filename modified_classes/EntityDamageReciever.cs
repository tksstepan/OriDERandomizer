using System;
using Game;
using UnityEngine;
using Core;
public class EntityDamageReciever : DamageReciever, IDynamicGraphicHierarchy, IProjectileDetonatable
{
	public new void OnValidate()
	{
		this.Entity = base.transform.FindComponentUpwards<Entity>();
		this.Entity.DamageReciever = this;
		base.OnValidate();
	}

	public new void Awake()
	{
		base.Awake();
		if (this.Entity == null)
		{
			this.OnValidate();
		}
	}

	public override GameObject DisableTarget
	{
		get
		{
			return this.Entity.gameObject;
		}
	}

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

	public void OnTriggerEnter(Collider collider)
	{
		if (this.CanBeCrushed && collider.GetComponent<CrushPlayer>())
		{
			Damage damage = new Damage(10000f, Vector2.zero, this.Entity.Position, DamageType.Crush, base.gameObject);
			damage.DealToComponents(base.gameObject);
		}
	}

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
				RandomizerLocationManager.GivePickup(this.Entity.MoonGuid);
			}
		}
	}

	public bool CanDetonateProjectiles()
	{
		return this.IgnoreDamageCondition == null || !this.IgnoreDamageCondition(null);
	}

	public Entity Entity;

	public EntityDamageReciever.ModifyDamageDelegate OnModifyDamage = delegate
	{
	};

	public static Action<Entity> OnEntityDeathEvent = delegate
	{
	};

	public bool CanBeCrushed = true;

	public delegate void ModifyDamageDelegate(Damage d);
}
