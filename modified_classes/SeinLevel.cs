using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class SeinLevel : SaveSerialize, ISeinReceiver
{
	public int TotalExperience
	{
		get
		{
			return this.Experience + this.ConsumedExperience;
		}
	}

	public int TotalExperienceForNextLevel
	{
		get
		{
			return this.ExperienceForNextLevel + this.ConsumedExperience;
		}
	}

	public int ExperienceNeedForNextLevel
	{
		get
		{
			return this.ExperienceForNextLevel - this.Experience;
		}
	}

	public float ExperienceVisualMinNormalized
	{
		get
		{
			return this.ExperienceVisualMin / (float)this.ExperienceForNextLevel;
		}
	}

	public float ExperienceVisualMaxNormalized
	{
		get
		{
			return this.ExperienceVisualMax / (float)this.ExperienceForNextLevel;
		}
	}

	public int ExperienceForNextLevel
	{
		get
		{
			return Mathf.RoundToInt(this.ExperienceRequiredPerLevel.Evaluate((float)this.Current));
		}
	}

	public int ConsumedExperience
	{
		get
		{
			int num = 0;
			for (int i = this.Current - 1; i >= 0; i--)
			{
				num += Mathf.RoundToInt(this.ExperienceRequiredPerLevel.Evaluate((float)i));
			}
			return num;
		}
	}

	public void GainExperience(int amount)
	{
		this.Experience += amount;
		this.ExperienceVisualMax = (float)this.Experience;
	}

	public void Update()
	{
	}

	public void FixedUpdate()
	{
		if (this.m_sein.IsSuspended)
		{
			return;
		}
		float maxDelta = Time.deltaTime * this.ExperienceGainPerSecond * (float)this.ExperienceForNextLevel;
		this.ExperienceVisualMax = Mathf.MoveTowards(this.ExperienceVisualMax, (float)this.Experience, maxDelta);
		this.ExperienceVisualMin = Mathf.MoveTowards(this.ExperienceVisualMin, (float)this.Experience, maxDelta);
		if (this.ExperienceVisualMin >= (float)this.ExperienceForNextLevel)
		{
			this.LevelUp();
		}
	}

	public void LevelUp()
	{
		this.Experience -= this.ExperienceForNextLevel;
		this.ExperienceVisualMin = 0f;
		this.ExperienceVisualMax = (float)this.Experience;
		if (this.Current < 99)
		{
			this.Current++;
			this.SkillPoints++;
		}
		this.AttemptInstantiateLevelUp();
	}

	public void LoseExperience(int amount)
	{
		this.Experience -= amount;
		this.ExperienceVisualMin = (float)this.Experience;
		if (this.Experience < 0)
		{
			this.Experience = 0;
		}
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.Current);
		ar.Serialize(ref this.Experience);
		ar.Serialize(ref this.SkillPoints);
		ar.Serialize(ref SeinLevel.HasSpentSkillPoint);
		if (ar.Reading)
		{
			this.ExperienceVisualMax = (this.ExperienceVisualMin = (float)this.Current);
		}
	}

	public float ApplyLevelingToDamage(float damage)
	{
		return damage + damage * (float)this.m_sein.PlayerAbilities.OriStrength * 0.5f;
	}

	public float CalculateLevelBasedMaxHealth(int level, float health)
	{
		return (float)Mathf.RoundToInt(health * this.DamageMultiplierPerOriStrength.Evaluate((float)level));
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
	}

	public void GainSkillPoint()
	{
		this.SkillPoints++;
	}

	public void AttemptInstantiateLevelUp()
	{
		if (this.OnLevelUpGameObject)
		{
			GameObject obj = (GameObject)InstantiateUtility.Instantiate(this.OnLevelUpGameObject, Characters.Sein.Position, Quaternion.identity);
			TargetPositionFollower target = obj.GetComponent<TargetPositionFollower>();
			target.Target = Characters.Sein.Transform;
		}
	}

	public int SkillPoints;

	public int Current;

	public AnimationCurve DamageMultiplierPerOriStrength;

	public int Experience;

	public float ExperienceVisualMin;

	public float ExperienceVisualMax;

	public AnimationCurve ExperienceRequiredPerLevel;

	public GameObject OnLevelUpGameObject;

	public static bool HasSpentSkillPoint = false;

	public float ExperienceGainPerSecond = 30f;

	private static readonly HashSet<string> CollectablesToSerialize = new HashSet<string>
	{
		"largeExpOrbPlaceholder",
		"mediumExpOrbPlaceholder",
		"smallExpOrbPlaceholder"
	};

	private static HashSet<Type> TypesToSerialize = new HashSet<Type>
	{
		typeof(ExpOrbPickup)
	};

	private SeinCharacter m_sein;
}
