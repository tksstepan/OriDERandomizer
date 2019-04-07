using System;
using Game;
using UnityEngine;

// Token: 0x0200081D RID: 2077
public class RuntimeWorldMapIcon
{
	// Token: 0x06002CD7 RID: 11479 RVA: 0x00024BEE File Offset: 0x00022DEE
	public RuntimeWorldMapIcon(GameWorldArea.WorldMapIcon icon, RuntimeGameWorldArea area)
	{
		this.Icon = icon.Icon;
		this.Guid = icon.Guid;
		this.Position = icon.Position;
		this.Area = area;
		this.IsSecret = icon.IsSecret;
	}

	// Token: 0x06002CD8 RID: 11480
	public bool IsVisible(AreaMapUI areaMap)
	{
		return Characters.Sein.PlayerAbilities.MapMarkers.HasAbility;
	}

	// Token: 0x06002CD9 RID: 11481 RVA: 0x000C3830 File Offset: 0x000C1A30
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
		}
		else
		{
			GameObject icon = instance.IconManager.GetIcon(this.Icon);
			this.m_iconGameObject = (GameObject)InstantiateUtility.Instantiate(icon);
			Transform transform = this.m_iconGameObject.transform;
			transform.parent = instance.Navigation.MapPivot.transform;
			transform.localPosition = this.Position;
			transform.localRotation = Quaternion.identity;
			transform.localScale = icon.transform.localScale;
			TransparencyAnimator.Register(transform);
		}
	}

	// Token: 0x06002CDA RID: 11482 RVA: 0x00024C2D File Offset: 0x00022E2D
	public void Hide()
	{
		if (this.m_iconGameObject)
		{
			this.m_iconGameObject.SetActive(false);
		}
	}

	// Token: 0x06002CDB RID: 11483 RVA: 0x00024C4B File Offset: 0x00022E4B
	public void SetIcon(WorldMapIconType icon)
	{
		if (this.m_iconGameObject)
		{
			InstantiateUtility.Destroy(this.m_iconGameObject);
		}
		this.Icon = icon;
	}

	// Token: 0x0400285B RID: 10331
	public MoonGuid Guid;

	// Token: 0x0400285C RID: 10332
	public WorldMapIconType Icon;

	// Token: 0x0400285D RID: 10333
	public Vector2 Position;

	// Token: 0x0400285E RID: 10334
	private RuntimeGameWorldArea Area;

	// Token: 0x0400285F RID: 10335
	public bool IsSecret;

	// Token: 0x04002860 RID: 10336
	private GameObject m_iconGameObject;
}
