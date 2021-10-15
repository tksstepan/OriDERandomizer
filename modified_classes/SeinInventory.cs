using System;
using System.Collections.Generic;

public class SeinInventory : SaveSerialize
{
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
	}

	public int GetRandomizerItem(int code)
	{
		return Randomizer.Inventory.GetRandomizerItem(code);
	}

	public int SetRandomizerItem(int code, int value)
	{
		return Randomizer.Inventory.SetRandomizerItem(code, value);
	}

	public int IncRandomizerItem(int code, int value)
	{
		return Randomizer.Inventory.IncRandomizerItem(code, value);
	}

	public int Keystones;

	public int MapStones;

	public int SkillPointsCollected;
}
