using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class LevelUpDamageAction : ActionMethod, ISuspendable
{
	public override void Perform(IContext context)
	{
		this.m_active = true;
	}

	public override void Awake()
	{
		base.Awake();
		SuspensionManager.Register(this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SuspensionManager.Unregister(this);
	}

	public void FixedUpdate()
	{
		if (!this.m_active)
		{
			return;
		}
		this.m_time += Time.deltaTime;
		this.m_delayTime -= Time.deltaTime;
		if (this.m_delayTime < 0f)
		{
			this.m_delayTime = 0.1f;
			float num = this.DistanceOverTime.Evaluate(this.m_time);
			List<IAttackable> attackables = Targets.Attackables;
			for (int i = 0; i < attackables.Count; i++)
			{
				IAttackable attackable = attackables[i];
				if (!InstantiateUtility.IsDestroyed(attackable as Component) && !TeleporterController.IsTeleporting)
				{
					if (attackable.CanBeLevelUpBlasted())
					{
						if (!this.m_attackables.Contains(attackable))
						{
							if (Vector3.Distance(base.transform.position, attackable.Position) <= num)
							{
								this.m_attackables.Add(attackable);
								Damage damage = new Damage((float)this.Damage, (attackable.Position - base.transform.position).normalized, attackable.Position, DamageType.LevelUp, base.gameObject);
								damage.DealToComponents((attackable as Component).gameObject); 
							}
						}
					}
				}
			}
		}
		if (this.m_time > this.Duration)
		{
			this.m_active = false;
			this.m_time = 0f;
			this.m_attackables.Clear();
		}
	}

	public bool IsSuspended { get; set; }

	private readonly HashSet<IAttackable> m_attackables = new HashSet<IAttackable>();

	private bool m_active;

	private float m_time;

	public AnimationCurve DistanceOverTime;

	public float Duration;

	public int Damage;

	private float m_delayTime;
}
