using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using Core;
using UnityEngine;

public static class RandomizerSettings {
	public static void WriteDefaultFile() {
		dirty = true;
		WriteSettings();
	}
 
	public static void ParseSettings() {
		if (!File.Exists("RandomizerSettings.txt")) {
			WriteDefaultFile();
		}

		try {
			List<string> unseenSettings  = new List<string>(All.Keys);
			unseenSettings.Remove("Dev");
			List<string> writeList = new List<string>();
			string[] lines = File.ReadAllLines("RandomizerSettings.txt");

			// parse step 1: read settings from file
			foreach (string rawLine in lines) {
				var line = rawLine;

				if(line.Contains("//"))
					line = line.Split(new string[]{"//"}, StringSplitOptions.None)[0].Trim();

				if (!line.Contains(":"))
					continue;				

				string[] parts = line.Split(new char[]{':'}, 2);
				string setting = parts[0].Trim();
				if (!All.ContainsKey(setting)) {
					continue;
				}
				string value = parts[1].Trim();
				if(setting == "Grenade Jump Mode" && value.ToLower() == "free") {
					dirty = true;
					value = "Auto";
				}
				if(setting == "Hints" && value.ToLower() == "skilled") {
					dirty = true;
					value = "Experienced";
				}
				if(setting == "Default Map Filter" && value.ToLower() == "all") {
					dirty = true;
					value = "Uncollected";
				}

				ParseSettingLine(setting, value);
				unseenSettings.Remove(setting);
			}

			foreach (string missing in unseenSettings) {
				All[missing].Reset();
				writeList.Add(missing);
			}

			if (writeList.Count > 0 && !dirty) {
				string writeText = "";
				var nagList = new List<string>();
				foreach (string writeKey in writeList) {
					SettingBase setting = All[writeKey];
					writeText += Environment.NewLine + writeKey + ": " + setting.ToString();
					if (setting.Nag) {
						nagList.Add(writeKey);
					}
				}

				if (nagList.Count > 0) {
					Randomizer.printInfo("Default settings written for these missing settings: " + String.Join(", ", nagList.ToArray()), 480);
				}

				File.AppendAllText("RandomizerSettings.txt", writeText);
			}
			CurrentFilter = Customization.DefaultMapFilter.Value;
			if(dirty)
				WriteSettings();
		}
		catch (Exception e) {
			Randomizer.LogError("Error parsing settings: " + e.Message);
		}
	}

	public static void ParseSettingLine(string setting, string value) {
		try {
			if (All.ContainsKey(setting)) {
				All[setting].Parse(value);
				return;
			}
		}
		catch (Exception) {
			All[setting].Reset();
			Randomizer.printInfo("@" + setting + ": failed to parse value '" + value + "'. Using default value: '" + All[setting].ToString() + "'@", 240);
		}
	}

	public static void WriteSettings() {
		if (!dirty)
			return;

		using (var writer = new StreamWriter("RandomizerSettings.txt", false)) {
				writer.WriteLine("// This file contains a variety of randomizer-specific settings");
			foreach (var setting in All) {
				if (setting.Value.Hidden && !Dev.Value && setting.Value.IsDefault())
					continue;
				if(setting.Value.Comment != "")
					writer.WriteLine($"// {setting.Value.Comment.Replace("\n", "\n// ")}");
				writer.Write(setting.Key);
				writer.Write(": ");
				writer.WriteLine($"{setting.Value.ToString()}\n");
			}
		}

		dirty = false;
	}

	public static bool IsSwimBoosting() {
		if (Controls.InvertSwim)
			return !Core.Input.Jump.IsPressed;
		else
			return Core.Input.Jump.IsPressed;
	}

	public static bool SwimBoostPressed() {
		if (Controls.InvertSwim)
			return Core.Input.Jump.OnReleased;
		else
			return Core.Input.Jump.OnPressed;
	}

	public static void SetDirty() {
		dirty = true;
	}

	static RandomizerSettings() {
		Controls.BashDeadzone = new FloatSetting("Controller Bash Deadzone", 0.5f, "(0.0-1.0,  Default=0.5): Size of the controller stick deadzone when aiming Bash.");
		Controls.FastGrenadeAim = new BoolSetting("Instant Grenade Aim", false, "True: When aiming Grenade on a controller, throw the grenade in the direction the stick is aimed.\nFalse (Default): Vanilla behavior (move the stick to move the target location).");
		Controls.GrenadeAimSpeed = new FloatSetting("Grenade Aim Speed", 1.0f, "(Default 1.0 - higher numbers are faster): The speed at which controller/wsad inputs move the Grenade target.");
		Controls.InvertSwim = new BoolSetting("Invert Swim", false, "True: Ori swims fast by default, and slows down while pressing [Jump].\nFalse (default): Vanilla behavior (hold [Jump] to swim faster.");
		Controls.InvertClimb = new BoolSetting("Invert Climb", false, "True: Ori Climbs on walls by default, and lets when holding [Climb]\nFalse (default): Vanilla behavior (hold [Climb] to Climb.");
		Controls.GrenadeJump = new EnumSetting<GrenadeJumpMode>("Grenade Jump Mode", GrenadeJumpMode.Auto, "Auto (default): Grenade Jump by pressing [[Grenade Jump]] (Default [Grenade]+[Jump]).\nManual: Vanilla behavior (Grenade Jump by using Grenade, then Jump 1 frame later).");
		Controls.WallChargeMouseAim = new BoolSetting("Wall Charge Mouse Aim", true, "True (default): On Keyboard+Mouse, allows aiming Wall Charge Jumps with the mouse.\nFalse: Vanilla behavior.");
		Controls.SwimmingMouseAim = new BoolSetting("Swimming Mouse Aim", false, "True: On Keyboard+Mouse, Ori will swim towards the mouse cursor.\nFalse (default): Vanilla behavior.");
		Controls.SlowClimbVault = new BoolSetting("Slow Climb Vault", true, "True (default): slightly slows Climb vaults, making it easier to land on small vertical platforms with Climb.\nFalse: Vanilla behavior.");
		Controls.Autofire = new EnumSetting<AutofireMode>("Autofire", AutofireMode.Off, "Hold: When [SpiritFlame] is held, autofire - (Charge Flame by holding [[Suppress Autofire]] and [SpiritFlame]).\nToggle: Press [SpiritFlame] to start autofiring. Press it again to stop. (Charge Flame as normal).\nOff: Vanilla behavior (no autofire).");
		Controls.LongerBashAimTime = new BoolSetting("Longer Bash Aim Time", false, "True: Allows holding [Bash] for about 3x as long, giving you more time to aim.\nFalse (default): Vanilla behavior (about 1.3 seconds of Bash aiming time).");

		Customization.ColdColor = new ColorSetting("Cold Color", new Color(0f, 0.5f, 0.5f, 0.5f), 511f, "Red, Blue, Green, Transparency (0-255 for each): The color Ori turns when Sensing an item at max range.");
		Customization.HotColor = new ColorSetting("Hot Color", new Color(0.5f, 0.1666667f, 0f, 0.5f), 511f, "Red, Blue, Green, Transparency (0-255 for each): The color Ori turns when Sensing an item at range 0.");
		Customization.DiscoSense = new BoolSetting("Disco Sense", false, "True: Ignore sense colors, and instead speed up the color.txt rotation when sense is triggering.\nFalse (default): colors.txt rotation is overwritten by Sense colors.",false);
		Customization.MultiplePickupMessages = new BoolSetting("Display Multiple Pickup Messages", false, "True: Allows up to 5 pickup messages to be visible at once, on left side of the screen. Hold [[Replay Message]] to show more.\nFalse (default): New pickup messages are queued behind old ones, and display at the top center of the screen.", false);
		Customization.AlwaysShowLastFivePickups = new BoolSetting("Always Show Last Five Pickup Messages", false, "True: Always show the last 5 pickups gained.\nFalse (default): Only show pickups when found or on pressing [[Replay Message]].", false);
		Customization.WarpTeleporterColor = new ColorSetting("Warp Teleporter Color", new Color(202f/255f, 57f/255f, 243f/255f, 1f), 255f, "Red, Blue, Green, Transparency (0-255 for each): The color that Warp-created Teleporters are on the map.");
		Customization.DefaultMapFilter = new EnumSetting<MapFilterMode>("Default Map Filter", MapFilterMode.InLogic, "InLogic (default): Select the In Logic map filter when first opening the map.\nUncollected: Select the Uncollected map filter when first opening the map.", false);
		Customization.HintLevel = new EnumSetting<HintLevels>("Hints", HintLevels.NewPlayer, "NewPlayer (default): Show loading tips intended for new rando players.\nExperienced: Show loading tips intended for more experienced rando players.\nDisabled: do not show loading screen tips.",  false);
		Customization.RandomizedExpNames = new BoolSetting("Randomized Experience Names", false, "True: Replace the word \"Experience\" with a random currency name whenever you gain experience from a pickup.\nFalse (default): Do not do this.", false);

		QOL.AbilityMenuOpacity = new FloatSetting("Ability Menu Opacity", 0.5f, "(0.0-1.0) The opacity of the ability menu when performing a Save Anywhere.", false);
		QOL.CursorLock = new BoolSetting("Cursor Lock", false, "True: Locks the mouse cursor inside the window\nFalse (default): Do not do this.", false);

		Game.DefaultDifficulty = new EnumSetting<Difficulty>("Default Difficulty", Difficulty.Relaxing, "(Relaxing (default), Challenging, Punishing, OneLife): The default difficulty on new file selection.", false);

		Accessibility.ApplySoundCompression = new BoolSetting("Apply Sound Compression", false, "True: Caps sound from getting too loud (relevant when e.g. charge jumping very echo-y areas, like Spirit Caverns).\nFalse (default): Vanilla behavior.", false);
		Accessibility.SoundCompressionFactor = new FloatSetting("Sound Compression Factor", 0.6f, "(0.0-1.0) Higher values mean more sound compression (fewer sounds louder than the rest of them).", false);
		Accessibility.CameraShakeFactor = new FloatSetting("Camera Shake Factor", 1f, "(0.0-1.0) Reduce the intensity of camera shake effects in the game. Set to 0 to disable camera shake entirely.", false);

		Dev = new BoolSetting("Dev", false, "", false, true);
		DevSettings.AreasOri = new BoolSetting("Keep Areas.Ori Updated", true, "", false, true);
		DevSettings.ImprovedSpiritFlame = new BoolSetting("Improved Spirit Flame", true, "", false, true);
		DevSettings.BlackrootOrbRoomClimbAssist = new BoolSetting("Blackroot Orb Room Climb Assist", true, "", false, true);
	}

	public static Dictionary<string, SettingBase> All = new Dictionary<string, SettingBase>();

	public static BoolSetting Dev = new BoolSetting("Dev", false, "", false, true);

	public static MapFilterMode CurrentFilter = MapFilterMode.InLogic;

	private static bool dirty = false;

	public enum AutofireMode {
		Off,
		Hold,
		Toggle
	}

	public enum Difficulty {
		Relaxing,
		Challenging,
		Punishing,
		OneLife
	}

	public enum HintLevels {
		NewPlayer,
		Experienced,
		Disabled,
	}

	public enum GrenadeJumpMode {
		Manual,
		Auto
	}
	public enum MapFilterMode {
		[Description("In Logic")]
		InLogic,
		[Description("Uncollected")]
		Uncollected
	}

	public static class Controls {
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

		public static BoolSetting LongerBashAimTime;
	}

	public static class Customization {
		public static ColorSetting ColdColor;

		public static ColorSetting HotColor;

		public static BoolSetting DiscoSense;

		public static BoolSetting MultiplePickupMessages;

		public static BoolSetting AlwaysShowLastFivePickups;

		public static ColorSetting WarpTeleporterColor;

		public static EnumSetting<MapFilterMode> DefaultMapFilter;

		public static EnumSetting<HintLevels> HintLevel;

		public static BoolSetting RandomizedExpNames;
	}

	public static class QOL {
		public static FloatSetting AbilityMenuOpacity;

		public static BoolSetting CursorLock;
	}

	public static class Game {
		public static EnumSetting<Difficulty> DefaultDifficulty;
	}

	public static class Accessibility {
		public static BoolSetting ApplySoundCompression;

		public static FloatSetting SoundCompressionFactor;

		public static FloatSetting CameraShakeFactor;
	}


	public static class DevSettings {
		public static BoolSetting AreasOri;
		public static BoolSetting ImprovedSpiritFlame;
		public static BoolSetting BlackrootOrbRoomClimbAssist;
	}

	public abstract class SettingBase {
		public SettingBase(string name, string comment = "", bool nag = true, bool hidden = false) {
			this.Name = name;
			All[name] = this;
			this.Nag = nag;
			this.Hidden = hidden;
			this.Comment = comment;
		}
		public abstract bool IsDefault();

		public abstract void Parse(string value);

		public abstract new string ToString();

		public abstract void Reset();

		public virtual string ValidValues() => ""; 

		public string Name;

		public bool Nag;

		public string Comment;

		public bool Hidden;

	}

	public abstract class Setting<T> : SettingBase {
		public Setting(string name, T defaultValue, string comment = "", bool nag = true, bool hidden = false) : base(name, comment, nag, hidden) {
			this.Default = defaultValue;
			this.Value = this.Default;
		}

		public override bool IsDefault() => this.Value.Equals(this.Default);

		public override string ToString() {
			return this.Value.ToString();
		}

		public override void Reset() {
			this.Value = this.Default;
		}

		public static implicit operator T(Setting<T> setting) => setting.Value;

		public T Default;

		public T Value;
	}

	public class BoolSetting : Setting<bool> {
		public BoolSetting(string name, bool defaultValue, string comment = "", bool nag = true, bool hidden = false) : base(name, defaultValue, comment, nag, hidden) {}

		public override void Parse(string value) {
			this.Value = bool.Parse(value);
		}

		public override string ValidValues() => "[True|False]";
	}

	public class FloatSetting : Setting<float> {
		public FloatSetting(string name, float defaultValue, string comment = "", bool nag = true, bool hidden = false) : base(name, defaultValue, comment, nag, hidden) {}

		public override void Parse(string value) {
			this.Value = float.Parse(value);
		}
		public override string ValidValues() => "A decimal number";

	}

	public class ColorSetting : Setting<Color> {
        public ColorSetting(string name, Color defaultValue, float divisor, string comment = "", bool nag = true, bool hidden = false) : base(name, defaultValue, comment, nag, hidden) {	
			this.divisor = divisor;
		}
		public override string ValidValues() => "R,G,B,A (more details at top of file)";

        public override void Parse(string value) {
			string[] parts = value.Split(new char[]{','});
			this.Value = new UnityEngine.Color(float.Parse(parts[0]) / divisor, float.Parse(parts[1]) / divisor, float.Parse(parts[2]) / divisor, float.Parse(parts[3]) / divisor);
		}

		public override string ToString() {
			return String.Format("{0:F0}, {1:F0}, {2:F0}, {3:F0}", this.Value.r * divisor, this.Value.g * divisor, this.Value.b * divisor, this.Value.a * divisor);
		}

		public float divisor;
	}

	public class EnumSetting<T> : Setting<T> where T : System.Enum {
		public EnumSetting(string name, T defaultValue, string comment ="", bool nag = true, bool hidden = false) : base(name, defaultValue, comment, nag, hidden) {}

		public override void Parse(string value) {
			this.Value = (T)Enum.Parse(typeof(T), value, true);
		}

		public override string ValidValues()  => $"{String.Join("|",Enum.GetNames(typeof(T)))}";
	}
}
