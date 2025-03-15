using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using UnityEngine;

public static class RandomizerRebinding {
	public static void WriteBindsToFile() {
		StreamWriter streamWriter = new StreamWriter("RandomizerRebinding.txt");
		streamWriter.WriteLine("Bind syntax: Key1+Key2, Key1+Key3+Key4, ... Syntax errors will load default binds.");
		streamWriter.WriteLine("Functions are unbound if there is no binding specified.");
		streamWriter.WriteLine("Supported binds are Unity KeyCodes (https://docs.unity3d.com/ScriptReference/KeyCode.html) and the following actions:");
		streamWriter.WriteLine("Jump, SpiritFlame, Bash, SoulFlame, ChargeJump, Glide, Dash, Grenade, Left, Right, Up, Down, LeftStick, RightStick, Start, Select");
		streamWriter.WriteLine("");
		streamWriter.WriteLine("In addition, Xbox-style controller buttons are supported, and must be preceded by an underscore:");
		streamWriter.WriteLine("_A (bottom), _B (right), _X (left), _Y (top), _LB, _RB, _LT, _RT, _DUp, _DDown, _DLeft, _DRight,");
		streamWriter.WriteLine("_LUp, _LDown, _LLeft, _LRight, _RUp, _RDown, _RLeft, _RRight, _LS, _RS, _Start, _Back");
		streamWriter.WriteLine("--------");
		streamWriter.WriteLine("");
		foreach(KeyValuePair<string, BindSet> bindparts in rebindMap) {
			if(bindparts.Value.HasBind())
				streamWriter.WriteLine($"{bindparts.Key}: {bindparts.Value}");
			else
				streamWriter.WriteLine($"{bindparts.Key}: {DefaultBinds[bindparts.Key]}");
		}
		streamWriter.Flush();
		streamWriter.Close();
	}


	public static void ParseRebinding() {
		var dirty = false;

		try {
			if (!File.Exists("RandomizerRebinding.txt")) {
				WriteBindsToFile();
			}

			string[] lines = File.ReadAllLines("RandomizerRebinding.txt");
			ArrayList unseenActions = new ArrayList(DefaultBinds.Keys);
			List<string> writeList = new List<string>();

			// parse step 1: read binds from file
			foreach (string line in lines) {
				if (!line.Contains(":")){
					continue;
				}
				string[] parts = line.Split(new char[]{':'}, 2);
				string action = parts[0].Trim();
				if(action == "Free Grenade Jump") {
					action = "Grenade Jump";
					dirty = true;
				} else if(action == "Return to Start") {
					action = "Warp";
					dirty = true;
				}

				if (!DefaultBinds.ContainsKey(action))
					continue;

				string bindingString = parts[1].Trim();
				AssignBind(action, bindingString, writeList);
				unseenActions.Remove(action);
			}

			// parse step 2: load defaults for missing binds
			foreach (string missingAction in unseenActions) {
				AssignBind(missingAction, null, writeList);
			}

			if (writeList.Count > 0) {
				List<string> warnList = new List<string>();

				foreach (string writeAction in writeList) {
					if (DefaultBinds[writeAction] != "") {
						warnList.Add(writeAction);
					}
				}

				if (warnList.Count > 0) {
					Randomizer.printInfo("Default Binds written for these missing binds: " + String.Join(", ", warnList.ToArray()) + ".", 480);
				}

				string writeText = "";
				foreach (string writeAction in writeList) {
					writeText += Environment.NewLine + writeAction + ": " + DefaultBinds[writeAction];
				}

				File.AppendAllText("RandomizerRebinding.txt", writeText);
			} 
			if(dirty) {
				// this is redundant with the above but who cares! :D 
				WriteBindsToFile();
			}

		}
		catch (Exception e) {
			Randomizer.LogError("Error parsing bindings: " + e.Message);
		}
	}

	public static void AssignBind(string action, string bindingString, List<string> writeList) {
		if (!rebindMap.ContainsKey(action)) {
			return;
		}

		rebindMap[action].Binds = ParseOrDefault(action, bindingString, writeList).Binds;
		rebindMap[action].deprecated_wasPressed = true;
	}

	public static BindSet ParseOrDefault(string action, string bindingString, List<string> writeList) {
		string defaultBinds = DefaultBinds[action];
		if (bindingString == null) {
			bindingString = defaultBinds;
			writeList.Add(action);
		}

		try {
			return ParseBinds(action, bindingString);
		}
		catch (Exception) {
			Randomizer.printInfo("@" + action + ": failed to parse '" + bindingString + "'. Using default value: '" + defaultBinds + "'@", 240);
			bindingString = defaultBinds;
		}

		return ParseBinds(action, bindingString);
	}

	public static KeyCode StringToKeyBinding(string s) {
		if (s != "") {
			return (KeyCode)Enum.Parse(typeof(KeyCode), s, true);
		}
		return KeyCode.None;
	}

	public static BindSet ParseBinds(string action, string bindingString) {
		List<SingleBind> binds = new List<SingleBind>();

		foreach (string bind in bindingString.Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries)) {
			List<SingleInput> singleBind = new List<SingleInput>();
			foreach (string input in bind.Trim().Split(new char[]{'+'}, StringSplitOptions.RemoveEmptyEntries)) {
				if (action == "Double Bash" && input.Trim().ToLower() == "tap") {
					Randomizer.BashTap = true;
				}
				else {
					singleBind.Add(new SingleInput(input.Trim()));
				}
			}
			if (singleBind.Count > 0) {
				binds.Add(new SingleBind(singleBind));
			}
		}

		return new BindSet(binds);
	}

	public static void FixedUpdate() {
		foreach (BindSet bindSet in rebindMap.Values) {
			bindSet.FixedUpdate();
		}
	}

	public static Dictionary<string, Core.Input.InputButtonProcessor> CoreInputMap = new Dictionary<string, Core.Input.InputButtonProcessor> { 
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
	public static Dictionary<string, string> DefaultBinds = new Dictionary<string, string> { 
				{"Replay Message", "LeftAlt+T, RightAlt+T"},
				{"Warp", "LeftAlt+R, RightAlt+R"},
				{"Reload Seed", "LeftAlt+L, RightAlt+L"},
				{"Toggle Chaos", "LeftAlt+K, RightAlt+K"},
				{"Chaos Verbosity", "LeftAlt+V, RightAlt+V"},
				{"Force Chaos Effect","LeftAlt+F, RightAlt+F"},
				{"Show Progress", "LeftAlt+P, RightAlt+P"},
				{"Color Shift", "LeftAlt+C, RightAlt+C"},
				{"Double Bash", "Grenade"},
				{"Toggle Map Mode", "Grenade"},
				{"Grenade Jump", "Grenade+Jump"},
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

	public static BindSet ReplayMessage = new BindSet(new List<SingleBind>());
	public static BindSet ReturnToStart = new BindSet(new List<SingleBind>());
	public static BindSet ReloadSeed = new BindSet(new List<SingleBind>());
	public static BindSet ToggleChaos = new BindSet(new List<SingleBind>());
	public static BindSet ChaosVerbosity = new BindSet(new List<SingleBind>());
	public static BindSet ForceChaosEffect = new BindSet(new List<SingleBind>());
	public static BindSet ShowProgress = new BindSet(new List<SingleBind>());
	public static BindSet ColorShift = new BindSet(new List<SingleBind>());
	public static BindSet DoubleBash = new BindSet(new List<SingleBind>());
	public static BindSet FreeGrenadeJump = new BindSet(new List<SingleBind>());
	public static BindSet ToggleMapMode = new BindSet(new List<SingleBind>());
	public static BindSet ShowBonuses = new BindSet(new List<SingleBind>());
	public static BindSet BonusSwitch = new BindSet(new List<SingleBind>());
	public static BindSet BonusToggle = new BindSet(new List<SingleBind>());
	public static BindSet ResetGrenadeAim = new BindSet(new List<SingleBind>());
	public static BindSet SuppressAutofire = new BindSet(new List<SingleBind>());
	public static BindSet ListTrees = new BindSet(new List<SingleBind>());
	public static BindSet ListRelics = new BindSet(new List<SingleBind>());
	public static BindSet ListMapAltars = new BindSet(new List<SingleBind>());
	public static BindSet ListTeleporters = new BindSet(new List<SingleBind>());
	public static BindSet ShowStats = new BindSet(new List<SingleBind>());
	public static BindSet ShowKeysanityProgress = new BindSet(new List<SingleBind>());
	public static BindSet Bonus1 = new BindSet(new List<SingleBind>());
	public static BindSet Bonus2 = new BindSet(new List<SingleBind>());
	public static BindSet Bonus3 = new BindSet(new List<SingleBind>());
	public static BindSet Bonus4 = new BindSet(new List<SingleBind>());
	public static BindSet Bonus5 = new BindSet(new List<SingleBind>());
	public static BindSet Bonus6 = new BindSet(new List<SingleBind>());
	public static BindSet Bonus7 = new BindSet(new List<SingleBind>());
	public static BindSet Bonus8 = new BindSet(new List<SingleBind>());
	public static BindSet Bonus9 = new BindSet(new List<SingleBind>());

	private static Dictionary<string, BindSet> rebindMap = new Dictionary<string, BindSet> { 
		{"Replay Message", ReplayMessage}, 
		{"Warp", ReturnToStart}, 
		{"Reload Seed", ReloadSeed}, 
		{"Show Progress", ShowProgress}, 
		{"Color Shift", ColorShift}, 
		{"Double Bash", DoubleBash},
		{"Grenade Jump", FreeGrenadeJump}, 
		{"Toggle Map Mode", ToggleMapMode},
		{"Show Bonuses", ShowBonuses}, 
		{"Bonus Switch", BonusSwitch},
	 	{"Bonus Toggle", BonusToggle},
	 	{"Reset Grenade Aim", ResetGrenadeAim},
	 	{"Suppress Autofire", SuppressAutofire},
	 	{"List Trees", ListTrees},
	 	{"List Relics", ListRelics},
		{"List Map Altars", ListMapAltars},
		{"List Teleporters", ListTeleporters},
		{"Show Stats", ShowStats},
		{"Show Keysanity Progress", ShowKeysanityProgress},
		{"Bonus 1", Bonus1},
		{"Bonus 2", Bonus2},
		{"Bonus 3", Bonus3},
		{"Bonus 4", Bonus4},
		{"Bonus 5", Bonus5},
		{"Bonus 6", Bonus6},
		{"Bonus 7", Bonus7},
		{"Bonus 8", Bonus8},
		{"Bonus 9", Bonus9},
		{"Toggle Chaos", ToggleChaos},
		{"Chaos Verbosity", ChaosVerbosity},
		{"Force Chaos Effect", ForceChaosEffect}
	};

	private static Dictionary<string, string> coreInputNameRepl = new Dictionary<string, string> { 
		{"Grenade", "LightSpheres"},
		{"ChargeJump", "ChargeJumpCharge"}
	};

	public class SingleInput : Core.Input.InputButtonProcessor {
		public SingleInput(string input) {
			raw = input;
			if (input.StartsWith("_")) {
				this.Type = ActionType.ControllerButton;
				this.Button = (PlayerInputRebinding.ControllerButton)Enum.Parse(typeof(PlayerInputRebinding.ControllerButton), input.Substring(1), true);
			}
			else if (CoreInputMap.ContainsKey(input)) {
				this.Type = ActionType.CoreInput;
				this.CoreInput = CoreInputMap[input];
			}
			else {
				this.Type = ActionType.KeyCode;
				this.Key = StringToKeyBinding(input);
			}
		}

		public void FixedUpdate() {
			switch (this.Type) {
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

		public override string ToString() {
			switch (this.Type) {
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

		public string RawStr() {
			switch (this.Type) {
			case ActionType.CoreInput:
				return raw;
			case ActionType.ControllerButton:
				return $"_{this.Button}";
			case ActionType.KeyCode:
				return $"{this.Key}";
			default:
				return "";
			}			
		}


		public KeyCode Key;

		private string raw;

		public Core.Input.InputButtonProcessor CoreInput;

		public PlayerInputRebinding.ControllerButton Button;

		public ActionType Type;

		public enum ActionType {
			CoreInput,
			ControllerButton,
			KeyCode
		};
	}

	public class SingleBind : Core.Input.InputButtonProcessor {
		public SingleBind(List<SingleInput> inputs) {
			this.Inputs = inputs;
		}

		public void FixedUpdate() {
			bool pressed = true;

			foreach (SingleInput input in this.Inputs) {
				input.FixedUpdate();

				if (input.Released) {
					pressed = false;
				}
			}

			this.Update(pressed);
		}

		public override string ToString() => String.Join("+", this.Inputs.Select(input => input.ToString()).ToArray());
		public string RawStr() => String.Join("+", this.Inputs.Select(input => input.RawStr()).ToArray());
		

		public List<SingleInput> Inputs;
	}

	public class BindSet : Core.Input.InputButtonProcessor {
		public BindSet(List<SingleBind> binds) {
			this.deprecated_wasPressed = true;
			this.Binds = binds;
		}

		public override string ToString() => String.Join(", ", this.Binds.Select(binds => binds.RawStr()).ToArray());

		public string FirstBindName() {
			if (HasBind()) 
				return this.Binds[0].ToString();
			else
				return "<NO BIND>";
		}

		public bool HasBind() => this.Binds.Count > 0;

		public bool IsPressed() {
			foreach (SingleBind bind in this.Binds) {
				if (bind.Pressed) {
					if (this.deprecated_wasPressed) {
						return false;
					}
					this.deprecated_wasPressed = true;
					return true;
				}
			}
			this.deprecated_wasPressed = false;
			return false;
		}

		public void FixedUpdate() {
			bool pressed = false;

			foreach (SingleBind bind in this.Binds) {
				bind.FixedUpdate();

				if (bind.Pressed) {
					pressed = true;
				}
			}

			this.Update(pressed);
		}

		public List<SingleBind> Binds;

		public bool deprecated_wasPressed;
	}

}