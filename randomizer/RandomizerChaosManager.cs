using System;
using System.Collections.Generic;
using UnityEngine;

public static class RandomizerChaosManager
{
	public static void initialize()
	{
		RandomizerChaosManager.Countdown = UnityEngine.Random.Range(300, 1800);
		RandomizerChaosManager.Effects = new List<RandomizerChaosEffect>();
		RandomizerChaosManager.Frequencies = new List<int>();
		RandomizerChaosManager.Effects.Add(new RandomizerChaosMovementSpeed());
		RandomizerChaosManager.Frequencies.Add(19);
		RandomizerChaosManager.Effects.Add(new RandomizerChaosGravity());
		RandomizerChaosManager.Frequencies.Add(13);
		RandomizerChaosManager.Effects.Add(new RandomizerChaosTeleport());
		RandomizerChaosManager.Frequencies.Add(5);
		RandomizerChaosManager.Effects.Add(new RandomizerChaosZoom());
		RandomizerChaosManager.Frequencies.Add(5);
		RandomizerChaosManager.Effects.Add(new RandomizerChaosPoison());
		RandomizerChaosManager.Frequencies.Add(1);
		RandomizerChaosManager.Effects.Add(new RandomizerChaosColor());
		RandomizerChaosManager.Frequencies.Add(5);
		RandomizerChaosManager.Effects.Add(new RandomizerChaosDamageModifier());
		RandomizerChaosManager.Frequencies.Add(5);
		RandomizerChaosManager.Effects.Add(new RandomizerChaosVelocityVector());
		RandomizerChaosManager.Frequencies.Add(11);
	}

	public static void Update()
	{
		RandomizerChaosManager.Countdown--;
		if (RandomizerChaosManager.Countdown <= 0)
		{
			RandomizerChaosManager.SpawnEffect();
			RandomizerChaosManager.Countdown = UnityEngine.Random.Range(300, 900);
		}
		for (int i = 0; i < RandomizerChaosManager.Effects.Count; i++)
		{
			RandomizerChaosManager.Effects[i].Update();
		}
	}

	public static void ClearEffects()
	{
		for (int i = 0; i < RandomizerChaosManager.Effects.Count; i++)
		{
			RandomizerChaosManager.Effects[i].Clear();
		}
	}

	public static void SpawnEffect()
	{
		int num = 0;
		int num2 = UnityEngine.Random.Range(0, 64);
		for (int i = 0; i < RandomizerChaosManager.Effects.Count; i++)
		{
			num += RandomizerChaosManager.Frequencies[i];
			if (num > num2)
			{
				RandomizerChaosManager.Effects[i].Start();
				return;
			}
		}
	}

	public static int Countdown;

	public static List<RandomizerChaosEffect> Effects;

	public static List<int> Frequencies;
}
