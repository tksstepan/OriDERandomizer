using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

// Token: 0x02000A15 RID: 2581
public static class RandomizerSettings
{
	// Token: 0x0600381B RID: 14363 RVA: 0x000E7DB8 File Offset: 0x000E5FB8
	public static void WriteDefaultFile()
	{
		StreamWriter streamWriter = new StreamWriter("RandomizerSettings.txt");
		foreach(KeyValuePair<string, string> lineparts in DefaultSettings) {
			if(lineparts.Key == "Dev")
				continue;
			streamWriter.WriteLine(lineparts.Key + ": " + lineparts.Value);
		}
		streamWriter.Flush();
		streamWriter.Close();
	}
 
	// Token: 0x0600381C RID: 14364 RVA: 0x000E7E28 File Offset: 0x000E6028
	public static void ParseSettings()
	{
		DefaultSettings = new Dictionary<string, string>(){
			{"Controller Bash Deadzone", "0.5"},
			{"Ability Menu Opacity", "0.5"},
			{"Instant Grenade Aim", "False"},
			{"Grenade Aim Speed", "1.0"},
			{"Cold Color", "0, 255, 255, 255"},
			{"Hot Color", "255, 85, 0, 255"},
			{"Invert Swim", "False"},
			{"Dev", "False"}
		};
		if (!File.Exists("RandomizerSettings.txt"))
		{
			RandomizerSettings.WriteDefaultFile();
		}
		try
		{
			List<string> unseenSettings  = new List<string>(DefaultSettings.Keys);
			unseenSettings.Remove("Dev");
			List<string> writeList = new List<string>();
			string[] lines = File.ReadAllLines("RandomizerSettings.txt");
			// parse step 1: read settings from file
			foreach(string line in lines) {
				if(!line.Contains(":")) {
					continue;
				}
				string[] parts = line.Split(':');
				string setting = parts[0].Trim();
				if(!DefaultSettings.ContainsKey(setting)) {
					continue;
				}
				string value = parts[1].Trim();
				ParseSettingLine(setting, value);
				unseenSettings.Remove(setting);
			}
			foreach(string missing in unseenSettings) {
				ParseSettingLine(missing, DefaultSettings[missing]);
				writeList.Add(missing);
			}
			if(writeList.Count > 0) {
				Randomizer.printInfo("Default Settings written for these missing settings: " + String.Join(", ", writeList.ToArray()), 480);
				string writeText = "";
				foreach(string writeKey in writeList) {
					writeText += Environment.NewLine + writeKey+ ": " + DefaultSettings[writeKey];
				}
				File.AppendAllText("RandomizerSettings.txt", writeText);
			}
		}
		catch(Exception e) {
			Randomizer.LogError("Error parsing settings: " + e.Message);
		}
	}

	public static void ParseSettingLine(string setting, string value) {
		try {
			switch(setting) {
				case "Controller Bash Deadzone":
					RandomizerSettings.BashDeadzone = float.Parse(value);
					break;
				case "Ability Menu Opacity":
					RandomizerSettings.AbilityMenuOpacity = float.Parse(value);
					break;
				case "Instant Grenade Aim":
					RandomizerSettings.FastGrenadeAim = (value.Trim().ToLower() == "true");
					break;
				case "Grenade Aim Speed":
					RandomizerSettings.GrenadeAimSpeed = float.Parse(value);
					break;
				case "Cold Color":
				RandomizerSettings.ColdColor = RandomizerSettings.ParseColor(value);
					break;
				case "Hot Color":
				RandomizerSettings.HotColor = RandomizerSettings.ParseColor(value);
					break;
				case "Invert Swim":
					RandomizerSettings.InvertSwim = (value.Trim().ToLower() == "true");
					break;
				case "Dev":
					RandomizerSettings.Dev = (value.Trim().ToLower() == "true");
					break;
			}
		} catch(Exception) {
			ParseSettingLine(setting, DefaultSettings[setting]);
			Randomizer.printInfo("@"+setting+ ": failed to parse value '" + value + "'. Using default value: '"+DefaultSettings[setting]+"'@", 240);
		}
	}

	public static bool IsSwimBoosting()
	{
		if(RandomizerSettings.InvertSwim)
			return !Core.Input.Jump.IsPressed;
		else
			return Core.Input.Jump.IsPressed;
	}

	public static bool SwimBoostPressed()
	{
		if(RandomizerSettings.InvertSwim)
			return Core.Input.Jump.OnReleased;
		else
			return Core.Input.Jump.OnPressed;
	}

	// Token: 0x0600381D RID: 14365 RVA: 0x000E7FCC File Offset: 0x000E61CC
	public static void LoadDefaultSettings()
	{
		RandomizerSettings.BashDeadzone = 0.5f;
		RandomizerSettings.AbilityMenuOpacity = 0.5f;
		RandomizerSettings.FastGrenadeAim = false;
		RandomizerSettings.GrenadeAimSpeed = 1f;
		RandomizerSettings.ColdColor = new Color(0f, 0.5f, 0.5f, 0.5f);
		RandomizerSettings.HotColor = new Color(0.5f, 0.1666f, 0f, 0.5f);
	}

	// Token: 0x0600381E RID: 14366 RVA: 0x000E803C File Offset: 0x000E623C
	private static Color ParseColor(string input)
	{
		string[] array = input.Split(new char[]
		{
			','
		});
		return new Color(float.Parse(array[0]) / 511f, float.Parse(array[1]) / 511f, float.Parse(array[2]) / 511f, float.Parse(array[3]) / 511f);
	}

	// Token: 0x040032EB RID: 13035
	public static float BashDeadzone;

	// Token: 0x040032EC RID: 13036
	public static float AbilityMenuOpacity;

	// Token: 0x040032ED RID: 13037
	public static bool FastGrenadeAim;

	// Token: 0x040032EE RID: 13038
	public static float GrenadeAimSpeed;

	// Token: 0x040032EF RID: 13039
	public static Color ColdColor;
	public static Color HotColor;
	public static bool InvertSwim;
	public static bool Dev;

	public static Dictionary<string, string> DefaultSettings;
}
