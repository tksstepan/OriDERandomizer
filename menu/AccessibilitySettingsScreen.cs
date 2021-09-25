using System;

public class AccessibilitySettingsScreen : CustomSettingsScreen
{
	public override void InitScreen()
	{
		AddSlider("Camera Shake", RandomizerSettings.Accessibility.CameraShakeFactor, 0f, 1f, 0.1f);
		AddToggle("Sound Compression", RandomizerSettings.Accessibility.ApplySoundCompression);
		AddSlider("Sound Compression Factor", RandomizerSettings.Accessibility.SoundCompressionFactor, 0f, 1f, 0.1f);
		AddSlider("Ability Menu Opacity", RandomizerSettings.QOL.AbilityMenuOpacity, 0f, 1f, 0.1f);
		AddToggle("Cursor Lock", RandomizerSettings.QOL.CursorLock);
	}
}
