using System;
using Core;
using UnityEngine;

public class ToggleCustomSettingsAction : MonoBehaviour
{
	public void Awake()
	{
		ToggleSettingsAction componentInChildren = base.GetComponentInChildren<ToggleSettingsAction>();
		this.OnSound = componentInChildren.OnSound;
		this.OffSound = componentInChildren.OffSound;
		UnityEngine.Object.Destroy(componentInChildren);
	}

	private void PlaySound(bool on)
	{
		if (on && this.OnSound)
		{
			Sound.Play(this.OnSound.GetSound(null), base.transform.position, null);
			return;
		}
		if (this.OffSound && !on)
		{
			Sound.Play(this.OffSound.GetSound(null), base.transform.position, null);
		}
	}

	public void Toggle()
	{
		this.SetSetting(!this.IsEnabled);
		this.PlaySound(this.IsEnabled);
		this.Setting.Value = this.IsEnabled;
		RandomizerSettings.SetDirty();
	}

	public void SetSetting(bool enabled)
	{
		this.MessageBox.SetMessage(new MessageDescriptor(enabled ? "ON" : "OFF"));
		this.IsEnabled = enabled;
	}

	public void Init()
	{
		this.MessageBox = base.transform.FindChild("text/stateText").GetComponent<MessageBox>();
		this.SetSetting(this.Setting.Value);
	}

	public SoundProvider OnSound;

	public SoundProvider OffSound;

	public MessageBox MessageBox;

	public bool IsEnabled;

	public RandomizerSettings.BoolSetting Setting;
}
