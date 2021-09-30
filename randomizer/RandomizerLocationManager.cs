using System;
using System.Collections.Generic;
using System.IO;
using Game;
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

	public static Dictionary<MoonGuid, Location> LocationsByGuid = new Dictionary<MoonGuid, Location>();

	public static Dictionary<MoonGuid, Location> LocationsByWorldMapGuid = new Dictionary<MoonGuid, Location>();

	public static Dictionary<string, Location> LocationsByName = new Dictionary<string, Location>();

	public static Dictionary<int, Location> LocationsByKey = new Dictionary<int, Location>();

	public static Location[] ProgressiveMapLocations = new Location[9];

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
				if (this.SpecialIndex == 0)
				{
					Characters.Sein.PlayerAbilities.SetAbility(AbilityType.SpiritFlame, true);
					TeleporterController.Activate("sunkenGlades");
					return;
				}
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