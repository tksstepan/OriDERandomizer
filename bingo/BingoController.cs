using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using Game;
using Core;
using Sein.World;

public static class BingoController
{
    private static string scene() {
        return Scenes.Manager.CurrentScene != null ? Scenes.Manager.CurrentScene.Scene : "" ;
    }
    private static string locStr() {
        string ret = " at ";
        if(Characters.Sein != null)
        {
            ret += "Pos: " + Characters.Sein.Position + ", ";
        }
            ret += "Scene: " + scene() + ", ";
        ret += "Zone: " + RandomizerStatsManager.CurrentZone();
        return ret;
    }

    public static void Tick() {
        try {
            if(!Active || Characters.Sein == null) return;
            if(Characters.Sein.Inventory.Keystones > IntGoals["UnspentKeystones"].Value)
                IntGoals["UnspentKeystones"].Value = Characters.Sein.Inventory.Keystones;
            if(CoreSkipTimeout > 0)
                CoreSkipTimeout--;
            string s = scene();
            if(s == "catAndMouseRight" && Characters.Sein.Position.x > 190f)
                MultiBoolGoals["CompleteEscape"]["Mount Horu"] = true;
            if(s != CurrentScene) {
                CurrentScene = s;
                if(SingleSceneListeners.ContainsKey(CurrentScene)) {
                    SingleSceneListeners[CurrentScene].Handle();
                }
                foreach(SceneListener listener in SceneListeners)
                    listener.Handle(CurrentScene);
            }
            if(UpdateTimer > 0)
                UpdateTimer--;
            else 
                PostUpdate();
        } catch(Exception e) {
            Randomizer.LogError("Bingo Tick: " + e.Message);
        }
            
    }

    public static void OnStompPost(MoonGuid guid) {
        if(!Active) return;
        if(SingleGuidSwitchListeners.ContainsKey(guid))
            SingleGuidSwitchListeners[guid].Handle();
//        if(RandomizerSettings.Dev) Randomizer.log("Stomped post, guid " + guid.ToString() + locStr()); 
    }

    public static void OnPurpleDoor(MoonGuid guid) {
        if(!Active) return;
        if(SingleGuidSwitchListeners.ContainsKey(guid))
            SingleGuidSwitchListeners[guid].Handle();
//        if(RandomizerSettings.Dev) Randomizer.log("opend purple door, guid " + guid.ToString() + locStr()); 
    }

    public static void OnLanternLit(MoonGuid guid, bool byGrenade) {
        if(!Active) return;
        if(SingleGuidSwitchListeners.ContainsKey(guid))
            SingleGuidSwitchListeners[guid].Handle();
        if(!BlackrootLanterns.Contains(guid))
            IntGoals["LightLanterns"].Value++;
//        if(RandomizerSettings.Dev) Randomizer.log("Lit lantern with " + (byGrenade ? "grenade " : "orb ")  + guid.ToString() + locStr());
    }

    public static void OnDestroyEntity(Entity entity, Damage damage) {
        try {
            if(!Active) return;
            if(entity.MoonGuid == StomplessRocks && Scenes.Manager.CurrentScene != null && Scenes.Manager.CurrentScene.Scene != "sorrowPassValleyD") {
                BoolGoals["FastStompless"].Completed = true;
            } else if(entity.MoonGuid == Drain) {
                BoolGoals["DrainSwamp"].Completed = true;
            }
            else if(entity.MoonGuid == CoreSkipRight || entity.MoonGuid == CoreSkipLeft && damage.Type == DamageType.LevelUp) {
                if(CoreSkipTimeout > 0) {
                    BoolGoals["CoreSkip"].Completed = true;
                } else {
                    CoreSkipTimeout = 5;
                }
            } else if(Amphibians.Contains(entity.name) && damage.Type == DamageType.Water) {
                BoolGoals["DrownFrog"].Completed = true;
            }
            if(Walls.Contains(entity.MoonGuid)) {
                IntGoals["BreakWalls"].Value++;
            }
            else if(Floors.Contains(entity.MoonGuid)) {
                IntGoals["BreakFloors"].Value++;
            }
            if(entity is Enemy && !entity.name.Contains("swarmEnemySmall") && !entity.name.Contains("swarmEnemyTiny")) {
                IntGoals["KillEnemies"].Value++;
            }
            if(SingleGuidSwitchListeners.ContainsKey(entity.MoonGuid))
                SingleGuidSwitchListeners[entity.MoonGuid].Handle();
//            if(RandomizerSettings.Dev) Randomizer.log("destroyed entity, name " + entity.name + ", guid " + entity.MoonGuid.ToString() + " with damage (" + damage.Type.ToString() + ", " + damage.Amount.ToString() + ")"  + locStr());
        } catch(Exception e) {
            Randomizer.LogError("OnDestroyEntity: " + e.Message);
        }
    }

    public static void OnDeath(Damage damage)
    {
        try {
            if(!Active) return;
            UpdateTimer = Math.Min(UpdateTimer, 3);
            // string log_out ="Killed by:" + damage.Sender.name + " ";
            string currentScene = scene();
            Entity test = damage.Sender.FindComponent<Entity>();
            // if(test != null)
            //     log_out += "(entity: " + test.MoonGuid + ")";


            GuidOwner owner = damage.Sender.FindComponent<GuidOwner>();
            // if(owner != null)
            // {
            //     log_out += "(owner: " + owner.MoonGuid + ")";
            // }
            switch(currentScene)
            {
                case "ginsoTreeWaterRisingBtm":
                case "ginsoTreeWaterRisingEnd":
                    if((damage.Type == DamageType.Explosion) || 
                       (owner != null && owner.MoonGuid == new MoonGuid(-1008478342, 1331842787, -1292489029, -195874113))) 
                        MultiBoolGoals["DieTo"]["Ginso Escape Fronkey"] = true;
                    break;
                case "thornfeltSwampStompAbility":
                    if(owner != null && owner.MoonGuid == new MoonGuid(864189451, 1278497087, -115370064, 1863139783)) 
                        MultiBoolGoals["DieTo"]["Stomp Rhino"] = true;
                    break;
                case "valleyOfTheWindTop":
                    if(damage.Type == DamageType.Spikes && damage.Amount > 1000) 
                        MultiBoolGoals["DieTo"]["Sunstone Lightning"] = true;
                    break;
                case "valleyOfTheWindWideMid":
                case "valleyOfTheWindWideLeft":
                case "valleyOfTheWindWideRight":
                    if(damage.Type == DamageType.Spikes) 
                        MultiBoolGoals["DieTo"]["NoobSpikes"] = true;
                    break;
                case "mangroveFallsDashEscalation":
                    if(damage.Type == DamageType.Crush) 
                        MultiBoolGoals["DieTo"]["Blackroot Teleporter Crushers"] = true;
                    break;
                case "southMangroveFallsGrenadeEscalationBR":
                    if(damage.Type == DamageType.Laser || damage.Type == DamageType.Lava) 
                        MultiBoolGoals["DieTo"]["Lost Grove Laser"] = true;
                    break;
                case "forlornRuinsGetIceB":
                    if(damage.Type == DamageType.Laser || damage.Type == DamageType.Lava) 
                        MultiBoolGoals["DieTo"]["Right Forlorn Laser"] = true;
                    break;
                case "horuFieldsB":
                    if(damage.Type == DamageType.Spikes && damage.Amount > 1000) 
                        MultiBoolGoals["DieTo"]["Horu Fields Acid"] = true;
                    break;
                case "mountHoruHubBottom":
                    if(damage.Type == DamageType.Spikes && damage.Amount > 1000) 
                        MultiBoolGoals["DieTo"]["Doorwarp Lava"] = true;
                    break;
                case "forlornRuinsEntrancePlaceholder":
                    if(damage.Type == DamageType.Spikes && damage.Amount > 1000) 
                        MultiBoolGoals["DieTo"]["Forlorn Void"] = true;
                    break;
                case "mistyWoodsLaserFlipPlatforms":
                    if(damage.Type == DamageType.Laser || damage.Type == DamageType.Lava)
                        MultiBoolGoals["DieTo"]["Misty Vertical Lasers"] = true;
                    break;
            }
            // log_out += " with damage (" + damage.Type.ToString() + ", " + damage.Amount.ToString() + ")" + locStr();
            // if(RandomizerSettings.Dev) Randomizer.log(log_out);
        } catch(Exception e) {
            Randomizer.LogError("OnDeath: " + e.Message);
        }
    }

    public static void OnScream() {
        if(!Active) return;
        BoolGoals["WilhelmScream"].Completed = true;
    }

    public static void OnKSDoor(MoonGuid doorGuid) {
        if(!Active) return;
        IntGoals["OpenKSDoors"].Value++;
//        if(RandomizerSettings.Dev) Randomizer.log("Opened door, guid " + doorGuid.ToString() + " " + locStr());
    }

    public static void OnEnergyDoor(MoonGuid doorGuid) {
        if(!Active) return;
        IntGoals["OpenEnergyDoors"].Value++;
//        if(RandomizerSettings.Dev) Randomizer.log("Opened door, guid " + doorGuid.ToString() + " " + locStr());
    }

    public static void OnLoc(int loc) {
        if(!Active || Randomizer.HaveCoord(loc))
            return;
        if(SingleLocListeners.ContainsKey(loc)) 
            foreach(SingleLocListener listener in SingleLocListeners[loc])
                listener.Handle();
        foreach(LocListener listener in LocListeners) 
            listener.Handle(loc);
    }
    public static void OnItem(RandomizerAction action, int coords) {
        try
        {
            if(!Active) return;
            if(coords == 2 && (action.Action == "HC" || action.Action == "EC" || action.Action == "AC"))
                return;
            string itemCode = action.Action + "|" + action.Value.ToString();
            if(action.Action == "RB")
            {
                SingleItemListeners["EV|0"].Set(Keys.GinsoTree);
                SingleItemListeners["EV|2"].Set(Keys.ForlornRuins);
                SingleItemListeners["EV|4"].Set(Keys.MountHoru);
            }
            if(SingleItemListeners.ContainsKey(itemCode))
                SingleItemListeners[itemCode].Handle();

            IntGoals["TotalPickups"].OnChange(2);
            string piz = "PickupsIn"+RandomizerStatsManager.CurrentZone(true);
            if(IntGoals.ContainsKey(piz))
                IntGoals[piz].OnChange(2);

            foreach(ItemListener listener in ItemListeners) 
                listener.Handle(itemCode);
        } catch(Exception e) {
            Randomizer.LogError("OnItem: " + e.Message);
        }
    }

    public static void OnExp(int exp){
        if(!Active) return;
        IntGoals["GainExperience"].Value += exp;
    }

    public static void OnTree(int treeNum){
        if(!Active) return;
        set(2566+treeNum, 1);
    }

    public static void OnActivateTeleporter(string identifier) {
        if(!Active || !MultiBoolGoals["ActivateTeleporter"].Subgoals.ContainsKey(identifier)) return;
        MultiBoolGoals["ActivateTeleporter"][identifier] = true;
    }

    public static void OnTouchMapstone() {
        try {
        if(!Active) return;
            MultiBoolGoals["TouchMapstone"][RandomizerStatsManager.CurrentZone()] = true;
        } catch(Exception e) {
            Randomizer.LogError("OnTouchMapstone: " + e.Message);
        }
    }

    public static bool TouchedMapstone(string zone) {
        try {
            return MultiBoolGoals["TouchMapstone"][zone];
        } catch(Exception e) {
            Randomizer.LogError("TouchedMapstone: " + e.Message);
            return false;
        }
    }

    public static void OnResetAP() {
        if(!Active) return;
        MultiBoolGoals["GetAbility"]["Spirit Potency"] = false;
        MultiBoolGoals["GetAbility"]["Ultra Defense"] = false;
        MultiBoolGoals["GetAbility"]["Ultra Stomp"] = false;
    }
    
    public static void OnGainAbility(AbilityType ability) {
        if(!Active) return;
        switch(ability) {
            case AbilityType.UltraStomp:
                MultiBoolGoals["GetAbility"]["Ultra Stomp"] = true;
                break;
            case AbilityType.UltraDefense:
                MultiBoolGoals["GetAbility"]["Ultra Defense"] = true;
                break;
            case AbilityType.SoulEfficiency:
                MultiBoolGoals["GetAbility"]["Spirit Potency"] = true;
                break;
            default:
                break;
        }
        IntGoals["SpendPoints"].OnChange(2);
    }


    public abstract class BingoGoal {
        public abstract string ToJson();
        public string Name;
        virtual public string GetName() { return this.Name; }
    }

    public interface LocListener { void Handle(int loc); }
    public interface ItemListener { void Handle(string itemCode); }
    public interface SceneListener { void Handle(string sceneName); }

    public interface SingleLocListener {
        string GetName();
        void Handle();
    }
    public interface SingleItemListener {
        string GetName();
        void Handle();
        void Set(bool newValue);
    }
    public interface SingleGuidSwitchListener {
        string GetName();
        void Handle();
    }
    public interface SingleSceneListener {
        string GetName();
        void Handle();
    }

    public class BoolGoal : BingoGoal {
        public int ItemId;
        public MultiBoolGoal Owner;
        public bool Completed {
            get { return get(this.ItemId) != 0; }
            set { 
                bool prior = this.Completed;
                set(this.ItemId, value ? 1 : 0);
                if(prior != value)
                    if(Owner == null)
                        GoalChanged(this.Name, 0);
                    else
                        MultiGoalChanged(this.Owner.Name, this.Name);

            }
        }
        public BoolGoal(string name, int id) {
            this.Name = name;
            this.ItemId = id;
        }
        public static void mk(string name, int id) {
            BoolGoal goal = new BoolGoal(name, id);
            BoolGoals[goal.Name] = goal;
        }
        public override string ToJson() {
            return "\"" + this.Name + "\": { \"value\": " + this.Completed.ToString().ToLower() + "}";
        }
    }

    public class BoolItemGoal : BoolGoal, SingleItemListener {
        public BoolItemGoal(string name, int id, string itemCode) : base(name, id) {
            if(SingleItemListeners.ContainsKey(itemCode)) 
                Randomizer.LogError(SingleItemListeners[itemCode].GetName() + " conflicts with " + this.Name + ". The latter has overwritten the former.");
            SingleItemListeners[itemCode] = this;
        }
        public void Handle() { this.Completed = true; }
        public void Set(bool newValue) { this.Completed = newValue; }
    }

    public class BoolGuidSwitchGoal : BoolGoal, SingleGuidSwitchListener {
        public BoolGuidSwitchGoal(string name, int id, MoonGuid switchId) : base(name, id) {
            if(SingleGuidSwitchListeners.ContainsKey(switchId)) 
                Randomizer.LogError(SingleGuidSwitchListeners[switchId].GetName() + " conflicts with " + this.Name + ". The latter has overwritten the former.");
            SingleGuidSwitchListeners[switchId] = this;
        }
        public void Handle() { this.Completed = true; }
    }

    public class SceneBoolGuidSwitchGoal : BoolGoal, SingleGuidSwitchListener {
        public SceneBoolGuidSwitchGoal(string name, int id, MoonGuid switchId, string sceneName) : base(name, id) {
            if(SingleGuidSwitchListeners.ContainsKey(switchId)) 
                Randomizer.LogError(SingleGuidSwitchListeners[switchId].GetName() + " conflicts with " + this.Name + ". The latter has overwritten the former.");
            SingleGuidSwitchListeners[switchId] = this;
            this.scene = sceneName;

        }
        public string scene;
        public void Handle() { if(scene() == this.scene) { this.Completed = true; } ; }
    }


    public class BoolLocGoal : BoolGoal, SingleLocListener {
        public BoolLocGoal(string name, int id, int loc) : base(name, id) {
            if(!SingleLocListeners.ContainsKey(loc)) 
                SingleLocListeners[loc] = new List<SingleLocListener>();    
            SingleLocListeners[loc].Add(this);
        }
        public void Handle() { this.Completed = true; }
    }
    public class BoolMultiSceneGoal : BoolGoal, SceneListener {
        public HashSet<string> Scenes;
        public BoolMultiSceneGoal(string name, int id,  HashSet<string> scenes) : base(name, id) {
            this.Scenes = scenes;
            SceneListeners.Add(this);
        }
        public static void mk(string name, int id, HashSet<string> scenes) {
            BoolMultiSceneGoal goal = new BoolMultiSceneGoal(name, id, scenes);
            BoolGoals[goal.Name] = goal;
        }
        public void Handle(string scene) { this.Completed = this.Completed || this.Scenes.Contains(scene); }
    }


    public class BoolSceneGoal : BoolGoal, SingleSceneListener {
        public BoolSceneGoal(string name, int id, string sceneName) : base(name, id) {
            if(SingleSceneListeners.ContainsKey(sceneName)) 
                Randomizer.LogError(SingleSceneListeners[sceneName].GetName() + " conflicts with " + this.Name + ". The latter has overwritten the former.");
            SingleSceneListeners[sceneName] = this;
        }
        public void Handle() { this.Completed = true; }
    }

    public class MultiBoolGoal : BingoGoal {
        public Dictionary<string, BoolGoal> Subgoals;
        public bool this[string key]
        {
            get
            {
                if(!this.Subgoals.ContainsKey(key)) {
                    Randomizer.LogError("Key " + key + " not found in MultiBoolGoal " + this.Name);
                    return false;
                }
                return this.Subgoals[key].Completed;
            }
            set { this.Subgoals[key].Completed = value; }
        }
        public MultiBoolGoal(string name, List<BoolGoal> subgoals) {
            this.Name = name;
            this.Subgoals = new Dictionary<string, BoolGoal>();
            foreach(BoolGoal subgoal in subgoals)
            {
                subgoal.Owner = this;
                this.Subgoals[subgoal.Name] = subgoal;
            }
        }
        public static void mk(string name, List<BoolGoal> subgoals) {
            MultiBoolGoal goal = new MultiBoolGoal(name, subgoals);
            MultiBoolGoals[goal.Name] = goal;
        }
        public override string ToJson() {
            string jsonStr = "\"" + this.Name + "\": { \"value\": {";
            int count = 0;
            foreach(BoolGoal subgoal in this.Subgoals.Values)
            {
                jsonStr += subgoal.ToJson() + ",";
                if(subgoal.Completed)
                    count++;
            }
            return jsonStr.TrimEnd(',') + "}, \"total\": " + count.ToString() + "}";
        }
    }

    public class IntGoal : BingoGoal {
        public int ItemId;
        public int Timeout = 1;
        public int Target = 0;
        public void OnChange(int delta) {
            int prior = this.Value - delta;
            if(prior < this.Target)
            {
                if(this.Value >= this.Target)
                    GoalChanged(this.Name, 0);
                else
                    GoalChanged(this.Name, this.Timeout);
            }
        }
        public int Value {
            get { return get(this.ItemId); }
            set { 
                    int delta = value - this.Value;
                    set(this.ItemId, value);
                    this.OnChange(delta);
                }
        }
        public IntGoal(string name, int id) {
            this.Name = name;
            this.ItemId = id;
        }
        public static void mk(string name, int id) {
            IntGoal goal = new IntGoal(name, id);
            IntGoals[goal.Name] = goal;
        }
        public static void mk(string name, int id, int timeout) {
            IntGoal goal = new IntGoal(name, id);
            IntGoals[goal.Name] = goal;
            goal.Timeout = timeout;
        }
        public override string ToJson() {
            return "\"" + this.Name + "\": { \"value\": " + this.Value.ToString() + "}";
        }
    }

    public class IntItemGoal : IntGoal, SingleItemListener {
        public IntItemGoal(string name, int id, string itemCode) : base(name, id) {
            if(SingleItemListeners.ContainsKey(itemCode)) {
                Randomizer.LogError(SingleItemListeners[itemCode].GetName() + " conflicts with " + this.Name + ". The latter has overwritten the former.");
            }
            SingleItemListeners[itemCode] = this;
        }
        public static void mk(string name, int id, string itemCode) {
            IntItemGoal goal = new IntItemGoal(name, id, itemCode);
            IntGoals[goal.Name] = goal;
        }
        public void Handle() { this.Value++; }
        public void Set(bool newValue) { this.Value += newValue ? 1 : -1 ; }
    }

    public class IntLocsGoal : IntGoal, LocListener {
        public HashSet<int> Locs;
        public IntLocsGoal(string name, int id, HashSet<int> locs) : base(name, id) {
            this.Locs = locs;
            LocListeners.Add(this);
        }
        public static void mk(string name, int id, HashSet<int> locs) {
            IntLocsGoal goal = new IntLocsGoal(name, id, locs);
            IntGoals[goal.Name] = goal; 
        }
        public void Handle(int loc) {
            if(this.Locs.Contains(loc))
                this.Value += 1;
        }
    }
    public static void PostCallback(object sender, UploadValuesCompletedEventArgs e)
    {
        if((e.Cancelled || e.Error != null) && (e.Error.GetType().Name == "WebException"))
            UpdateTimer = Math.Min(1, UpdateTimer);
    }

    public static void Init(string goalLine) {
        try
        {
            if(!Randomizer.SyncId.Contains("."))
            {
                Randomizer.LogError("Unable to initialize bingo: " + Randomizer.SyncId + " is not a valid SyncId");
                return;
            }
            string[] idParts = Randomizer.SyncId.Split('.');
            UpdateUrl = "http://orirandov3.appspot.com/netcode/game/" + idParts[0] + "/player/" + idParts[1] + "/bingo";
            if(!Active)
            {
                UpdateClient = new WebClient();
                UpdateClient.UploadValuesCompleted += PostCallback; 
                SingleLocListeners = new Dictionary<int, List<SingleLocListener>>();
                SingleItemListeners = new Dictionary<string, SingleItemListener>();
                SingleSceneListeners = new Dictionary<string, SingleSceneListener>();
                SingleGuidSwitchListeners = new Dictionary<MoonGuid, SingleGuidSwitchListener>();
                ItemListeners = new List<ItemListener>();
                LocListeners = new List<LocListener>();
                SceneListeners = new List<SceneListener>();
                BoolGoals = new Dictionary<string, BoolGoal>();
                BoolGoal.mk("FastStompless", 2500);
                BoolGoal.mk("CoreSkip", 2501);
                BoolGoal.mk("DrownFrog", 2502);
                BoolGoal.mk("DrainSwamp", 2503);
                BoolGoal.mk("WilhelmScream", 2504);
                IntGoals = new Dictionary<string, IntGoal>();
                IntLocsGoal.mk("MapstoneLocs", 2505, new HashSet<int> {-1840228, -4359680, -4440152, -5640092, 1480360, 2999904, 3439744, 5119584, 7959788});
                IntGoal.mk("OpenKSDoors", 2506);
                IntGoal.mk("BreakFloors", 2507);
                IntGoal.mk("BreakWalls", 2508);
                IntGoal.mk("UnspentKeystones", 2509);
                IntLocsGoal.mk("BreakPlants", 2510, new HashSet<int> {-11040068, -12320248, -1800088, -4680068, -4799416, -6080316, -6319752, -8160268, 1240020, 3119768, 3160244, 3279920, 3399820, 3639880, 399844, 4319860, 4359656, 4439632, 4919600, 5119900, 5359824, 5399780, 5400100, 6080608, 6279880});
                IntGoal.mk("TotalPickups", 1600, 1);            // already tracked by stats C:
                IntLocsGoal.mk("UnderwaterPickups", 2511, new HashSet<int>() {1839836, 3559792, -5160280, -3600088, 39756, 3959588, 4199724, 7679852, 5919864, 7959788, 3359784, -3200164, -400240, 559720, 7599824, 6839792, 7639816, 8719856, 5239456, 3519820});
                IntLocsGoal.mk("HealthCellLocs", 2512, new HashSet<int>() {-6119704, -6280316, -800192, 1479880, 1599920, 2599880, 3199820, 3919624, 3919688, 4239780, 5399808, 5799932});
                IntLocsGoal.mk("EnergyCellLocs", 2513, new HashSet<int>() {-1560188, -280256, -3200164, -3360288, -400240, -6279608, 1720000, 2480400, 2719900, 4199828, 5119556, 5360432, 5439640, 599844, 7199904});
                IntLocsGoal.mk("AbilityCellLocs", 2514, new HashSet<int>() {-10760004, -1680140, -2080116, -2160176, -2919980, -3520100, -3559936, -4160080, -4600188, -480168, -5119796, -6479528, -6719712, 1759964, 1799708, 2079568, 2519668, 2759624, 3319936, 3359784, 3519820, 3879576, 4079964, 4479568, 4479704, 4559492, 4999892, 5239456, 639888, 6399872, 6999916, 799804, 919908 } );
                IntGoal.mk("LightLanterns", 2515); 
                IntGoal.mk("SpendPoints", 80, 1);
                IntGoal.mk("GainExperience", 2516, 3);
                IntGoal.mk("KillEnemies", 2518, 3);
                IntGoal.mk("OpenEnergyDoors", 2519);
                IntGoal.mk("ActivateMaps", 23);
                IntItemGoal.mk("HealthCells", 2605, "HC|1");
                IntItemGoal.mk("EnergyCells", 2606, "EC|1");
                IntItemGoal.mk("AbilityCells", 2607,"AC|1");
                IntItemGoal.mk("CollectMapstones", 2608,"MS|1");

                IntGoal.mk("PickupsInGlades", 1601, 1);
                IntGoal.mk("PickupsInGrove", 1602, 1);
                IntGoal.mk("PickupsInGrotto", 1603, 1);
                IntGoal.mk("PickupsInBlackroot", 1604, 1);
                IntGoal.mk("PickupsInSwamp", 1605, 1);
                IntGoal.mk("PickupsInGinso", 1606, 1);
                IntGoal.mk("PickupsInValley", 1607, 1);
                IntGoal.mk("PickupsInMisty", 1608, 1);
                IntGoal.mk("PickupsInForlorn", 1609, 1);
                IntGoal.mk("PickupsInSorrow", 1610, 1);
                IntGoal.mk("PickupsInHoru", 1611, 1);


                MultiBoolGoals = new Dictionary<string, MultiBoolGoal>();
                MultiBoolGoal.mk("CompleteHoruRoom", new List<BoolGoal>() {
                    new BoolLocGoal("L1", 2522, -919624),
                    new BoolLocGoal("L2", 2523, -199724),
                    new BoolLocGoal("L3", 2524, -1639664),
                    new BoolLocGoal("L4", 2525, -959848),
                    new BoolLocGoal("R1", 2526, 2640380),
                    new BoolLocGoal("R2", 2527, 1720288),
                    new BoolLocGoal("R3", 2528, 3040304),
                    new BoolLocGoal("R4", 2529, 2160192)
                });

                MultiBoolGoal.mk("VanillaEventLocs", new List<BoolGoal>() {
                    new BoolLocGoal("Water Vein", 2609, 4999752),
                    new BoolLocGoal("Gumon Seal", 2610, -7200024),
                    new BoolLocGoal("Sunstone", 2611, -5599400),
                    new BoolLocGoal("Clean Water", 2612, 5480952),
                    new BoolLocGoal("Wind Restored", 2613, -7320236),
                    new BoolLocGoal("Warmth Returned", 2614, -2399488)
                });

                MultiBoolGoal.mk("DieTo", new List<BoolGoal>() {
                    new BoolGoal("Sunstone Lightning", 1598),
                    new BoolGoal("Lost Grove Laser", 1597),
                    new BoolGoal("Forlorn Void", 1596),
                    new BoolGoal("Stomp Rhino", 1595),
                    new BoolGoal("Horu Fields Acid", 1594),
                    new BoolGoal("Doorwarp Lava", 1593),
                    new BoolGoal("Ginso Escape Fronkey", 1592),
                    new BoolGoal("Blackroot Teleporter Crushers", 1591),
                    new BoolGoal("NoobSpikes", 1590), // 1589 and 1587 are being used by bonus skill
                    new BoolGoal("Right Forlorn Laser", 1588),
                    new BoolGoal("Misty Vertical Lasers", 1586)  

                    // 1584 and below are taken

                });

                MultiBoolGoal.mk("CompleteEscape", new List<BoolGoal>() {
                    new BoolLocGoal("Ginso Tree", 2530, 5480952),
                    new BoolSceneGoal("Forlorn Ruins", 2531, "forlornRuinsNestC"),
                    new BoolGoal("Mount Horu", 1599)
                });

                MultiBoolGoal.mk("ActivateTeleporter", new List<BoolGoal>() {
                    new BoolGoal("swamp", 2532),
                    new BoolGoal("sorrowPass", 2520),
                    new BoolGoal("sunkenGlades", 2533),
                    new BoolGoal("moonGrotto", 2534),
                    new BoolGoal("mangroveFalls", 2535),
                    new BoolGoal("valleyOfTheWind", 2536),
                    new BoolGoal("spiritTree", 2537),
                    new BoolGoal("mangroveB", 2538),
                    new BoolGoal("horuFields", 2539),
                    new BoolGoal("ginsoTree", 2540),
                    new BoolGoal("forlorn", 2541),
                    new BoolGoal("mountHoru", 2542)
                });

                MultiBoolGoal.mk("EnterArea", new List<BoolGoal>() {
                    new BoolMultiSceneGoal("Lost Grove", 2543, new HashSet<string>() { "southMangroveFallsStoryRoomA", "southMangroveFallsGrenadeEscalationB", "southMangroveFallsGrenadeEscalationBR"}),
                    new BoolMultiSceneGoal("Misty Woods", 2544,  new HashSet<string>() { "sorrowPassForestB", "mistyWoodsGetTorch", "mistyWoodsIntro" }),
                    new BoolMultiSceneGoal("Forlorn Ruins", 2545, new HashSet<string>() {"forlornRuinsGravityRoomA", "forlornRuinsGetNightberry", "forlornRuinsGetIceB"}),
                    new BoolMultiSceneGoal("Sorrow Pass", 2546, new HashSet<string>() {"valleyOfTheWindEArt", "valleyOfTheWindLaserShaft", "valleyOfTheWindGauntlet", "valleyOfTheWindTop", "valleyOfTheWindHubR"}),
                    new BoolMultiSceneGoal("Mount Horu", 2547, new HashSet<string>() {"mountHoruHubBottom", "mountHoruHubMid"}),
                    new BoolMultiSceneGoal("Ginso Tree", 2517,new HashSet<string>() { "ginsoTreeSaveRoom", "ginsoEntranceIntro", "ginsoTreeWaterRisingEnd"})
                });

                MultiBoolGoal.mk("GetEvent", new List<BoolGoal>() {
                    new BoolItemGoal("Water Vein", 2548, "EV|0"),
                    new BoolItemGoal("Gumon Seal", 2549, "EV|2"),
                    new BoolItemGoal("Sunstone", 2550, "EV|4"),
                    new BoolItemGoal("Clean Water", 2551, "EV|1"),
                    new BoolItemGoal("Wind Restored", 2552, "EV|3"),
                    new BoolItemGoal("Warmth Returned", 2553, "EV|5")
                });

                MultiBoolGoal.mk("GetItemAtLoc", new List<BoolGoal>() {
                    new BoolLocGoal("LostGroveLongSwim", 2554, 5239456),
                    new BoolLocGoal("ValleyEntryGrenadeLongSwim", 2555, -3200164),
                    new BoolLocGoal("SpiderSacEnergyDoor", 2556, 639888),
                    new BoolLocGoal("SorrowHealthCell", 2557, -6119704),
                    new BoolLocGoal("SunstonePlant", 2558, -4799416),
                    new BoolLocGoal("GladesLaser", 2559, -1560188),
                    new BoolLocGoal("LowerBlackrootLaserAbilityCell", 2560, 3879576),
                    new BoolLocGoal("MistyGrenade", 2561, -6720040),
                    new BoolLocGoal("LeftSorrowGrenade", 2562, -6799732),
                    new BoolLocGoal("DoorWarpExp", 2563, 1040112),
                    new BoolLocGoal("HoruR3Plant", 2564, 3160244),
                    new BoolLocGoal("RightForlornHealthCell", 2565, -6280316),
                    new BoolLocGoal("ForlornEscapePlant", 2566, -12320248)
                });
                MultiBoolGoal.mk("VisitTree", new List<BoolGoal>() {
                    new BoolGoal("Wall Jump", 2567), 
                    new BoolGoal("Charge Flame", 2568), 
                    new BoolGoal("Double Jump", 2569), 
                    new BoolGoal("Bash", 2570), 
                    new BoolGoal("Stomp", 2571), 
                    new BoolGoal("Glide", 2572), 
                    new BoolGoal("Climb", 2573), 
                    new BoolGoal("Charge Jump", 2574), 
                    new BoolGoal("Grenade", 2575), 
                    new BoolGoal("Dash", 2576)
                });
                MultiBoolGoal.mk("GetAbility", new List<BoolGoal>() {
                    new BoolGoal("Ultra Defense", 2577), 
                    new BoolGoal("Spirit Light Efficiency", 2578), 
                    new BoolGoal("Ultra Stomp", 2579)
                });
                MultiBoolGoal.mk("StompPeg", new List<BoolGoal>() {
                    new BoolGuidSwitchGoal("BlackrootTeleporter", 2580, new MoonGuid(-896629726, 1267685881, 1301835908, 1482947216)), 
                    new BoolGuidSwitchGoal("SwampPostStomp", 2581, new MoonGuid(-1973919964, 1235174309, 1801441926, 1977910307)), 
                    new BoolGuidSwitchGoal("GroveMapstoneTree", 2582, new MoonGuid(-1664353560, 1216217354, 845171129, -1310424046)), 
                    new BoolGuidSwitchGoal("HoruFieldsTPAccess", 2583, new MoonGuid(938332473, 1306647788, 243261569, 1200294177)), 
                    new BoolGuidSwitchGoal("SorrowLasersArea", 2620, new MoonGuid(-344918519, 1287316567, 75338928, 233490553)),
                    new BoolGuidSwitchGoal("L1", 2584, new MoonGuid(-931451667, 1186606623, -1576090735, 604062528)),
                    new BoolGuidSwitchGoal("R2", 2585, new MoonGuid(-1449971991, 1203470121, 209341883, 254513811)), 
                    new BoolGuidSwitchGoal("L2", 2586, new MoonGuid(1123382356, 1244294063, 1435789238, 1593458155)), 
                    new BoolGuidSwitchGoal("L4Fire", 2589, new MoonGuid(-338506493, 1267621739, -966392693, -623848418)), 
                    new BoolGuidSwitchGoal("L4Drain", 2590, new MoonGuid(2098905692, 1318113199, 1820486584, 962123723)), 
                    new BoolGuidSwitchGoal("SpiderLake", 2591, new MoonGuid(-859228674, 1320898488, 1858384318, 1959278247)), 
                    new BoolGuidSwitchGoal("GroveGrottoUpper", 2592, new MoonGuid(-550813708, 1106430997, -1135517261, -531706068)), 
                    new BoolGuidSwitchGoal("GroveGrottoLower", 2593, new MoonGuid(1980402418, 1183311360, -882091623, 275381859)),
                    new BoolGuidSwitchGoal("ForlornLaserPeg", 2625, new MoonGuid(970409280, 1324809336, 1682715272, 1648746300))
                });
                MultiBoolGoal.mk("HuntEnemies", new List<BoolGoal>() {
                    new BoolGuidSwitchGoal("Misty Miniboss", 2596, new MoonGuid(-1042451585, 1166751436, 1922297510, -83736415)), 
                    new BoolGuidSwitchGoal("Frog Toss", 2597, new MoonGuid(-2143519163, 1146437181, -51560278, -1978077749)),
                    new BoolGuidSwitchGoal("Lost Grove Fight Room", 2598, new MoonGuid(-1679036972, 1237382256, -182501967, -2059998279)),
                    new BoolGuidSwitchGoal("R2", 2599, new MoonGuid(-1624679962, 1208388157, 520226958, -1390952276)),
                    new BoolGuidSwitchGoal("Grotto Miniboss", 2600, new MoonGuid(-2054701236, 1079020693, -51310956, 1825594796)),
                    new BoolGuidSwitchGoal("Lower Ginso Miniboss", 2601, new MoonGuid(74624213, 1320731591, 1926247103, 701829352)),
                    new BoolGuidSwitchGoal("Upper Ginso Miniboss", 2602, new MoonGuid(-627974393, 1255028302, -367677274, 668375081)),
                    new BoolGuidSwitchGoal("Swamp Rhino Miniboss", 2603, new MoonGuid(320654260, 1306461320, -1091082354, -1855445076)),
                    new BoolGuidSwitchGoal("Mount Horu Miniboss", 2604,  new MoonGuid(-1829316912, 1244306941, 1626759309, -571989581))
                });

                MultiBoolGoal.mk("TouchMapstone", new List<BoolGoal>() {
                        new BoolGoal("sunkenGlades", 2615),
                        new BoolGoal("hollowGrove", 2616),
                        new BoolGoal("moonGrotto", 2617),
                        new BoolGoal("mangrove", 2618),
                        new BoolGoal("thornfeltSwamp", 2619),
                        new BoolGoal("valleyOfTheWind", 2621),
                        new BoolGoal("forlornRuins", 2622),
                        new BoolGoal("sorrowPass", 2623),
                        new BoolGoal("mountHoru", 2624)
                });


                Active = true;
            }
            SetActiveGoals(goalLine.Substring(6));
        }
        catch(Exception e) {
            Randomizer.LogError("BingoController.Init: " + e.Message + " " + e.StackTrace);
        }
    }

    public static void SetActiveGoals(string goals) {
        try{
            var lines = goals.Split('/');
            ActiveSingleGoals = new HashSet<String>();
            ActiveMultiGoals = new Dictionary<String, HashSet<String>>();
            foreach(string singleGoal in lines[0].Split(','))
            {
                if(singleGoal.Contains('-')) {
                    var goalParts = singleGoal.Split('-');
                    ActiveSingleGoals.Add(goalParts[0]);
                    int count = int.Parse(goalParts[1]);
                    IntGoals[goalParts[0]].Target = count;
                }
                else
                    ActiveSingleGoals.Add(singleGoal);
            }
            foreach(string multiGoal in lines.Skip(1)) {
                try{
                    var parts = multiGoal.Split(':');
                    ActiveMultiGoals[parts[0]] = new HashSet<String>(parts[1].Split(','));
                } catch(Exception e) {
                    Randomizer.LogError("SAG." + multiGoal + ": " + e.Message);
                }
            }            
        } catch(Exception e) {
            Randomizer.LogError("SAG: " + e.Message);
        }

    }

    public static void GoalChanged(string goalName, int timeout) {
        if(ActiveSingleGoals.Contains(goalName))
        {
            UpdateTimer = Math.Min(timeout, UpdateTimer);
        }
    }


    public static void MultiGoalChanged(string goalName, string subgoalName) {

    if(ActiveMultiGoals.ContainsKey(goalName) && (ActiveMultiGoals[goalName].Contains(subgoalName) || ActiveMultiGoals[goalName].Contains("COUNT")))
            UpdateTimer = 0;
    }

    public static HashSet<String> ActiveSingleGoals;
    public static Dictionary<String, HashSet<String>> ActiveMultiGoals;
    public static HashSet<MoonGuid> BlackrootLanterns = new HashSet<MoonGuid>() {
                new MoonGuid(-247741005, 1196428260, -687048288, -31634124),
                new MoonGuid(1907989719, 1277885764, -201315168, 756894943),
                new MoonGuid(1145583265, 1113096007, 1499060158, 1321600423),
                new MoonGuid(2036180722, 1271722027, -1468527710, -1171618564),
                new MoonGuid(-1230368003, 1203943358, 1445926043, 1361606719),
                new MoonGuid(-1776579092, 1105227369, -108936522, 1268437567),
                new MoonGuid(939157475, 1204164414, 1274659233, 466487750),
                new MoonGuid(113579066, 1094186079, 393414551, 435335703)
            };
    public static HashSet<MoonGuid> Walls = new HashSet<MoonGuid>() {
                new MoonGuid(996714861, 1239808899, 1900786868, -1496533060),
                new MoonGuid(-282304521, 1106903372, 1209236670, 205465054),
                new MoonGuid(2014579407, 1164325780, 1399366826, -192348871),
                new MoonGuid(815745988, 1118372593, -247997034, -1317346796),
                new MoonGuid(-74808989, 1319810112, 95298987, -521388410),
                new MoonGuid(1216248947, 1275981194, 934545855, -110433433),
                new MoonGuid(407948821, 1174798978, -1954545729, -218047736),
                new MoonGuid(-373271364, 1124911338, -74387529, 356637800),
                new MoonGuid(-1747543229, 1285318697, -82818144, -1426638781),
                new MoonGuid(973630304, 1213945254, 1403756978, 179678160),
                new MoonGuid(-862990717, 1273110166, -831455066, 1122535008),
                new MoonGuid(-1075583388, 1205168908, -911657594, -991414846),
                new MoonGuid(1745611776, 1339341637, 1514650023, -361154042),
                new MoonGuid(-843996807, 1182290364, 240613310, 908023576),
                new MoonGuid(909095086, 1188207515, -535261054, -455502955),
                new MoonGuid(2035568949, 1292912205, 1880333756, -517447972),
                new MoonGuid(-1954305623, 1248443809, -687266910, -516773669),
                new MoonGuid(1968002262, 1143732535, -1986575699, 25897699),
                new MoonGuid(1712452026, 1115945981, 564064446, 430399509),
                new MoonGuid(144989734, 1123438917, -505982036, 146126186),
                new MoonGuid(1444079458, 1244809381, 118366602, 562094288),
                new MoonGuid(-398413180, 1111956010, 1890083992, 732274829),
                new MoonGuid(768679515, 1121299506, -1248609130, -1421449463),
                new MoonGuid(242695656, 1294785020, -2095004543, 1012572914),
                new MoonGuid(943738338, 1146463710, 802828453, -123999703),
                new MoonGuid(-138274205, 1238088176, 420129701, 1302900470),
                new MoonGuid(-713103345, 1136979644, -554798671, 88957067),
                new MoonGuid(-428733311, 1310679551, -241037431, -1345976781),
                new MoonGuid(-1512077958, 1188663915, 1905064588, 2019919965),
                new MoonGuid(-773868360, 1108658051, 469763253, 1729895317)
            };
    public static HashSet<MoonGuid> Floors = new HashSet<MoonGuid>() {
                new MoonGuid(-920679693, 1232503605, 72320169, -1907458604),
                new MoonGuid(-1709608458, 1158899166, -1771762550, -59165922),
                new MoonGuid(794839184, 1159253274, -554590529, 1980315570),
                new MoonGuid(-1922533474, 1182231239, 1216241579, 615847897),
                new MoonGuid(114393758, 1108032672, 277900701, 467544015),
                new MoonGuid(-611604502, 1153438031, 1199875203, -481652861),
                new MoonGuid(-906811856, 1093725306, 458941853, 380268441),
                new MoonGuid(-788607148, 1315643098, 1762814087, -1505686428),
                new MoonGuid(15659313, 1287801037, 1545598344, -522479087),
                new MoonGuid(1238793573, 1176622299, -879247739, 781883528),
                new MoonGuid(-1355981316, 1116822596, -361698652, 616722726),
                new MoonGuid(-683912057, 1176764413, -1759720560, 1848816384),
                new MoonGuid(-259817809, 1231640693, -1344545386, -1072514037),
                new MoonGuid(-1780990681, 1145891208, -743069018, -1320426726),
                new MoonGuid(-275011930, 1300881743, -1127026030, -511299636),
                new MoonGuid(1481390194, 1183494433, 332443009, 1459600434),
                new MoonGuid(-131652703, 1267136605, -14188927, 659055181),
                new MoonGuid(-1157962264, 1162523472, 1003923615, 304488755),
                new MoonGuid(-719065131, 1148724296, 129746866, 1316646464),
                new MoonGuid(865712815, 1295772191, -90299502, -282588832),
                new MoonGuid(174637128, 1075297796, -267605321, 1328562411),
                new MoonGuid(-814088378, 1327252339, -1856829564, 202043573),
                new MoonGuid(1589083967, 1292766321, -595680093, 1864601538),
                new MoonGuid(-1171070044, 1187451151, -924354384, -1115994997),
                new MoonGuid(-435529362, 1209040538, -834301303, 1939964072),
                new MoonGuid(1106028832, 1185849774, 1306986684, -35732515),
                new MoonGuid(-1160214076, 1339344548, -1852593771, -760957908),
                new MoonGuid(1711549718, 1225123502, -2036372807, 248162391),
                new MoonGuid(1878899019, 1234476004, 2059718046, 2041905613),
                new MoonGuid(-24413245, 1120289301, -1067001194, -1871977343),
                new MoonGuid(-1527811234, 1216946668, 1893311635, -87979110),
                new MoonGuid(630775061, 1228671812, 1086367895, 850198016),
                new MoonGuid(-194004294, 1318750600, -1713114953, -1498080888),
                new MoonGuid(372960494, 1126739753, 1014154926, -996270949),
                new MoonGuid(-1556519952, 1243140001, -139996753, -675942968),
                new MoonGuid(-1145782769, 1085404665, -598239091, -761133242),
                new MoonGuid(802418842, 1095217114, -1547353417, -914820306),
                new MoonGuid(1158529902, 1188190493, 1181593535, 701845248),
                new MoonGuid(1725935194, 1266022690, -1577623361, 1242392208),
                new MoonGuid(998418087, 1216993156, -93010042, 915725725),
                new MoonGuid(1872689565, 1159149407, 1137880232, -1645879840),
                new MoonGuid(-1868936768, 1317137295, 1171267237, 1986710352),
                new MoonGuid(-1154496313, 1246546881, 1570956217, 1617876293),
                new MoonGuid(704514374, 1223475466, 872969108, 1830638633),
                new MoonGuid(-379111785, 1117012912, 1234316431, 1476933075)
            };

    public static string GetJson() {
        string jsonStr = "{\n";
        List<string> jsonFrags = new List<string>();
        foreach(BoolGoal goal in BoolGoals.Values) {
            jsonFrags.Add(goal.ToJson());
        }
        foreach(IntGoal goal in IntGoals.Values) {
            jsonFrags.Add(goal.ToJson());
        }
        foreach(MultiBoolGoal goal in MultiBoolGoals.Values) {
            jsonFrags.Add(goal.ToJson());
        }
        jsonStr += String.Join(",\n", jsonFrags.ToArray()) + "\n}";
        return jsonStr;

    }
    public static void PostUpdate() {
        NameValueCollection values = new NameValueCollection();
        values["bingoData"] = GetJson();
        values["version"] = Randomizer.VERSION;
        if(!UpdateClient.IsBusy)
        {
            UpdateClient.UploadValuesAsync(new Uri(UpdateUrl), values);
            UpdateTimer = 15;
        }
    }

    public static MoonGuid StomplessRocks = new MoonGuid(-1118019250, 1080908127, 1929144468, -1515713832);
    public static MoonGuid Drain = new MoonGuid(1711549718, 1225123502, -2036372807, 248162391);
    public static MoonGuid CoreSkipRight = new MoonGuid(1165644159, 1142717490, -237578866, -2119320164);
    public static MoonGuid CoreSkipLeft = new MoonGuid(1709969197, 1275364087, -792362568, -1385507206);
    public static HashSet<string> Amphibians = new HashSet<string>() { "jumperEnemy", "spitterEnemy", "fastSpitterEnemy" };
    public static string CurrentScene;

    private static int get(int item) { return Characters.Sein.Inventory.GetRandomizerItem(item); }
    private static int set(int item, int value) { return Characters.Sein.Inventory.SetRandomizerItem(item, value); }
    private static int inc(int item, int value) { return Characters.Sein.Inventory.IncRandomizerItem(item, value); }

    public static int UpdateTimer = 15;
    public static WebClient UpdateClient;
    public static string UpdateUrl;
    public static bool Active;
    public static int CoreSkipTimeout = 0;
    public static bool InCutscene = false;
    public static int LockCount = 0;
    public static int NetFailCount = 5;
    public static Dictionary<string, BoolGoal> BoolGoals;
    public static Dictionary<string, IntGoal> IntGoals;
    public static Dictionary<string, MultiBoolGoal> MultiBoolGoals;
    public static Dictionary<int, List<SingleLocListener>> SingleLocListeners;
    public static Dictionary<string, SingleItemListener> SingleItemListeners;
    public static Dictionary<MoonGuid, SingleGuidSwitchListener> SingleGuidSwitchListeners;
    public static Dictionary<string, SingleSceneListener> SingleSceneListeners;
    public static List<ItemListener> ItemListeners;
    public static List<LocListener> LocListeners;
    public static List<SceneListener> SceneListeners;
}