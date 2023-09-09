using System;
using System.Collections.Generic;
using System.IO;
using Game;
using UnityEngine;

public static class RandomizerColorManager
{
	public static void Initialize()
	{
		HotColdTarget = new Vector3(0f, 0f);
		bool found = false;
		if (File.Exists("Color.txt"))
		{
			string text = File.ReadAllText("Color.txt").ToLower();
			string[] lines = text.Split(new char[]
			{
				'\n'
			});
			if (lines != null && lines.Length >= 1 && lines[0].Trim().Equals("customrotation"))
			{
				colors.Clear();
				float red = 0f;
				float green = 0f;
				float blue = 0f;
				float alpha = 0f;
				int i = 1;
				while (i < lines.Length - 1 && !string.IsNullOrEmpty(lines[i]) && lines[i].Length >= 6)
				{
					string[] components = lines[i].Split(new char[]
					{
						','
					});
					if (components != null && components.Length >= 4)
					{
						float.TryParse(components[0], out red);
						float.TryParse(components[1], out green);
						float.TryParse(components[2], out blue);
						float.TryParse(components[3], out alpha);
						red /= 511f;
						green /= 511f;
						blue /= 511f;
						alpha /= 511f;
						colors.Add(new Color(red, green, blue, alpha));
					}
					components = lines[i + 1].Split(new char[]
					{
						','
					});
					if (components != null && components.Length >= 5)
					{
						float red2;
						float.TryParse(components[0], out red2);
						float green2;
						float.TryParse(components[1], out green2);
						float blue2;
						float.TryParse(components[2], out blue2);
						float alpha2;
						float.TryParse(components[3], out alpha2);
						int frames;
						int.TryParse(components[4], out frames);
						frames = Math.Min(frames, 36000);
						red2 /= 511f;
						green2 /= 511f;
						blue2 /= 511f;
						alpha2 /= 511f;
						for (int j = 1; j <= (int)frames; j++)
						{
							colors.Add(new Color(red + (red2 - red) * (float)j / frames, green + (green2 - green) * (float)j / frames, blue + (blue2 - blue) * (float)j / frames, alpha + (alpha2 - alpha) * (float)j / frames));
						}
					}
					i++;
				}
				customColor = false;
				customRotation = true;
				return;
			}
			colors.Clear();
			customRotation = false;
			string[] components2 = text.Split(new char[]
			{
				','
			});
			if (components2 != null && (components2.Length == 3 || components2.Length == 4))
			{
				float red3 = 0f;
				float green3 = 0f;
				float blue3 = 0f;
				float alpha3 = 0f;
				float.TryParse(components2[0], out red3);
				float.TryParse(components2[1], out green3);
				float.TryParse(components2[2], out blue3);
				if (components2.Length == 4)
				{
					float.TryParse(components2[3], out alpha3);
				}
				else
				{
					alpha3 = 255f;
				}
				colors.Add(new Color(red3 / 511f, green3 / 511f, blue3 / 511f, alpha3 / 511f));
				found = true;
				customColor = true;
			}
		}
		if (!found && (customColor || customRotation))
		{
			customColor = false;
			customRotation = false;
		}
	}

	public static void UpdateColors()
	{
		try {
			if (Randomizer.HotCold || Characters.Sein.PlayerAbilities.Sense.HasAbility)
			{
				float scale = 64f;
				float distance = 100f;
				if (Characters.Ori.InsideMapstone)
				{
					int currentMap = 20 + RandomizerBonus.MapStoneProgression() * 4;
					using (List<int>.Enumerator enumerator = (RandomizerBonus.SenseFragsActive ? Randomizer.HotColdMapsWithFrags : Randomizer.HotColdMaps).GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							int map = enumerator.Current;
							if (map > currentMap)
							{
								distance = (float)(map - currentMap - 4) * 2f;
								break;
							}
						}
					}
					if(distance < scale && RandomizerBonus.SenseFragsEnabled && !RandomizerBonus.SenseFragsActive) {
						RandomizerBonus.SenseFragsActive = true;
					}
				}
				else
				{
					distance = Vector3.Distance(HotColdTarget, Characters.Sein.Position);
				}
				if (distance < scale)
				{
					if(colorBeforeSense.Count == 0) {
						colorBeforeSense.Add(Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color);
					}
					if (!(customRotation && RandomizerSettings.Customization.DiscoSense))
					{
						Color hotColor = RandomizerSettings.Customization.HotColor;
						Color coldColor = RandomizerSettings.Customization.ColdColor;
						float scaleFactor = distance / scale;
						Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = new Color(Mathf.Lerp(hotColor.r, coldColor.r, scaleFactor), Mathf.Lerp(hotColor.g, coldColor.g, scaleFactor), Mathf.Lerp(hotColor.b, coldColor.b, scaleFactor), Mathf.Lerp(hotColor.a, coldColor.a, scaleFactor));
					}
					else {
						colorIndex += (int)(20f * (1f - distance / scale));
						Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = colors[colorIndex];
					}
				return;
				}
				if(!(customRotation || customColor) && colorBeforeSense.Count > 0) {
					Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = colorBeforeSense[0];
					colorBeforeSense.Clear();
				}
			}
			if (customRotation)
			{
				colorIndex = (colorIndex + 1) % colors.Count;
				Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = colors[colorIndex];
				return;
			}
			if (customColor)
			{
				Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = colors[0];
			}
		}
		catch(Exception e) {
			Randomizer.LogError("ColorTick: " + colorIndex.ToString() + " out of " + colors.Count.ToString() + ": " + e.Message);
		}

	}

	public static void UpdateHotColdTarget()
	{
		float minimum = float.MaxValue;
		foreach (RandomizerHotColdItem target in Randomizer.HotColdItems.Values)
		{
			if (Characters.Sein.Inventory.GetRandomizerItem(target.Id) == 0)
			{
				float distance = Vector3.Distance(target.Position, Characters.Sein.Position);
				if (distance < minimum)
				{
					minimum = distance;
					HotColdTarget = target.Position;
				}
			}
		}
		if(RandomizerBonus.SenseFragsActive)
		{
			foreach (RandomizerHotColdItem target in Randomizer.HotColdFrags.Values) {
				if (Characters.Sein.Inventory.GetRandomizerItem(target.Id) == 0)
				{
					float distance = Vector3.Distance(target.Position, Characters.Sein.Position);
					if (distance < minimum)
					{
						minimum = distance;
						HotColdTarget = target.Position;
					}
				}
			}
		}
	}

	private static bool customColor = false;

	private static bool customRotation = false;

	private static List<Color> colors = new List<Color>();

	private static List<Color> colorBeforeSense = new List<Color>(); // this is an Optional actually

	private static int colorIndex = 0;

	private static Vector3 HotColdTarget;
}
