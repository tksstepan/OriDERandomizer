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
		Trees = new Dictionary<int, string>();
		Trees.Add(0, "Spirit Flame");
		Trees.Add(1, "Wall Jump");
		Trees.Add(2, "Charge Flame");
		Trees.Add(3, "Double Jump");
		Trees.Add(4, "Bash");
		Trees.Add(5, "Stomp");
		Trees.Add(6, "Glide");
		Trees.Add(7, "Climb");
		Trees.Add(8, "Charge Jump");
		Trees.Add(9, "Grenade");
		Trees.Add(10, "Dash");

		RelicFound = new Dictionary<string, int>();
		RelicFound.Add("Glades", 0);
		RelicFound.Add("Grove", 1);
		RelicFound.Add("Grotto", 2);
		RelicFound.Add("Blackroot", 3);
		RelicFound.Add("Swamp", 4);
		RelicFound.Add("Ginso", 5);
		RelicFound.Add("Valley", 6);
		RelicFound.Add("Misty", 7);
		RelicFound.Add("Forlorn", 8);
		RelicFound.Add("Sorrow", 9);
		RelicFound.Add("Horu", 10);

		RelicExists = new Dictionary<string, int>();
		RelicExists.Add("Glades", 11);
		RelicExists.Add("Grove", 12);
		RelicExists.Add("Grotto", 13);
		RelicExists.Add("Blackroot", 14);
		RelicExists.Add("Swamp", 15);
		RelicExists.Add("Ginso", 16);
		RelicExists.Add("Valley", 17);
		RelicExists.Add("Misty", 18);
		RelicExists.Add("Forlorn", 19);
		RelicExists.Add("Sorrow", 20);
		RelicExists.Add("Horu", 21);


		Pedistals = new Dictionary<string, MapstoneData>();
		Pedistals.Add("sunkenGlades", new MapstoneData("Glades", 0));
		Pedistals.Add("mangrove", new MapstoneData("Blackroot", 1));
		Pedistals.Add("hollowGrove", new MapstoneData("Grove", 2));
		Pedistals.Add("moonGrotto", new MapstoneData("Grotto", 3));
		Pedistals.Add("thornfeltSwamp", new MapstoneData("Swamp", 4));
		Pedistals.Add("valleyOfTheWind", new MapstoneData("Valley", 5));
		Pedistals.Add("forlornRuins", new MapstoneData("Forlorn", 6));
		Pedistals.Add("sorrowPass", new MapstoneData("Sorrow", 7));
		Pedistals.Add("mountHoru", new MapstoneData("Horu", 8));

		Teleporters = new Dictionary<string, int>();

		Teleporters.Add("Grove", 0);
		Teleporters.Add("Swamp", 1);
		Teleporters.Add("Grotto", 2);
		Teleporters.Add("Valley", 3);
		Teleporters.Add("Forlorn", 4);
		Teleporters.Add("Sorrow", 5);
		Teleporters.Add("Ginso", 6);
		Teleporters.Add("Horu", 7);
		Teleporters.Add("Blackroot", 8);
		Teleporters.Add("Glades", 9);

		Skills = new Dictionary<int, AbilityType>();

		Skills.Add(11, AbilityType.SpiritFlame);
		Skills.Add(12, AbilityType.WallJump);
		Skills.Add(13, AbilityType.ChargeFlame);
		Skills.Add(14, AbilityType.DoubleJump);
		Skills.Add(15, AbilityType.Bash);
		Skills.Add(16, AbilityType.Stomp);
		Skills.Add(17, AbilityType.Glide);
		Skills.Add(18, AbilityType.Climb);
		Skills.Add(19, AbilityType.ChargeJump);
		Skills.Add(20, AbilityType.Grenade);
		Skills.Add(21, AbilityType.Dash);
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
		string ownedLine   = "TPs active: " + string.Join(", ", owned.ToArray());
		string unownedLine = "remaining: " + string.Join(", ", unowned.ToArray());
		Randomizer.printInfo(ownedLine + "\n" + unownedLine);
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
		string ownedLine = "Trees active: " + string.Join(", ", owned.ToArray());
		string unownedLine = "remaining: " + string.Join(", ", unowned.ToArray());
		Randomizer.printInfo(ownedLine + "\n" + unownedLine);
	}

	public static void ListRelics() {
		UpdateBitfields();
		List<string> owned = new List<string>();
		List<string> unowned = new List<string>();

		foreach(KeyValuePair<string, int> relic in RelicFound) {
			if(Randomizer.RelicZoneLookup.ContainsValue(relic.Key))
				if((RelicBitfield >> relic.Value) % 2 == 1) {
					owned.Add(relic.Key);
				} else {
					unowned.Add(relic.Key);
				}
		}
		string ownedLine = "Relics collected: " + string.Join(", ", owned.ToArray());
		string unownedLine = "remaining: " + string.Join(", ", unowned.ToArray());
		Randomizer.printInfo(ownedLine + "\n" + unownedLine);
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
		string ownedLine = "Maps active: " + string.Join(", ", owned.ToArray());
		string unownedLine = "remaining: " + string.Join(", ", unowned.ToArray());
		Randomizer.printInfo(ownedLine + "\n" + unownedLine);
	}


	public static void SetTree(int treeNum) {
		if(!GetTree(treeNum)) {
			TreeBitfield = Characters.Sein.Inventory.IncRandomizerItem(1001, 1 << treeNum);
		}
	}

	public static bool GetTree(int treeNum) {
		return (TreeBitfield >> treeNum) % 2 == 1;
	}

	public static void SetRelic(string zone) {
		if(!GetRelic(zone)) {
			RelicBitfield = Characters.Sein.Inventory.IncRandomizerItem(1002, 1 << RelicFound[zone]);
		}
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
