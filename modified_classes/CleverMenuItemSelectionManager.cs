using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

// Token: 0x02000383 RID: 899
public class CleverMenuItemSelectionManager : MonoBehaviour, ISuspendable
{
	// Token: 0x060013FC RID: 5116 RVA: 0x0006F784 File Offset: 0x0006D984
	public void SetVisible(bool visible)
	{
		if (visible)
		{
			base.gameObject.SetActive(true);
			this.m_isVisible = true;
			if (this.FadeAnimator)
			{
				this.FadeAnimator.Initialize();
				this.FadeAnimator.AnimatorDriver.ContinueForward();
				return;
			}
		}
		else
		{
			this.m_isVisible = false;
			if (this.FadeAnimator)
			{
				this.FadeAnimator.Initialize();
				this.FadeAnimator.AnimatorDriver.ContinueBackwards();
				return;
			}
			base.gameObject.SetActive(false);
		}
	}

	// Token: 0x060013FD RID: 5117 RVA: 0x0006F80C File Offset: 0x0006DA0C
	public void SetVisibleImmediate(bool visible)
	{
		if (visible)
		{
			base.gameObject.SetActive(true);
			this.m_isVisible = true;
			if (this.FadeAnimator)
			{
				this.FadeAnimator.Initialize();
				this.FadeAnimator.AnimatorDriver.GoToEnd();
				this.FadeAnimator.AnimatorDriver.Pause();
				return;
			}
		}
		else
		{
			this.m_isVisible = false;
			if (this.FadeAnimator)
			{
				this.FadeAnimator.Initialize();
				this.FadeAnimator.AnimatorDriver.GoToStart();
				this.FadeAnimator.AnimatorDriver.Pause();
			}
			base.gameObject.SetActive(false);
		}
	}

	// Token: 0x1700039B RID: 923
	// (get) Token: 0x060013FE RID: 5118 RVA: 0x000114E3 File Offset: 0x0000F6E3
	public bool IsVisible
	{
		get
		{
			return this.m_isVisible;
		}
	}

	// Token: 0x1700039C RID: 924
	// (get) Token: 0x060013FF RID: 5119 RVA: 0x000114EB File Offset: 0x0000F6EB
	// (set) Token: 0x06001400 RID: 5120 RVA: 0x0006F8B4 File Offset: 0x0006DAB4
	public bool IsHighlightVisible
	{
		get
		{
			return this.m_isHighlightVisible;
		}
		set
		{
			this.m_isHighlightVisible = value;
			if (this.m_isHighlightVisible)
			{
				if (this.CurrentMenuItem)
				{
					this.CurrentMenuItem.OnHighlight();
					return;
				}
			}
			else if (this.CurrentMenuItem)
			{
				this.CurrentMenuItem.OnUnhighlight();
			}
		}
	}

	// Token: 0x06001401 RID: 5121 RVA: 0x0006F904 File Offset: 0x0006DB04
	public void RefreshVisible()
	{
		foreach (CleverMenuItem cleverMenuItem in this.MenuItems)
		{
			cleverMenuItem.RefreshVisible();
		}
	}

	// Token: 0x06001402 RID: 5122 RVA: 0x000114F3 File Offset: 0x0000F6F3
	public void OnEnable()
	{
		this.m_isVisible = true;
		if (this.FadeAnimator)
		{
			this.FadeAnimator.Initialize();
			this.FadeAnimator.AnimatorDriver.ContinueForward();
		}
		this.RefreshVisible();
	}

	// Token: 0x06001403 RID: 5123 RVA: 0x0001152A File Offset: 0x0000F72A
	public void OnDisable()
	{
		this.m_isVisible = false;
	}

	// Token: 0x1700039D RID: 925
	// (get) Token: 0x06001404 RID: 5124 RVA: 0x00011533 File Offset: 0x0000F733
	// (set) Token: 0x06001405 RID: 5125 RVA: 0x0001153B File Offset: 0x0000F73B
	public bool IsActive
	{
		get
		{
			return this.m_isActive;
		}
		set
		{
			this.m_isActive = value;
		}
	}

	// Token: 0x1700039E RID: 926
	// (get) Token: 0x06001406 RID: 5126 RVA: 0x00011544 File Offset: 0x0000F744
	// (set) Token: 0x06001407 RID: 5127 RVA: 0x0001154C File Offset: 0x0000F74C
	public bool IsLocked { get; set; }

	// Token: 0x1700039F RID: 927
	// (get) Token: 0x06001408 RID: 5128 RVA: 0x00011555 File Offset: 0x0000F755
	public CleverMenuItem CurrentMenuItem
	{
		get
		{
			if (this.Index < 0 || this.Index >= this.MenuItems.Count)
			{
				return null;
			}
			return this.MenuItems[this.Index];
		}
	}

	// Token: 0x06001409 RID: 5129 RVA: 0x00002AB0 File Offset: 0x00000CB0
	public void Awake()
	{
		SuspensionManager.Register(this);
	}

	// Token: 0x0600140A RID: 5130 RVA: 0x000023E2 File Offset: 0x000005E2
	public void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

	// Token: 0x0600140B RID: 5131 RVA: 0x0006F954 File Offset: 0x0006DB54
	public void MoveSelection(bool forward)
	{
		int num = this.Index;
		int num2 = 0;
		if (forward)
		{
			do
			{
				num = (num + 1) % this.MenuItems.Count;
				if (num2++ > this.MenuItems.Count)
				{
					goto IL_43;
				}
			}
			while (!this.MenuItems[num].IsActivated);
			goto IL_93;
			IL_43:
			num = this.Index;
		}
		else
		{
			do
			{
				num = ((num - 1 >= 0) ? (num - 1) : (this.MenuItems.Count - 1));
				if (num2++ > this.MenuItems.Count)
				{
					goto IL_8C;
				}
			}
			while (!this.MenuItems[num].IsActivated);
			goto IL_93;
			IL_8C:
			num = this.Index;
		}
		IL_93:
		if (num == this.Index)
		{
			return;
		}
		if (this.MenuItems[num].IsActivated)
		{
			this.SetCurrentItem(num);
		}
	}

	// Token: 0x0600140C RID: 5132 RVA: 0x0006FA18 File Offset: 0x0006DC18
	public void SetCurrentMenuItem(CleverMenuItem menuItem)
	{
		int currentItem = this.MenuItems.FindIndex((CleverMenuItem a) => a == menuItem);
		this.SetCurrentItem(currentItem);
	}

	// Token: 0x0600140D RID: 5133 RVA: 0x0006FA54 File Offset: 0x0006DC54
	public void SetCurrentItem(int index)
	{
		if (this.CurrentMenuItem)
		{
			this.CurrentMenuItem.OnUnhighlight();
		}
		this.Index = index;
		
		if (this.CurrentMenuItem)
		{
			this.CurrentMenuItem.OnHighlight();
			this.OptionChangeCallback();
			if (this.OptionChangeAction)
			{
				this.OptionChangeAction.Perform(null);
			}
		}
	}

	// Token: 0x0600140E RID: 5134 RVA: 0x00011586 File Offset: 0x0000F786
	public void Start()
	{
		if (this.IsHighlightVisible && this.CurrentMenuItem)
		{
			this.CurrentMenuItem.OnHighlight();
		}
	}

	// Token: 0x0600140F RID: 5135 RVA: 0x0006FAF0 File Offset: 0x0006DCF0
	public void SetIndexToFirst()
	{
		for (int i = 0; i < this.MenuItems.Count; i++)
		{
			if (this.MenuItems[i].IsActivated)
			{
				this.SetCurrentItem(i);
				return;
			}
		}
	}

	// Token: 0x06001410 RID: 5136 RVA: 0x0006FB30 File Offset: 0x0006DD30
	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		if (!GameController.IsFocused)
		{
			return;
		}
		if (!this.IsVisible)
		{
			if (this.FadeAnimator && this.FadeAnimator.AnimatorDriver.IsReversed && !this.FadeAnimator.AnimatorDriver.IsPlaying)
			{
				base.gameObject.SetActive(false);
			}
			return;
		}
		if (this.CurrentMenuItem && this.CurrentMenuItem.IsPerforming())
		{
			return;
		}
		if (this.IsLocked)
		{
			return;
		}
		if (Core.Input.LeftClick.OnPressed)
		{
			CleverMenuItem cleverMenuItemUnderCursor = this.CleverMenuItemUnderCursor;
			if (cleverMenuItemUnderCursor)
			{
				this.SetCurrentMenuItem(cleverMenuItemUnderCursor);
				this.PressCurrentItem();
				return;
			}
		}
		if (Core.Input.CursorMoved && this.HighlightOnMouseOver)
		{
			CleverMenuItem cleverMenuItemUnderCursor2 = this.CleverMenuItemUnderCursor;
			if (cleverMenuItemUnderCursor2 && cleverMenuItemUnderCursor2 != this.CurrentMenuItem)
			{
				this.SetCurrentMenuItem(cleverMenuItemUnderCursor2);
			}
			if (this.UnhighlightOnMouseLeave && cleverMenuItemUnderCursor2 == null && this.CurrentMenuItem.IsHighlighted)
			{
				this.CurrentMenuItem.OnUnhighlight();
			}
			if (this.HighlightOnMouseOver && cleverMenuItemUnderCursor2 != null && !cleverMenuItemUnderCursor2.IsHighlighted)
			{
				this.CurrentMenuItem.OnHighlight();
			}
		}
		if (!this.IsActive)
		{
			return;
		}
		switch (this.ItemDirection)
		{
		case CleverMenuItemSelectionManager.Direction.LeftToRight:
			if (Core.Input.MenuLeft.OnPressed)
			{
				this.MoveSelection(false);
				this.m_holdRemainingTime = 0.4f;
			}
			if (Core.Input.MenuRight.OnPressed)
			{
				this.MoveSelection(true);
				this.m_holdRemainingTime = 0.4f;
			}
			if (Core.Input.MenuLeft.Pressed || Core.Input.MenuRight.Pressed)
			{
				this.m_holdRemainingTime -= Time.deltaTime;
				if (this.m_holdRemainingTime < 0f)
				{
					if (Core.Input.MenuLeft.Pressed)
					{
						this.MoveSelection(false);
					}
					if (Core.Input.MenuRight.Pressed)
					{
						this.MoveSelection(true);
					}
				}
			}
			break;
		case CleverMenuItemSelectionManager.Direction.TopToBottom:
			if (Core.Input.MenuUp.OnPressed)
			{
				this.MoveSelection(false);
				this.m_holdRemainingTime = 0.4f;
			}
			if (Core.Input.MenuDown.OnPressed)
			{
				this.MoveSelection(true);
				this.m_holdRemainingTime = 0.4f;
			}
			if (Core.Input.MenuUp.Pressed || Core.Input.MenuDown.Pressed)
			{
				this.m_holdRemainingTime -= Time.deltaTime;
				if (this.m_holdRemainingTime < 0f)
				{
					if (Core.Input.MenuUp.Pressed)
					{
						this.m_holdRemainingTime = 0.04f;
						this.MoveSelection(false);
					}
					if (Core.Input.MenuDown.Pressed)
					{
						this.m_holdRemainingTime = 0.04f;
						this.MoveSelection(true);
					}
				}
			}
			break;
		case CleverMenuItemSelectionManager.Direction.NavigationCage:
			this.HandleNavigationCage();
			break;
		}
		if (Core.Input.ActionButtonA.OnPressed && !Core.Input.ActionButtonA.Used)
		{
			if (this.m_buttonPressDelay <= 0f)
			{
				this.m_buttonPressDelay = this.ButtonPressDelay;
				Core.Input.ActionButtonA.Used = true;
				Core.Input.Jump.Used = true;
				this.PressCurrentItem();
			}
			return;
		}
		this.m_buttonPressDelay = Mathf.Max(0f, this.m_buttonPressDelay - Time.deltaTime);
		if (Core.Input.Cancel.OnPressed && !Core.Input.Cancel.Used)
		{
			Core.Input.Cancel.Used = true;
			Core.Input.SoulFlame.Used = true;
			this.OnBackPressed();
		}
	}

	// Token: 0x06001411 RID: 5137 RVA: 0x0006FE84 File Offset: 0x0006E084
	public void OnDrawGizmosSelected()
	{
		if (this.ItemDirection == CleverMenuItemSelectionManager.Direction.NavigationCage)
		{
			Gizmos.color = Color.yellow;
			foreach (CleverMenuItemSelectionManager.NavigationData navigationData in this.Navigation)
			{
				if (navigationData.From && navigationData.To)
				{
					Gizmos.DrawLine(navigationData.From.transform.position, navigationData.To.transform.position);
				}
			}
			Gizmos.color = Color.white;
		}
	}

	// Token: 0x06001412 RID: 5138 RVA: 0x0006FF30 File Offset: 0x0006E130
	public void HandleNavigationCage()
	{
		if (Core.Input.Axis.magnitude > 0.5f)
		{
			if (this.m_nextPressDelay == 0f)
			{
				if (this.ChangeMenuItem())
				{
					this.m_nextPressDelay = 0.4f;
					return;
				}
				this.m_nextPressDelay = 0f;
				return;
			}
			else if (this.m_nextPressDelay > 0f)
			{
				this.m_nextPressDelay -= Time.deltaTime;
				if (this.m_nextPressDelay < 0f)
				{
					this.m_nextPressDelay = 0f;
					return;
				}
			}
		}
		else
		{
			this.m_nextPressDelay = 0f;
		}
	}

	// Token: 0x06001413 RID: 5139 RVA: 0x0006FFC4 File Offset: 0x0006E1C4
	public bool ChangeMenuItem()
	{
		Vector2 normalized = Core.Input.Axis.normalized;
		if (!this.CurrentMenuItem)
		{
			return false;
		}
		Vector2 b = this.CurrentMenuItem.Transform.position;
		CleverMenuItem cleverMenuItem = this.CurrentMenuItem;
		float num = Mathf.Cos(this.AngleTolerance * 0.0174532924f);
		foreach (CleverMenuItemSelectionManager.NavigationData navigationData in this.Navigation)
		{
			if ((navigationData.Condition == null || navigationData.Condition(navigationData)) && navigationData.From == this.CurrentMenuItem && navigationData.To.IsVisible)
			{
				Vector2 normalized2 = (navigationData.To.Transform.position - b).normalized;
				float num2 = Vector2.Dot(normalized, normalized2);
				if (num2 > num)
				{
					num = num2;
					cleverMenuItem = navigationData.To;
				}
			}
		}
		if (cleverMenuItem != this.CurrentMenuItem)
		{
			this.SetCurrentMenuItem(cleverMenuItem);
			return true;
		}
		return false;
	}

	// Token: 0x06001414 RID: 5140 RVA: 0x000115A8 File Offset: 0x0000F7A8
	public void PressCurrentItem()
	{
		this.OptionPressedCallback();
		if (this.CurrentMenuItem)
		{
			this.CurrentMenuItem.OnPressed();
		}
	}

	// Token: 0x06001415 RID: 5141 RVA: 0x000115CD File Offset: 0x0000F7CD
	public void OnBackPressed()
	{
		this.OnBackPressedCallback();
		if (this.BackItem)
		{
			this.BackItem.OnPressed();
		}
		if (this.BackAction)
		{
			this.BackAction.Perform(null);
		}
	}

	// Token: 0x170003A0 RID: 928
	// (get) Token: 0x06001416 RID: 5142 RVA: 0x000700FC File Offset: 0x0006E2FC
	public CleverMenuItem CleverMenuItemUnderCursor
	{
		get
		{
			Vector2 cursorPositionUI = Core.Input.CursorPositionUI;
			float num = float.PositiveInfinity;
			CleverMenuItem result = null;
			foreach (CleverMenuItem cleverMenuItem in this.MenuItems)
			{
				if (cleverMenuItem.IsVisible && cleverMenuItem.Bounds.Contains(cursorPositionUI))
				{
					float num2 = Vector3.Distance(cleverMenuItem.Bounds.center, cursorPositionUI);
					if (num > num2)
					{
						num = num2;
						result = cleverMenuItem;
					}
				}
			}
			return result;
		}
	}

	// Token: 0x06001417 RID: 5143 RVA: 0x000701A4 File Offset: 0x0006E3A4
	[ContextMenu("Create navigation from cage")]
	public void CreateNavigationStructureFromCageTool()
	{
		List<CleverMenuItem> list = UnityEngine.Object.FindObjectsOfType(typeof(CleverMenuItem)).Cast<CleverMenuItem>().ToList<CleverMenuItem>();
		Dictionary<CageStructureTool.Vertex, CleverMenuItem> dictionary = new Dictionary<CageStructureTool.Vertex, CleverMenuItem>();
		foreach (CageStructureTool.Vertex vertex in this.CopyFromCage.Vertices)
		{
			Vector3 a = this.CopyFromCage.transform.TransformPoint(vertex.Position);
			float num = float.MaxValue;
			CleverMenuItem value = null;
			foreach (CleverMenuItem cleverMenuItem in list)
			{
				float num2 = Vector3.Distance(a, cleverMenuItem.transform.position);
				if (num2 < num)
				{
					value = cleverMenuItem;
					num = num2;
				}
			}
			dictionary[vertex] = value;
		}
		this.Navigation.Clear();
		foreach (CageStructureTool.Edge edge in this.CopyFromCage.Edges)
		{
			CageStructureTool.Vertex key = this.CopyFromCage.VertexByIndex(edge.VertexA);
			CageStructureTool.Vertex key2 = this.CopyFromCage.VertexByIndex(edge.VertexB);
			this.Navigation.Add(new CleverMenuItemSelectionManager.NavigationData
			{
				From = dictionary[key],
				To = dictionary[key2]
			});
			this.Navigation.Add(new CleverMenuItemSelectionManager.NavigationData
			{
				From = dictionary[key2],
				To = dictionary[key]
			});
		}
		this.MenuItems.Clear();
		foreach (CleverMenuItemSelectionManager.NavigationData navigationData in this.Navigation)
		{
			if (!this.MenuItems.Contains(navigationData.From))
			{
				this.MenuItems.Add(navigationData.From);
			}
		}
	}

	// Token: 0x170003A1 RID: 929
	// (get) Token: 0x06001418 RID: 5144 RVA: 0x0001160B File Offset: 0x0000F80B
	// (set) Token: 0x06001419 RID: 5145 RVA: 0x00011613 File Offset: 0x0000F813
	public bool IsSuspended { get; set; }

	// Token: 0x0600141A RID: 5146 RVA: 0x0001161C File Offset: 0x0000F81C
	public void AddMenuItem(string label, Action onPress)
	{
		this.AddMenuItem(label, this.MenuItems.Count - 1, onPress);
	}

	// Token: 0x0600141B RID: 5147 RVA: 0x000703E4 File Offset: 0x0006E5E4
	public void AddMenuItem(string label, int index, Action onPress)
	{
		CleverMenuItemLayout component = base.gameObject.GetComponent<CleverMenuItemLayout>();
		if (component != null)
		{
			this.AddMenuItem(label, index, component, onPress);
			return;
		}
	}

	// Token: 0x0600141C RID: 5148 RVA: 0x00070414 File Offset: 0x0006E614
	public void AddMenuItem(string label, int index, CleverMenuItemLayout layout, Action onPress)
	{
		CleverMenuItem cleverMenuItem = UnityEngine.Object.Instantiate<CleverMenuItem>(this.MenuItems[0]);
		cleverMenuItem.gameObject.name = label;
		cleverMenuItem.transform.SetParent(this.MenuItems[1].transform.parent);
		cleverMenuItem.PressedCallback += onPress;
		cleverMenuItem.gameObject.GetComponentInChildren<MessageBox>().SetMessage(new MessageDescriptor(label));
		cleverMenuItem.ApplyColors();
		this.MenuItems.Insert(index, cleverMenuItem);
		layout.AddItem(cleverMenuItem, index);
	}

	// Token: 0x040012BB RID: 4795
	public const float HOLD_DELAY = 0.4f;

	// Token: 0x040012BC RID: 4796
	public const float HOLD_FAST_DELAY = 0.04f;

	// Token: 0x040012BD RID: 4797
	public List<CleverMenuItemSelectionManager.NavigationData> Navigation = new List<CleverMenuItemSelectionManager.NavigationData>();

	// Token: 0x040012BE RID: 4798
	public CageStructureTool CopyFromCage;

	// Token: 0x040012BF RID: 4799
	public List<CleverMenuItem> MenuItems;

	// Token: 0x040012C0 RID: 4800
	public CleverMenuItemSelectionManager.Direction ItemDirection;

	// Token: 0x040012C1 RID: 4801
	public ActionMethod OptionChangeAction;

	// Token: 0x040012C2 RID: 4802
	public Action OptionChangeCallback = delegate
	{
	};

	// Token: 0x040012C3 RID: 4803
	public Action OptionPressedCallback = delegate
	{
	};

	// Token: 0x040012C4 RID: 4804
	public Action OnBackPressedCallback = delegate
	{
	};

	// Token: 0x040012C5 RID: 4805
	public bool HighlightOnMouseOver = true;

	// Token: 0x040012C6 RID: 4806
	public bool UnhighlightOnMouseLeave;

	// Token: 0x040012C7 RID: 4807
	public TransparencyAnimator FadeAnimator;

	// Token: 0x040012C8 RID: 4808
	public int Index;

	// Token: 0x040012C9 RID: 4809
	private int m_defaultIndex;

	// Token: 0x040012CA RID: 4810
	public CleverMenuItem BackItem;

	// Token: 0x040012CB RID: 4811
	public ActionMethod BackAction;

	// Token: 0x040012CC RID: 4812
	public float ButtonPressDelay = 0.2f;

	// Token: 0x040012CD RID: 4813
	public float AngleTolerance = 60f;

	// Token: 0x040012CE RID: 4814
	private bool m_isVisible = true;

	// Token: 0x040012CF RID: 4815
	private bool m_isActive = true;

	// Token: 0x040012D0 RID: 4816
	private float m_buttonPressDelay;

	// Token: 0x040012D1 RID: 4817
	private float m_nextPressDelay;

	// Token: 0x040012D2 RID: 4818
	private float m_holdDelayDuration;

	// Token: 0x040012D3 RID: 4819
	private float m_holdRemainingTime;

	// Token: 0x040012D4 RID: 4820
	private bool m_isHighlightVisible = true;

	// Token: 0x02000384 RID: 900
	[Serializable]
	public class NavigationData
	{
		// Token: 0x040012D7 RID: 4823
		public CleverMenuItem From;

		// Token: 0x040012D8 RID: 4824
		public CleverMenuItem To;

		// Token: 0x040012D9 RID: 4825
		public Func<CleverMenuItemSelectionManager.NavigationData, bool> Condition;
	}

	// Token: 0x02000385 RID: 901
	public enum FocusState
	{
		// Token: 0x040012DB RID: 4827
		None,
		// Token: 0x040012DC RID: 4828
		InFocus,
		// Token: 0x040012DD RID: 4829
		ChildInFocus
	}

	// Token: 0x02000386 RID: 902
	public enum Direction
	{
		// Token: 0x040012DF RID: 4831
		LeftToRight,
		// Token: 0x040012E0 RID: 4832
		TopToBottom,
		// Token: 0x040012E1 RID: 4833
		NavigationCage
	}
}
