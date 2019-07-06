using System;
using System.Collections.Generic;
using Core;
using Game;
using SmartInput;
using UnityEngine;

// Token: 0x020002F8 RID: 760
public class PlayerInput : MonoBehaviour
{
	// Token: 0x06000EA4 RID: 3748 RVA: 0x0005A5BC File Offset: 0x000587BC
	public PlayerInput()
	{
		this.m_lastPressedButtonInput = -1;
		this.m_lastPressedAxisInput = -1;
	}

	// Token: 0x06000EA5 RID: 3749 RVA: 0x0005A768 File Offset: 0x00058968
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

	// Token: 0x06000EA6 RID: 3750 RVA: 0x000028E7 File Offset: 0x00000AE7
	public void AddXboxOneControls()
	{
	}

	// Token: 0x06000EA7 RID: 3751 RVA: 0x0005A8F8 File Offset: 0x00058AF8
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

	// Token: 0x06000EA8 RID: 3752 RVA: 0x0005AC6C File Offset: 0x00058E6C
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

	// Token: 0x06000EA9 RID: 3753 RVA: 0x0005AF50 File Offset: 0x00059150
	private void AddKeyCodesToButtonInput(KeyCode[] keyCodes, CompoundButtonInput buttonInput)
	{
		foreach (KeyCode keyCode in keyCodes)
		{
			buttonInput.Add(new KeyCodeButtonInput(keyCode));
		}
	}

	// Token: 0x06000EAA RID: 3754 RVA: 0x0005AF80 File Offset: 0x00059180
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

	// Token: 0x06000EAB RID: 3755 RVA: 0x0000CAAC File Offset: 0x0000ACAC
	public float SimplifyAxis(float x)
	{
		return Utility.Round(x, 0.001f);
	}

	// Token: 0x06000EAC RID: 3756 RVA: 0x0000CAB9 File Offset: 0x0000ACB9
	public void ApplyDeadzone(ref float x, ref float y)
	{
		if (x * x + y * y < 0.0400000028f)
		{
			x = 0f;
			y = 0f;
		}
	}

	// Token: 0x06000EAD RID: 3757 RVA: 0x0005B290 File Offset: 0x00059490
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

	// Token: 0x06000EAE RID: 3758 RVA: 0x0005B490 File Offset: 0x00059690
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
	}

	// Token: 0x06000EAF RID: 3759 RVA: 0x0000CADB File Offset: 0x0000ACDB
	public void RefreshControlScheme()
	{
		this.ClearControls();
		this.AddControllerControls();
		this.AddXboxOneControls();
		this.AddKeyboardControls();
		PlayerInputRebinding.RefreshControllerButtonRemappings();
	}

	// Token: 0x06000EB0 RID: 3760 RVA: 0x0005B54C File Offset: 0x0005974C
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

	// Token: 0x1700022E RID: 558
	// (get) Token: 0x06000EB1 RID: 3761 RVA: 0x0000CAFA File Offset: 0x0000ACFA
	public bool WasKeyboardUsedLast
	{
		get
		{
			return GameSettings.Instance.CurrentControlScheme > ControlScheme.Controller;
		}
	}

	// Token: 0x06000EB2 RID: 3762 RVA: 0x0005B598 File Offset: 0x00059798
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

	// Token: 0x06000EB3 RID: 3763 RVA: 0x0005B5E8 File Offset: 0x000597E8
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

	// Token: 0x06000EB4 RID: 3764 RVA: 0x0005B634 File Offset: 0x00059834
	private bool IsAnyStartPressed()
	{
		return XboxControllerInput.GetButton(XboxControllerInput.Button.Start, -1) || XboxControllerInput.GetButton(XboxControllerInput.Button.ButtonA, -1) || XboxControllerInput.GetButton(XboxControllerInput.Button.ButtonB, -1) || XboxControllerInput.GetButton(XboxControllerInput.Button.ButtonX, -1) || XboxControllerInput.GetButton(XboxControllerInput.Button.ButtonY, -1) || MoonInput.GetKey(KeyCode.Space) || MoonInput.GetKey(KeyCode.X) || MoonInput.GetKey(KeyCode.Mouse0) || MoonInput.GetKey(KeyCode.Return) || MoonInput.GetKey(KeyCode.Escape) || MoonInput.anyKey;
	}

	// Token: 0x06000EB5 RID: 3765 RVA: 0x0005B6A8 File Offset: 0x000598A8
	public void AddControllerButtonsToButtonInput(PlayerInputRebinding.ControllerButton[] buttons, CompoundButtonInput buttonInput)
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			buttonInput.Add(this.ControllerButtonToButtonInput(buttons[i]));
		}
	}

	// Token: 0x06000EB6 RID: 3766 RVA: 0x0005B6D4 File Offset: 0x000598D4
	private IButtonInput ControllerButtonToButtonInput(PlayerInputRebinding.ControllerButton button)
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

	// Token: 0x04000DEC RID: 3564
	public static PlayerInput Instance;

	// Token: 0x04000DED RID: 3565
	public bool Active = true;

	// Token: 0x04000DEE RID: 3566
	public CompoundAxisInput HorizontalAnalogLeft = new CompoundAxisInput();

	// Token: 0x04000DEF RID: 3567
	public CompoundAxisInput VerticalAnalogLeft = new CompoundAxisInput();

	// Token: 0x04000DF0 RID: 3568
	public CompoundAxisInput HorizontalAnalogRight = new CompoundAxisInput();

	// Token: 0x04000DF1 RID: 3569
	public CompoundAxisInput VerticalAnalogRight = new CompoundAxisInput();

	// Token: 0x04000DF2 RID: 3570
	public CompoundAxisInput HorizontalDigiPad = new CompoundAxisInput();

	// Token: 0x04000DF3 RID: 3571
	public CompoundAxisInput VerticalDigiPad = new CompoundAxisInput();

	// Token: 0x04000DF4 RID: 3572
	public CompoundButtonInput Jump = new CompoundButtonInput();

	// Token: 0x04000DF5 RID: 3573
	public CompoundButtonInput SpiritFlame = new CompoundButtonInput();

	// Token: 0x04000DF6 RID: 3574
	public CompoundButtonInput SoulFlame = new CompoundButtonInput();

	// Token: 0x04000DF7 RID: 3575
	public CompoundButtonInput Bash = new CompoundButtonInput();

	// Token: 0x04000DF8 RID: 3576
	public CompoundButtonInput ChargeJump = new CompoundButtonInput();

	// Token: 0x04000DF9 RID: 3577
	public CompoundButtonInput Glide = new CompoundButtonInput();

	// Token: 0x04000DFA RID: 3578
	public CompoundButtonInput Grab = new CompoundButtonInput();

	// Token: 0x04000DFB RID: 3579
	public CompoundButtonInput ZoomIn = new CompoundButtonInput();

	// Token: 0x04000DFC RID: 3580
	public CompoundButtonInput ZoomOut = new CompoundButtonInput();

	// Token: 0x04000DFD RID: 3581
	public CompoundButtonInput LeftShoulder = new CompoundButtonInput();

	// Token: 0x04000DFE RID: 3582
	public CompoundButtonInput RightShoulder = new CompoundButtonInput();

	// Token: 0x04000DFF RID: 3583
	public CompoundButtonInput Select = new CompoundButtonInput();

	// Token: 0x04000E00 RID: 3584
	public CompoundButtonInput Start = new CompoundButtonInput();

	// Token: 0x04000E01 RID: 3585
	public CompoundButtonInput LeftStick = new CompoundButtonInput();

	// Token: 0x04000E02 RID: 3586
	public CompoundButtonInput RightStick = new CompoundButtonInput();

	// Token: 0x04000E03 RID: 3587
	public CompoundButtonInput MenuDown = new CompoundButtonInput();

	// Token: 0x04000E04 RID: 3588
	public CompoundButtonInput MenuUp = new CompoundButtonInput();

	// Token: 0x04000E05 RID: 3589
	public CompoundButtonInput MenuLeft = new CompoundButtonInput();

	// Token: 0x04000E06 RID: 3590
	public CompoundButtonInput MenuRight = new CompoundButtonInput();

	// Token: 0x04000E07 RID: 3591
	public CompoundButtonInput MenuPageLeft = new CompoundButtonInput();

	// Token: 0x04000E08 RID: 3592
	public CompoundButtonInput MenuPageRight = new CompoundButtonInput();

	// Token: 0x04000E09 RID: 3593
	public CompoundButtonInput ActionButtonA = new CompoundButtonInput();

	// Token: 0x04000E0A RID: 3594
	public CompoundButtonInput Cancel = new CompoundButtonInput();

	// Token: 0x04000E0B RID: 3595
	public CompoundButtonInput Copy = new CompoundButtonInput();

	// Token: 0x04000E0C RID: 3596
	public CompoundButtonInput Delete = new CompoundButtonInput();

	// Token: 0x04000E0D RID: 3597
	public CompoundButtonInput Focus = new CompoundButtonInput();

	// Token: 0x04000E0E RID: 3598
	public CompoundButtonInput Filter = new CompoundButtonInput();

	// Token: 0x04000E0F RID: 3599
	public CompoundButtonInput Legend = new CompoundButtonInput();

	// Token: 0x04000E10 RID: 3600
	public IButtonInput LeftClick;

	// Token: 0x04000E11 RID: 3601
	public IButtonInput RightClick;

	// Token: 0x04000E12 RID: 3602
	public List<IButtonInput> m_allButtonInput;

	// Token: 0x04000E13 RID: 3603
	public List<Core.Input.InputButtonProcessor> m_allButtonProcessor;

	// Token: 0x04000E14 RID: 3604
	public List<IAxisInput> m_allAxisInput;

	// Token: 0x04000E15 RID: 3605
	private int m_lastPressedButtonInput;

	// Token: 0x04000E16 RID: 3606
	private int m_lastPressedAxisInput;

	// Token: 0x04000E17 RID: 3607
	public CompoundButtonInput Stomp = new CompoundButtonInput();
}
