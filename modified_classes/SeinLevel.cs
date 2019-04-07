using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

// Token: 0x0200032E RID: 814
public class SeinLevel : SaveSerialize, ISeinReceiver
{
	// Token: 0x170002FD RID: 765
	// (get) Token: 0x060011C0 RID: 4544 RVA: 0x0000FA68 File Offset: 0x0000DC68
	public int TotalExperience
	{
		get
		{
			return this.Experience + this.ConsumedExperience;
		}
	}

	// Token: 0x170002FE RID: 766
	// (get) Token: 0x060011C1 RID: 4545 RVA: 0x0000FA77 File Offset: 0x0000DC77
	public int TotalExperienceForNextLevel
	{
		get
		{
			return this.ExperienceForNextLevel + this.ConsumedExperience;
		}
	}

	// Token: 0x170002FF RID: 767
	// (get) Token: 0x060011C2 RID: 4546 RVA: 0x0000FA86 File Offset: 0x0000DC86
	public int ExperienceNeedForNextLevel
	{
		get
		{
			return this.ExperienceForNextLevel - this.Experience;
		}
	}

	// Token: 0x17000300 RID: 768
	// (get) Token: 0x060011C3 RID: 4547 RVA: 0x0000FA95 File Offset: 0x0000DC95
	public float ExperienceVisualMinNormalized
	{
		get
		{
			return this.ExperienceVisualMin / (float)this.ExperienceForNextLevel;
		}
	}

	// Token: 0x17000301 RID: 769
	// (get) Token: 0x060011C4 RID: 4548 RVA: 0x0000FAA5 File Offset: 0x0000DCA5
	public float ExperienceVisualMaxNormalized
	{
		get
		{
			return this.ExperienceVisualMax / (float)this.ExperienceForNextLevel;
		}
	}

	// Token: 0x17000302 RID: 770
	// (get) Token: 0x060011C5 RID: 4549 RVA: 0x0000FAB5 File Offset: 0x0000DCB5
	public int ExperienceForNextLevel
	{
		get
		{
			return Mathf.RoundToInt(this.ExperienceRequiredPerLevel.Evaluate((float)(this.Current % 128)));
		}
	}

	// Token: 0x17000303 RID: 771
	// (get) Token: 0x060011C6 RID: 4550
	public int ConsumedExperience
	{
		get
		{
			int num = 0;
			for (int i = this.Current % 128 - 1; i >= 0; i--)
			{
				num += Mathf.RoundToInt(this.ExperienceRequiredPerLevel.Evaluate((float)i));
			}
			return num;
		}
	}

	// Token: 0x060011C7 RID: 4551 RVA: 0x0000FAD4 File Offset: 0x0000DCD4
	public void GainExperience(int amount)
	{
		this.Experience += amount;
		this.ExperienceVisualMax = (float)this.Experience;
	}

	// Token: 0x060011C8 RID: 4552 RVA: 0x000028E7 File Offset: 0x00000AE7
	public void Update()
	{
	}

	// Token: 0x060011C9 RID: 4553 RVA: 0x00067AF0 File Offset: 0x00065CF0
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

	// Token: 0x060011CA RID: 4554 RVA: 0x00067B70 File Offset: 0x00065D70
	public void LevelUp()
	{
		this.Experience -= this.ExperienceForNextLevel;
		this.ExperienceVisualMin = 0f;
		this.ExperienceVisualMax = (float)this.Experience;
		if (this.Current % 128 < 99)
		{
			this.Current++;
			this.SkillPoints++;
		}
		if (this.OnLevelUpGameObject)
		{
			((GameObject)InstantiateUtility.Instantiate(this.OnLevelUpGameObject, Characters.Sein.Position, Quaternion.identity)).GetComponent<TargetPositionFollower>().Target = Characters.Sein.Transform;
		}
	}

	// Token: 0x060011CB RID: 4555 RVA: 0x0000FAF1 File Offset: 0x0000DCF1
	public void LoseExperience(int amount)
	{
		this.Experience -= amount;
		this.ExperienceVisualMin = (float)this.Experience;
		if (this.Experience < 0)
		{
			this.Experience = 0;
		}
	}

	// Token: 0x060011CC RID: 4556
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.Current);
		ar.Serialize(ref this.Experience);
		ar.Serialize(ref this.SkillPoints);
		ar.Serialize(ref SeinLevel.HasSpentSkillPoint);
		if (ar.Reading)
		{
			this.ExperienceVisualMax = (this.ExperienceVisualMin = (float)(this.Current % 128));
		}
	}

	// Token: 0x060011CD RID: 4557 RVA: 0x0000FB21 File Offset: 0x0000DD21
	public float ApplyLevelingToDamage(float damage)
	{
		return damage + damage * (float)this.m_sein.PlayerAbilities.OriStrength * 0.5f;
	}

	// Token: 0x060011CE RID: 4558 RVA: 0x0000FB3E File Offset: 0x0000DD3E
	public float CalculateLevelBasedMaxHealth(int level, float health)
	{
		return (float)Mathf.RoundToInt(health * this.DamageMultiplierPerOriStrength.Evaluate((float)level));
	}

	// Token: 0x060011CF RID: 4559 RVA: 0x0000FB55 File Offset: 0x0000DD55
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
	}

	// Token: 0x060011D0 RID: 4560 RVA: 0x0000FB5E File Offset: 0x0000DD5E
	public void GainSkillPoint()
	{
		this.SkillPoints++;
	}

	// Token: 0x040010BD RID: 4285
	public int SkillPoints;

	// Token: 0x040010BE RID: 4286
	public int Current;

	// Token: 0x040010BF RID: 4287
	public AnimationCurve DamageMultiplierPerOriStrength;

	// Token: 0x040010C0 RID: 4288
	public int Experience;

	// Token: 0x040010C1 RID: 4289
	public float ExperienceVisualMin;

	// Token: 0x040010C2 RID: 4290
	public float ExperienceVisualMax;

	// Token: 0x040010C3 RID: 4291
	public AnimationCurve ExperienceRequiredPerLevel;

	// Token: 0x040010C4 RID: 4292
	public GameObject OnLevelUpGameObject;

	// Token: 0x040010C5 RID: 4293
	public static bool HasSpentSkillPoint = false;

	// Token: 0x040010C6 RID: 4294
	public float ExperienceGainPerSecond = 30f;

	// Token: 0x040010C7 RID: 4295
	private static readonly HashSet<string> CollectablesToSerialize = new HashSet<string>
	{
		"largeExpOrbPlaceholder",
		"mediumExpOrbPlaceholder",
		"smallExpOrbPlaceholder"
	};

	// Token: 0x040010C8 RID: 4296
	private static HashSet<Type> TypesToSerialize = new HashSet<Type>
	{
		typeof(ExpOrbPickup)
	};

	// Token: 0x040010C9 RID: 4297
	private SeinCharacter m_sein;
}
