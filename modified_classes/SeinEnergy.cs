using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200035F RID: 863
public class SeinEnergy : SaveSerialize
{
	// Token: 0x06001310 RID: 4880 RVA: 0x00010FDF File Offset: 0x0000F1DF
	public SeinEnergy()
	{
	}

	// Token: 0x06001311 RID: 4881 RVA: 0x00010FF2 File Offset: 0x0000F1F2
	public void SetCurrent(float current)
	{
		this.Current = current;
		this.MinVisual = this.Current;
		this.MaxVisual = this.Current;
	}

	// Token: 0x06001312 RID: 4882 RVA: 0x00011013 File Offset: 0x0000F213
	public void NotifyOutOfEnergy()
	{
		UI.SeinUI.ShakeEnergyOrbBar();
		Sound.Play(this.OutOfEnergySound.GetSound(null), base.transform.position, null);
	}

	// Token: 0x06001313 RID: 4883 RVA: 0x0001103D File Offset: 0x0000F23D
	public bool CanAfford(float amount)
	{
		return this.Current >= amount;
	}

	// Token: 0x17000350 RID: 848
	// (get) Token: 0x06001314 RID: 4884 RVA: 0x0001104B File Offset: 0x0000F24B
	public float VisualMin
	{
		get
		{
			return this.MinVisual / this.Max;
		}
	}

	// Token: 0x17000351 RID: 849
	// (get) Token: 0x06001315 RID: 4885 RVA: 0x0001105A File Offset: 0x0000F25A
	public float VisualMax
	{
		get
		{
			return this.MaxVisual / this.Max;
		}
	}

	// Token: 0x06001316 RID: 4886 RVA: 0x0006C3CC File Offset: 0x0006A5CC
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

	// Token: 0x06001317 RID: 4887 RVA: 0x00011069 File Offset: 0x0000F269
	public void Spend(float amount)
	{
		this.Current -= amount;
		if (this.Current < 0f)
		{
			this.Current = 0f;
		}
		this.MinVisual = this.Current;
	}

	// Token: 0x06001318 RID: 4888 RVA: 0x0006C41C File Offset: 0x0006A61C
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.Current);
		ar.Serialize(ref this.Max);
		if (ar.Reading)
		{
			this.MinVisual = (this.MaxVisual = this.Current);
		}
	}

	// Token: 0x17000352 RID: 850
	// (get) Token: 0x06001319 RID: 4889 RVA: 0x000110A0 File Offset: 0x0000F2A0
	public bool EnergyActive
	{
		get
		{
			return this.Max > 0f;
		}
	}

	// Token: 0x17000353 RID: 851
	// (get) Token: 0x0600131A RID: 4890 RVA: 0x0001105A File Offset: 0x0000F25A
	public float VisualMaxNormalized
	{
		get
		{
			return this.MaxVisual / this.Max;
		}
	}

	// Token: 0x17000354 RID: 852
	// (get) Token: 0x0600131B RID: 4891 RVA: 0x0001104B File Offset: 0x0000F24B
	public float VisualMinNormalized
	{
		get
		{
			return this.MinVisual / this.Max;
		}
	}

	// Token: 0x17000355 RID: 853
	// (get) Token: 0x0600131C RID: 4892 RVA: 0x000110AF File Offset: 0x0000F2AF
	public object EnergyUpgradesCollected
	{
		get
		{
			return this.Max;
		}
	}

	// Token: 0x0600131D RID: 4893
	public void Update()
	{
		this.MinVisual = Mathf.MoveTowards(this.MinVisual, (float)((int)(this.Current * 4f)) / 4f, Time.deltaTime);
		this.MaxVisual = Mathf.MoveTowards(this.MaxVisual, (float)((int)(this.Current * 4f)) / 4f, Time.deltaTime);
	}

	// Token: 0x0600131E RID: 4894 RVA: 0x000110F6 File Offset: 0x0000F2F6
	public void RestoreAllEnergy()
	{
		if (this.Current < this.Max)
		{
			this.Current = this.Max;
		}
	}

	// Token: 0x04001205 RID: 4613
	public float MinVisual;

	// Token: 0x04001206 RID: 4614
	public float MaxVisual;

	// Token: 0x04001207 RID: 4615
	public float Current;

	// Token: 0x04001208 RID: 4616
	public float Max = 3f;

	// Token: 0x04001209 RID: 4617
	public SoundProvider OutOfEnergySound;
}
