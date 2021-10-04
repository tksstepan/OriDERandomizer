using System;
using System.Collections.Generic;
using Game;
using Sein.World;

public class GetWorldEventCondition : Condition
{
	public override bool Validate(IContext context)
	{
		if (this.WorldEvents.UniqueID == 26 && RandomizerStatsManager.FinishedGinsoEscape)
		{
			return this.State != 21;
		}
		int value = World.Events.Find(this.WorldEvents).Value;
		if (this.States.Count == 0)
		{
			return this.State == value;
		}
		for (int i = 0; i < this.States.Count; i++)
		{
			int num = this.States[i];
			if (value == num)
			{
				return true;
			}
		}
		return false;
	}

	public WorldEvents WorldEvents;

	public int State;

	public List<int> States;
}
