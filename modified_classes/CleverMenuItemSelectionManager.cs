using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

public class CleverMenuItemSelectionManager : MonoBehaviour, ISuspendable
{
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

	public bool IsVisible
	{
		get
		{
			return this.m_isVisible;
		}
	}

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

	public void RefreshVisible()
	{
		foreach (CleverMenuItem cleverMenuItem in this.MenuItems)
		{
			cleverMenuItem.RefreshVisible();
		}
	}

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

	public void OnDisable()
	{
		this.m_isVisible = false;
	}

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

	public bool IsLocked { get; set; }

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

	public void Awake()
	{
		SuspensionManager.Register(this);
	}

	public void OnDestroy()
	{
		SuspensionManager.Unregister(this);
	}

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

	public void SetCurrentMenuItem(CleverMenuItem menuItem)
	{
		int currentItem = this.MenuItems.FindIndex((CleverMenuItem a) => a == menuItem);
		this.SetCurrentItem(currentItem);
	}

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
				return;
			}
		}
	}

	public void Start()
	{
		this.m_holdRemainingTime = 0.4f;
		this.m_delayNavigation = (Core.Input.MenuDown.IsPressed || Core.Input.MenuUp.IsPressed);
		if (this.IsHighlightVisible && this.CurrentMenuItem)
		{
			this.CurrentMenuItem.OnHighlight();
		}
		if (base.name == "inventoryScreen")
		{
			this.m_isPauseScreen = true;
			CleverMenuItem cleverMenuItem = this.MenuItems[0];
			CleverMenuItem cleverMenuItem2 = this.MenuItems[9];
			this.Navigation.Add(new CleverMenuItemSelectionManager.NavigationData
			{
				From = cleverMenuItem,
				To = cleverMenuItem2
			});
			this.Navigation.Add(new CleverMenuItemSelectionManager.NavigationData
			{
				From = cleverMenuItem2,
				To = cleverMenuItem
			});
		}
	}

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
			if (this.m_delayNavigation)
			{
				if (Core.Input.MenuDown.IsPressed || Core.Input.MenuUp.IsPressed)
				{
					break;
				}
				this.m_delayNavigation = false;
			}
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
				Vector2 a = navigationData.To.Transform.position;
				if (this.m_isPauseScreen)
				{
					if (cleverMenuItem == this.MenuItems[0] && navigationData.To == this.MenuItems[9])
					{
						a = new Vector2(0f, 2f);
					}
					else if (cleverMenuItem == this.MenuItems[9] && navigationData.To == this.MenuItems[0])
					{
						a = new Vector2(0f, -2f);
					}
				}
				Vector2 normalized2 = (a - b).normalized;
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

	public void PressCurrentItem()
	{
		this.OptionPressedCallback();
		if (this.CurrentMenuItem)
		{
			this.CurrentMenuItem.OnPressed();
		}
	}

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

	public bool IsSuspended { get; set; }

	public void AddMenuItem(string label, Action onPress)
	{
		this.AddMenuItem(label, this.MenuItems.Count - 1, onPress);
	}

	public void AddMenuItem(string label, int index, Action onPress)
	{
		CleverMenuItemLayout component = base.gameObject.GetComponent<CleverMenuItemLayout>();
		if (component != null)
		{
			this.AddMenuItem(label, index, component, onPress);
			return;
		}
	}

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

	public const float HOLD_DELAY = 0.4f;

	public const float HOLD_FAST_DELAY = 0.04f;

	public List<CleverMenuItemSelectionManager.NavigationData> Navigation = new List<CleverMenuItemSelectionManager.NavigationData>();

	public CageStructureTool CopyFromCage;

	public List<CleverMenuItem> MenuItems;

	public CleverMenuItemSelectionManager.Direction ItemDirection;

	public ActionMethod OptionChangeAction;

	public Action OptionChangeCallback = delegate
	{
	};

	public Action OptionPressedCallback = delegate
	{
	};

	public Action OnBackPressedCallback = delegate
	{
	};

	public bool HighlightOnMouseOver = true;

	public bool UnhighlightOnMouseLeave;

	public TransparencyAnimator FadeAnimator;

	public int Index;

	private int m_defaultIndex;

	public CleverMenuItem BackItem;

	public ActionMethod BackAction;

	public float ButtonPressDelay = 0.2f;

	public float AngleTolerance = 60f;

	private bool m_isVisible = true;

	private bool m_isActive = true;

	private float m_buttonPressDelay;

	private float m_nextPressDelay;

	private float m_holdDelayDuration;

	private float m_holdRemainingTime;

	private bool m_isHighlightVisible = true;

	private bool m_delayNavigation;

	private bool m_isPauseScreen;

	[Serializable]
	public class NavigationData
	{
		public CleverMenuItem From;

		public CleverMenuItem To;

		public Func<CleverMenuItemSelectionManager.NavigationData, bool> Condition;
	}

	public enum FocusState
	{
		None,
		InFocus,
		ChildInFocus
	}

	public enum Direction
	{
		LeftToRight,
		TopToBottom,
		NavigationCage
	}
}
