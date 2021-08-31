using System;
using System.Collections.Generic;
using Sein.World;

// Token: 0x02000A05 RID: 2565
public static class RandomizerClues
{
	// Token: 0x060037BC RID: 14268 RVA: 0x0002BD5B File Offset: 0x00029F5B
	public static void initialize()
	{
		RandomizerClues.RevealOrder = new int[3];
		RandomizerClues.Clues = new List<string>();
	}

	// Token: 0x060037BD RID: 14269 RVA: 0x0002BD72 File Offset: 0x00029F72
	public static void AddClue(string clue, int order)
	{
		RandomizerClues.Clues.Add(clue);
		RandomizerClues.RevealOrder[order] = RandomizerClues.Clues.Count;
	}

	// Token: 0x060037BE RID: 14270 RVA: 0x000E472C File Offset: 0x000E292C
	public static string GetClues()
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		string[] array = new string[]
		{
			"????",
			"????",
			"????"
		};
		if (Keys.GinsoTree)
		{
			array[0] = RandomizerClues.Clues[RandomizerClues.RevealOrder[0] - 1];
			text = "*";
		}
		if (Keys.ForlornRuins)
		{
			array[1] = RandomizerClues.Clues[RandomizerClues.RevealOrder[1] - 1];
			text2 = "#";
		}
		if (Keys.MountHoru)
		{
			array[2] = RandomizerClues.Clues[RandomizerClues.RevealOrder[2] - 1];
			text3 = "@";
		}
		for (int i = 0; i < 3; i++)
		{
			if (RandomizerBonus.SkillTreeProgression() >= RandomizerClues.RevealOrder[i] * 3)
			{
				array[i] = RandomizerClues.Clues[RandomizerClues.RevealOrder[i] - 1];
			}
		}
		return string.Concat(new string[]
		{
			text,
			"WV: ",
			array[0],
			text,
			"  ",
			text2,
			"GS: ",
			array[1],
			text2,
			"  ",
			text3,
			"SS: ",
			array[2],
			text3
		});
	}
	public static void FinishClues()
	{
		for (int i = 0; i < 3; i++)
		{
			if (RandomizerClues.RevealOrder[i] == 0)
			{
				RandomizerClues.Clues.Add("Unknown");
				RandomizerClues.RevealOrder[i] = RandomizerClues.Clues.Count;
			}
		}
	}

	// Token: 0x04003289 RID: 12937
	public static int[] RevealOrder;

	// Token: 0x0400328A RID: 12938
	public static List<string> Clues;
}
