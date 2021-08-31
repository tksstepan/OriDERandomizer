using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000D1 RID: 209
public class SkillTreeLaneLogic : SaveSerialize
{
	// Token: 0x0600050E RID: 1294 RVA: 0x000067F8 File Offset: 0x000049F8
	public SkillTreeLaneLogic()
	{
	}

	// Token: 0x0600050F RID: 1295 RVA: 0x0000680B File Offset: 0x00004A0B
	static SkillTreeLaneLogic()
	{
	}

	// Token: 0x170000D1 RID: 209
	// (get) Token: 0x06000510 RID: 1296 RVA: 0x0000682F File Offset: 0x00004A2F
	public float Index
	{
		get
		{
			return this.m_index;
		}
	}

	// Token: 0x06000511 RID: 1297 RVA: 0x0003F198 File Offset: 0x0003D398
	public void OnEnable()
	{
		this.UpdateItems(true);
		foreach (SkillItem skillItem in this.Skills)
		{
			skillItem.LargeIconColor = this.LargeIconColor;
		}
	}

	// Token: 0x06000512 RID: 1298 RVA: 0x00006837 File Offset: 0x00004A37
	public void FixedUpdate()
	{
		this.UpdateItems(false);
	}

	// Token: 0x06000513 RID: 1299 RVA: 0x0003F220 File Offset: 0x0003D420
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

	// Token: 0x170000D2 RID: 210
	// (get) Token: 0x06000514 RID: 1300 RVA: 0x0003F2DC File Offset: 0x0003D4DC
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

	// Token: 0x06000515 RID: 1301 RVA: 0x00006840 File Offset: 0x00004A40
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_laneAchievedAwarded);
	}

	// Token: 0x040004B4 RID: 1204
	public BaseAnimator SkillEarntAnimator;

	// Token: 0x040004B5 RID: 1205
	public List<SkillItem> Skills = new List<SkillItem>();

	// Token: 0x040004B6 RID: 1206
	private float m_index;

	// Token: 0x040004B7 RID: 1207
	public Color LargeIconColor;

	// Token: 0x040004B8 RID: 1208
	public SkillTreeLaneLogic.SkillTreeType Type;

	// Token: 0x040004B9 RID: 1209
	private bool m_laneAchievedAwarded;

	// Token: 0x040004BA RID: 1210
	public static Action<SkillTreeLaneLogic.SkillTreeType> OnSkillTreeDoneEvent = delegate(SkillTreeLaneLogic.SkillTreeType A_0)
	{
	};

	// Token: 0x020000D2 RID: 210
	public enum SkillTreeType
	{
		// Token: 0x040004BD RID: 1213
		Energy,
		// Token: 0x040004BE RID: 1214
		Utility,
		// Token: 0x040004BF RID: 1215
		Combat
	}
}