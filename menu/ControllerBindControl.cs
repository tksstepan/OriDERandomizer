using System;
using System.Collections.Generic;
using SmartInput;
using UnityEngine;

public class ControllerBindControl : MonoBehaviour
{
	public void Awake()
	{
		this.messageBox = base.transform.Find("text/stateText").GetComponent<MessageBox>();
	}

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
		this.tooltipProvider.SetMessage("Start: finish editing");
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
		if (Input.GetKeyDown(KeyCode.Escape) || (this.WasPressed(XboxControllerInput.Button.Start) && this.currentKeys.Count > 0))
		{
			this.editing = false;
			SuspensionManager.ResumeAll();
			this.SetKeys(this.currentKeys.ToArray());
			PlayerInputRebinding.WriteControllerRebindSettings();
			PlayerInput.Instance.RefreshControlScheme();
			this.tooltipProvider.SetMessage(this.owner.DefaultTooltip);
			this.owner.tooltipController.UpdateTooltip();
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

	public void UpdateMessageBox()
	{
		this.messageBox.SetMessage(new MessageDescriptor(ControllerBindControl.KeyBindingToString(this.currentKeys.ToArray())));
	}

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

	public void Reset()
	{
		this.messageBox.SetMessage(new MessageDescriptor(ControllerBindControl.KeyBindingToString(this.GetKeys())));
		this.editing = false;
	}

	private bool WasPressed(XboxControllerInput.Button button)
	{
		return !this.buttonsPressed[(int)button] && XboxControllerInput.GetButton(button, -1);
	}

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

	public void Init(Func<PlayerInputRebinding.ControllerButton[]> getKeys, Action<PlayerInputRebinding.ControllerButton[]> setKeys, CustomSettingsScreen owner)
	{
		this.owner = owner;
		this.GetKeys = getKeys;
		this.SetKeys = setKeys;
		this.messageBox.SetMessage(new MessageDescriptor(ControllerBindControl.KeyBindingToString(getKeys())));
		CleverMenuItemTooltip component = base.GetComponent<CleverMenuItemTooltip>();
		this.tooltipProvider = ScriptableObject.CreateInstance<RandomizerMessageProvider>();
		this.tooltipProvider.SetMessage(owner.DefaultTooltip);
		component.Tooltip = this.tooltipProvider;
		owner.tooltipController.UpdateTooltip();
	}

	public Func<PlayerInputRebinding.ControllerButton[]> GetKeys;

	public Action<PlayerInputRebinding.ControllerButton[]> SetKeys;

	public bool editing;

	public MessageBox messageBox;

	public List<PlayerInputRebinding.ControllerButton> currentKeys = new List<PlayerInputRebinding.ControllerButton>();

	public int exit;

	private bool[] buttonsPressed;

	private XboxControllerInput.Button[] allButtons;

	private CustomSettingsScreen owner;

	private RandomizerMessageProvider tooltipProvider;
}
