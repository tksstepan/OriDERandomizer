using System;
using Core;
using Game;
using UnityEngine;

public class AreaMapDebugNavigation : MonoBehaviour
{
	public void Awake()
	{
		this.m_areaMapUi = base.GetComponent<AreaMapUI>();
	}

	public void Advance()
	{
		if (XboxLiveController.IsContentPackage)
		{
			return;
		}
		Core.Input.ChargeJump.Used = true;
		if (!DebugMenuB.DebugControlsEnabled)
		{
			return;
		}
		if (Core.Input.RightShoulder.OnPressed)
		{
			if (this.UndiscoveredMapVisible)
			{
				this.ToggleUndiscoveredMap(false);
			}
			else
			{
				this.ToggleUndiscoveredMap(true);
			}
		}
		if (Core.Input.RightClick.OnPressed)
		{
			Vector2 cursorPosition = Core.Input.CursorPositionUI;
			Vector2 worldPosition = this.m_areaMapUi.Navigation.MapToWorldPosition(cursorPosition);
			if (Characters.Sein != null)
			{
				Characters.Sein.Position = worldPosition + new Vector2(0f, 0.5f);
				UI.Cameras.Current.MoveCameraToTargetInstantly(true);
				UI.Menu.HideMenuScreen(true);
				return;
			}
		}
	}

	public void ToggleUndiscoveredMap(bool show)
	{
		this.UndiscoveredMapVisible = show;
		this.m_areaMapUi.ResetMaps();
		this.m_areaMapUi.Navigation.UpdateScrollLimits();
	}

	public GameObject DebugSceneBoundsMarkerPrefab;

	public float HiddenColorAlpha;

	public float UndiscoveredColorAlpha = 0.2f;

	private AreaMapUI m_areaMapUi;

	public bool UndiscoveredMapVisible;
}
