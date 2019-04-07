using System;
using System.Collections.Generic;
using SmartInput;
using UnityEngine;

// Token: 0x02000A33 RID: 2611
public abstract class CustomSettingsScreen : MonoBehaviour
{
	// Token: 0x06003873 RID: 14451 RVA: 0x000E59D8 File Offset: 0x000E3BD8
	public virtual void Awake()
	{
		this.layout = base.GetComponent<CleverMenuItemLayout>();
		this.selectionManager = base.GetComponent<CleverMenuItemSelectionManager>();
		this.group = base.GetComponent<CleverMenuItemGroup>();
		this.layout.MenuItems.Clear();
		this.selectionManager.MenuItems.Clear();
		this.group.Options.Clear();
		this.pivot = base.transform.FindChild("highlightFade/pivot");

		// Delete existing buttons/sliders/etc
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
		this.InitScreen();
		this.selectionManager.SetCurrentItem(0);
	}

	// Token: 0x06003876 RID: 14454 RVA: 0x000E5BB0 File Offset: 0x000E3DB0
	public void AddKeybind(string label, Func<KeyCode[]> getKeys, Action<KeyCode[]> setKeys, CompoundButtonInput compoundButtonInput)
	{
		CleverMenuItem cleverMenuItem = this.AddItem(label);
		cleverMenuItem.gameObject.name = "Keybind (" + label + ")";
		KeybindControl kc = cleverMenuItem.gameObject.AddComponent<KeybindControl>();
		kc.Init(getKeys, setKeys, compoundButtonInput);
		cleverMenuItem.PressedCallback += delegate
		{
			kc.BeginEditing();
		};
	}

	// Token: 0x06003877 RID: 14455
	public abstract void InitScreen();

	// Token: 0x06003878 RID: 14456 RVA: 0x000E5C18 File Offset: 0x000E3E18
	public void AddKeybind(string label, Func<KeyCode[]> getKeys, Action<KeyCode[]> setKeys, CompoundAxisInput compoundAxisInput, ButtonAxisInput.Mode axisMode)
	{
		CleverMenuItem cleverMenuItem = this.AddItem(label);
		cleverMenuItem.gameObject.name = "Keybind (" + label + ")";
		KeybindControl kc = cleverMenuItem.gameObject.AddComponent<KeybindControl>();
		kc.Init(getKeys, setKeys, compoundAxisInput, axisMode);
		cleverMenuItem.OnUnhighlight();
		cleverMenuItem.PressedCallback += delegate
		{
			kc.BeginEditing();
		};
	}

	// Token: 0x06003879 RID: 14457 RVA: 0x000E5C88 File Offset: 0x000E3E88
	public void AddButton(string caption, Action onClick)
	{
		CleverMenuItem cleverMenuItem = this.AddItem("");
		cleverMenuItem.gameObject.name = "Button (" + caption + ")";
		cleverMenuItem.gameObject.transform.Find("text/stateText").GetComponent<MessageBox>().SetMessage(new MessageDescriptor(caption));
		cleverMenuItem.PressedCallback += onClick;
	}

	// Token: 0x0600387A RID: 14458
	private CleverMenuItem AddItem(string label)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(SettingsScreen.Instance.transform.Find("highlightFade/pivot/damageText").gameObject);
		gameObject.transform.SetParent(this.pivot);
		CleverMenuItem component = gameObject.GetComponent<CleverMenuItem>();
		this.selectionManager.MenuItems.Add(component);
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

	// Token: 0x04003316 RID: 13078
	public CleverMenuItemLayout layout;

	// Token: 0x04003317 RID: 13079
	public CleverMenuItemSelectionManager selectionManager;

	// Token: 0x04003318 RID: 13080
	public Transform pivot;

	// Token: 0x04003319 RID: 13081
	public CleverMenuItemGroup group;
}
