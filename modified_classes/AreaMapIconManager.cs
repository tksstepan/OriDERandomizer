using System;
using UnityEngine;

public class AreaMapIconManager : MonoBehaviour
{
	public void Awake()
	{
	}

	public void ShowAreaIcons()
	{
		for (int i = 0; i < GameWorld.Instance.RuntimeAreas.Count; i++)
		{
			RuntimeGameWorldArea runtimeGameWorldArea = GameWorld.Instance.RuntimeAreas[i];
			foreach (var icon in RandomizerWorldMapIconManager.Icons)
			{
				if (!runtimeGameWorldArea.Area.InsideFace(icon.Position))
					continue;
				
				RuntimeWorldMapIcon runtimeWorldMapIcon = null;
				for (int j = 0; j < runtimeGameWorldArea.Icons.Count; j++)
				{
					if (runtimeGameWorldArea.Icons[j].Guid == icon.Guid)
					{
						runtimeWorldMapIcon = runtimeGameWorldArea.Icons[j];
						break;
					}
				}

				bool collected = RandomizerLocationManager.IsPickupCollected(icon.Guid);
				if (runtimeWorldMapIcon == null && !collected)
				{
					GameWorldArea.WorldMapIcon worldMapIcon = new GameWorldArea.WorldMapIcon
					{
						Guid = icon.Guid,
						Icon = WorldMapIconType.HealthUpgrade,
						IsSecret = false,
						Position = icon.Position
					};
					runtimeGameWorldArea.Icons.Add(new RuntimeWorldMapIcon(worldMapIcon, runtimeGameWorldArea)
					{
						IsPlant = true,
						RandomizerIconType = icon.Type
					});
				}
				else if (runtimeWorldMapIcon != null)
				{
					runtimeWorldMapIcon.Icon = collected ? WorldMapIconType.Invisible : WorldMapIconType.HealthUpgrade;
				}
			}
			foreach (MoonGuid moonGuid in RandomizerPlantManager.Plants.Keys)
			{
				RandomizerPlantManager.PlantData plantData = RandomizerPlantManager.Plants[moonGuid];
				if (runtimeGameWorldArea.Area.InsideFace(plantData.Position))
				{
					RuntimeWorldMapIcon runtimeWorldMapIcon = null;
					for (int j = 0; j < runtimeGameWorldArea.Icons.Count; j++)
					{
						if (runtimeGameWorldArea.Icons[j].Guid == moonGuid)
						{
							runtimeWorldMapIcon = runtimeGameWorldArea.Icons[j];
							break;
						}
					}
					if (runtimeWorldMapIcon == null && RandomizerPlantManager.Display(moonGuid))
					{
						GameWorldArea.WorldMapIcon icon = new GameWorldArea.WorldMapIcon
						{
							Guid = moonGuid,
							Icon = WorldMapIconType.HealthUpgrade,
							IsSecret = false,
							Position = plantData.Position
						};
						runtimeGameWorldArea.Icons.Add(new RuntimeWorldMapIcon(icon, runtimeGameWorldArea)
						{
							IsPlant = true
						});
					}
					else if (runtimeWorldMapIcon != null)
					{
						runtimeWorldMapIcon.Icon = (RandomizerPlantManager.Display(moonGuid) ? WorldMapIconType.HealthUpgrade : WorldMapIconType.Invisible);
					}
				}
			}
			for (int k = 0; k < runtimeGameWorldArea.Icons.Count; k++)
			{
				runtimeGameWorldArea.Icons[k].Hide();
			}
			if (!runtimeGameWorldArea.Area.VisitableCondition || runtimeGameWorldArea.Area.VisitableCondition.Validate(null))
			{
				for (int l = 0; l < runtimeGameWorldArea.Icons.Count; l++)
				{
					RuntimeWorldMapIcon runtimeWorldMapIcon2 = runtimeGameWorldArea.Icons[l];
					if (!GameMapUI.Instance.ShowingTeleporters || runtimeWorldMapIcon2.Icon != WorldMapIconType.SavePedestal)
					{
						runtimeWorldMapIcon2.Show();
					}
				}
			}
		}
	}

	public GameObject GetIcon(WorldMapIconType iconType)
	{
		switch (iconType)
		{
		case WorldMapIconType.Keystone:
			return this.Icons.Keystone;
		case WorldMapIconType.Mapstone:
			return this.Icons.Mapstone;
		case WorldMapIconType.BreakableWall:
			return this.Icons.BreakableWall;
		case WorldMapIconType.BreakableWallBroken:
			return this.Icons.BreakableWallBroken;
		case WorldMapIconType.StompableFloor:
			return this.Icons.StompableFloor;
		case WorldMapIconType.StompableFloorBroken:
			return this.Icons.StompableFloorBroken;
		case WorldMapIconType.EnergyGateTwo:
			return this.Icons.EnergyGateTwo;
		case WorldMapIconType.EnergyGateOpen:
			return this.Icons.EnergyGateOpen;
		case WorldMapIconType.KeystoneDoorFour:
			return this.Icons.KeystoneDoorFour;
		case WorldMapIconType.KeystoneDoorOpen:
			return this.Icons.KeystoneDoorOpen;
		case WorldMapIconType.AbilityPedestal:
			return this.Icons.AbilityPedestal;
		case WorldMapIconType.HealthUpgrade:
			return this.Icons.HealthUpgrade;
		case WorldMapIconType.EnergyUpgrade:
			return this.Icons.EnergyUpgrade;
		case WorldMapIconType.SavePedestal:
			return this.Icons.SavePedestal;
		case WorldMapIconType.AbilityPoint:
			return this.Icons.AbilityPoint;
		case WorldMapIconType.KeystoneDoorTwo:
			return this.Icons.KeystoneDoorTwo;
		case WorldMapIconType.Experience:
			return this.Icons.Experience;
		case WorldMapIconType.MapstonePickup:
			return this.Icons.MapstonePickup;
		case WorldMapIconType.EnergyGateTwelve:
			return this.Icons.EnergyGateTwelve;
		case WorldMapIconType.EnergyGateTen:
			return this.Icons.EnergyGateTen;
		case WorldMapIconType.EnergyGateEight:
			return this.Icons.EnergyGateEight;
		case WorldMapIconType.EnergyGateSix:
			return this.Icons.EnergyGateSix;
		case WorldMapIconType.EnergyGateFour:
			return this.Icons.EnergyGateFour;
		}
		return null;
	}

	public AreaMapIconManager.IconGameObjects Icons;

	[Serializable]
	public class IconGameObjects
	{
		public GameObject Keystone;

		public GameObject Mapstone;

		public GameObject BreakableWall;

		public GameObject BreakableWallBroken;

		public GameObject StompableFloor;

		public GameObject StompableFloorBroken;

		public GameObject EnergyGateOpen;

		public GameObject KeystoneDoorTwo;

		public GameObject KeystoneDoorFour;

		public GameObject KeystoneDoorOpen;

		public GameObject AbilityPedestal;

		public GameObject HealthUpgrade;

		public GameObject EnergyUpgrade;

		public GameObject SavePedestal;

		public GameObject AbilityPoint;

		public GameObject Experience;

		public GameObject MapstonePickup;

		public GameObject EnergyGateTwelve;

		public GameObject EnergyGateTen;

		public GameObject EnergyGateEight;

		public GameObject EnergyGateSix;

		public GameObject EnergyGateFour;

		public GameObject EnergyGateTwo;
	}
}
