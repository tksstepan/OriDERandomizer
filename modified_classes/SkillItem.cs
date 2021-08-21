using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

// Token: 0x020000CD RID: 205
public class SkillItem : MonoBehaviour
{
	// Token: 0x170000C8 RID: 200
	// (get) Token: 0x060004F7 RID: 1271 RVA: 0x00005EBF File Offset: 0x000040BF
	public int ActualRequiredSkillPoints
	{
		get
		{
			if (DifficultyController.Instance.Difficulty == DifficultyMode.Hard)
			{
				return this.RequiredHardSkillPoints;
			}
			return this.RequiredSkillPoints;
		}
	}

	// Token: 0x170000C9 RID: 201
	// (get) Token: 0x060004F8 RID: 1272 RVA: 0x00005EDE File Offset: 0x000040DE
	// (set) Token: 0x060004F9 RID: 1273 RVA: 0x00005EE6 File Offset: 0x000040E6
	public Color LargeIconColor
	{
		get;
		set;
	}

	// Token: 0x170000CA RID: 202
	// (get) Token: 0x060004FA RID: 1274 RVA: 0x00004236 File Offset: 0x00002436
	public bool Visible
	{
		get
		{
			return true;
		}
	}

	// Token: 0x170000CB RID: 203
	// (get) Token: 0x060004FB RID: 1275 RVA: 0x00005EEF File Offset: 0x000040EF
	public bool RequiresAbilitiesOrItems
	{
		get
		{
			return this.RequiredAbilities.Count != 0 || this.RequiredItems.Count != 0;
		}
	}

	// Token: 0x170000CC RID: 204
	// (get) Token: 0x060004FC RID: 1276 RVA: 0x00005F15 File Offset: 0x00004115
	public bool SoulRequirementMet
	{
		get
		{
			return this.ActualRequiredSkillPoints <= Characters.Sein.Level.SkillPoints;
		}
	}

	// Token: 0x170000CD RID: 205
	// (get) Token: 0x060004FD RID: 1277 RVA: 0x0003CEB0 File Offset: 0x0003B0B0
	public bool AbilitiesRequirementMet
	{
		get
		{
			using (List<SkillItem>.Enumerator enumerator = this.RequiredItems.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.HasSkillItem)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	// Token: 0x060004FE RID: 1278 RVA: 0x00005F31 File Offset: 0x00004131
	public void Awake()
	{
		this.m_animator = this.Icon.GetComponent<TransparencyAnimator>();
	}

	// Token: 0x170000CE RID: 206
	// (get) Token: 0x060004FF RID: 1279 RVA: 0x00005F44 File Offset: 0x00004144
	public bool CanEarnSkill
	{
		get
		{
			return this.SoulRequirementMet && this.AbilitiesRequirementMet;
		}
	}

	// Token: 0x06000500 RID: 1280 RVA: 0x00005F5A File Offset: 0x0000415A
	public void FixedUpdate()
	{
		this.UpdateItem();
	}

	// Token: 0x06000501 RID: 1281 RVA: 0x0003CF0C File Offset: 0x0003B10C
	public void UpdateItem()
	{
		this.LearntSkillGlow.SetActive(this.HasSkillItem && this.Visible);
		this.Icon.gameObject.SetActive(this.Visible);
		if (this.HasSkillItem == this.m_animator.AnimatorDriver.IsReversed)
		{
			this.m_animator.Initialize();
			if (this.HasSkillItem)
			{
				this.m_animator.AnimatorDriver.ContinueForward();
			}
			else
			{
				this.m_animator.AnimatorDriver.ContinueBackwards();
			}
		}
	}

	// Token: 0x06000502 RID: 1282 RVA: 0x0003CFA4 File Offset: 0x0003B1A4
	public void OnEnable()
	{
		this.HasSkillItem = Characters.Sein.PlayerAbilities.HasAbility(this.Ability);
		this.UpdateItem();
		this.m_animator.Initialize();
		if (this.HasSkillItem)
		{
			this.m_animator.AnimatorDriver.GoToEnd();
		}
		else
		{
			this.m_animator.AnimatorDriver.GoToStart();
		}
	}

	public MessageProvider Name
	{
		get
		{
			return RandomizerText.GetAbilityName(this.Ability) ?? NameMessageProvider;
		}
	}

	public MessageProvider Description
	{
		get
		{
			return RandomizerText.GetAbilityDescription(this.Ability) ?? DescriptionMessageProvider;
		}
	}

	// Token: 0x040004A3 RID: 1187
	public int RequiredSkillPoints = 1;

	// Token: 0x040004A4 RID: 1188
	public int RequiredHardSkillPoints = 1;

	// Token: 0x040004A5 RID: 1189
	public List<AbilityType> RequiredAbilities = new List<AbilityType>();

	// Token: 0x040004A6 RID: 1190
	public List<SkillItem> RequiredItems = new List<SkillItem>();

	// Token: 0x040004A7 RID: 1191
	public AbilityType Ability;

	// Token: 0x040004A8 RID: 1192
	public Texture LargeIcon;

	// Token: 0x040004A9 RID: 1193
	public MessageProvider NameMessageProvider;

	// Token: 0x040004AA RID: 1194
	public MessageProvider DescriptionMessageProvider;

	// Token: 0x040004AB RID: 1195
	public Renderer Icon;

	// Token: 0x040004AC RID: 1196
	public ActionMethod GainSkillSequence;

	// Token: 0x040004AD RID: 1197
	private TransparencyAnimator m_animator;

	// Token: 0x040004AE RID: 1198
	public GameObject LearntSkillGlow;

	// Token: 0x040004AF RID: 1199
	public bool HasSkillItem;
}
