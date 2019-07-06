using System;
using System.Collections.Generic;
using Core;

// Token: 0x02000064 RID: 100
public class CleverMenuItemGroup : CleverMenuItemGroupBase
{
	// Token: 0x17000076 RID: 118
	// (get) Token: 0x06000261 RID: 609 RVA: 0x00003FCA File Offset: 0x000021CA
	// (set) Token: 0x06000262 RID: 610 RVA: 0x00035B3C File Offset: 0x00033D3C
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

	// Token: 0x17000077 RID: 119
	// (get) Token: 0x06000263 RID: 611 RVA: 0x00003FD7 File Offset: 0x000021D7
	public override bool CanBeEntered
	{
		get
		{
			return !this.CanBeEnteredCondition || this.CanBeEnteredCondition.Validate(null);
		}
	}

	// Token: 0x17000078 RID: 120
	// (get) Token: 0x06000264 RID: 612 RVA: 0x00003FF7 File Offset: 0x000021F7
	// (set) Token: 0x06000265 RID: 613 RVA: 0x00035B9C File Offset: 0x00033D9C
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

	// Token: 0x06000266 RID: 614 RVA: 0x00004004 File Offset: 0x00002204
	public void OnDisable()
	{
		if (this.SuspendOnActivated && this.m_isFrozen)
		{
			this.m_isFrozen = false;
			SuspensionManager.ResumeExcluding(this.m_suspendablesIgnore);
			this.m_suspendablesIgnore.Clear();
		}
	}

	// Token: 0x17000079 RID: 121
	// (get) Token: 0x06000267 RID: 615 RVA: 0x00004039 File Offset: 0x00002239
	// (set) Token: 0x06000268 RID: 616 RVA: 0x00004046 File Offset: 0x00002246
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

	// Token: 0x06000269 RID: 617 RVA: 0x00004054 File Offset: 0x00002254
	public void OnSelectionManagerBackPressed()
	{
		this.OnBackPressed();
	}

	// Token: 0x0600026A RID: 618 RVA: 0x00035C34 File Offset: 0x00033E34
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

	// Token: 0x0600026B RID: 619 RVA: 0x00035D2C File Offset: 0x00033F2C
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

	// Token: 0x0600026C RID: 620 RVA: 0x00035E24 File Offset: 0x00034024
	public void Start()
	{
		foreach (CleverMenuItemGroup.CleverMenuItemGroupItem cleverMenuItemGroupItem in this.Options)
		{
			bool isVisible = this.SelectionManager.CurrentMenuItem == cleverMenuItemGroupItem.MenuItem && this.ExpandOnHighlight;
			cleverMenuItemGroupItem.ItemGroup.IsVisible = isVisible;
		}
	}

	// Token: 0x0600026D RID: 621 RVA: 0x00035EA8 File Offset: 0x000340A8
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

	// Token: 0x0600026E RID: 622 RVA: 0x00035F6C File Offset: 0x0003416C
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

	// Token: 0x0600026F RID: 623 RVA: 0x00036044 File Offset: 0x00034244
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

	// Token: 0x06000270 RID: 624 RVA: 0x000360D0 File Offset: 0x000342D0
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

	// Token: 0x06000271 RID: 625 RVA: 0x0003620C File Offset: 0x0003440C
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

	// Token: 0x06000272 RID: 626 RVA: 0x00004061 File Offset: 0x00002261
	public void OnEnteredChildGroup()
	{
		this.IsActive = false;
	}

	// Token: 0x06000273 RID: 627 RVA: 0x00036274 File Offset: 0x00034474
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

	// Token: 0x06000275 RID: 629 RVA: 0x00036308 File Offset: 0x00034508
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

	// Token: 0x040002C1 RID: 705
	public CleverMenuItemGroup Root;

	// Token: 0x040002C2 RID: 706
	public List<CleverMenuItemGroup.CleverMenuItemGroupItem> Options;

	// Token: 0x040002C3 RID: 707
	public CleverMenuItemSelectionManager SelectionManager;

	// Token: 0x040002C4 RID: 708
	public SoundProvider OnExpandSound;

	// Token: 0x040002C5 RID: 709
	public SoundProvider OnCollapseSound;

	// Token: 0x040002C6 RID: 710
	public SoundProvider OnChangeSelectionSound;

	// Token: 0x040002C7 RID: 711
	public bool ExpandOnHighlight;

	// Token: 0x040002C8 RID: 712
	public Condition CanBeEnteredCondition;

	// Token: 0x040002C9 RID: 713
	public TransparencyAnimator HighlightAnimator;

	// Token: 0x040002CA RID: 714
	public bool SuspendOnActivated;

	// Token: 0x040002CB RID: 715
	private bool m_playChangeSound = true;

	// Token: 0x040002CC RID: 716
	private bool m_isFrozen;

	// Token: 0x040002CD RID: 717
	private HashSet<ISuspendable> m_suspendablesIgnore = new HashSet<ISuspendable>();

	// Token: 0x02000065 RID: 101
	[Serializable]
	public class CleverMenuItemGroupItem
	{
		// Token: 0x040002CE RID: 718
		public CleverMenuItem MenuItem;

		// Token: 0x040002CF RID: 719
		public CleverMenuItemGroupBase ItemGroup;
	}
}
