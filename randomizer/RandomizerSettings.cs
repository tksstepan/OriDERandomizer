using System;
using System.Collections.Generic;
using System.IO;
using Core;
using UnityEngine;

public static class RandomizerSettings
{
	public static void WriteDefaultFile()
	{
		StreamWriter streamWriter = new StreamWriter("RandomizerSettings.txt");
		foreach (SettingBase setting in RandomizerSettings.All.Values)
		{
			if (setting.Name != "Dev")
			{
				streamWriter.WriteLine(setting.Name + ": " + setting.ToString());
			}
		}
		streamWriter.Flush();
		streamWriter.Close();
	}
 
	public static void ParseSettings()
	{
		if (!File.Exists("RandomizerSettings.txt"))
		{
			RandomizerSettings.WriteDefaultFile();
		}

		try
		{
			List<string> unseenSettings  = new List<string>(RandomizerSettings.All.Keys);
			unseenSettings.Remove("Dev");
			List<string> writeList = new List<string>();
			string[] lines = File.ReadAllLines("RandomizerSettings.txt");

			// parse step 1: read settings from file
			foreach (string line in lines)
			{
				if (!line.Contains(":"))
				{
					continue;
				}
				string[] parts = line.Split(new char[]{':'}, 2);
				string setting = parts[0].Trim();
				if (!RandomizerSettings.All.ContainsKey(setting))
				{
					continue;
				}
				string value = parts[1].Trim();
				ParseSettingLine(setting, value);
				unseenSettings.Remove(setting);
			}

			foreach (string missing in unseenSettings)
			{
				RandomizerSettings.All[missing].Reset();
				writeList.Add(missing);
			}

			if (writeList.Count > 0)
			{
				string writeText = "";
				var nagList = new List<string>();
				foreach (string writeKey in writeList)
				{
					SettingBase setting = RandomizerSettings.All[writeKey];
					writeText += Environment.NewLine + writeKey + ": " + setting.ToString();
					if (setting.Nag)
					{
						nagList.Add(writeKey);
					}
				}

				if (nagList.Count > 0)
				{
					Randomizer.printInfo("Default settings written for these missing settings: " + String.Join(", ", nagList.ToArray()), 480);
				}

				File.AppendAllText("RandomizerSettings.txt", writeText);
			}
		}
		catch (Exception e)
		{
			Randomizer.LogError("Error parsing settings: " + e.Message);
		}
	}

	public static void ParseSettingLine(string setting, string value)
	{
		try
		{
			if (RandomizerSettings.All.ContainsKey(setting))
			{
				RandomizerSettings.All[setting].Parse(value);
				return;
			}
		}
		catch (Exception)
		{
			RandomizerSettings.All[setting].Reset();
			Randomizer.printInfo("@" + setting + ": failed to parse value '" + value + "'. Using default value: '" + RandomizerSettings.All[setting].ToString() + "'@", 240);
		}
	}

	public static bool IsSwimBoosting()
	{
		if (RandomizerSettings.Controls.InvertSwim)
			return !Core.Input.Jump.IsPressed;
		else
			return Core.Input.Jump.IsPressed;
	}

	public static bool SwimBoostPressed()
	{
		if (RandomizerSettings.Controls.InvertSwim)
			return Core.Input.Jump.OnReleased;
		else
			return Core.Input.Jump.OnPressed;
	}

	static RandomizerSettings()
	{
		Controls.BashDeadzone = new FloatSetting("Controller Bash Deadzone", 0.5f);
		Controls.FastGrenadeAim = new BoolSetting("Instant Grenade Aim", false);
		Controls.GrenadeAimSpeed = new FloatSetting("Grenade Aim Speed", 1.0f);
		Controls.InvertSwim = new BoolSetting("Invert Swim", false);
		Controls.InvertClimb = new BoolSetting("Invert Climb", false);
		Controls.GrenadeJump = new EnumSetting<GrenadeJumpMode>("Grenade Jump Mode", GrenadeJumpMode.Free);
		Controls.WallChargeMouseAim = new BoolSetting("Wall Charge Mouse Aim", true);
		Controls.SwimmingMouseAim = new BoolSetting("Swimming Mouse Aim", false);
		Controls.SlowClimbVault = new BoolSetting("Slow Climb Vault", true);
		Controls.Autofire = new EnumSetting<AutofireMode>("Autofire", AutofireMode.Toggle);

		Customization.ColdColor = new ColorSetting("Cold Color", new Color(0f, 0.5f, 0.5f, 0.5f));
		Customization.HotColor = new ColorSetting("Hot Color", new Color(0.5f, 0.1666667f, 0f, 0.5f));
		Customization.DiscoSense = new BoolSetting("Disco Sense", false, false);

		QOL.AbilityMenuOpacity = new FloatSetting("Ability Menu Opacity", 0.5f);
		QOL.CursorLock = new BoolSetting("Cursor Lock", false, false);

		Game.ImprovedSpiritFlame = new BoolSetting("Improved Spirit Flame", true, false);
		Game.BlackrootOrbRoomClimbAssist = new BoolSetting("Blackroot Orb Room Climb Assist", true, false);
		Game.FixGrottoBridgeDrop = new BoolSetting("Fix Grotto Bridge Drop", true, false);
		Game.UseTeleportAnywhere = new BoolSetting("Use Teleport Anywhere for Alt+R", false, false);
	}

	public static Dictionary<string, SettingBase> All = new Dictionary<string, SettingBase>();

	public static BoolSetting Dev = new BoolSetting("Dev", false);

	public enum AutofireMode
	{
		Off,
		Hold,
		Toggle
	}

	public enum GrenadeJumpMode
	{
		Manual,
		Free
	}

	public static class Controls
	{
		public static FloatSetting BashDeadzone;

		public static BoolSetting FastGrenadeAim;

		public static FloatSetting GrenadeAimSpeed;

		public static BoolSetting InvertSwim;

		public static BoolSetting InvertClimb;

		public static EnumSetting<GrenadeJumpMode> GrenadeJump;

		public static BoolSetting WallChargeMouseAim;

		public static BoolSetting SwimmingMouseAim;

		public static BoolSetting SlowClimbVault;

		public static EnumSetting<AutofireMode> Autofire;
	}

	public static class Customization
	{
		public static ColorSetting ColdColor;

		public static ColorSetting HotColor;

		public static BoolSetting DiscoSense;
	}

	public static class QOL
	{
		public static FloatSetting AbilityMenuOpacity;

		public static BoolSetting CursorLock;
	}

	public static class Game
	{
		public static BoolSetting ImprovedSpiritFlame;

		public static BoolSetting BlackrootOrbRoomClimbAssist;

		public static BoolSetting FixGrottoBridgeDrop;

		public static BoolSetting UseTeleportAnywhere;
	}

	public abstract class SettingBase
	{
		public SettingBase(string name, bool nag = true)
		{
			this.Name = name;
			RandomizerSettings.All[name] = this;

			this.Nag = nag;
		}

		public abstract void Parse(string value);

		public abstract new string ToString();

		public abstract void Reset();

		public string Name;

		public bool Nag;
	}

	public abstract class Setting<T> : SettingBase
	{
		public Setting(string name, T defaultValue, bool nag = true) : base(name, nag)
		{
			this.Default = defaultValue;
			this.Value = this.Default;
		}

		public override string ToString()
		{
			return this.Value.ToString();
		}

		public override void Reset()
		{
			this.Value = this.Default;
		}

		public static implicit operator T(RandomizerSettings.Setting<T> setting) => setting.Value;

		public T Default;

		public T Value;
	}

	public class BoolSetting : Setting<bool>
	{
		public BoolSetting(string name, bool defaultValue, bool nag = true) : base(name, defaultValue, nag) {}

		public override void Parse(string value)
		{
			this.Value = bool.Parse(value);
		}
	}

	public class FloatSetting : Setting<float>
	{
		public FloatSetting(string name, float defaultValue, bool nag = true) : base(name, defaultValue, nag) {}

		public override void Parse(string value)
		{
			this.Value = float.Parse(value);
		}
	}

	public class ColorSetting : Setting<Color>
	{
		public ColorSetting(string name, Color defaultValue, bool nag = true) : base(name, defaultValue, nag) {}

		public override void Parse(string value)
		{
			string[] parts = value.Split(new char[]{','});
			this.Value = new UnityEngine.Color(float.Parse(parts[0]) / 511f, float.Parse(parts[1]) / 511f, float.Parse(parts[2]) / 511f, float.Parse(parts[3]) / 511f);
		}

		public override string ToString()
		{
			return String.Format("{0:F0}, {1:F0}, {2:F0}, {3:F0}", this.Value.r * 511f, this.Value.g * 511f, this.Value.b * 511f, this.Value.a * 511f);
		}
	}

	public class EnumSetting<T> : Setting<T> where T : System.Enum
	{
		public EnumSetting(string name, T defaultValue, bool nag = true) : base(name, defaultValue, nag) {}

		public override void Parse(string value)
		{
			this.Value = (T)Enum.Parse(typeof(T), value, true);
		}
	}
}
