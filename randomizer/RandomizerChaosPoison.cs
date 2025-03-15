using System;
using Game;
using UnityEngine;

public class RandomizerChaosPoison : RandomizerChaosEffect
{
	public override void Clear()
	{
		this.Countdown = 0;
	}

	public override void Start()
	{
		Randomizer.showChaosEffect("Poison");
		this.Countdown = UnityEngine.Random.Range(1200, 3600);
		this.DamageRate = UnityEngine.Random.Range(0.5f, 2f) * (float)Characters.Sein.Mortality.Health.MaxHealth / (float)this.Countdown;
	}

	public override void Update()
	{
		if (this.Countdown > 0)
		{
			this.Countdown--;
			Characters.Sein.Mortality.Health.LoseHealth(this.DamageRate);
			if (Characters.Sein.Mortality.Health.Amount <= 0f)
			{
				Characters.Sein.Mortality.DamageReciever.OnRecieveDamage(new Damage(1f, default(Vector2), default(Vector3), DamageType.Water, null));
			}
			if (this.Countdown == 0)
			{
				this.Clear();
			}
		}
	}

	public int Countdown;

	public float DamageRate;
}
