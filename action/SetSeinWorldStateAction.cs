using System;
using Sein.World;
using UnityEngine;

public class SetSeinWorldStateAction : ActionMethod
{
	public override void Perform(IContext context)
	{
		GameWorld.Instance.CurrentArea.DirtyCompletionAmount();
		switch (this.State)
		{
		case WorldState.WaterPurified:
			Events.WarmthReturned = this.IsTrue;
			Randomizer.NeedGinsoEscapeCleanup = true;
			RandomizerLocationManager.GivePickup(this.MoonGuid);
			return;
		case WorldState.GumoFree:
			Events.GumoFree = this.IsTrue;
			return;
		case WorldState.SpiritTreeReached:
			Events.SpiritTreeReached = this.IsTrue;
			return;
		case WorldState.GinsoTreeKey:
			RandomizerLocationManager.GivePickup(this.MoonGuid);
			return;
		case (WorldState)4:
		case (WorldState)6:
			return;
		case WorldState.GinsoTreeEntered:
			Events.GinsoTreeEntered = this.IsTrue;
			return;
		case WorldState.WindRestored:
			RandomizerLocationManager.GivePickup(this.MoonGuid);
			return;
		case WorldState.GravityActivated:
			Events.GravityActivated = this.IsTrue;
			return;
		case WorldState.MistLifted:
			Events.MistLifted = this.IsTrue;
			return;
		case WorldState.ForlornRuinsKey:
			RandomizerLocationManager.GivePickup(this.MoonGuid);
			return;
		case WorldState.MountHoruKey:
			RandomizerLocationManager.GivePickup(this.MoonGuid);
			return;
		case WorldState.WarmthReturned:
			RandomizerLocationManager.GivePickup(this.MoonGuid);
			return;
		case WorldState.DarknessLifted:
			Events.DarknessLifted = this.IsTrue;
			return;
		default:
			return;
		}
	}

	public override string GetNiceName()
	{
		return "Set " + ActionHelper.GetName(this.State.ToString()) + " to " + ActionHelper.GetName(this.IsTrue.ToString());
	}

	public WorldState State;

	public bool IsTrue;
}
