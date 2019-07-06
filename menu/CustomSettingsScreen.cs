using System;
using System.Collections.Generic;
using SmartInput;
using UnityEngine;

// Token: 0x02000A35 RID: 2613
public abstract class CustomSettingsScreen : MonoBehaviour
{
	// Token: 0x06003880 RID: 14464 RVA: 0x000E6BBC File Offset: 0x000E4DBC
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

	// Token: 0x06003883 RID: 14467 RVA: 0x000E6D94 File Offset: 0x000E4F94
	public void AddKeybind(string label, Func<KeyCode[]> getKeys, Action<KeyCode[]> setKeys)
	{
		CleverMenuItem cleverMenuItem = this.AddItem(label);
		cleverMenuItem.gameObject.name = "Keybind (" + label + ")";
		KeybindControl kc = cleverMenuItem.gameObject.AddComponent<KeybindControl>();
		kc.Init(getKeys, setKeys);
		cleverMenuItem.PressedCallback += delegate
		{
			kc.BeginEditing();
		};
	}

	// Token: 0x06003884 RID: 14468
	public abstract void InitScreen();

	// Token: 0x06003886 RID: 14470 RVA: 0x000E6E6C File Offset: 0x000E506C
	public void AddButton(string caption, Action onClick)
	{
		CleverMenuItem cleverMenuItem = this.AddItem("");
		cleverMenuItem.gameObject.name = "Button (" + caption + ")";
		cleverMenuItem.gameObject.transform.Find("text/stateText").GetComponent<MessageBox>().SetMessage(new MessageDescriptor(caption));
		cleverMenuItem.PressedCallback += onClick;
	}

	// Token: 0x06003887 RID: 14471 RVA: 0x000E6ECC File Offset: 0x000E50CC
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

	// Token: 0x06003888 RID: 14472 RVA: 0x000E6FE4 File Offset: 0x000E51E4
	public void AddControllerBind(string label, Func<PlayerInputRebinding.ControllerButton[]> getKeys, Action<PlayerInputRebinding.ControllerButton[]> setKeys)
	{
		CleverMenuItem cleverMenuItem = this.AddItem(label);
		cleverMenuItem.gameObject.name = "Controller Bind (" + label + ")";
		ControllerBindControl kc = cleverMenuItem.gameObject.AddComponent<ControllerBindControl>();
		kc.Init(getKeys, setKeys);
		cleverMenuItem.PressedCallback += delegate
		{
			kc.BeginEditing();
		};
	}

	// Token: 0x04003357 RID: 13143
	public CleverMenuItemLayout layout;

	// Token: 0x04003358 RID: 13144
	public CleverMenuItemSelectionManager selectionManager;

	// Token: 0x04003359 RID: 13145
	public Transform pivot;

	// Token: 0x0400335A RID: 13146
	public CleverMenuItemGroup group;
}
