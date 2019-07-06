using System;
using System.Collections.Generic;
using System.IO;
using SmartInput;
using UnityEngine;

// Token: 0x020002F9 RID: 761
public class PlayerInputRebinding
{
	// Token: 0x06000EB8 RID: 3768 RVA: 0x0005B87C File Offset: 0x00059A7C
	static PlayerInputRebinding()
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
			13
		};
		PlayerInputRebinding.m_hasReadControllerRemappingsFile = false;
		PlayerInputRebinding.intToButton = new XboxControllerInput.Button[]
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
	}

	// Token: 0x1700022F RID: 559
	// (get) Token: 0x06000EB9 RID: 3769 RVA: 0x0000CB09 File Offset: 0x0000AD09
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
	// (get) Token: 0x06000EBA RID: 3770 RVA: 0x0000CB34 File Offset: 0x0000AD34
	private static string KeyRebindingFile
	{
		get
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, PlayerInputRebinding.keyRebindingFileName);
		}
	}

	// Token: 0x06000EBB RID: 3771 RVA: 0x0005B910 File Offset: 0x00059B10
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
				try
				{
					PlayerInputRebinding.m_keyRebindings.Stomp = PlayerInputRebinding.StringToKeyBinding(streamReader.ReadLine());
				}
				catch
				{
					PlayerInputRebinding.m_keyRebindings.Stomp = new KeyCode[]
					{
						KeyCode.S
					};
				}
			}
		}
		catch (Exception)
		{
			PlayerInputRebinding.SetDefaultKeyBindingSettings();
		}
	}

	// Token: 0x06000EBC RID: 3772 RVA: 0x0005BC0C File Offset: 0x00059E0C
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

	// Token: 0x06000EBD RID: 3773 RVA: 0x0005BC84 File Offset: 0x00059E84
	public static void WriteKeyRebindSettings()
	{
		using (StreamWriter streamWriter = new StreamWriter(new FileStream(PlayerInputRebinding.KeyRebindingFile, FileMode.OpenOrCreate)))
		{
			PlayerInputRebinding.KeyBindingSettings keyRebindings = PlayerInputRebinding.m_keyRebindings;
			streamWriter.WriteLine("Keyboard Rebindings");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Activate Key Rebinding: " + keyRebindings.IsRebinding.ToString());
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
			streamWriter.WriteLine("Stomp: " + PlayerInputRebinding.KeyBindingToString(keyRebindings.Stomp));
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

	// Token: 0x06000EBE RID: 3774 RVA: 0x0005C10C File Offset: 0x0005A30C
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

	// Token: 0x06000EBF RID: 3775 RVA: 0x0000CB45 File Offset: 0x0000AD45
	public static void SetDefaultKeyBindingSettings()
	{
		PlayerInputRebinding.m_keyRebindings = PlayerInputRebinding.DefaultKeyBindingSettings();
	}

	// Token: 0x06000EC0 RID: 3776 RVA: 0x0005C160 File Offset: 0x0005A360
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
		keyBindingSettings.Stomp = new KeyCode[]
		{
			KeyCode.S
		};
		return keyBindingSettings;
	}

	// Token: 0x17000231 RID: 561
	// (get) Token: 0x06000EC1 RID: 3777 RVA: 0x0000CB51 File Offset: 0x0000AD51
	private static string ControllerRemappingFile
	{
		get
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, PlayerInputRebinding.controllerRebindingFileName);
		}
	}

	// Token: 0x06000EC2 RID: 3778 RVA: 0x0000CB62 File Offset: 0x0000AD62
	public static XboxControllerInput.Button GetRemappedJoystickButton(XboxControllerInput.Button joystickButtonIndex)
	{
		return PlayerInputRebinding.intToButton[PlayerInputRebinding.m_controllerButtonRemappings[PlayerInputRebinding.ButtonToInt(joystickButtonIndex)]];
	}

	// Token: 0x06000EC3 RID: 3779 RVA: 0x0005C508 File Offset: 0x0005A708
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
				bool flag = bool.Parse(streamReader.ReadLine().Split(new string[]
				{
					": "
				}, StringSplitOptions.None)[1]);
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
		catch (Exception)
		{
			PlayerInputRebinding.SetDefaultControllerButtonRemappings();
		}
	}

	// Token: 0x06000EC4 RID: 3780 RVA: 0x0005C80C File Offset: 0x0005AA0C
	public static void WriteControllerButtonRemappings()
	{
		using (StreamWriter streamWriter = new StreamWriter(new FileStream(PlayerInputRebinding.ControllerRemappingFile, FileMode.OpenOrCreate)))
		{
			streamWriter.WriteLine("Controller Button Remapping - remaps controller buttons to different DirectInput button IDs");
			streamWriter.WriteLine("Only for DirectInput controllers");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Activate DirectInput Button Rebinding: " + PlayerInputRebinding.m_controllerIsRemappingButtons.ToString());
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

	// Token: 0x06000EC5 RID: 3781 RVA: 0x0000CB76 File Offset: 0x0000AD76
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

	// Token: 0x06000EC6 RID: 3782 RVA: 0x0000CBA2 File Offset: 0x0000ADA2
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

	// Token: 0x06000EC7 RID: 3783 RVA: 0x0005CAA8 File Offset: 0x0005ACA8
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

	// Token: 0x06000EC8 RID: 3784 RVA: 0x0005CB18 File Offset: 0x0005AD18
	public static void GetControllerRebindSettingsFromFile()
	{
		try
		{
			using (StreamReader streamReader = new StreamReader(new FileStream(PlayerInputRebinding.ControllerRebindingFile, FileMode.Open)))
			{
				streamReader.ReadLine();
				streamReader.ReadLine();
				PlayerInputRebinding.m_controllerRebindings = new PlayerInputRebinding.ControllerBindingSettings
				{
					HorizontalDigiPadLeft = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					HorizontalDigiPadRight = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					VerticalDigiPadDown = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					VerticalDigiPadUp = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					MenuLeft = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					MenuRight = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					MenuDown = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					MenuUp = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					MenuPageLeft = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					MenuPageRight = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					ActionButtonA = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					SoulFlame = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Jump = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Grab = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					SpiritFlame = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Bash = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Glide = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					ChargeJump = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Select = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Start = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Cancel = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					LeftShoulder = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					RightShoulder = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					LeftStick = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					RightStick = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					ZoomIn = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					ZoomOut = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Copy = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Delete = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Focus = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Filter = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Legend = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine()),
					Stomp = PlayerInputRebinding.StringToControllerBinding(streamReader.ReadLine())
				};
			}
		}
		catch (Exception)
		{
			PlayerInputRebinding.SetDefaultControllerBindingSettings();
		}
	}

	// Token: 0x06000EC9 RID: 3785 RVA: 0x0005CDCC File Offset: 0x0005AFCC
	public static PlayerInputRebinding.ControllerButton[] StringToControllerBinding(string s)
	{
		s = s.Split(new string[]
		{
			": "
		}, StringSplitOptions.None)[1];
		string[] array = s.Split(new string[]
		{
			", "
		}, StringSplitOptions.RemoveEmptyEntries);
		List<PlayerInputRebinding.ControllerButton> list = new List<PlayerInputRebinding.ControllerButton>();
		foreach (string value in array)
		{
			list.Add((PlayerInputRebinding.ControllerButton)((int)Enum.Parse(typeof(PlayerInputRebinding.ControllerButton), value)));
		}
		return list.ToArray();
	}

	// Token: 0x06000ECA RID: 3786 RVA: 0x0005CE44 File Offset: 0x0005B044
	public static void WriteControllerRebindSettings()
	{
		using (StreamWriter streamWriter = new StreamWriter(new FileStream(PlayerInputRebinding.ControllerRebindingFile, FileMode.OpenOrCreate)))
		{
			PlayerInputRebinding.ControllerBindingSettings controllerRebindings = PlayerInputRebinding.m_controllerRebindings;
			streamWriter.WriteLine("Controller Rebindings");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Movement Left: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.HorizontalDigiPadLeft));
			streamWriter.WriteLine("Movement Right: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.HorizontalDigiPadRight));
			streamWriter.WriteLine("Movement Down: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.VerticalDigiPadDown));
			streamWriter.WriteLine("Movement Up: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.VerticalDigiPadUp));
			streamWriter.WriteLine("Menu Left: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.MenuLeft));
			streamWriter.WriteLine("Menu Right: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.MenuRight));
			streamWriter.WriteLine("Menu Down: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.MenuDown));
			streamWriter.WriteLine("Menu Up: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.MenuUp));
			streamWriter.WriteLine("Menu Previous: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.MenuPageLeft));
			streamWriter.WriteLine("Menu Next: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.MenuPageRight));
			streamWriter.WriteLine("Proceed: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.ActionButtonA));
			streamWriter.WriteLine("Soul Link: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.SoulFlame));
			streamWriter.WriteLine("Jump: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Jump));
			streamWriter.WriteLine("Grab: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Grab));
			streamWriter.WriteLine("Spirit Flame: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.SpiritFlame));
			streamWriter.WriteLine("Bash: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Bash));
			streamWriter.WriteLine("Glide: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Glide));
			streamWriter.WriteLine("Charge Jump: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.ChargeJump));
			streamWriter.WriteLine("Map: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Select));
			streamWriter.WriteLine("Start: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Start));
			streamWriter.WriteLine("Cancel: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Cancel));
			streamWriter.WriteLine("Grenade: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.LeftShoulder));
			streamWriter.WriteLine("Dash: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.RightShoulder));
			streamWriter.WriteLine("Left Stick: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.LeftStick));
			streamWriter.WriteLine("Debug Menu (shhh): " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.RightStick));
			streamWriter.WriteLine("Zoom In World Map: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.ZoomIn));
			streamWriter.WriteLine("Zoom Out World Map: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.ZoomOut));
			streamWriter.WriteLine("Copy: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Copy));
			streamWriter.WriteLine("Delete: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Delete));
			streamWriter.WriteLine("Focus: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Focus));
			streamWriter.WriteLine("Filter: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Filter));
			streamWriter.WriteLine("Legend: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Legend));
			streamWriter.WriteLine("Stomp: " + PlayerInputRebinding.ControllerBindingToString(controllerRebindings.Stomp));
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Usage:");
			streamWriter.WriteLine("- There is no guarantee of the game still being playable after key rebinding. Please use with caution and delete this file in case of breakage");
			streamWriter.WriteLine("- Spelling and syntactical errors will result in the key rebindings not registering properly, and the game will get set to default");
			streamWriter.WriteLine("- Deleting this file will result in this file being recreated by the game, containing the default settings");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Don't forget to restart the game after editing this file!");
			streamWriter.WriteLine("Don't forget to close this file before restarting the game!");
			streamWriter.WriteLine("--------");
			streamWriter.WriteLine("Movement is always on the left thumbstick");
			streamWriter.WriteLine("LLeft, LRight, LUp, LDown is for the left thumbstick");
			streamWriter.WriteLine("RLeft, RRight, RUp, RDown is for the right thumbstick");
			streamWriter.WriteLine("DLeft, DRight, DUp, DDown is for the D-Pad");
			streamWriter.WriteLine("Based on an xbox controller - i.e. A, B, X, Y, LT, RT, LB, RB, LS, RS correspond to Cross, Circle, Square, Triangle, L2, R2, L1, R1, L3, R3");
			streamWriter.WriteLine("Not guaranteed to work on controllers other than xbox");
		}
	}

	// Token: 0x06000ECB RID: 3787 RVA: 0x0005D2C8 File Offset: 0x0005B4C8
	public static string ControllerBindingToString(PlayerInputRebinding.ControllerButton[] codes)
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

	// Token: 0x06000ECC RID: 3788 RVA: 0x0000CBBB File Offset: 0x0000ADBB
	public static void SetDefaultControllerBindingSettings()
	{
		PlayerInputRebinding.m_controllerRebindings = PlayerInputRebinding.DefaultControllerBindingSettings();
	}

	// Token: 0x06000ECD RID: 3789 RVA: 0x0005D31C File Offset: 0x0005B51C
	public static PlayerInputRebinding.ControllerBindingSettings DefaultControllerBindingSettings()
	{
		return new PlayerInputRebinding.ControllerBindingSettings
		{
			HorizontalDigiPadLeft = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.DLeft
			},
			HorizontalDigiPadRight = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.DRight
			},
			VerticalDigiPadUp = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.DUp
			},
			VerticalDigiPadDown = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.DDown
			},
			ActionButtonA = new PlayerInputRebinding.ControllerButton[1],
			Bash = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.Y
			},
			Cancel = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.B
			},
			ChargeJump = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LT
			},
			Copy = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.X
			},
			Delete = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.Y
			},
			Filter = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.X
			},
			Focus = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.X
			},
			Glide = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.RT
			},
			Grab = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.RT
			},
			Jump = new PlayerInputRebinding.ControllerButton[1],
			LeftShoulder = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LB
			},
			LeftStick = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LS
			},
			Legend = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.Y
			},
			MenuDown = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LDown,
				PlayerInputRebinding.ControllerButton.DDown
			},
			MenuLeft = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LLeft,
				PlayerInputRebinding.ControllerButton.DLeft
			},
			MenuPageLeft = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LT
			},
			MenuPageRight = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.RT
			},
			MenuRight = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LRight,
				PlayerInputRebinding.ControllerButton.DRight
			},
			MenuUp = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LUp,
				PlayerInputRebinding.ControllerButton.DUp
			},
			RightShoulder = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.RB
			},
			RightStick = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.RS
			},
			Select = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.Back
			},
			SoulFlame = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.B
			},
			SpiritFlame = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.X
			},
			Start = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.Start
			},
			Stomp = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LDown,
				PlayerInputRebinding.ControllerButton.DDown
			},
			ZoomIn = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.RT
			},
			ZoomOut = new PlayerInputRebinding.ControllerButton[]
			{
				PlayerInputRebinding.ControllerButton.LT
			}
		};
	}

	// Token: 0x17000232 RID: 562
	// (get) Token: 0x06000ECE RID: 3790 RVA: 0x0000CBC7 File Offset: 0x0000ADC7
	public static PlayerInputRebinding.ControllerBindingSettings ControllerRebindings
	{
		get
		{
			if (PlayerInputRebinding.m_controllerRebindings == null)
			{
				if (!File.Exists(PlayerInputRebinding.ControllerRebindingFile))
				{
					PlayerInputRebinding.SetDefaultControllerBindingSettings();
					PlayerInputRebinding.WriteControllerRebindSettings();
				}
				else
				{
					PlayerInputRebinding.GetControllerRebindSettingsFromFile();
				}
			}
			return PlayerInputRebinding.m_controllerRebindings;
		}
	}

	// Token: 0x17000233 RID: 563
	// (get) Token: 0x06000ECF RID: 3791 RVA: 0x0000CBF2 File Offset: 0x0000ADF2
	public static string ControllerRebindingFile
	{
		get
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, PlayerInputRebinding.controllerInputRebindingsFileName);
		}
	}

	// Token: 0x04000E18 RID: 3608
	private static string keyRebindingFileName = "KeyRebindings.txt";

	// Token: 0x04000E19 RID: 3609
	private static string controllerRebindingFileName = "ControllerButtonRemaps.txt";

	// Token: 0x04000E1A RID: 3610
	private static PlayerInputRebinding.KeyBindingSettings m_keyRebindings;

	// Token: 0x04000E1B RID: 3611
	private static int[] m_controllerButtonRemappings;

	// Token: 0x04000E1C RID: 3612
	private static bool m_controllerIsRemappingButtons;

	// Token: 0x04000E1D RID: 3613
	private static bool m_hasReadControllerRemappingsFile;

	// Token: 0x04000E1E RID: 3614
	private static XboxControllerInput.Button[] intToButton;

	// Token: 0x04000E1F RID: 3615
	public static string controllerInputRebindingsFileName = "ControllerRebindings.txt";

	// Token: 0x04000E20 RID: 3616
	public static PlayerInputRebinding.ControllerBindingSettings m_controllerRebindings;

	// Token: 0x020002FA RID: 762
	public class KeyBindingSettings
	{
		// Token: 0x06000ED0 RID: 3792 RVA: 0x0005D560 File Offset: 0x0005B760
		public KeyBindingSettings()
		{
			this.Glide = new KeyCode[0];
			this.ChargeJump = new KeyCode[0];
			this.Select = new KeyCode[0];
			this.Start = new KeyCode[0];
			this.Cancel = new KeyCode[0];
			this.LeftShoulder = new KeyCode[0];
			this.RightShoulder = new KeyCode[0];
			this.LeftStick = new KeyCode[0];
			this.RightStick = new KeyCode[0];
			this.ZoomIn = new KeyCode[0];
			this.ZoomOut = new KeyCode[0];
			this.Copy = new KeyCode[0];
			this.Delete = new KeyCode[0];
			this.Focus = new KeyCode[0];
			this.Filter = new KeyCode[0];
			this.Legend = new KeyCode[0];
		}

		// Token: 0x04000E21 RID: 3617
		public bool IsRebinding;

		// Token: 0x04000E22 RID: 3618
		public KeyCode[] HorizontalDigiPadLeft = new KeyCode[0];

		// Token: 0x04000E23 RID: 3619
		public KeyCode[] HorizontalDigiPadRight = new KeyCode[0];

		// Token: 0x04000E24 RID: 3620
		public KeyCode[] VerticalDigiPadDown = new KeyCode[0];

		// Token: 0x04000E25 RID: 3621
		public KeyCode[] VerticalDigiPadUp = new KeyCode[0];

		// Token: 0x04000E26 RID: 3622
		public KeyCode[] MenuLeft = new KeyCode[0];

		// Token: 0x04000E27 RID: 3623
		public KeyCode[] MenuRight = new KeyCode[0];

		// Token: 0x04000E28 RID: 3624
		public KeyCode[] MenuDown = new KeyCode[0];

		// Token: 0x04000E29 RID: 3625
		public KeyCode[] MenuUp = new KeyCode[0];

		// Token: 0x04000E2A RID: 3626
		public KeyCode[] MenuPageLeft = new KeyCode[0];

		// Token: 0x04000E2B RID: 3627
		public KeyCode[] MenuPageRight = new KeyCode[0];

		// Token: 0x04000E2C RID: 3628
		public KeyCode[] ActionButtonA = new KeyCode[0];

		// Token: 0x04000E2D RID: 3629
		public KeyCode[] SoulFlame = new KeyCode[0];

		// Token: 0x04000E2E RID: 3630
		public KeyCode[] Jump = new KeyCode[0];

		// Token: 0x04000E2F RID: 3631
		public KeyCode[] Grab = new KeyCode[0];

		// Token: 0x04000E30 RID: 3632
		public KeyCode[] SpiritFlame = new KeyCode[0];

		// Token: 0x04000E31 RID: 3633
		public KeyCode[] Bash = new KeyCode[0];

		// Token: 0x04000E32 RID: 3634
		public KeyCode[] Glide;

		// Token: 0x04000E33 RID: 3635
		public KeyCode[] ChargeJump;

		// Token: 0x04000E34 RID: 3636
		public KeyCode[] Select;

		// Token: 0x04000E35 RID: 3637
		public KeyCode[] Start;

		// Token: 0x04000E36 RID: 3638
		public KeyCode[] Cancel;

		// Token: 0x04000E37 RID: 3639
		public KeyCode[] LeftShoulder;

		// Token: 0x04000E38 RID: 3640
		public KeyCode[] RightShoulder;

		// Token: 0x04000E39 RID: 3641
		public KeyCode[] LeftStick;

		// Token: 0x04000E3A RID: 3642
		public KeyCode[] RightStick;

		// Token: 0x04000E3B RID: 3643
		public KeyCode[] ZoomIn;

		// Token: 0x04000E3C RID: 3644
		public KeyCode[] ZoomOut;

		// Token: 0x04000E3D RID: 3645
		public KeyCode[] Copy;

		// Token: 0x04000E3E RID: 3646
		public KeyCode[] Delete;

		// Token: 0x04000E3F RID: 3647
		public KeyCode[] Focus;

		// Token: 0x04000E40 RID: 3648
		public KeyCode[] Filter;

		// Token: 0x04000E41 RID: 3649
		public KeyCode[] Legend;

		// Token: 0x04000E42 RID: 3650
		public KeyCode[] Stomp = new KeyCode[0];
	}

	// Token: 0x020002FB RID: 763
	public enum ControllerButton
	{
		// Token: 0x04000E44 RID: 3652
		A,
		// Token: 0x04000E45 RID: 3653
		B,
		// Token: 0x04000E46 RID: 3654
		X,
		// Token: 0x04000E47 RID: 3655
		Y,
		// Token: 0x04000E48 RID: 3656
		LT,
		// Token: 0x04000E49 RID: 3657
		RT,
		// Token: 0x04000E4A RID: 3658
		LB,
		// Token: 0x04000E4B RID: 3659
		RB,
		// Token: 0x04000E4C RID: 3660
		LS,
		// Token: 0x04000E4D RID: 3661
		RS,
		// Token: 0x04000E4E RID: 3662
		LUp,
		// Token: 0x04000E4F RID: 3663
		LDown,
		// Token: 0x04000E50 RID: 3664
		LLeft,
		// Token: 0x04000E51 RID: 3665
		LRight,
		// Token: 0x04000E52 RID: 3666
		DUp,
		// Token: 0x04000E53 RID: 3667
		DDown,
		// Token: 0x04000E54 RID: 3668
		DLeft,
		// Token: 0x04000E55 RID: 3669
		DRight,
		// Token: 0x04000E56 RID: 3670
		RUp,
		// Token: 0x04000E57 RID: 3671
		RDown,
		// Token: 0x04000E58 RID: 3672
		RLeft,
		// Token: 0x04000E59 RID: 3673
		RRight,
		// Token: 0x04000E5A RID: 3674
		Back,
		// Token: 0x04000E5B RID: 3675
		Start
	}

	// Token: 0x020002FC RID: 764
	public class ControllerBindingSettings
	{
		// Token: 0x04000E5C RID: 3676
		public PlayerInputRebinding.ControllerButton[] HorizontalDigiPadLeft = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E5D RID: 3677
		public PlayerInputRebinding.ControllerButton[] HorizontalDigiPadRight = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E5E RID: 3678
		public PlayerInputRebinding.ControllerButton[] VerticalDigiPadDown = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E5F RID: 3679
		public PlayerInputRebinding.ControllerButton[] VerticalDigiPadUp = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E60 RID: 3680
		public PlayerInputRebinding.ControllerButton[] MenuLeft = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E61 RID: 3681
		public PlayerInputRebinding.ControllerButton[] MenuRight = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E62 RID: 3682
		public PlayerInputRebinding.ControllerButton[] MenuDown = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E63 RID: 3683
		public PlayerInputRebinding.ControllerButton[] MenuUp = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E64 RID: 3684
		public PlayerInputRebinding.ControllerButton[] MenuPageLeft = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E65 RID: 3685
		public PlayerInputRebinding.ControllerButton[] MenuPageRight = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E66 RID: 3686
		public PlayerInputRebinding.ControllerButton[] ActionButtonA = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E67 RID: 3687
		public PlayerInputRebinding.ControllerButton[] SoulFlame = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E68 RID: 3688
		public PlayerInputRebinding.ControllerButton[] Jump = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E69 RID: 3689
		public PlayerInputRebinding.ControllerButton[] Grab = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E6A RID: 3690
		public PlayerInputRebinding.ControllerButton[] SpiritFlame = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E6B RID: 3691
		public PlayerInputRebinding.ControllerButton[] Bash = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E6C RID: 3692
		public PlayerInputRebinding.ControllerButton[] Glide = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E6D RID: 3693
		public PlayerInputRebinding.ControllerButton[] ChargeJump = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E6E RID: 3694
		public PlayerInputRebinding.ControllerButton[] Select = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E6F RID: 3695
		public PlayerInputRebinding.ControllerButton[] Start = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E70 RID: 3696
		public PlayerInputRebinding.ControllerButton[] Cancel = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E71 RID: 3697
		public PlayerInputRebinding.ControllerButton[] LeftShoulder = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E72 RID: 3698
		public PlayerInputRebinding.ControllerButton[] RightShoulder = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E73 RID: 3699
		public PlayerInputRebinding.ControllerButton[] LeftStick = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E74 RID: 3700
		public PlayerInputRebinding.ControllerButton[] RightStick = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E75 RID: 3701
		public PlayerInputRebinding.ControllerButton[] ZoomIn = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E76 RID: 3702
		public PlayerInputRebinding.ControllerButton[] ZoomOut = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E77 RID: 3703
		public PlayerInputRebinding.ControllerButton[] Copy = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E78 RID: 3704
		public PlayerInputRebinding.ControllerButton[] Delete = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E79 RID: 3705
		public PlayerInputRebinding.ControllerButton[] Focus = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E7A RID: 3706
		public PlayerInputRebinding.ControllerButton[] Filter = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E7B RID: 3707
		public PlayerInputRebinding.ControllerButton[] Legend = new PlayerInputRebinding.ControllerButton[0];

		// Token: 0x04000E7C RID: 3708
		public PlayerInputRebinding.ControllerButton[] Stomp = new PlayerInputRebinding.ControllerButton[0];
	}
}
