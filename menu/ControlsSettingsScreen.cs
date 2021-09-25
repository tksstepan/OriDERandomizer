public class ControlsSettingsScreen : CustomSettingsScreen
{
	public override void InitScreen()
	{
		AddSlider("Bash Deadzone", RandomizerSettings.Controls.BashDeadzone, 0f, 1f, 0.05f);
        AddToggle("Instant Grenade Aim", RandomizerSettings.Controls.FastGrenadeAim);
		AddSlider("Grenade Aim Speed", RandomizerSettings.Controls.GrenadeAimSpeed, 0f, 2f, 0.1f);
		AddToggle("Swim Mouse Aim", RandomizerSettings.Controls.SwimmingMouseAim);
		AddToggle("Invert Swim", RandomizerSettings.Controls.InvertSwim);
		AddToggle("Wall Charge Jump Mouse Aim", RandomizerSettings.Controls.WallChargeMouseAim);
		AddToggle("Invert Climb", RandomizerSettings.Controls.InvertClimb);
		AddToggle("Slow Climb Vault", RandomizerSettings.Controls.SlowClimbVault);
	}
}