using System;
using Game;
using UnityEngine;

public class RandomizerChaosZoom : RandomizerChaosEffect
{
	public override void Clear()
	{
		this.Countdown = 0;
		this.Zooming = false;
		UI.Cameras.Current.OffsetController.AdditiveDefaultOffset = new Vector3(UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.x, UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.y, this.InitialOffset);
	}

	public override void Start()
	{
		this.Zooming = false;
		if (this.Countdown == 0)
		{
			this.InitialOffset = UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.z;
		}
		this.Countdown = UnityEngine.Random.Range(360, 1200);
		int num = UnityEngine.Random.Range(0, 16);
		if (num <= 5)
		{
			Randomizer.showChaosEffect("Zoom in");
			float z = UnityEngine.Random.Range(-20f, -1f);
			UI.Cameras.Current.OffsetController.AdditiveDefaultOffset = new Vector3(UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.x, UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.y, z);
			return;
		}
		if (num <= 11)
		{
			Randomizer.showChaosEffect("Zoom out");
			float z2 = UnityEngine.Random.Range(1f, 100f);
			UI.Cameras.Current.OffsetController.AdditiveDefaultOffset = new Vector3(UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.x, UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.y, z2);
			return;
		}
		if (num <= 13)
		{
			Randomizer.showChaosEffect("Zoom in");
			this.Zooming = true;
			this.ZoomRate = UnityEngine.Random.Range(-20f, -1f) / (float)this.Countdown;
			return;
		}
		if (num <= 15)
		{
			Randomizer.showChaosEffect("Zoom out");
			this.Zooming = true;
			this.ZoomRate = UnityEngine.Random.Range(1f, 100f) / (float)this.Countdown;
		}
	}

	public override void Update()
	{
		if (this.Countdown > 0)
		{
			if (this.Zooming)
			{
				UI.Cameras.Current.OffsetController.AdditiveDefaultOffset = new Vector3(UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.x, UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.y, UI.Cameras.Current.OffsetController.AdditiveDefaultOffset.z + this.ZoomRate);
			}
			this.Countdown--;
			if (this.Countdown == 0)
			{
				this.Clear();
			}
		}
	}

	public int Countdown;

	public float InitialOffset;

	public bool Zooming;

	public float ZoomRate;
}
