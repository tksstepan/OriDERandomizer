using System;
using Core;
using Game;
using UnityEngine;

public class OptionsScreen : MenuScreen, ISuspendable
{
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

	public void OnDestroy()
	{
		CleverMenuItemSelectionManager navigation = this.Navigation;
		navigation.OnBackPressedCallback = (Action)Delegate.Remove(navigation.OnBackPressedCallback, new Action(this.OnBackPressed));
	}

	public void FixedUpdate()
	{
		if (Core.Input.Bash.OnPressed)
		{
			XboxOne.Help();
		}
	}

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

	public override void ShowImmediate()
	{
		this.Navigation.SetVisibleImmediate(true);
		this.Navigation.SetIndexToFirst();
	}

	public override void HideImmediate()
	{
		this.Navigation.SetVisibleImmediate(false);
	}

	public override void Show()
	{
		this.Navigation.RefreshVisible();
		this.Navigation.SetVisible(true);
		this.Navigation.SetIndexToFirst();
	}

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

	public bool IsSuspended { get; set; }

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

	public static OptionsScreen Instance;

	public SoundProvider OpenSound;

	public SoundProvider CloseSound;

	public CleverMenuItemSelectionManager Navigation;
}
