using System;
using System.Collections.Generic;
using Game;
using UnityEngine;
using Sein.World;

public static class RandomizerTrackedDataManager
{
	public static void Initialize()
	{
		RandomizerTrackedDataManager.TreeBitfield = -559038737;
		Trees = new Dictionary<int, string>() {
			{0, "Spirit Flame"},
			{1, "Wall Jump"},
			{2, "Charge Flame"},
			{3, "Double Jump"},
			{4, "Bash"},
			{5, "Stomp"},
			{6, "Glide"},
			{7, "Climb"},
			{8, "Charge Jump"},
			{9, "Grenade"},
			{10, "Dash"},
		};
		
		Zones = new Dictionary<int, string>() {			
			{0, "Glades"},
			{1, "Grove"},
			{2, "Grotto"},
			{3, "Blackroot"},
			{4, "Swamp"},
			{5, "Ginso"},
			{6, "Valley"},
			{7, "Misty"},
			{8, "Forlorn"},
			{9, "Sorrow"},
			{10, "Horu"},
		};

		RelicFound = new Dictionary<string, int>() {			
			{"Glades", 0},
			{"Grove", 1},
			{"Grotto", 2},
			{"Blackroot", 3},
			{"Swamp", 4},
			{"Ginso", 5},
			{"Valley", 6},
			{"Misty", 7},
			{"Forlorn", 8},
			{"Sorrow", 9},
			{"Horu", 10},
		};

		RelicExists = new Dictionary<string, int>() {
			{"Glades", 11},
			{"Grove", 12},
			{"Grotto", 13},
			{"Blackroot", 14},
			{"Swamp", 15},
			{"Ginso", 16},
			{"Valley", 17},
			{"Misty", 18},
			{"Forlorn", 19},
			{"Sorrow", 20},
			{"Horu", 21},
		};

		Pedistals = new Dictionary<string, MapstoneData>() {
			{"sunkenGlades", new MapstoneData("Glades", 0)},
			{"mangrove", new MapstoneData("Blackroot", 1)},
			{"hollowGrove", new MapstoneData("Grove", 2)},
			{"moonGrotto", new MapstoneData("Grotto", 3)},
			{"thornfeltSwamp", new MapstoneData("Swamp", 4)},
			{"valleyOfTheWind", new MapstoneData("Valley", 5)},
			{"forlornRuins", new MapstoneData("Forlorn", 6)},
			{"sorrowPass", new MapstoneData("Sorrow", 7)},
			{"mountHoru", new MapstoneData("Horu", 8)},
		};

		Teleporters = new Dictionary<string, int>() {
			{"Grove", 0},
			{"Swamp", 1},
			{"Grotto", 2},
			{"Valley", 3},
			{"Forlorn", 4},
			{"Sorrow", 5},
			{"Ginso", 6},
			{"Horu", 7},
			{"Blackroot", 8},
			{"Glades", 9},
		};

		Skills = new Dictionary<int, AbilityType>() {
			{11, AbilityType.SpiritFlame},
			{12, AbilityType.WallJump},
			{13, AbilityType.ChargeFlame},
			{14, AbilityType.DoubleJump},
			{15, AbilityType.Bash},
			{16, AbilityType.Stomp},
			{17, AbilityType.Glide},
			{18, AbilityType.Climb},
			{19, AbilityType.ChargeJump},
			{20, AbilityType.Grenade},
			{21, AbilityType.Dash},
		};
	}

	public static void UpdateBitfields() {
		if(Characters.Sein)
		{
			TreeBitfield = Characters.Sein.Inventory.GetRandomizerItem(1001) + GetSkillBitfield();
			RelicBitfield = Characters.Sein.Inventory.GetRandomizerItem(1002) + GetRelicExistsBitfield();
			MapstoneBitfield = Characters.Sein.Inventory.GetRandomizerItem(1003) + (RandomizerBonus.WarmthFrags() << 9);
			TeleporterBitfield = GetTeleporters() + (Randomizer.fragKeyFinish << 10);
			KeyEventBitfield = GetKeyEvents();
		}
	}
	public static void Reset() {
			TreeBitfield = 0;
			RelicBitfield = 0;
			MapstoneBitfield = 0;
			TeleporterBitfield = 0;
			KeyEventBitfield = 0;
	}

	public static int GetRelicExistsBitfield() {
		int bf = 0;
		foreach(string zone in Randomizer.RelicZoneLookup.Values) {
			bf += (1 << RelicExists[zone]);
		}
		return bf;
	}

	public static int GetKeyEvents() {
		int bf = 0;
		int wvShards = RandomizerBonus.WaterVeinShards();
		int gsShards = RandomizerBonus.GumonSealShards();
		int ssShards = RandomizerBonus.SunstoneShards();
		if(wvShards > 0)
			bf += 1 << 0;
		if(wvShards > 1)
			bf += 1 << 1;
		if(Keys.GinsoTree)
			bf += 1 << 2;
		if(gsShards > 0)
			bf += 1 << 3;
		if(gsShards > 1)
			bf += 1 << 4;
		if(Keys.ForlornRuins)
			bf += 1 << 5;
		if(ssShards > 0)
			bf += 1 << 6;
		if(ssShards > 1)
			bf += 1 << 7;
		if(Keys.MountHoru)
			bf += 1 << 8;
		if(Sein.World.Events.WaterPurified)
			bf += 1 << 9;
		if(Sein.World.Events.WindRestored)
			bf += 1 << 10;
		if(Randomizer.ForceTrees)
			bf += 1 << 11;
		if(Randomizer.Shards)
			bf += 1 << 12;
		if(Randomizer.fragsEnabled)
			bf += 1 << 13;
		if(Randomizer.WorldTour)
			bf += 1 << 14;
		return bf;
	}

	public static int GetTeleporters() {
		int bf = 0;
		List<string> unlockedTPids = new List<string>();
		foreach (GameMapTeleporter gameMapTP in TeleporterController.Instance.Teleporters)
		{
			if(gameMapTP.Activated)
				unlockedTPids.Add(gameMapTP.Identifier);
		}
		foreach(KeyValuePair<string, int> tp in Teleporters) {
			if(unlockedTPids.Contains(Randomizer.TeleportTable[tp.Key].ToString())) {
				bf += 1 << tp.Value;
			}
		}
		return bf;
	}

	public static void ListTeleporters() {
		UpdateBitfields();
		List<string> owned = new List<string>();
		List<string> unowned = new List<string>();
		foreach(KeyValuePair<string, int> tp in Teleporters) {
			if((TeleporterBitfield >> tp.Value) % 2 == 1) {
				owned.Add(tp.Key);
			} else {
				unowned.Add(tp.Key);
			}
		}
		string output = "TPs active: " + string.Join(", ", owned.ToArray());
		if(unowned.Count > 0)
			output += "\nremaining: " + string.Join(", ", unowned.ToArray());
		Randomizer.printInfo(output);
	}

	public static void ListTrees() {
		UpdateBitfields();
		List<string> owned = new List<string>();
		List<string> unowned = new List<string>();
		foreach(KeyValuePair<int, string> tree in Trees) {
			if(tree.Key == 0) {
				continue;
			}
			if((TreeBitfield >> tree.Key) % 2 == 1) {
				owned.Add(tree.Value);
			} else {
				unowned.Add(tree.Value);
			}
		}
		string output = "Trees active: " + string.Join(", ", owned.ToArray());
		if(unowned.Count > 0)
			output += "\nremaining: " + string.Join(", ", unowned.ToArray());
		Randomizer.printInfo(output);
	}

	public static void ListRelics() {
		UpdateBitfields();
		List<string> owned = new List<string>();
		List<string> unowned = new List<string>();
		List<string> no_relics = new List<string>();

		foreach(KeyValuePair<string, int> relic in RelicFound) {
			if(Randomizer.RelicZoneLookup.ContainsValue(relic.Key))
			{
				if((RelicBitfield >> relic.Value) % 2 == 1) {
					owned.Add(relic.Key);
				} else {
					unowned.Add(relic.Key);
				}
			} else {
				no_relics.Add(relic.Key);
			}
		}
		string output = "Relics collected: " + string.Join(", ", owned.ToArray());
		if(unowned.Count > 0)
			output += "\nremaining: " + string.Join(", ", unowned.ToArray());
		if(no_relics.Count > 0)
			output += "\nrelicless: " + string.Join(", ", no_relics.ToArray());
		Randomizer.printInfo(output);
	}

	public static void ListMapstones() {
		UpdateBitfields();
		List<string> owned = new List<string>();
		List<string> unowned = new List<string>();
		foreach(MapstoneData data in Pedistals.Values) {
			if((MapstoneBitfield >> data.Bit) % 2 == 1) {
				owned.Add(data.Zone);
			} else {
				unowned.Add(data.Zone);
			}
		}
		string output = "Maps active: " + string.Join(", ", owned.ToArray());
		if(unowned.Count > 0)
			output += "\nremaining: " + string.Join(", ", unowned.ToArray());
		Randomizer.printInfo(output);
	}


	public static bool SetTree(int treeNum) {
		if(!GetTree(treeNum)) {
			TreeBitfield = Characters.Sein.Inventory.IncRandomizerItem(1001, 1 << treeNum);
			if(treeNum != 0)
				Characters.Sein.Inventory.IncRandomizerItem(27, 1);
			return true;
		}
		return false;
	}

	public static bool GetTree(int treeNum) {
		return (TreeBitfield >> treeNum) % 2 == 1;
	}

	public static bool SetRelic(string zone) {
		if(!GetRelic(zone)) {
            Characters.Sein.Inventory.IncRandomizerItem(302, 1);
			RelicBitfield = Characters.Sein.Inventory.IncRandomizerItem(1002, 1 << RelicFound[zone]);
			return true;
		}
		return false;
	}

	public static bool GetRelic(string zone) {
		return (RelicBitfield >> RelicFound[zone]) % 2 == 1;
	}

	public static void SetMapstone(string areaIdentifier) {
		try {
			MapstoneData data = Pedistals[areaIdentifier];
			if(!GetMapstone(data)) {
				MapstoneBitfield = Characters.Sein.Inventory.IncRandomizerItem(1003, (1 << data.Bit));
			} 
		} 
		catch(Exception e) {
			Randomizer.LogError("@SetMapstone:@ area " + areaIdentifier + ": " + e.Message);
		}
	}

	public static bool GetMapstone(MapstoneData data) {
		return (MapstoneBitfield >> data.Bit) % 2 == 1;
	}

	public static int GetSkillBitfield() {
		int bf = 0;
		foreach(KeyValuePair<int, AbilityType> kvp in Skills) {
			if(Characters.Sein.PlayerAbilities.HasAbility(kvp.Value))
				bf += (1 << kvp.Key);
		}
		return bf;
	}

	public static int TreeBitfield;
	public static int MapstoneBitfield;
	public static int TeleporterBitfield;
	public static int RelicBitfield;
	public static int KeyEventBitfield;

	// Token: 0x040032E2 RID: 13026
	public static Dictionary<int, string> Trees;

	public static Dictionary<int, string> Zones;

	public static Dictionary<string, int> RelicFound;

	public static Dictionary<string, int> RelicExists;

	public static Dictionary<string, int> Teleporters;

	public static Dictionary<int, AbilityType> Skills;

	public static Dictionary<string, MapstoneData> Pedistals;
	// Token: 0x02000A1B RID: 2587
	public class MapstoneData
	{
		// Token: 0x06003815 RID: 14357 RVA: 0x0002C10D File Offset: 0x0002A30D
		public MapstoneData(string zone, int bit)
		{
			this.Bit = bit;
			this.Zone = zone;
		}

		public int Bit;
		public string Zone;
	}
}
