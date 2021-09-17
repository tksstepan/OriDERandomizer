using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core;
using Game;
using Sein.World;
using UnityEngine;

// Token: 0x020009F5 RID: 2549
public static class Randomizer
{
    public static string VERSION = "4.BETA";
    public static void initialize()
    {
        try {
            Randomizer.OHKO = false;
            Randomizer.ZeroXP = false;
            Randomizer.BonusActive = true;
            Randomizer.GiveAbility = false;
            Randomizer.Chaos = false;
            Randomizer.ChaosVerbose = false;
            Randomizer.Returning = false;
            Randomizer.Sync = false;
            Randomizer.SyncId = "";
            Randomizer.ForceMaps = false;
            Randomizer.SyncMode = 4;
            Randomizer.StringKeyPickupTypes = new List<string> {"TP", "SH", "NO", "WT", "MU", "HN", "WP", "RP", "WS"};
            RandomizerChaosManager.initialize();
            Randomizer.DamageModifier = 1f;
            Randomizer.Table = new Dictionary<int, RandomizerAction>();
            Randomizer.GridFactor = 4.0;
            Randomizer.Message = "Good luck on your rando!";
            Randomizer.MessageProvider = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
            Randomizer.ProgressiveMapStones = true;
            Randomizer.ForceTrees = false;
            Randomizer.CluesMode = false;
            Randomizer.Shards = false;
            Randomizer.WorldTour = false;
            Randomizer.SeedMeta = "";
            Randomizer.MistySim = new WorldEvents();
            Randomizer.MistySim.MoonGuid = new MoonGuid(1061758509, 1206015992, 824243626, -2026069462);
            Randomizer.TeleportTable = new Hashtable();
            Randomizer.TeleportTable["Forlorn"] = "forlorn";
            Randomizer.TeleportTable["Grotto"] = "moonGrotto";
            Randomizer.TeleportTable["Sorrow"] = "valleyOfTheWind";
            Randomizer.TeleportTable["Grove"] = "spiritTree";
            Randomizer.TeleportTable["Swamp"] = "swamp";
            Randomizer.TeleportTable["Valley"] = "sorrowPass";
            Randomizer.TeleportTable["Ginso"] = "ginsoTree";
            Randomizer.TeleportTable["Horu"] = "mountHoru";
            Randomizer.TeleportTable["Glades"] = "sunkenGlades";
            Randomizer.TeleportTable["Blackroot"] = "mangroveFalls";
            Randomizer.Entrance = false;
            Randomizer.DoorTable = new Hashtable();
            Randomizer.ColorShift = false;
            Randomizer.MessageQueue = new Queue();
            Randomizer.MessageQueueTime = 0;
            Randomizer.QueueBash = false;
            Randomizer.BashWasQueued = false;
            Randomizer.BashTap = false;
            Randomizer.GrenadeJumpQueued = false;
            Randomizer.fragsEnabled = false;
            Randomizer.LastTick = 10000000L;
            Randomizer.LockedCount = 0;
            Randomizer.ResetTrackerCount = 0;
            Randomizer.HotCold = false;
            Randomizer.HotColdTypes = new HashSet<string>() {"EV", "RB17", "RB19", "RB21", "RB28", "SK", "TPForlorn", "TPHoru", "TPGinso", "TPValley", "TPSorrow"};
            Randomizer.HotColdItems = new Dictionary<int, RandomizerHotColdItem>();
            Randomizer.HotColdFrags = new Dictionary<int, RandomizerHotColdItem>();
            Randomizer.HotColdMaps = new List<int>();
            Randomizer.HotColdMapsWithFrags = new List<int>();
            int HotColdSaveId = 2000;
            Randomizer.OpenMode = true;
            Randomizer.OpenWorld = false;
            RandomizerDataMaps.LoadGladesData();
            RandomizerDataMaps.LoadGinsoData();
            RandomizerDataMaps.LoadForlornData();
            RandomizerDataMaps.LoadHoruData();
            RandomizerDataMaps.LoadValleyData();
            RandomizerColorManager.Initialize();
            RandomizerPlantManager.Initialize();
            RandomizerRebinding.ParseRebinding();
            RandomizerSettings.ParseSettings();
            Randomizer.RelicZoneLookup = new Dictionary<string, string>();
            RandomizerTrackedDataManager.Initialize();
            RandomizerStatsManager.Initialize();
            Randomizer.RelicCount = 0;
            Randomizer.GrenadeZone = "MIA";
            Randomizer.StompZone = "MIA";
            Randomizer.Repeatables = new HashSet<int>();
            Randomizer.StompTriggers = false;
            Randomizer.GoalModeFinish = false;
            Randomizer.SpawnWith = "";
            Randomizer.IgnoreEnemyExp = false;
            Randomizer.RelicCountOverride = false;
            Randomizer.AllowOrbWarps = false;
            Randomizer.RandomizedFirstEnergy = false;
            try {
                if(File.Exists("randomizer.dat")) {
                    List<String> allLines = File.ReadAllLines("randomizer.dat").ToList();
                    string[] flagLine = allLines[0].Split('|');
                    string s = flagLine[1];
                    string[] flags = flagLine[0].Split(',');
                    Randomizer.SeedMeta = allLines[0];
                    bool doBingo = Randomizer.ParseFlags(s, flags);
                    if(doBingo) {
                        Randomizer.Message = "Good luck on your bingo!";
                        BingoController.Init(allLines[allLines.Count-1]);
                        allLines.RemoveAt(allLines.Count-1);
                    }
                    else
                        BingoController.Active = false;

                    if(Randomizer.CluesMode) {
                        RandomizerClues.initialize();
                    }
                    foreach (string line in allLines.Skip(1))
                    {
                        string[] lineParts = line.Split('|');
                        int coords;
                        int.TryParse(lineParts[0], out coords);
                        if (coords == 2) {
                            SpawnWith = lineParts[1] + lineParts[2];
                            continue;
                        }
                        if (Randomizer.HotColdTypes.Contains(lineParts[1]) || Randomizer.HotColdTypes.Any((string t) => (lineParts[1] + lineParts[2]).StartsWith(t))) {
                            if (Math.Abs(coords) > 100) {
                                Randomizer.HotColdItems.Add(coords, new RandomizerHotColdItem(Randomizer.HashKeyToVector(coords), HotColdSaveId));
                                HotColdSaveId++;
                            } else {
                                Randomizer.HotColdMaps.Add(coords);
                                Randomizer.HotColdMapsWithFrags.Add(coords);
                            }
                        } else if(lineParts[1] == "MS") {
                            if (Math.Abs(coords) > 100) {
                                Randomizer.HotColdFrags.Add(coords, new RandomizerHotColdItem(Randomizer.HashKeyToVector(coords), HotColdSaveId));
                                HotColdSaveId++; 
                            } else {
                                Randomizer.HotColdMapsWithFrags.Add(coords);
                            }
                        }
                        if (Randomizer.StringKeyPickupTypes.Contains(lineParts[1]))
                        {
                            Randomizer.Table[coords] = new RandomizerAction(lineParts[1], lineParts[2]);
                            if(lineParts[1] == "WT") {
                                Randomizer.RelicZoneLookup[lineParts[2]] = lineParts[3];
                                if(!Randomizer.RelicCountOverride) {
                                    Randomizer.RelicCount++;
                                }
                            }
                            if(CluesMode && (lineParts[1] == "RP" || lineParts[1] == "MU")) {
                                if(lineParts[2].Contains("EV/0"))
                                    RandomizerClues.AddClue(lineParts[3], 0);
                                else if(lineParts[2].Contains("EV/2"))
                                    RandomizerClues.AddClue(lineParts[3], 1);
                                else if(lineParts[2].Contains("EV/4"))
                                    RandomizerClues.AddClue(lineParts[3], 2);
                            }
                            if(lineParts[1] == "RP") {
                                Repeatables.Add(coords);
                            }
                        }
                        else
                        {
                            int id;
                            int.TryParse(lineParts[2], out id);
                            if (lineParts[1] == "EN")
                            {
                                // door entries are coord|EN|targetX|targetY
                                int doorY;
                                int.TryParse(lineParts[3], out doorY);
                                Randomizer.DoorTable[coords] = new Vector3((float)id, (float)doorY);
                            }
                            else 
                            {
                                Randomizer.Table[coords] = new RandomizerAction(lineParts[1], id);
                                if (lineParts[1] == "SK") {
                                    if(id == 51) {
                                        GrenadeZone = lineParts[3];
                                    } else if(id == 4) {
                                        StompZone = lineParts[3];
                                    }
                                }
                                if (CluesMode && lineParts[1] == "EV" && id % 2 == 0)
                                    RandomizerClues.AddClue(lineParts[3], id / 2);
                            }
                        }
                    }
                    Randomizer.HotColdMaps.Sort();
                    Randomizer.HotColdMapsWithFrags.Sort();
                    if (Randomizer.CluesMode) {
                        RandomizerClues.FinishClues();
                    }
                } else {
                    Randomizer.printInfo("Error: randomizer.dat not found");
                }
            }
            catch(Exception e) {
                Randomizer.printInfo("Error parsing randomizer.dat:" + e.Message, 300);
        }
        RandomizerBonusSkill.Reset();
    
        } catch(Exception e) {
            Randomizer.log("init: " + e.Message);
        }
    }

    public static void InitializeOnce()
    {
        RandomizerLocationManager.Initialize();
        RandomizerUI.Initialize();
        RandomizerBootstrap.Initialize();
    }

    public static void getPickup()
    {
        Randomizer.getPickup(Characters.Sein.Position);
    }

    public static void WarpTo(Vector3 position, int warpDelay) {
        Randomizer.Warping = warpDelay;
        Randomizer.WarpTarget = position;
        if (!Characters.Sein.Controller.CanMove || !Characters.Sein.Active || Characters.Sein.IsSuspended)
        {
            DelayedWarp = true;
            return;
        }
        if(Characters.Sein.Abilities.Carry.IsCarrying && !AllowOrbWarps)
            Characters.Sein.Abilities.Carry.CurrentCarryable.Drop();
        if(Characters.Sein.Abilities.Dash && Characters.Sein.Abilities.Dash.IsDashingOrChangeDashing)
            Characters.Sein.Abilities.Dash.StopDashing();
        Characters.Sein.Position = position;
        Characters.Sein.Speed = new Vector3(0f, 0f);
        Characters.Ori.Position = new Vector3(position.x, position.y+5);
        Scenes.Manager.SetTargetPositions(Characters.Sein.Position);
        Game.UI.Cameras.Current.CameraTarget.SetTargetPosition(Characters.Sein.Position);
        Game.UI.Cameras.Current.MoveCameraToTargetInstantly(true);
    }


    public static void returnToStart()
    {
        if (!Characters.Sein.Controller.CanMove || !Characters.Sein.Active)
            return;
        if (Items.NightBerry != null)
            Items.NightBerry.transform.position = new Vector3(-755f, -400f);
        if(Characters.Sein.Abilities.Carry.IsCarrying && !AllowOrbWarps)
            Characters.Sein.Abilities.Carry.CurrentCarryable.Drop();
        RandomizerStatsManager.WarpedToStart();
        RandomizerBonusSkill.LastAltR = Characters.Sein.Position;
        Randomizer.Returning = true;
        Characters.Sein.Position = new Vector3(189f, -215f);
        Characters.Sein.Speed = new Vector3(0f, 0f);
        Characters.Ori.Position = new Vector3(190f, -210f);
        Scenes.Manager.SetTargetPositions(Characters.Sein.Position);
        Game.UI.Cameras.Current.CameraTarget.SetTargetPosition(Characters.Sein.Position);
        Game.UI.Cameras.Current.MoveCameraToTargetInstantly(true);
        int value = World.Events.Find(Randomizer.MistySim).Value;
        if (value != 1 && value != 8)
        {
            World.Events.Find(Randomizer.MistySim).Value = 10;
        }
    }

    //  more reliable hook for game end / credit starts
    public static void onNaruDestroyed() {
        if (Scenes.Manager.CurrentScene.Scene == "theSacrifice" && RandomizerStatsManager.Active)
            {
                RandomizerStatsManager.Finish();
                RandomizerCreditsManager.Initialize();
            }
    }

    public static void showHint(string message)
    {
        LastMessageCredits = false;
        Randomizer.Message = message;
        Randomizer.MessageQueue.Enqueue(message);
    }

    public static void showHint(string message, int frames)
    {
        LastMessageCredits = false;
        Randomizer.Message = message;
        Randomizer.MessageQueue.Enqueue(new object[] {message, frames});
    }

    public static void printInfo(string message)
    {
        Randomizer.MessageQueue.Enqueue(message);
    }

    public static void printInfo(string message, int frames)
    {
        Randomizer.MessageQueue.Enqueue(new object[] {message, frames});
    }

    public static void playLastMessage()
    {
        if(LastMessageCredits)
            Randomizer.showCredits(Randomizer.Message, 5);
        else
            Randomizer.MessageQueue.Enqueue(Randomizer.Message);
    }

    public static void log(string message)
    {
        StreamWriter streamWriter = File.AppendText("randomizer.log");
        streamWriter.WriteLine(DateTime.Now.ToString() + ": " + message);
        streamWriter.Flush();
        streamWriter.Dispose();
    }

    public static bool WindRestored()
    {
        return Sein.World.Events.WindRestored && Scenes.Manager.CurrentScene != null && Scenes.Manager.CurrentScene.Scene != "forlornRuinsResurrection" && Scenes.Manager.CurrentScene.Scene != "forlornRuinsRotatingLaserFlipped";
    }

    public static void getSkill()
    {
        Randomizer.getPickup();
        Randomizer.showProgress();
    }

    public static void hintAndLog(float x, float y)
    {
        string message = ((int)x).ToString() + " " + ((int)y).ToString();
        Randomizer.showHint(message);
        Randomizer.log(message);
    }

    public static int GetHashKey(Vector3 position)
    {
        int baseId = (int)(Math.Floor((double)((int)position.x) / Randomizer.GridFactor) * Randomizer.GridFactor) * 10000 + (int)(Math.Floor((double)((int)position.y) / Randomizer.GridFactor) * Randomizer.GridFactor);
        if(Randomizer.Table.ContainsKey(baseId)) 
            return baseId;
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int offsetCoord = baseId + (int)Randomizer.GridFactor * (10000 * x + y);
                if (Randomizer.Table.ContainsKey(offsetCoord))
                    return offsetCoord;
            }
        }
        for (int x = -2; x <= 2; x += 4)
        {
            for (int y = -1; y <= 1; y++)
            {
                int offsetCoord = baseId + (int)Randomizer.GridFactor * (10000 * x + y);
                if (Randomizer.Table.ContainsKey(offsetCoord))
                    return offsetCoord;
            }
        }
        Randomizer.printInfo("Error finding pickup at " + ((int)position.x).ToString() + ", " + ((int)position.y).ToString());
        return -1;

    }

    public static void getPickup(Vector3 position)
    {
        try {
            if (Randomizer.ColorShift)
                Randomizer.changeColor();
            int hashKey = GetHashKey(position);
            if (hashKey != -1)
            {
                BingoController.OnLoc(hashKey);
                RandomizerStatsManager.IncPickup(hashKey);
                RandomizerSwitch.GivePickup(Randomizer.Table[hashKey], hashKey, true);
                if (Randomizer.HotColdItems.ContainsKey(hashKey)) {
                    set(Randomizer.HotColdItems[hashKey].Id, 1);
                    RandomizerColorManager.UpdateHotColdTarget();
                } else if(Randomizer.HotColdFrags.ContainsKey(hashKey)) {
                    set(Randomizer.HotColdFrags[hashKey].Id, 1);
                    RandomizerColorManager.UpdateHotColdTarget();
                }
                return;
            }
        }
        catch(Exception e) {
            Randomizer.LogError("GetPickup: " + e.Message);
        }
    }

    public static void Update()
    {
        Randomizer.UpdateMessages();
        if (Characters.Sein && SkillTreeManager.Instance != null && SkillTreeManager.Instance.NavigationManager.IsVisible)
        {
            if (Characters.Sein.IsSuspended)
            {
                SkillTreeManager.Instance.NavigationManager.FadeAnimator.SetParentOpacity(1f);
            }
            else
            {
                SkillTreeManager.Instance.NavigationManager.FadeAnimator.SetParentOpacity(RandomizerSettings.QOL.AbilityMenuOpacity);
            }
        }
        Randomizer.Tick();
        if (Characters.Sein && !Characters.Sein.IsSuspended)
        {
            RandomizerBonus.Update();
            if (!Randomizer.ColorShift)
            {
                RandomizerColorManager.UpdateColors();
            }
            if (get(82) > 0 && Items.NightBerry != null)
            {
                Items.NightBerry.transform.position = new Vector3(-910f, -300f);
                set(82, 0);
            }
            if(RandomizerBonusSkill.LevelExplosionCooldown > 0)
            {
                RandomizerBonusSkill.LevelExplosionCooldown--;
                if(RandomizerBonusSkill.LevelExplosionCooldown > 10) {
                    Characters.Sein.Energy.SetCurrent(RandomizerBonusSkill.OldEnergy);
                    Characters.Sein.Mortality.Health.SetAmount(RandomizerBonusSkill.OldHealth);
                }
            }
            if (Randomizer.Chaos)
            {
                RandomizerChaosManager.Update();
            }
            if (Randomizer.Sync)
            {
                RandomizerSyncManager.Update();
            }
            if (Randomizer.Warping > 0) {
                if (Randomizer.DelayedWarp)
                {
                    Randomizer.DelayedWarp = false;
                    Randomizer.WarpTo(Randomizer.WarpTarget, Randomizer.Warping);
                } else {
                    Characters.Sein.Position = Randomizer.WarpTarget;
                    Characters.Sein.Speed = new Vector3(0f, 0f);
                    Characters.Ori.Position = new Vector3(Randomizer.WarpTarget.x, Randomizer.WarpTarget.y-5);
                    bool loading = false;
                    foreach (SceneManagerScene sms in Scenes.Manager.ActiveScenes)
                    {
                        if (sms.CurrentState == SceneManagerScene.State.Loading)
                        {
                            loading = true;
                            break;
                        }
                    }
                    if(!loading)
                    Randomizer.Warping--;
                    if(Randomizer.Warping == 0 && Randomizer.SaveAfterWarp)
                    {
                        GameController.Instance.CreateCheckpoint();
                        RandomizerStatsManager.OnSave(false);
                        GameController.Instance.SaveGameController.PerformSave();
                        Randomizer.SaveAfterWarp = false;
                    }
                }                
            } else if (Randomizer.Returning)
            {
                Characters.Sein.Position = new Vector3(189f, -215f);
                if (Scenes.Manager.CurrentScene?.Scene == "sunkenGladesRunaway")
                {
                    Randomizer.Returning = false;
                }
            }
        }
        if (CreditsActive)
            return;
        if (RandomizerRebinding.ReloadSeed.IsPressed())
        {
            Randomizer.initialize();
            if(RandomizerSettings.Dev)
            {
                Randomizer.log("Reset and loaded seed: " + SeedMeta);
            }
            Randomizer.showSeedInfo();
            return;
        }
        if(Characters.Sein)
        {
            if (RandomizerRebinding.ShowStats.IsPressed())
            {
                RandomizerStatsManager.ShowStats(10);
                if(BingoController.Active)
                    Randomizer.log("Current bingo state: \n"+BingoController.GetJson());
                return;
            }
            if (RandomizerRebinding.ListTrees.IsPressed())
            {
                Randomizer.MessageQueueTime = 0;
                RandomizerTrackedDataManager.ListTrees();
                return;
            }
            if (RandomizerRebinding.ListRelics.IsPressed())
            {
                Randomizer.MessageQueueTime = 0;
                RandomizerTrackedDataManager.ListRelics();
                return;
            }
            if (RandomizerRebinding.ListMapAltars.IsPressed())
            {
                Randomizer.MessageQueueTime = 0;
                RandomizerTrackedDataManager.ListMapstones();
                return;
            }
            if (RandomizerRebinding.ListTeleporters.IsPressed())
            {
                Randomizer.MessageQueueTime = 0;
                RandomizerTrackedDataManager.ListTeleporters();
                return;
            }
            if (RandomizerRebinding.ShowBonuses.IsPressed()) {
                RandomizerBonus.ListBonuses();
                return;
            }
            if (RandomizerRebinding.BonusSwitch.IsPressed())
            {
                RandomizerBonusSkill.SwitchBonusSkill();
                return;
            }
            if (RandomizerRebinding.BonusToggle.IsPressed())
            {
                RandomizerBonusSkill.ActivateBonusSkill();
                return;
            }
            if (RandomizerRebinding.Bonus1.IsPressed())
            {
                RandomizerBonusSkill.BonusSkillSlot(0);
                return;
            }
            if (RandomizerRebinding.Bonus2.IsPressed())
            {
                RandomizerBonusSkill.BonusSkillSlot(1);
                return;
            }
            if (RandomizerRebinding.Bonus3.IsPressed())
            {
                RandomizerBonusSkill.BonusSkillSlot(2);
                return;
            }
            if (RandomizerRebinding.Bonus4.IsPressed())
            {
                RandomizerBonusSkill.BonusSkillSlot(3);
                return;
            }
            if (RandomizerRebinding.Bonus5.IsPressed())
            {
                RandomizerBonusSkill.BonusSkillSlot(4);
                return;
            }
            if (RandomizerRebinding.Bonus6.IsPressed())
            {
                RandomizerBonusSkill.BonusSkillSlot(5);
                return;
            }
            if (RandomizerRebinding.Bonus7.IsPressed())
            {
                RandomizerBonusSkill.BonusSkillSlot(6);
                return;
            }
            if (RandomizerRebinding.Bonus8.IsPressed())
            {
                RandomizerBonusSkill.BonusSkillSlot(7);
                return;
            }
            if (RandomizerRebinding.Bonus9.IsPressed())
            {
                RandomizerBonusSkill.BonusSkillSlot(8);
                return;
            }
        }

        if (RandomizerRebinding.ReplayMessage.IsPressed())
        {
            Randomizer.playLastMessage();
            return;
        }
        if (RandomizerRebinding.ReturnToStart.IsPressed() && Characters.Sein && Randomizer.Warping <= 0)
        {
            if(CanWarp > 0 && Vector3.Distance(Randomizer.WarpSource, Characters.Sein.Position) < 7)
            {
                Randomizer.WarpTo(Randomizer.WarpTarget, 15);
                Randomizer.CanWarp = 0;
                return;
            }
            if(Randomizer.AltRDisabled || RandomizerBonus.AltRDisabled())
            {
                Randomizer.printInfo("Return to start is disabled!");
                return;
            }

            if (RandomizerSettings.Game.UseTeleportAnywhere)
            {
                if (Characters.Sein.Active && !Characters.Sein.IsSuspended && Characters.Sein.Controller.CanMove && !UI.MainMenuVisible)
                {
                    if (TeleporterController.CanTeleport(null))
                    {
                        TeleporterController.Show("sunkenGlades");
                        Randomizer.IsUsingRandomizerTeleportAnywhere = true;
                    }
                    else
                    {
                        Randomizer.printInfo("No #Spirit Wells# have been activated yet!");
                    }
                }
            }
            else
            {
                Randomizer.returnToStart();
            }
            return;
        }
        if (RandomizerRebinding.ShowProgress.IsPressed() && Characters.Sein)
        {
            Randomizer.MessageQueueTime = 0;
            Randomizer.showProgress();
            return;
        }
        if (RandomizerRebinding.ColorShift.IsPressed())
        {
            string obj = "Color shift enabled";
            if (Randomizer.ColorShift)
            {
                obj = "Color shift disabled";
            }
            else
            {
                Randomizer.changeColor();
            }
            Randomizer.ColorShift = !Randomizer.ColorShift;
            Randomizer.printInfo(obj);
        }
        if (RandomizerRebinding.ToggleChaos.IsPressed() && Characters.Sein)
        {
            if (Randomizer.Chaos)
            {
                Randomizer.showChaosMessage("Chaos deactivated");
                Randomizer.Chaos = false;
                RandomizerChaosManager.ClearEffects();
                return;
            }
            Randomizer.showChaosMessage("Chaos activated");
            Randomizer.Chaos = true;
            return;
        }
        else if (RandomizerRebinding.ChaosVerbosity.IsPressed() && Randomizer.Chaos)
        {
            Randomizer.ChaosVerbose = !Randomizer.ChaosVerbose;
            if (Randomizer.ChaosVerbose)
            {
                Randomizer.showChaosMessage("Chaos messages enabled");
                return;
            }
            Randomizer.showChaosMessage("Chaos messages disabled");
            return;
        }
        else
        {
            if (RandomizerRebinding.ForceChaosEffect.IsPressed() && Randomizer.Chaos && Characters.Sein)
            {
                RandomizerChaosManager.SpawnEffect();
                return;
            }
            return;
        }
    }

    public static void showChaosEffect(string message)
    {
        if (Randomizer.ChaosVerbose)
        {
            Randomizer.printInfo(message);
        }
    }

    public static void showChaosMessage(string message)
    {
        Randomizer.printInfo(message);
    }

    public static void getMapStone()
    {
        if (!Randomizer.ProgressiveMapStones) {
            Randomizer.getPickup();
            return;
        }
        RandomizerBonus.CollectMapstone();
        RandomizerStatsManager.FoundMapstone();

        if (Randomizer.ColorShift) {
            Randomizer.changeColor();
        }
        BingoController.OnLoc(20 + RandomizerBonus.MapStoneProgression() * 4);
        RandomizerSwitch.GivePickup(Randomizer.Table[20 + RandomizerBonus.MapStoneProgression() * 4], 20 + RandomizerBonus.MapStoneProgression() * 4, true);
    }

    public static void showProgress()
    {
        try {
            string text = "";
            text += "BETA BETA BETA BETA\n";
            if(Randomizer.ForceTrees || Randomizer.CluesMode)
            {
                if (RandomizerBonus.SkillTreeProgression() == 10)
                {
                    text += "$Trees (10/10)$  ";
                }
                else
                {
                    text = text + "Trees (" + RandomizerBonus.SkillTreeProgression().ToString() + "/10)  ";
                }
            }    
            if (Randomizer.WorldTour && Characters.Sein) {
                int relics = get(302);
                if(relics < Randomizer.RelicCount) {
                    text += "Relics (" + relics.ToString() + "/"+Randomizer.RelicCount.ToString() + ")  ";
                } else {
                    text += "$Relics (" + relics.ToString() + "/"+Randomizer.RelicCount.ToString() + ")$  ";
                }
            }
            if (RandomizerBonus.MapStoneProgression() == 9 && Randomizer.ForceMaps)
            {
                text += "$Maps (9/9)$  ";
            }
            else
            {
                text = text + "Maps (" + RandomizerBonus.MapStoneProgression().ToString() + "/9)  ";
            }
            text = text + "Total (" + get(1600).ToString() + "/256)\n";
            if (Randomizer.CluesMode)
            {
                text += RandomizerClues.GetClues();
            }
            else
            {
                if (Keys.GinsoTree)
                {
                    text += "*WV (3/3)*  ";
                }
                else
                {
                    text = text + " *WV* (" + RandomizerBonus.WaterVeinShards().ToString() + "/3)  ";
                }
                if (Keys.ForlornRuins)
                {
                    text += "#GS (3/3)#  ";
                }
                else
                {
                    text = text + "#GS# (" + RandomizerBonus.GumonSealShards().ToString() + "/3)  ";
                }
                if (Keys.MountHoru)
                {
                    text += "@SS (3/3)@";
                }
                else
                {
                    text = text + "@SS@ (" + RandomizerBonus.SunstoneShards().ToString() + "/3)";
                }
            }
            if (Randomizer.fragsEnabled)
            {
                text = string.Concat(new string[] { text, " Frags: (", RandomizerBonus.WarmthFrags().ToString(), "/", Randomizer.fragKeyFinish.ToString(), ")" });
            }
            if(RandomizerBonus.ForlornEscapeHint())
            {
                string s_color = "";
                string g_color = "";
                 if(Characters.Sein)
                 {
                    if(Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Stomp))
                        s_color = "$";
                    if(Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Grenade))
                        g_color = "$";
                 }

                text += "\n" +s_color + "Stomp: " + StompZone + s_color + g_color+ "    Grenade: "+ GrenadeZone + g_color;
            }
            Randomizer.printInfo(text);
        }
        catch(Exception e) {
            Randomizer.LogError("ShowProgress: " + e.Message);
        }
    }

    public static void showSeedInfo()
    {

        string seedInfo = "v" + Randomizer.VERSION;
        seedInfo += "- seed loaded: " + Randomizer.SeedMeta;
        Randomizer.printInfo(seedInfo);
    }

    public static void changeColor()
    {
        if (Characters.Sein)
        {
            Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = new Color(FixedRandom.Values[0], FixedRandom.Values[1], FixedRandom.Values[2], 0.5f);
        }
    }

    public static void UpdateMessages()
    {
        if (Randomizer.MessageQueueTime <= 0)
        {
            if (Randomizer.MessageQueue.Count == 0)
            {
                return;
            }
            object queueItem = Randomizer.MessageQueue.Dequeue();
            string message;
            if (queueItem is object[])
            {
                message = (string)((object[])queueItem)[0];
                Randomizer.MessageQueueTime = (int)((object[])queueItem)[1] / 2;
            }
            else
            {
                message = (string)queueItem;
                Randomizer.MessageQueueTime = 60;
            }
            if(message != "") {
                Randomizer.MessageProvider.SetMessage(message);
                Game.UI.Hints.Show(Randomizer.MessageProvider, HintLayer.GameSaved, (float)Randomizer.MessageQueueTime / 30f + 1f);
            }
        }
        Randomizer.MessageQueueTime--;
    }

    public static void OnDeath()
    {
        RandomizerBonusSkill.OnDeath();
        RandomizerTrackedDataManager.UpdateBitfields();
        RandomizerStatsManager.OnDeath();

        if (Randomizer.IsUsingRandomizerTeleportAnywhere)
        {
            TeleporterController.Instance.CancelTeleport();
            UI.Menu.HideMenuScreen(false);
        }
    }

    public static void OnSave()
    {
        RandomizerBonusSkill.OnSave();
    }

    public static bool canFinalEscape()
    {
        return Randomizer.canFinalEscape(true);
    }

    public static bool canFinalEscape(bool verbose)
    {
        if (Randomizer.fragsEnabled && RandomizerBonus.WarmthFrags() < Randomizer.fragKeyFinish)
        {
            if(verbose)
                Randomizer.printInfo(string.Concat(new string[]
                {
                    "Frags: (",
                    RandomizerBonus.WarmthFrags().ToString(),
                    "/",
                    Randomizer.fragKeyFinish.ToString(),
                    ")"
                }));
            return false;
        }
        if (Randomizer.WorldTour) {
            int relics = get(302);
            if(relics < Randomizer.RelicCount) {
                if(verbose)
                    Randomizer.printInfo("Relics (" + relics.ToString() + "/" + Randomizer.RelicCount.ToString() + ")");
                return false;
            }
        }
        if (Randomizer.ForceTrees && RandomizerBonus.SkillTreeProgression() < 10)
        {

            if(verbose)
                Randomizer.printInfo("Trees (" + RandomizerBonus.SkillTreeProgression().ToString() + "/10)");
            return false;
        }
        if (Randomizer.ForceMaps && RandomizerBonus.MapStoneProgression() < 9)
        {
            if(verbose)
                Randomizer.printInfo("Maps (" + RandomizerBonus.MapStoneProgression().ToString() + "/9)");
            return false;
        }
        return true;
    }

    public static void EnterDoor(Vector3 position)
    {
        if (!Randomizer.Entrance)
        {
            return;
        }
        int num = (int)(Math.Floor((double)((int)position.x) / Randomizer.GridFactor) * Randomizer.GridFactor) * 10000 + (int)(Math.Floor((double)((int)position.y) / Randomizer.GridFactor) * Randomizer.GridFactor);
        if (Randomizer.DoorTable.ContainsKey(num))
        {
            Characters.Sein.Position = (Vector3)Randomizer.DoorTable[num];
            return;
        }
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (Randomizer.DoorTable.ContainsKey(num + (int)Randomizer.GridFactor * (10000 * i + j)))
                {
                    Characters.Sein.Position = (Vector3)Randomizer.DoorTable[num + (int)Randomizer.GridFactor * (10000 * i + j)];
                    return;
                }
            }
        }
        for (int k = -2; k <= 2; k += 4)
        {
            for (int l = -1; l <= 1; l++)
            {
                if (Randomizer.DoorTable.ContainsKey(num + (int)Randomizer.GridFactor * (10000 * k + l)))
                {
                    Characters.Sein.Position = (Vector3)Randomizer.DoorTable[num + (int)Randomizer.GridFactor * (10000 * k + l)];
                    return;
                }
            }
        }
        Randomizer.showHint("Error using door at " + ((int)position.x).ToString() + ", " + ((int)position.y).ToString());
    }

    public static void getSkill(int tree)
    {
        RandomizerTrackedDataManager.SetTree(tree);
        Randomizer.getSkill();
    }

    public static int ordHash(string s)
    {
        int num = 0;
        foreach (char c in s)
        {
            num += (int)c;
        }
        return num;
    }

    public static void PrintImmediately(string text, int seconds, bool mute, bool setMessage, bool devOnly)
    {
        Print(text, seconds, mute, setMessage, devOnly, true);
    }

    public static void Print(string text, int seconds, bool mute, bool setMessage, bool devOnly, bool immediate)
    {
        if(devOnly && !RandomizerSettings.Dev)
            return;
        if(immediate)
        {
            MessageProvider.SetMessage(text);
            if(mute)
            {
                CachedVolume = Math.Max(GameSettings.Instance.SoundEffectsVolume, CachedVolume);
                GameSettings.Instance.SoundEffectsVolume = 0f;
                ResetVolume = 3;
            }
            Game.UI.Hints.Show(Randomizer.MessageProvider, HintLayer.GameSaved, (float)seconds);
            if(setMessage)
            {
                Message = text;
                LastMessageCredits = true;
            }            
        } else {
            Randomizer.MessageQueue.Enqueue(new object[] {text, 60 * seconds});
            LastMessageCredits = false;
        }
    }

    public static void LogError(string errorText) {
        Randomizer.log(errorText);
        Randomizer.PrintImmediately(errorText, 15, false, false, true);
    }

    public static void showCredits(string text, int seconds)
    {
        PrintImmediately(text, seconds, true, true, false);
    }

    // Token: 0x06003848 RID: 14408
    public static void Tick()
    {
        try {
            long old_tick = Randomizer.LastTick;
            Randomizer.LastTick = DateTime.Now.Ticks % 10000000L;
            if (Randomizer.LastTick < old_tick)
            {
                if(RandomizerSettings.QOL.CursorLock)
                    Cursor.lockState = CursorLockMode.Confined;
                BingoController.Tick();
                if(ResetVolume == 1)
                {
                    ResetVolume = 0;
                    GameSettings.Instance.SoundEffectsVolume = CachedVolume;
                } else if(ResetVolume > 1) {
                    ResetVolume--;
                }
                if(CanWarp > 0)
                {
                    CanWarp--;
                }
                if(RepeatableCooldown > 0)
                    RepeatableCooldown--;
                if(RandomizerStatsManager.StatsTimer > 0)
                    RandomizerStatsManager.StatsTimer--;
                RandomizerStatsManager.IncTime();
                if(Scenes.Manager.CurrentScene != null)
                {
                    string scene = Scenes.Manager.CurrentScene.Scene;
                    if(scene == "thornfeltSwampActTwoStart" && NeedGinsoEscapeCleanup) {
                        try
                        {
                            GameController.Instance.CreateCheckpoint();
                            RandomizerStatsManager.OnSave(false);
                            GameController.Instance.SaveGameController.PerformSave();
                            GameController.Instance.SaveGameController.PerformLoad();
                        }
                        catch (Exception e)
                        {
                            Randomizer.LogError("GinsoEscapeCleanup: " + e.Message);
                        }
                        NeedGinsoEscapeCleanup = false;
                    }
                    if(scene == "titleScreenSwallowsNest")
                    {
                        ResetTrackerCount++;
                        if(ResetTrackerCount > 10)
                        {
                            RandomizerTrackedDataManager.Reset();
                            ResetTrackerCount = 0;
                        }
                        if(RandomizerCreditsManager.CreditsDone)
                        {
                            RandomizerCreditsManager.CreditsDone = false;
                        }
                    }
                    else if(scene == "creditsScreen")
                    {
                        if(!CreditsActive && !RandomizerCreditsManager.CreditsDone)
                        {
                            CreditsActive = true;
                        }
                    }
                }

                if(CreditsActive && !RandomizerCreditsManager.CreditsDone)
                    RandomizerCreditsManager.Tick();

                if(Characters.Sein)
                {
                    if(!Characters.Sein.IsSuspended && Scenes.Manager.CurrentScene != null)
                    {
                        if(GoalModeFinish && RandomizerSyncManager.NetworkFree && Randomizer.canFinalEscape(false))
                        {
                            RandomizerBonusSkill.UnlockCreditWarp("Goal mode(s) completed!");
                        }
                        ResetTrackerCount = 0;
                        RandomizerTrackedDataManager.UpdateBitfields();
                        RandomizerColorManager.UpdateHotColdTarget();
                        if (Characters.Sein.Position.y > 935f && Sein.World.Events.WarmthReturned && Scenes.Manager.CurrentScene.Scene == "ginsoTreeWaterRisingEnd")
                        {
                            if (Characters.Sein.Abilities.Bash && Characters.Sein.Abilities.Bash.IsBashing)
                            {
                                Characters.Sein.Abilities.Bash.BashGameComplete(0f);
                            }
                            Characters.Sein.Position = new Vector3(750f, -120f);
                            return;
                        }
                        if (Characters.Sein.Position.y > -225f && Scenes.Manager.CurrentScene.Scene == "forlornRuinsResurrection")
                        {
                            if (Characters.Sein.Abilities.Bash && Characters.Sein.Abilities.Bash.IsBashing)
                            {
                                Characters.Sein.Abilities.Bash.BashGameComplete(0f);
                            }
                            Characters.Sein.Position = new Vector3(-1350f, -420f);
                            return;
                        }                        
                        if (Scenes.Manager.CurrentScene.Scene == "catAndMouseResurrectionRoom" && !Randomizer.canFinalEscape()) {
                            if (Randomizer.Entrance) {
                                Randomizer.EnterDoor(new Vector3(-242f, 489f));
                                return;
                            }
                            Characters.Sein.Position = new Vector3(20f, 105f);
                            return;
                        }
                        else if (!Characters.Sein.Controller.CanMove && Scenes.Manager.CurrentScene.Scene == "moonGrottoGumosHideoutB") {
                            Randomizer.LockedCount++;
                            if (Randomizer.LockedCount >= 4)
                            {
                                GameController.Instance.ResetInputLocks();
                                return;
                            }
                        }
                        else {
                            Randomizer.LockedCount = 0;
                        }
                    }
                    if(RandomizerSyncManager.NetworkFree)
                    {
                        foreach (GameMapTeleporter gameMapTP in TeleporterController.Instance.Teleporters)
                        {
                            if(gameMapTP.Activated)
                                continue;
                            if(gameMapTP.Identifier == "ginsoTree" && get(1024) == 1 && RandomizerBonus.WaterVeinShards() >= 2)
                            {
                                TeleporterController.Activate(Randomizer.TeleportTable["Ginso"].ToString(), false);
                                Randomizer.MessageQueue.Enqueue("*Ginso teleporter activated*");
                            }
                            else if(gameMapTP.Identifier == "forlorn" && get(1025) == 1 && RandomizerBonus.GumonSealShards() >= 2)
                            {
                                TeleporterController.Activate(Randomizer.TeleportTable["Forlorn"].ToString(), false);
                                Randomizer.MessageQueue.Enqueue("#Forlorn teleporter activated#");
                            }
                            else if(gameMapTP.Identifier == "mountHoru" && get(1026) == 1 && RandomizerBonus.SunstoneShards() >= 2)
                            {
                                TeleporterController.Activate(Randomizer.TeleportTable["Horu"].ToString(), false);
                                Randomizer.MessageQueue.Enqueue("@Horu teleporter activated@");
                            }
                        }
                    }
                }
            }
        } catch (Exception e2)
        {
            Randomizer.LogError("Tick: " + e2.Message);
        }
    }

    public static Vector3 HashKeyToVector(int key)
    {
        if (key >= 0)
        {
            if (key % 10000 > 5000)
            {
                return new Vector3((float)key / 10000f, -(float)(10000 - key % 10000));
            }
            return new Vector3((float)key / 10000f, (float)(key % 10000));
        }
        else
        {
            if (-key % 10000 > 5000)
            {
                return new Vector3((float)key / 10000f, (float)(10000 - -(float)key % 10000));
            }
            return new Vector3((float)key / 10000f, -(float)(-(float)key % 10000));
        }
    }

    public static int RepeatableCheck(Vector3 position){
        // 2: grabbable, 1: cooldown, 0: not repeatable
        try{
            if(Repeatables.Contains(GetHashKey(position)))
            {
                if(RepeatableCooldown <= 0)
                {
                    RepeatableCooldown = 2;
                    return 2;
                } else {
                    return 1;
                }
            }
        }
        catch(Exception e) {
            Randomizer.LogError("RepeatableCheck: " + e.Message);
        }
        return 0;
    }

    private static string cct(IEnumerable<char> cs) => new String(cs.ToArray());

    public static bool ParseFlags(string seed, string[] rawFlags) {
        bool doBingo = false;

        foreach (string rawFlag in rawFlags)
        {
            string flag = rawFlag.ToLower();
            if (flag == "ohko")
            {
                Randomizer.OHKO = true;
            }
            if (flag == "race") {
                SeedMeta = $"{String.Join(",", rawFlags.Select(f => f.ToLower().StartsWith("sync") ? "Sync" + cct(f.Skip(f.IndexOf('.') - 1)) : f).ToArray())}|{cct(seed.Skip(1).SkipWhile(c => Char.IsLower(c)))}"; // yeah that's right we're LINQ wizards baby anyways this is just a bit of censorship to make things a bit harder for people trying to cheat
            }
            if (flag.StartsWith("worldtour"))
            {
                Randomizer.WorldTour = true;
                if(flag.Contains("=")) {
                    Randomizer.RelicCountOverride = true;
                    Randomizer.RelicCount = int.Parse(flag.Substring(10));
                }
            }
            if (flag.StartsWith("sync"))
            {
                Randomizer.Sync = true;
                Randomizer.SyncId = flag.Substring(4);
                RandomizerSyncManager.Initialize();
            }
            if (flag.StartsWith("frags/"))
            {
                Randomizer.fragsEnabled = true;
                string[] fragParams = flag.Split(new char[]{'/'});
                Randomizer.maxFrags =  int.Parse(fragParams[2]);
                Randomizer.fragKeyFinish = int.Parse(fragParams[1]);
            }
            if (flag.StartsWith("mode="))
            {
                string modeStr = flag.Substring(5).ToLower();
                int syncMode;
                if (modeStr == "shared")
                {
                    syncMode = 1;
                } else if (modeStr == "none") {
                    syncMode = 4;
                } else {
                    syncMode = int.Parse(modeStr);
                }
                Randomizer.SyncMode = syncMode;
            }
            if(flag == "bingo")
            {
                doBingo = true;
            }
            if (flag == "noextraexp")
            {
                Randomizer.IgnoreEnemyExp = true;
            }
            if (flag == "0xp")
            {
                Randomizer.IgnoreEnemyExp = true;
                Randomizer.ZeroXP = true;
            }
            if (flag == "nobonus")
            {
                Randomizer.BonusActive = false;
            }
            if (flag == "nonprogressivemapstones")
            {
                Randomizer.ProgressiveMapStones = false;
            }
            if (flag == "forcetrees")
            {
                Randomizer.ForceTrees = true;
            }
            if (flag == "forcemaps")
            {
                Randomizer.ForceMaps = true;
            }
            if (flag == "clues")
            {
                Randomizer.CluesMode = true;
            }
            if (flag == "shards")
            {
                Randomizer.Shards = true;
            }
            if (flag == "entrance")
            {
                Randomizer.Entrance = true;
            }
            if (flag == "closeddungeons")
            {
                Randomizer.OpenMode = false;
            }
            if (flag == "openworld")
            {
                Randomizer.OpenWorld = true;
            }
            if (flag.StartsWith("hotcold="))
            {
                Randomizer.HotCold = true;
                Randomizer.HotColdTypes = new HashSet<string>(rawFlag.Substring(8).Split(new char[]{'+'}).ToList<string>());
            }
            if (flag.StartsWith("sense="))
            {
                Randomizer.HotColdTypes = new HashSet<string>(rawFlag.Substring(6).Split(new char[]{'+'}).ToList<string>());
            }
            if (flag == "noaltr")
            {
                Randomizer.AltRDisabled = true;
            }
            if (flag == "stomptriggers")
            {
                Randomizer.StompTriggers = true;
            }
            if (flag == "goalmodefinish")
            {
                Randomizer.GoalModeFinish = true;
            }
            if (flag == "orbwarp")
            {
                Randomizer.AllowOrbWarps = true;
            }
            if (flag == "randomizedfirstenergy")
            {
                Randomizer.RandomizedFirstEnergy = true;
            }
        }
        return doBingo;

    }
    private static int get(int item) { return Characters.Sein.Inventory.GetRandomizerItem(item); }
    private static int set(int item, int value) { return Characters.Sein.Inventory.SetRandomizerItem(item, value); }

    public static bool SeenCoord(int coord) {
        if(!RandomizerTrackedDataManager.CoordsMap.ContainsKey(coord))
        {
            if(coord != 2 && coord != -1)
                Randomizer.LogError("Unknown coord: " + coord.ToString());
            return false;
        }
        int locID = 1560 + RandomizerTrackedDataManager.CoordsMap[coord]/32;
        return  0 != (get(locID) >> (RandomizerTrackedDataManager.CoordsMap[coord]%32)) % 2;
    }

    public static bool HaveCoord(int coord) {
        if(!RandomizerTrackedDataManager.CoordsMap.ContainsKey(coord))
        {
            if(coord != 2 && coord != -1)
                Randomizer.LogError("Unknown coord: " + coord.ToString());
            return false;
        }
        int locID = 930 + RandomizerTrackedDataManager.CoordsMap[coord]/32;
        return  0 != (get(locID) >> (RandomizerTrackedDataManager.CoordsMap[coord]%32)) % 2;
    }

    public static void OnCoord(int coord) {
        if(!RandomizerTrackedDataManager.CoordsMap.ContainsKey(coord))
        {
            if(coord != 2 && coord != -1)
                Randomizer.LogError("Unknown coord: " + coord.ToString());
            return;
        }
        int locID = 1560 + RandomizerTrackedDataManager.CoordsMap[coord]/32;
        int offset = RandomizerTrackedDataManager.CoordsMap[coord]%32;
        int current = get(locID);
        // set Seen
        if((current >> offset) % 2 == 0)
            set(locID, current + (1 << offset));
        // set Have
        locID = 930 + RandomizerTrackedDataManager.CoordsMap[coord]/32;
        current = get(locID);
        if((current >> offset) % 2 == 0)
            set(locID, current + (1 << offset));
    }

    public static void ApplyGrabForgiveness()
    {
        if (!RandomizerSettings.Game.BlackrootOrbRoomClimbAssist)
        {
            Randomizer.GrabForgivenessFrames = 0f;
            return;
        }

        // XP orb jump in Blackroot lantern room, right side (initial crappy slope)
        if (new Rect(152.26f, -298.6f, 0.02f, 0.7f).Contains(Characters.Sein.PlatformBehaviour.PlatformMovement.Position2D))
        {
            Randomizer.GrabForgivenessFrames = 4f;
            return;
        }

        // XP orb jump in Blackroot lantern room, left side (*extra* crappy slope)
        if (new Rect(147.2f, -296.5f, 0.1f, 1f).Contains(Characters.Sein.PlatformBehaviour.PlatformMovement.Position2D))
        {
            Randomizer.GrabForgivenessFrames = 8f;
            return;
        }

        Randomizer.GrabForgivenessFrames = 0f;
    }

    public static bool DoesGrabForgivenessExpire(float time)
    {
        float scaledTime = Mathf.Round(time * 120f);
        bool expires = Randomizer.GrabForgivenessFrames < scaledTime;
        Randomizer.GrabForgivenessFrames -= Mathf.Min(Randomizer.GrabForgivenessFrames, Mathf.Round(time * 120f));
        return expires;
    }

    public static void SetupNewGame()
    {
        // start everyone with 1 energy on all difficulties if "RandomizedFirstEnergy" flag set
        if (Randomizer.RandomizedFirstEnergy)
        {
            Characters.Sein.Energy.Max += 1f;
            Characters.Sein.Energy.SetCurrent(Characters.Sein.Energy.Max);
        }

        // relaxed difficulty players start with +1 health and +1 energy, plus the first ability in each tree
        if (DifficultyController.Instance.Difficulty == DifficultyMode.Easy)
        {
            Characters.Sein.Mortality.Health.MaxHealth += 4;
            Characters.Sein.Mortality.Health.SetAmount(Characters.Sein.Mortality.Health.MaxHealth);

            Characters.Sein.Energy.Max += 1f;
            Characters.Sein.Energy.SetCurrent(Characters.Sein.Energy.Max);

            Characters.Sein.PlayerAbilities.Rekindle.HasAbility = true;
            Characters.Sein.PlayerAbilities.Magnet.HasAbility = true;
            Characters.Sein.PlayerAbilities.QuickFlame.HasAbility = true;
        }

        // flag this save file for OpenWorld/ClosedDungeons flags
        if (Randomizer.OpenWorld)
        {
            set(800, 1);
        }

        if (!Randomizer.OpenMode)
        {
            set(801, 1);
        }

        // grant other spawn items determined by the seed
        if (Randomizer.SpawnWith != "")
        {
            RandomizerAction spawnItem;
            if (Randomizer.StringKeyPickupTypes.Contains(SpawnWith.Substring(0, 2)))
            {
                spawnItem = new RandomizerAction(SpawnWith.Substring(0, 2), SpawnWith.Substring(2));
            }
            else
            {
                spawnItem = new RandomizerAction(SpawnWith.Substring(0, 2), int.Parse(SpawnWith.Substring(2)));
            }
            RandomizerSwitch.GivePickup(spawnItem, 2, true);
        }
    }

    public static Dictionary<int, RandomizerAction> Table;
    public static bool GiveAbility;
    public static double GridFactor;
    public static RandomizerMessageProvider MessageProvider;
    public static bool OHKO;
    public static bool ZeroXP;
    public static bool BonusActive;
    public static string Message;
    public static bool Chaos;
    public static bool ChaosVerbose;
    public static float DamageModifier;
    public static bool ProgressiveMapStones;
    public static bool ForceTrees;
    public static string SeedMeta;
    public static Hashtable TeleportTable;
    public static WorldEvents MistySim;
    public static bool Returning;
    public static bool CluesMode;
    public static bool Shards;
    public static bool ColorShift;
    public static Queue MessageQueue;
    public static int MessageQueueTime;
    public static bool Sync;
    public static string SyncId;
    public static int SyncMode;
    public static List<string> StringKeyPickupTypes;
    public static bool ForceMaps;
    public static bool Entrance;
    public static Hashtable DoorTable;
    public static bool QueueBash;
    public static bool BashWasQueued;
    public static bool BashTap;
    public static bool WorldTour;
    public static bool fragsEnabled;
    public static int fragKeyFinish;
    public static int maxFrags;
    public static ArrayList GinsoData;
    public static ArrayList ForlornData;
    public static ArrayList HoruData;
    public static bool OpenMode;

    public static long LastTick;

    public static ArrayList GladesData;

    public static int LockedCount;

    public static int ResetTrackerCount;

    public static Vector3 WarpTarget;

    public static int Warping;

    public static bool HotCold;

    public static HashSet<string> HotColdTypes;

    public static Dictionary<int, RandomizerHotColdItem> HotColdItems;

    public static Dictionary<int, RandomizerHotColdItem> HotColdFrags;

    public static List<int> HotColdMaps;

    public static List<int> HotColdMapsWithFrags;

    public static Dictionary<string, string> RelicZoneLookup;

    public static int RelicCount;

    public static ArrayList ValleyStompDoorData;

    public static string GrenadeZone;
    // welcome to the...
    public static string StompZone;

    public static bool CreditsActive;

    public static float CachedVolume;

    public static int ResetVolume;

    public static bool LastMessageCredits;

    public static bool sacrificeStarted;

    public static bool AltRDisabled;

    public static HashSet<int> Repeatables;

    public static int RepeatableCooldown;

    public static bool OpenWorld;

    public static bool StompTriggers;

    public static string SpawnWith;

    public static bool DelayedWarp;

    public static bool SaveAfterWarp;

    public static bool IgnoreEnemyExp;

    public static bool NeedGinsoEscapeCleanup;

    public static bool RelicCountOverride;

    public static Vector3 WarpSource;

    public static int CanWarp;

    public static bool GoalModeFinish;

    public static bool AllowOrbWarps;
    
    public static bool GrenadeJumpQueued;

    public static float GrabForgivenessFrames;

    public static bool IsUsingRandomizerTeleportAnywhere;

    public static bool RandomizedFirstEnergy;
}
