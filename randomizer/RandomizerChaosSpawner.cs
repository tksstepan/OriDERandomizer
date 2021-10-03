using System;

public class RandomizerChaosSpawner : RandomizerChaosEffect
{
	public override void Clear()
	{
		this.Countdown = 0;
	}

	public override void Start()
	{
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
