using System;
using System.Linq;
using System.Collections.Generic;
using Game;
using UnityEngine;

// Token: 0x02000A11 RID: 2577
public static class RandomizerBonusSkill
{
    // Token: 0x060037F8 RID: 14328
    public static void SwitchBonusSkill()
    {
        Dictionary<int, int> unlocked = new Dictionary<int, int>(UnlockedBonusSkills);
        int slot_0 = unlocked.Keys.Min();
        int slot_n = unlocked.Keys.Max();
        if(ActiveBonus == 0)
        {
            if(unlocked.Count == 0) {
                Randomizer.Print("No bonus skills unlocked!",  3, false, false, false, true);
                return;
            }
            ActiveBonus = unlocked[slot_0];
        }
        int active_slot = get(ActiveBonus) >> 2;
        int cur_slot = active_slot;
        if (unlocked.Count == 1)
        {
                Randomizer.Print("Bonus Skill (" + (cur_slot+1).ToString() + "): " + BonusSkillNames[unlocked[cur_slot]],  3, false, false, false, true);
            return;
        }
        while (cur_slot < slot_n)
        {
            cur_slot++;
            if (unlocked.ContainsKey(cur_slot) && get(unlocked[cur_slot]) > 0)
            {
                ActiveBonus = unlocked[cur_slot];
                Randomizer.Print("Bonus Skill (" + (cur_slot+1).ToString() + "): " + BonusSkillNames[unlocked[cur_slot]],  3, false, false, false, true);
                return;
            }
        }
        cur_slot = slot_0;
        while (cur_slot < active_slot)
        {
            if (unlocked.ContainsKey(cur_slot) && get(unlocked[cur_slot]) > 0)
            {
                ActiveBonus = unlocked[cur_slot];
                Randomizer.Print("Bonus Skill (" + (cur_slot+1).ToString() + "): " + BonusSkillNames[unlocked[cur_slot]],  3, false, false, false, true);
                return;
            }
            cur_slot++;
        }
    }

    public static void BonusSkillSlot(int slot)
    {
        int bonus = -1;
        UnlockedBonusSkills.TryGetValue(slot, out bonus);
        if(bonus < 0)
        {
            Randomizer.Print("No bonus skill in slot "+ (1+slot).ToString(),  3, false, false, false, true);
            return;
        }
        ActivateBonusSkill(bonus);
    }

    public static void ActivateBonusSkill()
    {
        int ab = ActiveBonus;
        ActivateBonusSkill(ab);
    }

    public static void ActivateBonusSkill(int ab)
    {
        if (!Characters.Sein || Characters.Sein.IsSuspended || ab == 0)
        {
            return;
        }
        switch (ab)
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
        case 103:
            if (IsActive(ab))
            {
                Deactivate(ab);
                Randomizer.printInfo(BonusSkillNames[ab] + " off");
                RandomizerBonusSkill.EnergyDrainRate -= 0.00112f;
            } else if (Characters.Sein.Energy.Current > 0.0112f)
            {
                Activate(ab);
                Randomizer.printInfo(BonusSkillNames[ab] + " on");
                RandomizerBonusSkill.EnergyDrainRate += 0.00112f;
            } else {
                UI.SeinUI.ShakeEnergyOrbBar();
                Characters.Sein.Energy.NotifyOutOfEnergy();
                return;                
            }
            if(ab == 102) {
                Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle += 180f;
                Characters.Sein.PlatformBehaviour.LeftRightMovement.PlatformMovement.LocalSpeedX *= -1;
            }
            break;
        case 104:
            if (!CanWarpTo(LastAltR))
                return;
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
            if (!CanWarpTo(LastSoulLink))
                return;
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
                Randomizer.Print("You can only Respec at a Soul Link!",  3, false, false, false, true);
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
        case 108:
            if (IsActive(ab))
            {
                Deactivate(ab);
                Randomizer.printInfo("Skill Velocity on");
            }
            else
            {
                Activate(ab);
                Randomizer.printInfo("Skill Velocity off");
            }
            return;
        default:
            return;
        }
    }
    static bool CanWarpTo(Vector3 target) {
        return !(Characters.Sein.Abilities.Carry.IsCarrying || !Characters.Sein.Controller.CanMove || !Characters.Sein.Active || (target.x == 0f && target.y == 0f));
    }
    // Token: 0x060037FA RID: 14330
    static RandomizerBonusSkill()
    {
        RandomizerBonusSkill.Reset();
    }

    // Token: 0x060037FB RID: 14331
    public static void Update()
    {
        if (!Characters.Sein.IsSuspended && Characters.Sein.Controller.CanMove && Characters.Sein.Active && RandomizerBonusSkill.EnergyDrainRate > 0f)
        {
            if (RandomizerBonusSkill.EnergyDrainRate > Characters.Sein.Energy.Current)
            {
                foreach(int ds in ActiveDrainSkills)
                {
                    Deactivate(ds);
                    if(ds == 102) {
                        Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle += 180f;
                        Characters.Sein.PlatformBehaviour.LeftRightMovement.PlatformMovement.LocalSpeedX *= -1;
                    }
                }
                UpdateDrain();
                return;
            }
            Characters.Sein.Energy.Spend(RandomizerBonusSkill.EnergyDrainRate);
        }
    }

    // Token: 0x060037FC RID: 14332
    // Token: 0x060037FD RID: 14333
    public static void OnSave()
    {
        UpdateDrain();
    }

    // Token: 0x060037FE RID: 14334
    public static void OnDeath()
    {
        UpdateDrain();
    }

    // Token: 0x06003801 RID: 14337
    public static void FoundBonusSkill(int ID)
    {
        if(get(ID) > 0) {
            return;
        }
        if(ID != 108)
            Randomizer.showHint("Unlocked Bonus Skill: " + RandomizerBonusSkill.BonusSkillNames[ID]);
        int offset = 0;
        Dictionary<int, int> ubs = new Dictionary<int, int>(UnlockedBonusSkills);
        if(ubs.Count > 0) 
        offset = (1+UnlockedBonusSkills.Keys.Max()) << 2;
        set(ID, offset+1);
        if(ActiveBonus == 0)
            ActiveBonus = ID;
    }

    // Token: 0x06003802 RID: 14338
    public static void Reset()
    {   
        UpdateDrain();
    }

    public static void UpdateDrain() {
        if(!Characters.Sein || !Characters.Sein.Inventory)
            return;
        HashSet<int> ads = new HashSet<int>(ActiveDrainSkills);
        EnergyDrainRate = ads.Count * 0.00112f;

        if(ads.Contains(103)) {
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.MaxSpeed = 35f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.Acceleration = 90f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.Decceleration = 45f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.MaxSpeed = 35f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.Acceleration = 39f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.Decceleration = 39f;
        } else {
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.Acceleration = 60f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.Decceleration = 30f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Ground.MaxSpeed = 11.6666f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.Acceleration = 26f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.Decceleration = 26f;
            Characters.Sein.PlatformBehaviour.LeftRightMovement.Settings.Air.MaxSpeed = 11.6666f;
        }
    }

    // Token: 0x040032C8 RID: 13000
    public static int ActiveBonus 
    {
        get { return get(1589); }
        set { set(1589, value); }
    }
    public static int LevelExplosionCooldown = 0;
    public static Vector3 LastAltR
    {
        get { return new Vector3(
            ((float)get(84))/100f, 
            ((float)get(85))/100f
        );}
        set { 
            set(84, (int)(value.x*100));
            set(85, (int)(value.y*100));
        }
    }
    public static Vector3 LastSoulLink
    {
        get { return new Vector3(
            ((float)get(86))/100f, 
            ((float)get(87))/100f
        );}
        set { 
            set(86, (int)(value.x*100));
            set(87, (int)(value.y*100));
        }
    }
    public static float OldHealth;
    public static float OldEnergy;

    // Token: 0x040032C9 RID: 13001
    public static Dictionary<int, int> UnlockedBonusSkills
    {
        get {
            Dictionary<int, int> ubs = new Dictionary<int, int>();
            foreach(int id in BonusSkillNames.Keys) {
                int val = get(id);
                if(val % 2 == 1)
                {
                    if(ubs.ContainsKey(val >> 2)) {
                        Randomizer.LogError("Duplicate keys: " + ubs[val >> 2].ToString() + " and " + id.ToString());
                    }
                    ubs[val >> 2] = id;
                }
            }
            
            return ubs;
        }
    }
    public static float ExtremeSpeed
    {
        get {
            return IsActive(103) ? 3.0f : 1.0f;
        }
    }

    public static float EnergyDrainRate;
    public static bool IsActive(int id) {
        try {
            return (get(id) >> 1) % 2 == 1;
        }
        catch(Exception e)
        {
            Randomizer.LogError("IsActive: " + e.Message);
            return false;
        }
    }
    public static void Deactivate(int id) {
        if(IsActive(id))
            set(id, get(id)-2);
        else
            if(RandomizerSettings.Dev)
                Randomizer.log("ignoring deactivation of " + id.ToString() + " since skill was not active");
        UpdateDrain();
    }
    public static void Activate(int id) {
        if(!IsActive(id))
            set(id, get(id)+2);
        else
            if(RandomizerSettings.Dev)
                Randomizer.log("ignoring activation of " + id.ToString() + " since skill was already active");
        UpdateDrain();
    }

    public static HashSet<int> ActiveDrainSkills
    {
        get {
            HashSet<int> ads = new HashSet<int>();
            foreach(int id in DrainSkills) {
                if(IsActive(id))
                    ads.Add(id);
            }
            return ads;
        }
    }

    private static int get(int item) { return Characters.Sein.Inventory.GetRandomizerItem(item); }
    private static int set(int item, int value) { return Characters.Sein.Inventory.SetRandomizerItem(item, value); }
    // Token: 0x040032CD RID: 13005
    public static Dictionary<int, string> BonusSkillNames = new Dictionary<int, string>
    {
        { 101, "Polarity Shift" },
        { 102, "Gravity Swap" },
        { 103, "Extreme Speed" },
        { 104, "Teleport to Last AltR" },
        { 105, "Teleport to Soul Link" },
        { 106, "Respec" },
        { 107, "Level Explosion" },
        { 108, "Toggle Skill Velocity" }
    };

    public static HashSet<int> DrainSkills = new HashSet<int> { 102, 103 };
}

