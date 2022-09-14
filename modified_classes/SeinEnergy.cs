using System;
using Core;
using Game;
using UnityEngine;

public class SeinEnergy : SaveSerialize
{
	public void SetCurrent(float current)
	{
		this.Current = current;
		this.MinVisual = this.Current;
		this.MaxVisual = this.Current;
	}

	public void NotifyOutOfEnergy()
	{
		UI.SeinUI.ShakeEnergyOrbBar();
		Sound.Play(this.OutOfEnergySound.GetSound(null), base.transform.position, null);
	}

	public bool CanAfford(float amount)
	{
		return this.Current >= amount;
	}

	public float VisualMin
	{
		get
		{
			return this.MinVisual / this.Max;
		}
	}

	public float VisualMax
	{
		get
		{
			return this.MaxVisual / this.Max;
		}
	}

	public void Gain(float amount)
	{
		if (this.Current > this.Max)
		{
			return;
		}
		this.Current += amount;
		if (this.Current > this.Max)
		{
			this.Current = this.Max;
		}
		this.MaxVisual = this.Current;
	}

	public void Spend(float amount)
	{
		this.Current -= amount;
		if (this.Current < 0f)
		{
			this.Current = 0f;
		}
		this.MinVisual = this.Current;
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.Current);
		ar.Serialize(ref this.Max);
		if (ar.Reading)
		{
			this.MinVisual = (this.MaxVisual = this.Current);
		}
	}

	public bool EnergyActive
	{
		get
		{
			return this.Max > 0f;
		}
	}

	public float VisualMaxNormalized
	{
		get
		{
			return this.MaxVisual / this.Max;
		}
	}

	public float VisualMinNormalized
	{
		get
		{
			return this.MinVisual / this.Max;
		}
	}

	public object EnergyUpgradesCollected
	{
		get
		{
			return this.Max;
		}
	}

	public void Update()
	{
		this.MinVisual = Mathf.MoveTowards(this.MinVisual, (float)((int)(this.Current * 4f)) / 4f, Time.deltaTime);
		this.MaxVisual = Mathf.MoveTowards(this.MaxVisual, (float)((int)(this.Current * 4f)) / 4f, Time.deltaTime);
	}

	public void RestoreAllEnergy()
	{
		if (this.Current < this.Max)
		{
			this.Current = this.Max;
		}
	}

	public float MinVisual;

	public float MaxVisual;

	public float Current;

	public float Max = 3f;

	public SoundProvider OutOfEnergySound;
}
