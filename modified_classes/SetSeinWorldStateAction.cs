using System;
using Sein.World;
using UnityEngine;

// Token: 0x02000993 RID: 2451
public class SetSeinWorldStateAction : ActionMethod
{
	// Token: 0x06003591 RID: 13713 RVA: 0x00003EB3 File Offset: 0x000020B3
	public SetSeinWorldStateAction()
	{
	}

	// Token: 0x06003592 RID: 13714
	public override void Perform(IContext context)
	{
		switch (this.State)
		{
		case WorldState.WaterPurified:
			Events.WarmthReturned = this.IsTrue;
			Randomizer.getPickup(new Vector3(548f, 952f));
			return;
		case WorldState.GumoFree:
			Events.GumoFree = this.IsTrue;
			return;
		case WorldState.SpiritTreeReached:
			Events.SpiritTreeReached = this.IsTrue;
			return;
		case WorldState.GinsoTreeKey:
			Randomizer.getPickup(new Vector3(500f, -248f));
			return;
		case (WorldState)4:
		case (WorldState)6:
			return;
		case WorldState.GinsoTreeEntered:
			Events.GinsoTreeEntered = this.IsTrue;
			return;
		case WorldState.WindRestored:
			Randomizer.getPickup(new Vector3(-732f, -236f));
			return;
		case WorldState.GravityActivated:
			Events.GravityActivated = this.IsTrue;
			return;
		case WorldState.MistLifted:
			Events.MistLifted = this.IsTrue;
			return;
		case WorldState.ForlornRuinsKey:
			Randomizer.getPickup(new Vector3(-720f, -24f));
			return;
		case WorldState.MountHoruKey:
			Randomizer.getPickup(new Vector3(-560f, 600f));
			return;
		case WorldState.WarmthReturned:
			Randomizer.getPickup(new Vector3(-240f, 512f));
			return;
		case WorldState.DarknessLifted:
			Events.DarknessLifted = this.IsTrue;
			return;
		default:
			return;
		}
	}

	// Token: 0x06003593 RID: 13715 RVA: 0x0002A53E File Offset: 0x0002873E
	public override string GetNiceName()
	{
		return "Set " + ActionHelper.GetName(this.State.ToString()) + " to " + ActionHelper.GetName(this.IsTrue.ToString());
	}

	// Token: 0x04003018 RID: 12312
	public WorldState State;

	// Token: 0x04003019 RID: 12313
	public bool IsTrue;
}
