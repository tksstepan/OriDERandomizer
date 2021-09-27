using System;
using UnityEngine;

public abstract class CustomSettingsScreen : MonoBehaviour
{
    public void OnDisable()
    {
        // Will only write if there have been changes
        RandomizerSettings.WriteSettings();
    }

    public virtual void Awake()
    {
        // Layout and selection manager
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

        TransparencyAnimator[] componentsInChildren = base.GetComponentsInChildren<TransparencyAnimator>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            if (componentsInChildren[i].gameObject != base.gameObject)
            {
                componentsInChildren[i].Reset();
            }
        }

        // Tooltip
        Transform originalToolip = SettingsScreen.Instance.transform.Find("highlightFade/pivot/tooltip");
        Transform tooltip = Instantiate(originalToolip);
        tooltip.SetParent(this.pivot);
        tooltip.position = originalToolip.position;
        this.tooltipController = tooltip.GetComponent<CleverMenuItemTooltipController>();
        this.tooltipController.Selection = this.selectionManager;
        this.tooltipController.UpdateTooltip();
        this.tooltipController.enabled = true;

        this.InitScreen();
        this.selectionManager.SetCurrentItem(0);
    }

    public void AddKeybind(string label, Func<KeyCode[]> getKeys, Action<KeyCode[]> setKeys)
    {
        CleverMenuItem cleverMenuItem = this.AddItem(label);
        cleverMenuItem.gameObject.name = "Keybind (" + label + ")";
        KeybindControl kc = cleverMenuItem.gameObject.AddComponent<KeybindControl>();
        kc.Init(getKeys, setKeys, this);
        cleverMenuItem.PressedCallback += delegate ()
        {
            kc.BeginEditing();
        };
    }

    public abstract void InitScreen();

    public void HideLegend()
    {
        UnityEngine.Object.Destroy(transform.FindChild("highlightFade/legend").gameObject);
    }

    public void AddButton(string caption, Action onClick)
    {
        CleverMenuItem cleverMenuItem = this.AddItem("");
        cleverMenuItem.gameObject.name = "Button (" + caption + ")";
        cleverMenuItem.gameObject.transform.Find("text/stateText").GetComponent<MessageBox>().SetMessage(new MessageDescriptor(caption));
        cleverMenuItem.PressedCallback += onClick;
    }

    public void AddControllerBind(string label, Func<PlayerInputRebinding.ControllerButton[]> getKeys, Action<PlayerInputRebinding.ControllerButton[]> setKeys)
    {
        CleverMenuItem cleverMenuItem = this.AddItem(label);
        cleverMenuItem.gameObject.name = "Controller Bind (" + label + ")";
        ControllerBindControl kc = cleverMenuItem.gameObject.AddComponent<ControllerBindControl>();
        kc.Init(getKeys, setKeys, this);
        cleverMenuItem.PressedCallback += delegate ()
        {
            kc.BeginEditing();
        };
    }

    private void AddToLayout(CleverMenuItem item)
    {
        this.layout.AddItem(item);
        this.layout.Sort();
        item.SetOpacity(1f);
        item.OnUnhighlight();
    }

    public CleverMenuItem AddItem(string label)
    {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(SettingsScreen.Instance.transform.Find("highlightFade/pivot/damageText").gameObject);
        gameObject.transform.SetParent(this.pivot);
        foreach (var c in gameObject.GetComponentsInChildren<MonoBehaviour>())
            c.enabled = true;
        CleverMenuItem component = gameObject.GetComponent<CleverMenuItem>();
        component.Pressed = null;
        this.selectionManager.MenuItems.Add(component);
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

    public void AddToggle(RandomizerSettings.BoolSetting setting, string tooltip)
    {
        CleverMenuItem cleverMenuItem = this.AddItem(setting.Name);
        cleverMenuItem.name = setting.Name;
        ToggleCustomSettingsAction toggleCustomSettingsAction = cleverMenuItem.gameObject.AddComponent<ToggleCustomSettingsAction>();
        toggleCustomSettingsAction.Setting = setting;
        toggleCustomSettingsAction.Init();
        cleverMenuItem.PressedCallback += toggleCustomSettingsAction.Toggle;

        ConfigureTooltip(cleverMenuItem.GetComponent<CleverMenuItemTooltip>(), tooltip);
    }

    public void AddSlider(RandomizerSettings.FloatSetting setting, float min, float max, float step, string tooltip)
    {
        // Template is music volume slider
        GameObject clone = UnityEngine.Object.Instantiate<GameObject>(SettingsScreen.Instance.transform.Find("highlightFade/pivot/musicVolume").gameObject);
        clone.gameObject.name = setting.Name;
        foreach (var c in clone.GetComponentsInChildren<MonoBehaviour>())
            c.enabled = true;

        // Add to navigation manager (required for all option types)
        clone.transform.SetParent(this.pivot);
        CleverMenuItem cleverMenuItem = clone.GetComponent<CleverMenuItem>();
        this.selectionManager.MenuItems.Add(cleverMenuItem);
        this.AddToLayout(cleverMenuItem);

        // Add to group (required for sliders and dropdown items, but not toggles)
        CleverValueSlider slider = clone.transform.FindChild("slider").GetComponent<CleverValueSlider>();
        slider.NavigateMessageBoxes = new MessageBox[]
		{
			transform.FindChild("highlightFade/legend/pcLegend/navigate").GetComponent<MessageBox>(),
			transform.FindChild("highlightFade/legend/xBoxLegend/navigate").GetComponent<MessageBox>()
		};
        this.group.AddItem(cleverMenuItem, slider);

        // Set up slider properties
        slider.MinValue = min;
        slider.MaxValue = max;
        slider.Step = step;
        (slider as MusicVolumeSlider).Setting = setting;

        // Update label
        MessageBox nameTextBox = clone.transform.Find("nameText").GetComponent<MessageBox>();
        nameTextBox.MessageProvider = null;
        nameTextBox.SetMessage(new MessageDescriptor(setting.Name));

        // Update tooltip
        ConfigureTooltip(clone.GetComponent<CleverMenuItemTooltip>(), tooltip);
    }

    private void ConfigureTooltip(CleverMenuItemTooltip tooltipComponent, string tooltip)
    {
        var tooltipMessageProvider = ScriptableObject.CreateInstance<RandomizerMessageProvider>();
        tooltipMessageProvider.SetMessage(tooltip);
        tooltipComponent.Tooltip = tooltipMessageProvider;
    }

    public CleverMenuItemLayout layout;

    public CleverMenuItemSelectionManager selectionManager;

    public Transform pivot;

    public CleverMenuItemGroup group;

    public CleverMenuItem fakeTooltip;

    public CleverMenuItemTooltipController tooltipController;

    public string DefaultTooltip = "Click on an action to add or remove binds";
}
