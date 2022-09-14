using System;
using Game;

[Category("Sein")]
public class GetAbilityAction : ActionMethod
{
	public override void Perform(IContext context)
	{
		RandomizerLocationManager.GivePickup(this.MoonGuid);
		GameWorld.Instance.CurrentArea.DirtyCompletionAmount();
	}

	public AbilityType Ability;

	public bool Gain = true;
}
