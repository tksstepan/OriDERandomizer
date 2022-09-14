using System;
using Game;
using UnityEngine;

namespace Core
{
	public class Input
	{
		static Input()
		{
			Input.Filter = new Input.InputButtonProcessor();
			Input.Legend = new Input.InputButtonProcessor();
			Input.Buttons = new Input.InputButtonProcessor[]
			{
				Input.Down,
				Input.Up,
				Input.Left,
				Input.Right,
				Input.Jump,
				Input.SpiritFlame,
				Input.Bash,
				Input.SoulFlame,
				Input.ChargeJump,
				Input.Glide,
				Input.Grab,
				Input.LeftShoulder,
				Input.RightShoulder,
				Input.Start,
				Input.AnyStart,
				Input.Select,
				Input.LeftStick,
				Input.RightStick,
				Input.MenuDown,
				Input.MenuUp,
				Input.MenuLeft,
				Input.MenuRight,
				Input.MenuPageLeft,
				Input.MenuPageRight,
				Input.ActionButtonA,
				Input.ZoomIn,
				Input.ZoomOut,
				Input.Cancel,
				Input.Copy,
				Input.Delete,
				Input.Focus,
				Input.Filter,
				Input.Legend,
				Input.Stomp
			};
		}

		public static int NormalizedHorizontal
		{
			get
			{
				if (Input.Horizontal < -0.4f)
				{
					return -1;
				}
				if (Input.Horizontal > 0.4f)
				{
					return 1;
				}
				return 0;
			}
		}

		public static float NormalizedVertical
		{
			get
			{
				if (Input.Vertical < -0.6f)
				{
					return -1f;
				}
				if (Input.Vertical > 0.6f)
				{
					return 1f;
				}
				return 0f;
			}
		}

		public static Vector2 Axis
		{
			get
			{
				return new Vector2(Input.Horizontal, Input.Vertical);
			}
		}

		public static Vector2 AnalogAxisLeft
		{
			get
			{
				return new Vector2(Input.HorizontalAnalogLeft, Input.VerticalAnalogLeft);
			}
		}

		public static Vector2 AnalogAxisRight
		{
			get
			{
				return new Vector2(Input.HorizontalAnalogRight, Input.VerticalAnalogRight);
			}
		}

		public static Vector2 DigiPadAxis
		{
			get
			{
				return new Vector2((float)Input.HorizontalDigiPad, (float)Input.VerticalDigiPad);
			}
		}

		public static Vector2 CursorPositionUI
		{
			get
			{
				Camera camera = UI.Cameras.System.GUICamera.Camera;
				Vector2 cursorPosition = Input.CursorPosition;
				return camera.ViewportToWorldPoint(cursorPosition);
			}
		}

		public static bool OnAnyButtonPressed
		{
			get
			{
				for (int i = 0; i < Input.Buttons.Length; i++)
				{
					if (Input.Buttons[i].OnPressed)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static bool AnyButtonPressed
		{
			get
			{
				for (int i = 0; i < Input.Buttons.Length; i++)
				{
					if (Input.Buttons[i].IsPressed)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static bool AnyButtonReleased
		{
			get
			{
				for (int i = 0; i < Input.Buttons.Length; i++)
				{
					if (Input.Buttons[i].Released)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static bool OnAnyButtonReleased
		{
			get
			{
				for (int i = 0; i < Input.Buttons.Length; i++)
				{
					if (Input.Buttons[i].OnReleased)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static Input.InputButtonProcessor GetButton(Input.Button button)
		{
			switch (button)
			{
			case Input.Button.ButtonA:
				return Input.Jump;
			case Input.Button.ButtonX:
				return Input.SpiritFlame;
			case Input.Button.ButtonY:
				return Input.Bash;
			case Input.Button.ButtonB:
				return Input.SoulFlame;
			case Input.Button.LeftTrigger:
				return Input.ChargeJump;
			case Input.Button.RightTrigger:
				return Input.Glide;
			case Input.Button.LeftShoulder:
				return Input.LeftShoulder;
			case Input.Button.RightShoulder:
				return Input.RightShoulder;
			case Input.Button.Left:
				return Input.Left;
			case Input.Button.Right:
				return Input.Right;
			case Input.Button.Up:
				return Input.Up;
			case Input.Button.Down:
				return Input.Down;
			case Input.Button.LeftStick:
				return Input.LeftStick;
			case Input.Button.RightStick:
				return Input.RightStick;
			}
			return Input.Unassigned;
		}

		public static float Horizontal;

		public static float Vertical;

		public static int HorizontalDigiPad;

		public static int VerticalDigiPad;

		public static float HorizontalAnalogLeft;

		public static float VerticalAnalogLeft;

		public static float HorizontalAnalogRight;

		public static float VerticalAnalogRight;

		public static Input.InputButtonProcessor Down = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Up = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Left = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Right = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Jump = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor SpiritFlame = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Bash = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor SoulFlame = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor ChargeJump = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Glide = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Grab = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor ZoomIn = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor ZoomOut = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor LeftShoulder = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor RightShoulder = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Start = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor AnyStart = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Select = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Unassigned = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor LeftStick = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor RightStick = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor MenuDown = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor MenuUp = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor MenuLeft = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor MenuRight = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor MenuPageLeft = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor MenuPageRight = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor ActionButtonA = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Cancel = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor LeftClick = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor RightClick = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Copy = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Delete = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Focus = new Input.InputButtonProcessor();

		public static Input.InputButtonProcessor Filter;

		public static Input.InputButtonProcessor Legend;

		public static Vector2 CursorPosition;

		public static bool CursorMoved;

		public static Input.InputButtonProcessor[] Buttons;

		public static Input.InputButtonProcessor Stomp = new Input.InputButtonProcessor();

		public class InputButtonProcessor
		{
			public void Update(bool isPressed)
			{
				this.WasPressed = this.IsPressed;
				this.IsPressed = isPressed;
			}

			public bool OnPressed
			{
				get
				{
					return this.IsPressed && !this.WasPressed;
				}
			}

			public bool OnPressedNotUsed
			{
				get
				{
					return this.IsPressed && !this.WasPressed && !this.Used;
				}
			}

			public bool OnReleased
			{
				get
				{
					return !this.IsPressed && this.WasPressed;
				}
			}

			public bool Pressed
			{
				get
				{
					return this.IsPressed;
				}
			}

			public bool Released
			{
				get
				{
					return !this.IsPressed;
				}
			}

			public bool WasPressed;

			public bool IsPressed;

			public bool Used;
		}

		public enum Button
		{
			ButtonA,
			ButtonX,
			ButtonY,
			ButtonB,
			LeftTrigger,
			RightTrigger,
			LeftShoulder,
			RightShoulder,
			Left,
			Right,
			Up,
			Down,
			Unassigned,
			Any,
			LeftStick,
			RightStick
		}
	}
}
