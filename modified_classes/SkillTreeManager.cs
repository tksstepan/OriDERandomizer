using System;
using System.Collections.Generic;
using System.Text;
using Game;
using UnityEngine;

public class SkillTreeManager : MenuScreen
{
	public bool AllLanesFull
	{
		get
		{
			return this.EnergyLane.HasAllSkills && this.UtilityLane.HasAllSkills && this.CombatLane.HasAllSkills;
		}
	}

	public void Awake()
	{
		SkillTreeManager.Instance = this;
		CleverMenuItemSelectionManager navigationManager = this.NavigationManager;
		navigationManager.OptionChangeCallback = (Action)Delegate.Combine(navigationManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
		navigationManager.OptionPressedCallback = (Action)Delegate.Combine(navigationManager.OptionPressedCallback, new Action(this.OnMenuItemPressed));
		navigationManager.OnBackPressedCallback = (Action)Delegate.Combine(navigationManager.OnBackPressedCallback, new Action(this.OnBackPressed));
		this.OnMenuItemChange();
		foreach (CleverMenuItemSelectionManager.NavigationData navigationData in this.NavigationManager.Navigation)
		{
			navigationData.Condition = new Func<CleverMenuItemSelectionManager.NavigationData, bool>(SkillTreeManager.Condition);
		}
		this.UpdateRequirementsText();
	}

	public void OnBackPressed()
	{
		UI.Menu.HideMenuScreen(false);
	}

	public override void Hide()
	{
		this.NavigationManager.SetVisible(false);
	}

	public override void ShowImmediate()
	{
		this.NavigationManager.SetVisibleImmediate(true);
		this.OnMenuItemChange();
	}

	public override void HideImmediate()
	{
		this.NavigationManager.SetVisibleImmediate(false);
	}

	public override void Show()
	{
		this.NavigationManager.SetVisible(true);
		this.OnMenuItemChange();
	}

	public static bool Condition(CleverMenuItemSelectionManager.NavigationData navigationData)
	{
		SkillItem component = navigationData.To.GetComponent<SkillItem>();
		return !component || component.Visible;
	}

	public void OnDestroy()
	{
		CleverMenuItemSelectionManager navigationManager = this.NavigationManager;
		navigationManager.OptionChangeCallback = (Action)Delegate.Remove(navigationManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
		navigationManager.OptionPressedCallback = (Action)Delegate.Remove(navigationManager.OptionPressedCallback, new Action(this.OnMenuItemPressed));
		navigationManager.OnBackPressedCallback = (Action)Delegate.Remove(navigationManager.OnBackPressedCallback, new Action(this.OnBackPressed));
		SkillTreeManager.Instance = null;
	}

	public void OnMenuItemPressed()
	{
		if (this.CurrentSkillItem == null)
		{
			if (Characters.Sein && !Characters.Sein.IsSuspended)
			{
				this.NavigationManager.Index = -1;
			}
			return;
		}
		if (this.CurrentSkillItem.HasSkillItem)
		{
			if (this.OnAlreadyEarnedAbility)
			{
				this.RequirementsLineAShake.Restart();
				this.OnAlreadyEarnedAbility.Perform(null);
			}
			return;
		}
		if (this.CurrentSkillItem.CanEarnSkill)
		{
			this.CurrentSkillItem.HasSkillItem = true;
			Characters.Sein.PlayerAbilities.SetAbility(this.CurrentSkillItem.Ability, true);
			Characters.Sein.PlayerAbilities.GainAbilityAction = this.CurrentSkillItem.GainSkillSequence;
			InstantiateUtility.Instantiate(this.GainSkillEffect, this.CurrentSkillItem.transform.position, Quaternion.identity);
			RandomizerBonus.SpentAP(this.CurrentSkillItem.ActualRequiredSkillPoints);
			BingoController.OnGainAbility(this.CurrentSkillItem.Ability);			
			Characters.Sein.Level.SkillPoints -= this.CurrentSkillItem.ActualRequiredSkillPoints;
			if (this.OnGainAbility)
			{
				this.OnGainAbility.Perform(null);
			}
			SeinLevel.HasSpentSkillPoint = true;
			AchievementsController.AwardAchievement(this.SpentFirstSkillPointAchievement);
			GameController.Instance.CreateCheckpoint();
			RandomizerStatsManager.OnSave(false);
			GameController.Instance.SaveGameController.PerformSave();
			this.UpdateRequirementsText();
			return;
		}
		if (!this.CurrentSkillItem.SoulRequirementMet)
		{
			if (this.CurrentSkillItem.RequiresAbilitiesOrItems)
			{
				this.RequirementsLineAShake.Restart();
			}
			else
			{
				this.RequirementsLineAShake.Restart();
			}
		}
		if (!this.CurrentSkillItem.AbilitiesRequirementMet)
		{
			this.RequirementsLineAShake.Restart();
		}
		if (this.OnCantEarnSkill)
		{
			this.OnCantEarnSkill.Perform(null);
		}
	}

	public MessageDescriptor AbilityMastered
	{
		get
		{
			return new MessageDescriptor("$" + this.AbilityMasteredMessageProvider + "$");
		}
	}

	public MessageProvider AbilityName(AbilityType ability)
	{
		foreach (SkillTreeManager.AbilityMessageProvider abilityMessageProvider in this.AbilityMessages)
		{
			if (abilityMessageProvider.AbilityType == ability)
			{
				return abilityMessageProvider.MessageProvider;
			}
		}
		return null;
	}

	public string RequiredAbilitiesText(SkillItem skillItem)
	{
		bool abilitiesRequirementMet = skillItem.AbilitiesRequirementMet;
		StringBuilder stringBuilder = new StringBuilder(30);
		stringBuilder.Append(" ");
		for (int j = 0; j < skillItem.RequiredItems.Count; j++)
		{
			SkillItem skillItem2 = skillItem.RequiredItems[j];
			if (abilitiesRequirementMet)
			{
				stringBuilder.Append("$" + skillItem2.Name + "$");
			}
			else
			{
				stringBuilder.Append("#" + skillItem2.Name + "#");
			}
			if (j != skillItem.RequiredItems.Count - 1)
			{
				stringBuilder.Append((!abilitiesRequirementMet) ? "@,@ " : "$,$ ");
			}
		}
		if (abilitiesRequirementMet)
		{
			return "$" + this.RequiresMessageProvider.ToString().Replace("[Requirements]", "$" + stringBuilder + "$") + "$";
		}
		return "@" + this.RequiresMessageProvider.ToString().Replace("[Requirements]", "@" + stringBuilder + "@") + "@";
	}

	public void UpdateRequirementsText()
	{
		this.CurrentSkillItem = this.NavigationManager.CurrentMenuItem.GetComponent<SkillItem>();
		if (this.CurrentSkillItem)
		{
			this.AbilityTitle.SetMessageProvider(this.CurrentSkillItem.Name);
			this.AbilityDescription.SetMessageProvider(this.CurrentSkillItem.Description);
			if (this.CurrentSkillItem.HasSkillItem)
			{
				this.RequirementsLineA.SetMessage(this.AbilityMastered);
				return;
			}
			if (this.CurrentSkillItem.RequiresAbilitiesOrItems)
			{
				this.RequirementsLineA.SetMessage(new MessageDescriptor(this.RequiredAbilitiesText(this.CurrentSkillItem) + "\n" + this.RequiredSoulsText(this.CurrentSkillItem)));
				return;
			}
			this.RequirementsLineA.SetMessage(new MessageDescriptor(this.RequiredSoulsText(this.CurrentSkillItem)));
		}
	}

	public string NameText(SkillItem skillItem)
	{
		if (skillItem.HasSkillItem)
		{
			return "$" + skillItem.Name + "$";
		}
		if (skillItem.CanEarnSkill)
		{
			return "#" + skillItem.Name + "#";
		}
		return "@" + skillItem.Name + "@";
	}

	public string RequiredSoulsText(SkillItem skillItem)
	{
		if (skillItem.HasSkillItem)
		{
			return string.Empty;
		}
		int requiredPoints = skillItem.ActualRequiredSkillPoints;
		int totalRequiredPoints = skillItem.ActualTotalRequiredSkillPoints;
		string costMessage = (requiredPoints != 1) ? RandomizerText.CostsAbilityPoints : RandomizerText.CostsAbilityPoint;
		if (totalRequiredPoints <= Characters.Sein.Level.SkillPoints)
		{
			return "$" + costMessage.Replace("[Amount]", requiredPoints.ToString()).Replace("[Total]", totalRequiredPoints.ToString()) + "$";
		}
		return "@" + costMessage.Replace("[Amount]", requiredPoints.ToString()).Replace("[Total]", totalRequiredPoints.ToString()) + "@";
	}

	public void OnMenuItemChange()
	{
		this.CurrentSkillItem = this.NavigationManager.CurrentMenuItem.GetComponent<SkillItem>();
		if (this.CurrentSkillItem == null)
		{
			this.Cursor.gameObject.SetActive(false);
			this.InfoPanel.SetActive(false);
			this.AbilityDiskInfoPanel.SetActive(true);
			this.AbilityDiskInfoPanelDescription.RefreshText();
			return;
		}
		this.Cursor.gameObject.SetActive(true);
		this.Cursor.position = this.CurrentSkillItem.transform.position;
		foreach (object obj in this.LargeIcon.transform)
		{
			Transform transform = (Transform)obj;
			transform.gameObject.SetActive(transform.name == this.CurrentSkillItem.LargeIcon.name);
		}
		this.InfoPanel.SetActive(true);
		this.AbilityDiskInfoPanel.SetActive(false);
		this.UpdateRequirementsText();
	}

	public void FixedUpdate()
	{
		if (this.NavigationManager.Index == -1)
		{
			this.NavigationManager.Index = 0;
		}
	}

	public static SkillTreeManager Instance;

	public CleverMenuItemSelectionManager NavigationManager;

	public SkillItem CurrentSkillItem;

	public Transform Cursor;

	public SoundProvider OpenSound;

	public SoundProvider CloseSound;

	public GameObject LargeIcon;

	public Renderer LargeIconGlow;

	public MessageBox RequirementsLineA;

	public MessageBox AbilityTitle;

	public MessageBox AbilityDescription;

	public GameObject InfoPanel;

	public MessageBox AbilityDiskInfoPanelDescription;

	public GameObject AbilityDiskInfoPanel;

	public SkillTreeLaneLogic EnergyLane;

	public SkillTreeLaneLogic UtilityLane;

	public SkillTreeLaneLogic CombatLane;

	public GameObject GainSkillEffect;

	public LegacyAnimator RequirementsLineAShake;

	public ActionMethod OnGainAbility;

	public ActionMethod OnAlreadyEarnedAbility;

	public ActionMethod OnCantEarnSkill;

	public MessageProvider AbilityPointMessageProvider;

	public MessageProvider AbilityPointsMessageProvider;

	public MessageProvider RequiresMessageProvider;

	public MessageProvider AbilityMasteredMessageProvider;

	public AchievementAsset SpentFirstSkillPointAchievement;

	public List<SkillTreeManager.AbilityMessageProvider> AbilityMessages;

	[Serializable]
	public class AbilityMessageProvider
	{
		public AbilityType AbilityType;

		public MessageProvider MessageProvider;
	}
}
