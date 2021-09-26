using System;
using UnityEngine;

// Token: 0x02000A59 RID: 2649
public class MenuKeybindsScreen : CustomSettingsScreen
{
	// Token: 0x06003B70 RID: 15216
	public override void InitScreen()
	{
		this.AddKeybind("Pause", () => PlayerInputRebinding.KeyRebindings.Start, k => PlayerInputRebinding.KeyRebindings.Start = k);
        this.AddKeybind("Cancel", () => PlayerInputRebinding.KeyRebindings.Cancel, k => PlayerInputRebinding.KeyRebindings.Cancel = k);
        this.AddKeybind("Proceed", () => PlayerInputRebinding.KeyRebindings.ActionButtonA, k => PlayerInputRebinding.KeyRebindings.ActionButtonA = k);
        this.AddKeybind("Menu Up", () => PlayerInputRebinding.KeyRebindings.MenuUp, k => PlayerInputRebinding.KeyRebindings.MenuUp = k);
        this.AddKeybind("Menu Down", () => PlayerInputRebinding.KeyRebindings.MenuDown, k => PlayerInputRebinding.KeyRebindings.MenuDown = k);
        this.AddKeybind("Menu Left", () => PlayerInputRebinding.KeyRebindings.MenuLeft, k => PlayerInputRebinding.KeyRebindings.MenuLeft = k);
        this.AddKeybind("Menu Right", () => PlayerInputRebinding.KeyRebindings.MenuRight, k => PlayerInputRebinding.KeyRebindings.MenuRight = k);
        this.AddKeybind("Menu Previous", () => PlayerInputRebinding.KeyRebindings.MenuPageLeft, k => PlayerInputRebinding.KeyRebindings.MenuPageLeft = k);
        this.AddKeybind("Menu Next", () => PlayerInputRebinding.KeyRebindings.MenuPageRight, k => PlayerInputRebinding.KeyRebindings.MenuPageRight = k);
        this.AddKeybind("Map", () => PlayerInputRebinding.KeyRebindings.Select, k => PlayerInputRebinding.KeyRebindings.Select = k);
        this.AddKeybind("Zoom In (Map)", () => PlayerInputRebinding.KeyRebindings.ZoomIn, k => PlayerInputRebinding.KeyRebindings.ZoomIn = k);
        this.AddKeybind("Zoom Out (Map)", () => PlayerInputRebinding.KeyRebindings.ZoomOut, k => PlayerInputRebinding.KeyRebindings.ZoomOut = k);
		base.AddButton("Reset Keybinds", new Action(this.ResetKeybinds));

		// Lower tooltip so it fits under the options
		var pos = this.tooltipController.transform.position;
		pos.y = -3.38f;
		this.tooltipController.transform.position = pos;
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
