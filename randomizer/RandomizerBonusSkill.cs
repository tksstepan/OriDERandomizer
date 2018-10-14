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
        if (RandomizerBonusSkill.UnlockedBonusSkills.Count < 1)
        {
            RandomizerBonusSkill.Reset();
            if (RandomizerBonusSkill.UnlockedBonusSkills.Count < 1)
            {
                Randomizer.MessageQueue.Enqueue("No bonus skills unlocked!");
                return;
            }
        }
        RandomizerBonusSkill.ActiveBonus = (RandomizerBonusSkill.ActiveBonus + 1) % RandomizerBonusSkill.UnlockedBonusSkills.Count;
        Randomizer.MessageQueue.Enqueue("Active Bonus Skill: " + RandomizerBonusSkill.CurrentBonusName());
    }

    // Token: 0x060037F9 RID: 14329
    public static void ActivateBonusSkill()
    {
        if (RandomizerBonusSkill.UnlockedBonusSkills.Count == 0)
        {
            return;
        }
        int item = RandomizerBonusSkill.CurrentBonus();
        switch (item)
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
            if (RandomizerBonusSkill.ActiveDrainSkills.Contains(item))
            {
                RandomizerBonusSkill.ActiveDrainSkills.Remove(item);
                Randomizer.MessageQueue.Enqueue("Gravity Shift off");
                Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle = 0f;
                RandomizerBonusSkill.EnergyDrainRate -= 0.001f;
                return;
            }
            if (Characters.Sein.Energy.Current > 0f)
            {
                RandomizerBonusSkill.ActiveDrainSkills.Add(item);
                Randomizer.MessageQueue.Enqueue("Gravity Shift on");
                Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle = 180f;
                RandomizerBonusSkill.EnergyDrainRate += 0.001f;
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        case 103:
            if (RandomizerBonusSkill.ActiveDrainSkills.Contains(item))
            {
                RandomizerBonusSkill.ActiveDrainSkills.Remove(item);
                Randomizer.MessageQueue.Enqueue("ExtremeSpeed off");
                Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.MaxSpeed = 11.6666f;
                Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.MaxSpeed = 11.6666f;
                RandomizerBonusSkill.EnergyDrainRate -= 0.001f;
                return;
            }
            if (Characters.Sein.Energy.Current > 0f)
            {
                RandomizerBonusSkill.ActiveDrainSkills.Add(item);
                Randomizer.MessageQueue.Enqueue("ExtremeSpeed on");
                Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.MaxSpeed = 40f;
                Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.MaxSpeed = 40f;
                RandomizerBonusSkill.EnergyDrainRate += 0.001f;
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        case 104:
            if (Characters.Sein.Abilities.Carry.IsCarrying || !Characters.Sein.Controller.CanMove || !Characters.Sein.Active || (Randomizer.LastReturnPoint.x == 0f && Randomizer.LastReturnPoint.y == 0f))
            {
                return;
            }
            if (Characters.Sein.Energy.Current >= 0.5f)
            {
                Characters.Sein.Energy.SetCurrent(Characters.Sein.Energy.Current - 0.5f);
                Randomizer.Warping = 5;
                Randomizer.WarpTarget = Randomizer.LastReturnPoint;
                Characters.Sein.Position = Randomizer.LastReturnPoint;
                Characters.Sein.Speed = new Vector3(0f, 0f);
                Characters.Ori.Position = Randomizer.LastReturnPoint;
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        case 105:
            if (Characters.Sein.Abilities.Carry.IsCarrying || !Characters.Sein.Controller.CanMove || !Characters.Sein.Active || (Randomizer.LastSoulLink.x == 0f && Randomizer.LastSoulLink.y == 0f))
            {
                return;
            }
            if (Characters.Sein.Energy.Current >= 0.5f)
            {
                Characters.Sein.Energy.SetCurrent(Characters.Sein.Energy.Current - 0.5f);
                Randomizer.Warping = 5;
                Randomizer.WarpTarget = Randomizer.LastSoulLink;
                Characters.Sein.Position = Randomizer.LastSoulLink;
                Characters.Sein.Speed = new Vector3(0f, 0f);
                Characters.Ori.Position = Randomizer.LastSoulLink;
                return;
            }
            UI.SeinUI.ShakeEnergyOrbBar();
            Characters.Sein.Energy.NotifyOutOfEnergy();
            return;
        case 106:
            if (!Characters.Sein.SoulFlame.AllowedToAccessSkillTree)
            {
                Randomizer.showHint("You can only Respec at a Soul Link!");
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
        default:
            return;
        }
    }

    // Token: 0x060037FA RID: 14330
    static RandomizerBonusSkill()
    {
        RandomizerBonusSkill.UnlockedBonusSkills = new List<int>();
        RandomizerBonusSkill.BonusSkillsLastSave = new List<int>();
        RandomizerBonusSkill.Reset();
    }

    // Token: 0x060037FB RID: 14331
    public static void Update()
    {
        if (RandomizerBonusSkill.EnergyDrainRate > 0f)
        {
            if (RandomizerBonusSkill.EnergyDrainRate > Characters.Sein.Energy.Current)
            {
                RandomizerBonusSkill.EnergyDrainRate = 0f;
                RandomizerBonusSkill.DisableAllPersistant();
                return;
            }
            Characters.Sein.Energy.SetCurrent(Characters.Sein.Energy.Current - RandomizerBonusSkill.EnergyDrainRate);
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
        RandomizerBonusSkill.BonusSkillsLastSave = new List<int>(RandomizerBonusSkill.UnlockedBonusSkills);
    }

    // Token: 0x060037FE RID: 14334
    public static void OnDeath()
    {
        RandomizerBonusSkill.DisableAllPersistant();
        RandomizerBonusSkill.UnlockedBonusSkills = new List<int>(RandomizerBonusSkill.BonusSkillsLastSave);
    }

    // Token: 0x060037FF RID: 14335
    public static int CurrentBonus()
    {
        if (RandomizerBonusSkill.UnlockedBonusSkills.Count > RandomizerBonusSkill.ActiveBonus)
        {
            return RandomizerBonusSkill.UnlockedBonusSkills[RandomizerBonusSkill.ActiveBonus];
        }
        return 0;
    }

    // Token: 0x06003800 RID: 14336
    public static string CurrentBonusName()
    {
        if (RandomizerBonusSkill.UnlockedBonusSkills.Count > RandomizerBonusSkill.ActiveBonus)
        {
            return RandomizerBonusSkill.BonusSkillNames[RandomizerBonusSkill.CurrentBonus()];
        }
        return "None";
    }

    // Token: 0x06003801 RID: 14337
    public static void FoundBonusSkill(int ID)
    {
        Randomizer.showHint("Unlocked Bonus Skill: " + RandomizerBonusSkill.BonusSkillNames[ID]);
        Characters.Sein.Inventory.IncRandomizerItem(ID, 1);
        if (!RandomizerBonusSkill.UnlockedBonusSkills.Contains(ID))
        {
            RandomizerBonusSkill.UnlockedBonusSkills.Add(ID);
            RandomizerBonusSkill.ActiveBonus = RandomizerBonusSkill.UnlockedBonusSkills.Count - 1;
        }
    }

    // Token: 0x06003802 RID: 14338
    public static void Reset()
    {
        RandomizerBonusSkill.UnlockedBonusSkills = new List<int>();
        RandomizerBonusSkill.BonusSkillsLastSave = new List<int>();
        RandomizerBonusSkill.ActiveDrainSkills = new HashSet<int>();
        RandomizerBonusSkill.EnergyDrainRate = 0f;
        foreach (int num in RandomizerBonusSkill.BonusSkillNames.Keys)
        {
            if (Characters.Sein && num >= 100 && Characters.Sein.Inventory.GetRandomizerItem(num) > 0)
            {
                RandomizerBonusSkill.UnlockedBonusSkills.Add(num);
            }
        }
        RandomizerBonusSkill.ActiveBonus = 0;
    }

    // Token: 0x040032C8 RID: 13000
    public static int ActiveBonus = 0;

    // Token: 0x040032C9 RID: 13001
    public static List<int> UnlockedBonusSkills;

    // Token: 0x040032CA RID: 13002
    public static float EnergyDrainRate;

    // Token: 0x040032CB RID: 13003
    public static HashSet<int> ActiveDrainSkills;

    // Token: 0x040032CC RID: 13004
    public static List<int> BonusSkillsLastSave;

    // Token: 0x040032CD RID: 13005
    public static Dictionary<int, string> BonusSkillNames = new Dictionary<int, string>
    {
        {
            101,
            "Polarity Shift"
        },
        {
            102,
            "Gravity Swap"
        },
        {
            103,
            "ExtremeSpeed"
        },
        {
            104,
            "Roose's Wind"
        },
        {
            105,
            "Respawn Without Dying"
        },
        {
            106,
            "Respec"
        }
    };
}
