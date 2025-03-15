using System;

public class ControllerMenuBindsScreen : CustomSettingsScreen
{
	public override void InitScreen()
	{
		base.AddControllerBind("Pause", () => PlayerInputRebinding.ControllerRebindings.Start, k =>	PlayerInputRebinding.ControllerRebindings.Start = k);
		base.AddControllerBind("Cancel", () => PlayerInputRebinding.ControllerRebindings.Cancel, k => PlayerInputRebinding.ControllerRebindings.Cancel = k);
		base.AddControllerBind("Proceed", () => PlayerInputRebinding.ControllerRebindings.ActionButtonA, k => PlayerInputRebinding.ControllerRebindings.ActionButtonA = k);
		base.AddControllerBind("Menu Up", () => PlayerInputRebinding.ControllerRebindings.MenuUp, k => PlayerInputRebinding.ControllerRebindings.MenuUp = k);
		base.AddControllerBind("Menu Down", () => PlayerInputRebinding.ControllerRebindings.MenuDown, k => PlayerInputRebinding.ControllerRebindings.MenuDown = k);
		base.AddControllerBind("Menu Left", () => PlayerInputRebinding.ControllerRebindings.MenuLeft, k => PlayerInputRebinding.ControllerRebindings.MenuLeft = k);
		base.AddControllerBind("Menu Right", () => PlayerInputRebinding.ControllerRebindings.MenuRight, k => PlayerInputRebinding.ControllerRebindings.MenuRight = k);
		base.AddControllerBind("Menu Next", () => PlayerInputRebinding.ControllerRebindings.MenuPageRight, k => PlayerInputRebinding.ControllerRebindings.MenuPageRight = k);
		base.AddControllerBind("Menu Previous", () => PlayerInputRebinding.ControllerRebindings.MenuPageLeft, k => PlayerInputRebinding.ControllerRebindings.MenuPageLeft = k);
		base.AddControllerBind("Map", () => PlayerInputRebinding.ControllerRebindings.Select, k => PlayerInputRebinding.ControllerRebindings.Select = k);
		base.AddControllerBind("Zoom In (Map)", () => PlayerInputRebinding.ControllerRebindings.ZoomIn, k => PlayerInputRebinding.ControllerRebindings.ZoomIn = k);
		base.AddControllerBind("Zoom Out (Map)", () => PlayerInputRebinding.ControllerRebindings.ZoomOut, k => PlayerInputRebinding.ControllerRebindings.ZoomOut = k);
		base.AddButton("Reset Keybinds", new Action(this.ResetKeybinds));

		// Lower tooltip so it fits under the options
		var pos = this.tooltipController.transform.position;
		pos.y = -3.38f;
		this.tooltipController.transform.position = pos;
		HideLegend();
	}

	private void ResetKeybinds()
	{
		PlayerInputRebinding.SetDefaultControllerBindingSettings();
		PlayerInput instance = PlayerInput.Instance;
		if (instance != null)
		{
			instance.RefreshControlScheme();
		}
		ControllerBindControl[] componentsInChildren = OptionsScreen.Instance.transform.GetComponentsInChildren<ControllerBindControl>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Reset();
		}
		PlayerInputRebinding.WriteControllerRebindSettings();
	}
}
