using System;
using System.Collections.Generic;
using SmartInput;
using UnityEngine;

// Token: 0x02000A3D RID: 2621
public class KeybindControl : MonoBehaviour
{
	// Token: 0x06003898 RID: 14488 RVA: 0x0002BDF1 File Offset: 0x00029FF1
	private void Awake()
	{
		this.messageBox = base.transform.Find("text/stateText").GetComponent<MessageBox>();
	}

	// Token: 0x06003899 RID: 14489 RVA: 0x0002BE0E File Offset: 0x0002A00E
	public void BeginEditing()
	{
		this.currentKeys.Clear();
		this.currentKeys.AddRange(this.GetKeys());
		SuspensionManager.SuspendAll();
		this.editing = true;
		this.exit = 0;
	}

	// Token: 0x0600389A RID: 14490 RVA: 0x000E70B8 File Offset: 0x000E52B8
	public void Update()
	{
		if (!this.editing)
		{
			return;
		}
		if (this.exit < 2)
		{
			this.exit++;
			return;
		}
		if (Input.GetKeyDown(KeyCode.Return) && this.currentKeys.Count > 0)
		{
			this.editing = false;
			SuspensionManager.ResumeAll();
			this.SetKeys(this.currentKeys.ToArray());
			PlayerInputRebinding.WriteKeyRebindSettings();
			PlayerInput.Instance.RefreshControlScheme();
			this.owner.SetFakeTooltip(this.owner.DefaultTooltip);
			return;
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			if (this.currentKeys.Count > 0)
			{
				this.currentKeys.RemoveAt(this.currentKeys.Count - 1);
				this.UpdateMessageBox();
				return;
			}
		}
		else if (Input.anyKeyDown)
		{
			foreach (object obj in Enum.GetValues(typeof(KeyCode)))
			{
				KeyCode keyCode3 = (KeyCode)obj;
				if (Input.GetKeyDown(keyCode3) && !this.currentKeys.Contains(keyCode3))
				{
					this.currentKeys.Add(keyCode3);
					this.UpdateMessageBox();
				}
			}
		}
	}

	// Token: 0x0600389C RID: 14492 RVA: 0x0002BE57 File Offset: 0x0002A057
	private void UpdateMessageBox()
	{
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(this.currentKeys.ToArray())));
	}

	// Token: 0x0600389D RID: 14493 RVA: 0x0005C10C File Offset: 0x0005A30C
	public static string KeyBindingToString(KeyCode[] codes)
	{
		string text = string.Empty;
		bool flag = true;
		foreach (KeyCode keyCode in codes)
		{
			text += ((!flag) ? ", " : string.Empty);
			text += keyCode;
			flag = false;
		}
		return text;
	}

	// Token: 0x0600389E RID: 14494 RVA: 0x0002BE79 File Offset: 0x0002A079
	public void Init(Func<KeyCode[]> getKeys, Action<KeyCode[]> setKeys, CustomSettingsScreen owner)
	{
		this.owner = owner;
		this.GetKeys = getKeys;
		this.SetKeys = setKeys;
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(getKeys())));
	}

	// Token: 0x060038A0 RID: 14496 RVA: 0x0002BEE5 File Offset: 0x0002A0E5
	public void Reset()
	{
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(this.GetKeys())));
		this.editing = false;
	}

	// Token: 0x04003369 RID: 13161
	private Func<KeyCode[]> GetKeys;

	// Token: 0x0400336A RID: 13162
	private Action<KeyCode[]> SetKeys;

	// Token: 0x0400336B RID: 13163
	private bool editing;

	// Token: 0x0400336C RID: 13164
	private MessageBox messageBox;

	// Token: 0x0400336D RID: 13165
	private List<KeyCode> currentKeys = new List<KeyCode>();

	// Token: 0x0400336F RID: 13167
	private int exit;
	private CustomSettingsScreen owner;
}
