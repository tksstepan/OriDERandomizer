using System;
using Game;
using UnityEngine;

public class RandomizerChaosTeleport : RandomizerChaosEffect
{
	public override void Clear()
	{
		this.Countdown = 0;
	}

	public override void Start()
	{
		this.Countdown = UnityEngine.Random.Range(360, 3600);
		int num = UnityEngine.Random.Range(0, 16);
		Randomizer.showChaosEffect("Teleportation");
		if (num <= 14)
		{
			this.TeleportCount = UnityEngine.Random.Range(1, 10);
			return;
		}
		if (num <= 15)
		{
			this.Teleport(200f);
		}
	}

	public override void Update()
	{
		if (this.Countdown > 0)
		{
			if (UnityEngine.Random.Range(0f, 1f) < (float)this.TeleportCount / (float)this.Countdown)
			{
				this.Teleport(10f);
			}
			this.Countdown--;
			if (this.Countdown == 0)
			{
				this.Clear();
			}
		}
	}

	public void Teleport(float range)
	{
		Characters.Sein.Position = new Vector3(Characters.Sein.Position.x + UnityEngine.Random.Range(-range, range), Characters.Sein.Position.y + UnityEngine.Random.Range(-range, range));
		this.TeleportCount--;
	}

	public int Countdown;

	public int TeleportCount;
}
