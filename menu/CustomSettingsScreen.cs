using System;
using UnityEngine;

public abstract class CustomSettingsScreen : MonoBehaviour
{
	public virtual void Awake()
	{
		this.layout = base.GetComponent<CleverMenuItemLayout>();
		this.selectionManager = base.GetComponent<CleverMenuItemSelectionManager>();
		this.group = base.GetComponent<CleverMenuItemGroup>();
		this.layout.MenuItems.Clear();
		this.selectionManager.MenuItems.Clear();
		this.group.Options.Clear();
		this.pivot = base.transform.FindChild("highlightFade/pivot");
		foreach (object obj in this.pivot)
		{
			UnityEngine.Object.Destroy(((Transform)obj).gameObject);
		}
		foreach (object obj2 in base.transform.FindChild("highlightFade/legend"))
		{
			UnityEngine.Object.Destroy(((Transform)obj2).gameObject);
		}
		TransparencyAnimator[] componentsInChildren = base.GetComponentsInChildren<TransparencyAnimator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject != base.gameObject)
			{
				componentsInChildren[i].Reset();
			}
		}
		this.InitScreen();
		this.fakeTooltip = this.AddItem(this.DefaultTooltip, false);
		this.fakeTooltip.transform.FindChild("text/stateText").gameObject.SetActive(false);
		this.selectionManager.SetCurrentItem(0);
	}

	public void AddKeybind(string label, Func<KeyCode[]> getKeys, Action<KeyCode[]> setKeys)
	{
		CleverMenuItem cleverMenuItem = this.AddItem(label, true);
		cleverMenuItem.gameObject.name = "Keybind (" + label + ")";
		KeybindControl kc = cleverMenuItem.gameObject.AddComponent<KeybindControl>();
		kc.Init(getKeys, setKeys, this);
		cleverMenuItem.PressedCallback += delegate
		{
			this.SetFakeTooltip("Backspace: remove bind\nEnter: finish editing");
			kc.BeginEditing();
		};
	}

	public abstract void InitScreen();

	public void AddButton(string caption, Action onClick)
	{
		CleverMenuItem cleverMenuItem = this.AddItem("", true);
		cleverMenuItem.gameObject.name = "Button (" + caption + ")";
		cleverMenuItem.gameObject.transform.Find("text/stateText").GetComponent<MessageBox>().SetMessage(new MessageDescriptor(caption));
		cleverMenuItem.PressedCallback += onClick;
	}

	public void AddControllerBind(string label, Func<PlayerInputRebinding.ControllerButton[]> getKeys, Action<PlayerInputRebinding.ControllerButton[]> setKeys)
	{
		CleverMenuItem cleverMenuItem = this.AddItem(label, true);
		cleverMenuItem.gameObject.name = "Controller Bind (" + label + ")";
		ControllerBindControl kc = cleverMenuItem.gameObject.AddComponent<ControllerBindControl>();
		kc.Init(getKeys, setKeys, this);
		cleverMenuItem.PressedCallback += delegate
		{
			this.SetFakeTooltip("Start: finish editing");
			kc.BeginEditing();
		};
	}

	public void SetFakeTooltip(string label)
	{
		this.fakeTooltip.transform.Find("text/nameText").GetComponent<MessageBox>().SetMessage(new MessageDescriptor(label));
	}

	public CleverMenuItem AddItem(string label, bool addToNavigation = true)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(SettingsScreen.Instance.transform.Find("highlightFade/pivot/damageText").gameObject);
		gameObject.transform.SetParent(this.pivot);
		CleverMenuItem component = gameObject.GetComponent<CleverMenuItem>();
		if (addToNavigation)
		{
			this.selectionManager.MenuItems.Add(component);
		}
		component.Pressed = null;
		this.layout.AddItem(component);
		this.layout.Sort();
		component.OnUnhighlight();
		TransparencyAnimator[] componentsInChildren = component.transform.GetComponentsInChildren<TransparencyAnimator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Reset();
			componentsInChildren[i].enabled = true;
		}
		foreach (object obj in gameObject.transform.FindChild("glowGroup"))
		{
			TransparencyAnimator.Register((Transform)obj);
		}
		gameObject.transform.Find("text/nameText").GetComponent<MessageBox>().SetMessage(new MessageDescriptor(label));
		return component;
	}

	public void AddToggle(string caption, string settingID, bool initialValue, Action<bool> set)
	{
		CleverMenuItem cleverMenuItem = this.AddItem(caption, true);
		cleverMenuItem.name = string.Format("toggle ({0})", caption);
		ToggleCustomSettingsAction toggle = cleverMenuItem.gameObject.AddComponent<ToggleCustomSettingsAction>();
		toggle.Init(settingID, initialValue, set);
		cleverMenuItem.PressedCallback += toggle.Toggle;
	}

	public CleverMenuItemLayout layout;

	public CleverMenuItemSelectionManager selectionManager;

	public Transform pivot;

	public CleverMenuItemGroup group;

	public CleverMenuItemTooltipController tooltipController;

	public CleverMenuItem fakeTooltip;

	public string DefaultTooltip = "Click on an action to add or remove binds";
}
