using System;
using SmartInput;
using UnityEngine;

// Token: 0x02000A31 RID: 2609
public class KeybindsScreen : CustomSettingsScreen
{
	// Token: 0x06003B3D RID: 15165
	public override void InitScreen()
	{
		this.AddKeybind("Bash", () => PlayerInputRebinding.KeyRebindings.Bash, k => PlayerInputRebinding.KeyRebindings.Bash = k, PlayerInput.Instance.Bash);
        this.AddKeybind("Charge Jump", () => PlayerInputRebinding.KeyRebindings.ChargeJump, k => PlayerInputRebinding.KeyRebindings.ChargeJump = k, PlayerInput.Instance.ChargeJump);
        this.AddKeybind("Dash", () => PlayerInputRebinding.KeyRebindings.RightShoulder, k => PlayerInputRebinding.KeyRebindings.RightShoulder = k, PlayerInput.Instance.RightShoulder);
        this.AddKeybind("Glide", () => PlayerInputRebinding.KeyRebindings.Glide, k => PlayerInputRebinding.KeyRebindings.Glide = k, PlayerInput.Instance.Glide);
        this.AddKeybind("Grab", () => PlayerInputRebinding.KeyRebindings.Grab, k => PlayerInputRebinding.KeyRebindings.Grab = k, PlayerInput.Instance.Grab);
        this.AddKeybind("Grenade", () => PlayerInputRebinding.KeyRebindings.LeftShoulder, k => PlayerInputRebinding.KeyRebindings.LeftShoulder = k, PlayerInput.Instance.LeftShoulder);
        this.AddKeybind("Jump", () => PlayerInputRebinding.KeyRebindings.Jump, k => PlayerInputRebinding.KeyRebindings.Jump = k, PlayerInput.Instance.Jump);
        this.AddKeybind("Soul Flame", () => PlayerInputRebinding.KeyRebindings.SoulFlame, k => PlayerInputRebinding.KeyRebindings.SoulFlame = k, PlayerInput.Instance.SoulFlame);
        this.AddKeybind("Spirit Flame", () => PlayerInputRebinding.KeyRebindings.SpiritFlame, k => PlayerInputRebinding.KeyRebindings.SpiritFlame = k, PlayerInput.Instance.SpiritFlame);
        this.AddKeybind("Movement Up", () => PlayerInputRebinding.KeyRebindings.VerticalDigiPadUp, k => PlayerInputRebinding.KeyRebindings.VerticalDigiPadUp = k, PlayerInput.Instance.VerticalDigiPad, SmartInput.ButtonAxisInput.Mode.Positive);
        this.AddKeybind("Movement Down", () => PlayerInputRebinding.KeyRebindings.VerticalDigiPadDown, k => PlayerInputRebinding.KeyRebindings.VerticalDigiPadDown = k, PlayerInput.Instance.VerticalDigiPad, SmartInput.ButtonAxisInput.Mode.Negative);
        this.AddKeybind("Movement Left", () => PlayerInputRebinding.KeyRebindings.HorizontalDigiPadLeft, k => PlayerInputRebinding.KeyRebindings.HorizontalDigiPadLeft = k, PlayerInput.Instance.HorizontalDigiPad, SmartInput.ButtonAxisInput.Mode.Negative);
        this.AddKeybind("Movement Right", () => PlayerInputRebinding.KeyRebindings.HorizontalDigiPadRight, k => PlayerInputRebinding.KeyRebindings.HorizontalDigiPadRight = k, PlayerInput.Instance.HorizontalDigiPad, SmartInput.ButtonAxisInput.Mode.Positive);
	}
}
