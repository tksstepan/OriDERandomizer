using System;
using System.Collections.Generic;
using System.IO;
using Game;
using Protogen;
using Sein.World;
using UnityEngine;

public class RandomizerLocationManager
{
	public static void Initialize()
	{
		StringReader reader = new StringReader(RandomizerLocationData.All);
		string line;		
		while (true)
		{
			line = reader.ReadLine();
			if (line == null)
			{
				break;
			}

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
		}
	}

	public static void InitializeLogic()
	{
		HashSet<string> paths = new HashSet<string>();
		int firstComma = Randomizer.SeedMeta.IndexOf(',');
		string preset = Randomizer.SeedMeta.Substring(0, firstComma);

		if (preset.StartsWith("Sync"))
		{
			preset = Randomizer.SeedMeta.Substring(firstComma, Randomizer.SeedMeta.IndexOf(',', firstComma));
		}

		switch(preset)
		{
		case "Casual":
			paths.Add("casual");
			break;
		case "Standard":
			paths.Add("casual");
			paths.Add("standard");
			break;
		case "Expert":
			paths.Add("casual");
			paths.Add("standard");
			paths.Add("expert");
			paths.Add("dbash");
			break;
		case "Master":
			paths.Add("casual");
			paths.Add("standard");
			paths.Add("expert");
			paths.Add("dbash");
			paths.Add("master");
			paths.Add("gjump");
			break;
		}

		if (!File.Exists("areas.ori"))
		{
			RandomizerLocationManager.Areas = null;
			RandomizerLocationManager.s_logicLastUpdated = DateTime.MinValue;
			RandomizerLocationManager.s_lastLogicPaths = paths;
			return;
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

		HashSet<string> reachable = null;

		if (RandomizerLocationManager.Areas != null)
		{
			reachable = OriReachable.Reachable(RandomizerLocationManager.Areas, currentInventory);
			reachable.Add("FirstEnergyCell");
			reachable.Add("Sein");

			if (reachable.Contains("ForlornEscape"))
			{
				reachable.Add("ForlornEscapePlant");
			}
		}

		foreach (var item in RandomizerLocationManager.LocationsByName)
		{
			if (reachable == null || reachable.Contains(item.Key))
			{
				item.Value.Reachable = true;
			}
			else if (item.Value.Reachable)
			{
				Randomizer.log("!!!! " + item.Key + " became unreachable!");
				item.Value.Reachable = false;
			}
		}
	}

	public static Dictionary<MoonGuid, Location> LocationsByGuid = new Dictionary<MoonGuid, Location>();

	public static Dictionary<MoonGuid, Location> LocationsByWorldMapGuid = new Dictionary<MoonGuid, Location>();

	public static Dictionary<string, Location> LocationsByName = new Dictionary<string, Location>();

	public static Dictionary<int, Location> LocationsByKey = new Dictionary<int, Location>();

	public static Location[] ProgressiveMapLocations = new Location[9];

	public static AreaGraph Areas;

	private static DateTime s_logicLastUpdated = DateTime.MinValue;

	private static HashSet<string> s_lastLogicPaths = null;

	public class Location
	{
		public Location(string locationData)
		{
			string[] parts = locationData.Split();
			this.Name = parts[0];
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
			case LocationType.Plant:
				RandomizerPlantManager.DestroyPlant(this.MoonGuid);
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

		public MoonGuid MoonGuid;

		public string Name;

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