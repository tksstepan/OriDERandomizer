using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using UnityEngine;

public static class RandomizerRebinding
{
	public static void WriteDefaultFile()
	{
		StreamWriter streamWriter = new StreamWriter("RandomizerRebinding.txt");
		streamWriter.WriteLine("Bind syntax: Key1+Key2, Key1+Key3+Key4, ... Syntax errors will load default binds.");
		streamWriter.WriteLine("Functions are unbound if there is no binding specified.");
		streamWriter.WriteLine("Supported binds are Unity KeyCodes (https://docs.unity3d.com/ScriptReference/KeyCode.html) and the following actions:");
		streamWriter.WriteLine("Jump, SpiritFlame, Bash, SoulFlame, ChargeJump, Glide, Dash, Grenade, Left, Right, Up, Down, LeftStick, RightStick, Start, Select");
		streamWriter.WriteLine("");
		streamWriter.WriteLine("In addition, Xbox-style controller buttons are supported, and must be preceded by an underscore:");
		streamWriter.WriteLine("_A (bottom), _B (right), _X (left), _Y (top), _LB, _RB, _LT, _RT, _DUp, _DDown, _DLeft, _DRight,");
		streamWriter.WriteLine("_LUp, _LDown, _LLeft, _LRight, _RUp, _RDown, _RLeft, _RRight, _LS, _RS, _Start, _Select");
		streamWriter.WriteLine("--------");
		foreach(KeyValuePair<string, string> lineparts in DefaultBinds)
		{
			streamWriter.WriteLine(lineparts.Key + ": " + lineparts.Value);
		}
		streamWriter.Flush();
		streamWriter.Close();
	}

	public static void ParseRebinding()
	{
		try
		{
			CoreInputMap = new Dictionary<string, Core.Input.InputButtonProcessor>
			{
				{"Jump", Core.Input.Jump},
				{"SpiritFlame", Core.Input.SpiritFlame},
				{"Bash", Core.Input.Bash},
				{"SoulFlame", Core.Input.SoulFlame},
				{"ChargeJump", Core.Input.ChargeJump},
				{"Glide", Core.Input.Glide},
				{"Dash", Core.Input.RightShoulder},
				{"Grenade", Core.Input.LeftShoulder},
				{"Left", Core.Input.Left},
				{"Right", Core.Input.Right},
				{"Up", Core.Input.Up},
				{"Down", Core.Input.Down},
				{"LeftStick", Core.Input.LeftStick},
				{"RightStick", Core.Input.RightStick},
				{"Start", Core.Input.Start},
				{"Select", Core.Input.Select}
			};

			DefaultBinds = new Dictionary<string, string>
			{
				{"Replay Message", "LeftAlt+T, RightAlt+T"},
				{"Return to Start","LeftAlt+R, RightAlt+R"},
				{"Reload Seed", "LeftAlt+L, RightAlt+L"},
				{"Toggle Chaos", "LeftAlt+K, RightAlt+K"},
				{"Chaos Verbosity", "LeftAlt+V, RightAlt+V"},
				{"Force Chaos Effect","LeftAlt+F, RightAlt+F"},
				{"Show Progress", "LeftAlt+P, RightAlt+P"},
				{"Color Shift", "LeftAlt+C, RightAlt+C"},
				{"Double Bash", "Grenade"},
				{"Toggle Map Mode", "Grenade"},
				{"Free Grenade Jump", "Grenade+Jump"},
				{"Show Bonuses", "LeftAlt+B, RightAlt+B"},
				{"Bonus Switch", "LeftAlt+Q, RightAlt+Q"},
				{"Bonus Toggle", "LeftAlt+Mouse1, RightAlt+Mouse1"},
				{"Reset Grenade Aim",""},
				{"Suppress Autofire",""},
				{"List Trees", "LeftAlt+Alpha1, RightAlt+Alpha1"},
				{"List Map Altars", "LeftAlt+Alpha2, RightAlt+Alpha2"},
				{"List Teleporters", "LeftAlt+Alpha3, RightAlt+Alpha3"},
				{"List Relics", "LeftAlt+Alpha4, RightAlt+Alpha4"},
				{"Show Stats", "LeftAlt+Alpha5, RightAlt+Alpha5"},
				{"Bonus 1",""},
				{"Bonus 2",""},
				{"Bonus 3",""},
				{"Bonus 4",""},
				{"Bonus 5",""},
				{"Bonus 6",""},
				{"Bonus 7",""},
				{"Bonus 8",""},
				{"Bonus 9",""},
			};

			if (!File.Exists("RandomizerRebinding.txt"))
			{
				RandomizerRebinding.WriteDefaultFile();
			}

			string[] lines = File.ReadAllLines("RandomizerRebinding.txt");
			ArrayList unseenActions = new ArrayList(RandomizerRebinding.DefaultBinds.Keys);
			List<string> writeList = new List<string>();

			// parse step 1: read binds from file
			foreach (string line in lines)
			{
				if (!line.Contains(":"))
				{
					continue;
				}
				string[] parts = line.Split(new char[]{':'}, 2);
				string action = parts[0].Trim();
				if (!DefaultBinds.ContainsKey(action))
				{
					continue;
				}
				string bindingString = parts[1].Trim();
				AssignBind(action, bindingString, writeList);
				unseenActions.Remove(action);
			}

			// parse step 2: load defaults for missing binds
			foreach (string missingAction in unseenActions)
			{
				AssignBind(missingAction, null, writeList);
			}

			if (writeList.Count > 0)
			{
				List<string> warnList = new List<string>();

				foreach (string writeAction in writeList)
				{
					if (DefaultBinds[writeAction] != "")
					{
						warnList.Add(writeAction);
					}
				}

				if (warnList.Count > 0)
				{
					Randomizer.printInfo("Default Binds written for these missing binds: " + String.Join(", ", warnList.ToArray()) + ".", 480);
				}

				string writeText = "";
				foreach (string writeAction in writeList)
				{
					writeText += Environment.NewLine + writeAction + ": " + DefaultBinds[writeAction];
				}

				File.AppendAllText("RandomizerRebinding.txt", writeText);
			}
		}
		catch (Exception e)
		{
			Randomizer.LogError("Error parsing bindings: " + e.Message);
		}
	}

	public static void AssignBind(string action, string bindingString, List<string> writeList)
	{
		if (!rebindMap.ContainsKey(action))
		{
			return;
		}

		rebindMap[action].Binds = ParseOrDefault(action, bindingString, writeList).Binds;
		rebindMap[action].deprecated_wasPressed = true;
	}

	public static RandomizerRebinding.BindSet ParseOrDefault(string action, string bindingString, List<string> writeList)
	{
		string defaultBinds = DefaultBinds[action];
		if (bindingString == null)
		{
			bindingString = defaultBinds;
			writeList.Add(action);
		}

		try
		{
			return ParseBinds(action, bindingString);
		}
		catch (Exception)
		{
			Randomizer.printInfo("@" + action + ": failed to parse '" + bindingString + "'. Using default value: '" + defaultBinds + "'@", 240);
			bindingString = defaultBinds;
		}

		return ParseBinds(action, bindingString);
	}

	public static KeyCode StringToKeyBinding(string s)
	{
		if (s != "")
		{
			return (KeyCode)Enum.Parse(typeof(KeyCode), s, true);
		}
		return KeyCode.None;
	}

	public static RandomizerRebinding.BindSet ParseBinds(string action, string bindingString)
	{
		List<RandomizerRebinding.SingleBind> binds = new List<RandomizerRebinding.SingleBind>();

		foreach (string bind in bindingString.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries))
		{
			List<RandomizerRebinding.SingleInput> singleBind = new List<RandomizerRebinding.SingleInput>();
			foreach (string input in bind.Trim().Split(new char[]{'+'}, StringSplitOptions.RemoveEmptyEntries))
			{
				if (action == "Double Bash" && input.Trim().ToLower() == "tap")
				{
					Randomizer.BashTap = true;
				}
				else
				{
					singleBind.Add(new RandomizerRebinding.SingleInput(input.Trim()));
				}
			}
			if (singleBind.Count > 0)
			{
				binds.Add(new RandomizerRebinding.SingleBind(singleBind));
			}
		}

		return new RandomizerRebinding.BindSet(binds);
	}

	public static void FixedUpdate()
	{
		foreach (RandomizerRebinding.BindSet bindSet in RandomizerRebinding.rebindMap.Values)
		{
			bindSet.FixedUpdate();
		}
	}

	public static Dictionary<string, Core.Input.InputButtonProcessor> CoreInputMap;
	public static Dictionary<string, string> DefaultBinds;

	public static RandomizerRebinding.BindSet ReplayMessage = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ReturnToStart = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ReloadSeed = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ToggleChaos = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ChaosVerbosity = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ForceChaosEffect = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ShowProgress = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ColorShift = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet DoubleBash = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet FreeGrenadeJump = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ToggleMapMode = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ShowBonuses = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet BonusSwitch = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet BonusToggle = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ResetGrenadeAim = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet SuppressAutofire = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ListTrees = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ListRelics = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ListMapAltars = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ListTeleporters = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet ShowStats = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet Bonus1 = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet Bonus2 = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet Bonus3 = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet Bonus4 = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet Bonus5 = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet Bonus6 = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet Bonus7 = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet Bonus8 = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());
	public static RandomizerRebinding.BindSet Bonus9 = new RandomizerRebinding.BindSet(new List<RandomizerRebinding.SingleBind>());

	private static Dictionary<string, RandomizerRebinding.BindSet> rebindMap = new Dictionary<string, RandomizerRebinding.BindSet>
	{
		{"Replay Message", RandomizerRebinding.ReplayMessage},
		{"Return to Start", RandomizerRebinding.ReturnToStart},
		{"Reload Seed", RandomizerRebinding.ReloadSeed},
		{"Toggle Chaos", RandomizerRebinding.ToggleChaos},
		{"Chaos Verbosity", RandomizerRebinding.ChaosVerbosity},
		{"Force Chaos Effect", RandomizerRebinding.ForceChaosEffect},
		{"Show Progress", RandomizerRebinding.ShowProgress},
		{"Color Shift", RandomizerRebinding.ColorShift},
		{"Double Bash", RandomizerRebinding.DoubleBash},
		{"Free Grenade Jump", RandomizerRebinding.FreeGrenadeJump},
		{"Toggle Map Mode", RandomizerRebinding.ToggleMapMode},
		{"Show Bonuses", RandomizerRebinding.ShowBonuses},
		{"Bonus Switch", RandomizerRebinding.BonusSwitch},
		{"Bonus Toggle", RandomizerRebinding.BonusToggle},
		{"Reset Grenade Aim", RandomizerRebinding.ResetGrenadeAim},
		{"Suppress Autofire", RandomizerRebinding.SuppressAutofire},
		{"List Trees", RandomizerRebinding.ListTrees},
		{"List Relics", RandomizerRebinding.ListRelics},
		{"List Map Altars", RandomizerRebinding.ListMapAltars},
		{"List Teleporters", RandomizerRebinding.ListTeleporters},
		{"Show Stats", RandomizerRebinding.ShowStats},
		{"Bonus 1", RandomizerRebinding.Bonus1},
		{"Bonus 2", RandomizerRebinding.Bonus2},
		{"Bonus 3", RandomizerRebinding.Bonus3},
		{"Bonus 4", RandomizerRebinding.Bonus4},
		{"Bonus 5", RandomizerRebinding.Bonus5},
		{"Bonus 6", RandomizerRebinding.Bonus6},
		{"Bonus 7", RandomizerRebinding.Bonus7},
		{"Bonus 8", RandomizerRebinding.Bonus8},
		{"Bonus 9", RandomizerRebinding.Bonus9}
	};

	private static Dictionary<string, string> coreInputNameRepl = new Dictionary<string, string>
	{
		{"Grenade", "LightSpheres"},
		{"ChargeJump", "ChargeJumpCharge"}
	};

	public class SingleInput : Core.Input.InputButtonProcessor
	{
		public SingleInput(string input)
		{
			raw = input;
			if (input.StartsWith("_"))
			{
				this.Type = ActionType.ControllerButton;
				this.Button = (PlayerInputRebinding.ControllerButton)Enum.Parse(typeof(PlayerInputRebinding.ControllerButton), input.Substring(1), true);
			}
			else if (RandomizerRebinding.CoreInputMap.ContainsKey(input))
			{
				this.Type = ActionType.CoreInput;
				this.CoreInput = RandomizerRebinding.CoreInputMap[input];
			}
			else
			{
				this.Type = ActionType.KeyCode;
				this.Key = RandomizerRebinding.StringToKeyBinding(input);
			}
		}

		public void FixedUpdate()
		{
			switch (this.Type)
			{
			case ActionType.CoreInput:
				this.Update(this.CoreInput.Pressed);
				break;
			case ActionType.ControllerButton:
				this.Update(PlayerInput.Instance.ControllerButtonToButtonInput(this.Button).GetButton());
				break;
			case ActionType.KeyCode:
				this.Update(MoonInput.GetKey(this.Key));
				break;
			}
		}

		public override string ToString()
		{
			switch (this.Type)
			{
			case ActionType.CoreInput:
				return $"[{(coreInputNameRepl.ContainsKey(raw) ? coreInputNameRepl[raw] : raw)}]";
			case ActionType.ControllerButton:
				return "_" + this.Button.ToString();
			case ActionType.KeyCode:
				return this.Key.ToString();
			default:
				return "";
			}
		}

		public KeyCode Key;

		private string raw;

		public Core.Input.InputButtonProcessor CoreInput;

		public PlayerInputRebinding.ControllerButton Button;

		public ActionType Type;

		public enum ActionType
		{
			CoreInput,
			ControllerButton,
			KeyCode
		};
	}

	public class SingleBind : Core.Input.InputButtonProcessor
	{
		public SingleBind(List<RandomizerRebinding.SingleInput> inputs)
		{
			this.Inputs = inputs;
		}

		public void FixedUpdate()
		{
			bool pressed = true;

			foreach (RandomizerRebinding.SingleInput input in this.Inputs)
			{
				input.FixedUpdate();

				if (input.Released)
				{
					pressed = false;
				}
			}

			this.Update(pressed);
		}

		public string ToString()
		{
			return String.Join("+", (from input in this.Inputs select input.ToString()).ToArray());
		}

		public List<RandomizerRebinding.SingleInput> Inputs;
	}

	public class BindSet : Core.Input.InputButtonProcessor
	{
		public BindSet(List<RandomizerRebinding.SingleBind> binds)
		{
			this.deprecated_wasPressed = true;
			this.Binds = binds;
		}

		public string FirstBindName()
		{
			if (this.Binds.Count > 0)
			{
				return this.Binds[0].ToString();
			}

			return "<NO BIND>";
		}

		public bool IsPressed()
		{
			foreach (RandomizerRebinding.SingleBind bind in this.Binds)
			{
				if (bind.Pressed)
				{
					if (this.deprecated_wasPressed)
					{
						return false;
					}
					this.deprecated_wasPressed = true;
					return true;
				}
			}
			this.deprecated_wasPressed = false;
			return false;
		}

		public void FixedUpdate()
		{
			bool pressed = false;

			foreach (RandomizerRebinding.SingleBind bind in this.Binds)
			{
				bind.FixedUpdate();

				if (bind.Pressed)
				{
					pressed = true;
				}
			}

			this.Update(pressed);
		}

		public List<RandomizerRebinding.SingleBind> Binds;

		public bool deprecated_wasPressed;
	}
}