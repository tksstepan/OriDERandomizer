using UnityEngine;

public class RandomizerWorldMapIconManager
{
    public static readonly RandomizerWorldMapIcon[] Icons = 
    {
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.WaterVein, new Vector3(503.9977f, -246.8492f),  new MoonGuid(-311986442, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.CleanWater, new Vector3(524.7007f, 573.2695f),  new MoonGuid(-311986443, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.WindRestored, new Vector3(-733.6296f, -229.0052f),  new MoonGuid(-311986444, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Sunstone, new Vector3(-558.3355f, 604.2133f),  new MoonGuid(-311986445, 1339883091, -1367161930, -1482660984)),

        // L1 -> L4
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-92.54841f, 379.0250f),  new MoonGuid(-311986446, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-17.79449f, 277.3809f),  new MoonGuid(-311986447, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-156.1515f, 351.3048f),  new MoonGuid(-311986448, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-94.61309f, 156.4479f),  new MoonGuid(-311986449, 1339883091, -1367161930, -1482660984)),

        // R1 -> R4
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(251.9695f, 383.9832f),  new MoonGuid(-311986452, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(171.4578f, 292.5522f),  new MoonGuid(-311986462, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(310.0000f, 286.8892f),  new MoonGuid(-311986472, 1339883091, -1367161930, -1482660984)),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(207.0726f, 196.6679f),  new MoonGuid(-311986482, 1339883091, -1367161930, -1482660984))
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
