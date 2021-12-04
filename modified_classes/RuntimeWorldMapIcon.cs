using System;
using Game;
using UnityEngine;

public class RuntimeWorldMapIcon
{
	public RuntimeWorldMapIcon(GameWorldArea.WorldMapIcon icon, RuntimeGameWorldArea area)
	{
		this.Icon = icon.Icon;
		this.Guid = icon.Guid;
		this.Position = icon.Position;
		this.Area = area;
		this.IsSecret = icon.IsSecret;
	}

	public bool IsVisible(AreaMapUI areaMap)
	{
		if (!Characters.Sein.PlayerAbilities.MapMarkers.HasAbility)
		{
			return false;
		}

		// show randomizer pickup icons only if they're reachable and not yet collected
		if (RandomizerLocationManager.LocationsByWorldMapGuid.ContainsKey(this.Guid))
		{
			RandomizerLocationManager.Location loc = RandomizerLocationManager.LocationsByWorldMapGuid[this.Guid];
			return loc.Reachable && !loc.Collected;
		}

		return true;
	}

	public void Show()
	{
		AreaMapUI instance = AreaMapUI.Instance;
		if (this.Icon == WorldMapIconType.Invisible)
		{
			return;
		}
		if (!this.IsVisible(instance))
		{
			return;
		}
		if (this.m_iconGameObject)
		{
			this.m_iconGameObject.SetActive(true);
			return;
		}

		if (this.RandomizerIconType != RandomizerWorldMapIconType.None)
		{
			InitRandomizerIcon();
		}
		else
		{
			InitStandardIcon(this.Icon);
		}
	}

	private void InitStandardIcon(WorldMapIconType iconType)
	{
		GameObject icon = AreaMapUI.Instance.IconManager.GetIcon(iconType);
		this.m_iconGameObject = (GameObject)InstantiateUtility.Instantiate(icon);
		Transform transform = this.m_iconGameObject.transform;
		transform.parent = AreaMapUI.Instance.Navigation.MapPivot.transform;
		transform.localPosition = this.Position;
		transform.localRotation = Quaternion.identity;
		transform.localScale = icon.transform.localScale;
		TransparencyAnimator.Register(transform);
	}

	private void InitRandomizerIcon()
	{
		switch (RandomizerIconType)
		{
			case RandomizerWorldMapIconType.WaterVein:
				CreateIconFromInventory(InventoryManager.Instance.GinsoTreeKey.transform.Find("ginsoKeyGraphic"), 4);
				break;
			case RandomizerWorldMapIconType.CleanWater:
				CreateIconFromInventory(InventoryManager.Instance.WorldEventsGroup.transform.Find("waterPurifiedIcon/waterPurifiedGraphics"), 20);
				var offset = m_iconGameObject.transform.Find("waterPurifiedGraphic").localPosition;
				foreach (var child in m_iconGameObject.transform)
					((Transform)child).localPosition -= offset;
				break;
			case RandomizerWorldMapIconType.WindRestored:
				CreateIconFromInventory(InventoryManager.Instance.WorldEventsGroup.transform.Find("windRestoredIcon/windRestoredIcon"), 10);
				break;
			case RandomizerWorldMapIconType.Sunstone:
				CreateIconFromInventory(InventoryManager.Instance.MountHoruKey.transform.Find("sunStoneA"), 8);
				break;
			case RandomizerWorldMapIconType.HoruRoom:
				CreateIconFromInventory(InventoryManager.Instance.WorldEventsGroup.transform.Find("warmthReturned/warmthReturnedGraphics"), 10);
				break;
			case RandomizerWorldMapIconType.Plant:
				InitStandardIcon(WorldMapIconType.HealthUpgrade);
				this.m_iconGameObject.name = "plantMapIcon(Clone)";
				Renderer[] componentsInChildren = this.m_iconGameObject.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
					componentsInChildren[i].material.color = new Color(0.1792157f, 0.2364706f, 0.8656863f);
				this.m_iconGameObject.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
				break;
			case RandomizerWorldMapIconType.SkillTree:
				InitStandardIcon(WorldMapIconType.AbilityPedestal);
				break;
			default:
				break;
		}
	}

	private void CreateIconFromInventory(Transform obj, float scale)
	{
		var clone = GameObject.Instantiate(obj).gameObject;
		clone.SetActive(true);
		clone.transform.SetParent(AreaMapUI.Instance.Navigation.MapPivot.transform);
		clone.transform.localScale = new Vector3(scale, scale, 1);
		clone.transform.localPosition = this.Position;
		TransparencyAnimator.Register(clone.transform);
		m_iconGameObject = clone;
	}

	public void Hide()
	{
		if (this.m_iconGameObject)
		{
			this.m_iconGameObject.SetActive(false);
		}
	}

	public void SetIcon(WorldMapIconType icon)
	{
		if (this.m_iconGameObject)
		{
			InstantiateUtility.Destroy(this.m_iconGameObject);
		}
		this.Icon = icon;
	}

	public MoonGuid Guid;

	public WorldMapIconType Icon;

	public Vector2 Position;

	private RuntimeGameWorldArea Area;

	public bool IsSecret;

	private GameObject m_iconGameObject;

	public RandomizerWorldMapIconType RandomizerIconType;
}
