using System;
using Core;
using Game;
using Sein.World;
using UnityEngine;

public class InventoryManager : MenuScreen
{
	public override void Show()
	{
		this.NavigationManager.SetVisible(true);
		this.NavigationManager.SetIndexToFirst();
	}

	public override void Hide()
	{
		this.NavigationManager.SetVisible(false);
	}

	public override void ShowImmediate()
	{
		this.NavigationManager.SetVisibleImmediate(true);
		this.NavigationManager.SetIndexToFirst();
	}

	public override void HideImmediate()
	{
		this.NavigationManager.SetVisibleImmediate(false);
	}

	public void Awake()
	{
		InventoryManager.Instance = this;
		
		CleverMenuItemSelectionManager navigationManager = this.NavigationManager;
		navigationManager.OptionChangeCallback = (Action)Delegate.Combine(navigationManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
		navigationManager.OptionPressedCallback = (Action)Delegate.Combine(navigationManager.OptionPressedCallback, new Action(this.OnMenuItemPressed));
		navigationManager.OnBackPressedCallback = (Action)Delegate.Combine(navigationManager.OnBackPressedCallback, new Action(this.OnBackPressed));
		
		DifficultyController instance = DifficultyController.Instance;
		instance.OnDifficultyChanged = (Action)Delegate.Combine(instance.OnDifficultyChanged, new Action(this.OnDifficultyChanged));

		if (this.Difficulty)
		{
			DifficultyModeMessageProvider difficultyProvider = (DifficultyModeMessageProvider)this.Difficulty.MessageProvider;
			difficultyProvider.Easy = RandomizerText.DifficultyOverrides.Easy.NameOverrideUpper;
			difficultyProvider.Normal = RandomizerText.DifficultyOverrides.Normal.NameOverrideUpper;
			difficultyProvider.Hard = RandomizerText.DifficultyOverrides.Hard.NameOverrideUpper;
			difficultyProvider.OneLife = RandomizerText.DifficultyOverrides.OneLife.NameOverrideUpper;

			ActionSequence difficultySequence = (ActionSequence)this.Difficulty.transform.parent.GetComponent<RunActionCondition>().Action;
			InstantiateAction difficultyAction = (InstantiateAction)difficultySequence.Actions[0];
			ChangeDifficultyScreen difficultyScreen = difficultyAction.Prefab.GetComponent<ChangeDifficultyScreen>();
			difficultyScreen.Easy = RandomizerText.DifficultyOverrides.Easy.NameOverride;
			difficultyScreen.Normal = RandomizerText.DifficultyOverrides.Normal.NameOverride;
			difficultyScreen.Hard = RandomizerText.DifficultyOverrides.Hard.NameOverride;
			difficultyScreen.OneLife = RandomizerText.DifficultyOverrides.OneLife.NameOverride;

			CleverMenuItemSelectionManager changeDifficultyManager = difficultyAction.Prefab.GetComponent<CleverMenuItemSelectionManager>();
			changeDifficultyManager.MenuItems[0].GetComponentInChildren<MessageBox>().SetMessageProvider(RandomizerText.DifficultyOverrides.Easy.NameOverrideUpper);
			changeDifficultyManager.MenuItems[1].GetComponentInChildren<MessageBox>().SetMessageProvider(RandomizerText.DifficultyOverrides.Normal.NameOverrideUpper);
			changeDifficultyManager.MenuItems[2].GetComponentInChildren<MessageBox>().SetMessageProvider(RandomizerText.DifficultyOverrides.Hard.NameOverrideUpper);
		}

		this.waterVeinClueText = UnityEngine.Object.Instantiate<MessageBox>(this.EnergyUpgradesText);
		this.waterVeinClueText.transform.position = this.GinsoTreeKey.transform.position + Vector3.down * 0.55f;
		this.waterVeinClueText.transform.SetParent(this.GinsoTreeKey.transform);
		this.gumonSealClueText = UnityEngine.Object.Instantiate<MessageBox>(this.EnergyUpgradesText);
		this.gumonSealClueText.transform.position = this.ForlornRuinsKey.transform.position + Vector3.down * 0.55f;
		this.gumonSealClueText.transform.SetParent(this.ForlornRuinsKey.transform);
		this.sunstoneClueText = UnityEngine.Object.Instantiate<MessageBox>(this.EnergyUpgradesText);
		this.sunstoneClueText.transform.position = this.MountHoruKey.transform.position + Vector3.down * 0.55f;
		this.sunstoneClueText.transform.SetParent(this.MountHoruKey.transform);
	}

	public void OnBackPressed()
	{
		UI.Menu.HideMenuScreen(false);
	}

	public void OnMenuItemChange()
	{
	}

	public void OnMenuItemPressed()
	{
		InventoryAbilityItem component = this.NavigationManager.CurrentMenuItem.GetComponent<InventoryAbilityItem>();
		if (component && !component.HasAbility)
		{
			if (this.PressUngainedAbilityOptionSound)
			{
				Sound.Play(this.PressUngainedAbilityOptionSound.GetSound(null), base.transform.position, null);
			}
			return;
		}
		InventoryItemHelpText component2 = this.NavigationManager.CurrentMenuItem.GetComponent<InventoryItemHelpText>();
		if (component2)
		{
			SuspensionManager.SuspendAll();
			MessageBox messageBox = UI.MessageController.ShowMessageBoxB(this.HelpMessageBox, component2.HelpMessage, Vector3.zero, float.PositiveInfinity);
			if (messageBox)
			{
				messageBox.SetAvatar(component2.Avatar);
				messageBox.OnMessageScreenHide += this.OnMessageScreenHide;
			}
			else
			{
				SuspensionManager.ResumeAll();
			}
			this.m_currentCloseMessageSound = ((!component) ? this.CloseStatisticsMessageSound : this.CloseAbilityMessageSound);
			if (component && this.PressAbilityOptionSound)
			{
				Sound.Play(this.PressAbilityOptionSound.GetSound(null), base.transform.position, null);
			}
		}
	}

	public void OnMessageScreenHide()
	{
		SuspensionManager.ResumeAll();
		if (this.m_currentCloseMessageSound && base.transform)
		{
			Sound.Play(this.m_currentCloseMessageSound.GetSound(null), base.transform.position, null);
		}
	}

	public void OnDestroy()
	{
		if (InventoryManager.Instance == this)
		{
			InventoryManager.Instance = null;
		}

		CleverMenuItemSelectionManager navigationManager = this.NavigationManager;
		navigationManager.OptionChangeCallback = (Action)Delegate.Remove(navigationManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
		navigationManager.OptionPressedCallback = (Action)Delegate.Remove(navigationManager.OptionPressedCallback, new Action(this.OnMenuItemPressed));
		navigationManager.OnBackPressedCallback = (Action)Delegate.Remove(navigationManager.OnBackPressedCallback, new Action(this.OnBackPressed));

		DifficultyController instance = DifficultyController.Instance;
		instance.OnDifficultyChanged = (Action)Delegate.Remove(instance.OnDifficultyChanged, new Action(this.OnDifficultyChanged));
	}

	public void OnDifficultyChanged()
	{
		if (this.Difficulty)
		{
			this.Difficulty.RefreshText();
		}
	}

	public void UpdateItems()
	{
		SeinCharacter sein = Characters.Sein;
		if (sein == null)
		{
			return;
		}
		this.CompletionText.SetMessage(new MessageDescriptor(GameWorld.Instance.CompletionPercentage + "%"));
		this.DeathText.SetMessage(new MessageDescriptor(SeinDeathCounter.Count.ToString()));
		this.HealthUpgradesText.SetMessage(new MessageDescriptor(sein.Mortality.Health.HealthUpgradesCollected + " / " + 12));
		this.EnergyUpgradesText.SetMessage(new MessageDescriptor(sein.Energy.EnergyUpgradesCollected + " / " + 15));
		this.SkillPointUniquesText.SetMessage(new MessageDescriptor((sein.Inventory.SkillPointsCollected & 63) + " / " + 33));
		this.waterVeinClueText.SetMessage(new MessageDescriptor(this.GetKeyLabel(Keys.GinsoTree, RandomizerBonus.WaterVeinShards(), 0)));
		this.gumonSealClueText.SetMessage(new MessageDescriptor(this.GetKeyLabel(Keys.ForlornRuins, RandomizerBonus.GumonSealShards(), 1)));
		this.sunstoneClueText.SetMessage(new MessageDescriptor(this.GetKeyLabel(Keys.MountHoru, RandomizerBonus.SunstoneShards(), 2)));
		GameTimer timer = GameController.Instance.Timer;
		this.TimeText.SetMessage(new MessageDescriptor(string.Format("{0:D2}:{1:D2}:{2:D2}", timer.Hours, timer.Minutes, timer.Seconds)));
		InventoryAbilityItem component = this.NavigationManager.CurrentMenuItem.GetComponent<InventoryAbilityItem>();
		if (component)
		{
			this.AbilityNameText.gameObject.SetActive(true);
			this.AbilityItemHighlight.SetActive(true);
			this.AbilityItemHighlight.transform.position = component.transform.position;
			if (component.HasAbility)
			{
				this.AbilityNameText.SetMessageProvider(component.AbilityName);
			}
			else
			{
				this.AbilityNameText.SetMessageProvider(this.LockedMessageProvider);
			}
		}
		else
		{
			this.AbilityNameText.gameObject.SetActive(false);
			this.AbilityItemHighlight.SetActive(false);
		}
		if (this.Difficulty)
		{
			this.Difficulty.RefreshText();
		}
	}

	public void FixedUpdate()
	{
		this.UpdateItems();
	}

	public void OnEnable()
	{
		this.UpdateItems();
	}

	public string GetKeyLabel(bool hasKey, int shards, int keyIndex)
	{
		if (hasKey)
		{
			return "";
		}
		if (Randomizer.Shards)
		{
			return string.Format("{0}/3", shards);
		}
		if (!Randomizer.CluesMode)
		{
			return "";
		}
		if (RandomizerBonus.SkillTreeProgression() >= RandomizerClues.RevealOrder[keyIndex] * 3)
		{
			return RandomizerClues.Clues[RandomizerClues.RevealOrder[keyIndex] - 1];
		}
		return "";
	}

	public const int TotalHealthUpgrades = 12;

	public const int TotalEnergyUpgrades = 15;

	public const int TotalSkillPoints = 33;

	public const int MaxLevel = 20;

	public static InventoryManager Instance;

	public CleverMenuItemSelectionManager NavigationManager;

	public SoundProvider OpenSound;

	public SoundProvider CloseSound;

	public SoundProvider PressAbilityOptionSound;

	public SoundProvider PressUngainedAbilityOptionSound;

	public SoundProvider CloseAbilityMessageSound;

	public SoundProvider CloseStatisticsMessageSound;

	private SoundProvider m_currentCloseMessageSound;

	public GameObject AbilityItemHighlight;

	public MessageBox AbilityNameText;

	public MessageBox TimeText;

	public MessageBox CompletionText;

	public MessageBox DeathText;

	public MessageBox HealthUpgradesText;

	public MessageBox EnergyUpgradesText;

	public MessageBox SkillPointUniquesText;

	public GameObject GinsoTreeKey;

	public GameObject ForlornRuinsKey;

	public GameObject MountHoruKey;

	public GameObject WorldEventsGroup;

	public MessageBox Difficulty;

	public MessageProvider LockedMessageProvider;

	public MessageProvider NotAvailableYetMessageProvider;

	public MessageProvider DiedZeroTimesMessageProvider;

	public MessageProvider DiedOneTimeMessagProvider;

	public MessageProvider DiedMultipleTimesMessageProvider;

	public GameObject HelpMessageBox;

	private MessageBox gumonSealClueText;

	private MessageBox waterVeinClueText;

	private MessageBox sunstoneClueText;
}
