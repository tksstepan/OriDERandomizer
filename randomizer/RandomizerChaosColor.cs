using System;
using Game;
using UnityEngine;

public class RandomizerChaosColor : RandomizerChaosEffect
{
	public override void Clear()
	{
		this.Countdown = 0;
		this.Fading = false;
		if (this.Activated)
		{
			Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = new Color(this.InitialColor.r, this.InitialColor.g, this.InitialColor.b, 0.5f);
		}
	}

	public override void Start()
	{
		this.Activated = true;
		this.Fading = false;
		this.Countdown = UnityEngine.Random.Range(600, 3600);
		this.InitialColor = Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color;
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			Randomizer.showChaosEffect("Invisible Ori");
			Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = new Color(this.InitialColor.r, this.InitialColor.g, this.InitialColor.b, 0f);
			return;
		}
		Randomizer.showChaosEffect("Ghostly Ori");
		this.Fading = true;
		this.FadeRate = UnityEngine.Random.Range(0.5f, 2f) / (float)this.Countdown;
	}

	public override void Update()
	{
		if (this.Countdown > 0)
		{
			if (this.Fading)
			{
				Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = new Color(this.InitialColor.r, this.InitialColor.g, this.InitialColor.b, Math.Max(0f, Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color.a - this.FadeRate));
			}
			this.Countdown--;
			if (this.Countdown == 0)
			{
				this.Clear();
			}
		}
	}

	public int Countdown;

	public Color InitialColor;

	public float FadeRate;

	public bool Fading;

	public bool Activated;
}
