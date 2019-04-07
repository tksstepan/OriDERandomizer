using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

// Token: 0x02000A11 RID: 2577
public static class RandomizerBonusSkill
{
    // Token: 0x060037F8 RID: 14328
    public static void SwitchBonusSkill()
    {
        if(ActiveBonus == 0)
        {
            foreach (int skillId in BonusSkillNames.Keys)
            {
                if(Characters.Sein.Inventory.GetRandomizerItem(skillId) > 0)
                {
                    ActiveBonus = skillId;
                    Randomizer.printInfo("Active Bonus Skill: " + BonusSkillNames[ActiveBonus]);
                    return;
                }
            }
                Randomizer.printInfo("No bonus skills unlocked!");
                return;            
        }
        int newBonus = ActiveBonus;
        while(newBonus < LastId)
        {
            newBonus++;
            if(Characters.Sein.Inventory.GetRandomizerItem(newBonus) > 0)
            {
                ActiveBonus = newBonus;
                Randomizer.printInfo("Active Bonus Skill: " + BonusSkillNames[ActiveBonus]);
                return;
            }
        }
        newBonus = FirstId;
        while(newBonus < ActiveBonus)
        {
            if(Characters.Sein.Inventory.GetRandomizerItem(newBonus) > 0)
            {
                ActiveBonus = newBonus;
                Randomizer.printInfo("Active Bonus Skill: " + BonusSkillNames[ActiveBonus]);
                return;
            }
            newBonus++;
        }
    }

    // Token: 0x060037F9 RID: 14329
    public static void ActivateBonusSkill()
    {
        if (!Characters.Sein || Characters.Sein.IsSuspended || ActiveBonus == 0)
        {
            return;
        }
        switch (ActiveBonus)
        {
        case 101:
            if (Characters.Sein.Energy.Current > 0f)
            {
                float amount = Characters.Sein.Energy.Current * 4f;
                Characters.Sein.Energy.SetCurrent(Characters.Sein.Mortality.Health.Amount / 4f);
                Characters.Sein.Mortality.Health.SetAmount(amount);
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        case 102:
            if (RandomizerBonusSkill.ActiveDrainSkills.Contains(ActiveBonus))
            {
                RandomizerBonusSkill.ActiveDrainSkills.Remove(ActiveBonus);
                Randomizer.printInfo(BonusSkillNames[ActiveBonus] + " off");
                Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle = 0f;
                RandomizerBonusSkill.EnergyDrainRate -= 0.001f;
                return;
            }
            if (Characters.Sein.Energy.Current > 0f)
            {
                RandomizerBonusSkill.ActiveDrainSkills.Add(ActiveBonus);
                Randomizer.printInfo(BonusSkillNames[ActiveBonus] + " on");
                Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle = 180f;
                RandomizerBonusSkill.EnergyDrainRate += 0.001f;
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        case 103:
            if (RandomizerBonusSkill.ActiveDrainSkills.Contains(ActiveBonus))
            {
                RandomizerBonusSkill.ActiveDrainSkills.Remove(ActiveBonus);
                Randomizer.printInfo(BonusSkillNames[ActiveBonus] + " off");
                Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.MaxSpeed = 11.6666f;
                Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.MaxSpeed = 11.6666f;
                RandomizerBonusSkill.EnergyDrainRate -= 0.001f;
                return;
            }
            if (Characters.Sein.Energy.Current > 0f)
            {
                RandomizerBonusSkill.ActiveDrainSkills.Add(ActiveBonus);
                Randomizer.printInfo(BonusSkillNames[ActiveBonus] + " on");
                Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.MaxSpeed = 40f;
                Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.MaxSpeed = 40f;
                RandomizerBonusSkill.EnergyDrainRate += 0.001f;
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        case 104:
            if (Characters.Sein.Abilities.Carry.IsCarrying || !Characters.Sein.Controller.CanMove || !Characters.Sein.Active || (LastAltR.x == 0f && LastAltR.y == 0f))
            {
                return;
            }
            if (Characters.Sein.Energy.Current >= 0.5f)
            {
               Characters.Sein.Energy.Spend(0.5f);
                Randomizer.WarpTo(LastAltR, 0);
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        case 105:
            if (Characters.Sein.Abilities.Carry.IsCarrying || !Characters.Sein.Controller.CanMove || !Characters.Sein.Active || (LastSoulLink.x == 0f && LastSoulLink.y == 0f))
            {
                return;
            }
            if (Characters.Sein.Energy.Current >= 0.5f)
            {
                Characters.Sein.Energy.Spend(0.5f);
                Randomizer.WarpTo(LastSoulLink, 0);
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        case 106:
            if (!Characters.Sein.SoulFlame.InsideCheckpointMarker)
            {
                Randomizer.printInfo("You can only Respec at a Soul Link!");
                return;
            }
            {
                int apToGain = RandomizerBonus.ResetAP();
                CharacterAbility[] abilities = Characters.Sein.PlayerAbilities.Abilities;
                List<CharacterAbility> actuallySkills = new List<CharacterAbility>() {
                    Characters.Sein.PlayerAbilities.WallJump,
                    Characters.Sein.PlayerAbilities.ChargeFlame,
                    Characters.Sein.PlayerAbilities.DoubleJump,
                    Characters.Sein.PlayerAbilities.Bash,
                    Characters.Sein.PlayerAbilities.Stomp,
                    Characters.Sein.PlayerAbilities.Climb,
                    Characters.Sein.PlayerAbilities.Glide,
                    Characters.Sein.PlayerAbilities.ChargeJump,
                    Characters.Sein.PlayerAbilities.Dash,
                    Characters.Sein.PlayerAbilities.Grenade,
                Characters.Sein.PlayerAbilities.SpiritFlame
                };
                for (int i = 0; i < abilities.Length; i++)
                {
                    if(!actuallySkills.Contains(abilities[i]))
                        abilities[i].HasAbility = false;
                }
                Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
                Characters.Sein.Inventory.SkillPointsCollected += apToGain;
                Characters.Sein.Level.SkillPoints += apToGain;
            }
            return;
        case 107:
        if(LevelExplosionCooldown > 0)
        {
            return;
        }
        if (Characters.Sein.Energy.Current >= 1f)
            {
                Characters.Sein.Energy.Spend(1f);
                OldHealth = Characters.Sein.Mortality.Health.Amount;
                OldEnergy = Characters.Sein.Energy.Current;
                Characters.Sein.Level.AttemptInstantiateLevelUp();
                LevelExplosionCooldown = 15;
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        default:
            return;
        }
    }

    // Token: 0x060037FA RID: 14330
    static RandomizerBonusSkill()
    {
        RandomizerBonusSkill.Reset();
    }

    // Token: 0x060037FB RID: 14331
    public static void Update()
    {
        if (!Characters.Sein.IsSuspended && RandomizerBonusSkill.EnergyDrainRate > 0f)
        {
            if (RandomizerBonusSkill.EnergyDrainRate > Characters.Sein.Energy.Current)
            {
                RandomizerBonusSkill.EnergyDrainRate = 0f;
                RandomizerBonusSkill.DisableAllPersistant();
                return;
            }
            Characters.Sein.Energy.Spend(RandomizerBonusSkill.EnergyDrainRate);
        }
    }

    // Token: 0x060037FC RID: 14332
    public static void DisableAllPersistant()
    {
        RandomizerBonusSkill.ActiveDrainSkills.Clear();
        RandomizerBonusSkill.EnergyDrainRate = 0f;
        Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.MaxSpeed = 11.6666f;
        Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.MaxSpeed = 11.6666f;
        Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle = 0f;
    }

    // Token: 0x060037FD RID: 14333
    public static void OnSave()
    {
    }

    // Token: 0x060037FE RID: 14334
    public static void OnDeath()
    {
        RandomizerBonusSkill.DisableAllPersistant();
    }

    // Token: 0x06003801 RID: 14337
    public static void FoundBonusSkill(int ID)
    {
        if(Characters.Sein.Inventory.GetRandomizerItem(ID) > 0) {
            return;
        }
        Randomizer.showHint("Unlocked Bonus Skill: " + RandomizerBonusSkill.BonusSkillNames[ID]);
        Characters.Sein.Inventory.SetRandomizerItem(ID, 1);
        if(ActiveBonus == 0)
            ActiveBonus = ID;
    }

    // Token: 0x06003802 RID: 14338
    public static void Reset()
    {       
        ActiveDrainSkills = new HashSet<int>();
        EnergyDrainRate = 0f;
    }

    // Token: 0x040032C8 RID: 13000
    public static int ActiveBonus 
    {
        get { return Characters.Sein.Inventory.GetRandomizerItem(83); }
        set { Characters.Sein.Inventory.SetRandomizerItem(83, value); }
    }
    public static int LevelExplosionCooldown = 0;
    public static Vector3 LastAltR
    {
        get { return new Vector3(
            ((float)Characters.Sein.Inventory.GetRandomizerItem(84))/100f, 
            ((float)Characters.Sein.Inventory.GetRandomizerItem(85))/100f
        );}
        set { 
            Characters.Sein.Inventory.SetRandomizerItem(84, (int)(value.x*100));
            Characters.Sein.Inventory.SetRandomizerItem(85, (int)(value.y*100));
        }
    }
    public static Vector3 LastSoulLink
    {
        get { return new Vector3(
            ((float)Characters.Sein.Inventory.GetRandomizerItem(86))/100f, 
            ((float)Characters.Sein.Inventory.GetRandomizerItem(87))/100f
        );}
        set { 
            Characters.Sein.Inventory.SetRandomizerItem(86, (int)(value.x*100));
            Characters.Sein.Inventory.SetRandomizerItem(87, (int)(value.y*100));
        }
    }
    public static float OldHealth;
    public static float OldEnergy;

    // Token: 0x040032C9 RID: 13001
    public static List<int> UnlockedBonusSkills;
    public static float EnergyDrainRate;
    public static HashSet<int> ActiveDrainSkills;

    public static int FirstId = 101;

    public static int LastId = 107;

    // Token: 0x040032CD RID: 13005
    public static Dictionary<int, string> BonusSkillNames = new Dictionary<int, string>
    {
        { 101, "Polarity Shift" },
        { 102, "Gravity Swap" },
        { 103, "Extreme Speed" },
        { 104, "Teleport to Last AltR" },
        { 105, "Teleport to Soul Link" },
        { 106, "Respec" },
        { 107, "Level Explosion" }
    };
}
