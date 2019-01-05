using System;
using System.Collections.Generic;
using System.Text;
using Game;
using UnityEngine;

// Token: 0x020000D2 RID: 210
public class SkillTreeManager : MenuScreen
{
	// Token: 0x170000D1 RID: 209
	// (get) Token: 0x06000511 RID: 1297
	public bool AllLanesFull
	{
		get
		{
			return this.EnergyLane.HasAllSkills && this.UtilityLane.HasAllSkills && this.CombatLane.HasAllSkills;
		}
	}

	// Token: 0x06000512 RID: 1298
	public void Awake()
	{
		SkillTreeManager.Instance = this;
		CleverMenuItemSelectionManager navigationManager = this.NavigationManager;
		navigationManager.OptionChangeCallback = (Action)Delegate.Combine(navigationManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
		CleverMenuItemSelectionManager navigationManager2 = this.NavigationManager;
		navigationManager2.OptionPressedCallback = (Action)Delegate.Combine(navigationManager2.OptionPressedCallback, new Action(this.OnMenuItemPressed));
		CleverMenuItemSelectionManager navigationManager3 = this.NavigationManager;
		navigationManager3.OnBackPressedCallback = (Action)Delegate.Combine(navigationManager3.OnBackPressedCallback, new Action(this.OnBackPressed));
		this.OnMenuItemChange();
		foreach (CleverMenuItemSelectionManager.NavigationData navigationData in this.NavigationManager.Navigation)
		{
			navigationData.Condition = new Func<CleverMenuItemSelectionManager.NavigationData, bool>(SkillTreeManager.Condition);
		}
		this.UpdateRequirementsText();
	}

	// Token: 0x06000513 RID: 1299
	public void OnBackPressed()
	{
		UI.Menu.HideMenuScreen(false);
	}

	// Token: 0x06000514 RID: 1300
	public override void Hide()
	{
		this.NavigationManager.SetVisible(false);
	}

	// Token: 0x06000515 RID: 1301
	public override void ShowImmediate()
	{
		this.NavigationManager.SetVisibleImmediate(true);
		this.OnMenuItemChange();
	}

	// Token: 0x06000516 RID: 1302
	public override void HideImmediate()
	{
		this.NavigationManager.SetVisibleImmediate(false);
	}

	// Token: 0x06000517 RID: 1303
	public override void Show()
	{
		this.NavigationManager.SetVisible(true);
		this.OnMenuItemChange();
	}

	// Token: 0x06000518 RID: 1304
	public static bool Condition(CleverMenuItemSelectionManager.NavigationData navigationData)
	{
		SkillItem component = navigationData.To.GetComponent<SkillItem>();
		return !component || component.Visible;
	}

	// Token: 0x06000519 RID: 1305
	public void OnDestroy()
	{
		CleverMenuItemSelectionManager navigationManager = this.NavigationManager;
		navigationManager.OptionChangeCallback = (Action)Delegate.Remove(navigationManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
		CleverMenuItemSelectionManager navigationManager2 = this.NavigationManager;
		navigationManager2.OptionPressedCallback = (Action)Delegate.Remove(navigationManager2.OptionPressedCallback, new Action(this.OnMenuItemPressed));
		CleverMenuItemSelectionManager navigationManager3 = this.NavigationManager;
		navigationManager3.OnBackPressedCallback = (Action)Delegate.Remove(navigationManager3.OnBackPressedCallback, new Action(this.OnBackPressed));
		SkillTreeManager.Instance = null;
	}

	// Token: 0x0600051A RID: 1306
	public void OnMenuItemPressed()
	{
		if (this.CurrentSkillItem == null)
		{
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

	// Token: 0x170000D2 RID: 210
	// (get) Token: 0x0600051B RID: 1307
	public MessageDescriptor AbilityMastered
	{
		get
		{
			return new MessageDescriptor("$" + this.AbilityMasteredMessageProvider + "$");
		}
	}

	// Token: 0x0600051C RID: 1308
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

	// Token: 0x0600051D RID: 1309
	public string RequiredAbilitiesText(SkillItem skillItem)
	{
		bool abilitiesRequirementMet = skillItem.AbilitiesRequirementMet;
		StringBuilder stringBuilder = new StringBuilder(30);
		stringBuilder.Append(" ");
		for (int i = 0; i < skillItem.RequiredAbilities.Count; i++)
		{
			AbilityType ability = skillItem.RequiredAbilities[i];
			if (abilitiesRequirementMet)
			{
				MessageProvider messageProvider = this.AbilityName(ability);
				if (messageProvider)
				{
					stringBuilder.Append("$" + messageProvider + "$");
				}
			}
			else
			{
				MessageProvider messageProvider2 = this.AbilityName(ability);
				if (messageProvider2)
				{
					stringBuilder.Append("#" + messageProvider2 + "#");
				}
			}
			if (i != skillItem.RequiredAbilities.Count - 1 || skillItem.RequiredItems.Count > 0)
			{
				stringBuilder.Append((!abilitiesRequirementMet) ? "@,@ " : "$,$ ");
			}
		}
		for (int j = 0; j < skillItem.RequiredItems.Count; j++)
		{
			SkillItem skillItem2 = skillItem.RequiredItems[j];
			if (abilitiesRequirementMet)
			{
				stringBuilder.Append("$" + skillItem2.NameMessageProvider + "$");
			}
			else
			{
				stringBuilder.Append("#" + skillItem2.NameMessageProvider + "#");
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

	// Token: 0x0600051E RID: 1310
	public void UpdateRequirementsText()
	{
		this.CurrentSkillItem = this.NavigationManager.CurrentMenuItem.GetComponent<SkillItem>();
		if (this.CurrentSkillItem)
		{
			this.AbilityTitle.SetMessageProvider(this.CurrentSkillItem.NameMessageProvider);
			this.AbilityDescription.SetMessageProvider(this.CurrentSkillItem.DescriptionMessageProvider);
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

	// Token: 0x0600051F RID: 1311
	public string NameText(SkillItem skillItem)
	{
		if (skillItem.HasSkillItem)
		{
			return "$" + skillItem.NameMessageProvider + "$";
		}
		if (skillItem.CanEarnSkill)
		{
			return "#" + skillItem.NameMessageProvider + "#";
		}
		return "@" + skillItem.NameMessageProvider + "@";
	}

	// Token: 0x06000520 RID: 1312
	public string RequiredSoulsText(SkillItem skillItem)
	{
		if (skillItem.HasSkillItem)
		{
			return string.Empty;
		}
		MessageProvider messageProvider = (skillItem.ActualRequiredSkillPoints != 1) ? this.AbilityPointsMessageProvider : this.AbilityPointMessageProvider;
		if (skillItem.ActualRequiredSkillPoints <= Characters.Sein.Level.SkillPoints)
		{
			return "$" + messageProvider.ToString().Replace("[Amount]", skillItem.ActualRequiredSkillPoints.ToString()) + "$";
		}
		return "@" + messageProvider.ToString().Replace("[Amount]", skillItem.ActualRequiredSkillPoints.ToString()) + "@";
	}

	// Token: 0x06000521 RID: 1313
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

	// Token: 0x040004BE RID: 1214
	public static SkillTreeManager Instance;

	// Token: 0x040004BF RID: 1215
	public CleverMenuItemSelectionManager NavigationManager;

	// Token: 0x040004C0 RID: 1216
	public SkillItem CurrentSkillItem;

	// Token: 0x040004C1 RID: 1217
	public Transform Cursor;

	// Token: 0x040004C2 RID: 1218
	public SoundProvider OpenSound;

	// Token: 0x040004C3 RID: 1219
	public SoundProvider CloseSound;

	// Token: 0x040004C4 RID: 1220
	public GameObject LargeIcon;

	// Token: 0x040004C5 RID: 1221
	public Renderer LargeIconGlow;

	// Token: 0x040004C6 RID: 1222
	public MessageBox RequirementsLineA;

	// Token: 0x040004C7 RID: 1223
	public MessageBox AbilityTitle;

	// Token: 0x040004C8 RID: 1224
	public MessageBox AbilityDescription;

	// Token: 0x040004C9 RID: 1225
	public GameObject InfoPanel;

	// Token: 0x040004CA RID: 1226
	public MessageBox AbilityDiskInfoPanelDescription;

	// Token: 0x040004CB RID: 1227
	public GameObject AbilityDiskInfoPanel;

	// Token: 0x040004CC RID: 1228
	public SkillTreeLaneLogic EnergyLane;

	// Token: 0x040004CD RID: 1229
	public SkillTreeLaneLogic UtilityLane;

	// Token: 0x040004CE RID: 1230
	public SkillTreeLaneLogic CombatLane;

	// Token: 0x040004CF RID: 1231
	public GameObject GainSkillEffect;

	// Token: 0x040004D0 RID: 1232
	public LegacyAnimator RequirementsLineAShake;

	// Token: 0x040004D1 RID: 1233
	public ActionMethod OnGainAbility;

	// Token: 0x040004D2 RID: 1234
	public ActionMethod OnAlreadyEarnedAbility;

	// Token: 0x040004D3 RID: 1235
	public ActionMethod OnCantEarnSkill;

	// Token: 0x040004D4 RID: 1236
	public MessageProvider AbilityPointMessageProvider;

	// Token: 0x040004D5 RID: 1237
	public MessageProvider AbilityPointsMessageProvider;

	// Token: 0x040004D6 RID: 1238
	public MessageProvider RequiresMessageProvider;

	// Token: 0x040004D7 RID: 1239
	public MessageProvider AbilityMasteredMessageProvider;

	// Token: 0x040004D8 RID: 1240
	public AchievementAsset SpentFirstSkillPointAchievement;

	// Token: 0x040004D9 RID: 1241
	public List<SkillTreeManager.AbilityMessageProvider> AbilityMessages;

	// Token: 0x020000D3 RID: 211
	[Serializable]
	public class AbilityMessageProvider
	{
		// Token: 0x040004DA RID: 1242
		public AbilityType AbilityType;

		// Token: 0x040004DB RID: 1243
		public MessageProvider MessageProvider;
	}
}
