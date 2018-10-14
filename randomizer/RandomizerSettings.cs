using System;
using System.IO;

// Token: 0x02000A10 RID: 2576
public static class RandomizerSettings
{
	// Token: 0x06003802 RID: 14338
	public static void WriteDefaultFile()
	{
		StreamWriter streamWriter = new StreamWriter("RandomizerSettings.txt");
		streamWriter.WriteLine("Controller Bash Deadzone: 0.5");
		streamWriter.WriteLine("Ability Menu Opacity: 0.5");
		streamWriter.WriteLine("Instant Grenade Aim: False");
		streamWriter.WriteLine("Grenade Aim Speed: 1.0");
		streamWriter.Flush();
		streamWriter.Close();
	}

	// Token: 0x06003803 RID: 14339
	public static void ParseSettings()
	{
		if (!File.Exists("RandomizerSettings.txt"))
		{
			RandomizerSettings.WriteDefaultFile();
		}
		try
		{
			string[] array = File.ReadAllLines("RandomizerSettings.txt");
			RandomizerSettings.BashDeadzone = float.Parse(array[0].Split(new char[]
			{
				':'
			})[1]);
			RandomizerSettings.AbilityMenuOpacity = float.Parse(array[1].Split(new char[]
			{
				':'
			})[1]);
			RandomizerSettings.FastGrenadeAim = (array[2].Split(new char[]
			{
				':'
			})[1].Trim() == "True");
			RandomizerSettings.GrenadeAimSpeed = float.Parse(array[3].Split(new char[]
			{
				':'
			})[1]);
			RandomizerSettings.BashDeadzone = Math.Max(0f, Math.Min(1f, RandomizerSettings.BashDeadzone));
			RandomizerSettings.AbilityMenuOpacity = Math.Max(0f, Math.Min(1f, RandomizerSettings.AbilityMenuOpacity));
		}
		catch (Exception)
		{
			RandomizerSettings.LoadDefaultSettings();
		}
	}

	// Token: 0x06003804 RID: 14340
	public static void LoadDefaultSettings()
	{
		RandomizerSettings.BashDeadzone = 0.5f;
		RandomizerSettings.AbilityMenuOpacity = 0.5f;
		RandomizerSettings.FastGrenadeAim = false;
		RandomizerSettings.GrenadeAimSpeed = 1f;
	}

	// Token: 0x040032C4 RID: 12996
	public static float BashDeadzone;

	// Token: 0x040032C5 RID: 12997
	public static float AbilityMenuOpacity;

	// Token: 0x040034A1 RID: 13473
	public static bool FastGrenadeAim;

	// Token: 0x04003A23 RID: 14883
	public static float GrenadeAimSpeed;
}
