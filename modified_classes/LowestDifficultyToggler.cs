using System;
using UnityEngine;

public class LowestDifficultyToggler : MonoBehaviour, IDebugMenuToggleable
{
	public string Name
	{
		get
		{
			return "Lowest Difficulty";
		}
	}

	public string HelpText
	{
		get
		{
			return "Toggle lowest difficulty";
		}
	}

	public string[] ToggleOptions
	{
		get
		{
			return new string[]
			{
				RandomizerText.DifficultyOverrides.Easy.NameOverride.ToString(),
				RandomizerText.DifficultyOverrides.Normal.NameOverride.ToString(),
				RandomizerText.DifficultyOverrides.Hard.NameOverride.ToString(),
				RandomizerText.DifficultyOverrides.OneLife.NameOverride.ToString()
			};
		}
	}

	public int CurrentToggleOptionId
	{
		get
		{
			return (int)DifficultyController.Instance.LowestDifficulty;
		}
		set
		{
			DifficultyController.Instance.LowestDifficulty = (DifficultyMode)((value % this.ToggleOptions.Length + this.ToggleOptions.Length) % this.ToggleOptions.Length);
		}
	}

	private int m_currentOption;
}
