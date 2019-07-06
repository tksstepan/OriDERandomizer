using System;

// Token: 0x02000A4A RID: 2634
public class ControllerBindsScreen : CustomSettingsScreen
{
	// Token: 0x0600390C RID: 14604 RVA: 0x000E7A0C File Offset: 0x000E5C0C
	public override void InitScreen()
	{
		base.AddControllerBind("Bash", () => PlayerInputRebinding.ControllerRebindings.Bash, k => PlayerInputRebinding.ControllerRebindings.Bash = k);
		base.AddControllerBind("Charge Jump", () => PlayerInputRebinding.ControllerRebindings.ChargeJump, k => PlayerInputRebinding.ControllerRebindings.ChargeJump = k);
		base.AddControllerBind("Dash", () => PlayerInputRebinding.ControllerRebindings.RightShoulder, k => PlayerInputRebinding.ControllerRebindings.RightShoulder = k);
		base.AddControllerBind("Glide", () => PlayerInputRebinding.ControllerRebindings.Glide, k => PlayerInputRebinding.ControllerRebindings.Glide = k);
		base.AddControllerBind("Grab", () => PlayerInputRebinding.ControllerRebindings.Grab, k => PlayerInputRebinding.ControllerRebindings.Grab = k);
		base.AddControllerBind("Grenade", () => PlayerInputRebinding.ControllerRebindings.LeftShoulder, k => PlayerInputRebinding.ControllerRebindings.LeftShoulder = k);
		base.AddControllerBind("Jump", () => PlayerInputRebinding.ControllerRebindings.Jump, k => PlayerInputRebinding.ControllerRebindings.Jump = k);
		base.AddControllerBind("Soul Link", () => PlayerInputRebinding.ControllerRebindings.SoulFlame, k => PlayerInputRebinding.ControllerRebindings.SoulFlame = k);
		base.AddControllerBind("Spirit Flame", () => PlayerInputRebinding.ControllerRebindings.SpiritFlame, k => PlayerInputRebinding.ControllerRebindings.SpiritFlame = k);
		base.AddControllerBind("Stomp", () => PlayerInputRebinding.ControllerRebindings.Stomp, k => PlayerInputRebinding.ControllerRebindings.Stomp = k);
		base.AddControllerBind("Movement Up", () => PlayerInputRebinding.ControllerRebindings.VerticalDigiPadUp, k => PlayerInputRebinding.ControllerRebindings.VerticalDigiPadUp = k);
		base.AddControllerBind("Movement Down", () => PlayerInputRebinding.ControllerRebindings.VerticalDigiPadDown, k => PlayerInputRebinding.ControllerRebindings.VerticalDigiPadDown = k);
		base.AddControllerBind("Movement Left", () => PlayerInputRebinding.ControllerRebindings.HorizontalDigiPadLeft, k => PlayerInputRebinding.ControllerRebindings.HorizontalDigiPadLeft = k);
		base.AddControllerBind("Movement Right", () => PlayerInputRebinding.ControllerRebindings.HorizontalDigiPadRight, k => PlayerInputRebinding.ControllerRebindings.HorizontalDigiPadRight = k);
	}
}
