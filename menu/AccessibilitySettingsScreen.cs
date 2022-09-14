using System;

public class AccessibilitySettingsScreen : CustomSettingsScreen
{
	public override void InitScreen()
	{
		AddSlider(RandomizerSettings.Accessibility.CameraShakeFactor, 0f, 1f, 0.1f, "Allows adjusting the strength of camera shake effects (0% - 100%).");
		AddToggle(RandomizerSettings.Accessibility.ApplySoundCompression, "Toggles the sound compression effect, reducing the volume of loud sounds while increasing the volume of quiet sounds.");
		AddSlider(RandomizerSettings.Accessibility.SoundCompressionFactor, 0f, 1f, 0.1f, "Allows changing the scale of sound compression effect (0% - 100%).");
		AddSlider(RandomizerSettings.QOL.AbilityMenuOpacity, 0f, 1f, 0.1f, "Allows changing the opacity of the ability menu while performing Save Anywhere (0% - 100%).");
		AddToggle(RandomizerSettings.QOL.CursorLock, "Toggles whether the cursor should remain locked to the screen.");
	}
}
