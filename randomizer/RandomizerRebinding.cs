using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core;
using UnityEngine;

// Token: 0x02000A02 RID: 2562
public static class RandomizerRebinding {
	// Token: 0x060037B4 RID: 14260
	public static void WriteDefaultFile() {
		StreamWriter streamWriter = new StreamWriter("RandomizerRebinding.txt");
		streamWriter.WriteLine("Bind syntax: Key1+Key2, Key1+Key3+Key4, ... Syntax errors will load default binds.");
		streamWriter.WriteLine("Functions are unbound if there is no binding specified.");
		streamWriter.WriteLine("Supported binds are Unity KeyCodes (https://docs.unity3d.com/ScriptReference/KeyCode.html) and the following actions:");
		streamWriter.WriteLine("Jump, SpiritFlame, Bash, SoulFlame, ChargeJump, Glide, Dash, Grenade, Left, Right, Up, Down, LeftStick, RightStick, Start, Select");
		streamWriter.WriteLine("");
		foreach(KeyValuePair<string, string> lineparts in DefaultBinds) {
			streamWriter.WriteLine(lineparts.Key + ": " + lineparts.Value);
		}
		streamWriter.Flush();
		streamWriter.Close();
	}
	// Token: 0x060037B5 RID: 14261
	public static void ParseRebinding() {
		try {
			ActionMap = new Hashtable(){
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
			DefaultBinds = new Dictionary<string, string>(){
				{"Replay Message", "LeftAlt+T, RightAlt+T"},
				{"Return to Start","LeftAlt+R, RightAlt+R"},
				{"Reload Seed", "LeftAlt+L, RightAlt+L"},
				{"Toggle Chaos", "LeftAlt+K, RightAlt+K"},
				{"Chaos Verbosity", "LeftAlt+V, RightAlt+V"},
				{"Force Chaos Effect","LeftAlt+F, RightAlt+F"},
				{"Show Progress", "LeftAlt+P, RightAlt+P"},
				{"Color Shift", "LeftAlt+C, RightAlt+C"},
				{"Double Bash", "Grenade"},
				{"Bonus Switch", "LeftAlt+Q, RightAlt+Q"},
				{"Bonus Toggle", "LeftAlt+Mouse1, RightAlt+Mouse1"},
				{"Reset Grenade Aim",""},
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
			foreach(string line in lines) {
				if(!line.Contains(":")) {
					continue;
				}
				string[] parts = line.Split(':');
				string key = parts[0].Trim();
				if(!DefaultBinds.ContainsKey(key)) {
					continue;
				}
				string bind = parts[1].Trim();
				AssignBind(key, bind, writeList);
				unseenBinds.Remove(key);
			}
			// parse step 2: load defaults for missing binds
			foreach(string missingKey in unseenBinds) {
				AssignBind(missingKey, null, writeList);
			}
			if(writeList.Count > 0) {
				List<string> warnList = new List<string>();
				foreach(string writeKey in writeList)
				{
					if(DefaultBinds[writeKey] != "")
						warnList.Add(writeKey);
				}
				if(warnList.Count > 0)
					Randomizer.printInfo("Default Binds written for these missing binds: " + String.Join(", ", warnList.ToArray()) + ".", 480);
				string writeText = "";
				foreach(string writeKey in writeList) {
					writeText += Environment.NewLine + writeKey+ ": " + DefaultBinds[writeKey];
				}
				File.AppendAllText("RandomizerRebinding.txt", writeText);
			}
		}
		catch(Exception e) {
			Randomizer.LogError("Error parsing bindings: " + e.Message);
		}
	}
	public static void AssignBind(string key, string bind, List<string> writeList) {
		if(key == "Replay Message") {
				RandomizerRebinding.ReplayMessage = ParseOrDefault(bind, key, writeList);
		} else if(key == "Return to Start") {
				RandomizerRebinding.ReturnToStart = ParseOrDefault(bind, key, writeList);
		} else if(key == "Reload Seed") {
				RandomizerRebinding.ReloadSeed = ParseOrDefault(bind, key, writeList);
		} else if(key == "Toggle Chaos") {
				RandomizerRebinding.ToggleChaos = ParseOrDefault(bind, key, writeList);
		} else if(key == "Chaos Verbosity") {
				RandomizerRebinding.ChaosVerbosity = ParseOrDefault(bind, key, writeList);
		} else if(key == "Force Chaos Effect") {
				RandomizerRebinding.ForceChaosEffect = ParseOrDefault(bind, key, writeList);
		} else if(key == "Show Progress") {
				RandomizerRebinding.ShowProgress = ParseOrDefault(bind, key, writeList);
		} else if(key == "Color Shift") {
				RandomizerRebinding.ColorShift = ParseOrDefault(bind, key, writeList);
		} else if(key == "Double Bash") {
				RandomizerRebinding.DoubleBash = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus Switch") {
				RandomizerRebinding.BonusSwitch = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus Toggle") {
				RandomizerRebinding.BonusToggle = ParseOrDefault(bind, key, writeList);
		} else if(key == "Reset Grenade Aim") {
				RandomizerRebinding.ResetGrenadeAim = ParseOrDefault(bind, key, writeList);
		} else if(key == "List Trees") {
			RandomizerRebinding.ListTrees = ParseOrDefault(bind, key, writeList);
		} else if(key == "List Relics") {
			RandomizerRebinding.ListRelics = ParseOrDefault(bind, key, writeList);
		} else if(key == "List Map Altars") {
			RandomizerRebinding.ListMapAltars = ParseOrDefault(bind, key, writeList);
		} else if(key == "List Teleporters") {
			RandomizerRebinding.ListTeleporters = ParseOrDefault(bind, key, writeList);
		} else if(key == "Show Stats") {
			RandomizerRebinding.ShowStats = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus 1") {
			RandomizerRebinding.Bonus1 = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus 2") {
			RandomizerRebinding.Bonus2 = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus 3") {
			RandomizerRebinding.Bonus3 = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus 4") {
			RandomizerRebinding.Bonus4 = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus 5") {
			RandomizerRebinding.Bonus5 = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus 6") {
			RandomizerRebinding.Bonus6 = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus 7") {
			RandomizerRebinding.Bonus7 = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus 8") {
			RandomizerRebinding.Bonus8 = ParseOrDefault(bind, key, writeList);
		} else if(key == "Bonus 9") {
			RandomizerRebinding.Bonus9 = ParseOrDefault(bind, key, writeList);
		}
	}

	public static BindSet ParseOrDefault(string bind, string key, List<string> writeList) {
		string defaultBind = DefaultBinds[key];
		if(bind == null) {
			bind = defaultBind;
			writeList.Add(key);
		}
		try {
			return ParseBinds(bind);
		}
		catch (Exception)
		{
			Randomizer.printInfo("@"+key+ ": failed to parse '" + bind + "'. Using default value: '"+defaultBind+"'@", 240);
			bind = defaultBind;
		}
		return ParseBinds(bind);
	}

	// Token: 0x060037B6 RID: 14262
	public static KeyCode StringToKeyBinding(string s)
	{
		if (s != "")
		{
			return (KeyCode)((int)Enum.Parse(typeof(KeyCode), s));
		}
		return KeyCode.None;
	}

	// Token: 0x060037B8 RID: 14264
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

	public static RandomizerRebinding.BindSet ReplayMessage;
	public static RandomizerRebinding.BindSet ReturnToStart;
	public static RandomizerRebinding.BindSet ReloadSeed;
	public static RandomizerRebinding.BindSet ToggleChaos;
	public static RandomizerRebinding.BindSet ChaosVerbosity;
	public static RandomizerRebinding.BindSet ForceChaosEffect;
	public static RandomizerRebinding.BindSet ShowProgress;
	public static RandomizerRebinding.BindSet ColorShift;
	public static RandomizerRebinding.BindSet DoubleBash;
	public static RandomizerRebinding.BindSet BonusSwitch;
	public static RandomizerRebinding.BindSet BonusToggle;
	public static RandomizerRebinding.BindSet ResetGrenadeAim;
	public static RandomizerRebinding.BindSet ListTrees;
	public static RandomizerRebinding.BindSet ListRelics;
	public static RandomizerRebinding.BindSet ListMapAltars;
	public static RandomizerRebinding.BindSet ListTeleporters;
	public static RandomizerRebinding.BindSet ShowStats;
	public static RandomizerRebinding.BindSet Bonus1;
	public static RandomizerRebinding.BindSet Bonus2;
	public static RandomizerRebinding.BindSet Bonus3;
	public static RandomizerRebinding.BindSet Bonus4;
	public static RandomizerRebinding.BindSet Bonus5;
	public static RandomizerRebinding.BindSet Bonus6;
	public static RandomizerRebinding.BindSet Bonus7;
	public static RandomizerRebinding.BindSet Bonus8;
	public static RandomizerRebinding.BindSet Bonus9;

	// Token: 0x02000A03 RID: 2563
	public class Bind
	{
		// Token: 0x060037B9 RID: 14265
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

		public override string ToString() {
			if(this.ActionBind)
				return this.Action.ToString();
			else
				return this.Key.ToString();
		}

		// Token: 0x060037BA RID: 14266
		public bool IsPressed()
		{
			if (this.ActionBind)
			{
				return this.Action.Pressed;
			}
			return MoonInput.GetKey(this.Key);
		}

		// Token: 0x04003286 RID: 12934
		public KeyCode Key;

		// Token: 0x04003287 RID: 12935
		public Core.Input.InputButtonProcessor Action;

		// Token: 0x04003288 RID: 12936
		public bool ActionBind;
	}

	// Token: 0x02000A04 RID: 2564
	public class BindSet
	{
		// Token: 0x060037BB RID: 14267
		public BindSet(List<List<Bind>> binds)
		{
			this.binds = binds;
			this.wasPressed = true;
		}

		public string FirstBindName() {
			if(this.binds.Count > 0)
			{
				string output = "";
				foreach(Bind bind in this.binds[0]){
					output += bind.ToString() + "+";
				}
				return output.Substring(0, output.Length-1);
			} else {
				return "<NO BIND>";
			}
		}

		// Token: 0x060037BC RID: 14268
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

		// Token: 0x04003289 RID: 12937
		public List<List<Bind>> binds;

		// Token: 0x0400328A RID: 12938
		public bool wasPressed;
	}
}