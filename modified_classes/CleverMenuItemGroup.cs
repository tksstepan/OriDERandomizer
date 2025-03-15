using System;
using System.Collections.Generic;
using Core;

public class CleverMenuItemGroup : CleverMenuItemGroupBase
{
	public override bool IsVisible
	{
		get
		{
			return this.SelectionManager.IsVisible;
		}
		set
		{
			if (this.SelectionManager.FadeAnimator && this.SelectionManager.FadeAnimator.FinalOpacity < 0.05f && !value)
			{
				this.SelectionManager.SetVisibleImmediate(false);
			}
			else
			{
				this.SelectionManager.SetVisible(value);
			}
		}
	}

	public override bool CanBeEntered
	{
		get
		{
			return !this.CanBeEnteredCondition || this.CanBeEnteredCondition.Validate(null);
		}
	}

	public override bool IsActive
	{
		get
		{
			return this.SelectionManager.IsActive;
		}
		set
		{
			this.SelectionManager.IsActive = value;
			this.UpdateHighlight();
			if (this.SuspendOnActivated)
			{
				if (value)
				{
					if (!this.m_isFrozen)
					{
						this.m_isFrozen = true;
						this.m_suspendablesIgnore.Clear();
						SuspensionManager.GetSuspendables(this.m_suspendablesIgnore, base.gameObject);
						SuspensionManager.SuspendExcluding(this.m_suspendablesIgnore);
					}
				}
				else if (this.m_isFrozen)
				{
					this.m_isFrozen = false;
					SuspensionManager.ResumeExcluding(this.m_suspendablesIgnore);
					this.m_suspendablesIgnore.Clear();
				}
			}
		}
	}

	public void OnDisable()
	{
		if (this.SuspendOnActivated && this.m_isFrozen)
		{
			this.m_isFrozen = false;
			SuspensionManager.ResumeExcluding(this.m_suspendablesIgnore);
			this.m_suspendablesIgnore.Clear();
		}
	}

	public override bool IsHighlightVisible
	{
		get
		{
			return this.SelectionManager.IsHighlightVisible;
		}
		set
		{
			this.SelectionManager.IsHighlightVisible = value;
		}
	}

	public void OnSelectionManagerBackPressed()
	{
		this.OnBackPressed();
	}

	public new void Awake()
	{
		base.Awake();
		CleverMenuItemSelectionManager selectionManager = this.SelectionManager;
		selectionManager.OptionChangeCallback = (Action)Delegate.Combine(selectionManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
		CleverMenuItemSelectionManager selectionManager2 = this.SelectionManager;
		selectionManager2.OptionPressedCallback = (Action)Delegate.Combine(selectionManager2.OptionPressedCallback, new Action(this.OnMenuItemPressed));
		CleverMenuItemSelectionManager selectionManager3 = this.SelectionManager;
		selectionManager3.OnBackPressedCallback = (Action)Delegate.Combine(selectionManager3.OnBackPressedCallback, new Action(this.OnSelectionManagerBackPressed));
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in this.Options)
		{
			cleverMenuItemGroupItem.ItemGroup.IsActive = false;
			CleverMenuItemGroupBase itemGroup = cleverMenuItemGroupItem.ItemGroup;
			itemGroup.OnBackPressed = (Action)Delegate.Combine(itemGroup.OnBackPressed, new Action(this.OnOptionBackPressed));
		}
	}

	public new void OnDestroy()
	{
		base.OnDestroy();
		CleverMenuItemSelectionManager selectionManager = this.SelectionManager;
		selectionManager.OptionChangeCallback = (Action)Delegate.Remove(selectionManager.OptionChangeCallback, new Action(this.OnMenuItemChange));
		CleverMenuItemSelectionManager selectionManager2 = this.SelectionManager;
		selectionManager2.OptionPressedCallback = (Action)Delegate.Remove(selectionManager2.OptionPressedCallback, new Action(this.OnMenuItemPressed));
		CleverMenuItemSelectionManager selectionManager3 = this.SelectionManager;
		selectionManager3.OnBackPressedCallback = (Action)Delegate.Remove(selectionManager3.OnBackPressedCallback, new Action(this.OnSelectionManagerBackPressed));
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in this.Options)
		{
			CleverMenuItemGroupBase itemGroup = cleverMenuItemGroupItem.ItemGroup;
			itemGroup.OnBackPressed = (Action)Delegate.Remove(itemGroup.OnBackPressed, new Action(this.OnOptionBackPressed));
		}
	}

	public void Start()
	{
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in this.Options)
		{
			bool isVisible = this.SelectionManager.CurrentMenuItem == cleverMenuItemGroupItem.MenuItem && this.ExpandOnHighlight;
			cleverMenuItemGroupItem.ItemGroup.IsVisible = isVisible;
		}
	}

	public void OnOptionBackPressed()
	{
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in this.Options)
		{
			if (!this.ExpandOnHighlight)
			{
				cleverMenuItemGroupItem.ItemGroup.IsVisible = false;
			}
			cleverMenuItemGroupItem.ItemGroup.IsActive = false;
			cleverMenuItemGroupItem.ItemGroup.IsHighlightVisible = false;
		}
		if (!this.IsActive && this.OnCollapseSound)
		{
			Sound.Play(this.OnCollapseSound.GetSound(null), base.transform.position, null);
		}
		this.IsActive = true;
	}

	public void OnMenuItemChange()
	{
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in this.Options)
		{
			if (this.SelectionManager.CurrentMenuItem == cleverMenuItemGroupItem.MenuItem && this.ExpandOnHighlight)
			{
				cleverMenuItemGroupItem.ItemGroup.IsVisible = true;
			}
			else
			{
				cleverMenuItemGroupItem.ItemGroup.IsVisible = false;
			}
		}
		if (this.OnChangeSelectionSound && this.IsActive && this.m_playChangeSound)
		{
			Sound.Play(this.OnChangeSelectionSound.GetSound(null), base.transform.position, null);
		}
		this.IsActive = true;
		this.Root.OnMenuItemChangedInGroup(this);
	}

	public override bool OnMenuItemChangedInGroup(CleverMenuItemGroup group)
	{
		bool flag = false;
		if (group == this)
		{
			flag = true;
		}
		else
		{
			this.IsActive = false;
		}
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in this.Options)
		{
			if (cleverMenuItemGroupItem.ItemGroup.OnMenuItemChangedInGroup(group))
			{
				flag = true;
			}
		}
		this.IsHighlightVisible = flag;
		return flag;
	}

	public void OnMenuItemPressed()
	{
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in this.Options)
		{
			cleverMenuItemGroupItem.ItemGroup.IsVisible = (this.SelectionManager.CurrentMenuItem == cleverMenuItemGroupItem.MenuItem);
		}
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem2 in this.Options)
		{
			if (this.SelectionManager.CurrentMenuItem == cleverMenuItemGroupItem2.MenuItem && cleverMenuItemGroupItem2.ItemGroup.CanBeEntered)
			{
				cleverMenuItemGroupItem2.ItemGroup.EnterInGroup();
				this.OnEnteredChildGroup();
				if (this.SelectionManager.CurrentMenuItem == cleverMenuItemGroupItem2.MenuItem && this.OnExpandSound)
				{
					Sound.Play(this.OnExpandSound.GetSound(null), base.transform.position, null);
				}
			}
		}
	}

	public void UpdateHighlight()
	{
		if (this.HighlightAnimator == null)
		{
			return;
		}
		if (this.IsActive)
		{
			this.HighlightAnimator.Initialize();
			this.HighlightAnimator.AnimatorDriver.ContinueForward();
		}
		else
		{
			this.HighlightAnimator.Initialize();
			this.HighlightAnimator.AnimatorDriver.ContinueBackwards();
		}
	}

	public void OnEnteredChildGroup()
	{
		this.IsActive = false;
	}

	public override void EnterInGroup()
	{
		this.m_playChangeSound = false;
		this.SelectionManager.SetIndexToFirst();
		this.m_playChangeSound = true;
		this.IsActive = true;
		this.IsHighlightVisible = true;
		if (!this.ExpandOnHighlight)
		{
			foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in this.Options)
			{
				cleverMenuItemGroupItem.ItemGroup.IsVisible = false;
			}
		}
	}

	public void AddItem(CleverMenuItem item, CleverMenuItemGroupBase itemGroup)
	{
		CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem = new CleverMenuItemGroup.CleverMenuItemGroupItem
		{
			ItemGroup = itemGroup,
			MenuItem = item
		};
		cleverMenuItemGroupItem.ItemGroup.IsActive = false;
		itemGroup.OnBackPressed = (Action)Delegate.Combine(itemGroup.OnBackPressed, new Action(this.OnOptionBackPressed));
		this.Options.Add(cleverMenuItemGroupItem);
	}

	public CleverMenuItemGroup Root;

	public List<CleverMenuItemGroup.CleverMenuItemGroupItem> Options;

	public CleverMenuItemSelectionManager SelectionManager;

	public SoundProvider OnExpandSound;

	public SoundProvider OnCollapseSound;

	public SoundProvider OnChangeSelectionSound;

	public bool ExpandOnHighlight;

	public Condition CanBeEnteredCondition;

	public TransparencyAnimator HighlightAnimator;

	public bool SuspendOnActivated;

	private bool m_playChangeSound = true;

	private bool m_isFrozen;

	private HashSet<ISuspendable> m_suspendablesIgnore = new HashSet<ISuspendable>();

	[Serializable]
	public class CleverMenuItemGroupItem
	{
		public CleverMenuItem MenuItem;

		public CleverMenuItemGroupBase ItemGroup;
	}
}
