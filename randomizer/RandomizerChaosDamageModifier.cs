using System;
using UnityEngine;

public class RandomizerChaosDamageModifier : RandomizerChaosEffect
{
	public override void Clear()
	{
		this.Countdown = 0;
		Randomizer.DamageModifier = 1f;
	}

	public override void Start()
	{
		this.Countdown = UnityEngine.Random.Range(360, 3600);
		int num = UnityEngine.Random.Range(0, 8);
		if (num <= 3)
		{
			Randomizer.showChaosEffect("Damage vulnerability");
			Randomizer.DamageModifier = UnityEngine.Random.Range(1.5f, 4f);
			return;
		}
		if (num <= 6)
		{
			Randomizer.showChaosEffect("Damage reduction");
			Randomizer.DamageModifier = UnityEngine.Random.Range(0.25f, 0.8f);
			return;
		}
		if (num <= 7)
		{
			Randomizer.showChaosEffect("Invulnerability");
			Randomizer.DamageModifier = 0f;
		}
	}

	public override void Update()
	{
		if (this.Countdown > 0)
		{
			this.Countdown--;
			if (this.Countdown == 0)
			{
				this.Clear();
			}
		}
	}

	public int Countdown;
}
