using System;
using Game;
using UnityEngine;

public class RandomizerChaosVelocityVector : RandomizerChaosEffect
{
	public override void Clear()
	{
		this.Countdown = 0;
		this.Pushing = false;
	}

	public override void Start()
	{
		this.Countdown = UnityEngine.Random.Range(1, 300);
		this.Pushing = false;
		int num = UnityEngine.Random.Range(0, 8);
		if (num <= 5)
		{
			Randomizer.showChaosEffect("Throw");
			Characters.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedX = UnityEngine.Random.Range(-100f, 100f);
			Characters.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedY = UnityEngine.Random.Range(-100f, 100f);
			return;
		}
		if (num <= 7)
		{
			Randomizer.showChaosEffect("Push");
			this.Pushing = true;
			this.Push = new Vector2(UnityEngine.Random.Range(-40f, 40f), UnityEngine.Random.Range(-40f, 40f));
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
			if (this.Pushing)
			{
				Characters.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedX = this.Push.x;
				Characters.Sein.PlatformBehaviour.PlatformMovement.LocalSpeedY = this.Push.y;
			}
		}
	}

	public int Countdown;

	public bool Pushing;

	public Vector2 Push;
}
