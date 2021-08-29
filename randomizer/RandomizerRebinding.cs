using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
			ActionMap = new Hashtable()
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

			DefaultBinds = new Dictionary<string, string>()
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
				{"Show Bonuses", "LeftAlt+B, RightAlt+B"},
				{"Bonus Switch", "LeftAlt+Q, RightAlt+Q"},
				{"Bonus Toggle", "LeftAlt+Mouse1, RightAlt+Mouse1"},
				{"Reset Grenade Aim",""},
				{"Suppress Autofire","Grenade"},
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
			ArrayList unseenBinds = new ArrayList(RandomizerRebinding.DefaultBinds.Keys);
			List<string> writeList = new List<string>();
			Hashtable binds = new Hashtable();

			// parse step 1: read binds from file
			foreach (string line in lines)
			{
				if (!line.Contains(":"))
				{
					continue;
				}
				string[] parts = line.Split(':');
				string key = parts[0].Trim();
				if(!DefaultBinds.ContainsKey(key))
				{
					continue;
				}
				string bind = parts[1].Trim();
				AssignBind(key, bind, writeList);
				unseenBinds.Remove(key);
			}

			// parse step 2: load defaults for missing binds
			foreach (string missingKey in unseenBinds)
			{
				AssignBind(missingKey, null, writeList);
			}

			if (writeList.Count > 0)
			{
				List<string> warnList = new List<string>();

				foreach (string writeKey in writeList)
				{
					if (DefaultBinds[writeKey] != "")
					{
						warnList.Add(writeKey);
					}
				}

				if (warnList.Count > 0)
				{
					Randomizer.printInfo("Default Binds written for these missing binds: " + String.Join(", ", warnList.ToArray()) + ".", 480);
				}

				string writeText = "";
				foreach (string writeKey in writeList)
				{
					writeText += Environment.NewLine + writeKey+ ": " + DefaultBinds[writeKey];
				}

				File.AppendAllText("RandomizerRebinding.txt", writeText);
			}
		}
		catch (Exception e)
		{
			Randomizer.LogError("Error parsing bindings: " + e.Message);
		}
	}

	public static void AssignBind(string key, string bind, List<string> writeList)
	{
		if (!rebindMap.ContainsKey(key))
		{
			return;
		}

		rebindMap[key].binds = ParseOrDefault(bind, key, writeList).binds;
	}

	public static BindSet ParseOrDefault(string bind, string key, List<string> writeList)
	{
		string defaultBind = DefaultBinds[key];
		if(bind == null)
		{
			bind = defaultBind;
			writeList.Add(key);
		}

		try
		{
			return ParseBinds(bind);
		}
		catch (Exception)
		{
			Randomizer.printInfo("@" + key + ": failed to parse '" + bind + "'. Using default value: '" + defaultBind + "'@", 240);
			bind = defaultBind;
		}

		return ParseBinds(bind);
	}

	public static KeyCode StringToKeyBinding(string s)
	{
		if (s != "")
		{
			return (KeyCode)((int)Enum.Parse(typeof(KeyCode), s));
		}
		return KeyCode.None;
	}

	public static RandomizerRebinding.BindSet ParseBinds(string binds)
	{
		string[] array3 = binds.Split(new char[]
		{
			','
		});
		List<List<Bind>> bindSets = new List<List<Bind>>();
		string[] array2 = array3;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array4 = array2[i].Split(new char[]
			{
				'+'
			});
			List<Bind> bindSet = new List<Bind>();
			foreach (string text in array4)
			{
				if (text.Trim().ToLower() == "tap")
				{
					Randomizer.BashTap = true;
				}
				else
				{
					bindSet.Add(new RandomizerRebinding.Bind(text));
				}
			}
			if (bindSet.Count > 0)
			{
				bindSets.Add(bindSet);
			}
		}
		return new RandomizerRebinding.BindSet(bindSets);
	}

	public static Hashtable ActionMap;
	public static Dictionary<string, string> DefaultBinds;

	public static RandomizerRebinding.BindSet ReplayMessage = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ReturnToStart = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ReloadSeed = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ToggleChaos = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ChaosVerbosity = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ForceChaosEffect = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ShowProgress = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ColorShift = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet DoubleBash = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ShowBonuses = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet BonusSwitch = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet BonusToggle = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ResetGrenadeAim = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet SuppressAutofire = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ListTrees = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ListRelics = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ListMapAltars = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ListTeleporters = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet ShowStats = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet Bonus1 = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet Bonus2 = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet Bonus3 = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet Bonus4 = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet Bonus5 = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet Bonus6 = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet Bonus7 = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet Bonus8 = new RandomizerRebinding.BindSet(new List<List<Bind>>());
	public static RandomizerRebinding.BindSet Bonus9 = new RandomizerRebinding.BindSet(new List<List<Bind>>());

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

	public class Bind
	{
		public Bind(string input)
		{
			input = input.Trim();
			if (RandomizerRebinding.ActionMap.ContainsKey(input))
			{
				this.Action = (Core.Input.InputButtonProcessor)RandomizerRebinding.ActionMap[input];
				this.ActionBind = true;
				return;
			}
			this.ActionBind = false;
			this.Key = RandomizerRebinding.StringToKeyBinding(input);
		}

		public override string ToString()
		{
			if (this.ActionBind)
			{
				return this.Action.ToString();
			}
			else
			{
				return this.Key.ToString();
			}
		}

		public bool IsPressed()
		{
			if (this.ActionBind)
			{
				return this.Action.Pressed;
			}
			return MoonInput.GetKey(this.Key);
		}

		public bool OnPressed
		{
			get
			{
				if (this.ActionBind)
				{
					return this.Action.OnPressed;
				}
				return MoonInput.GetKeyDown(this.Key);
			}
		}

		public bool Pressed
		{
			get
			{
				if (this.ActionBind)
				{
					return this.Action.Pressed;
				}
				return MoonInput.GetKey(this.Key);
			}
		}

		public bool OnReleased
		{
			get
			{
				if (this.ActionBind)
				{
					return this.Action.OnReleased;
				}
				return MoonInput.GetKeyUp(this.Key);
			}
		}

		public bool Released
		{
			get
			{
				if (this.ActionBind)
				{
					return this.Action.Released;
				}
				return !MoonInput.GetKey(this.Key);
			}
		}

		public KeyCode Key;

		public Core.Input.InputButtonProcessor Action;

		public bool ActionBind;
	}

	public class BindSet
	{
		public BindSet(List<List<Bind>> binds)
		{
			this.binds = binds;
			this.wasPressed = true;
		}

		public string FirstBindName()
		{
			if (this.binds.Count > 0)
			{
				string output = "";
				foreach (Bind bind in this.binds[0])
				{
					output += bind.ToString() + "+";
				}
				return output.Substring(0, output.Length-1);
			}
			else
			{
				return "<NO BIND>";
			}
		}

		public bool IsPressed()
		{
			foreach (List<Bind> bindGroup in this.binds)
			{
				bool flag = true;
				foreach(Bind bind in bindGroup)
				{
					if (!bind.IsPressed())
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					if (this.wasPressed)
					{
						return false;
					}
					this.wasPressed = true;
					return true;
				}
			}
			this.wasPressed = false;
			return false;
		}

		public bool OnPressedGroup(List<Bind> bindGroup)
		{
			bool onPressed = false;
			foreach (Bind bind in bindGroup)
			{
				if (!bind.Pressed)
				{
					return false;
				}
				if (bind.OnPressed)
				{
					onPressed = true;
				}
			}
			return onPressed;
		}

		public bool PressedGroup(List<Bind> bindGroup)
		{
			foreach (Bind bind in bindGroup)
			{
				if (!bind.Pressed)
				{
					return false;
				}
			}
			return true;
		}

		public bool OnReleasedGroup(List<Bind> bindGroup)
		{
			bool onReleased = false;
			foreach (Bind bind in bindGroup)
			{
				if (!bind.Released)
				{
					return false;
				}
				if (bind.OnReleased)
				{
					onReleased = true;
				}
			}
			return onReleased;
		}

		public bool ReleasedGroup(List<Bind> bindGroup)
		{
			foreach (Bind bind in bindGroup)
			{
				if (!bind.Released)
				{
					return false;
				}
			}
			return true;
		}

		public bool OnPressed
		{
			get
			{
				bool onPressed = false;
				foreach (List<Bind> bindGroup in this.binds)
				{
					if (OnPressedGroup(bindGroup))
					{
						onPressed = true;
					}
					else if (PressedGroup(bindGroup) || OnReleasedGroup(bindGroup))
					{
						return false;
					}
				}
				return onPressed;
			}
		}

		public bool Pressed
		{
			get
			{
				foreach (List<Bind> bindGroup in this.binds)
				{
					if (PressedGroup(bindGroup))
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool OnReleased
		{
			get
			{
				bool onReleased = false;
				foreach (List<Bind> bindGroup in this.binds)
				{
					if (OnReleasedGroup(bindGroup))
					{
						onReleased = true;
					}
					else if (ReleasedGroup(bindGroup) || OnPressedGroup(bindGroup))
					{
						return false;
					}
				}
				return onReleased;
			}
		}

		public bool Released
		{
			get
			{
				foreach (List<Bind> bindGroup in this.binds)
				{
					if (!ReleasedGroup(bindGroup))
					{
						return false;
					}
				}
				return true;
			}
		}

		public List<List<Bind>> binds;

		public bool wasPressed;
	}
}