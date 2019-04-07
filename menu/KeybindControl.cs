using System;
using System.Collections.Generic;
using SmartInput;
using UnityEngine;

// Token: 0x02000A39 RID: 2617
public class KeybindControl : MonoBehaviour
{
	// Token: 0x06003886 RID: 14470 RVA: 0x0002BDA0 File Offset: 0x00029FA0
	private void Awake()
	{
		this.messageBox = base.transform.Find("text/stateText").GetComponent<MessageBox>();
	}

	// Token: 0x06003887 RID: 14471 RVA: 0x0002BDBD File Offset: 0x00029FBD
	public void BeginEditing()
	{
		this.currentKeys.Clear();
		this.currentKeys.AddRange(this.GetKeys());
		SuspensionManager.SuspendAll();
		this.editing = true;
		this.exit = 0;
	}

	// Token: 0x06003888 RID: 14472
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
			if (this.compoundButtonInput != null)
			{
				this.compoundButtonInput.Clear();
				foreach (KeyCode keyCode in this.currentKeys)
				{
					this.compoundButtonInput.Add(new KeyCodeButtonInput(keyCode));
				}
			}
			if (this.compoundAxisInput != null)
			{
				this.compoundAxisInput.Clear();
				foreach (KeyCode keyCode2 in this.currentKeys)
				{
					this.compoundAxisInput.Add(new ButtonAxisInput(new KeyCodeButtonInput(keyCode2), this.axisMode));
				}
			}
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

	// Token: 0x0600388A RID: 14474 RVA: 0x0002BE06 File Offset: 0x0002A006
	private void UpdateMessageBox()
	{
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(this.currentKeys.ToArray())));
	}

	// Token: 0x0600388B RID: 14475 RVA: 0x000E6054 File Offset: 0x000E4254
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

	// Token: 0x0600388C RID: 14476 RVA: 0x0002BE28 File Offset: 0x0002A028
	public void Init(Func<KeyCode[]> getKeys, Action<KeyCode[]> setKeys, CompoundButtonInput compoundButtonInput)
	{
		this.GetKeys = getKeys;
		this.SetKeys = setKeys;
		this.compoundButtonInput = compoundButtonInput;
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(getKeys())));
	}

	// Token: 0x0600388D RID: 14477 RVA: 0x0002BE5A File Offset: 0x0002A05A
	public void Init(Func<KeyCode[]> getKeys, Action<KeyCode[]> setKeys, CompoundAxisInput compoundAxisInput, ButtonAxisInput.Mode axisMode)
	{
		this.GetKeys = getKeys;
		this.SetKeys = setKeys;
		this.compoundAxisInput = compoundAxisInput;
		this.axisMode = axisMode;
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(getKeys())));
	}

	// Token: 0x0600388E RID: 14478
	public void Reset()
	{
		this.messageBox.SetMessage(new MessageDescriptor(KeybindControl.KeyBindingToString(this.GetKeys())));
		this.editing = false;
	}

	// Token: 0x04003326 RID: 13094
	private Func<KeyCode[]> GetKeys;

	// Token: 0x04003327 RID: 13095
	private Action<KeyCode[]> SetKeys;

	// Token: 0x04003328 RID: 13096
	private bool editing;

	// Token: 0x04003329 RID: 13097
	private MessageBox messageBox;

	// Token: 0x0400332A RID: 13098
	private List<KeyCode> currentKeys = new List<KeyCode>();

	// Token: 0x0400332B RID: 13099
	private CompoundButtonInput compoundButtonInput;

	// Token: 0x0400332C RID: 13100
	private int exit;

	// Token: 0x0400332D RID: 13101
	private CompoundAxisInput compoundAxisInput;

	// Token: 0x0400332E RID: 13102
	public ButtonAxisInput.Mode axisMode;
}
