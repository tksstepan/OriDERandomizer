using System;
using SmartInput;
using UnityEngine;

public class KeybindsScreen : CustomSettingsScreen
{
	public override void InitScreen()
	{
		this.AddKeybind("Bash", () => PlayerInputRebinding.KeyRebindings.Bash, k => PlayerInputRebinding.KeyRebindings.Bash = k);
		this.AddKeybind("Charge Jump", () => PlayerInputRebinding.KeyRebindings.ChargeJump, k => PlayerInputRebinding.KeyRebindings.ChargeJump = k);
		this.AddKeybind("Dash", () => PlayerInputRebinding.KeyRebindings.RightShoulder, k => PlayerInputRebinding.KeyRebindings.RightShoulder = k);
		this.AddKeybind("Glide", () => PlayerInputRebinding.KeyRebindings.Glide, k => PlayerInputRebinding.KeyRebindings.Glide = k);
		this.AddKeybind("Grab", () => PlayerInputRebinding.KeyRebindings.Grab, k => PlayerInputRebinding.KeyRebindings.Grab = k);
		this.AddKeybind("Grenade", () => PlayerInputRebinding.KeyRebindings.LeftShoulder, k => PlayerInputRebinding.KeyRebindings.LeftShoulder = k);
		this.AddKeybind("Jump", () => PlayerInputRebinding.KeyRebindings.Jump, k => PlayerInputRebinding.KeyRebindings.Jump = k);
		this.AddKeybind("Soul Link", () => PlayerInputRebinding.KeyRebindings.SoulFlame, k => PlayerInputRebinding.KeyRebindings.SoulFlame = k);
		this.AddKeybind("Spirit Flame", () => PlayerInputRebinding.KeyRebindings.SpiritFlame, k => PlayerInputRebinding.KeyRebindings.SpiritFlame = k);
		this.AddKeybind("Stomp", () => PlayerInputRebinding.KeyRebindings.Stomp, k => PlayerInputRebinding.KeyRebindings.Stomp = k);
		this.AddKeybind("Movement Up", () => PlayerInputRebinding.KeyRebindings.VerticalDigiPadUp, k => PlayerInputRebinding.KeyRebindings.VerticalDigiPadUp = k);
		this.AddKeybind("Movement Down", () => PlayerInputRebinding.KeyRebindings.VerticalDigiPadDown, k => PlayerInputRebinding.KeyRebindings.VerticalDigiPadDown = k);
		this.AddKeybind("Movement Left", () => PlayerInputRebinding.KeyRebindings.HorizontalDigiPadLeft, k => PlayerInputRebinding.KeyRebindings.HorizontalDigiPadLeft = k);
		this.AddKeybind("Movement Right", () => PlayerInputRebinding.KeyRebindings.HorizontalDigiPadRight, k => PlayerInputRebinding.KeyRebindings.HorizontalDigiPadRight = k);

		// Lower tooltip so it fits under the options
		var pos = this.tooltipController.transform.position;
		pos.y = -3.38f;
		this.tooltipController.transform.position = pos;
		HideLegend();
	}
}