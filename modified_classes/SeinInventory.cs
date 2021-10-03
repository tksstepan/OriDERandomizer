using System;
using System.Collections.Generic;

public class SeinInventory : SaveSerialize
{
	public SeinInventory()
	{
		this.RandomizerItems = new Dictionary<int, int>();
	}

	public event Action OnCollectKeystones = delegate
	{
	};

	public event Action OnCollectMapstone = delegate
	{
	};

	public bool HasKeystones
	{
		get
		{
			return this.Keystones != 0;
		}
	}

	public bool HasMapstones
	{
		get
		{
			return this.MapStones != 0;
		}
	}

	public bool CanAfford(int cost)
	{
		return this.Keystones >= cost;
	}

	public void SpendKeystones(int cost)
	{
		this.Keystones -= cost;
		if (this.Keystones < 0)
		{
			this.Keystones = 0;
		}
	}

	public void SpendMapstone(int cost)
	{
		this.MapStones -= cost;
		if (this.MapStones < 0)
		{
			this.MapStones = 0;
		}
	}

	public void CollectKeystones(int amount)
	{
		this.Keystones += amount;
		this.OnCollectKeystones();
	}

	public void CollectMapstone(int amount)
	{
		this.MapStones += amount;
		this.OnCollectMapstone();
	}

	public void RestoreKeystones(int amount)
	{
		this.CollectKeystones(amount);
	}

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

	public int GetRandomizerItem(int code)
	{
		if (this.RandomizerItems.ContainsKey(code))
		{
			return this.RandomizerItems[code];
		}
		return 0;
	}

	public int SetRandomizerItem(int code, int value)
	{
		this.RandomizerItems[code] = value;
		return value;
	}

	public int IncRandomizerItem(int code, int value)
	{
		return this.SetRandomizerItem(code, this.GetRandomizerItem(code) + value);
	}

	public int Keystones;

	public int MapStones;

	public int SkillPointsCollected;

	public Dictionary<int, int> RandomizerItems;
}
