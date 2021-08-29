using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static class RandomizerSettings
{
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
			{"Disco Sense", "False"},
			{"Cursor Lock", "False"},
			{"Free Grenade Jump", "True"},
			{"Wall Charge Mouse Aim", "True"},
			{"Swimming Mouse Aim", "True"},
			{"Slow Climb Vault", "True"},
			{"Autofire", "Hold"},
			{"Improved Spirit Flame", "True"},
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
				string writeText = "";
				var nagList = new List<string>();
				foreach(string writeKey in writeList) {
					writeText += Environment.NewLine + writeKey+ ": " + DefaultSettings[writeKey];
					if(writeKey != "Disco Sense" && writeKey != "Cursor Lock") // added these in 3.4, don't nag
						nagList.Add(writeKey);
				}
				if(nagList.Count > 0) {
					Randomizer.printInfo("Default Settings written for these missing settings: " + String.Join(", ", nagList.ToArray()), 480);
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
				case "Disco Sense":
					RandomizerSettings.DiscoSense = (value.Trim().ToLower() == "true");
					break;
				case "Cursor Lock":
					RandomizerSettings.CursorLock = (value.Trim().ToLower() == "true");
					break;
				case "Free Grenade Jump":
					RandomizerSettings.FreeGrenadeJump = (value.Trim().ToLower() == "true");
					break;
				case "Wall Charge Mouse Aim":
					RandomizerSettings.WallChargeMouseAim = (value.Trim().ToLower() == "true");
					break;
				case "Swimming Mouse Aim":
					RandomizerSettings.SwimmingMouseAim = (value.Trim().ToLower() == "true");
					break;
				case "Slow Climb Vault":
					RandomizerSettings.SlowClimbVault = (value.Trim().ToLower() == "true");
					break;
				case "Autofire":
					RandomizerSettings.Autofire = (RandomizerSettings.AutofireMode)Enum.Parse(typeof(RandomizerSettings.AutofireMode), value.Trim(), true);
					break;
				case "Improved Spirit Flame":
					RandomizerSettings.ImprovedSpiritFlame = (value.Trim().ToLower() == "true");
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

	private static Color ParseColor(string input)
	{
		string[] array = input.Split(new char[]
		{
			','
		});
		return new Color(float.Parse(array[0]) / 511f, float.Parse(array[1]) / 511f, float.Parse(array[2]) / 511f, float.Parse(array[3]) / 511f);
	}

	public static float BashDeadzone;

	public static float AbilityMenuOpacity;

	public static bool FastGrenadeAim;

	public static float GrenadeAimSpeed;

	public static Color ColdColor;
	public static Color HotColor;
	public static bool InvertSwim;
	public static bool Dev;
	public static bool DiscoSense;
	public static bool CursorLock;

	public static bool FreeGrenadeJump;
	public static bool WallChargeMouseAim;
	public static bool SwimmingMouseAim;
	public static bool SlowClimbVault;
	public static RandomizerSettings.AutofireMode Autofire;
	public static bool ImprovedSpiritFlame;

	public static Dictionary<string, string> DefaultSettings;

	public enum AutofireMode
	{
		Off,
		Hold,
		Toggle
	}
}
