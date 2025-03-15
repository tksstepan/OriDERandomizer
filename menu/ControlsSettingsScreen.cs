public class ControlsSettingsScreen : CustomSettingsScreen
{
	public override void InitScreen()
	{
		AddSlider(RandomizerSettings.Controls.BashDeadzone, 0f, 1f, 0.05f, "Allows changing the bash deadzone for controller (0% - 100%).");
		AddToggle(RandomizerSettings.Controls.FastGrenadeAim, "Toggles fast grenade aim.");
		AddSlider(RandomizerSettings.Controls.GrenadeAimSpeed, 0f, 2f, 0.1f, "Allows adjusting the speed at which the grenade will aim on controller (0% - 200%).");
		AddToggle(RandomizerSettings.Controls.SwimmingMouseAim, "Toggles whether Ori should swim towards the mouse cursor.");
		AddToggle(RandomizerSettings.Controls.InvertSwim, "Toggles whether the swim speed input ([Jump]) is reversed. If enabled, press [Jump] to slow down in water.");
		AddToggle(RandomizerSettings.Controls.WallChargeMouseAim, "Toggles whether wall Charge Jump aiming should follow the mouse cursor.");
		AddToggle(RandomizerSettings.Controls.InvertClimb, "Toggles whether Climb ([Climb]) should be inverted. If enabled, hold [Climb] to stop climbing.");
		AddToggle(RandomizerSettings.Controls.SlowClimbVault, "Toggles the slow climb vault effect, making it easier to land on narrow edges after using Climb.");
	}
}