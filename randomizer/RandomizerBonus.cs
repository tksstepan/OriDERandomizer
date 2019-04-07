using System;
using Game;
using Sein.World;
using UnityEngine;

// Token: 0x020009F7 RID: 2551
public static class RandomizerBonus
{
    // Token: 0x0600376D RID: 14189 RVA: 0x000E2764 File Offset: 0x000E0964
    public static void UpgradeID(int ID)
    {
        bool flag = ID < 0;
        if (flag)
        {
            ID = -ID;
        }
        if (ID >= 100)
        {
            RandomizerBonusSkill.FoundBonusSkill(ID);
            return;
        }
        switch (ID)
        {
        case 0:
            if (!flag)
            {
                Characters.Sein.Mortality.Health.SetAmount((float)(Characters.Sein.Mortality.Health.MaxHealth + 20));
                Randomizer.showHint("Mega Health");
                return;
            }
            break;
        case 1:
            if (!flag)
            {
                Characters.Sein.Energy.SetCurrent(Characters.Sein.Energy.Max + 5f);
                Randomizer.showHint("Mega Energy");
                return;
            }
            break;
        case 2:
            Randomizer.returnToStart();
            Randomizer.showHint("Go Home!");
            return;
        case 20:
            break;
        case 6:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                Randomizer.showHint("Attack Upgrade (" + RandomizerBonus.SpiritFlameLevel().ToString() + ")");
                return;
            }
            if (RandomizerBonus.SpiritFlameLevel() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                Randomizer.showHint("Attack Upgrade (" + RandomizerBonus.SpiritFlameLevel().ToString() + ")");
                return;
            }
            break;
        case 8:
            Randomizer.showHint("Explosion Power Upgrade");
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
                Randomizer.showHint("Spirit Light Efficiency");
            else
                Randomizer.showHint("Spirit Light Efficiency (" + Characters.Sein.Inventory.GetRandomizerItem(ID).ToString() + ")");
            break;
        case 10:
            Randomizer.showHint("Extra Air Dash");
            if (!RandomizerBonus.DoubleAirDash())
            {
                Characters.Sein.Inventory.SetRandomizerItem(ID, 1);
                return;
            }
            break;
        case 11:
            Randomizer.showHint("Charge Dash Efficiency");
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
                    Randomizer.showHint("Extra Double Jump");
                    return;
                }
                Randomizer.showHint("Extra Double Jump (" + RandomizerBonus.DoubleJumpUpgrades().ToString() + ")");
                return;
            }
            else if (RandomizerBonus.DoubleJumpUpgrades() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                if (RandomizerBonus.DoubleJumpUpgrades() == 1)
                {
                    Randomizer.showHint("Extra Double Jump");
                    return;
                }
                Randomizer.showHint("Extra Double Jump (" + RandomizerBonus.DoubleJumpUpgrades().ToString() + ")");
                return;
            }
            break;
        case 13:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                Randomizer.showHint("Health Regeneration (" + RandomizerBonus.HealthRegeneration().ToString() + ")");
                return;
            }
            if (RandomizerBonus.HealthRegeneration() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                Randomizer.showHint("Health Regeneration (" + RandomizerBonus.HealthRegeneration().ToString() + ")");
                return;
            }
            break;
        case 15:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                Randomizer.showHint("Energy Regeneration (" + RandomizerBonus.EnergyRegeneration().ToString() + ")");
                return;
            }
            if (RandomizerBonus.EnergyRegeneration() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                Randomizer.showHint("Energy Regeneration (" + RandomizerBonus.EnergyRegeneration().ToString() + ")");
                return;
            }
            break;
        case 17:
            if (flag)
            {
                if (RandomizerBonus.WaterVeinShards() > 0)
                {
                    Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                    Randomizer.showHint("*Water Vein Shard (" + RandomizerBonus.WaterVeinShards().ToString() + "/3)*");
                }
            }
            else if (RandomizerBonus.WaterVeinShards() >= 3)
            {
                Randomizer.showHint("*Water Vein Shard (extra)*");
            }
            else
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                Randomizer.showHint("*Water Vein Shard (" + RandomizerBonus.WaterVeinShards().ToString() + "/3)*", 300);
                if (Characters.Sein.Inventory.GetRandomizerItem(1024) == 1 && RandomizerBonus.WaterVeinShards() == 2)
                {
                    TeleporterController.Activate(Randomizer.TeleportTable["Ginso"].ToString());
                    Randomizer.MessageQueue.Enqueue("*Ginso teleporter activated*");
                }
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
                    Randomizer.showHint("#Gumon Seal Shard (" + RandomizerBonus.GumonSealShards().ToString() + "/3)#");
                }
            }
            else if (RandomizerBonus.GumonSealShards() >= 3)
            {
                Randomizer.showHint("#Gumon Seal Shard (extra)#");
            }
            else
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                Randomizer.showHint("#Gumon Seal Shard (" + RandomizerBonus.GumonSealShards().ToString() + "/3)#", 300);
                if (Characters.Sein.Inventory.GetRandomizerItem(1025) == 1 && RandomizerBonus.GumonSealShards() == 2)
                {
                    TeleporterController.Activate(Randomizer.TeleportTable["Forlorn"].ToString());
                    Randomizer.MessageQueue.Enqueue("#Forlorn teleporter activated#");
                }
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
                    Randomizer.showHint("@Sunstone Shard (" + RandomizerBonus.SunstoneShards().ToString() + "/3)@");
                }
            }
            else if (RandomizerBonus.SunstoneShards() >= 3)
            {
                Randomizer.showHint("@Sunstone Shard (extra)@");
            }
            else
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                Randomizer.showHint("@Sunstone Shard (" + RandomizerBonus.SunstoneShards().ToString() + "/3)@", 300);
                if (Characters.Sein.Inventory.GetRandomizerItem(1026) == 1 && RandomizerBonus.SunstoneShards() == 2)
                {
                    TeleporterController.Activate(Randomizer.TeleportTable["Horu"].ToString());
                    Randomizer.MessageQueue.Enqueue("@Horu teleporter activated@");
                }
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
                Randomizer.showHint("@Warmth Fragment (extra)@", 300);
                return;
            }
            Randomizer.showHint(string.Concat(new object[] { "@Warmth Fragment (", RandomizerBonus.WarmthFrags().ToString(), "/", Randomizer.fragKeyFinish, ")@" }), 300);
            break;
        case 29:
            return;
        case 30:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                Randomizer.showHint("Bleeding x" + RandomizerBonus.Bleeding().ToString());
                return;
            }
            if (RandomizerBonus.Bleeding() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                Randomizer.showHint("Bleeding x" + RandomizerBonus.Bleeding().ToString());
                return;
            }
            break;
        case 31:
            if (!flag)
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            else if (RandomizerBonus.Lifesteal() > 0)
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
            if(Lifesteal() == 1)
                Randomizer.showHint("Health Drain");
            else
                Randomizer.showHint("Health Drain x" + RandomizerBonus.Lifesteal().ToString());
            break;
        case 32:
            if (!flag)
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            else if (RandomizerBonus.Manavamp() > 0)
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
            if(Manavamp() == 1)
                Randomizer.showHint("Energy Drain");
            else
                Randomizer.showHint("Health Drain x" + RandomizerBonus.Manavamp().ToString());
            break;
            break;
        case 33:
            if (!flag)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
                Randomizer.showHint("Skill Velocity Upgrade x" + RandomizerBonus.Velocity().ToString());
                return;
            }
            if (RandomizerBonus.Velocity() > 0)
            {
                Characters.Sein.Inventory.IncRandomizerItem(ID, -1);
                Randomizer.showHint("Skill Velocity Upgrade x" + RandomizerBonus.Velocity().ToString());
                return;
            }
            break;
        case 34:
            Characters.Sein.Inventory.SetRandomizerItem(34, 1);
            Randomizer.showHint("Return to start disabled!");
        break;
        case 35:
            Characters.Sein.Inventory.SetRandomizerItem(34, 0);
            Randomizer.showHint("Return to start enabled!");
        break;
        case 36:
            Randomizer.showHint("Underwater Skill Usage");
            Characters.Sein.Inventory.SetRandomizerItem(36, 1);
            break;
        case 40:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@Wall Jump Lost!!@", 240);
            Characters.Sein.PlayerAbilities.WallJump.HasAbility = false;
            return;
        case 41:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@ChargeFlame Lost!!@", 240);
            Characters.Sein.PlayerAbilities.ChargeFlame.HasAbility = false;
            return;
        case 42:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@DoubleJump Lost!!@", 240);
            Characters.Sein.PlayerAbilities.DoubleJump.HasAbility = false;
            return;
        case 43:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@Bash Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Bash.HasAbility = false;
            return;
        case 44:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@Stomp Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Stomp.HasAbility = false;
            return;
        case 45:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@Glide Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Glide.HasAbility = false;
            return;
        case 46:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@Climb Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Climb.HasAbility = false;
            return;
        case 47:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@Charge Jump Lost!!@", 240);
            Characters.Sein.PlayerAbilities.ChargeJump.HasAbility = false;
            return;
        case 48:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@Dash Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Dash.HasAbility = false;
            return;
        case 49:
            if (!Characters.Sein || flag)
                return;
            Randomizer.showHint("@Grenade Lost!!@", 240);
            Characters.Sein.PlayerAbilities.Grenade.HasAbility = false;
            return;
        case 81:
            Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
            string s_color = "";
            string g_color = "";
            if(Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Stomp))
                s_color = "$";
            if(Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Grenade))
                g_color = "$";
            Randomizer.showHint(s_color + "Stomp: " + Randomizer.StompZone + s_color + g_color+ "    Grenade: "+ Randomizer.GrenadeZone + g_color, 480);
            break;
        default:
            return;
        }
    }

    public static bool ForlornEscapeHint()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(81) > 0;
    }

    // Token: 0x0600376E RID: 14190 RVA: 0x0002B9F1 File Offset: 0x00029BF1
    public static bool DoubleAirDash()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(10) > 0;
    }

    // Token: 0x0600376F RID: 14191 RVA: 0x0002BA07 File Offset: 0x00029C07
    public static bool ChargeDashEfficiency()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(11) > 0;
    }

    // Token: 0x06003770 RID: 14192 RVA: 0x0002BA1D File Offset: 0x00029C1D
    public static bool DoubleJumpUpgrade()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(12) > 0;
    }

    // Token: 0x06003771 RID: 14193 RVA: 0x0002BA33 File Offset: 0x00029C33
    public static int HealthRegeneration()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(13);
    }

    // Token: 0x06003772 RID: 14194 RVA: 0x0002BA46 File Offset: 0x00029C46
    public static int EnergyRegeneration()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(15);
    }

    // Token: 0x06003774 RID: 14196 RVA: 0x0002BA59 File Offset: 0x00029C59
    public static int WaterVeinShards()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(17);
    }

    // Token: 0x06003775 RID: 14197 RVA: 0x0002BA6C File Offset: 0x00029C6C
    public static int SunstoneShards()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(21);
    }

    // Token: 0x06003776 RID: 14198 RVA: 0x0002BA7F File Offset: 0x00029C7F
    public static int GumonSealShards()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(19);
    }

    // Token: 0x06003777 RID: 14199 RVA: 0x0002BA92 File Offset: 0x00029C92
    public static int SpiritFlameLevel()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(6);
    }

    // Token: 0x06003778 RID: 14200 RVA: 0x0002BAA4 File Offset: 0x00029CA4
    public static int MapStoneProgression()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(23);
    }

    // Token: 0x06003779 RID: 14201 RVA: 0x0002BAB7 File Offset: 0x00029CB7
    public static int SkillTreeProgression()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(27);
    }

    // Token: 0x0600377A RID: 14202 RVA: 0x0002BACA File Offset: 0x00029CCA
    public static bool ExplosionPower()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(8) > 0;
    }

    // Token: 0x0600377B RID: 14203 RVA: 0x0002BADF File Offset: 0x00029CDF

    public static int ExpWithBonuses(int baseExp)
    {
        float mult = 1.0f + Characters.Sein.Inventory.GetRandomizerItem(9);
        if(Characters.Sein.PlayerAbilities.AbilityMarkers.HasAbility) 
            mult += .5f;
        if(Characters.Sein.PlayerAbilities.SoulEfficiency.HasAbility)
            mult += .5f;
        int total = (int)(baseExp*mult);
        RandomizerStatsManager.OnExp(baseExp, total-baseExp);
        return total;
    }

    // Token: 0x0600377C RID: 14204 RVA: 0x0002BAF5 File Offset: 0x00029CF5
    public static void CollectPickup()
    {
        Characters.Sein.Inventory.IncRandomizerItem(0, 1);
    }

    // Token: 0x0600377D RID: 14205 RVA: 0x0002BB08 File Offset: 0x00029D08
    public static int GetPickupCount()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(0);
    }

    // Token: 0x0600377E RID: 14206 RVA: 0x0002BB1A File Offset: 0x00029D1A
    public static int DoubleJumpUpgrades()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(12);
    }

    // Token: 0x0600377F RID: 14207 RVA: 0x0002BB2D File Offset: 0x00029D2D
    public static int UpgradeCount(int ID)
    {
        return Characters.Sein.Inventory.GetRandomizerItem(ID);
    }

    // Token: 0x06003780 RID: 14208 RVA: 0x0002BB3F File Offset: 0x00029D3F
    public static void CollectMapstone()
    {
        Characters.Sein.Inventory.IncRandomizerItem(23, 1);
        RandomizerBonus.CollectPickup();
    }

    // Token: 0x06003783 RID: 14211 RVA: 0x000E3050 File Offset: 0x000E1250
    public static void Update()
    {
        Characters.Sein.Mortality.Health.GainHealth((float)(RandomizerBonus.HealthRegeneration() + ((!Characters.Sein.PlayerAbilities.HealthMarkers.HasAbility) ? 0 : 2)) * 0.00112f);
        if (RandomizerBonus.Bleeding() > 0)
        {
            Characters.Sein.Mortality.Health.LoseHealth((float)RandomizerBonus.Bleeding() * 0.00112f);
        }
        if (RandomizerBonus.Bleeding() > 0 && Characters.Sein.Mortality.Health.Amount <= 0f)
        {
            Characters.Sein.Mortality.DamageReciever.OnRecieveDamage(new Damage(1f, default(Vector2), default(Vector3), DamageType.Water, null));
        }
        Characters.Sein.Energy.Gain((float)(RandomizerBonus.EnergyRegeneration() + ((!Characters.Sein.PlayerAbilities.EnergyMarkers.HasAbility) ? 0 : 2)) *  0.00028f);
        RandomizerBonusSkill.Update();
    }

    // Token: 0x06003784 RID: 14212 RVA: 0x000E314C File Offset: 0x000E134C
    public static void DamageDealt(float damage)
    {
        if (Characters.Sein)
        {
            if (damage > 20f)
            {
                damage = 20f;
            }
            Characters.Sein.Mortality.Health.GainHealth((float)RandomizerBonus.Lifesteal() * 0.8f * damage);
            Characters.Sein.Energy.Gain((float)RandomizerBonus.Manavamp() * 0.2f * damage);
        }
    }

    // Token: 0x06003785 RID: 14213 RVA: 0x0002BB91 File Offset: 0x00029D91
    public static int Bleeding()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(30);
    }

    public static bool ExpEfficiency()
    {
        return false;
    }

    // Token: 0x06003786 RID: 14214 RVA: 0x0002BBA4 File Offset: 0x00029DA4
    public static int Lifesteal()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(31);
    }

    // Token: 0x06003787 RID: 14215 RVA: 0x0002BBB7 File Offset: 0x00029DB7
    public static int Manavamp()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(32);
    }

    // Token: 0x06003788 RID: 14216 RVA: 0x0002BBCA File Offset: 0x00029DCA
    public static int Velocity()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(33);
    }

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
        return refund;
    }

    // Token: 0x06003789 RID: 14217 RVA: 0x0002BBDD File Offset: 0x00029DDD
    public static int WarmthFrags()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(28);
    }
    public static bool AltRDisabled()
    {
        return Characters.Sein.Inventory.GetRandomizerItem(34) == 1;
    }

    // Token: 0x04003262 RID: 12898
    public static bool DoubleAirDashUsed;
}
