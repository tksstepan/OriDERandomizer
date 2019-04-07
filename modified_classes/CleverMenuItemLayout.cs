using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200037E RID: 894
public class CleverMenuItemLayout : MonoBehaviour
{
	// Token: 0x060013ED RID: 5101 RVA: 0x0001148E File Offset: 0x0000F68E
	public void OnEnable()
	{
		this.Sort();
	}

	// Token: 0x060013EE RID: 5102 RVA: 0x0006F5A8 File Offset: 0x0006D7A8
	[ContextMenu("Apply")]
	public void Sort()
	{
		float num = 0f;
		foreach (CleverMenuItem cleverMenuItem in this.MenuItems)
		{
			if (cleverMenuItem.IsVisible)
			{
				cleverMenuItem.transform.localPosition = Vector3.down * num;
				num += cleverMenuItem.Space;
			}
		}
		foreach (CleverMenuItem cleverMenuItem2 in this.MenuItems)
		{
			if (cleverMenuItem2.IsVisible)
			{
				if (this.VerticalAlignment == CleverMenuItemLayout.Alignment.Center)
				{
					cleverMenuItem2.transform.localPosition += Vector3.up * num * 0.5f;
				}
				if (this.VerticalAlignment == CleverMenuItemLayout.Alignment.Bottom)
				{
					cleverMenuItem2.transform.localPosition += Vector3.up * num;
				}
			}
		}
	}

	// Token: 0x060013EF RID: 5103 RVA: 0x00011496 File Offset: 0x0000F696
	public void AddItem(CleverMenuItem item)
	{
		this.MenuItems.Add(item);
		this.Sort();
	}

	// Token: 0x060013F0 RID: 5104 RVA: 0x000114AA File Offset: 0x0000F6AA
	public void AddItem(CleverMenuItem item, int index)
	{
		this.MenuItems.Insert(index, item);
		this.Sort();
	}

	// Token: 0x040012AF RID: 4783
	public List<CleverMenuItem> MenuItems = new List<CleverMenuItem>();

	// Token: 0x040012B0 RID: 4784
	public CleverMenuItemLayout.Alignment VerticalAlignment;

	// Token: 0x0200037F RID: 895
	public enum Alignment
	{
		// Token: 0x040012B2 RID: 4786
		Top,
		// Token: 0x040012B3 RID: 4787
		Center,
		// Token: 0x040012B4 RID: 4788
		Bottom
	}
}
