using UnityEngine;

public class RandomizerWorldMapIconManager
{
    public static readonly RandomizerWorldMapIcon[] Icons = 
    {
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.WaterVein, new Vector3(503.9977f, -246.8492f),  new MoonGuid(-311986442, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.CleanWater, new Vector3(524.7007f, 573.2695f),  new MoonGuid(-311986443, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.WindRestored, new Vector3(-733.6296f, -229.0052f),  new MoonGuid(-311986444, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Sunstone, new Vector3(-558.3355f, 604.2133f),  new MoonGuid(1358934909, 1223903071, 482024081, 2044569179)),

        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.SkillTree, new Vector3(-162.4078f, -257.7189f), new MoonGuid(-550456551, 1312223365, -251340902, -293109681)), // Sein
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.SkillTree, new Vector3(-456.1564f, -13.787f), new MoonGuid(2007719075, 1284864229, 701022080, -877064706)), // Glide

        // L1 -> L4
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-92.54841f, 379.0250f),  new MoonGuid(-311986446, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-17.79449f, 277.3809f),  new MoonGuid(-311986447, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-156.1515f, 351.3048f),  new MoonGuid(-311986448, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-94.61309f, 156.4479f),  new MoonGuid(-311986449, 1339883091, -1367161930, -1482660984)),

        // R1 -> R4
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(251.9695f, 383.9832f),  new MoonGuid(-311986452, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(171.4578f, 292.5522f),  new MoonGuid(-311986462, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(310.0000f, 286.8892f),  new MoonGuid(-311986472, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(207.0726f, 196.6679f),  new MoonGuid(-311986482, 1339883091, -1367161930, -1482660984)),

        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(313.3f, -231.6f), new MoonGuid(-1556827621, 1266247965, 658524593, -439112014)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(43.9f, -156.1f), new MoonGuid(1357098119, 1185246384, -60723813, -1846269103)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(330.5f, -77f), new MoonGuid(1515223554, 1193340384, -1596868467, 697952739)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(365f, -118.7f), new MoonGuid(-1886220547, 1283851600, 332946051, -743667011)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(342.2f, -178.5f), new MoonGuid(-1689898933, 1299319593, 113649851, 1261499278)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(124.5f, 21.1f), new MoonGuid(456723677, 1250158164, 601204362, -996068525)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(435.6f, -139.5f), new MoonGuid(-163073312, 1290407986, 1056024991, 258406927)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(537.9f, -176.2f), new MoonGuid(-1758061565, 1316907676, -776224081, -125897729)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(541f, -220.9f), new MoonGuid(2004147296, 1137671468, -1569331061, -1629975829)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(439.6f, -344.9f), new MoonGuid(509607018, 1143884117, -863808114, -1366570643)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(447.7f, -367.7f), new MoonGuid(-227868995, 1177742190, -644734542, 1909369139)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(493f, -400.8f), new MoonGuid(-1427834574, 1228643039, 258065063, 192959857)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(515.1f, -100.5f), new MoonGuid(132560558, 1120862650, 766732468, -1275181277)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(628.4f, -119.5f), new MoonGuid(560810798, 1177388095, 1561676448, -1886145880)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(540.7f, 101.1f), new MoonGuid(-782504693, 1227994259, -1940970872, -746412143)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(610.7f, 611.6f), new MoonGuid(9391235, 1178635865, 1621941659, -1071529561)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-179.9f, -88.1f), new MoonGuid(1034685702, 1181486759, -2087364183, 1444765681)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-468.2f, -67.5f), new MoonGuid(1410428307, 1137579496, 21711539, 297702887)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-814.6f, -265.7f), new MoonGuid(1639481262, 1084101684, 2037171352, 1939769945)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-606.7f, -313.9f), new MoonGuid(-1834790605, 1271225833, 899401347, -1089496419)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-629.3f, 249.6f), new MoonGuid(-1954742180, 1119052110, -509724515, 1760957283)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-477.1f, 586f), new MoonGuid(-124617321, 1079866299, 2108253884, -1797343447)),
		new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(318.5f, 245.6f), new MoonGuid(-1672031488, 1215719569, 1495684759, -746944328))

    };

    public class RandomizerWorldMapIcon
    {
        public MoonGuid Guid;
        public RandomizerWorldMapIconType Type;
        public Vector3 Position;
        public Vector3 Scale = new Vector3(4, 4, 1);

        public RandomizerWorldMapIcon(RandomizerWorldMapIconType type, Vector3 position, MoonGuid guid)
        {
            Guid = guid;
            Type = type;
            Position = position;
        }
    }
}
