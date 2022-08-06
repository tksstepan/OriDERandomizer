using System;
using System.Linq;
using System.Collections.Generic;
using Game;
using UnityEngine;

public static class RandomizerBonusSkill
{
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
        if (!Characters.Sein || Characters.Sein.IsSuspended || ab == 0 || RandomizerBonusSkill.get(ab) == 0)
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
        case 109:
            if (IsActive(ab))
            {
                Deactivate(ab);
                BonusSkillText(BonusSkillNames[ab] + " off");
                RandomizerBonusSkill.EnergyDrainRate -= DrainRates[ab];
            } else if (Characters.Sein.Energy.Current > 5*DrainRates[ab]) {
                Activate(ab);
                BonusSkillText(BonusSkillNames[ab] + " on");
                RandomizerBonusSkill.EnergyDrainRate += DrainRates[ab];
            } else {
                UI.SeinUI.ShakeEnergyOrbBar();
                Characters.Sein.Energy.NotifyOutOfEnergy();
                return;                
            }
            if(ab == 102) {
                if(IsActive(ab) || Characters.Sein.Abilities.Carry.IsCarrying)
                    Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle += 180f;
                else
                    Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle = 0f;
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
                BonusSkillText("You can only Respec on a Soul Link!");
                return;
            }
            {
                int apToGain = RandomizerBonus.ResetAP();
                if(apToGain == 0) {
                    BonusSkillText("No AP to refund");
                    return;
                }
                BonusSkillText("Respec successful. " + apToGain.ToString() + " AP refunded!");
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
        if (Characters.Sein.Energy.Current >= 1f || Characters.Sein.Inventory.GetRandomizerItem(1107) == 1)
            {
                if (Characters.Sein.Inventory.GetRandomizerItem(1107) == 0)
                {
                    Characters.Sein.Energy.Spend(1f);
                }
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
                BonusSkillText("Movement Bonuses on");
            }
            else
            {
                Activate(ab);
                BonusSkillText("Movement Bonuses off");
            }
            return;
        case 110:
            if (IsActive(ab))
            {
                Deactivate(ab);
                BonusSkillText(BonusSkillNames[ab] + " off");
                RandomizerBonusSkill.EnergyDrainRate -= DrainRates[ab];
            } else if (Characters.Sein.Energy.Current > 1f)
            {
                Activate(ab);
                Characters.Sein.Energy.Spend(0.5f);
                BonusSkillText(BonusSkillNames[ab] + " on");
                RandomizerBonusSkill.EnergyDrainRate += DrainRates[ab];
            } else {
                UI.SeinUI.ShakeEnergyOrbBar();
                Characters.Sein.Energy.NotifyOutOfEnergy();
                return;                
            }
        break;
        case 111:
                Characters.Sein.Mortality.DamageReciever.OnRecieveDamage(new Damage(9000f, new Vector2(0, 0), Characters.Sein.Position, DamageType.Lava, Characters.Sein.GameObject));
        break;
        case 112:
            if(CapturedEnemy == null)
            {
                BonusSkillNames[112] = "Pokeball (empty)";
                Characters.Sein.Abilities.SpiritFlameTargetting.UpdateClosestAttackables();
                foreach(ISpiritFlameAttackable target in Characters.Sein.Abilities.SpiritFlameTargetting.ClosestAttackables)
                {
                    string logMessage = target.GetType().ToString();
                    if(target is EntityTargetting)
                    {
                        var enemyTarget = target as EntityTargetting;
                        var entity = enemyTarget.Entity;
                        if(entity is JumperEnemy) 
                            CapturedName = "Fronkey";
                        else if(entity is FishEnemy)
                            CapturedName = "Fish";
                        else if(entity is SpitterEnemy)
                            CapturedName = "Frog";
                        else if(entity is KamikazeSootEnemy)
                            CapturedName = "Baneling";
                        else if(entity is DashOwlEnemy)
                            CapturedName = "Bird";
                        else
                            return;
                        CapturedEnemy = entity as Enemy;
                        Randomizer.LogError(CapturedEnemy.BoundingBox.ToString());
                        CapturedOffset = CapturedEnemy.PositionToPlayerPosition;
                        CapturedLeft = Characters.Sein.FaceLeft;
                        CapturedEnemy.gameObject.SetActiveRecursively(false);
                        //Events.Scheduler.OnSceneRootDisabled.Remove(new Action<SceneRoot>(CapturedEnemy.OnSceneUnloaded));
                        BonusSkillNames[112] = "Pokeball ("+CapturedName+")";
                        
                    }
                }
            } else {
                    if(CapturedLeft != Characters.Sein.FaceLeft)
                        CapturedOffset.x *= -1;
                    CapturedEnemy.Position = Characters.Sein.Position + CapturedOffset;
                    CapturedEnemy.gameObject.SetActiveRecursively(true);
                    CapturedEnemy = null;
                    BonusSkillNames[112] = "Pokeball (empty)";
            }
        break;
        case 114:
            if (IsActive(ab))
            {
                Deactivate(ab);
                BonusSkillText("Ori");
            }
            else
            {
                Activate(ab);
                BonusSkillText("Naru");
            }
            return;
        break;
        case 1587:
            if (!Characters.Sein.Controller.CanMove || !Characters.Sein.Active)
                return;
            Randomizer.WarpTo(new Vector3(-2478,-593, 0), 0);
            GameController.Instance.RemoveGameplayObjects();
            RandomizerStatsManager.Active = false;
            RandomizerCreditsManager.Initialize();
        break;
        case 113:
            if (IsActive(ab))
            {
                Deactivate(ab);
                BonusSkillText("Bash/Stomp Damage on");
            } else {
                Activate(ab);
                BonusSkillText("Bash/Stomp Damage off");
            }
        break;
        default:
            return;
        }
    }
    
    static bool CanWarpTo(Vector3 target) {
        return !((Characters.Sein.Abilities.Carry.IsCarrying && !Randomizer.AllowOrbWarps) || !Characters.Sein.Controller.CanMove || !Characters.Sein.Active || (target.x == 0f && target.y == 0f));
    }

    static RandomizerBonusSkill()
    {
        RandomizerBonusSkill.Reset();
    }

    public static void Update()
    {
        if (!Characters.Sein.IsSuspended && Characters.Sein.Controller.CanMove && Characters.Sein.Active)
        {
            if(DrainNextUpdate > 0)
                UpdateDrain();
            if (RandomizerBonusSkill.EnergyDrainRate > Characters.Sein.Energy.Current)
            {
                foreach(int ds in ActiveDrainSkills)
                {
                    Deactivate(ds);
                    if(ds == 102) {
                        if(Characters.Sein.Abilities.Carry.IsCarrying)
                            Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle += 180f;
                        else
                            Characters.Sein.PlatformBehaviour.Gravity.BaseSettings.GravityAngle = 0f;

                        Characters.Sein.PlatformBehaviour.LeftRightMovement.PlatformMovement.LocalSpeedX *= -1;
                    }
                }
                BonusSkillText("Out of energy! Bonus skills disabled.");
                UpdateDrain();
                return;
            }
            if(RandomizerBonusSkill.EnergyDrainRate > 0f)
                Characters.Sein.Energy.Spend(RandomizerBonusSkill.EnergyDrainRate);
        } 
    }

    public static void OnSave()
    {
        SaveEnemy = CapturedEnemy;
        SaveOffset = CapturedOffset;
        SaveLeft = CapturedLeft;
        SaveName = CapturedName;

        UpdateDrain();
    }

    public static void OnDeath()
    {
        CapturedEnemy = SaveEnemy;
        CapturedOffset = SaveOffset;
        CapturedLeft = SaveLeft;
        CapturedName = SaveName;
        if(CapturedName != null)
            BonusSkillNames[112] = "Pokeball ("+CapturedName+")";
        else
            BonusSkillNames[112] = "Pokeball (empty)";

        UpdateDrain();
    }

    public static void FoundBonusSkill(int ID)
    {
        bool psuedo = (ID == 108 || ID == 1587);
        if(get(ID) > 0) {
            if(!psuedo)
                RandomizerSwitch.PickupMessage(RandomizerBonusSkill.BonusSkillNames[ID] + " (duplicate)");
            return;
        }
        if(!psuedo)
            RandomizerSwitch.PickupMessage("Unlocked Bonus Skill: " + RandomizerBonusSkill.BonusSkillNames[ID]);
        int offset = 0;
        Dictionary<int, int> ubs = new Dictionary<int, int>(UnlockedBonusSkills);
        if(ubs.Count > 0) 
        offset = (1+ubs.Keys.Max()) << 2;
        set(ID, offset+1);
        if(ActiveBonus == 0)
            ActiveBonus = ID;
    }

    public static void Reset()
    {   
        UpdateDrain();
    }

    public static void UpdateDrain() {
        try
        {
            if(!Characters.Sein || !Characters.Sein.Inventory || !Characters.Sein.PlatformBehaviour)
                return;
            HashSet<int> ads = new HashSet<int>(ActiveDrainSkills);

            EnergyDrainRate = 0f;
            foreach(int ds in ads) {
                EnergyDrainRate += DrainRates[ds];
            }

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
            DrainNextUpdate -= 1;
        }
        catch(Exception e) {
            Randomizer.LogError("Update Drain: " + e.Message);
        }
    }

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
    public static bool Invincible
    {
        get {
            return IsActive(110);
        }
    }
    public static float EnergyDrainRate;
    public static bool IsActive(int id) {
        try {
            if(!Characters.Sein)
                return false;
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
            foreach(int id in DrainRates.Keys) {
                if(IsActive(id))
                    ads.Add(id);
            }
            return ads;
        }
    }

    private static int get(int item) { return Characters.Sein.Inventory.GetRandomizerItem(item); }
    private static int set(int item, int value) { return Characters.Sein.Inventory.SetRandomizerItem(item, value); }
    public static Dictionary<int, string> BonusSkillNames = new Dictionary<int, string>
    {
        { 101, "Polarity Shift" },
        { 102, "Gravity Swap" },
        { 103, "Extreme Speed" },
        { 104, "Teleport to Last AltR" },
        { 105, "Teleport to Soul Link" },
        { 106, "Respec" },
        { 107, "Level Explosion" },
        { 108, "Toggle Movement Bonuses" },
        { 109, "Timewarp" },
        { 110, "Invincibility" },
        { 111, "Wither" },
        { 112, "Pokeball" },
        { 113, "Toggle Bash/Stomp Damage" },
        { 114, "Summon Mom" },
        { 1587, "Warp to Credits" },
    };
    public static Dictionary <int, float> DrainRates = new Dictionary<int, float>
    {
        { 102, 0.00112f },
        { 103, 0.00112f },
        { 109, 0.00112f },
        { 110, 0.01667f },
        { 114, 0f },
    };

    public static float AbilityDamage(float orig) {
        if(IsActive(113))
            return 0;
        return orig;
    }

    public static float TimewarpFactor = 0.5f;

    public static float TimeScale(float orig)
    {
        return IsActive(109) ? orig * TimewarpFactor : orig;
    }

    public static Vector3 TimeScale(Vector3 orig)
    {
        return orig * TimeScale(1.0f);
    }
    public static Vector2 TimeScale(Vector2 orig)
    {
        return orig * TimeScale(1.0f);
    }
    public static void TimeScale(Animator animator)
    {
        animator.speed = TimeScale(1.0f);
    }
    public static void BonusSkillText(string text) {
        Randomizer.Print(text, 3, false, false, false, true);
    }
    public static bool UnlockCreditWarp(string message) {
        if(get(1587) > 0)
            return false;
        FoundBonusSkill(1587);
        ActiveBonus = 1587;
        message += "\nPress " + RandomizerRebinding.BonusToggle.FirstBindName() + " to warp to credits";
        Randomizer.Print(message, 10, false, true, false, false);
        RandomizerStatsManager.WriteStatsFile();
        return true;
    }
    public static void DelayDrainUpdate() {
        DrainNextUpdate = 5;
    }
    private static int DrainNextUpdate = 0;
    public static Enemy SaveEnemy;
    public static Vector3 SaveOffset;
    public static bool SaveLeft;
    public static string SaveName;
    public static Enemy CapturedEnemy;
    public static Vector3 CapturedOffset;
    public static bool CapturedLeft;
    public static string CapturedName;
}
