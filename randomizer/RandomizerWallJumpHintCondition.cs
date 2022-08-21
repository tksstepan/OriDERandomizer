using System;
using Game;
using UnityEngine;

public class RandomizerWallJumpHintCondition : Condition
{
	public override bool Validate(IContext context)
	{
        if (Characters.Sein.PlayerAbilities.HasAbility(AbilityType.WallJump)) {
            return false;
        }
        if (Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Climb)) {
            return false;
        }
        if (Characters.Sein.PlayerAbilities.HasAbility(AbilityType.ChargeJump)) {
            return false;
        }
        if (Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Bash) && Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Grenade)) {
            return false;
        }
        return true;
	}

	public RandomizerWallJumpHintCondition()
	{
	}
}
