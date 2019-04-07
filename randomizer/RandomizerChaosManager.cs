using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020009F2 RID: 2546
public static class RandomizerChaosManager
{
	// Token: 0x0600375E RID: 14174
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

	// Token: 0x0600375F RID: 14175 RVA: 0x000E1414 File Offset: 0x000DF614
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

	// Token: 0x06003760 RID: 14176 RVA: 0x000E1474 File Offset: 0x000DF674
	public static void ClearEffects()
	{
		for (int i = 0; i < RandomizerChaosManager.Effects.Count; i++)
		{
			RandomizerChaosManager.Effects[i].Clear();
		}
	}

	// Token: 0x06003761 RID: 14177 RVA: 0x000E14A8 File Offset: 0x000DF6A8
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

	// Token: 0x04003241 RID: 12865
	public static int Countdown;

	// Token: 0x04003242 RID: 12866
	public static List<RandomizerChaosEffect> Effects;

	// Token: 0x04003243 RID: 12867
	public static List<int> Frequencies;
}
