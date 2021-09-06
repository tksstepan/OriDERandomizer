using System;
using System.Collections.Generic;
using Core;
using Game;
using SmartInput;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
	public PlayerInput()
	{
		this.m_lastPressedButtonInput = -1;
		this.m_lastPressedAxisInput = -1;
	}

	public void ClearControls()
	{
		this.HorizontalAnalogLeft.Clear();
		this.VerticalAnalogLeft.Clear();
		this.HorizontalAnalogRight.Clear();
		this.VerticalAnalogRight.Clear();
		this.HorizontalDigiPad.Clear();
		this.VerticalDigiPad.Clear();
		this.Jump.Clear();
		this.SpiritFlame.Clear();
		this.SoulFlame.Clear();
		this.Bash.Clear();
		this.ChargeJump.Clear();
		this.Glide.Clear();
		this.Grab.Clear();
		this.LeftShoulder.Clear();
		this.RightShoulder.Clear();
		this.Select.Clear();
		this.Start.Clear();
		this.LeftStick.Clear();
		this.RightStick.Clear();
		this.MenuDown.Clear();
		this.MenuUp.Clear();
		this.MenuLeft.Clear();
		this.MenuRight.Clear();
		this.MenuPageLeft.Clear();
		this.MenuPageRight.Clear();
		this.ActionButtonA.Clear();
		this.ZoomIn.Clear();
		this.ZoomOut.Clear();
		this.Cancel.Clear();
		this.Copy.Clear();
		this.Delete.Clear();
		this.Focus.Clear();
		this.Filter.Clear();
		this.Legend.Clear();
		this.Stomp.Clear();
	}

	public void AddXboxOneControls()
	{
	}

	public void AddControllerControls()
	{
		this.HorizontalAnalogLeft.Add(new ControllerAxisInput(XboxControllerInput.Axis.LeftStickX));
		this.VerticalAnalogLeft.Add(new ControllerAxisInput(XboxControllerInput.Axis.LeftStickY));
		this.HorizontalAnalogRight.Add(new ControllerAxisInput(XboxControllerInput.Axis.RightStickX));
		this.VerticalAnalogRight.Add(new ControllerAxisInput(XboxControllerInput.Axis.RightStickY));
		PlayerInputRebinding.ControllerBindingSettings controllerRebindings = PlayerInputRebinding.ControllerRebindings;
		foreach (PlayerInputRebinding.ControllerButton button in controllerRebindings.HorizontalDigiPadLeft)
		{
			this.HorizontalDigiPad.Add(new ButtonAxisInput(this.ControllerButtonToButtonInput(button), ButtonAxisInput.Mode.Negative));
		}
		foreach (PlayerInputRebinding.ControllerButton button2 in controllerRebindings.HorizontalDigiPadRight)
		{
			this.HorizontalDigiPad.Add(new ButtonAxisInput(this.ControllerButtonToButtonInput(button2), ButtonAxisInput.Mode.Positive));
		}
		foreach (PlayerInputRebinding.ControllerButton button3 in controllerRebindings.VerticalDigiPadDown)
		{
			this.VerticalDigiPad.Add(new ButtonAxisInput(this.ControllerButtonToButtonInput(button3), ButtonAxisInput.Mode.Negative));
		}
		foreach (PlayerInputRebinding.ControllerButton button4 in controllerRebindings.VerticalDigiPadUp)
		{
			this.VerticalDigiPad.Add(new ButtonAxisInput(this.ControllerButtonToButtonInput(button4), ButtonAxisInput.Mode.Positive));
		}
		this.AddControllerButtonsToButtonInput(controllerRebindings.Jump, this.Jump);
		this.AddControllerButtonsToButtonInput(controllerRebindings.SpiritFlame, this.SpiritFlame);
		this.AddControllerButtonsToButtonInput(controllerRebindings.SoulFlame, this.SoulFlame);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Bash, this.Bash);
		this.AddControllerButtonsToButtonInput(controllerRebindings.ChargeJump, this.ChargeJump);
		this.AddControllerButtonsToButtonInput(controllerRebindings.ZoomIn, this.ZoomIn);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Glide, this.Glide);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Grab, this.Grab);
		this.AddControllerButtonsToButtonInput(controllerRebindings.ZoomOut, this.ZoomOut);
		this.AddControllerButtonsToButtonInput(controllerRebindings.LeftShoulder, this.LeftShoulder);
		this.AddControllerButtonsToButtonInput(controllerRebindings.RightShoulder, this.RightShoulder);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Select, this.Select);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Start, this.Start);
		this.AddControllerButtonsToButtonInput(controllerRebindings.LeftStick, this.LeftStick);
		this.AddControllerButtonsToButtonInput(controllerRebindings.RightStick, this.RightStick);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuDown, this.MenuDown);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuDown, this.MenuDown);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuUp, this.MenuUp);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuUp, this.MenuUp);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuLeft, this.MenuLeft);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuLeft, this.MenuLeft);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuRight, this.MenuRight);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuRight, this.MenuRight);
		this.AddControllerButtonsToButtonInput(controllerRebindings.ActionButtonA, this.ActionButtonA);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Cancel, this.Cancel);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuPageLeft, this.MenuPageLeft);
		this.AddControllerButtonsToButtonInput(controllerRebindings.MenuPageRight, this.MenuPageRight);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Copy, this.Copy);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Delete, this.Delete);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Focus, this.Focus);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Filter, this.Filter);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Legend, this.Legend);
		this.AddControllerButtonsToButtonInput(controllerRebindings.Stomp, this.Stomp);
	}

	public void AddKeyboardControls()
	{
		PlayerInputRebinding.KeyBindingSettings keyRebindings = PlayerInputRebinding.KeyRebindings;
		foreach (KeyCode keyCode in keyRebindings.HorizontalDigiPadLeft)
		{
			this.HorizontalDigiPad.Add(new ButtonAxisInput(new KeyCodeButtonInput(keyCode), ButtonAxisInput.Mode.Negative));
		}
		foreach (KeyCode keyCode2 in keyRebindings.HorizontalDigiPadRight)
		{
			this.HorizontalDigiPad.Add(new ButtonAxisInput(new KeyCodeButtonInput(keyCode2), ButtonAxisInput.Mode.Positive));
		}
		foreach (KeyCode keyCode3 in keyRebindings.VerticalDigiPadDown)
		{
			this.VerticalDigiPad.Add(new ButtonAxisInput(new KeyCodeButtonInput(keyCode3), ButtonAxisInput.Mode.Negative));
		}
		foreach (KeyCode keyCode4 in keyRebindings.VerticalDigiPadUp)
		{
			this.VerticalDigiPad.Add(new ButtonAxisInput(new KeyCodeButtonInput(keyCode4), ButtonAxisInput.Mode.Positive));
		}
		this.AddKeyCodesToButtonInput(keyRebindings.MenuLeft, this.MenuLeft);
		this.AddKeyCodesToButtonInput(keyRebindings.MenuRight, this.MenuRight);
		this.AddKeyCodesToButtonInput(keyRebindings.MenuDown, this.MenuDown);
		this.AddKeyCodesToButtonInput(keyRebindings.MenuUp, this.MenuUp);
		this.AddKeyCodesToButtonInput(keyRebindings.MenuPageLeft, this.MenuPageLeft);
		this.AddKeyCodesToButtonInput(keyRebindings.MenuPageRight, this.MenuPageRight);
		this.AddKeyCodesToButtonInput(keyRebindings.ActionButtonA, this.ActionButtonA);
		this.AddKeyCodesToButtonInput(keyRebindings.SoulFlame, this.SoulFlame);
		this.AddKeyCodesToButtonInput(keyRebindings.Jump, this.Jump);
		this.AddKeyCodesToButtonInput(keyRebindings.Grab, this.Grab);
		this.AddKeyCodesToButtonInput(keyRebindings.SpiritFlame, this.SpiritFlame);
		this.AddKeyCodesToButtonInput(keyRebindings.Bash, this.Bash);
		this.AddKeyCodesToButtonInput(keyRebindings.Glide, this.Glide);
		this.AddKeyCodesToButtonInput(keyRebindings.ChargeJump, this.ChargeJump);
		this.AddKeyCodesToButtonInput(keyRebindings.Select, this.Select);
		this.AddKeyCodesToButtonInput(keyRebindings.Start, this.Start);
		this.AddKeyCodesToButtonInput(keyRebindings.Cancel, this.Cancel);
		this.AddKeyCodesToButtonInput(keyRebindings.LeftShoulder, this.LeftShoulder);
		this.AddKeyCodesToButtonInput(keyRebindings.RightShoulder, this.RightShoulder);
		this.AddKeyCodesToButtonInput(keyRebindings.LeftStick, this.LeftStick);
		this.AddKeyCodesToButtonInput(keyRebindings.RightStick, this.RightStick);
		this.AddKeyCodesToButtonInput(keyRebindings.ZoomIn, this.ZoomIn);
		this.AddKeyCodesToButtonInput(keyRebindings.ZoomOut, this.ZoomOut);
		this.AddKeyCodesToButtonInput(keyRebindings.Copy, this.Copy);
		this.AddKeyCodesToButtonInput(keyRebindings.Delete, this.Delete);
		this.AddKeyCodesToButtonInput(keyRebindings.Focus, this.Focus);
		this.AddKeyCodesToButtonInput(keyRebindings.Filter, this.Filter);
		this.AddKeyCodesToButtonInput(keyRebindings.Legend, this.Legend);
		this.AddKeyCodesToButtonInput(keyRebindings.Stomp, this.Stomp);
	}

	private void AddKeyCodesToButtonInput(KeyCode[] keyCodes, CompoundButtonInput buttonInput)
	{
		foreach (KeyCode keyCode in keyCodes)
		{
			buttonInput.Add(new KeyCodeButtonInput(keyCode));
		}
	}

	public void Awake()
	{
		PlayerInput.Instance = this;
		this.RefreshControlScheme();
		this.LeftClick = new KeyCodeButtonInput(KeyCode.Mouse0);
		this.RightClick = new KeyCodeButtonInput(KeyCode.Mouse1);
		this.m_allButtonInput = new List<IButtonInput>
		{
			this.Jump,
			this.SpiritFlame,
			this.SoulFlame,
			this.Bash,
			this.ChargeJump,
			this.Glide,
			this.Grab,
			this.LeftShoulder,
			this.RightShoulder,
			this.Select,
			this.Start,
			this.LeftStick,
			this.RightStick,
			this.MenuDown,
			this.MenuUp,
			this.MenuLeft,
			this.MenuRight,
			this.MenuPageRight,
			this.MenuPageLeft,
			this.ActionButtonA,
			this.Cancel,
			this.Copy,
			this.Delete,
			this.Focus,
			this.Filter,
			this.Legend,
			this.Stomp
		};
		this.m_allButtonProcessor = new List<Core.Input.InputButtonProcessor>
		{
			Core.Input.Jump,
			Core.Input.SpiritFlame,
			Core.Input.SoulFlame,
			Core.Input.Bash,
			Core.Input.ChargeJump,
			Core.Input.Glide,
			Core.Input.Grab,
			Core.Input.LeftShoulder,
			Core.Input.RightShoulder,
			Core.Input.Select,
			Core.Input.Start,
			Core.Input.LeftStick,
			Core.Input.RightStick,
			Core.Input.MenuDown,
			Core.Input.MenuUp,
			Core.Input.MenuLeft,
			Core.Input.MenuRight,
			Core.Input.MenuPageRight,
			Core.Input.MenuPageLeft,
			Core.Input.ActionButtonA,
			Core.Input.Cancel,
			Core.Input.Copy,
			Core.Input.Delete,
			Core.Input.Focus,
			Core.Input.Filter,
			Core.Input.Legend,
			Core.Input.Stomp
		};
		this.m_allAxisInput = new List<IAxisInput>
		{
			this.HorizontalAnalogLeft,
			this.VerticalAnalogLeft,
			this.HorizontalAnalogRight,
			this.VerticalAnalogRight,
			this.HorizontalDigiPad,
			this.VerticalDigiPad
		};
	}

	public float SimplifyAxis(float x)
	{
		return Utility.Round(x, 0.001f);
	}

	public void ApplyDeadzone(ref float x, ref float y)
	{
		if (x * x + y * y < 0.0400000028f)
		{
			x = 0f;
			y = 0f;
		}
	}

	public void FixedUpdate()
	{
		if (!this.Active)
		{
			return;
		}
		Vector2 vector = UI.Cameras.Current.Camera.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
		Core.Input.CursorMoved = (Vector2.Distance(vector, Core.Input.CursorPosition) > 0.0001f);
		Core.Input.CursorPosition = vector;
		Core.Input.HorizontalAnalogLeft = this.SimplifyAxis(this.HorizontalAnalogLeft.AxisValue());
		Core.Input.VerticalAnalogLeft = this.SimplifyAxis(this.VerticalAnalogLeft.AxisValue());
		this.ApplyDeadzone(ref Core.Input.HorizontalAnalogLeft, ref Core.Input.VerticalAnalogLeft);
		Core.Input.HorizontalAnalogRight = this.SimplifyAxis(this.HorizontalAnalogRight.AxisValue());
		Core.Input.VerticalAnalogRight = this.SimplifyAxis(this.VerticalAnalogRight.AxisValue());
		this.ApplyDeadzone(ref Core.Input.HorizontalAnalogRight, ref Core.Input.VerticalAnalogRight);
		Core.Input.HorizontalDigiPad = Mathf.RoundToInt(this.HorizontalDigiPad.AxisValue());
		Core.Input.VerticalDigiPad = Mathf.RoundToInt(this.VerticalDigiPad.AxisValue());
		Core.Input.AnyStart.Update(this.IsAnyStartPressed());
		Core.Input.ZoomIn.Update(this.ZoomIn.GetButton());
		Core.Input.ZoomOut.Update(this.ZoomOut.GetButton());
		Core.Input.LeftClick.Update(this.LeftClick.GetButton());
		Core.Input.RightClick.Update(this.RightClick.GetButton());
		this.m_lastPressedButtonInput = -1;
		for (int i = 0; i < this.m_allButtonInput.Count; i++)
		{
			bool button = this.m_allButtonInput[i].GetButton();
			if (button)
			{
				this.m_lastPressedButtonInput = i;
			}
			this.m_allButtonProcessor[i].Update(button);
		}
		this.RefreshControls();
		if (!ControlsScreen.IsVisible && this.m_lastPressedButtonInput != -1)
		{
			bool flag = this.WasKeyboardUsedLast;
			if (this.m_lastPressedButtonInput != -1)
			{
				flag = this.KeyboardUsedLast(this.m_allButtonInput[this.m_lastPressedButtonInput]);
			}
			if (flag != this.WasKeyboardUsedLast)
			{
				GameSettings.Instance.CurrentControlScheme = ((!flag) ? ControlScheme.Controller : GameSettings.Instance.KeyboardScheme);
			}
		}
	}

	public void RefreshControls()
	{
		Core.Input.Horizontal = Mathf.Clamp((float)Core.Input.HorizontalDigiPad + Core.Input.HorizontalAnalogLeft, -1f, 1f);
		Core.Input.Vertical = Mathf.Clamp((float)Core.Input.VerticalDigiPad + Core.Input.VerticalAnalogLeft, -1f, 1f);
		Core.Input.Down.Update(Core.Input.NormalizedVertical == -1f);
		Core.Input.Up.Update(Core.Input.NormalizedVertical == 1f);
		Core.Input.Left.Update(Core.Input.NormalizedHorizontal == -1);
		Core.Input.Right.Update(Core.Input.NormalizedHorizontal == 1);
		for (int i = 0; i < Core.Input.Buttons.Length; i++)
		{
			Core.Input.Buttons[i].Used = false;
		}
		RandomizerRebinding.FixedUpdate();
	}

	public void RefreshControlScheme()
	{
		this.ClearControls();
		this.AddControllerControls();
		this.AddXboxOneControls();
		this.AddKeyboardControls();
		PlayerInputRebinding.RefreshControllerButtonRemappings();
	}

	private void RefreshLastPressedButton()
	{
		this.m_lastPressedButtonInput = -1;
		this.m_lastPressedAxisInput = -1;
		for (int i = 0; i < this.m_allButtonInput.Count; i++)
		{
			if (this.m_allButtonInput[i].GetButton())
			{
				this.m_lastPressedButtonInput = i;
				return;
			}
		}
	}

	public bool WasKeyboardUsedLast
	{
		get
		{
			return GameSettings.Instance.CurrentControlScheme > ControlScheme.Controller;
		}
	}

	private bool KeyboardUsedLast(IButtonInput iButtonInput)
	{
		if (iButtonInput is KeyCodeButtonInput)
		{
			return true;
		}
		AxisButtonInput axisButtonInput = iButtonInput as AxisButtonInput;
		if (axisButtonInput != null)
		{
			return this.KeyboardUsedLast(axisButtonInput.GetAxisInput());
		}
		CompoundButtonInput compoundButtonInput = iButtonInput as CompoundButtonInput;
		if (compoundButtonInput != null)
		{
			return this.KeyboardUsedLast(compoundButtonInput.GetLastPressed());
		}
		return iButtonInput is ControllerButtonInput && false;
	}

	private bool KeyboardUsedLast(IAxisInput iAxisInput)
	{
		if (iAxisInput is ButtonAxisInput)
		{
			return this.KeyboardUsedLast((iAxisInput as ButtonAxisInput).GetButtonInput());
		}
		if (iAxisInput is CompoundAxisInput)
		{
			return this.KeyboardUsedLast((iAxisInput as CompoundAxisInput).GetLastPressed());
		}
		return iAxisInput is ControllerAxisInput && false;
	}

	private bool IsAnyStartPressed()
	{
		return XboxControllerInput.GetButton(XboxControllerInput.Button.Start, -1) || XboxControllerInput.GetButton(XboxControllerInput.Button.ButtonA, -1) || XboxControllerInput.GetButton(XboxControllerInput.Button.ButtonB, -1) || XboxControllerInput.GetButton(XboxControllerInput.Button.ButtonX, -1) || XboxControllerInput.GetButton(XboxControllerInput.Button.ButtonY, -1) || MoonInput.GetKey(KeyCode.Space) || MoonInput.GetKey(KeyCode.X) || MoonInput.GetKey(KeyCode.Mouse0) || MoonInput.GetKey(KeyCode.Return) || MoonInput.GetKey(KeyCode.Escape) || MoonInput.anyKey;
	}

	public void AddControllerButtonsToButtonInput(PlayerInputRebinding.ControllerButton[] buttons, CompoundButtonInput buttonInput)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			buttonInput.Add(this.ControllerButtonToButtonInput(buttons[i]));
		}
	}

	public IButtonInput ControllerButtonToButtonInput(PlayerInputRebinding.ControllerButton button)
	{
		switch (button)
		{
		case PlayerInputRebinding.ControllerButton.A:
			return new ControllerButtonInput(XboxControllerInput.Button.ButtonA);
		case PlayerInputRebinding.ControllerButton.B:
			return new ControllerButtonInput(XboxControllerInput.Button.ButtonB);
		case PlayerInputRebinding.ControllerButton.X:
			return new ControllerButtonInput(XboxControllerInput.Button.ButtonX);
		case PlayerInputRebinding.ControllerButton.Y:
			return new ControllerButtonInput(XboxControllerInput.Button.ButtonY);
		case PlayerInputRebinding.ControllerButton.LT:
			return new ControllerButtonInput(XboxControllerInput.Button.LeftTrigger);
		case PlayerInputRebinding.ControllerButton.RT:
			return new ControllerButtonInput(XboxControllerInput.Button.RightTrigger);
		case PlayerInputRebinding.ControllerButton.LB:
			return new ControllerButtonInput(XboxControllerInput.Button.LeftShoulder);
		case PlayerInputRebinding.ControllerButton.RB:
			return new ControllerButtonInput(XboxControllerInput.Button.RightShoulder);
		case PlayerInputRebinding.ControllerButton.LS:
			return new ControllerButtonInput(XboxControllerInput.Button.LeftStick);
		case PlayerInputRebinding.ControllerButton.RS:
			return new ControllerButtonInput(XboxControllerInput.Button.RightStick);
		case PlayerInputRebinding.ControllerButton.LUp:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.LeftStickY), AxisButtonInput.AxisMode.GreaterThan, 0.5f);
		case PlayerInputRebinding.ControllerButton.LDown:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.LeftStickY), AxisButtonInput.AxisMode.LessThan, -0.5f);
		case PlayerInputRebinding.ControllerButton.LLeft:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.LeftStickX), AxisButtonInput.AxisMode.LessThan, -0.5f);
		case PlayerInputRebinding.ControllerButton.LRight:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.LeftStickX), AxisButtonInput.AxisMode.GreaterThan, 0.5f);
		case PlayerInputRebinding.ControllerButton.DUp:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.DpadY), AxisButtonInput.AxisMode.GreaterThan, 0.5f);
		case PlayerInputRebinding.ControllerButton.DDown:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.DpadY), AxisButtonInput.AxisMode.LessThan, -0.5f);
		case PlayerInputRebinding.ControllerButton.DLeft:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.DpadX), AxisButtonInput.AxisMode.LessThan, -0.5f);
		case PlayerInputRebinding.ControllerButton.DRight:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.DpadX), AxisButtonInput.AxisMode.GreaterThan, 0.5f);
		case PlayerInputRebinding.ControllerButton.RUp:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.RightStickY), AxisButtonInput.AxisMode.GreaterThan, 0.5f);
		case PlayerInputRebinding.ControllerButton.RDown:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.RightStickY), AxisButtonInput.AxisMode.LessThan, -0.5f);
		case PlayerInputRebinding.ControllerButton.RLeft:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.RightStickX), AxisButtonInput.AxisMode.LessThan, -0.5f);
		case PlayerInputRebinding.ControllerButton.RRight:
			return new AxisButtonInput(new ControllerAxisInput(XboxControllerInput.Axis.RightStickX), AxisButtonInput.AxisMode.GreaterThan, 0.5f);
		case PlayerInputRebinding.ControllerButton.Back:
			return new ControllerButtonInput(XboxControllerInput.Button.Select);
		case PlayerInputRebinding.ControllerButton.Start:
			return new ControllerButtonInput(XboxControllerInput.Button.Start);
		default:
			return null;
		}
	}

	public static PlayerInput Instance;

	public bool Active = true;

	public CompoundAxisInput HorizontalAnalogLeft = new CompoundAxisInput();

	public CompoundAxisInput VerticalAnalogLeft = new CompoundAxisInput();

	public CompoundAxisInput HorizontalAnalogRight = new CompoundAxisInput();

	public CompoundAxisInput VerticalAnalogRight = new CompoundAxisInput();

	public CompoundAxisInput HorizontalDigiPad = new CompoundAxisInput();

	public CompoundAxisInput VerticalDigiPad = new CompoundAxisInput();

	public CompoundButtonInput Jump = new CompoundButtonInput();

	public CompoundButtonInput SpiritFlame = new CompoundButtonInput();

	public CompoundButtonInput SoulFlame = new CompoundButtonInput();

	public CompoundButtonInput Bash = new CompoundButtonInput();

	public CompoundButtonInput ChargeJump = new CompoundButtonInput();

	public CompoundButtonInput Glide = new CompoundButtonInput();

	public CompoundButtonInput Grab = new CompoundButtonInput();

	public CompoundButtonInput ZoomIn = new CompoundButtonInput();

	public CompoundButtonInput ZoomOut = new CompoundButtonInput();

	public CompoundButtonInput LeftShoulder = new CompoundButtonInput();

	public CompoundButtonInput RightShoulder = new CompoundButtonInput();

	public CompoundButtonInput Select = new CompoundButtonInput();

	public CompoundButtonInput Start = new CompoundButtonInput();

	public CompoundButtonInput LeftStick = new CompoundButtonInput();

	public CompoundButtonInput RightStick = new CompoundButtonInput();

	public CompoundButtonInput MenuDown = new CompoundButtonInput();

	public CompoundButtonInput MenuUp = new CompoundButtonInput();

	public CompoundButtonInput MenuLeft = new CompoundButtonInput();

	public CompoundButtonInput MenuRight = new CompoundButtonInput();

	public CompoundButtonInput MenuPageLeft = new CompoundButtonInput();

	public CompoundButtonInput MenuPageRight = new CompoundButtonInput();

	public CompoundButtonInput ActionButtonA = new CompoundButtonInput();

	public CompoundButtonInput Cancel = new CompoundButtonInput();

	public CompoundButtonInput Copy = new CompoundButtonInput();

	public CompoundButtonInput Delete = new CompoundButtonInput();

	public CompoundButtonInput Focus = new CompoundButtonInput();

	public CompoundButtonInput Filter = new CompoundButtonInput();

	public CompoundButtonInput Legend = new CompoundButtonInput();

	public IButtonInput LeftClick;

	public IButtonInput RightClick;

	public List<IButtonInput> m_allButtonInput;

	public List<Core.Input.InputButtonProcessor> m_allButtonProcessor;

	public List<IAxisInput> m_allAxisInput;

	private int m_lastPressedButtonInput;

	private int m_lastPressedAxisInput;

	public CompoundButtonInput Stomp = new CompoundButtonInput();
}
