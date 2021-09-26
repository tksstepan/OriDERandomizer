using System;
using System.Collections.Generic;
using UnityEngine;

public class KeybindControl : MonoBehaviour
{
	private void Awake()
	{
		this.messageBox = base.transform.Find("text/stateText").GetComponent<MessageBox>();
	}

	public void BeginEditing()
	{
		this.currentKeys.Clear();
		this.currentKeys.AddRange(this.GetKeys());
		SuspensionManager.SuspendAll();
		this.editing = true;
		this.exit = 0;
		this.tooltipProvider.SetMessage("Backspace: remove bind\nEnter: finish editing");
		this.owner.tooltipController.UpdateTooltip();
	}

	public void Update()
	{
		if (!this.editing)
		{
			return;
		}
		if (this.exit < 2)
		{
			this.exit++;
			return;
		}
		if (Input.GetKeyDown(KeyCode.Return) && this.currentKeys.Count > 0)
		{
			this.editing = false;
			SuspensionManager.ResumeAll();
			this.SetKeys(this.currentKeys.ToArray());
			PlayerInputRebinding.WriteKeyRebindSettings();
			PlayerInput.Instance.RefreshControlScheme();
			this.tooltipProvider.SetMessage(this.owner.DefaultTooltip);
			this.owner.tooltipController.UpdateTooltip();
			return;
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			if (this.currentKeys.Count > 0)
			{
				this.currentKeys.RemoveAt(this.currentKeys.Count - 1);
				this.UpdateMessageBox();
				return;
			}
		}
		else if (Input.anyKeyDown)
		{
			foreach (object obj in Enum.GetValues(typeof(KeyCode)))
			{
				KeyCode keyCode = (KeyCode)obj;
				if (Input.GetKeyDown(keyCode) && !this.currentKeys.Contains(keyCode))
				{
					this.currentKeys.Add(keyCode);
					this.UpdateMessageBox();
				}
			}
		}
	}

	private void UpdateMessageBox()
	{
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(this.currentKeys.ToArray())));
	}

	public static string KeyBindingToString(KeyCode[] codes)
	{
		string text = string.Empty;
		bool flag = true;
		foreach (KeyCode keyCode in codes)
		{
			text += ((!flag) ? ", " : string.Empty);
			text += keyCode;
			flag = false;
		}
		return text;
	}

	public void Reset()
	{
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(this.GetKeys())));
		this.editing = false;
	}

	public void Init(Func<KeyCode[]> getKeys, Action<KeyCode[]> setKeys, CustomSettingsScreen owner)
	{
		this.owner = owner;
		this.GetKeys = getKeys;
		this.SetKeys = setKeys;
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(getKeys())));
		CleverMenuItemTooltip component = base.GetComponent<CleverMenuItemTooltip>();
		this.tooltipProvider = ScriptableObject.CreateInstance<RandomizerMessageProvider>();
		this.tooltipProvider.SetMessage(owner.DefaultTooltip);
		component.Tooltip = this.tooltipProvider;
		owner.tooltipController.UpdateTooltip();
	}

	private Func<KeyCode[]> GetKeys;

	private Action<KeyCode[]> SetKeys;

	private bool editing;

	private MessageBox messageBox;

	private List<KeyCode> currentKeys = new List<KeyCode>();

	private int exit;

	private CustomSettingsScreen owner;

	private RandomizerMessageProvider tooltipProvider;
}
