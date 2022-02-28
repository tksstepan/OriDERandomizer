    using System;
    using Game;
    using Sein.World;
    using Core;

    // Token: 0x02000189 RID: 393
    public class SeinAbilityCondition : Condition
    {
    	// Token: 0x0600086D RID: 2157 RVA: 0x00004BC2 File Offset: 0x00002DC2
    	public SeinAbilityCondition()
    	{
    	}

    	// Token: 0x0600086E RID: 2158
        public override bool Validate(IContext context)
        {
            if (Characters.Sein != null)
            {
                if (this.Ability == AbilityType.Stomp)
                {
                    if (Sein.World.Events.WarmthReturned && Scenes.Manager.CurrentScene != null)
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

    	// Token: 0x0400083D RID: 2109
    	public AbilityType Ability;
    }
