using System;
using Game;
using UnityEngine;

namespace Core
{
	// Token: 0x020002F4 RID: 756
	public class Input
	{
		// Token: 0x06000E8E RID: 3726 RVA: 0x0005A0C4 File Offset: 0x000582C4
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

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x06000E8F RID: 3727 RVA: 0x0000C9B0 File Offset: 0x0000ABB0
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

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x06000E90 RID: 3728 RVA: 0x0000C9CF File Offset: 0x0000ABCF
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

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x06000E91 RID: 3729 RVA: 0x0000C9FA File Offset: 0x0000ABFA
		public static Vector2 Axis
		{
			get
			{
				return new Vector2(Input.Horizontal, Input.Vertical);
			}
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x06000E92 RID: 3730 RVA: 0x0000CA0B File Offset: 0x0000AC0B
		public static Vector2 AnalogAxisLeft
		{
			get
			{
				return new Vector2(Input.HorizontalAnalogLeft, Input.VerticalAnalogLeft);
			}
		}

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x06000E93 RID: 3731 RVA: 0x0000CA1C File Offset: 0x0000AC1C
		public static Vector2 AnalogAxisRight
		{
			get
			{
				return new Vector2(Input.HorizontalAnalogRight, Input.VerticalAnalogRight);
			}
		}

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x06000E94 RID: 3732 RVA: 0x0000CA2D File Offset: 0x0000AC2D
		public static Vector2 DigiPadAxis
		{
			get
			{
				return new Vector2((float)Input.HorizontalDigiPad, (float)Input.VerticalDigiPad);
			}
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x06000E95 RID: 3733 RVA: 0x0005A378 File Offset: 0x00058578
		public static Vector2 CursorPositionUI
		{
			get
			{
				Camera camera = UI.Cameras.System.GUICamera.Camera;
				Vector2 cursorPosition = Input.CursorPosition;
				return camera.ViewportToWorldPoint(cursorPosition);
			}
		}

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x06000E96 RID: 3734 RVA: 0x0005A3AC File Offset: 0x000585AC
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

		// Token: 0x17000226 RID: 550
		// (get) Token: 0x06000E97 RID: 3735 RVA: 0x0005A3DC File Offset: 0x000585DC
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

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x06000E98 RID: 3736 RVA: 0x0005A40C File Offset: 0x0005860C
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

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x06000E99 RID: 3737 RVA: 0x0005A43C File Offset: 0x0005863C
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

		// Token: 0x06000E9A RID: 3738 RVA: 0x0005A46C File Offset: 0x0005866C
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

		// Token: 0x04000DA5 RID: 3493
		public static float Horizontal;

		// Token: 0x04000DA6 RID: 3494
		public static float Vertical;

		// Token: 0x04000DA7 RID: 3495
		public static int HorizontalDigiPad;

		// Token: 0x04000DA8 RID: 3496
		public static int VerticalDigiPad;

		// Token: 0x04000DA9 RID: 3497
		public static float HorizontalAnalogLeft;

		// Token: 0x04000DAA RID: 3498
		public static float VerticalAnalogLeft;

		// Token: 0x04000DAB RID: 3499
		public static float HorizontalAnalogRight;

		// Token: 0x04000DAC RID: 3500
		public static float VerticalAnalogRight;

		// Token: 0x04000DAD RID: 3501
		public static Input.InputButtonProcessor Down = new Input.InputButtonProcessor();

		// Token: 0x04000DAE RID: 3502
		public static Input.InputButtonProcessor Up = new Input.InputButtonProcessor();

		// Token: 0x04000DAF RID: 3503
		public static Input.InputButtonProcessor Left = new Input.InputButtonProcessor();

		// Token: 0x04000DB0 RID: 3504
		public static Input.InputButtonProcessor Right = new Input.InputButtonProcessor();

		// Token: 0x04000DB1 RID: 3505
		public static Input.InputButtonProcessor Jump = new Input.InputButtonProcessor();

		// Token: 0x04000DB2 RID: 3506
		public static Input.InputButtonProcessor SpiritFlame = new Input.InputButtonProcessor();

		// Token: 0x04000DB3 RID: 3507
		public static Input.InputButtonProcessor Bash = new Input.InputButtonProcessor();

		// Token: 0x04000DB4 RID: 3508
		public static Input.InputButtonProcessor SoulFlame = new Input.InputButtonProcessor();

		// Token: 0x04000DB5 RID: 3509
		public static Input.InputButtonProcessor ChargeJump = new Input.InputButtonProcessor();

		// Token: 0x04000DB6 RID: 3510
		public static Input.InputButtonProcessor Glide = new Input.InputButtonProcessor();

		// Token: 0x04000DB7 RID: 3511
		public static Input.InputButtonProcessor Grab = new Input.InputButtonProcessor();

		// Token: 0x04000DB8 RID: 3512
		public static Input.InputButtonProcessor ZoomIn = new Input.InputButtonProcessor();

		// Token: 0x04000DB9 RID: 3513
		public static Input.InputButtonProcessor ZoomOut = new Input.InputButtonProcessor();

		// Token: 0x04000DBA RID: 3514
		public static Input.InputButtonProcessor LeftShoulder = new Input.InputButtonProcessor();

		// Token: 0x04000DBB RID: 3515
		public static Input.InputButtonProcessor RightShoulder = new Input.InputButtonProcessor();

		// Token: 0x04000DBC RID: 3516
		public static Input.InputButtonProcessor Start = new Input.InputButtonProcessor();

		// Token: 0x04000DBD RID: 3517
		public static Input.InputButtonProcessor AnyStart = new Input.InputButtonProcessor();

		// Token: 0x04000DBE RID: 3518
		public static Input.InputButtonProcessor Select = new Input.InputButtonProcessor();

		// Token: 0x04000DBF RID: 3519
		public static Input.InputButtonProcessor Unassigned = new Input.InputButtonProcessor();

		// Token: 0x04000DC0 RID: 3520
		public static Input.InputButtonProcessor LeftStick = new Input.InputButtonProcessor();

		// Token: 0x04000DC1 RID: 3521
		public static Input.InputButtonProcessor RightStick = new Input.InputButtonProcessor();

		// Token: 0x04000DC2 RID: 3522
		public static Input.InputButtonProcessor MenuDown = new Input.InputButtonProcessor();

		// Token: 0x04000DC3 RID: 3523
		public static Input.InputButtonProcessor MenuUp = new Input.InputButtonProcessor();

		// Token: 0x04000DC4 RID: 3524
		public static Input.InputButtonProcessor MenuLeft = new Input.InputButtonProcessor();

		// Token: 0x04000DC5 RID: 3525
		public static Input.InputButtonProcessor MenuRight = new Input.InputButtonProcessor();

		// Token: 0x04000DC6 RID: 3526
		public static Input.InputButtonProcessor MenuPageLeft = new Input.InputButtonProcessor();

		// Token: 0x04000DC7 RID: 3527
		public static Input.InputButtonProcessor MenuPageRight = new Input.InputButtonProcessor();

		// Token: 0x04000DC8 RID: 3528
		public static Input.InputButtonProcessor ActionButtonA = new Input.InputButtonProcessor();

		// Token: 0x04000DC9 RID: 3529
		public static Input.InputButtonProcessor Cancel = new Input.InputButtonProcessor();

		// Token: 0x04000DCA RID: 3530
		public static Input.InputButtonProcessor LeftClick = new Input.InputButtonProcessor();

		// Token: 0x04000DCB RID: 3531
		public static Input.InputButtonProcessor RightClick = new Input.InputButtonProcessor();

		// Token: 0x04000DCC RID: 3532
		public static Input.InputButtonProcessor Copy = new Input.InputButtonProcessor();

		// Token: 0x04000DCD RID: 3533
		public static Input.InputButtonProcessor Delete = new Input.InputButtonProcessor();

		// Token: 0x04000DCE RID: 3534
		public static Input.InputButtonProcessor Focus = new Input.InputButtonProcessor();

		// Token: 0x04000DCF RID: 3535
		public static Input.InputButtonProcessor Filter;

		// Token: 0x04000DD0 RID: 3536
		public static Input.InputButtonProcessor Legend;

		// Token: 0x04000DD1 RID: 3537
		public static Vector2 CursorPosition;

		// Token: 0x04000DD2 RID: 3538
		public static bool CursorMoved;

		// Token: 0x04000DD3 RID: 3539
		public static Input.InputButtonProcessor[] Buttons;

		// Token: 0x04000DD4 RID: 3540
		public static Input.InputButtonProcessor Stomp = new Input.InputButtonProcessor();

		// Token: 0x020002F5 RID: 757
		public class InputButtonProcessor
		{
			// Token: 0x06000E9C RID: 3740 RVA: 0x0000CA40 File Offset: 0x0000AC40
			public void Update(bool isPressed)
			{
				this.WasPressed = this.IsPressed;
				this.IsPressed = isPressed;
			}

			// Token: 0x17000229 RID: 553
			// (get) Token: 0x06000E9D RID: 3741 RVA: 0x0000CA55 File Offset: 0x0000AC55
			public bool OnPressed
			{
				get
				{
					return this.IsPressed && !this.WasPressed;
				}
			}

			// Token: 0x1700022A RID: 554
			// (get) Token: 0x06000E9E RID: 3742 RVA: 0x0000CA6A File Offset: 0x0000AC6A
			public bool OnPressedNotUsed
			{
				get
				{
					return this.IsPressed && !this.WasPressed && !this.Used;
				}
			}

			// Token: 0x1700022B RID: 555
			// (get) Token: 0x06000E9F RID: 3743 RVA: 0x0000CA87 File Offset: 0x0000AC87
			public bool OnReleased
			{
				get
				{
					return !this.IsPressed && this.WasPressed;
				}
			}

			// Token: 0x1700022C RID: 556
			// (get) Token: 0x06000EA0 RID: 3744 RVA: 0x0000CA99 File Offset: 0x0000AC99
			public bool Pressed
			{
				get
				{
					return this.IsPressed;
				}
			}

			// Token: 0x1700022D RID: 557
			// (get) Token: 0x06000EA1 RID: 3745 RVA: 0x0000CAA1 File Offset: 0x0000ACA1
			public bool Released
			{
				get
				{
					return !this.IsPressed;
				}
			}

			// Token: 0x04000DD5 RID: 3541
			public bool WasPressed;

			// Token: 0x04000DD6 RID: 3542
			public bool IsPressed;

			// Token: 0x04000DD7 RID: 3543
			public bool Used;
		}

		// Token: 0x020002F6 RID: 758
		public enum Button
		{
			// Token: 0x04000DD9 RID: 3545
			ButtonA,
			// Token: 0x04000DDA RID: 3546
			ButtonX,
			// Token: 0x04000DDB RID: 3547
			ButtonY,
			// Token: 0x04000DDC RID: 3548
			ButtonB,
			// Token: 0x04000DDD RID: 3549
			LeftTrigger,
			// Token: 0x04000DDE RID: 3550
			RightTrigger,
			// Token: 0x04000DDF RID: 3551
			LeftShoulder,
			// Token: 0x04000DE0 RID: 3552
			RightShoulder,
			// Token: 0x04000DE1 RID: 3553
			Left,
			// Token: 0x04000DE2 RID: 3554
			Right,
			// Token: 0x04000DE3 RID: 3555
			Up,
			// Token: 0x04000DE4 RID: 3556
			Down,
			// Token: 0x04000DE5 RID: 3557
			Unassigned,
			// Token: 0x04000DE6 RID: 3558
			Any,
			// Token: 0x04000DE7 RID: 3559
			LeftStick,
			// Token: 0x04000DE8 RID: 3560
			RightStick
		}
	}
}
