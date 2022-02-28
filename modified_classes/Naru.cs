using System;
using Game;
using UnityEngine;

// Token: 0x0200000E RID: 14
public class Naru : MonoBehaviour, ICharacter
{
	// Token: 0x06000040 RID: 64 RVA: 0x00002DA9 File Offset: 0x00000FA9
	public void Awake()
	{
		Characters.Naru = this;
		Characters.Current = this;
	}

	// Token: 0x06000041 RID: 65
	public void OnDestroy()
	{
		Randomizer.onNaruDestroyed();
		if (Characters.Naru == this)
		{
			Characters.Naru = null;
		}
		if (Characters.Current == this)
		{
			Characters.Current = null;
		}
	}

	// Token: 0x1700000E RID: 14
	// (get) Token: 0x06000042 RID: 66 RVA: 0x00002AF6 File Offset: 0x00000CF6
	// (set) Token: 0x06000043 RID: 67 RVA: 0x00002B03 File Offset: 0x00000D03
	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	// Token: 0x06000044 RID: 68 RVA: 0x00002B11 File Offset: 0x00000D11
	public void Activate(bool active)
	{
		base.gameObject.SetActive(active);
	}

	// Token: 0x1700000F RID: 15
	// (get) Token: 0x06000045 RID: 69 RVA: 0x00002B1F File Offset: 0x00000D1F
	public GameObject GameObject
	{
		get
		{
			return base.gameObject;
		}
	}

	// Token: 0x17000010 RID: 16
	// (get) Token: 0x06000046 RID: 70 RVA: 0x00002DE5 File Offset: 0x00000FE5
	// (set) Token: 0x06000047 RID: 71 RVA: 0x00002DF7 File Offset: 0x00000FF7
	public bool FaceLeft
	{
		get
		{
			return this.Animation.SpriteMirror.FaceLeft;
		}
		set
		{
			this.Animation.SpriteMirror.FaceLeft = value;
		}
	}

	// Token: 0x17000011 RID: 17
	// (get) Token: 0x06000048 RID: 72 RVA: 0x00002E0A File Offset: 0x0000100A
	// (set) Token: 0x06000049 RID: 73 RVA: 0x00002E21 File Offset: 0x00001021
	public Vector3 Speed
	{
		get
		{
			return this.PlatformBehaviour.PlatformMovement.LocalSpeed;
		}
		set
		{
			this.PlatformBehaviour.PlatformMovement.LocalSpeed = value;
		}
	}

	// Token: 0x17000012 RID: 18
	// (get) Token: 0x0600004A RID: 74 RVA: 0x00002B7B File Offset: 0x00000D7B
	public Transform Transform
	{
		get
		{
			return base.transform;
		}
	}

	// Token: 0x17000013 RID: 19
	// (get) Token: 0x0600004B RID: 75 RVA: 0x00002E39 File Offset: 0x00001039
	public bool IsOnGround
	{
		get
		{
			return this.PlatformBehaviour.PlatformMovement.IsOnGround;
		}
	}

	// Token: 0x0600004C RID: 76 RVA: 0x00002E4B File Offset: 0x0000104B
	public void PlaceOnGround()
	{
		this.PlatformBehaviour.PlatformMovement.PlaceOnGround(0.5f, 0f);
	}

	// Token: 0x0400007B RID: 123
	public CharacterAnimationSystem Animation;

	// Token: 0x0400007C RID: 124
	public NaruController Controller;

	// Token: 0x0400007D RID: 125
	public PlatformBehaviour PlatformBehaviour;

	// Token: 0x0400007E RID: 126
	public bool SeinNaruComboEnabled;

	// Token: 0x0400007F RID: 127
	public NaruSounds Sounds;
}
