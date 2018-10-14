using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core;
using UnityEngine;

// Token: 0x02000A02 RID: 2562
public static class RandomizerRebinding
{
	// Token: 0x060037B4 RID: 14260
	public static void WriteDefaultFile()
	{
		StreamWriter streamWriter = new StreamWriter("RandomizerRebinding.txt");
		streamWriter.WriteLine("Bind syntax: Key1+Key2, Key1+Key3+Key4, ... Syntax errors will load default binds.");
		streamWriter.WriteLine("Functions are unbound if there is no binding specified.");
		streamWriter.WriteLine("Supported binds are Unity KeyCodes (https://docs.unity3d.com/ScriptReference/KeyCode.html) and the following actions:");
		streamWriter.WriteLine("Jump, SpiritFlame, Bash, SoulFlame, ChargeJump, Glide, Dash, Grenade, Left, Right, Up, Down, LeftStick, RightStick, Start, Select");
		streamWriter.WriteLine("");
		streamWriter.WriteLine("Replay Message: LeftAlt+T, RightAlt+T");
		streamWriter.WriteLine("Return to Start: LeftAlt+R, RightAlt+R");
		streamWriter.WriteLine("Reload Seed: LeftAlt+L, RightAlt+L");
		streamWriter.WriteLine("Toggle Chaos: LeftAlt+K, RightAlt+K");
		streamWriter.WriteLine("Chaos Verbosity: LeftAlt+V, RightAlt+V");
		streamWriter.WriteLine("Force Chaos Effect: LeftAlt+F, RightAlt+F");
		streamWriter.WriteLine("Show Progress: LeftAlt+P, RightAlt+P");
		streamWriter.WriteLine("Color Shift: LeftAlt+C, RightAlt+C");
		streamWriter.WriteLine("Double Bash: Grenade");
		streamWriter.WriteLine("Bonus Switch: LeftAlt+Q, RightAlt+Q");
		streamWriter.WriteLine("Bonus Toggle: LeftAlt+Mouse1, RightAlt+Mouse1");
		streamWriter.WriteLine("Reset Grenade Aim: ");
		streamWriter.Flush();
		streamWriter.Close();
	}
	// Token: 0x060037B5 RID: 14261
	public static void ParseRebinding()
	{
	try 
	{
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

	DefaultBinds = new Dictionary<string, BindSet>(){
		{"Replay Message", RandomizerRebinding.ParseBinds("LeftAlt+T, RightAlt+T")},
		{"Return to Start", RandomizerRebinding.ParseBinds("LeftAlt+R, RightAlt+R")},
		{"Reload Seed", RandomizerRebinding.ParseBinds("LeftAlt+L, RightAlt+L")},
		{"Toggle Chaos", RandomizerRebinding.ParseBinds("LeftAlt+K, RightAlt+K")},
		{"Chaos Verbosity", RandomizerRebinding.ParseBinds("LeftAlt+V, RightAlt+V")},
		{"Force Chaos Effect", RandomizerRebinding.ParseBinds("LeftAlt+F, RightAlt+F")},
		{"Show Progress", RandomizerRebinding.ParseBinds("LeftAlt+P, RightAlt+P")},
		{"Color Shift", RandomizerRebinding.ParseBinds("LeftAlt+C, RightAlt+C")},
		{"Double Bash", RandomizerRebinding.ParseBinds("Grenade")},
		{"Bonus Switch", RandomizerRebinding.ParseBinds("LeftAlt+Q, RightAlt+Q")},
		{"Bonus Toggle", RandomizerRebinding.ParseBinds("LeftAlt+Mouse1, RightAlt+Mouse1")},
		{"Reset Grenade Aim", RandomizerRebinding.ParseBinds("")}
	};
		if (!File.Exists("RandomizerRebinding.txt"))
		{
			RandomizerRebinding.WriteDefaultFile();
		}
		string[] lines = File.ReadAllLines("RandomizerRebinding.txt");
		ArrayList unseenBinds = new ArrayList(RandomizerRebinding.DefaultBinds.Keys);
		Hashtable binds = new Hashtable();
		foreach(string line in lines){
			if(!line.Contains(":")) {
				continue;
			}
			{
				string[] parts = line.Split(':');
				string key = parts[0].Trim();
				string bind = parts[1].Trim();
				if(key == "Replay Message") {
					try {
						RandomizerRebinding.ReplayMessage = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Replay Message");
						RandomizerRebinding.ReplayMessage = DefaultBinds["Replay Message"];
					}
				} else if(key == "Return to Start") {
					try {
						RandomizerRebinding.ReturnToStart = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Return to Start");
						RandomizerRebinding.ReturnToStart = DefaultBinds["Return to Start"];
					}
				} else if(key == "Reload Seed") {
					try {
						RandomizerRebinding.ReloadSeed = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Reload Seed");
						RandomizerRebinding.ReloadSeed = DefaultBinds["Reload Seed"];
					}
				} else if(key == "Toggle Chaos") {
					try {
						RandomizerRebinding.ToggleChaos = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Toggle Chaos");
						RandomizerRebinding.ToggleChaos = DefaultBinds["Toggle Chaos"];
					}
				} else if(key == "Chaos Verbosity") {
					try {
						RandomizerRebinding.ChaosVerbosity = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Chaos Verbosity");
						RandomizerRebinding.ChaosVerbosity = DefaultBinds["Chaos Verbosity"];
					}
				} else if(key == "Force Chaos Effect") {
					try {
						RandomizerRebinding.ForceChaosEffect = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Force Chaos Effect");
						RandomizerRebinding.ForceChaosEffect = DefaultBinds["Force Chaos Effect"];
					}
				} else if(key == "Show Progress") {
					try {
						RandomizerRebinding.ShowProgress = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Show Progress");
						RandomizerRebinding.ShowProgress = DefaultBinds["Show Progress"];
					}
				} else if(key == "Color Shift") {
					try {
						RandomizerRebinding.ColorShift = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Color Shift");
						RandomizerRebinding.ColorShift = DefaultBinds["Color Shift"];
					}
				} else if(key == "Double Bash") {
					try {
						RandomizerRebinding.DoubleBash = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Double Bash");
						RandomizerRebinding.DoubleBash = DefaultBinds["Double Bash"];
					}
				} else if(key == "Bonus Switch") {
					try {
						RandomizerRebinding.BonusSwitch = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Bonus Switch");
						RandomizerRebinding.BonusSwitch = DefaultBinds["Bonus Switch"];
					}
				} else if(key == "Bonus Toggle") {
					try {
						RandomizerRebinding.BonusToggle = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Bonus Toggle");
						RandomizerRebinding.BonusToggle = DefaultBinds["Bonus Toggle"];
					}
				} else if(key == "Reset Grenade Aim") {
					try {
						RandomizerRebinding.ResetGrenadeAim = RandomizerRebinding.ParseBinds(bind);
					}
					catch(Exception) {
						Randomizer.showHint("Failed to load bind for Reset Grenade Aim");
						RandomizerRebinding.ResetGrenadeAim = DefaultBinds["Reset Grenade Aim"];
					}
				} else {
					continue;
				}
				// so now if we didn't return, we set the bind for that key one way or the other, so...
				unseenBinds.Remove(key);
			}
		}
		foreach(string key in unseenBinds) {
			Randomizer.showHint("Didn't find a bind for " + key + ", loading default");
				if(key == "Replay Message") {
					RandomizerRebinding.ReplayMessage = DefaultBinds[key];
				} else if(key == "Return to Start") {
					RandomizerRebinding.ReturnToStart = DefaultBinds[key];
				} else if(key == "Reload Seed") {
					RandomizerRebinding.ReloadSeed = DefaultBinds[key];
				} else if(key == "Toggle Chaos") {
					RandomizerRebinding.ToggleChaos = DefaultBinds[key];
				} else if(key == "Chaos Verbosity") {
					RandomizerRebinding.ChaosVerbosity = DefaultBinds[key];
				} else if(key == "Force Chaos Effect") {
					RandomizerRebinding.ForceChaosEffect = DefaultBinds[key];
				} else if(key == "Show Progress") {
					RandomizerRebinding.ShowProgress = DefaultBinds[key];
				} else if(key == "Color Shift") {
					RandomizerRebinding.ColorShift = DefaultBinds[key];
				} else if(key == "Double Bash") {
					RandomizerRebinding.DoubleBash = DefaultBinds[key];
				} else if(key == "Bonus Switch") {
					RandomizerRebinding.BonusSwitch = DefaultBinds[key];
				} else if(key == "Bonus Toggle") {
					RandomizerRebinding.BonusToggle = DefaultBinds[key];
				} else if(key == "Reset Grenade Aim") {
					RandomizerRebinding.ResetGrenadeAim = DefaultBinds[key];
				}  
		}
	}
	catch(Exception e) {
		Randomizer.showHint(e.Message);
	}

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
		ArrayList arrayList = new ArrayList();
		string[] array2 = array3;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array4 = array2[i].Split(new char[]
			{
				'+'
			});
			ArrayList arrayList2 = new ArrayList();
			foreach (string text in array4)
			{
				if (text.Trim().ToLower() == "tap")
				{
					Randomizer.BashTap = true;
				}
				else
				{
					arrayList2.Add(new RandomizerRebinding.Bind(text));
				}
			}
			if (arrayList2.Count > 0)
			{
				arrayList.Add(arrayList2);
			}
		}
		return new RandomizerRebinding.BindSet(arrayList);
	}

	public static Hashtable ActionMap;
	public static Dictionary<string, BindSet> DefaultBinds;



	// Token: 0x0400327B RID: 12923
	public static RandomizerRebinding.BindSet ReplayMessage;

	// Token: 0x0400327C RID: 12924
	public static RandomizerRebinding.BindSet ReturnToStart;

	// Token: 0x0400327D RID: 12925
	public static RandomizerRebinding.BindSet ReloadSeed;

	// Token: 0x0400327E RID: 12926
	public static RandomizerRebinding.BindSet ToggleChaos;

	// Token: 0x0400327F RID: 12927
	public static RandomizerRebinding.BindSet ChaosVerbosity;

	// Token: 0x04003280 RID: 12928
	public static RandomizerRebinding.BindSet ForceChaosEffect;

	// Token: 0x04003281 RID: 12929
	public static RandomizerRebinding.BindSet ShowProgress;

	// Token: 0x04003282 RID: 12930
	public static RandomizerRebinding.BindSet ColorShift;

	// Token: 0x04003283 RID: 12931
	public static RandomizerRebinding.BindSet DoubleBash;

	// Token: 0x04003284 RID: 12932
	public static RandomizerRebinding.BindSet BonusSwitch;

	// Token: 0x04003285 RID: 12933
	public static RandomizerRebinding.BindSet BonusToggle;

	// Token: 0x04003A1A RID: 14874
	public static RandomizerRebinding.BindSet ResetGrenadeAim;

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
		public BindSet(ArrayList binds)
		{
			this.binds = binds;
			this.wasPressed = true;
		}

		// Token: 0x060037BC RID: 14268
		public bool IsPressed()
		{
			foreach (object obj in this.binds)
			{
				ArrayList arrayList = (ArrayList)obj;
				bool flag = true;
				for (int i = 0; i < arrayList.Count; i++)
				{
					if (!((RandomizerRebinding.Bind)arrayList[i]).IsPressed())
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
		public ArrayList binds;

		// Token: 0x0400328A RID: 12938
		public bool wasPressed;
	}
}