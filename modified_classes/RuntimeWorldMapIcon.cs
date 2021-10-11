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
		GameObject icon = instance.IconManager.GetIcon(this.Icon);
		this.m_iconGameObject = (GameObject)InstantiateUtility.Instantiate(icon);
		Transform transform = this.m_iconGameObject.transform;
		transform.parent = instance.Navigation.MapPivot.transform;
		transform.localPosition = this.Position;
		transform.localRotation = Quaternion.identity;
		transform.localScale = icon.transform.localScale;
		TransparencyAnimator.Register(transform);
		if (this.IsPlant)
		{
			this.m_iconGameObject.name = "plantMapIcon(Clone)";
			Renderer[] componentsInChildren = this.m_iconGameObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.color = new Color(0.1792157f, 0.2364706f, 0.8656863f);
			}
			this.m_iconGameObject.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
		}
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

	public bool IsPlant;
}
