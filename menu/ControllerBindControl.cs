using System;
using System.Collections.Generic;
using SmartInput;
using UnityEngine;

// Token: 0x02000A49 RID: 2633
public class ControllerBindControl : MonoBehaviour
{
	// Token: 0x060038FF RID: 14591 RVA: 0x0002C38B File Offset: 0x0002A58B
	public void Awake()
	{
		this.messageBox = base.transform.Find("text/stateText").GetComponent<MessageBox>();
	}

	// Token: 0x06003900 RID: 14592 RVA: 0x000E770C File Offset: 0x000E590C
	public void BeginEditing()
	{
		this.currentKeys.Clear();
		this.UpdateMessageBox();
		SuspensionManager.SuspendAll();
		this.editing = true;
		this.exit = 0;
		this.allButtons = (XboxControllerInput.Button[])Enum.GetValues(typeof(XboxControllerInput.Button));
		this.buttonsPressed = new bool[this.allButtons.Length];
		for (int i = 0; i < this.buttonsPressed.Length; i++)
		{
			this.buttonsPressed[i] = true;
		}
	}

	// Token: 0x06003901 RID: 14593 RVA: 0x000E7788 File Offset: 0x000E5988
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
		if (Input.GetKeyDown(KeyCode.Escape) || (this.WasPressed(XboxControllerInput.Button.Start) && this.currentKeys.Count > 0))
		{
			this.editing = false;
			SuspensionManager.ResumeAll();
			this.SetKeys(this.currentKeys.ToArray());
			PlayerInputRebinding.WriteControllerRebindSettings();
			PlayerInput.Instance.RefreshControlScheme();
			return;
		}
		PlayerInputRebinding.ControllerButton? pressedButtonAsBind = this.GetPressedButtonAsBind();
		if (pressedButtonAsBind != null && !this.currentKeys.Contains(pressedButtonAsBind.Value))
		{
			this.currentKeys.Add(pressedButtonAsBind.Value);
			this.UpdateMessageBox();
		}
		foreach (XboxControllerInput.Button button in this.allButtons)
		{
			this.buttonsPressed[(int)button] = XboxControllerInput.GetButton(button, -1);
		}
	}

	// Token: 0x06003903 RID: 14595 RVA: 0x0002C3BB File Offset: 0x0002A5BB
	public void UpdateMessageBox()
	{
		this.messageBox.SetMessage(new MessageDescriptor(ControllerBindControl.KeyBindingToString(this.currentKeys.ToArray())));
	}

	// Token: 0x06003904 RID: 14596 RVA: 0x0005D2C8 File Offset: 0x0005B4C8
	public static string KeyBindingToString(PlayerInputRebinding.ControllerButton[] codes)
	{
		string text = string.Empty;
		bool flag = true;
		foreach (PlayerInputRebinding.ControllerButton controllerButton in codes)
		{
			text += ((!flag) ? ", " : string.Empty);
			text += controllerButton;
			flag = false;
		}
		return text;
	}

	// Token: 0x06003905 RID: 14597 RVA: 0x0002C3DD File Offset: 0x0002A5DD
	public void Init(Func<PlayerInputRebinding.ControllerButton[]> getKeys, Action<PlayerInputRebinding.ControllerButton[]> setKeys)
	{
		this.GetKeys = getKeys;
		this.SetKeys = setKeys;
		this.messageBox.SetMessage(new MessageDescriptor(ControllerBindControl.KeyBindingToString(getKeys())));
	}

	// Token: 0x06003907 RID: 14599 RVA: 0x0002C449 File Offset: 0x0002A649
	public void Reset()
	{
		this.messageBox.SetMessage(new MessageDescriptor(ControllerBindControl.KeyBindingToString(this.GetKeys())));
		this.editing = false;
	}

	// Token: 0x06003908 RID: 14600 RVA: 0x0002C472 File Offset: 0x0002A672
	private bool WasPressed(XboxControllerInput.Button button)
	{
		return !this.buttonsPressed[(int)button] && XboxControllerInput.GetButton(button, -1);
	}

	// Token: 0x06003909 RID: 14601 RVA: 0x000E7864 File Offset: 0x000E5A64
	private PlayerInputRebinding.ControllerButton ToBind(XboxControllerInput.Button button)
	{
		switch (button)
		{
		case XboxControllerInput.Button.ButtonA:
			return PlayerInputRebinding.ControllerButton.A;
		case XboxControllerInput.Button.ButtonX:
			return PlayerInputRebinding.ControllerButton.X;
		case XboxControllerInput.Button.ButtonY:
			return PlayerInputRebinding.ControllerButton.Y;
		case XboxControllerInput.Button.ButtonB:
			return PlayerInputRebinding.ControllerButton.B;
		case XboxControllerInput.Button.LeftTrigger:
			return PlayerInputRebinding.ControllerButton.LT;
		case XboxControllerInput.Button.RightTrigger:
			return PlayerInputRebinding.ControllerButton.RT;
		case XboxControllerInput.Button.LeftShoulder:
			return PlayerInputRebinding.ControllerButton.LB;
		case XboxControllerInput.Button.RightShoulder:
			return PlayerInputRebinding.ControllerButton.RB;
		case XboxControllerInput.Button.LeftStick:
			return PlayerInputRebinding.ControllerButton.LS;
		case XboxControllerInput.Button.RightStick:
			return PlayerInputRebinding.ControllerButton.RS;
		case XboxControllerInput.Button.Select:
			return PlayerInputRebinding.ControllerButton.Back;
		case XboxControllerInput.Button.Start:
			return PlayerInputRebinding.ControllerButton.Start;
		default:
			return PlayerInputRebinding.ControllerButton.A;
		}
	}

	// Token: 0x0600390A RID: 14602 RVA: 0x000E78C8 File Offset: 0x000E5AC8
	public PlayerInputRebinding.ControllerButton? GetPressedButtonAsBind()
	{
		foreach (XboxControllerInput.Button button in this.allButtons)
		{
			if (this.WasPressed(button))
			{
				return new PlayerInputRebinding.ControllerButton?(this.ToBind(button));
			}
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.LeftStickX) < -0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.LLeft);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.LeftStickX) > 0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.LRight);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.LeftStickY) > 0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.LUp);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.LeftStickY) < -0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.LDown);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX) < -0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.RLeft);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX) > 0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.RRight);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickY) > 0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.RUp);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickY) < -0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.RDown);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadX) < -0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.DLeft);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadX) > 0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.DRight);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) > 0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.DUp);
		}
		if (XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) < -0.5f)
		{
			return new PlayerInputRebinding.ControllerButton?(PlayerInputRebinding.ControllerButton.DDown);
		}
		return null;
	}

	// Token: 0x04003392 RID: 13202
	public Func<PlayerInputRebinding.ControllerButton[]> GetKeys;

	// Token: 0x04003393 RID: 13203
	public Action<PlayerInputRebinding.ControllerButton[]> SetKeys;

	// Token: 0x04003394 RID: 13204
	public bool editing;

	// Token: 0x04003395 RID: 13205
	public MessageBox messageBox;

	// Token: 0x04003396 RID: 13206
	public List<PlayerInputRebinding.ControllerButton> currentKeys = new List<PlayerInputRebinding.ControllerButton>();

	// Token: 0x04003398 RID: 13208
	public int exit;

	// Token: 0x0400339B RID: 13211
	private bool[] buttonsPressed;

	// Token: 0x0400339C RID: 13212
	private XboxControllerInput.Button[] allButtons;
}
