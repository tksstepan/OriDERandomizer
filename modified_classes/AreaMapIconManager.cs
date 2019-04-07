using System;
using UnityEngine;

// Token: 0x020007EF RID: 2031
public class AreaMapIconManager : MonoBehaviour
{
	// Token: 0x06002BC5 RID: 11205 RVA: 0x00002858 File Offset: 0x00000A58
	public AreaMapIconManager()
	{
	}

	// Token: 0x06002BC6 RID: 11206 RVA: 0x00002EE7 File Offset: 0x000010E7
	public void Awake()
	{
	}

	// Token: 0x06002BC7 RID: 11207
	public void ShowAreaIcons()
	{
		for (int i = 0; i < GameWorld.Instance.RuntimeAreas.Count; i++)
		{
			RuntimeGameWorldArea runtimeGameWorldArea = GameWorld.Instance.RuntimeAreas[i];
			foreach (MoonGuid guid in RandomizerPlantManager.Plants.Keys)
			{
				RandomizerPlantManager.PlantData plant = RandomizerPlantManager.Plants[guid];
				if (runtimeGameWorldArea.Area.InsideFace(plant.Position))
				{
					RuntimeWorldMapIcon runtimeWorldMapIcon = null;
					for (int j = 0; j < runtimeGameWorldArea.Icons.Count; j++)
					{
						if (runtimeGameWorldArea.Icons[j].Guid == guid)
						{
							runtimeWorldMapIcon = runtimeGameWorldArea.Icons[j];
							break;
						}
					}
					if (runtimeWorldMapIcon == null && RandomizerPlantManager.Display(guid))
					{
						GameWorldArea.WorldMapIcon icon = new GameWorldArea.WorldMapIcon
						{
							Guid = guid,
							Icon = WorldMapIconType.Experience,
							IsSecret = false,
							Position = plant.Position
						};
						runtimeGameWorldArea.Icons.Add(new RuntimeWorldMapIcon(icon, runtimeGameWorldArea));
					}
					else if (runtimeWorldMapIcon != null)
					{
						runtimeWorldMapIcon.Icon = (RandomizerPlantManager.Display(guid) ? WorldMapIconType.Experience : WorldMapIconType.Invisible);
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

	// Token: 0x06002BC8 RID: 11208 RVA: 0x000BF190 File Offset: 0x000BD390
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

	// Token: 0x04002755 RID: 10069
	public AreaMapIconManager.IconGameObjects Icons;

	// Token: 0x020007F0 RID: 2032
	[Serializable]
	public class IconGameObjects
	{
		// Token: 0x06002BC9 RID: 11209 RVA: 0x00002AFF File Offset: 0x00000CFF
		public IconGameObjects()
		{
		}

		// Token: 0x04002756 RID: 10070
		public GameObject Keystone;

		// Token: 0x04002757 RID: 10071
		public GameObject Mapstone;

		// Token: 0x04002758 RID: 10072
		public GameObject BreakableWall;

		// Token: 0x04002759 RID: 10073
		public GameObject BreakableWallBroken;

		// Token: 0x0400275A RID: 10074
		public GameObject StompableFloor;

		// Token: 0x0400275B RID: 10075
		public GameObject StompableFloorBroken;

		// Token: 0x0400275C RID: 10076
		public GameObject EnergyGateOpen;

		// Token: 0x0400275D RID: 10077
		public GameObject KeystoneDoorTwo;

		// Token: 0x0400275E RID: 10078
		public GameObject KeystoneDoorFour;

		// Token: 0x0400275F RID: 10079
		public GameObject KeystoneDoorOpen;

		// Token: 0x04002760 RID: 10080
		public GameObject AbilityPedestal;

		// Token: 0x04002761 RID: 10081
		public GameObject HealthUpgrade;

		// Token: 0x04002762 RID: 10082
		public GameObject EnergyUpgrade;

		// Token: 0x04002763 RID: 10083
		public GameObject SavePedestal;

		// Token: 0x04002764 RID: 10084
		public GameObject AbilityPoint;

		// Token: 0x04002765 RID: 10085
		public GameObject Experience;

		// Token: 0x04002766 RID: 10086
		public GameObject MapstonePickup;

		// Token: 0x04002767 RID: 10087
		public GameObject EnergyGateTwelve;

		// Token: 0x04002768 RID: 10088
		public GameObject EnergyGateTen;

		// Token: 0x04002769 RID: 10089
		public GameObject EnergyGateEight;

		// Token: 0x0400276A RID: 10090
		public GameObject EnergyGateSix;

		// Token: 0x0400276B RID: 10091
		public GameObject EnergyGateFour;

		// Token: 0x0400276C RID: 10092
		public GameObject EnergyGateTwo;
	}
}
