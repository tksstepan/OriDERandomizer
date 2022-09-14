using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class SkillItem : MonoBehaviour
{
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

	public int ActualTotalRequiredSkillPoints
	{
		get
		{
			if (DifficultyController.Instance.Difficulty == DifficultyMode.Hard)
			{
				return this.TotalRequiredHardSkillPoints;
			}
			return this.TotalRequiredSkillPoints;
		}
	}

	public int TotalRequiredHardSkillPoints
	{
		get
		{
			return this.m_totalRequiredHardPoints;
		}
		set
		{
			this.m_totalRequiredHardPoints = value;
		}
	}

	public int TotalRequiredSkillPoints
	{
		get
		{
			return this.m_totalRequiredPoints;
		}
		set
		{
			this.m_totalRequiredPoints = value;
		}
	}

	public Color LargeIconColor
	{
		get;
		set;
	}

	public bool Visible
	{
		get
		{
			return true;
		}
	}

	public bool RequiresAbilitiesOrItems
	{
		get
		{
			return this.RequiredAbilities.Count != 0 || this.RequiredItems.Count != 0;
		}
	}

	public bool SoulRequirementMet
	{
		get
		{
			return this.ActualRequiredSkillPoints <= Characters.Sein.Level.SkillPoints;
		}
	}

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

	public void Awake()
	{
		this.m_animator = this.Icon.GetComponent<TransparencyAnimator>();
	}

	public bool CanEarnSkill
	{
		get
		{
			return this.SoulRequirementMet && this.AbilitiesRequirementMet;
		}
	}

	public void FixedUpdate()
	{
		this.UpdateItem();
	}

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

	public int RequiredSkillPoints = 1;

	public int RequiredHardSkillPoints = 1;

	public List<AbilityType> RequiredAbilities = new List<AbilityType>();

	public List<SkillItem> RequiredItems = new List<SkillItem>();

	public AbilityType Ability;

	public Texture LargeIcon;

	public MessageProvider NameMessageProvider;

	public MessageProvider DescriptionMessageProvider;

	public Renderer Icon;

	public ActionMethod GainSkillSequence;

	private TransparencyAnimator m_animator;

	public GameObject LearntSkillGlow;

	public bool HasSkillItem;

	private int m_totalRequiredPoints = 0;

	private int m_totalRequiredHardPoints = 0;
}
