using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public static class RandomizerText
{
	public static RandomizerMessageProvider GetAbilityName(AbilityType ability)
	{
		if (!RandomizerText.m_abilityOverrides.ContainsKey(ability))
		{
			return null;
		}

		AbilityTextOverrides overrides = RandomizerText.m_abilityOverrides[ability];
		return overrides.m_nameOverride;
	}

	public static RandomizerMessageProvider GetAbilityDescription(AbilityType ability)
	{
		if (!RandomizerText.m_abilityOverrides.ContainsKey(ability))
		{
			return null;
		}

		AbilityTextOverrides overrides = RandomizerText.m_abilityOverrides[ability];
		return overrides.m_descriptionOverride;
	}

	private static Dictionary<AbilityType, AbilityTextOverrides> m_abilityOverrides = new Dictionary<AbilityType, AbilityTextOverrides>
	{
		{
			AbilityType.ChargeFlameEfficiency,
			new AbilityTextOverrides(null, "Allows Charge Flame to be performed without spending Energy")
		},
		{
			AbilityType.ChargeDash,
			new AbilityTextOverrides(null, "Allows Ori to Charge Dash ([ChargeDashCharge]) to attack enemies or break blue plants and walls")
		},
		{
			AbilityType.MapMarkers,
			new AbilityTextOverrides(null, "Displays all pickups on the Map")
		},
		{
			AbilityType.HealthEfficiency,
			new AbilityTextOverrides("Health Efficiency", "Health pickups will restore twice as much Health")
		},
		{
			AbilityType.AbilityMarkers,
			new AbilityTextOverrides("Spirit Efficiency", "Increases all sources of Spirit Light by 50%")
		},
		{
			AbilityType.SoulEfficiency,
			new AbilityTextOverrides("Spirit Potency", "Increases all sources of Spirit Light by an additional 50%")
		},
		{
			AbilityType.HealthMarkers,
			new AbilityTextOverrides("Health Recovery", "Ori's Health will gradually refill (2 per minute)")
		},
		{
			AbilityType.EnergyMarkers,
			new AbilityTextOverrides("Energy Recovery", "Ori's Energy will gradually refill (2 per minute)")
		},
		{
			AbilityType.Sense,
			new AbilityTextOverrides("Sense Items", "Causes Ori to change color when approaching important items")
		}
	};

	private class AbilityTextOverrides
	{
		public AbilityTextOverrides(string name, string description)
		{
			if (name != null)
			{
				m_nameOverride = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
				m_nameOverride.SetMessage(name);
			}
			if (description != null)
			{
				m_descriptionOverride = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
				m_descriptionOverride.SetMessage(description);
			}
		}

		public RandomizerMessageProvider m_nameOverride;
		public RandomizerMessageProvider m_descriptionOverride;
	}
}