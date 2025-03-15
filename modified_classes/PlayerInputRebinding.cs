using System;
using System.Collections.Generic;
using System.IO;
using SmartInput;
using UnityEngine;

public class PlayerInputRebinding
{
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

	private static string KeyRebindingFile
	{
		get
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, PlayerInputRebinding.keyRebindingFileName);
		}
	}

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

	public static void SetDefaultKeyBindingSettings()
	{
		PlayerInputRebinding.m_keyRebindings = PlayerInputRebinding.DefaultKeyBindingSettings();
	}

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

	private static string ControllerRemappingFile
	{
		get
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, PlayerInputRebinding.controllerRebindingFileName);
		}
	}

	public static XboxControllerInput.Button GetRemappedJoystickButton(XboxControllerInput.Button joystickButtonIndex)
	{
		return PlayerInputRebinding.intToButton[PlayerInputRebinding.m_controllerButtonRemappings[PlayerInputRebinding.ButtonToInt(joystickButtonIndex)]];
	}

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

	public static void SetDefaultControllerBindingSettings()
	{
		PlayerInputRebinding.m_controllerRebindings = PlayerInputRebinding.DefaultControllerBindingSettings();
	}

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

	public static string ControllerRebindingFile
	{
		get
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, PlayerInputRebinding.controllerInputRebindingsFileName);
		}
	}

	private static string keyRebindingFileName = "KeyRebindings.txt";

	private static string controllerRebindingFileName = "ControllerButtonRemaps.txt";

	private static PlayerInputRebinding.KeyBindingSettings m_keyRebindings;

	private static int[] m_controllerButtonRemappings;

	private static bool m_controllerIsRemappingButtons;

	private static bool m_hasReadControllerRemappingsFile;

	private static XboxControllerInput.Button[] intToButton;

	public static string controllerInputRebindingsFileName = "ControllerRebindings.txt";

	public static PlayerInputRebinding.ControllerBindingSettings m_controllerRebindings;

	public class KeyBindingSettings
	{
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

		public bool IsRebinding;

		public KeyCode[] HorizontalDigiPadLeft = new KeyCode[0];

		public KeyCode[] HorizontalDigiPadRight = new KeyCode[0];

		public KeyCode[] VerticalDigiPadDown = new KeyCode[0];

		public KeyCode[] VerticalDigiPadUp = new KeyCode[0];

		public KeyCode[] MenuLeft = new KeyCode[0];

		public KeyCode[] MenuRight = new KeyCode[0];

		public KeyCode[] MenuDown = new KeyCode[0];

		public KeyCode[] MenuUp = new KeyCode[0];

		public KeyCode[] MenuPageLeft = new KeyCode[0];

		public KeyCode[] MenuPageRight = new KeyCode[0];

		public KeyCode[] ActionButtonA = new KeyCode[0];

		public KeyCode[] SoulFlame = new KeyCode[0];

		public KeyCode[] Jump = new KeyCode[0];

		public KeyCode[] Grab = new KeyCode[0];

		public KeyCode[] SpiritFlame = new KeyCode[0];

		public KeyCode[] Bash = new KeyCode[0];

		public KeyCode[] Glide;

		public KeyCode[] ChargeJump;

		public KeyCode[] Select;

		public KeyCode[] Start;

		public KeyCode[] Cancel;

		public KeyCode[] LeftShoulder;

		public KeyCode[] RightShoulder;

		public KeyCode[] LeftStick;

		public KeyCode[] RightStick;

		public KeyCode[] ZoomIn;

		public KeyCode[] ZoomOut;

		public KeyCode[] Copy;

		public KeyCode[] Delete;

		public KeyCode[] Focus;

		public KeyCode[] Filter;

		public KeyCode[] Legend;

		public KeyCode[] Stomp = new KeyCode[0];
	}

	public enum ControllerButton
	{
		A,
		B,
		X,
		Y,
		LT,
		RT,
		LB,
		RB,
		LS,
		RS,
		LUp,
		LDown,
		LLeft,
		LRight,
		DUp,
		DDown,
		DLeft,
		DRight,
		RUp,
		RDown,
		RLeft,
		RRight,
		Back,
		Start
	}

	public class ControllerBindingSettings
	{
		public PlayerInputRebinding.ControllerButton[] HorizontalDigiPadLeft = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] HorizontalDigiPadRight = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] VerticalDigiPadDown = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] VerticalDigiPadUp = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] MenuLeft = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] MenuRight = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] MenuDown = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] MenuUp = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] MenuPageLeft = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] MenuPageRight = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] ActionButtonA = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] SoulFlame = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Jump = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Grab = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] SpiritFlame = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Bash = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Glide = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] ChargeJump = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Select = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Start = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Cancel = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] LeftShoulder = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] RightShoulder = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] LeftStick = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] RightStick = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] ZoomIn = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] ZoomOut = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Copy = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Delete = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Focus = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Filter = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Legend = new PlayerInputRebinding.ControllerButton[0];

		public PlayerInputRebinding.ControllerButton[] Stomp = new PlayerInputRebinding.ControllerButton[0];
	}
}
