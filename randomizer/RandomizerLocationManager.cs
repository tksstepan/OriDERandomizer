using System;
using System.Collections.Generic;
using System.IO;
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
			RandomizerLocationManager.LocationsByGuid[newLocation.MoonGuid] = newLocation;
			RandomizerLocationManager.LocationsByName[newLocation.Name] = newLocation;
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

	public static void GivePickup(MoonGuid pickupGuid)
	{
		if (RandomizerLocationManager.LocationsByGuid.ContainsKey(pickupGuid))
		{
			Location pickupLocation = RandomizerLocationManager.LocationsByGuid[pickupGuid];
			RandomizerSwitch.GivePickup(Randomizer.Table[pickupLocation.Key], pickupLocation.Key, true);
		}
	}

	public static Dictionary<MoonGuid, Location> LocationsByGuid = new Dictionary<MoonGuid, Location>();

	public static Dictionary<string, Location> LocationsByName = new Dictionary<string, Location>();

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

			if (parts.Length > 10)
			{
				this.WorldMapGuid = new MoonGuid(int.Parse(parts[10]), int.Parse(parts[11]), int.Parse(parts[12]), int.Parse(parts[13]));
			}
		}

		public MoonGuid MoonGuid;

		public string Name;

		public Vector2 Position;

		public LocationType Type;

		public int Difficulty;

		public string Zone;

		public MoonGuid WorldMapGuid;

		public int Key => (int)(Mathf.Floor(Position.x / 4f) * 4f) * 10000 + (int)(Mathf.Floor(Position.y / 4f) * 4f);

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
			Cutscene
		}
	}
}