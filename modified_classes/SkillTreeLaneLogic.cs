using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeLaneLogic : SaveSerialize
{
	public float Index
	{
		get
		{
			return this.m_index;
		}
	}

	public void OnEnable()
	{
		this.UpdateItems(true);
		foreach (SkillItem skillItem in this.Skills)
		{
			skillItem.LargeIconColor = this.LargeIconColor;
		}
	}

	public void FixedUpdate()
	{
		this.UpdateItems(false);
	}

	public void UpdateItems(bool instant)
	{
		int firstUnlearnedIndex = 0;
		int totalPointsNeeded = 0;
		int totalHardPointsNeeded = 0;
		for (int i = 0; i < this.Skills.Count; i++)
		{
			SkillItem skillItem = this.Skills[i];
			if (!skillItem.HasSkillItem)
			{
				if (firstUnlearnedIndex == 0)
				{
					firstUnlearnedIndex = i + 1;
				}
				
				totalPointsNeeded += skillItem.RequiredSkillPoints;
				totalHardPointsNeeded += skillItem.RequiredHardSkillPoints;
				skillItem.TotalRequiredSkillPoints = totalPointsNeeded;
				skillItem.TotalRequiredHardSkillPoints = totalHardPointsNeeded;
			}
		}
		--firstUnlearnedIndex;
		this.m_index = ((!instant) ? Mathf.MoveTowards(this.m_index, (float)firstUnlearnedIndex, Time.deltaTime * 3f) : ((float)firstUnlearnedIndex));
		this.SkillEarntAnimator.Initialize();
		this.SkillEarntAnimator.SampleValue(this.m_index, true);
		if (!this.m_laneAchievedAwarded && this.HasAllSkills)
		{
			SkillTreeLaneLogic.OnSkillTreeDoneEvent(this.Type);
			this.m_laneAchievedAwarded = true;
		}
	}

	public bool HasAllSkills
	{
		get
		{
			bool result = true;
			for (int i = 0; i < this.Skills.Count; i++)
			{
				if (!this.Skills[i].HasSkillItem)
				{
					result = false;
					break;
				}
			}
			return result;
		}
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_laneAchievedAwarded);
	}

	public BaseAnimator SkillEarntAnimator;

	public List<SkillItem> Skills = new List<SkillItem>();

	private float m_index;

	public Color LargeIconColor;

	public SkillTreeLaneLogic.SkillTreeType Type;

	private bool m_laneAchievedAwarded;

	public static Action<SkillTreeLaneLogic.SkillTreeType> OnSkillTreeDoneEvent = delegate(SkillTreeLaneLogic.SkillTreeType A_0)
	{
	};

	public enum SkillTreeType
	{
		Energy,
		Utility,
		Combat
	}
}