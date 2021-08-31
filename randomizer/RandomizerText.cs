using System;
using System.Collections.Generic;
using UnityEngine;

public static class RandomizerText
{
	public static RandomizerMessageProvider GetAbilityName(AbilityType ability)
	{
		if (!RandomizerText.m_abilityOverrides.ContainsKey(ability))
		{
			return null;
		}
		return RandomizerText.m_abilityOverrides[ability].m_nameOverride;
	}

	public static RandomizerMessageProvider GetAbilityDescription(AbilityType ability)
	{
		if (!RandomizerText.m_abilityOverrides.ContainsKey(ability))
		{
			return null;
		}
		return RandomizerText.m_abilityOverrides[ability].m_descriptionOverride;
	}

	public static string GetObjectiveText()
	{
		if (Randomizer.canFinalEscape(false))
		{
			return "Find and Restore #Mount Horu#!";
		}
		if (Randomizer.ForceTrees)
		{
			return "Visit all 10 #Skill Trees#";
		}
		if (Randomizer.WorldTour)
		{
			return string.Format("Find all {0} #Relics# hidden throughout Nibel", Randomizer.RelicCount);
		}
		if (Randomizer.ForceMaps)
		{
			return "Restore all 9 #Mapstones#";
		}
		if (Randomizer.fragsEnabled)
		{
			return string.Format("Find all {0} #Warmth Fragments# hidden throughout Nibel", Randomizer.fragKeyFinish);
		}
		return "Continue to search for #Skills# and #Resources#";
	}

	private static Dictionary<AbilityType, RandomizerText.AbilityTextOverrides> m_abilityOverrides = new Dictionary<AbilityType, RandomizerText.AbilityTextOverrides>
	{
		{
			AbilityType.ChargeFlameEfficiency,
			new RandomizerText.AbilityTextOverrides(null, "Allows Charge Flame to be performed without spending Energy")
		},
		{
			AbilityType.ChargeDash,
			new RandomizerText.AbilityTextOverrides(null, "Allows Ori to Charge Dash ([ChargeJumpCharge]) to attack enemies or break blue plants and walls")
		},
		{
			AbilityType.MapMarkers,
			new RandomizerText.AbilityTextOverrides(null, "Displays all pickups on the Map")
		},
		{
			AbilityType.HealthEfficiency,
			new RandomizerText.AbilityTextOverrides("Health Efficiency", "Health pickups will restore twice as much Health")
		},
		{
			AbilityType.AbilityMarkers,
			new RandomizerText.AbilityTextOverrides("Spirit Efficiency", "Increases all sources of Spirit Light by 50%")
		},
		{
			AbilityType.SoulEfficiency,
			new RandomizerText.AbilityTextOverrides("Spirit Potency", "Increases all sources of Spirit Light by an additional 50%")
		},
		{
			AbilityType.HealthMarkers,
			new RandomizerText.AbilityTextOverrides("Health Recovery", "Ori's Health will gradually refill (2 per minute)")
		},
		{
			AbilityType.EnergyMarkers,
			new RandomizerText.AbilityTextOverrides("Energy Recovery", "Ori's Energy will gradually refill (2 per minute)")
		},
		{
			AbilityType.Sense,
			new RandomizerText.AbilityTextOverrides("Sense Items", "Causes Ori to change color when approaching important items")
		}
	};

	public static string CostsAbilityPoint = "Costs [Amount] Ability Point ([Total] Total)";

	public static string CostsAbilityPoints = "Costs [Amount] Ability Points ([Total] Total)";

	private class AbilityTextOverrides
	{
		public AbilityTextOverrides(string name, string description)
		{
			if (name != null)
			{
				this.m_nameOverride = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
				this.m_nameOverride.SetMessage(name);
			}
			if (description != null)
			{
				this.m_descriptionOverride = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
				this.m_descriptionOverride.SetMessage(description);
			}
		}

		public RandomizerMessageProvider m_nameOverride;

		public RandomizerMessageProvider m_descriptionOverride;
	}
}
