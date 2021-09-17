using System;
using Game;

[Category("Sein")]
public class GetAbilityAction : ActionMethod
{
	public override void Perform(IContext context)
	{
		Characters.Sein.PlayerAbilities.SetAbility(this.Ability, this.Gain);
		GameWorld.Instance.CurrentArea.DirtyCompletionAmount();

		if (this.Ability == AbilityType.SpiritFlame)
		{
			TeleporterController.Activate("sunkenGlades");
		}
	}

	public AbilityType Ability;

	public bool Gain = true;
}
