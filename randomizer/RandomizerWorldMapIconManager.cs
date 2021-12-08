using UnityEngine;

public class RandomizerWorldMapIconManager
{
    public static readonly RandomizerWorldMapIcon[] Icons =
    {
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.WaterVein, new Vector3(503.9977f, -246.8492f), "WaterVein"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.CleanWater, new Vector3(524.7007f, 573.2695f), "GinsoEscapeExit"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.WindRestored, new Vector3(-733.6296f, -229.0052f), "ForlornEscape"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Sunstone, new Vector3(-558.3355f, 604.2133f),  "Sunstone"),

        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.SkillTree, new Vector3(-162.4078f, -257.7189f), "Sein"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.SkillTree, new Vector3(-456.1564f, -13.787f), "GlideSkillFeather"),

        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-92.54841f, 379.0250f), "HoruL1"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-17.79449f, 277.3809f), "HoruL2"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-156.1515f, 351.3048f), "HoruL3"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(-94.61309f, 156.4479f), "HoruL4"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(251.9695f, 383.9832f), "HoruR1"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(171.4578f, 292.5522f), "HoruR2"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(310.0000f, 286.8892f), "HoruR3"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.HoruRoom, new Vector3(207.0726f, 196.6679f), "HoruR4"),

        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(313.3f, -231.6f), "DashAreaPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(43.9f, -156.1f), "ChargeFlameAreaPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(330.5f, -77f), "HollowGroveTreePlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(365f, -118.7f), "HollowGroveMapPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(342.2f, -178.5f), "DeathGauntletRoofPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(124.5f, 21.1f), "HoruFieldsPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(435.6f, -139.5f), "MoonGrottoStompPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(537.9f, -176.2f), "GrottoSwampDrainAccessPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(541f, -220.9f), "BelowGrottoTeleporterPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(439.6f, -344.9f), "LeftGumoHideoutUpperPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(447.7f, -367.7f), "LeftGumoHideoutLowerPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(493f, -400.8f), "GumoHideoutRedirectPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(515.1f, -100.5f), "OuterSwampMortarPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(628.4f, -119.5f), "SwampEntrancePlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(540.7f, 101.1f), "LowerGinsoPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(610.7f, 611.6f), "TopGinsoRightPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-179.9f, -88.1f), "ValleyEntryTreePlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-468.2f, -67.5f), "ValleyMainPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-814.6f, -265.7f), "ForlornPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-606.7f, -313.9f), "RightForlornPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-629.3f, 249.6f), "LeftSorrowPlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(-477.1f, 586f), "SunstonePlant"),
        new RandomizerWorldMapIcon(RandomizerWorldMapIconType.Plant, new Vector3(318.5f, 245.6f), "HoruR3Plant")
    };

    public class RandomizerWorldMapIcon
    {
        public MoonGuid Guid;
        public RandomizerWorldMapIconType Type;
        public Vector3 Position;

        public RandomizerWorldMapIcon(RandomizerWorldMapIconType type, Vector3 position, MoonGuid guid)
        {
            Guid = guid;
            Type = type;
            Position = position;
        }

        public RandomizerWorldMapIcon(RandomizerWorldMapIconType type, Vector3 position, string name)
        {
            Guid = RandomizerLocationManager.LocationsByName[name].MoonGuid;
            Type = type;
            Position = position;
        }
    }
}
