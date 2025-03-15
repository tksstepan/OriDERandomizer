using System;
using System.Collections.Generic;
using Game;
using Sein.World;
using UnityEngine;

public static class RandomizerBonus
{
    public static void UpgradeID(int ID)
    {
        bool flag = ID < 0;
        if (flag)
        {
            ID = -ID;
        }
        if (RandomizerBonusSkill.BonusSkillNames.ContainsKey(ID))
        {
            RandomizerBonusSkill.FoundBonusSkill(ID);
            return;
        }

        // keysanity
        if (ID >= 300 && ID < 312) {
            if(flag)
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
            else
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            Randomizer.Keysanity.ShowPickupHint(ID);
            return;
        }

        if(ID >= 200 && ID < 260)
        {
            int abilityId = (ID - 200) % 30;
            Ability ability = abilities[abilityId];
            if (ID < 230)
                ability.Found();
            else
                ability.Lost();
            return;
        }
        switch (ID)
        {
        case 0:
            if (!flag)
            {
                Characters.Sein.Mortality.Health.SetAmount((float)(Characters.Sein.Mortality.Health.MaxHealth + 20));
                RandomizerSwitch.PickupMessage("Mega Health");
                return;
            }
            break;
        case 1:
            if (!flag)
            {
                Characters.Sein.Energy.SetCurrent(Characters.Sein.Energy.Max + 5f);
                RandomizerSwitch.PickupMessage("Mega Energy");
                return;
            }
            break;
        case 2:
            Randomizer.returnToStart();
            RandomizerSwitch.PickupMessage("Go Home!");
            return;
        case 20:
            break;
        case 6:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                RandomizerSwitch.PickupMessage("Attack Upgrade (" + RandomizerBonus.SpiritFlameLevel().ToString() + ")");
                return;
            }
            if (RandomizerBonus.SpiritFlameLevel() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                RandomizerSwitch.PickupMessage("Attack Upgrade (" + RandomizerBonus.SpiritFlameLevel().ToString() + ")");
                return;
            }
            break;
        case 8:
            RandomizerSwitch.PickupMessage("Explosion Power Upgrade");
            if (!RandomizerBonus.ExplosionPower())
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                return;
            }
            break;
        case 9:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            }
            else if (Characters.Sein.Inventory.GetRandomizerItem(ID) > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
            }
            if (Characters.Sein.Inventory.GetRandomizerItem(ID) == 1)
                RandomizerSwitch.PickupMessage("Spirit Light Efficiency");
            else
                RandomizerSwitch.PickupMessage("Spirit Light Efficiency (" + Characters.Sein.Inventory.GetRandomizerItem(ID).ToString() + ")");
            break;
        case 10:
            RandomizerSwitch.PickupMessage("Extra Air Dash");
            if (!RandomizerBonus.DoubleAirDash())
            {
                Characters.Sein.Inventory.SetRandomizerItem(ID, 1);
                return;
            }
            break;
        case 11:
            RandomizerSwitch.PickupMessage("Charge Dash Efficiency");
            if (!RandomizerBonus.ChargeDashEfficiency())
            {
                Characters.Sein.Inventory.SetRandomizerItem(ID, 1);
                return;
            }
            break;
        case 12:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                if (RandomizerBonus.DoubleJumpUpgrades() == 1)
                {
                    RandomizerSwitch.PickupMessage("Extra Double Jump");
                    return;
                }
                RandomizerSwitch.PickupMessage("Extra Double Jump (" + RandomizerBonus.DoubleJumpUpgrades().ToString() + ")");
                return;
            }
            else if (RandomizerBonus.DoubleJumpUpgrades() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                if (RandomizerBonus.DoubleJumpUpgrades() == 1)
                {
                    RandomizerSwitch.PickupMessage("Extra Double Jump");
                    return;
                }
                RandomizerSwitch.PickupMessage("Extra Double Jump (" + RandomizerBonus.DoubleJumpUpgrades().ToString() + ")");
                return;
            }
            break;
        case 13:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                RandomizerSwitch.PickupMessage("Health Regeneration (" + RandomizerBonus.HealthRegeneration().ToString() + ")");
                return;
            }
            if (RandomizerBonus.HealthRegeneration() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                RandomizerSwitch.PickupMessage("Health Regeneration (" + RandomizerBonus.HealthRegeneration().ToString() + ")");
                return;
            }
            break;
        case 15:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                RandomizerSwitch.PickupMessage("Energy Regeneration (" + RandomizerBonus.EnergyRegeneration().ToString() + ")");
                return;
            }
            if (RandomizerBonus.EnergyRegeneration() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                RandomizerSwitch.PickupMessage("Energy Regeneration (" + RandomizerBonus.EnergyRegeneration().ToString() + ")");
                return;
            }
            break;
        case 17:
            if (flag)
            {
                if (RandomizerBonus.WaterVeinShards() > 0)
                {
                    Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                    RandomizerSwitch.PickupMessage("*Water Vein Shard (" + RandomizerBonus.WaterVeinShards().ToString() + "/3)*");
                }
            }
            else {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                RandomizerSwitch.PickupMessage("*Water Vein Shard (" + RandomizerBonus.WaterVeinShards().ToString() + "/3)*", 300);
            }
            Keys.GinsoTree = (RandomizerBonus.WaterVeinShards() >= 3);
            if(Keys.GinsoTree) 
                RandomizerStatsManager.FoundEvent(0);
            return;
        case 19:
            if (flag)
            {
                if (RandomizerBonus.GumonSealShards() > 0)
                {
                    Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                    RandomizerSwitch.PickupMessage("#Gumon Seal Shard (" + RandomizerBonus.GumonSealShards().ToString() + "/3)#");
                }
            }
            else {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                RandomizerSwitch.PickupMessage("#Gumon Seal Shard (" + RandomizerBonus.GumonSealShards().ToString() + "/3)#", 300);
            }
            Keys.ForlornRuins = (RandomizerBonus.GumonSealShards() >= 3);
            if(Keys.ForlornRuins) 
                RandomizerStatsManager.FoundEvent(2);
            return;
        case 21:
            if (flag)
            {
                if (RandomizerBonus.SunstoneShards() > 0)
                {
                    Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                    RandomizerSwitch.PickupMessage("@Sunstone Shard (" + RandomizerBonus.SunstoneShards().ToString() + "/3)@");
                }
            }
            else {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                RandomizerSwitch.PickupMessage("@Sunstone Shard (" + RandomizerBonus.SunstoneShards().ToString() + "/3)@", 300);
            }
            Keys.MountHoru = (RandomizerBonus.SunstoneShards() >= 3);
            if(Keys.MountHoru) 
                RandomizerStatsManager.FoundEvent(4);
            return;
        case 28:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            }else if (RandomizerBonus.WarmthFrags() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
            }
            if(Randomizer.fragKeyFinish < RandomizerBonus.WarmthFrags())
            {
                RandomizerSwitch.PickupMessage("@Warmth Fragment (extra)@", 300);
                return;
            }
            RandomizerSwitch.PickupMessage(string.Concat(new object[] { "@Warmth Fragment (", RandomizerBonus.WarmthFrags().ToString(), "/", Randomizer.fragKeyFinish, ")@" }), 300);
            break;
        case 29:
            return;
        case 30:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                RandomizerSwitch.PickupMessage("Bleeding x" + RandomizerBonus.Bleeding().ToString());
                return;
            }
            if (RandomizerBonus.Bleeding() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                RandomizerSwitch.PickupMessage("Bleeding x" + RandomizerBonus.Bleeding().ToString());
                return;
            }
            break;
        case 31:
            if (!flag)
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            else if (RandomizerBonus.Lifesteal() > 0)
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
            if(Lifesteal() == 1)
                RandomizerSwitch.PickupMessage("Health Leech");
            else
                RandomizerSwitch.PickupMessage("Health Leech x" + RandomizerBonus.Lifesteal().ToString());
            break;
        case 32:
            if (!flag)
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            else if (RandomizerBonus.Manavamp() > 0)
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
            if(Manavamp() == 1)
                RandomizerSwitch.PickupMessage("Energy Leech");
            else
                RandomizerSwitch.PickupMessage("Energy Leech x" + RandomizerBonus.Manavamp().ToString());
            break;
        case 33:
            if (!flag)
            {
                int v = Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                RandomizerSwitch.PickupMessage("Skill Velocity Upgrade x" + v.ToString());
                if(Characters.Sein.Inventory.GetRandomizerItem(108) == 0) 
                    RandomizerBonusSkill.FoundBonusSkill(108);
                return;
            }
            if (RandomizerBonus.Velocity() > 0)
            {
                int v = Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                RandomizerSwitch.PickupMessage("Skill Velocity Upgrade x" + v.ToString());
                return;
            }
            break;
        case 34:
            Characters.Sein.Inventory.SetRandomizerItem(34, 1);
            RandomizerSwitch.PickupMessage("Return to start disabled!");
        break;
        case 35:
            Characters.Sein.Inventory.SetRandomizerItem(34, 0);
            RandomizerSwitch.PickupMessage("Return to start enabled!");
        break;
        case 36:
            RandomizerSwitch.PickupMessage("Underwater Skill Usage");
            Characters.Sein.Inventory.SetRandomizerItem(36, 1);
            break;
        case 37:
            if (!flag)
            {
                int v = Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                RandomizerSwitch.PickupMessage("Jump Upgrade x" + v.ToString());
                if(Characters.Sein.Inventory.GetRandomizerItem(108) == 0) 
                    RandomizerBonusSkill.FoundBonusSkill(108);
                return;
            }
            if (RandomizerBonus.Jumpgrades() > 0)
            {
                int v = Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                RandomizerSwitch.PickupMessage("Jump Upgrade x" + v.ToString());
                return;
            }
            break;
        case 40:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@Wall Jump Lost!!@", 240);
            Characters.Sein.PlayerAbilities.WallJump.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 41:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@ChargeFlame Lost!!@", 240);
            Characters.Sein.PlayerAbilities.ChargeFlame.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 42:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@DoubleJump Lost!!@", 240);
            Characters.Sein.PlayerAbilities.DoubleJump.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 43:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@Bash Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Bash.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 44:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@Stomp Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Stomp.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 45:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@Glide Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Glide.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 46:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@Climb Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Climb.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 47:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@Charge Jump Lost!!@", 240);
            Characters.Sein.PlayerAbilities.ChargeJump.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 48:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@Dash Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Dash.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 49:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@Grenade Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Grenade.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 50:
            if (!Characters.Sein || flag)
                return;
            RandomizerSwitch.PickupMessage("@Sein Lost!!@", 240);
            Characters.Sein.PlayerAbilities.SpiritFlame.HasAbility = false;
            Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
            return;
        case 81:
            if(Characters.Sein.Inventory.GetRandomizerItem(ID) > 0)
                return;
            Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            string s_color = "";
            string g_color = "";
            if(Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Stomp))
                s_color = "$";
            if(Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Grenade))
                g_color = "$";
            RandomizerSwitch.PickupMessage(s_color + "Stomp: " + Randomizer.StompZone + s_color + g_color+ "    Grenade: "+ Randomizer.GrenadeZone + g_color, 480);
            break;
        case 1102:
            if (!flag)
                Characters.Sein.Inventory.SetRandomizerItem(ID, 1);
                return;
            Characters.Sein.Inventory.SetRandomizerItem(ID, 0);
            return;
        default:
            if(flag)
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
            else
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            return;
        }
    }

    public static bool SenseFragsEnabled => Characters.Sein.Inventory.GetRandomizerItem(1100) > 0;
    public static bool SenseFragsActive {
        get => SenseFragsEnabled && Characters.Sein.Inventory.GetRandomizerItem(1101) > 0;
        set => Characters.Sein.Inventory.SetRandomizerItem(1101, value ? 1 : 0);
    }

    public static bool ForlornEscapeHint()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(81) > 0;
    }

    public static bool DoubleAirDash()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(10) > 0;
    }

    public static bool ChargeDashEfficiency()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(11) > 0;
    }

    public static bool DoubleJumpUpgrade()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(12) > 0;
    }

    public static int HealthRegeneration()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(13);
    }

    public static int EnergyRegeneration()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(15);
    }

    public static int WaterVeinShards()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(17);
    }

    public static int SunstoneShards()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(21);
    }

    public static int GumonSealShards()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(19);
    }

    public static int SpiritFlameLevel()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(6);
    }

    public static int MapStoneProgression()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(23);
    }

    public static int SkillTreeProgression()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(27);
    }

    public static bool ExplosionPower()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(8) > 0;
    }

    public static int ExpWithBonuses(int baseExp, bool doTrack)
    {
        float mult = 1.0f + Characters.Sein.Inventory.GetRandomizerItem(9);
        if(Characters.Sein.PlayerAbilities.AbilityMarkers.HasAbility) 
            mult += .5f;
        if(Characters.Sein.PlayerAbilities.SoulEfficiency.HasAbility)
            mult += .5f;
        int total = (int)(baseExp*mult);
        if(doTrack)
        {
            RandomizerStatsManager.OnExp(baseExp, total-baseExp);
            BingoController.OnExp(total);
        }
        return total;
    }

    public static int GetPickupCount()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(0);
    }

    public static int DoubleJumpUpgrades()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(12);
    }

    public static int UpgradeCount(int ID)
    {
        if(ID == 17 || ID == 19 || ID == 21)
            return Math.Min(Characters.Sein.Inventory.GetRandomizerItem(ID), 3); 
        return Characters.Sein.Inventory.GetRandomizerItem(ID);
    }

    public static void CollectMapstone()
    {
        Characters.Sein.Inventory.IncRandomizerItem(23, 1);
    }

    public static void Update()
    {
        int healthRegenLevel = RandomizerBonus.HealthRegeneration() + (Characters.Sein.PlayerAbilities.HealthMarkers.HasAbility ? 2 : 0) - RandomizerBonus.Bleeding();
        int energyRegenLevel = RandomizerBonus.EnergyRegeneration() + (Characters.Sein.PlayerAbilities.EnergyMarkers.HasAbility ? 2 : 0);

        if (healthRegenLevel > 0)
        {
            Characters.Sein.Mortality.Health.GainHealth((float)healthRegenLevel * RandomizerBonus.HealthRegenAmount * Time.deltaTime / RandomizerBonus.HealthRegenTimeSeconds);
        }
        else if (healthRegenLevel < 0)
        {
            Characters.Sein.Mortality.Health.LoseHealth((float)(-healthRegenLevel) * RandomizerBonus.HealthRegenAmount * Time.deltaTime / RandomizerBonus.HealthRegenTimeSeconds);
        }
        if (RandomizerBonus.Bleeding() > 0 && Characters.Sein.Mortality.Health.Amount <= 0f)
        {
            Characters.Sein.Mortality.DamageReciever.OnRecieveDamage(new Damage(1f, default(Vector2), default(Vector3), DamageType.Water, null));
        }
        Characters.Sein.Energy.Gain((float)energyRegenLevel * RandomizerBonus.EnergyRegenAmount * Time.deltaTime / RandomizerBonus.EnergyRegenTimeSeconds);
        RandomizerBonusSkill.Update();
    }

    public static void DamageDealt(float damage)
    {
        if (Characters.Sein)
        {
            if (damage > 20f)
            {
                damage = 20f;
            }
            Characters.Sein.Mortality.Health.GainHealth((float)RandomizerBonus.Lifesteal() * 0.2f * damage);
            Characters.Sein.Energy.Gain((float)RandomizerBonus.Manavamp() * 0.05f * damage);
        }
    }

    public static int Bleeding()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(30);
    }

    public static bool ExpEfficiency()
    {
        return false;
    }

    public static int Lifesteal()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(31);
    }

    public static int Manavamp()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(32);
    }

    public static int Velocity()
    {
        try {
            if(RandomizerBonusSkill.IsActive(108))
                return 0;
            return Characters.Sein.Inventory.GetRandomizerItem(33);            
        }
        catch(Exception) {
            return 0;
        }
    }
    public static int Jumpgrades()
    {
        try {
            if(RandomizerBonusSkill.IsActive(108))
                return 0;
            return Characters.Sein.Inventory.GetRandomizerItem(37);            
        }
        catch(Exception) {
            return 0;
        }
    }
    public static float Jumpscale {get {return 1f + .25f * Jumpgrades();}}
    public static float DoubleJumpscale {get {return 1f + .10f * Jumpgrades();}}
    public static float Veloscale {get {return 1f + .20f * Velocity();}}
    public static bool GravitySuit()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(36) > 0;
    }

    public static bool Swimming()
    {
        return Characters.Sein.Controller.IsSwimming && !GravitySuit();
    }

    public static void SpentAP(int numSpent)
    {
        Characters.Sein.Inventory.IncRandomizerItem(80, numSpent);
    }

    public static int ResetAP() {
        int refund = Characters.Sein.Inventory.GetRandomizerItem(80);
        Characters.Sein.Inventory.SetRandomizerItem(80, 0);
        BingoController.OnResetAP();
        return refund;
    }

    public static void ListBonuses() {
        List<string> bonuses = new List<string>();
        foreach(var kv in BonusNames) {
            var amnt = Characters.Sein.Inventory.GetRandomizerItem(kv.Key);
            if(amnt == 0) continue;
            if(amnt == 1)
                bonuses.Add(kv.Value);
            else
                bonuses.Add($"{kv.Value} ({amnt})");
        }
        if(bonuses.Count > 0) {
            var msg = $"ALIGNRIGHTANCHORTOPPARAMS_12_14_1_{string.Join("\n", bonuses.ToArray())}";
            Randomizer.printInfo(msg);
        } else Randomizer.printInfo("No bonus passives");
    }

    private static Dictionary<int, String> BonusNames = new Dictionary<int, String>() {
        {6, "Attack Upgrade"},
        {13, "Health Regeneration"},
        {15, "Energy Regeneration"},
        {12, "Extra Double Jump"},
        {33, "Skill Velocity Upgrade"},
        {37, "Jumpgrade"},
        {31, "Health Leech"},
        {32, "Energy Leech"},
        {36, "Underwater Skill Usage"},
        {10, "Extra Air Dash"},
        {11, "Charge Dash Efficiency"},
        {9, "Spirit Light Efficiency"},
    };

    public static int WarmthFrags()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(28);
    }

    public static bool AltRDisabled()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(34) == 1;
    }

    public static bool DoubleAirDashUsed;

    private static RandomizerBonus.Ability[] abilities = new RandomizerBonus.Ability[]
    {
        new RandomizerBonus.Ability("Quick Flame", (PlayerAbilities p) => p.QuickFlame),
        new RandomizerBonus.Ability("Spark Flame", (PlayerAbilities p) => p.SparkFlame),
        new RandomizerBonus.Ability("Charge Flame Burn", (PlayerAbilities p) => p.ChargeFlameBurn),
        new RandomizerBonus.Ability("Split Flame", (PlayerAbilities p) => p.SplitFlameUpgrade),
        new RandomizerBonus.Ability("Ultra Light Burst", (PlayerAbilities p) => p.GrenadeUpgrade),
        new RandomizerBonus.Ability("Cinder Flame", (PlayerAbilities p) => p.CinderFlame),
        new RandomizerBonus.Ability("Ultra Stomp", (PlayerAbilities p) => p.StompUpgrade),
        new RandomizerBonus.Ability("Rapid Flame", (PlayerAbilities p) => p.RapidFire),
        new RandomizerBonus.Ability("Charge Flame Blast", (PlayerAbilities p) => p.ChargeFlameBlast),
        new RandomizerBonus.Ability("Ultra Split Flame", (PlayerAbilities p) => p.UltraSplitFlame),
        new RandomizerBonus.Ability("Spirit Magnet", (PlayerAbilities p) => p.Magnet),
        new RandomizerBonus.Ability("Map Markers", (PlayerAbilities p) => p.MapMarkers),
        new RandomizerBonus.Ability("Life Efficiency", (PlayerAbilities p) => p.HealthEfficiency),
        new RandomizerBonus.Ability("Ultra Spirit Magnet", (PlayerAbilities p) => p.UltraMagnet),
        new RandomizerBonus.Ability("Energy Efficiency", (PlayerAbilities p) => p.EnergyEfficiency),
        new RandomizerBonus.Ability("Ability Markers", (PlayerAbilities p) => p.AbilityMarkers),
        new RandomizerBonus.Ability("Spirit Efficiency", (PlayerAbilities p) => p.SoulEfficiency),
        new RandomizerBonus.Ability("Life Markers", (PlayerAbilities p) => p.HealthMarkers),
        new RandomizerBonus.Ability("Energy Markers", (PlayerAbilities p) => p.EnergyMarkers),
        new RandomizerBonus.Ability("Sense", (PlayerAbilities p) => p.Sense),
        new RandomizerBonus.Ability("Rekindle", (PlayerAbilities p) => p.Rekindle),
        new RandomizerBonus.Ability("Regroup", (PlayerAbilities p) => p.Regroup),
        new RandomizerBonus.Ability("Charge Flame Efficiency", (PlayerAbilities p) => p.ChargeFlameEfficiency),
        new RandomizerBonus.Ability("Air Dash", (PlayerAbilities p) => p.AirDash),
        new RandomizerBonus.Ability("Ultra Soul Link", (PlayerAbilities p) => p.UltraSoulFlame),
        new RandomizerBonus.Ability("Charge Dash", (PlayerAbilities p) => p.ChargeDash),
        new RandomizerBonus.Ability("Water Breath", (PlayerAbilities p) => p.WaterBreath),
        new RandomizerBonus.Ability("Soul Link Efficiency", (PlayerAbilities p) => p.SoulFlameEfficiency),
        new RandomizerBonus.Ability("Triple Jump", (PlayerAbilities p) => p.DoubleJumpUpgrade),
        new RandomizerBonus.Ability("Ultra Defense", (PlayerAbilities p) => p.UltraDefense)
    };

    public static float HealthRegenAmount = 4f;

    public static float HealthRegenTimeSeconds = 60f;

    public static float EnergyRegenAmount = 1f;

    public static float EnergyRegenTimeSeconds = 60f;

    private class Ability
    {
        public Ability(string name, Func<PlayerAbilities, CharacterAbility> selector)
        {
            this.name = name;
            this.selector = selector;
        }
        public void Found()
        {
            if (!Characters.Sein)
            {
                return;
            }
            RandomizerSwitch.PickupMessage("$" + this.name + "$", 240);
            this.selector(Characters.Sein.PlayerAbilities).HasAbility = true;
        }
        public void Lost()
        {
            if (!Characters.Sein)
            {
                return;
            }
            RandomizerSwitch.PickupMessage("@" + this.name + " Lost!!@", 240);
            this.selector(Characters.Sein.PlayerAbilities).HasAbility = false;
        }
        private string name;
        private Func<PlayerAbilities, CharacterAbility> selector;
    }
}
