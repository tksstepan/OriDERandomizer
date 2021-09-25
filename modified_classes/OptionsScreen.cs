using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000079 RID: 121
public class OptionsScreen : MenuScreen, ISuspendable
{
	// Token: 0x06000314 RID: 788 RVA: 0x00037AB4 File Offset: 0x00035CB4
	public void Awake()
	{
		OptionsScreen.Instance = this;
		SuspensionManager.Register(this);
		CleverMenuItemSelectionManager navigation = this.Navigation;
		navigation.OnBackPressedCallback = (Action)Delegate.Combine(navigation.OnBackPressedCallback, new Action(this.OnBackPressed));
		this.AddSubscreen<ControlsSettingsScreen>("CONTROL OPTIONS", 2);
		this.AddSubscreen<AccessibilitySettingsScreen>("ACCESSIBILITY", 3);
		this.AddSubscreen<KeybindsScreen>("KEYBINDS", 4);
		this.AddSubscreen<MenuKeybindsScreen>("MENU KEYBINDS", 5);
		this.AddSubscreen<ControllerBindsScreen>("CONTROLLER BINDS", 6);
		this.AddSubscreen<ControllerMenuBindsScreen>("CONTROLLER MENU BINDS", 7);
	}

	// Token: 0x06000315 RID: 789 RVA: 0x0000478E File Offset: 0x0000298E
	public void OnDestroy()
	{
		CleverMenuItemSelectionManager navigation = this.Navigation;
		navigation.OnBackPressedCallback = (Action)Delegate.Remove(navigation.OnBackPressedCallback, new Action(this.OnBackPressed));
	}

	// Token: 0x06000316 RID: 790 RVA: 0x000047B7 File Offset: 0x000029B7
	public void FixedUpdate()
	{
		if (Core.Input.Bash.OnPressed)
		{
			XboxOne.Help();
		}
	}

	// Token: 0x06000317 RID: 791 RVA: 0x00037B24 File Offset: 0x00035D24
	public override void Hide()
	{
		this.Navigation.SetVisible(false);
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in base.GetComponent<CleverMenuItemGroup>().Options)
		{
			if (cleverMenuItemGroupItem.ItemGroup)
			{
				cleverMenuItemGroupItem.ItemGroup.IsActive = false;
			}
		}
	}

	// Token: 0x06000318 RID: 792 RVA: 0x000047CE File Offset: 0x000029CE
	public override void ShowImmediate()
	{
		this.Navigation.SetVisibleImmediate(true);
		this.Navigation.SetIndexToFirst();
	}

	// Token: 0x06000319 RID: 793 RVA: 0x000047E7 File Offset: 0x000029E7
	public override void HideImmediate()
	{
		this.Navigation.SetVisibleImmediate(false);
	}

	// Token: 0x0600031A RID: 794 RVA: 0x000047F5 File Offset: 0x000029F5
	public override void Show()
	{
		this.Navigation.RefreshVisible();
		this.Navigation.SetVisible(true);
		this.Navigation.SetIndexToFirst();
	}

	// Token: 0x0600031B RID: 795 RVA: 0x00004819 File Offset: 0x00002A19
	public void OnBackPressed()
	{
		if (GameController.Instance.GameInTitleScreen)
		{
			UI.Menu.HideMenuScreen(false);
		}
		else
		{
			UI.Menu.ShowInventoryOrPauseMenu();
		}
	}

	// Token: 0x1700009E RID: 158
	// (get) Token: 0x0600031C RID: 796 RVA: 0x00004844 File Offset: 0x00002A44
	// (set) Token: 0x0600031D RID: 797 RVA: 0x0000484C File Offset: 0x00002A4C
	public bool IsSuspended { get; set; }

	// Token: 0x0600031E RID: 798 RVA: 0x00037B9C File Offset: 0x00035D9C
	public void AddSubscreen<TController>(string label, int index) where TController : MonoBehaviour
	{
		this.Navigation.AddMenuItem(label, index, this.Navigation.transform.FindChild("mainMenuUI").GetComponent<CleverMenuItemLayout>(), delegate
		{
		});
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(base.transform.FindChild("*settings").gameObject);
		gameObject.name = "*" + label.ToLower();
		gameObject.transform.SetParent(base.transform);
		UnityEngine.Object.Destroy(gameObject.GetComponent<SettingsScreen>());
		gameObject.AddComponent<TController>();
		gameObject.SetActive(false);
		base.GetComponent<CleverMenuItemGroup>().AddItem(this.Navigation.MenuItems[index], gameObject.GetComponent<CleverMenuItemGroupBase>());
	}

	// Token: 0x04000328 RID: 808
	public static OptionsScreen Instance;

	// Token: 0x04000329 RID: 809
	public SoundProvider OpenSound;

	// Token: 0x0400032A RID: 810
	public SoundProvider CloseSound;

	// Token: 0x0400032B RID: 811
	public CleverMenuItemSelectionManager Navigation;
}
