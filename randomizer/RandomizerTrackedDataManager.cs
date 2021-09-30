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
		try {
			UpdateBitfields();
			var owned = new List<string>();
			var unowned = new List<string>();
			var touched = new List<string>();
			foreach (KeyValuePair<string, int> pair in MapBitsByArea) {
				if (GetMapstone(pair.Value)) {
					owned.Add(MapZonesByBit[pair.Value]);
				} else {
					if (BingoController.Active && BingoController.TouchedMapstone(pair.Key))
						touched.Add(MapZonesByBit[pair.Value]);
					else
						unowned.Add(MapZonesByBit[pair.Value]);
				}
			}
			string output = "Maps active: " + string.Join(", ", owned.ToArray());
			if(touched.Count > 0)
			{
				output += "\ntouched: " + string.Join(", ",touched.ToArray());
			}
			if(unowned.Count > 0)
				output += "\nremaining: " + string.Join(", ", unowned.ToArray());
			Randomizer.printInfo(output);			
        } catch(Exception e) {
            Randomizer.LogError("ListMapstones: " + e.Message);
        }
	}

	public static bool SetTree(int treeNum) {
		if(!GetTree(treeNum)) {
			TreeBitfield = Characters.Sein.Inventory.IncRandomizerItem(1001, 1 << treeNum);
			if(treeNum != 0)
			{
				BingoController.OnTree(treeNum);
				Characters.Sein.Inventory.IncRandomizerItem(27, 1);
			}
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
			SetMapstone(MapBitsByArea[areaIdentifier]);
		} 
		catch(Exception e) {
			Randomizer.LogError("@SetMapstone:@ area " + areaIdentifier + ": " + e.Message);
		}
	}

	public static void SetMapstone(int mapNum) {
		if (!GetMapstone(mapNum)) {
			MapstoneBitfield = Characters.Sein.Inventory.IncRandomizerItem(1003, (1 << mapNum));
		}
	}

	public static bool GetMapstone(string areaIdentifier) {
		try {
			return GetMapstone(MapBitsByArea[areaIdentifier]);
		} 
		catch(Exception e) {
			Randomizer.LogError("@GetMapstone:@ area " + areaIdentifier + ": " + e.Message);
			return false;
		}
	}

	public static bool GetMapstone(int mapNum) {
		return (MapstoneBitfield >> mapNum) % 2 == 1;
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

	public static Dictionary<int, string> Trees = new Dictionary<int, string>() {
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
		
	public static Dictionary<int, string> Zones = new Dictionary<int, string>() {			
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

	public static Dictionary<string, int> RelicFound = new Dictionary<string, int>() {			
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

	public static Dictionary<string, int> RelicExists = new Dictionary<string, int>() {
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

	public static Dictionary<string, int> MapBitsByArea = new Dictionary<string, int>() {
			{"sunkenGlades", 0},
			{"mangrove", 1},
			{"hollowGrove", 2},
			{"moonGrotto", 3},
			{"thornfeltSwamp", 4},
			{"valleyOfTheWind", 5},
			{"forlornRuins", 6},
			{"sorrowPass", 7},
			{"mountHoru", 8},
		};

	public static Dictionary<int, string> MapZonesByBit = new Dictionary<int, string>() {
			{0, "Glades"},
			{1, "Blackroot"},
			{2, "Grove"},
			{3, "Grotto"},
			{4, "Swamp"},
			{5, "Valley"},
			{6, "Forlorn"},
			{7, "Sorrow"},
			{8, "Horu"},
		};

	public static Dictionary<string, int> Teleporters = new Dictionary<string, int>() {
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

	public static Dictionary<int, AbilityType>  Skills = new Dictionary<int, AbilityType>() {
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

	public static Dictionary<int, int> CoordsMap = new Dictionary<int, int>() {
		{ -10120036, 0 }, { -10440008, 1 }, { -10759968, 2 }, { -10760004, 3 }, { -10839992, 4 }, { -11040068, 5 }, { -11880100, 6 }, { -120208, 7 }, { -12320248, 8 }, { -1560188, 9 }, { -1560272, 10 }, { -160096, 11 }, { -1639664, 12 }, { -1680104, 13 }, { -1680140, 14 }, { -1800088, 15 }, { -1800156, 16 }, { -1840196, 17 }, { -1840228, 18 }, { -1919808, 19 }, { -199724, 20 }, { -2080116, 21 }, { -2160176, 22 }, { -2200148, 23 }, { -2200184, 24 }, { -2240084, 25 }, { -2399488, 26 }, { -2400212, 27 }, { -2480208, 28 }, { -2480280, 29 }, { -280256, 30 }, { -2840236, 31 }, 
		{ -2919980, 32 }, { -3160308, 33 }, { -319852, 34 }, { -3200164, 35 }, { -3360288, 36 }, { -3520100, 37 }, { -3559936, 38 }, { -3600088, 39 }, { -400240, 40 }, { -4159572, 41 }, { -4160080, 42 }, { -4199936, 43 }, { -4359680, 44 }, { -4440152, 45 }, { -4559584, 46 }, { -4600020, 47 }, { -4600188, 48 }, { -4600256, 49 }, { -4680068, 50 }, { -4799416, 51 }, { -480168, 52 }, { -4879680, 53 }, { -5039728, 54 }, { -5119796, 55 }, { -5159576, 56 }, { -5159700, 57 }, { -5160280, 58 }, { -5400104, 59 }, { -5400236, 60 }, { -5479592, 61 }, { -5479948, 62 }, { -5599400, 63 },
		{ -560160, 64 }, { -5640092, 65 }, { -5719844, 66 }, { -5919556, 67 }, { -5959772, 68 }, { -600244, 69 }, { -6039640, 70 }, { -6079672, 71 }, { -6080316, 72 }, { -6119656, 73 }, { -6119704, 74 }, { -6159632, 75 }, { -6279608, 76 }, { -6280316, 77 }, { -6319752, 78 }, { -6479528, 79 }, { -6719712, 80 }, { -6720040, 81 }, { -6799732, 82 }, { -6800032, 83 }, { -6959592, 84 }, { -7040392, 85 }, { -7200024, 86 }, { -7320236, 87 }, { -7680144, 88 }, { -7960144, 89 }, { -800192, 90 }, { -8160268, 91 }, { -8240012, 92 }, { -8400124, 93 }, { -8440352, 94 }, { -8600356, 95 },
		{ -8720256, 96 }, { -8880252, 97 }, { -8920328, 98 }, { -9120036, 99 }, { -919624, 100 }, { -959848, 101 }, { -9799980, 102 }, { 1040112, 103 }, { 120164, 104 }, { 1240020, 105 }, { 1280164, 106 }, { 1479880, 107 }, { 1480360, 108 }, { 1519708, 109 }, { 1599920, 110 }, { 1600136, 111 }, { 1719892, 112 }, { 1720000, 113 }, { 1720288, 114 }, { 1759964, 115 }, { 1799708, 116 }, { 1839836, 117 }, { 1880164, 118 }, { 1920384, 119 }, { 1959768, 120 }, { 2079568, 121 }, { 2160192, 122 }, { 2239640, 123 }, { 24, 124 }, { 2480400, 125 }, { 2519668, 126 }, { 2520192, 127 }, 
		{ 2559800, 128 }, { 2599880, 129 }, { 2640380, 130 }, { 2719900, 131 }, { 2759624, 132 }, { 28, 133 }, { 2919744, 134 }, { 2999808, 135 }, { 2999904, 136 }, { 3039472, 137 }, { 3039696, 138 }, { 3040304, 139 }, { 3119768, 140 }, { 3160244, 141 }, { 3199820, 142 }, { 32, 143 }, { 3279644, 144 }, { 3279920, 145 }, { 3319936, 146 }, { 3359580, 147 }, { 3359784, 148 }, { 3399820, 149 }, { 3439744, 150 }, { 3519820, 151 }, { 3559792, 152 }, { 36, 153 }, { 3639880, 154 }, { 3639888, 155 }, { 3879576, 156 }, { 3919624, 157 }, { 3919688, 158 }, { 3959588, 159 }, 
		{ 39756, 160 }, { 39804, 161 }, { 399844, 162 }, { 40, 163 }, { 4039612, 164 }, { 4079964, 165 }, { 4199724, 166 }, { 4199828, 167 }, { 4239780, 168 }, { 4319676, 169 }, { 4319860, 170 }, { 4319892, 171 }, { 4359656, 172 }, { 44, 173 }, { 4439632, 174 }, { 4479568, 175 }, { 4479704, 176 }, { 4479832, 177 }, { 4559492, 178 }, { 4560564, 179 }, { 4599508, 180 }, { 4639628, 181 }, { 4680612, 182 }, { 4759860, 183 }, { 48, 184 }, { 4919600, 185 }, { 4959628, 186 }, { 4999752, 187 }, { 4999892, 188 }, { 5039560, 189 }, { 5040476, 190 }, { 5080304, 191 }, 
		{ 5080496, 192 }, { 5119556, 193 }, { 5119584, 194 }, { 5119900, 195 }, { 5160336, 196 }, { 5160384, 197 }, { 5160864, 198 }, { 52, 199 }, { 5200140, 200 }, { 5239456, 201 }, { 5280264, 202 }, { 5280296, 203 }, { 5280404, 204 }, { 5280500, 205 }, { 5320328, 206 }, { 5320488, 207 }, { 5320660, 208 }, { 5320824, 209 }, { 5359824, 210 }, { 5360432, 211 }, { 5360732, 212 }, { 5399780, 213 }, { 5399808, 214 }, { 5400100, 215 }, { 5400276, 216 }, { 5439640, 217 }, { 5480952, 218 }, { 5519856, 219 }, { 559720, 220 }, { 56, 221 }, { 5639752, 222 }, { 5719620, 223 }, 
		{ 5799932, 224 }, { 5879616, 225 }, { 5919864, 226 }, { 599844, 227 }, { 6080608, 228 }, { 6159900, 229 }, { 6199596, 230 }, { 6279880, 231 }, { 6359836, 232 }, { 639888, 233 }, { 6399872, 234 }, { 6639952, 235 }, { 6839792, 236 }, { 6999916, 237 }, { 719620, 238 }, { 7199904, 239 }, { 7559600, 240 }, { 7599824, 241 }, { 7639816, 242 }, { 7679852, 243 }, { 7839588, 244 }, { 7959788, 245 }, { 799776, 246 }, { 799804, 247 }, { 8599904, 248 }, { 8719856, 249 }, { 8839900, 250 }, { 9119928, 251 }, { 919772, 252 }, { 919908, 253 }, { 959960, 254 }, { 960128, 255 }, 
	}; 
}
