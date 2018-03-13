using System;
using Game;
using Sein.World;

public static class RandomizerSwitch
{

    public static void SkillPointPickup()
    {
        Randomizer.showHint("Ability Cell");
        if(Randomizer.ZeroXP)
        {
            return;
        }
        Characters.Sein.Level.GainSkillPoint();
        Characters.Sein.Inventory.SkillPointsCollected++;
    }
    
    public static void MaxEnergyContainerPickup() 
    {
        Randomizer.showHint("Energy Cell");
        if (Characters.Sein.Energy.Max == 0f)
		{
			Characters.Sein.SoulFlame.FillSoulFlameBar();
		}
		Characters.Sein.Energy.Max += 1.0f;
		if (Characters.Sein.Energy.Current < Characters.Sein.Energy.Max)
		{
			Characters.Sein.Energy.Current = Characters.Sein.Energy.Max;
		}
    }
    
    public static void ExpOrbPickup(int Value)
    {
        Randomizer.showHint(Value.ToString() + " experience");
        if(Randomizer.ZeroXP)
        {
            return;
        }
        int num = Value * ((!Characters.Sein.PlayerAbilities.SoulEfficiency.HasAbility) ? 1 : 2);
        if (RandomizerBonus.ExpEfficiency())
		{
			num *= 2;
		}
		Characters.Sein.Level.GainExperience(num);
    }
    
    public static void KeystonePickup() {
        Randomizer.showHint("Keystone");
        Characters.Sein.Inventory.CollectKeystones(1);     
    }
    
    public static void MaxHealthContainerPickup() 
    {
        Randomizer.showHint("Health Cell");
        Characters.Sein.Mortality.Health.GainMaxHeartContainer();
    }
    
    public static void MapStonePickup() 
    {
        Randomizer.showHint("Map Stone");
        Characters.Sein.Inventory.MapStones++;
    }
    
    public static void AbilityPickup(int Ability) {
        Randomizer.GiveAbility = true;
        switch (Ability)
		{
		case 0:
            Randomizer.showHint("$Bash$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Bash, true);
			break;
		case 2:
            Randomizer.showHint("$Charge Flame$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.ChargeFlame, true);
			break;
		case 3:
            Randomizer.showHint("$Wall Jump$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.WallJump, true);
			break;
		case 4:
            Randomizer.showHint("$Stomp$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Stomp, true);
			break;
		case 5:
            Randomizer.showHint("$Double Jump$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.DoubleJump, true);
			break;
		case 8:
            Randomizer.showHint("$Charge Jump$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.ChargeJump, true);
			break;
		case 12:
            Randomizer.showHint("$Climb$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Climb, true);
			break;
		case 14:
            Randomizer.showHint("$Glide$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Glide, true);
			break;
		case 50:
            Randomizer.showHint("$Dash$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Dash, true);
			break;
		case 51:
            Randomizer.showHint("$Grenade$");
			Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Grenade, true);
			break;
        }
        Randomizer.GiveAbility = false;

    }
    public static void EventPickup(int Value) 
    {
        switch (Value)
        {
            case 0:
                Randomizer.showHint("*Water Vein*");
                Keys.GinsoTree = true;
                break;
            case 1:
                Randomizer.showHint("*Clean Water*#");
                Sein.World.Events.WaterPurified = true;
                break;
            case 2:
                Randomizer.showHint("#Gumon Seal#");
                Keys.ForlornRuins = true;
                break;
            case 3:
                Randomizer.showHint("#Wind Restored#");
                Sein.World.Events.WindRestored = true;
                break;
            case 4:
                Randomizer.showHint("@Sunstone@");
                Keys.MountHoru = true;
                break;
            case 5:
                Randomizer.showHint("@Warmth Returned@");
                break;
        }
    }
    
	public static void TeleportPickup(string Value)
	{
		TeleporterController.Activate(Randomizer.TeleportTable[Value].ToString());
		Randomizer.showHint(Value + " teleporter activated");
	}

    
    public static void GivePickup(RandomizerAction Action, int coords, bool found_locally=true)
    {
        if(found_locally)
            RandomizerSyncManager.FoundPickup(Action, coords);

        switch (Action.Action) {
            case "AC":                
                SkillPointPickup();
                break;
            case "EC":                
                MaxEnergyContainerPickup();
                break;
            case "EX":                
                ExpOrbPickup((int)Action.Value);
                break;
            case "KS":                
                KeystonePickup();
                break;
            case "HC":                
                MaxHealthContainerPickup();
                break;
            case "MS":
                MapStonePickup();
                break;
            case "SK":
                AbilityPickup((int)Action.Value);
                break;
            case "EV":
                EventPickup((int)Action.Value);
                break;
            case "RB":
                RandomizerBonus.UpgradeID((int)Action.Value);
                break;
            case "TP":
                TeleportPickup((string)Action.Value);
                break;
            case "NO":
                Randomizer.showHint("Nothing");
                return;
        }
    }
}