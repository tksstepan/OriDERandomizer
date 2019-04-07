using System;
using System.Collections.Generic;
using System.IO;
using SmartInput;
using UnityEngine;

// Token: 0x020002F9 RID: 761
public class PlayerInputRebinding
{
	// Token: 0x1700022F RID: 559
	// (get) Token: 0x06000EB7 RID: 3767 RVA: 0x0000CB2A File Offset: 0x0000AD2A
	public static PlayerInputRebinding.KeyBindingSettings KeyRebindings
	{
		get
		{
			if (PlayerInputRebinding.m_keyRebindings == null)
			{
				if (!File.Exists(PlayerInputRebinding.KeyRebindingFile))
				{
					PlayerInputRebinding.SetDefaultKeyBindingSettings();
					PlayerInputRebinding.WriteKeyRebindSettings();
				}
				else
				{
					PlayerInputRebinding.GetKeyRebindSettingsFromFile();
				}
			}
			return PlayerInputRebinding.m_keyRebindings;
		}
	}

	// Token: 0x17000230 RID: 560
	// (get) Token: 0x06000EB8 RID: 3768 RVA: 0x0000CB55 File Offset: 0x0000AD55
	private static string KeyRebindingFile
	{
		get
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, PlayerInputRebinding.keyRebindingFileName);
		}
	}

	// Token: 0x06000EB9 RID: 3769 RVA: 0x0005B2FC File Offset: 0x000594FC
	public static void GetKeyRebindSettingsFromFile()
	{
		try
		{
			using (StreamReader streamReader = new StreamReader(new FileStream(PlayerInputRebinding.KeyRebindingFile, FileMode.Open)))
			{
				streamReader.ReadLine();
				streamReader.ReadLine();
				streamReader.ReadLine();
				streamReader.ReadLine();
				PlayerInputRebinding.m_keyRebindings = new PlayerInputRebinding.KeyBindingSettings
				{
					IsRebinding = true,
					HorizontalDigiPadLeft = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					HorizontalDigiPadRight = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					VerticalDigiPadDown = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					VerticalDigiPadUp = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					MenuLeft = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					MenuRight = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					MenuDown = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					MenuUp = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					MenuPageLeft = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					MenuPageRight = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					ActionButtonA = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					SoulFlame = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Jump = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Grab = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					SpiritFlame = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Bash = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Glide = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					ChargeJump = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Select = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Start = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Cancel = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					LeftShoulder = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					RightShoulder = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					LeftStick = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					RightStick = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					ZoomIn = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					ZoomOut = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Copy = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Delete = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Focus = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Filter = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine()),
					Legend = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine())
				};
			}
		}
		catch (Exception)
		{
			PlayerInputRebinding.SetDefaultKeyBindingSettings();
		}
	}

	// Token: 0x06000EBA RID: 3770 RVA: 0x0005B5B4 File Offset: 0x000597B4
	private static KeyCode[] StringToKeyBinding(string s)
	{
		s = s.Split(new string[]
		{
			": "
		}, StringSplitOptions.None)[1];
		string[] array = s.Split(new string[]
		{
			", "
		}, StringSplitOptions.None);
		List<KeyCode> list = new List<KeyCode>();
		foreach (string value in array)
		{
			list.Add((KeyCode)((int)Enum.Parse(typeof(KeyCode), value)));
		}
		return list.ToArray();
	}

	// Token: 0x06000EBB RID: 3771 RVA: 0x0005B638 File Offset: 0x00059838
	public static void WriteKeyRebindSettings()
	{
		using (StreamWriter streamWriter = new StreamWriter(new FileStream(PlayerInputRebinding.KeyRebindingFile, FileMode.OpenOrCreate)))
		{
			PlayerInputRebinding.KeyBindingSettings keyRebindings = PlayerInputRebinding.m_keyRebindings;
			streamWriter.WriteLine("Keyboard Rebindings");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Activate Key Rebinding: " + keyRebindings.IsRebinding);
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Movement Left: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.HorizontalDigiPadLeft));
			streamWriter.WriteLine("Movement Right: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.HorizontalDigiPadRight));
			streamWriter.WriteLine("Movement Down: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.VerticalDigiPadDown));
			streamWriter.WriteLine("Movement Up: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.VerticalDigiPadUp));
			streamWriter.WriteLine("Menu Left: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.MenuLeft));
			streamWriter.WriteLine("Menu Right: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.MenuRight));
			streamWriter.WriteLine("Menu Down: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.MenuDown));
			streamWriter.WriteLine("Menu Up: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.MenuUp));
			streamWriter.WriteLine("Menu Previous: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.MenuPageLeft));
			streamWriter.WriteLine("Menu Next: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.MenuPageRight));
			streamWriter.WriteLine("Proceed: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.ActionButtonA));
			streamWriter.WriteLine("Soul Link: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.SoulFlame));
			streamWriter.WriteLine("Jump: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Jump));
			streamWriter.WriteLine("Grab: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Grab));
			streamWriter.WriteLine("Spirit Flame: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.SpiritFlame));
			streamWriter.WriteLine("Bash: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Bash));
			streamWriter.WriteLine("Glide: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Glide));
			streamWriter.WriteLine("Charge Jump: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.ChargeJump));
			streamWriter.WriteLine("Select: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Select));
			streamWriter.WriteLine("Start: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Start));
			streamWriter.WriteLine("Cancel: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Cancel));
			streamWriter.WriteLine("Grenade: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.LeftShoulder));
			streamWriter.WriteLine("Dash: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.RightShoulder));
			streamWriter.WriteLine("Left Stick: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.LeftStick));
			streamWriter.WriteLine("Debug Menu (shhh): " + PlayerInputRebinding.KeyBindingToString(keyRebindings.RightStick));
			streamWriter.WriteLine("Zoom In World Map: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.ZoomIn));
			streamWriter.WriteLine("Zoom Out World Map: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.ZoomOut));
			streamWriter.WriteLine("Copy: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Copy));
			streamWriter.WriteLine("Delete: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Delete));
			streamWriter.WriteLine("Focus: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Focus));
			streamWriter.WriteLine("Filter: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Filter));
			streamWriter.WriteLine("Legend: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Legend));
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Usage:");
			streamWriter.WriteLine("- There is no guarantee of the game still being playable after key rebinding. Please use with caution and delete this file in case of breakage");
			streamWriter.WriteLine("- Spelling and syntactical errors will result in the key rebindings not registering properly, and the game will get set to default");
			streamWriter.WriteLine("- Deleting this file will result in this file being recreated by the game, containing the default settings");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Don't forget to restart the game after editing this file!");
			streamWriter.WriteLine("Don't forget to close this file before restarting the game!");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Mouse0 is left mouse button");
			streamWriter.WriteLine("Mouse1 is right mouse button");
			streamWriter.WriteLine("AlphaX is the number X on the keyboard (not num pad, the stuff above your keys)");
		}
	}

	// Token: 0x06000EBC RID: 3772 RVA: 0x0005BAAC File Offset: 0x00059CAC
	private static string KeyBindingToString(KeyCode[] codes)
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

	// Token: 0x06000EBD RID: 3773 RVA: 0x0000CB66 File Offset: 0x0000AD66
	public static void SetDefaultKeyBindingSettings()
	{
		PlayerInputRebinding.m_keyRebindings = PlayerInputRebinding.DefaultKeyBindingSettings();
	}

	// Token: 0x06000EBE RID: 3774 RVA: 0x0005BB0C File Offset: 0x00059D0C
	private static PlayerInputRebinding.KeyBindingSettings DefaultKeyBindingSettings()
	{
		bool flag = GameSettings.Instance.CurrentControlScheme == ControlScheme.KeyboardAndMouse;
		bool flag2 = GameSettings.Instance.KeyboardLayout == KeyboardLayout.QWERTY;
		PlayerInputRebinding.KeyBindingSettings keyBindingSettings = new PlayerInputRebinding.KeyBindingSettings();
		keyBindingSettings.IsRebinding = false;
		keyBindingSettings.HorizontalDigiPadLeft = new KeyCode[]
		{
			(!flag2) ? KeyCode.Q : KeyCode.A,
			KeyCode.LeftArrow
		};
		keyBindingSettings.HorizontalDigiPadRight = new KeyCode[]
		{
			KeyCode.D,
			KeyCode.RightArrow
		};
		keyBindingSettings.VerticalDigiPadDown = new KeyCode[]
		{
			KeyCode.S,
			KeyCode.DownArrow
		};
		keyBindingSettings.VerticalDigiPadUp = new KeyCode[]
		{
			(!flag2) ? KeyCode.Z : KeyCode.W,
			KeyCode.UpArrow
		};
		keyBindingSettings.MenuLeft = new KeyCode[]
		{
			(!flag2) ? KeyCode.Q : KeyCode.A,
			KeyCode.LeftArrow
		};
		keyBindingSettings.MenuRight = new KeyCode[]
		{
			KeyCode.D,
			KeyCode.RightArrow
		};
		keyBindingSettings.MenuDown = new KeyCode[]
		{
			KeyCode.S,
			KeyCode.DownArrow
		};
		keyBindingSettings.MenuUp = new KeyCode[]
		{
			(!flag2) ? KeyCode.Z : KeyCode.W,
			KeyCode.UpArrow
		};
		keyBindingSettings.MenuPageLeft = new KeyCode[]
		{
			KeyCode.K,
			KeyCode.PageUp
		};
		keyBindingSettings.MenuPageRight = new KeyCode[]
		{
			KeyCode.L,
			KeyCode.PageDown
		};
		keyBindingSettings.ActionButtonA = new KeyCode[]
		{
			KeyCode.Space,
			KeyCode.Return
		};
		keyBindingSettings.SoulFlame = new KeyCode[]
		{
			KeyCode.E
		};
		keyBindingSettings.Jump = new KeyCode[]
		{
			KeyCode.Space,
			(!flag2) ? KeyCode.W : KeyCode.Z,
			KeyCode.Y
		};
		keyBindingSettings.Grab = new KeyCode[]
		{
			KeyCode.LeftShift,
			KeyCode.RightShift
		};
		PlayerInputRebinding.KeyBindingSettings keyBindingSettings2 = keyBindingSettings;
		KeyCode[] spiritFlame;
		if (flag)
		{
			KeyCode[] array = new KeyCode[2];
			array[0] = KeyCode.Mouse0;
			spiritFlame = array;
			array[1] = KeyCode.X;
		}
		else
		{
			KeyCode[] array2 = new KeyCode[2];
			array2[0] = KeyCode.X;
			spiritFlame = array2;
			array2[1] = KeyCode.Mouse0;
		}
		keyBindingSettings2.SpiritFlame = spiritFlame;
		PlayerInputRebinding.KeyBindingSettings keyBindingSettings3 = keyBindingSettings;
		KeyCode[] bash;
		if (flag)
		{
			KeyCode[] array3 = new KeyCode[2];
			array3[0] = KeyCode.Mouse1;
			bash = array3;
			array3[1] = KeyCode.C;
		}
		else
		{
			KeyCode[] array4 = new KeyCode[2];
			array4[0] = KeyCode.C;
			bash = array4;
			array4[1] = KeyCode.Mouse1;
		}
		keyBindingSettings3.Bash = bash;
		keyBindingSettings.Glide = new KeyCode[]
		{
			KeyCode.LeftShift,
			KeyCode.RightShift
		};
		PlayerInputRebinding.KeyBindingSettings keyBindingSettings4 = keyBindingSettings;
		KeyCode[] chargeJump;
		if (flag)
		{
			KeyCode[] array5 = new KeyCode[2];
			array5[0] = ((!flag2) ? KeyCode.Z : KeyCode.W);
			chargeJump = array5;
			array5[1] = KeyCode.UpArrow;
		}
		else
		{
			KeyCode[] array6 = new KeyCode[2];
			array6[0] = KeyCode.UpArrow;
			chargeJump = array6;
			array6[1] = ((!flag2) ? KeyCode.Z : KeyCode.W);
		}
		keyBindingSettings4.ChargeJump = chargeJump;
		keyBindingSettings.Select = new KeyCode[]
		{
			KeyCode.Tab
		};
		keyBindingSettings.Start = new KeyCode[]
		{
			KeyCode.Escape
		};
		keyBindingSettings.Cancel = new KeyCode[]
		{
			KeyCode.Escape,
			KeyCode.Mouse1
		};
		keyBindingSettings.LeftShoulder = new KeyCode[]
		{
			KeyCode.R
		};
		keyBindingSettings.RightShoulder = new KeyCode[]
		{
			KeyCode.LeftControl,
			KeyCode.RightControl
		};
		keyBindingSettings.LeftStick = new KeyCode[]
		{
			KeyCode.Alpha7
		};
		keyBindingSettings.RightStick = new KeyCode[]
		{
			KeyCode.Alpha8
		};
		keyBindingSettings.ZoomIn = new KeyCode[]
		{
			KeyCode.RightShift,
			KeyCode.LeftShift
		};
		keyBindingSettings.ZoomOut = new KeyCode[]
		{
			KeyCode.RightControl,
			KeyCode.LeftControl
		};
		keyBindingSettings.Copy = new KeyCode[]
		{
			KeyCode.C
		};
		keyBindingSettings.Delete = new KeyCode[]
		{
			KeyCode.Delete
		};
		keyBindingSettings.Focus = new KeyCode[]
		{
			KeyCode.F
		};
		keyBindingSettings.Filter = new KeyCode[]
		{
			KeyCode.F
		};
		keyBindingSettings.Legend = new KeyCode[]
		{
			KeyCode.L
		};
		return keyBindingSettings;
	}

	// Token: 0x17000231 RID: 561
	// (get) Token: 0x06000EBF RID: 3775 RVA: 0x0000CB72 File Offset: 0x0000AD72
	private static string ControllerRemappingFile
	{
		get
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, PlayerInputRebinding.controllerRebindingFileName);
		}
	}

	// Token: 0x06000EC0 RID: 3776 RVA: 0x0000CB83 File Offset: 0x0000AD83
	public static XboxControllerInput.Button GetRemappedJoystickButton(XboxControllerInput.Button joystickButtonIndex)
	{
		return PlayerInputRebinding.intToButton[PlayerInputRebinding.m_controllerButtonRemappings[PlayerInputRebinding.ButtonToInt(joystickButtonIndex)]];
	}

	// Token: 0x06000EC1 RID: 3777 RVA: 0x0005BED0 File Offset: 0x0005A0D0
	public static void GetControllerButtonRemappingsFromFile()
	{
		try
		{
			using (StreamReader streamReader = new StreamReader(new FileStream(PlayerInputRebinding.ControllerRemappingFile, FileMode.Open)))
			{
				PlayerInputRebinding.SetDefaultControllerButtonRemappings();
				streamReader.ReadLine();
				streamReader.ReadLine();
				streamReader.ReadLine();
				string[] array = streamReader.ReadLine().Split(new string[]
				{
					": "
				}, StringSplitOptions.None);
				bool flag = bool.Parse(array[1]);
				if (flag)
				{
					streamReader.ReadLine();
					PlayerInputRebinding.m_controllerIsRemappingButtons = flag;
					PlayerInputRebinding.m_controllerButtonRemappings[0] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[1] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[2] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[3] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[4] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[5] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[6] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[7] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[8] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[9] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[12] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					PlayerInputRebinding.m_controllerButtonRemappings[13] = int.Parse(streamReader.ReadLine().Split(new string[]
					{
						": "
					}, StringSplitOptions.None)[1]) - 1;
					for (int i = 0; i < 10; i++)
					{
						if (PlayerInputRebinding.m_controllerButtonRemappings[i] < 0 || PlayerInputRebinding.m_controllerButtonRemappings[i] > 11)
						{
							PlayerInputRebinding.SetDefaultControllerButtonRemappings();
						}
					}
					for (int j = 12; j < 14; j++)
					{
						if (PlayerInputRebinding.m_controllerButtonRemappings[j] < 0 || PlayerInputRebinding.m_controllerButtonRemappings[j] > 13)
						{
							PlayerInputRebinding.SetDefaultControllerButtonRemappings();
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			PlayerInputRebinding.SetDefaultControllerButtonRemappings();
		}
	}

	// Token: 0x06000EC2 RID: 3778 RVA: 0x0005C204 File Offset: 0x0005A404
	public static void WriteControllerButtonRemappings()
	{
		using (StreamWriter streamWriter = new StreamWriter(new FileStream(PlayerInputRebinding.ControllerRemappingFile, FileMode.OpenOrCreate)))
		{
			streamWriter.WriteLine("Controller Button Remapping - remaps controller buttons to different DirectInput button IDs");
			streamWriter.WriteLine("Only for DirectInput controllers");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Activate DirectInput Button Rebinding: " + PlayerInputRebinding.m_controllerIsRemappingButtons);
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("A: " + (PlayerInputRebinding.m_controllerButtonRemappings[0] + 1));
			streamWriter.WriteLine("B: " + (PlayerInputRebinding.m_controllerButtonRemappings[1] + 1));
			streamWriter.WriteLine("X: " + (PlayerInputRebinding.m_controllerButtonRemappings[2] + 1));
			streamWriter.WriteLine("Y: " + (PlayerInputRebinding.m_controllerButtonRemappings[3] + 1));
			streamWriter.WriteLine("LShoulder: " + (PlayerInputRebinding.m_controllerButtonRemappings[4] + 1));
			streamWriter.WriteLine("RShoulder: " + (PlayerInputRebinding.m_controllerButtonRemappings[5] + 1));
			streamWriter.WriteLine("Select: " + (PlayerInputRebinding.m_controllerButtonRemappings[6] + 1));
			streamWriter.WriteLine("Start: " + (PlayerInputRebinding.m_controllerButtonRemappings[7] + 1));
			streamWriter.WriteLine("LStick: " + (PlayerInputRebinding.m_controllerButtonRemappings[8] + 1));
			streamWriter.WriteLine("RStick: " + (PlayerInputRebinding.m_controllerButtonRemappings[9] + 1));
			streamWriter.WriteLine("LTrigger: " + (PlayerInputRebinding.m_controllerButtonRemappings[12] + 1));
			streamWriter.WriteLine("RTrigger: " + (PlayerInputRebinding.m_controllerButtonRemappings[13] + 1));
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Usage:");
			streamWriter.WriteLine("- There is no guarantee of the game still being playable after button remapping. Please use with caution and delete this file in case of breakage");
			streamWriter.WriteLine("- Syntactical errors will result in the key rebindings not registering properly, and the button bindings will get set to default");
			streamWriter.WriteLine("- Only numbers ranging from 1-12 should be used (with the exception of LTrigger and RTrigger)");
			streamWriter.WriteLine("- Deleting this file will result in this file being recreated by the game, containing the default settings");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Don't forget to restart the game after editing this file!");
			streamWriter.WriteLine("Don't forget to close this file before restarting the game!");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("To determine the correct button remaps you need, you can look at:");
			streamWriter.WriteLine("    \"Game Controllers\"->your controller->\"properties\"; and bash buttons until you know your current button IDs.");
			streamWriter.WriteLine("    Alternatively, you could look up controller test software that does the same.");
			streamWriter.WriteLine("After you found your controller's button mapping, you can fill them in accordingly, in the above list.");
			streamWriter.WriteLine("Leave left and right trigger at 13 and 14 to keep them working as controller axes 9 and 10");
		}
	}

	// Token: 0x06000EC3 RID: 3779 RVA: 0x0000CB97 File Offset: 0x0000AD97
	public static void RefreshControllerButtonRemappings()
	{
		if (!PlayerInputRebinding.m_hasReadControllerRemappingsFile)
		{
			if (!File.Exists(PlayerInputRebinding.ControllerRemappingFile))
			{
				PlayerInputRebinding.SetDefaultControllerButtonRemappings();
				PlayerInputRebinding.WriteControllerButtonRemappings();
			}
			else
			{
				PlayerInputRebinding.GetControllerButtonRemappingsFromFile();
			}
			PlayerInputRebinding.m_hasReadControllerRemappingsFile = true;
		}
	}

	// Token: 0x06000EC4 RID: 3780 RVA: 0x0000CBCC File Offset: 0x0000ADCC
	public static void SetDefaultControllerButtonRemappings()
	{
		PlayerInputRebinding.m_controllerButtonRemappings = new int[]
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			12,
			13,
			14,
			15
		};
	}

	// Token: 0x06000EC5 RID: 3781 RVA: 0x0005C4A8 File Offset: 0x0005A6A8
	private static int ButtonToInt(XboxControllerInput.Button button)
	{
		switch (button)
		{
		case XboxControllerInput.Button.ButtonA:
			return 0;
		case XboxControllerInput.Button.ButtonX:
			return 2;
		case XboxControllerInput.Button.ButtonY:
			return 3;
		case XboxControllerInput.Button.ButtonB:
			return 1;
		case XboxControllerInput.Button.LeftTrigger:
			return 12;
		case XboxControllerInput.Button.RightTrigger:
			return 13;
		case XboxControllerInput.Button.LeftShoulder:
			return 4;
		case XboxControllerInput.Button.RightShoulder:
			return 5;
		case XboxControllerInput.Button.LeftStick:
			return 8;
		case XboxControllerInput.Button.RightStick:
			return 9;
		case XboxControllerInput.Button.Select:
			return 6;
		case XboxControllerInput.Button.Start:
			return 7;
		case XboxControllerInput.Button.Button10:
			return 10;
		case XboxControllerInput.Button.Button11:
			return 11;
		default:
			return 0;
		}
	}

	// Token: 0x04000E16 RID: 3606
	private static string keyRebindingFileName = "KeyRebindings.txt";

	// Token: 0x04000E17 RID: 3607
	private static string controllerRebindingFileName = "ControllerButtonRemaps.txt";

	// Token: 0x04000E18 RID: 3608
	private static PlayerInputRebinding.KeyBindingSettings m_keyRebindings;

	// Token: 0x04000E19 RID: 3609
	private static int[] m_controllerButtonRemappings = new int[]
	{
		0,
		1,
		2,
		3,
		4,
		5,
		6,
		7,
		8,
		9,
		10,
		11,
		12,
		13
	};

	// Token: 0x04000E1A RID: 3610
	private static bool m_controllerIsRemappingButtons;

	// Token: 0x04000E1B RID: 3611
	private static bool m_hasReadControllerRemappingsFile = false;

	// Token: 0x04000E1C RID: 3612
	private static XboxControllerInput.Button[] intToButton = new XboxControllerInput.Button[]
	{
		XboxControllerInput.Button.ButtonA,
		XboxControllerInput.Button.ButtonB,
		XboxControllerInput.Button.ButtonX,
		XboxControllerInput.Button.ButtonY,
		XboxControllerInput.Button.LeftShoulder,
		XboxControllerInput.Button.RightShoulder,
		XboxControllerInput.Button.Select,
		XboxControllerInput.Button.Start,
		XboxControllerInput.Button.LeftStick,
		XboxControllerInput.Button.RightStick,
		XboxControllerInput.Button.Button10,
		XboxControllerInput.Button.Button11,
		XboxControllerInput.Button.LeftTrigger,
		XboxControllerInput.Button.RightTrigger
	};

	// Token: 0x020002FA RID: 762
	public class KeyBindingSettings
	{
		// Token: 0x04000E1D RID: 3613
		public bool IsRebinding;

		// Token: 0x04000E1E RID: 3614
		public KeyCode[] HorizontalDigiPadLeft = new KeyCode[0];

		// Token: 0x04000E1F RID: 3615
		public KeyCode[] HorizontalDigiPadRight = new KeyCode[0];

		// Token: 0x04000E20 RID: 3616
		public KeyCode[] VerticalDigiPadDown = new KeyCode[0];

		// Token: 0x04000E21 RID: 3617
		public KeyCode[] VerticalDigiPadUp = new KeyCode[0];

		// Token: 0x04000E22 RID: 3618
		public KeyCode[] MenuLeft = new KeyCode[0];

		// Token: 0x04000E23 RID: 3619
		public KeyCode[] MenuRight = new KeyCode[0];

		// Token: 0x04000E24 RID: 3620
		public KeyCode[] MenuDown = new KeyCode[0];

		// Token: 0x04000E25 RID: 3621
		public KeyCode[] MenuUp = new KeyCode[0];

		// Token: 0x04000E26 RID: 3622
		public KeyCode[] MenuPageLeft = new KeyCode[0];

		// Token: 0x04000E27 RID: 3623
		public KeyCode[] MenuPageRight = new KeyCode[0];

		// Token: 0x04000E28 RID: 3624
		public KeyCode[] ActionButtonA = new KeyCode[0];

		// Token: 0x04000E29 RID: 3625
		public KeyCode[] SoulFlame = new KeyCode[0];

		// Token: 0x04000E2A RID: 3626
		public KeyCode[] Jump = new KeyCode[0];

		// Token: 0x04000E2B RID: 3627
		public KeyCode[] Grab = new KeyCode[0];

		// Token: 0x04000E2C RID: 3628
		public KeyCode[] SpiritFlame = new KeyCode[0];

		// Token: 0x04000E2D RID: 3629
		public KeyCode[] Bash = new KeyCode[0];

		// Token: 0x04000E2E RID: 3630
		public KeyCode[] Glide = new KeyCode[0];

		// Token: 0x04000E2F RID: 3631
		public KeyCode[] ChargeJump = new KeyCode[0];

		// Token: 0x04000E30 RID: 3632
		public KeyCode[] Select = new KeyCode[0];

		// Token: 0x04000E31 RID: 3633
		public KeyCode[] Start = new KeyCode[0];

		// Token: 0x04000E32 RID: 3634
		public KeyCode[] Cancel = new KeyCode[0];

		// Token: 0x04000E33 RID: 3635
		public KeyCode[] LeftShoulder = new KeyCode[0];

		// Token: 0x04000E34 RID: 3636
		public KeyCode[] RightShoulder = new KeyCode[0];

		// Token: 0x04000E35 RID: 3637
		public KeyCode[] LeftStick = new KeyCode[0];

		// Token: 0x04000E36 RID: 3638
		public KeyCode[] RightStick = new KeyCode[0];

		// Token: 0x04000E37 RID: 3639
		public KeyCode[] ZoomIn = new KeyCode[0];

		// Token: 0x04000E38 RID: 3640
		public KeyCode[] ZoomOut = new KeyCode[0];

		// Token: 0x04000E39 RID: 3641
		public KeyCode[] Copy = new KeyCode[0];

		// Token: 0x04000E3A RID: 3642
		public KeyCode[] Delete = new KeyCode[0];

		// Token: 0x04000E3B RID: 3643
		public KeyCode[] Focus = new KeyCode[0];

		// Token: 0x04000E3C RID: 3644
		public KeyCode[] Filter = new KeyCode[0];

		// Token: 0x04000E3D RID: 3645
		public KeyCode[] Legend = new KeyCode[0];
	}
}
