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
        Characters.Sein.Level.GainExperience(RandomizerBonus.ExpWithBonuses(Value, true));
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
            Randomizer.showHint("$Bash$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Bash, true);
            break;
        case 2:
            Randomizer.showHint("$Charge Flame$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.ChargeFlame, true);
            break;
        case 3:
            Randomizer.showHint("$Wall Jump$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.WallJump, true);
            break;
        case 4:
            Randomizer.showHint("$Stomp$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Stomp, true);
            break;
        case 5:
            Randomizer.showHint("$Double Jump$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.DoubleJump, true);
            break;
        case 8:
            Randomizer.showHint("$Charge Jump$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.ChargeJump, true);
            break;
        case 12:
            Randomizer.showHint("$Climb$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Climb, true);
            break;
        case 14:
            Randomizer.showHint("$Glide$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Glide, true);
            break;
        case 15:
            Randomizer.showHint("$Spirit Flame$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.SpiritFlame, true);
            break;
        case 50:
            Randomizer.showHint("$Dash$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Dash, true);
            break;
        case 51:
            Randomizer.showHint("$Grenade$", 300);
            Characters.Sein.PlayerAbilities.SetAbility(AbilityType.Grenade, true);
            break;
        }
        Randomizer.GiveAbility = false;
        RandomizerStatsManager.FoundSkill(Ability);
    }
    public static void EventPickup(int Value) 
    {
        switch (Value)
        {
            case 0:
                Randomizer.showHint("*Water Vein*", 300);
                Keys.GinsoTree = true;
                break;
            case 1:
                Randomizer.showHint("*Clean Water*#", 300);
                Sein.World.Events.WaterPurified = true;
                break;
            case 2:
                Randomizer.showHint("#Gumon Seal#", 300);
                Keys.ForlornRuins = true;
                break;
            case 3:
                Randomizer.showHint("#Wind Restored#", 300);
                Sein.World.Events.WindRestored = true;
                break;
            case 4:
                Randomizer.showHint("@Sunstone@", 300);
                Keys.MountHoru = true;
                break;
            case 5:
                Randomizer.showHint("@Warmth Returned@", 300);
                break;
        }
        RandomizerStatsManager.FoundEvent(Value);
    }
    
    public static void TeleportPickup(string Value)
    {
        int shardCount = -1;
        char colorChar = ' ';
        string shardPart = "";
        if(Value == "Ginso")
        {
            Characters.Sein.Inventory.SetRandomizerItem(1024, 1);
            shardCount = RandomizerBonus.WaterVeinShards();
            shardPart = "Water Vein";
            colorChar = '*';
        }
        if(Value == "Forlorn")
        {
            Characters.Sein.Inventory.SetRandomizerItem(1025, 1);
            shardCount = RandomizerBonus.GumonSealShards();
            shardPart = "Gumon Seal";
            colorChar = '#';
        }
        if(Value == "Horu")
        {
            Characters.Sein.Inventory.SetRandomizerItem(1026, 1);
            shardCount = RandomizerBonus.SunstoneShards();
            shardPart = "Sunstone";
            colorChar = '@';
        }

        if(Randomizer.Shards && shardCount >= 0 && shardCount < 2)
        {
            if(shardCount == 1){
                shardPart = "1 more " + shardPart + " shard to activate";
            }
            else{
                shardPart = "2 " + shardPart + " shards to activate";  
            }
            Randomizer.showHint(colorChar + "Broken " + Value + " teleporter\nCollect " + shardPart + colorChar, 300);
            return;
        }
        TeleporterController.Activate(Randomizer.TeleportTable[Value].ToString());
        Randomizer.showHint(colorChar + Value + " teleporter activated" + colorChar);
    }
    
    public static void GivePickup(RandomizerAction Action, int coords, bool found_locally=true)
    {
        try {
        if (found_locally && Randomizer.Sync)
        {
            RandomizerSyncManager.FoundPickup(Action, coords);
        }
        BingoController.OnItem(Action, coords);
        
        switch (Action.Action) {
            case "RP":
            case "MU":
                string[] pieces = ((string)Action.Value).Split('/');
                for(int i = 0; i < pieces.Length; i+=2)
                {
                    string code = pieces[i];
                    if(Randomizer.StringKeyPickupTypes.Contains(code)) {
                        RandomizerSwitch.GivePickup(new RandomizerAction(code, pieces[i+1]), coords, found_locally);
                    } else {
                        int id;
                        int.TryParse(pieces[i+1], out id);
                        RandomizerSwitch.GivePickup(new RandomizerAction(code, id), coords, found_locally);
                    }
                }
                break;
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
            case "SH":
                Randomizer.showHint((string)Action.Value);
                break;
            case "WT":
                Characters.Sein.Inventory.IncRandomizerItem(302, 1);
                int relics = Characters.Sein.Inventory.GetRandomizerItem(302);
                RandomizerTrackedDataManager.SetRelic(Randomizer.RelicZoneLookup[(string)Action.Value]);
                string relicStr = "\n("+relics.ToString() + "/" + Randomizer.RelicCount.ToString() + ")";
                if(relics >= Randomizer.RelicCount) {
                    relicStr = "$" + relicStr + "$";
                }
                Randomizer.showHint((string)Action.Value + relicStr, 480);
                break;
            case "WS":
            case "WP":
                Randomizer.SaveAfterWarp = Action.Action == "WS";
                string[] xy = ((string)Action.Value).Split(',');
                if(xy.Length > 2 && xy[2] == "force") {
                    Randomizer.WarpTo(new UnityEngine.Vector3(float.Parse(xy[0]), float.Parse(xy[1])), 15);
                }
                else {
                    Randomizer.WarpTarget = new UnityEngine.Vector3(float.Parse(xy[0]), float.Parse(xy[1]));
                    Randomizer.WarpSource = Characters.Sein.Position;
                    Randomizer.CanWarp = 6;
	            }
                break;
            case "NO":
                break;
        }
        RandomizerTrackedDataManager.UpdateBitfields();
        }
        catch(Exception e) {
            Randomizer.LogError("Give Pickup: " + e.Message);
        }
    }
}