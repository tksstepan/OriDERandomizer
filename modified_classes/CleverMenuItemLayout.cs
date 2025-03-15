using System;
using System.Collections.Generic;
using UnityEngine;

public class CleverMenuItemLayout : MonoBehaviour
{
	public void OnEnable()
	{
		this.Sort();
	}

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

	public void AddItem(CleverMenuItem item)
	{
		this.MenuItems.Add(item);
		this.Sort();
	}

	public void AddItem(CleverMenuItem item, int index)
	{
		this.MenuItems.Insert(index, item);
		this.Sort();
	}

	public List<CleverMenuItem> MenuItems = new List<CleverMenuItem>();

	public CleverMenuItemLayout.Alignment VerticalAlignment;

	public enum Alignment
	{
		Top,
		Center,
		Bottom
	}
}
