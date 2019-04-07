using System;
using System.Collections.Generic;

// Token: 0x0200032D RID: 813
public class SeinInventory : SaveSerialize
{
	// Token: 0x06001188 RID: 4488
	public SeinInventory()
	{
		this.RandomizerItems = new Dictionary<int, int>();
	}

	// Token: 0x14000015 RID: 21
	// (add) Token: 0x06001189 RID: 4489
	// (remove) Token: 0x0600118A RID: 4490
	public event Action OnCollectKeystones = delegate
	{
	};

	// Token: 0x14000016 RID: 22
	// (add) Token: 0x0600118B RID: 4491
	// (remove) Token: 0x0600118C RID: 4492
	public event Action OnCollectMapstone = delegate
	{
	};

	// Token: 0x170002F6 RID: 758
	// (get) Token: 0x0600118D RID: 4493
	public bool HasKeystones
	{
		get
		{
			return this.Keystones != 0;
		}
	}

	// Token: 0x170002F7 RID: 759
	// (get) Token: 0x0600118E RID: 4494
	public bool HasMapstones
	{
		get
		{
			return this.MapStones != 0;
		}
	}

	// Token: 0x0600118F RID: 4495
	public bool CanAfford(int cost)
	{
		return this.Keystones >= cost;
	}

	// Token: 0x06001190 RID: 4496
	public void SpendKeystones(int cost)
	{
		this.Keystones -= cost;
		if (this.Keystones < 0)
		{
			this.Keystones = 0;
		}
	}

	// Token: 0x06001191 RID: 4497
	public void SpendMapstone(int cost)
	{
		this.MapStones -= cost;
		if (this.MapStones < 0)
		{
			this.MapStones = 0;
		}
	}

	// Token: 0x06001192 RID: 4498
	public void CollectKeystones(int amount)
	{
		this.Keystones += amount;
		this.OnCollectKeystones();
	}

	// Token: 0x06001193 RID: 4499
	public void CollectMapstone(int amount)
	{
		this.MapStones += amount;
		this.OnCollectMapstone();
	}

	// Token: 0x06001194 RID: 4500
	public void RestoreKeystones(int amount)
	{
		this.CollectKeystones(amount);
	}

	// Token: 0x06001195 RID: 4501
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.Keystones);
		ar.Serialize(ref this.MapStones);
		ar.Serialize(ref this.SkillPointsCollected);
		if(ar.Reading)
		{
			Dictionary<int, int> preserve = new Dictionary<int, int>();
			foreach(int code in this.RandomizerItems.Keys) 
				if(code >= 1500 && code < 1600)
					preserve[code] = this.RandomizerItems[code];
			ar.Serialize(ref this.RandomizerItems);
			foreach(int code in preserve.Keys) 
					this.RandomizerItems[code] = preserve[code];
		}  else {
			ar.Serialize(ref this.RandomizerItems);
		}
	}


	// Token: 0x06001197 RID: 4503
	public int GetRandomizerItem(int code)
	{
		if (this.RandomizerItems.ContainsKey(code))
		{
			return this.RandomizerItems[code];
		}
		return 0;
	}

	// Token: 0x06001196 RID: 4502
	public int SetRandomizerItem(int code, int value)
	{
		this.RandomizerItems[code] = value;
		return value;
	}
	// Token: 0x06001198 RID: 4504
	public int IncRandomizerItem(int code, int value)
	{
		return this.SetRandomizerItem(code, this.GetRandomizerItem(code) + value);
	}


	// Token: 0x04001098 RID: 4248
	public int Keystones;

	// Token: 0x04001099 RID: 4249
	public int MapStones;

	// Token: 0x0400109A RID: 4250
	public int SkillPointsCollected;

	// Token: 0x0400109D RID: 4253
	public Dictionary<int, int> RandomizerItems;
}
