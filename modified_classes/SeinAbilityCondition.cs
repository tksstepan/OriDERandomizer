using System;
using Game;
using Sein.World;
using Core;

public class SeinAbilityCondition : Condition
{
    public override bool Validate(IContext context)
    {
        if (Characters.Sein != null)
        {
            if (this.Ability == AbilityType.Stomp)
            {
                if (RandomizerStatsManager.FinishedGinsoEscape && Scenes.Manager.CurrentScene != null)
                {
                    string scene = Scenes.Manager.CurrentScene.Scene;
                    if(scene == "ginsoTreeTurrets")
                        return true;
                    if(scene == "kuroMomentTreeDuplicate")
                        return false;
                }
                if (Randomizer.OpenWorld)
                {
                    return false;
                }
                if (!Randomizer.StompTriggers)
                {
                    return true;
                }
            }
            return Characters.Sein.PlayerAbilities.HasAbility(this.Ability);
        }
        return false;
    }

	public AbilityType Ability;
}
