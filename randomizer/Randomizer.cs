using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using B83.Win32;
using Core;
using Game;
using Protogen;
using Sein.World;
using UnityEngine;

public static class Randomizer
{
    public static string VERSION = "4.0.15";
    public static void initialize()
    {
        try {
            Randomizer.OHKO = false;
            Randomizer.ZeroXP = false;
            Randomizer.BonusActive = true;
            Randomizer.Chaos = false;
            Randomizer.ChaosVerbose = false;
            Randomizer.Returning = false;
            Randomizer.Sync = false;
            Randomizer.SyncId = "";
            Randomizer.ForceMaps = false;
            Randomizer.SyncMode = 4;
            Randomizer.StringKeyPickupTypes = new List<string> {"TP", "SH", "NO", "WT", "MU", "HN", "WP", "RP", "WS", "TW", "NB"};
            Randomizer.RandomExpNames  = new List<String>() { "Apples",  "Bananas",  "Bells",  "Bits",  "Bolts",  "Boonbucks",  "Boxings",  "Brick",  "Brownie Points", 
     "Bytes",  "Cash",  "Coins",  "Comments",  "Credits",  "Crowns",  "Diamonds",  "Dollars",  "Dollerydoos",  "Doubloons",  "Drams",  "EXP",  "Echoes",  "Emeralds",  "Euros",
     "Exalted Orbs",  "Experience",  "Farthings",  "Fish",  "Fun",  "GP",  "Gallons",  "Geo",  "Gil",  "Glod",  "Gold",  "Hryvnia",  "Hugs",  "Kalganids",  "Leaves",  "Likes",
     "Marbles",  "Minerals",  "Money",  "Munny",  "Nobles",  "Notes",  "Nuts",  "Nuyen",  "Ori Money",  "Pesos",  "Pieces of Eight",  "Points",  "Poké",  "Pons",  "Pounds Sterling", 
     "Quatloos",  "Quills",  "Rings",  "Rubies",  "Runes",  "Rupees",  "Sapphires",  "Sheep",  "Shillings",  "Silver",  "Slivers",  "Socks",  "Solari",  "Souls",  "Sovereigns", 
     "Spheres",  "Spirit Bucks",  "Spirit Light",  "Stamps",  "Stonks",  "Strawberries",  "Subs",  "Tickets",  "Tokens",  "Vespine Gas",  "Wheat",  "Widgets",  "Wood",  "XP",
     "Yen",  "Zenny",  "Zloty"};
 
            RandomizerChaosManager.initialize();
            Randomizer.DamageModifier = 1f;
            Randomizer.GridFactor = 4.0;
            Randomizer.Message = "Good luck on your rando!";
            Randomizer.LastMessageCredits = false;
            Randomizer.PlayedGoodLuckOnce = false;
            Randomizer.MessageProvider = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
            RandomizerUI.Instance.ClearRecentNotifications();
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
            Randomizer.HotColdSaveId = 2000;
            Randomizer.OpenMode = true;
            Randomizer.OpenWorld = false;
            RandomizerDataMaps.LoadGladesData();
            RandomizerDataMaps.LoadGinsoData();
            RandomizerDataMaps.LoadForlornData();
            RandomizerDataMaps.LoadHoruData();
            RandomizerDataMaps.LoadValleyData();
            RandomizerColorManager.Initialize();
            RandomizerRebinding.ParseRebinding();
            RandomizerSettings.ParseSettings();
            Randomizer.RelicZoneLookup = new Dictionary<string, string>();
            RandomizerTrackedDataManager.Initialize();
            RandomizerStatsManager.Initialize();
            Randomizer.RelicCount = 0;
            Randomizer.GrenadeZone = "MIA";
            Randomizer.StompZone = "MIA";
            Randomizer.StompTriggers = false;
            Randomizer.GoalModeFinish = false;
            Randomizer.SpawnWith = "";
            Randomizer.IgnoreEnemyExp = false;
            Randomizer.RelicCountOverride = false;
            Randomizer.AllowOrbWarps = false;
            Randomizer.RandomizedFirstEnergy = false;
            Randomizer.NightBerryWarpPosition = new Vector3(-910f, -300f);
            Randomizer.InLogicWarps = false;
            Randomizer.TeleportersLockedByClues = false;
            Randomizer.WarpLogicLocations = new Hashtable();
            Keysanity.Initialize();

            if (Randomizer.SeedFilePath == null)
            {
                Randomizer.SeedFilePath = "randomizer.dat";
            }

            try {
                if(File.Exists(Randomizer.SeedFilePath)) {
                    List<String> allLines = File.ReadAllLines(Randomizer.SeedFilePath).ToList();
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
                        
                        GetDataFromSeedLine(coords, lineParts[1], lineParts[2], lineParts[3]);

                        if (coords == 2)
                        {
                            Randomizer.SpawnWith = lineParts[1] + lineParts[2];
                        }
                        else if (lineParts[1] != "EN")
                        {
                            bool repeatable = lineParts[1] == "RP";
                            RandomizerLocationManager.PlacePickup(coords, lineParts[1], lineParts[2], repeatable);
                        }
                    }
                    Randomizer.HotColdMaps.Sort();
                    Randomizer.HotColdMapsWithFrags.Sort();
                    if (Randomizer.CluesMode) {
                        RandomizerClues.FinishClues();
                    }
                    RandomizerLocationManager.InitializeLogic();
                    if (Characters.Sein) {
                        RandomizerLocationManager.UpdateReachable();
                    }
                } else {
                    Randomizer.printInfo("Error: " + Randomizer.SeedFilePath + " not found");
                    Randomizer.SeedFilePath = "randomizer.dat";
                }
            }
            catch(Exception e) {
                Randomizer.printInfo("Error parsing " + Randomizer.SeedFilePath + ":" + e.Message, 300);
                Randomizer.SeedFilePath = "randomizer.dat";
            }

            RandomizerBonusSkill.Reset();
        } catch(Exception e) {
            Randomizer.log("init: " + e.Message);
        }
    }

    public static void InitializeOnce()
    {
        Game.Events.Scheduler.OnGameSerializeLoad.Add(new Action(Randomizer.OnGameSerializeLoad));

        RandomizerLocationManager.Initialize();
        RandomizerUI.Initialize();
        RandomizerBootstrap.Initialize();
        Inventory = RandomizerInventory.Initialize();
        Keysanity = new RandomizerKeysanity(Inventory);

        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += Randomizer.OnDroppedFiles;

        Randomizer.unseededRandom = new System.Random();
    }

    public static void OnApplicationQuit()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    public static void OnDroppedFiles(List<string> aFiles, B83.Win32.POINT aPos)
    {
        if (aFiles.Count > 1)
        {
            return;
        }

        string filePath = aFiles[0];
        string fileName = filePath.Substring(filePath.LastIndexOf('\\') + 1);
        if (fileName.StartsWith("randomizer") && fileName.EndsWith(".dat"))
        {
            Randomizer.SeedFilePath = aFiles[0];
            Randomizer.initialize();
            Randomizer.showSeedInfo();
        }
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

    public static void TeleportAnywhere()
    {
        if (!Characters.Sein.Controller.CanMove || !Characters.Sein.Active)
            return;
        if (Characters.Sein.IsSuspended || UI.MainMenuVisible)
            return;

        if (TeleporterController.CanTeleport(null))
        {
            string defaultTeleporter = "sunkenGlades";
            float closestTeleporter = Mathf.Infinity;

            bool isInGlades = false;
            bool isInGrotto = false;

            if (Scenes.Manager.CurrentScene.Scene.StartsWith("sunkenGlades"))
            {
                isInGlades = true;
            }
            else if (Scenes.Manager.CurrentScene.Scene.StartsWith("moonGrotto"))
            {
                isInGrotto = true;
            }

            foreach (GameMapTeleporter teleporter in TeleporterController.Instance.Teleporters)
            {
                if (teleporter.Activated)
                {
                    if (isInGlades && teleporter.Identifier == "sunkenGlades")
                    {
                        defaultTeleporter = teleporter.Identifier;
                        break;
                    }
                    else if (isInGrotto && teleporter.Identifier == "moonGrotto")
                    {
                        defaultTeleporter = teleporter.Identifier;
                        break;
                    }

                    Vector3 distanceVector = teleporter.WorldPosition - Characters.Sein.Position;
                    if (distanceVector.sqrMagnitude < closestTeleporter)
                    {
                        defaultTeleporter = teleporter.Identifier;
                        closestTeleporter = distanceVector.sqrMagnitude;
                    }
                }
            }

            TeleporterController.Show(defaultTeleporter);
            Randomizer.IsUsingRandomizerTeleportAnywhere = true;
        }
        else
        {
            Randomizer.printInfo("No #Spirit Wells# have been activated yet!");
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
        Randomizer.LastMessageCredits = false;
        Randomizer.PlayedGoodLuckOnce = false;
        Randomizer.Message = message;

        if (RandomizerSettings.Customization.MultiplePickupMessages)
        {
            RandomizerUI.Instance.QueueSideNotification(message);
        }
        else
        {
            Randomizer.MessageQueue.Enqueue(message);
        }
    }

    public static void showHint(string message, int frames)
    {
        Randomizer.LastMessageCredits = false;
        Randomizer.PlayedGoodLuckOnce = false;
        Randomizer.Message = message;

        if (RandomizerSettings.Customization.MultiplePickupMessages)
        {
            RandomizerUI.Instance.QueueSideNotification(message, (float)frames / 60f + 3f);
        }
        else
        {
            Randomizer.MessageQueue.Enqueue(new object[] {message, frames});
        }
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
        if (LastMessageCredits)
        {
            Randomizer.showCredits(Randomizer.Message, 5);
        }
        else if (Randomizer.Message == "Good luck on your rando!" || Randomizer.Message == "Good luck on your bingo!")
        {
            if (Randomizer.PlayedGoodLuckOnce)
            {
                var split = Randomizer.Message.Substring(0, Randomizer.Message.Length - 1).ToLower().Split(new char[]{' '});
                int n = split.Count();
                while (n > 1)
                {
                    int k = Randomizer.unseededRandom.Next(n--);
                    string value = split[k];
                    split[k] = split[n];
                    split[n] = value;
                }

                split[0] = split[0][0].ToString().ToUpper() + split[0].Substring(1);
                var shuffled = String.Join(" ", split) + "!";
                Randomizer.MessageQueue.Enqueue(shuffled);
            }
            else
            {
                Randomizer.MessageQueue.Enqueue(Randomizer.Message);
            }

            Randomizer.PlayedGoodLuckOnce = true;
        }
        else
        {
            Randomizer.MessageQueue.Enqueue(Randomizer.Message);
        }
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

    public static void hintAndLog(float x, float y)
    {
        string message = ((int)x).ToString() + " " + ((int)y).ToString();
        Randomizer.showHint(message);
        Randomizer.log(message);
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
                Items.NightBerry.transform.position = NightBerryWarpPosition;
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

        if (RandomizerRebinding.ReturnToStart.IsPressed() && Characters.Sein && !SafeIsBashing && Randomizer.Warping <= 0)
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
            if (get(1104)>0)
                Randomizer.returnToStart();
            else
                Randomizer.TeleportAnywhere();
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

        if (RandomizerRebinding.ShowKeysanityProgress.IsPressed()) {
            Keysanity.ShowKeyProgress();
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
            Randomizer.printInfo(message);
    }

    public static void showChaosMessage(string message)
    {
        Randomizer.printInfo(message);
    }

    public static void showProgress()
    {
        try {
            string text = "";
            string g = "";
            if(Randomizer.ForceTrees || Randomizer.CluesMode)
            {
                int trees = RandomizerBonus.SkillTreeProgression();
                g = trees >= 10 ? "$" : "";
                text += $"{g}Trees ({trees}/10){g}  ";
            }    
            if (Randomizer.WorldTour && Characters.Sein) {
                int relics = get(302);
                g = relics >= Randomizer.RelicCount ? "$" : "";
                text += $"{g}Relics ({relics}/{Randomizer.RelicCount}){g}  ";
            }
            int maps = RandomizerBonus.MapStoneProgression();
            g = maps >= 9 ? "$" : "";
            text += $"{g}Maps ({maps}/9){g}  ";
            int pickups = get(1600);
            g = pickups >= 256 ? "$" : "";

            text += $"{g}Total ({pickups}/256){g}\n";
            if (Randomizer.CluesMode)
                text += RandomizerClues.GetClues();
            else if(Randomizer.Shards) {
                if (Keys.GinsoTree)
                    text += $"*WV ({RandomizerBonus.WaterVeinShards()}/3)*  ";
                else
                    text += $"*WV* ({RandomizerBonus.WaterVeinShards()}/3)  ";
                if (Keys.ForlornRuins)
                    text += $"#GS ({RandomizerBonus.GumonSealShards()}/3)#  ";
                else
                    text += $"#GS# ({RandomizerBonus.GumonSealShards()}/3)  ";
                if (Keys.MountHoru)
                    text += $"@SS ({RandomizerBonus.SunstoneShards()}/3)@  ";
                else
                    text += $"@SS@ ({RandomizerBonus.SunstoneShards()}/3)  ";
            } else  // the below is ugly code, but otoh it's also code that will only run for people who are playing clueless shardless seeds, and. :orishrug: they had it coming, or something.
                text += $"*WV{(Keys.GinsoTree ? ": Found*" : "*: ????")}  #GS{(Keys.ForlornRuins ? ": Found#" : "#: ????")}  @SS{(Keys.MountHoru ? ": Found@" : "@: ????")}";
            if (Randomizer.fragsEnabled)
            {
                int frags = RandomizerBonus.WarmthFrags();
                g = frags >= Randomizer.fragKeyFinish ? "$" : "";
                text += $" {g}Frags: ({RandomizerBonus.WarmthFrags()}/{Randomizer.fragKeyFinish}){g}";
            }
            if(RandomizerBonus.ForlornEscapeHint())
            {
                string s = "";
                string g_color = "";
                 if(Characters.Sein) {
                    s = Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Stomp)   ? "$" : "";
                    g = Characters.Sein && Characters.Sein.PlayerAbilities.HasAbility(AbilityType.Grenade) ? "$" : "";
                }
                text += $"\n{s}Stomp: {StompZone}{s}{g}    Grenade: {GrenadeZone}{g}";
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
                Game.UI.Hints.Show(Randomizer.MessageProvider, HintLayer.Randomizer, (float)Randomizer.MessageQueueTime / 30f + 1f);
            }
        }
        Randomizer.MessageQueueTime--;
    }

    public static void OnDeath()
    {
        RandomizerBonusSkill.OnDeath();
        RandomizerStatsManager.OnDeath();

        if (Randomizer.IsUsingRandomizerTeleportAnywhere)
        {
            TeleporterController.Instance.CancelTeleport();
            UI.Menu.HideMenuScreen(false);
        }
    }

    public static void OnGameSerializeLoad()
    {
        Randomizer.ResetTrackerCount = 0;
        if (Scenes.Manager.CurrentScene?.Scene != "titleScreenSwallowsNest")
        {
            RandomizerTrackedDataManager.Reset();
            RandomizerTrackedDataManager.UpdateBitfields();

            RandomizerLocationManager.UpdateReachable();
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
        Randomizer.printInfo("Error using door at " + ((int)position.x).ToString() + ", " + ((int)position.y).ToString());
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
            Game.UI.Hints.Show(Randomizer.MessageProvider, HintLayer.Randomizer, (float)seconds);
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
                    } else if(scene == "forlornRuinsNestC" && get(1105) == 0) {
                            RandomizerBonus.UpgradeID(81);
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
                        RandomizerTrackedDataManager.UpdateBitfields();
                        RandomizerColorManager.UpdateHotColdTarget();
                        if (Characters.Sein.Position.y > 935f && Randomizer.Inventory.FinishedGinsoEscape && Scenes.Manager.CurrentScene.Scene == "ginsoTreeWaterRisingEnd")
                        {
                            if (SafeIsBashing)
                                Characters.Sein.Abilities.Bash.BashGameComplete(0f);
                            Characters.Sein.Position = new Vector3(750f, -120f);
                            return;
                        }
                        if (get(1106) > 0 && Characters.Sein.Position.y > -235f && Scenes.Manager.CurrentScene.Scene == "forlornRuinsResurrection")
                        {
                            if (SafeIsBashing)
                                Characters.Sein.Abilities.Bash.BashGameComplete(0f);
                            Characters.Sein.Position = new Vector3(-1350f, -410f);
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
                            if(gameMapTP.Identifier == "ginsoTree" && get(1024) == 1 && (RandomizerBonus.WaterVeinShards() >= 2 || RandomizerClues.IsClueActive("WV")))
                            {
                                TeleporterController.Activate(Randomizer.TeleportTable["Ginso"].ToString(), false);
                                if (RandomizerSettings.Customization.MultiplePickupMessages)
                                {
                                    RandomizerSwitch.PickupMessage("*Ginso teleporter activated*");
                                }
                                else
                                {
                                    Randomizer.MessageQueue.Enqueue("*Ginso teleporter activated*");
                                }
                            }
                            else if(gameMapTP.Identifier == "forlorn" && get(1025) == 1 && (RandomizerBonus.GumonSealShards() >= 2 || RandomizerClues.IsClueActive("GS")))
                            {
                                TeleporterController.Activate(Randomizer.TeleportTable["Forlorn"].ToString(), false);
                                if (RandomizerSettings.Customization.MultiplePickupMessages)
                                {
                                    RandomizerSwitch.PickupMessage("#Forlorn teleporter activated#");
                                }
                                else
                                {
                                    Randomizer.MessageQueue.Enqueue("#Forlorn teleporter activated#");
                                }
                            }
                            else if(gameMapTP.Identifier == "mountHoru" && get(1026) == 1 && (RandomizerBonus.SunstoneShards() >= 2 || RandomizerClues.IsClueActive("SS")))
                            {
                                TeleporterController.Activate(Randomizer.TeleportTable["Horu"].ToString(), false);
                                if (RandomizerSettings.Customization.MultiplePickupMessages)
                                {
                                    RandomizerSwitch.PickupMessage("@Horu teleporter activated@");
                                }
                                else
                                {
                                    Randomizer.MessageQueue.Enqueue("@Horu teleporter activated@");
                                }
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

    public static bool RepeatableCheck()
    {
        if (Randomizer.RepeatableCooldown <= 0)
        {
            Randomizer.RepeatableCooldown = 2;
            return true;
        }

        return false;
    }

    private static string cct(IEnumerable<char> cs) => new String(cs.ToArray());

    public static bool ParseFlags(string seed, string[] rawFlags) {
        bool doBingo = false;

        foreach (string rawFlag in rawFlags) {
            string flag = rawFlag.ToLower();

            if (flag == "ohko")
                Randomizer.OHKO = true;

            if (flag == "race") 
                SeedMeta = $"{String.Join(",", rawFlags.Select(f => f.ToLower().StartsWith("sync") ? "Sync" + cct(f.Skip(f.IndexOf('.') - 1)) : f).ToArray())}|{cct(seed.Skip(1).SkipWhile(c => Char.IsLower(c)))}"; // yeah that's right we're LINQ wizards baby anyways this is just a bit of censorship to make things a bit harder for people trying to cheat

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
                doBingo = true;

            if (flag == "noextraexp")
                Randomizer.IgnoreEnemyExp = true;

            if (flag == "0xp") {
                Randomizer.IgnoreEnemyExp = true;
                Randomizer.ZeroXP = true;                
            }

            if (flag == "nobonus")
                Randomizer.BonusActive = false;

            if (flag == "nonprogressivemapstones")
                Randomizer.ProgressiveMapStones = false;

            if (flag == "forcetrees")
                Randomizer.ForceTrees = true;

            if (flag == "forcemaps")
                Randomizer.ForceMaps = true;

            if (flag == "clues")
                Randomizer.CluesMode = true;

            if (flag == "shards")
                Randomizer.Shards = true;

            if (flag == "entrance")
                Randomizer.Entrance = true;

            if (flag == "closeddungeons")
                Randomizer.OpenMode = false;

            if (flag == "openworld")
                Randomizer.OpenWorld = true;

            if (flag.StartsWith("hotcold="))
            {
                Randomizer.HotCold = true;
                Randomizer.HotColdTypes = new HashSet<string>(rawFlag.Substring(8).Split(new char[]{'+'}).ToList<string>());
            }
            if (flag.StartsWith("sense="))
                Randomizer.HotColdTypes = new HashSet<string>(rawFlag.Substring(6).Split(new char[]{'+'}).ToList<string>());

            if (flag == "noaltr")
                Randomizer.AltRDisabled = true;

            if (flag == "stomptriggers")
                Randomizer.StompTriggers = true;

            if (flag == "goalmodefinish")
                Randomizer.GoalModeFinish = true;

            if (flag == "orbwarp")
                Randomizer.AllowOrbWarps = true;

            if (flag == "randomizedfirstenergy")
                Randomizer.RandomizedFirstEnergy = true;

            if (flag == "inlogicwarps")
                Randomizer.InLogicWarps = true;

            if (flag == "cluelockedtps")
                Randomizer.TeleportersLockedByClues = true;
            
            if (flag == "keysanity")
                Keysanity.IsActive = true;

        }
        return doBingo;
    }

    public static void GetSenseFromSeedLine(int coords, string code, string id, string area)
    {
        // Prepare sense information, but not for pickups at spawn, and we only need at most 1 addition for each coordinate.
        if (coords == 2)
        {
            return;
        }
        if (Randomizer.HotColdTypes.Contains(code) || Randomizer.HotColdTypes.Any((string t) => (code + id).StartsWith(t)))
        {
            if (Math.Abs(coords) > 100)
            {
                if (!Randomizer.HotColdItems.ContainsKey(coords))
                {
                    Randomizer.HotColdItems.Add(coords, new RandomizerHotColdItem(Randomizer.HashKeyToVector(coords), Randomizer.HotColdSaveId));
                    Randomizer.HotColdSaveId++;
                }
            } else {
                if (!Randomizer.HotColdMaps.Contains(coords))
                {
                    Randomizer.HotColdMaps.Add(coords);
                }
                if (!Randomizer.HotColdMapsWithFrags.Contains(coords))
                {
                    Randomizer.HotColdMapsWithFrags.Add(coords);
                }
            }
        }
        else if (code == "MS")
        {
            if (Math.Abs(coords) > 100)
            {
                if (!Randomizer.HotColdFrags.ContainsKey(coords))
                {
                    Randomizer.HotColdFrags.Add(coords, new RandomizerHotColdItem(Randomizer.HashKeyToVector(coords), Randomizer.HotColdSaveId));
                    Randomizer.HotColdSaveId++;
                }
            } else {
                if (!Randomizer.HotColdMapsWithFrags.Contains(coords))
                { 
                    Randomizer.HotColdMapsWithFrags.Add(coords);
                }
            }
        }
    }

    public static void GetDataFromSeedLine(int coords, string code, string id, string area)
    {
        int id_number;
        int.TryParse(id, out id_number);
        // If we are processing a repeatable or multipickup recur over items in them.
        if (code == "RP" || code == "MU")
        {
            // Check the full pickup code + id for sense. This is for sense=MUEC cases. Otherwise processed in the recursion.
            GetSenseFromSeedLine(coords, code, id, area);
            string[] pieces = id.Split('/');
            for (int i = 0; i < pieces.Length; i += 2)
            {
                GetDataFromSeedLine(coords, pieces[i], pieces[i + 1], area);
            }
            return;
        }

        GetSenseFromSeedLine(coords, code, id, area);
        
        if (code == "WT")
        {
            Randomizer.RelicZoneLookup[id] = area;
            if (!Randomizer.RelicCountOverride)
            {
                Randomizer.RelicCount++;
            }
        }
        if (code == "EN")
        {
            // door entries are coord|EN|targetX|targetY
            int doorY;
            int.TryParse(area, out doorY);
            Randomizer.DoorTable[coords] = new Vector3((float)id_number, (float)doorY);
        }
        if (code == "SK")
        {
            if (id_number == 51)
            {
                Randomizer.GrenadeZone = area;
            }
            else if (id_number == 4)
            {
                Randomizer.StompZone = area;
            }
        }
        if (Randomizer.CluesMode && code == "EV" && id_number % 2 == 0)
        {
            RandomizerClues.AddClue(area, id_number / 2);
        }

        if (Keysanity.IsActive && code == "RB") {
            Keysanity.AddClue(id_number, coords, area);
        }

        if (code == "TW")
        {
            //6399872|TW|Warp to Spirit Cavern AC,-219,-176,SpiritCavernsACWarp|Swamp
            string[] Pieces = id.Split(new char[]
            {
                ','
            });
            if (Pieces.Length > 3)
            {
                Randomizer.WarpLogicLocations.Add(Pieces[0], Pieces[3]);
            }
        }
    }

    private static int get(int item) { return Characters.Sein.Inventory.GetRandomizerItem(item); }
    private static int set(int item, int value) { return Characters.Sein.Inventory.SetRandomizerItem(item, value); }
    private static HashSet<int> knownUnknowns = new HashSet<int>() {-1, 2, -1640264 }; // remove -1640264 once appropriate seedgen changes happen ig?

    public static bool SeenCoord(int coord) {
        if(!RandomizerTrackedDataManager.CoordsMap.ContainsKey(coord))
        {
            if(!knownUnknowns.Contains(coord))
                Randomizer.LogError("Unknown coord: " + coord.ToString());
            return false;
        }
        int locID = 1560 + RandomizerTrackedDataManager.CoordsMap[coord]/32;
        return  0 != (get(locID) >> (RandomizerTrackedDataManager.CoordsMap[coord]%32)) % 2;
    }

    public static bool HaveCoord(int coord) {
        if(!RandomizerTrackedDataManager.CoordsMap.ContainsKey(coord))
        {
            if(!knownUnknowns.Contains(coord))
                Randomizer.LogError("Unknown coord: " + coord.ToString());
            return false;
        }
        int locID = 930 + RandomizerTrackedDataManager.CoordsMap[coord]/32;
        return  0 != (get(locID) >> (RandomizerTrackedDataManager.CoordsMap[coord]%32)) % 2;
    }

    public static void OnCoord(int coord) {
        if(!RandomizerTrackedDataManager.CoordsMap.ContainsKey(coord))
        {
            if(!knownUnknowns.Contains(coord))
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
        if (!RandomizerSettings.DevSettings.BlackrootOrbRoomClimbAssist)
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
        Randomizer.Inventory.Clear();
        TeleporterController.RemoveCustomTeleporters();
        int spawnHCs = 0;
        int spawnECs = 0;
        // start everyone with 1 energy on all difficulties if "RandomizedFirstEnergy" flag set
        if (Randomizer.RandomizedFirstEnergy)
            spawnECs += 1;

        // relaxed difficulty players start with +1 health and +1 energy, plus the first ability in each tree
        if (DifficultyController.Instance.Difficulty == DifficultyMode.Easy)
        {
            spawnHCs += 1;
            spawnECs += 1;
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
        if (Randomizer.SpawnWith != "") {
            RandomizerAction spawnItem;

            // horrible idea thanks
            spawnItem = new RandomizerAction(SpawnWith.Substring(0, 2), SpawnWith.Substring(2));

            var spawnItems = spawnItem.Decompose();
            // is it stupid to do it this way? yes. does it technically cover the edge case where your spawn item has HC/1/HC/1/HC/-1? also yes
            // does it make HC|4 valid but literally only on spawn? haha don't even worry about that my friends
            spawnHCs += spawnItems.Where(item => item.Action == "HC").Select(item => (int)item.Value).Aggregate(0, (acc, next) => acc+next);
            spawnECs += spawnItems.Where(item => item.Action == "EC").Select(item => (int)item.Value).Aggregate(0, (acc, next) => acc+next);
            // let the survivors regroup
            spawnItems = spawnItems.Where(item => item.Action != "HC" && item.Action != "EC").ToList();
            if(spawnItems.Count == 1) 
                RandomizerSwitch.GivePickup(spawnItems[0], 2, true);
            else if(spawnItems.Count > 1)
               RandomizerSwitch.GivePickup(RandomizerAction.AsMulti(spawnItems), 2, true);
        }
        Characters.Sein.Energy.Max += spawnECs;
        Characters.Sein.Mortality.Health.MaxHealth += 4*spawnHCs;
        Characters.Sein.Mortality.Health.SetAmount(Characters.Sein.Mortality.Health.MaxHealth);
        Characters.Sein.Energy.SetCurrent(Characters.Sein.Energy.Max);
        RandomizerLocationManager.UpdateReachable();
    }

    public static string ExpName(int p) {
            if(RandomizerSettings.Customization.RandomizedExpNames) 
                return RandomExpNames[new System.Random(31 * Randomizer.SeedMeta.GetHashCode() + p).Next(RandomExpNames.Count)];
            return "Experience";
    }

    public static bool SafeIsBashing {get => (Characters.Sein.Abilities.Bash && Characters.Sein.Abilities.Bash.IsBashing) || false; }

    public static RandomizerInventory Inventory { get; private set; }
    public static RandomizerKeysanity Keysanity { get; private set; }

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

    public static List<String> RandomExpNames;

    public static string GrenadeZone;
    // welcome to the...
    public static string StompZone;

    public static bool CreditsActive;

    public static float CachedVolume;

    public static int ResetVolume;

    public static bool LastMessageCredits;

    public static bool sacrificeStarted;

    public static bool AltRDisabled;

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

    public static string SeedFilePath;

    public static Vector3 NightBerryWarpPosition;

    public static int HotColdSaveId;

    public static bool InLogicWarps;

    public static Hashtable WarpLogicLocations;

    public static bool TeleportersLockedByClues;

    public static bool PlayedGoodLuckOnce;

    private static System.Random unseededRandom;
}
