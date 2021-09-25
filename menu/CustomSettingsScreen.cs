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
		cleverMenuItem.PressedCallback += delegate()
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
		cleverMenuItem.PressedCallback += delegate()
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
		foreach (var c in gameObject.GetComponentsInChildren<MonoBehaviour>())
			c.enabled = true;
		CleverMenuItem component = gameObject.GetComponent<CleverMenuItem>();
		component.Pressed = null;
		if (addToNavigation)
		{
			this.selectionManager.MenuItems.Add(component);
		}
		this.AddToLayout(component);
		TransparencyAnimator[] componentsInChildren = component.transform.GetComponentsInChildren<TransparencyAnimator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Reset();
			componentsInChildren[i].enabled = true;
		}
		foreach (object obj in component.transform.FindChild("glowGroup"))
		{
			TransparencyAnimator.Register((Transform)obj);
		}
		gameObject.transform.Find("text/nameText").GetComponent<MessageBox>().SetMessage(new MessageDescriptor(label));
		return component;
	}

	public void AddToggle(string caption, RandomizerSettings.BoolSetting setting)
	{
		CleverMenuItem cleverMenuItem = this.AddItem(caption, true);
		cleverMenuItem.name = string.Format("toggle ({0})", caption);
		ToggleCustomSettingsAction toggleCustomSettingsAction = cleverMenuItem.gameObject.AddComponent<ToggleCustomSettingsAction>();
		toggleCustomSettingsAction.Setting = setting;
		toggleCustomSettingsAction.Init();
		cleverMenuItem.PressedCallback += toggleCustomSettingsAction.Toggle;
	}

	private void AddToLayout(CleverMenuItem item)
	{
		this.layout.AddItem(item);
		this.layout.Sort();
		item.SetOpacity(1f);
		item.OnUnhighlight();
	}

	public void AddSlider(string caption, RandomizerSettings.FloatSetting setting, float min, float max, float step)
	{
		// Template is music volume slider
		GameObject clone = UnityEngine.Object.Instantiate<GameObject>(SettingsScreen.Instance.transform.Find("highlightFade/pivot/musicVolume").gameObject);
		clone.gameObject.name = caption;
		foreach (var c in clone.GetComponentsInChildren<MonoBehaviour>())
			c.enabled = true;
		
		// Add to navigation manager (required for all option types)
		clone.transform.SetParent(this.pivot);
		CleverMenuItem cleverMenuItem = clone.GetComponent<CleverMenuItem>();
		this.selectionManager.MenuItems.Add(cleverMenuItem);
		this.AddToLayout(cleverMenuItem);

		// Add to group (required for sliders and dropdown items, but not toggles)
		CleverValueSlider slider = clone.transform.FindChild("slider").GetComponent<CleverValueSlider>();
		slider.NavigateMessageBoxes = new MessageBox[0]; // This is usually the control label at the bottom of the screen, but that's hidden for now
		this.group.AddItem(cleverMenuItem, slider);

		// Set up slider properties
		slider.MinValue = min;
		slider.MaxValue = max;
		slider.Step = step;
		(slider as MusicVolumeSlider).Setting = setting;

		// Update label
		MessageBox component = clone.transform.Find("nameText").GetComponent<MessageBox>();
		component.MessageProvider = null;
		component.SetMessage(new MessageDescriptor(caption));
	}

	public CleverMenuItemLayout layout;

	public CleverMenuItemSelectionManager selectionManager;

	public Transform pivot;

	public CleverMenuItemGroup group;

	public CleverMenuItemTooltipController tooltipController;

	public CleverMenuItem fakeTooltip;

	public string DefaultTooltip = "Click on an action to add or remove binds";
}
