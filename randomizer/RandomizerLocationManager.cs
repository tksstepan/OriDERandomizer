using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Game;
using Protogen;
using Sein.World;
using UnityEngine;
using System.Threading;
public class RandomizerLocationManager
{
	public static void Initialize()
	{
		StringReader reader = new StringReader(RandomizerLocationData.All);
		string line = reader.ReadLine();		
		while (line != null)
		{
			Location newLocation = new Location(line.Trim());
			RandomizerLocationManager.LocationsByName[newLocation.Name] = newLocation;
			RandomizerLocationManager.LocationsByKey[newLocation.Key] = newLocation;

			if (newLocation.Type == Location.LocationType.ProgressiveMap)
			{
				RandomizerLocationManager.ProgressiveMapLocations[newLocation.Difficulty] = newLocation;
			}
			else
			{
				RandomizerLocationManager.LocationsByGuid[newLocation.MoonGuid] = newLocation;
				RandomizerLocationManager.LocationsByWorldMapGuid[newLocation.WorldMapGuid] = newLocation;
			}
			line = reader.ReadLine();
		}
		if(!HaveDownloadedAreas && (DLThread == null)) {
			DLThread = new Thread(DownloadAreas);
			DLThread.Start();
		}

	}

	public static void InitializeLogic()
	{
		if(!HaveDownloadedAreas) return; //Areas thread hasn't returned yet, skip. It'll run this on its own on completion.
		
		HashSet<string> paths = new HashSet<string>();
		int firstComma = Randomizer.SeedMeta.IndexOf(',');
		string preset = Randomizer.SeedMeta.Substring(0, firstComma);

		if (preset.StartsWith("Sync"))
		{
			preset = Randomizer.SeedMeta.Substring(firstComma + 1, Randomizer.SeedMeta.IndexOf(',', firstComma + 1) - firstComma - 1);
		}

		switch(preset)
		{
		case "Casual":
			paths.Add("casual-core");
			paths.Add("casual-dboost");
			break;
		case "Standard":
			paths.Add("casual-core");
			paths.Add("casual-dboost");
			paths.Add("standard-core");
			paths.Add("standard-dboost");
			paths.Add("standard-lure");
			paths.Add("standard-abilities");
			break;
		case "Expert":
			paths.Add("casual-core");
			paths.Add("casual-dboost");
			paths.Add("standard-core");
			paths.Add("standard-dboost");
			paths.Add("standard-lure");
			paths.Add("standard-abilities");
			paths.Add("expert-core");
			paths.Add("expert-dboost");
			paths.Add("expert-lure");
			paths.Add("expert-abilities");
			paths.Add("dbash");
			break;
		case "Master":
			paths.Add("casual-core");
			paths.Add("casual-dboost");
			paths.Add("standard-core");
			paths.Add("standard-dboost");
			paths.Add("standard-lure");
			paths.Add("standard-abilities");
			paths.Add("expert-core");
			paths.Add("expert-dboost");
			paths.Add("expert-lure");
			paths.Add("expert-abilities");
			paths.Add("dbash");
			paths.Add("master-core");
			paths.Add("master-dboost");
			paths.Add("master-lure");
			paths.Add("master-abilities");
			paths.Add("gjump");		
			break;
		default:
			if (preset.StartsWith("Custom"))
			{
				int pathMask = 0;
				if (int.TryParse(preset.Remove(0, "Custom".Length), out pathMask))
				{
					HashSet<string> newPaths = OriParse.PathMaskToPathSet(pathMask);
					if (newPaths != null)
					{
						//Randomizer.log("Got custom pathset: " + OriParse.PathMaskToString(pathMask));
						paths = newPaths;
					}
				}
			}
			paths.Add("casual-core");
			break;
		}


		if (!File.Exists("areas.ori"))
		{
			RandomizerLocationManager.Areas = null;
			RandomizerLocationManager.s_logicLastUpdated = DateTime.MinValue;
			RandomizerLocationManager.s_lastLogicPaths = paths;
			Randomizer.log("No areas.ori found, will not update logic.");
			RandomizerSettings.CurrentFilter = RandomizerSettings.MapFilterMode.All;
			return;
		}

		spawnNodeName = null;
		if(Randomizer.SpawnWith.Contains("WS")){
			foreach(var kvp in stupidBullshit) {
				if(Randomizer.SpawnWith.EndsWith(kvp.Key)){
					spawnNodeName = kvp.Value;
					break;
				}
			}
		}

		if (RandomizerLocationManager.s_logicLastUpdated == DateTime.MinValue || File.GetLastWriteTime("areas.ori") > RandomizerLocationManager.s_logicLastUpdated || !paths.SetEquals(RandomizerLocationManager.s_lastLogicPaths))
		{
			RandomizerLocationManager.Areas = OriParse.Parse("areas.ori", paths);
			RandomizerLocationManager.s_logicLastUpdated = File.GetLastWriteTime("areas.ori");
			RandomizerLocationManager.s_lastLogicPaths = paths;

			foreach (Location location in RandomizerLocationManager.LocationsByName.Values)
			{
				location.Reachable = false;
			}
		}		
	}

	public static RandomizerPickupAction AddPickupAction(GameObject parentObj, string pickupName, string actionName = null)
	{
		if (!RandomizerLocationManager.LocationsByName.ContainsKey(pickupName))
		{
			return null;
		}

		GameObject obj = new GameObject((actionName != null) ? actionName : "pickupAction");
		obj.transform.parent = parentObj.transform;

		RandomizerPickupAction pickupAction = obj.AddComponent<RandomizerPickupAction>();
		pickupAction.MoonGuid = new MoonGuid(RandomizerLocationManager.LocationsByName[pickupName].MoonGuid);
		pickupAction.LocationName = pickupName;
		return pickupAction;
	}

	public static void PlacePickup(int key, string action, object value, bool repeatable = false)
	{
		if (!RandomizerLocationManager.LocationsByKey.ContainsKey(key))
		{
			Randomizer.printInfo("Error: Unknown location key " + key + " in seed file " + Randomizer.SeedFilePath);
			return;
		}

		Location pickupLocation = RandomizerLocationManager.LocationsByKey[key];
		pickupLocation.Pickup = new RandomizerAction(action, value);
		pickupLocation.Repeatable = repeatable;
	}

	public static bool IsPickupCollected(MoonGuid pickupGuid)
	{
		if (RandomizerLocationManager.LocationsByGuid.ContainsKey(pickupGuid))
		{
			return RandomizerLocationManager.LocationsByGuid[pickupGuid].Collected;
		}

		return false;
	}

	public static bool HasPickupBeenTouched(MoonGuid pickupGuid)
	{
		if (RandomizerLocationManager.LocationsByGuid.ContainsKey(pickupGuid))
		{
			return RandomizerLocationManager.LocationsByGuid[pickupGuid].Touched;
		}
		return false;
	}

	public static bool IsPickupRepeatable(MoonGuid pickupGuid)
	{
		if (RandomizerLocationManager.LocationsByGuid.ContainsKey(pickupGuid))
		{
			return RandomizerLocationManager.LocationsByGuid[pickupGuid].Repeatable;
		}

		return false;
	}

	public static void GivePickup(MoonGuid pickupGuid)
	{
		if (RandomizerLocationManager.LocationsByGuid.ContainsKey(pickupGuid))
		{
			RandomizerLocationManager.LocationsByGuid[pickupGuid].Give();
		}
	}

	public static void GivePickupByWorldMapGuid(MoonGuid pickupMapGuid)
	{
		if (RandomizerLocationManager.LocationsByWorldMapGuid.ContainsKey(pickupMapGuid))
		{
			RandomizerLocationManager.LocationsByWorldMapGuid[pickupMapGuid].Give();
		}
	}

	public static void UpdateReachable()
	{
		if(LogicThread != null && LogicThread.IsAlive) {
			LogicThread.Abort();
			Randomizer.log("Killing existing logic thread");
		}
		LogicThread = new Thread(UpdateReachableWorker);
		LogicThread.Start();
		
	}

	public static void DownloadAreas() {
		var webClient = new WebClient();
		try {
			if(File.Exists("areas.ori")) File.Move("areas.ori", "areas.ori.old"); // backup
			ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
			webClient.DownloadFile(AreasURL, "areas.ori");
			if(File.Exists("areas.ori.old")) File.Delete("areas.ori.old"); // clean backup
		} catch(Exception e) {
			Randomizer.LogError($"Failed to download areas.ori: ${e}");
			if(File.Exists("areas.ori")) File.Delete("areas.ori");  					 // remove broken / failed
			if(File.Exists("areas.ori.old")) File.Move("areas.ori.old", "areas.ori");    // restore backup
		}
		HaveDownloadedAreas = true;
		InitializeLogic();
	}

	public static void UpdateReachableWorker() {
			Inventory currentInventory = Inventory.FromCharacter();
			currentInventory.Unlocks.Add("Mapstone");

			if (Characters.Sein.Inventory.GetRandomizerItem(70) > currentInventory.Keystones)
			{
				currentInventory.Keystones = Characters.Sein.Inventory.GetRandomizerItem(70);
			}

			if (Characters.Sein.Inventory.GetRandomizerItem(71) > currentInventory.Mapstones)
			{
				currentInventory.Mapstones = Characters.Sein.Inventory.GetRandomizerItem(71);
			}

			if (Randomizer.OpenMode)
			{
				currentInventory.Unlocks.Add("Open");
			}

			if (Randomizer.OpenWorld)
			{
				currentInventory.Unlocks.Add("OpenWorld");
			}

            if (Randomizer.InLogicWarps)
			{
				currentInventory.Unlocks.Add("InLogicWarps");
			}
			
			HashSet<string> reachable = null;

			if (RandomizerLocationManager.Areas != null)
			{
				reachable = OriReachable.Reachable(RandomizerLocationManager.Areas, currentInventory, spawnNodeName);
				if(reachable.Contains("FronkeyFight")) { // hacky hack hack
					reachable.Add("FirstEnergyCell");
					reachable.Add("Sein");
				}

				if (reachable.Contains("ForlornEscape"))
					reachable.Add("ForlornEscapePlant");
				foreach (var item in RandomizerLocationManager.LocationsByName) 
					item.Value.Reachable = reachable.Contains(item.Key);
/* can toggle this on for debugging but logging in a thread is spoopy and the conditionals are more work than overwriting bools
				{
					if (reachable.Contains(item.Key))
					{
						item.Value.Reachable = true;
					}
					else if (item.Value.Reachable)
					{
						Randomizer.log("!!!! " + item.Key + " became unreachable!"); // can toggle this on for debugging but logging in a thread is spoopy
						item.Value.Reachable = false;
					}
				}*/
			}
		
	}

	public static Dictionary<MoonGuid, Location> LocationsByGuid = new Dictionary<MoonGuid, Location>();

	public static Dictionary<MoonGuid, Location> LocationsByWorldMapGuid = new Dictionary<MoonGuid, Location>();

	public static Dictionary<string, Location> LocationsByName = new Dictionary<string, Location>();

	public static Dictionary<int, Location> LocationsByKey = new Dictionary<int, Location>();

	public static Location[] ProgressiveMapLocations = new Location[9];

	public static AreaGraph Areas;

	public static Thread LogicThread;

	public static Thread DLThread;

	public static bool HaveDownloadedAreas = false;

	public static string AreasURL = "http://orirandov3.appspot.com/netcode/areas";

	private static DateTime s_logicLastUpdated = DateTime.MinValue;

	private static HashSet<string> s_lastLogicPaths = null;

	private static Dictionary<string, string> stupidBullshit = new Dictionary<string, string>() {
		{"-159,-114,force", "SpiritTreeRefined"},
		{"491,-73,force", "SwampTeleporter"},
		{"519,-174,force",  "MoonGrotto"},
		{"-914,-298,force", "ForlornTeleporter"},
		{"-430,0,force",  "ValleyTeleporter"},
		{"88,142,force",   "HoruTeleporter"},
		{"570,539,force", "GinsoTeleporter"},
		{"-594,496,force",  "SorrowTeleporter"},
		{"381,-297,force",  "BlackrootGrottoConnection"}
	};

	private static string spawnNodeName = null;


	public class Location
	{
		public Location(string locationData)
		{
			string[] parts = locationData.Split();
			this.Name = parts[0];
			this.FriendlyName = Regex.Replace(this.Name, "([A-Z0-9]+)", " $1") + "\n" + parts[5];
			this.Position = new Vector2(float.Parse(parts[1]), float.Parse(parts[2]));
			this.Type = (LocationType)Enum.Parse(typeof(LocationType), parts[3]);
			this.Difficulty = int.Parse(parts[4]);
			this.Zone = parts[5];

			this.MoonGuid = new MoonGuid(int.Parse(parts[6]), int.Parse(parts[7]), int.Parse(parts[8]), int.Parse(parts[9]));

			if (parts.Length >= 14)
			{
				this.WorldMapGuid = new MoonGuid(int.Parse(parts[10]), int.Parse(parts[11]), int.Parse(parts[12]), int.Parse(parts[13]));
			}
			else
			{
				this.WorldMapGuid = this.MoonGuid;
			}

			if (this.Type == LocationType.Skill || this.Type == LocationType.Map)
			{
				this.SpecialIndex = int.Parse(parts[parts.Length - 1]);
			}
		}

		public void Give()
		{
			// special case for Sein pickup because it doesn't technically have a valid location key
			if (this.Type == LocationType.Skill && this.SpecialIndex == 0)
			{
				RandomizerTrackedDataManager.SetTree(0);
				Characters.Sein.PlayerAbilities.SetAbility(AbilityType.SpiritFlame, true);
				TeleporterController.Activate("sunkenGlades");
				return;
			}

			if (this.Collected)
			{
				return;
			}

			switch (this.Type)
			{
			case LocationType.Map:
				RandomizerTrackedDataManager.SetMapstone(this.SpecialIndex);
				break;
			case LocationType.Skill:
				RandomizerTrackedDataManager.SetTree(this.SpecialIndex);
				break;
			default:
				break;
			}

			if (this.Type == LocationType.Map && Randomizer.ProgressiveMapStones)
			{
				RandomizerLocationManager.ProgressiveMapLocations[RandomizerBonus.MapStoneProgression()].Give();
				return;
			}

			if (Randomizer.ColorShift)
			{
				Randomizer.changeColor();
			}

			if (this.Type == LocationType.ProgressiveMap)
			{
				RandomizerBonus.CollectMapstone();
				RandomizerStatsManager.FoundMapstone();
			}
			else
			{
				RandomizerStatsManager.IncPickup(this.Key);
			}

			BingoController.OnLoc(this.Key);
			if(this.Key == -7320236) {
				Characters.Sein.Inventory.SetRandomizerItem(1106, 1);
			}
			RandomizerSwitch.GivePickup(this.Pickup, this.Key);
			RandomizerLocationManager.UpdateReachable();

			if (Randomizer.HotColdItems.ContainsKey(this.Key))
			{
				Characters.Sein.Inventory.SetRandomizerItem(Randomizer.HotColdItems[this.Key].Id, 1);
				RandomizerColorManager.UpdateHotColdTarget();
			}
			else if (Randomizer.HotColdFrags.ContainsKey(this.Key))
			{
				Characters.Sein.Inventory.SetRandomizerItem(Randomizer.HotColdFrags[this.Key].Id, 1);
				RandomizerColorManager.UpdateHotColdTarget();
			}

			if (this.Type == LocationType.Skill)
			{
				Randomizer.showProgress();
			}
		}

		public int Key => (int)(Mathf.Floor((float)((int)this.Position.x) / 4f) * 4f) * 10000 + (int)(Mathf.Floor((float)((int)this.Position.y) / 4f) * 4f);

		public bool Collected => this.Repeatable ? false : (this.Type == LocationType.Map ? RandomizerTrackedDataManager.GetMapstone(this.SpecialIndex) : Randomizer.HaveCoord(this.Key));

		public bool Touched => Collected || this.Repeatable && Randomizer.HaveCoord(this.Key);

		public MoonGuid MoonGuid;

		public string Name;

		public string FriendlyName;

		public Vector2 Position;

		public LocationType Type;

		public int Difficulty;

		public string Zone;

		public MoonGuid WorldMapGuid;

		public bool HasWorldMapGuid;

		public RandomizerAction Pickup;

		public bool Repeatable;

		public int SpecialIndex;

		public bool Reachable;

		public enum LocationType
		{
			ExpSmall,
			ExpMedium,
			ExpLarge,
			HealthCell,
			EnergyCell,
			AbilityCell,
			Keystone,
			Mapstone,
			Skill,
			Plant,
			Map,
			Event,
			Cutscene,
			ProgressiveMap
		}
	}
}
