using System;
using Core;
using UnityEngine;

public class ToggleCustomSettingsAction : MonoBehaviour
{
	public void Awake()
	{
		ToggleSettingsAction tsa = base.GetComponentInChildren<ToggleSettingsAction>();
		this.OnSound = tsa.OnSound;
		this.OffSound = tsa.OffSound;
		UnityEngine.Object.Destroy(tsa);
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
		this.SetFunc(this.IsEnabled);
	}

	public void SetSetting(bool enabled)
	{
		if (!this.MessageBox)
		{
			this.MessageBox = base.transform.FindChild("text/stateText").GetComponent<MessageBox>();
		}
		this.MessageBox.SetMessage(new MessageDescriptor(enabled ? "ON" : "OFF"));
		this.IsEnabled = enabled;
	}

	public void Init(string settingID, bool initialValue, Action<bool> setter)
	{
		this.SettingID = settingID;
		this.SetSetting(initialValue);
		this.SetFunc = setter;
	}

	public string SettingID;

	public SoundProvider OnSound;

	public SoundProvider OffSound;

	public MessageBox MessageBox;

	public bool IsEnabled;

	public Action<bool> SetFunc;
}
