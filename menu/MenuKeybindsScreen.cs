using System;
using UnityEngine;

// Token: 0x02000A59 RID: 2649
public class MenuKeybindsScreen : CustomSettingsScreen
{
	// Token: 0x06003B70 RID: 15216
	public override void InitScreen()
	{
		this.AddKeybind("Start", () => PlayerInputRebinding.KeyRebindings.Start, k => PlayerInputRebinding.KeyRebindings.Start = k, PlayerInput.Instance.Start);
        this.AddKeybind("Cancel", () => PlayerInputRebinding.KeyRebindings.Cancel, k => PlayerInputRebinding.KeyRebindings.Cancel = k, PlayerInput.Instance.Cancel);
        this.AddKeybind("Proceed", () => PlayerInputRebinding.KeyRebindings.ActionButtonA, k => PlayerInputRebinding.KeyRebindings.ActionButtonA = k, PlayerInput.Instance.ActionButtonA);
        this.AddKeybind("Menu Up", () => PlayerInputRebinding.KeyRebindings.MenuUp, k => PlayerInputRebinding.KeyRebindings.MenuUp = k, PlayerInput.Instance.MenuUp);
        this.AddKeybind("Menu Down", () => PlayerInputRebinding.KeyRebindings.MenuDown, k => PlayerInputRebinding.KeyRebindings.MenuDown = k, PlayerInput.Instance.MenuDown);
        this.AddKeybind("Menu Left", () => PlayerInputRebinding.KeyRebindings.MenuLeft, k => PlayerInputRebinding.KeyRebindings.MenuLeft = k, PlayerInput.Instance.MenuLeft);
        this.AddKeybind("Menu Right", () => PlayerInputRebinding.KeyRebindings.MenuRight, k => PlayerInputRebinding.KeyRebindings.MenuRight = k, PlayerInput.Instance.MenuRight);

        this.AddKeybind("Menu Next", () => PlayerInputRebinding.KeyRebindings.MenuPageRight, k => PlayerInputRebinding.KeyRebindings.MenuPageRight = k, PlayerInput.Instance.MenuPageRight);
        this.AddKeybind("Menu Previous", () => PlayerInputRebinding.KeyRebindings.MenuPageLeft, k => PlayerInputRebinding.KeyRebindings.MenuPageLeft = k, PlayerInput.Instance.MenuPageLeft);
        this.AddKeybind("Map", () => PlayerInputRebinding.KeyRebindings.Select, k => PlayerInputRebinding.KeyRebindings.Select = k, PlayerInput.Instance.Select);
        this.AddKeybind("Zoom In (Map)", () => PlayerInputRebinding.KeyRebindings.ZoomIn, k => PlayerInputRebinding.KeyRebindings.ZoomIn = k, PlayerInput.Instance.ZoomIn);
        this.AddKeybind("Zoom Out (Map)", () => PlayerInputRebinding.KeyRebindings.ZoomOut, k => PlayerInputRebinding.KeyRebindings.ZoomOut = k, PlayerInput.Instance.ZoomOut);
		base.AddButton("Reset Keybinds", new Action(this.ResetKeybinds));
	}

    private void ResetKeybinds()
	{
		PlayerInputRebinding.SetDefaultKeyBindingSettings();
		PlayerInput instance = PlayerInput.Instance;
		if (instance != null)
		{
			instance.RefreshControlScheme();
		}
		KeybindControl[] componentsInChildren = OptionsScreen.Instance.transform.GetComponentsInChildren<KeybindControl>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Reset();
		}
		PlayerInputRebinding.WriteKeyRebindSettings();
	}
}
