using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

// Token: 0x02000A1A RID: 2586
public static class RandomizerPlantManager
{
	// Token: 0x06003819 RID: 14361
	public static void Initialize()
	{
		RandomizerPlantManager.Plants = new Dictionary<MoonGuid, RandomizerPlantManager.PlantData>();
		RandomizerPlantManager.Plants.Add(new MoonGuid(-1556827621, 1266247965, 658524593, -439112014), new RandomizerPlantManager.PlantData(0, new Vector3(313.3f, -231.6f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(1357098119, 1185246384, -60723813, -1846269103), new RandomizerPlantManager.PlantData(1, new Vector3(43.9f, -156.1f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(1515223554, 1193340384, -1596868467, 697952739), new RandomizerPlantManager.PlantData(2, new Vector3(330.5f, -77f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-1886220547, 1283851600, 332946051, -743667011), new RandomizerPlantManager.PlantData(3, new Vector3(365f, -118.7f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-1689898933, 1299319593, 113649851, 1261499278), new RandomizerPlantManager.PlantData(4, new Vector3(342.2f, -178.5f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(456723677, 1250158164, 601204362, -996068525), new RandomizerPlantManager.PlantData(5, new Vector3(124.5f, 21.1f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-163073312, 1290407986, 1056024991, 258406927), new RandomizerPlantManager.PlantData(6, new Vector3(435.6f, -139.5f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-1758061565, 1316907676, -776224081, -125897729), new RandomizerPlantManager.PlantData(7, new Vector3(537.9f, -176.2f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(2004147296, 1137671468, -1569331061, -1629975829), new RandomizerPlantManager.PlantData(8, new Vector3(541f, -220.9f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(509607018, 1143884117, -863808114, -1366570643), new RandomizerPlantManager.PlantData(9, new Vector3(439.6f, -344.9f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-227868995, 1177742190, -644734542, 1909369139), new RandomizerPlantManager.PlantData(10, new Vector3(447.7f, -367.7f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-1427834574, 1228643039, 258065063, 192959857), new RandomizerPlantManager.PlantData(11, new Vector3(493f, -400.8f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(132560558, 1120862650, 766732468, -1275181277), new RandomizerPlantManager.PlantData(12, new Vector3(515.1f, -100.5f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(560810798, 1177388095, 1561676448, -1886145880), new RandomizerPlantManager.PlantData(13, new Vector3(628.4f, -119.5f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-782504693, 1227994259, -1940970872, -746412143), new RandomizerPlantManager.PlantData(14, new Vector3(540.7f, 101.1f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(9391235, 1178635865, 1621941659, -1071529561), new RandomizerPlantManager.PlantData(15, new Vector3(610.7f, 611.6f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(1034685702, 1181486759, -2087364183, 1444765681), new RandomizerPlantManager.PlantData(16, new Vector3(-179.9f, -88.1f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(1410428307, 1137579496, 21711539, 297702887), new RandomizerPlantManager.PlantData(17, new Vector3(-468.2f, -67.5f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(1639481262, 1084101684, 2037171352, 1939769945), new RandomizerPlantManager.PlantData(18, new Vector3(-814.6f, -265.7f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-1834790605, 1271225833, 899401347, -1089496419), new RandomizerPlantManager.PlantData(19, new Vector3(-606.7f, -313.9f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-1954742180, 1119052110, -509724515, 1760957283), new RandomizerPlantManager.PlantData(20, new Vector3(-629.3f, 249.6f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-124617321, 1079866299, 2108253884, -1797343447), new RandomizerPlantManager.PlantData(21, new Vector3(-477.1f, 586f)));
		RandomizerPlantManager.Plants.Add(new MoonGuid(-1672031488, 1215719569, 1495684759, -746944328), new RandomizerPlantManager.PlantData(22, new Vector3(318.5f, 245.6f)));
	}

	// Token: 0x0600381A RID: 14362
	public static bool Display(int id)
	{
		return (Characters.Sein.Inventory.GetRandomizerItem(1000) >> id) % 2 == 0;
	}

	// Token: 0x06003837 RID: 14391
	public static bool Display(MoonGuid guid)
	{
		return RandomizerPlantManager.Display(RandomizerPlantManager.Plants[guid].Id);
	}

	// Token: 0x0600383C RID: 14396
	public static void DestroyPlant(MoonGuid guid)
	{
		if (!RandomizerPlantManager.Plants.ContainsKey(guid))
		{
			return;
		}
		RandomizerPlantManager.PlantData plantData = RandomizerPlantManager.Plants[guid];
		if (Display(plantData.Id))
		{
			Characters.Sein.Inventory.IncRandomizerItem(1000, 1 << plantData.Id);
		}
	}

	// Token: 0x04003310 RID: 13072
	public static Dictionary<MoonGuid, RandomizerPlantManager.PlantData> Plants;

	// Token: 0x02000A1B RID: 2587
	public class PlantData
	{
		// Token: 0x0600382C RID: 14380
		public PlantData(int id, Vector3 position)
		{
			this.Id = id;
			this.Position = position;
		}

		// Token: 0x04003323 RID: 13091
		public Vector3 Position;

		// Token: 0x04003324 RID: 13092
		public int Id;
	}
}
