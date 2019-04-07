using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000897 RID: 2199
public class MapStone : SaveSerialize
{
	// Token: 0x06002FFA RID: 12282 RVA: 0x000264CC File Offset: 0x000246CC
	public override void Awake()
	{
		base.Awake();
		this.m_transform = base.transform;
	}

	// Token: 0x06002FFB RID: 12283 RVA: 0x000264E0 File Offset: 0x000246E0
	public void FindWorldArea()
	{
		if (GameWorld.Instance)
		{
			this.WorldArea = GameWorld.Instance.WorldAreaAtPosition(this.m_transform.position);
		}
		if (this.WorldArea == null)
		{
		}
	}

	// Token: 0x06002FFC RID: 12284 RVA: 0x0002651D File Offset: 0x0002471D
	public void Start()
	{
		if (this.WorldArea == null)
		{
			this.FindWorldArea();
		}
	}

	// Token: 0x17000798 RID: 1944
	// (get) Token: 0x06002FFD RID: 12285 RVA: 0x0004D894 File Offset: 0x0004BA94
	public bool OriHasTargets
	{
		get
		{
			SeinSpiritFlameTargetting spiritFlameTargetting = Characters.Sein.Abilities.SpiritFlameTargetting;
			return spiritFlameTargetting && spiritFlameTargetting.ClosestAttackables.Count > 0;
		}
	}

	// Token: 0x06002FFE RID: 12286 RVA: 0x000CA688 File Offset: 0x000C8888
	public void Highlight()
	{
		if (this.OriTarget)
		{
			Characters.Ori.MoveOriToPosition(this.OriTarget.position, this.OriDuration);
		}
		if (Characters.Sein.Abilities.SpiritFlame)
		{
			Characters.Sein.Abilities.SpiritFlame.AddLock("mapStone");
		}
		Characters.Ori.GetComponent<Rigidbody>().velocity = Vector3.zero;
		Characters.Ori.EnableHoverWobbling = false;
		Characters.Ori.InsideMapstone = true;
		if (this.m_hint == null)
		{
			this.m_hint = UI.Hints.Show(this.HintMessage, HintLayer.HintZone, 3f);
		}
		if (this.OriEnterAction)
		{
			this.OriEnterAction.Perform(null);
		}
	}

	// Token: 0x06002FFF RID: 12287 RVA: 0x000CA760 File Offset: 0x000C8960
	public void Unhighlight()
	{
		Characters.Ori.ChangeState(Ori.State.Hovering);
		Characters.Ori.EnableHoverWobbling = true;
		Characters.Ori.InsideMapstone = false;
		if (Characters.Sein.Abilities.SpiritFlame)
		{
			Characters.Sein.Abilities.SpiritFlame.RemoveLock("mapStone");
		}
		if (this.OriExitAction)
		{
			this.OriExitAction.Perform(null);
		}
		if (this.m_hint)
		{
			this.m_hint.HideMessageScreen();
		}
	}

	// Token: 0x06003000 RID: 12288 RVA: 0x00026536 File Offset: 0x00024736
	public void OnDisable()
	{
		if (this.CurrentState == MapStone.State.Highlighted)
		{
			this.CurrentState = MapStone.State.Normal;
			this.Unhighlight();
		}
	}

	// Token: 0x17000799 RID: 1945
	// (get) Token: 0x06003001 RID: 12289 RVA: 0x00026551 File Offset: 0x00024751
	public bool Activated
	{
		get
		{
			return this.CurrentState == MapStone.State.Activated;
		}
	}

	// Token: 0x06003002 RID: 12290 RVA: 0x0002655C File Offset: 0x0002475C
	public override void Serialize(Archive ar)
	{
		this.CurrentState = (MapStone.State)ar.Serialize((int)this.CurrentState);
	}

	// Token: 0x1700079A RID: 1946
	// (get) Token: 0x06003003 RID: 12291 RVA: 0x00026570 File Offset: 0x00024770
	public float DistanceToSein
	{
		get
		{
			return Vector3.Distance(this.m_transform.position, Characters.Sein.Position);
		}
	}

	// Token: 0x06003004 RID: 12292
	public void FixedUpdate()
	{
		MapStone.State currentState = this.CurrentState;
		if (currentState != MapStone.State.Normal)
		{
			if (currentState == MapStone.State.Highlighted)
			{
				if (this.DistanceToSein > this.Radius || this.OriHasTargets || !Characters.Sein.IsOnGround)
				{
					this.Unhighlight();
					this.CurrentState = MapStone.State.Normal;
				}
				if (Characters.Sein.Controller.CanMove && !Characters.Sein.IsSuspended && Core.Input.SpiritFlame.OnPressed)
				{
					if (Characters.Sein.Inventory.MapStones > 0)
					{
						Characters.Sein.Inventory.MapStones--;
						if (this.OnOpenedAction)
						{
							this.OnOpenedAction.Perform(null);
						}
						AchievementsLogic.Instance.OnMapStoneActivated();
						this.CurrentState = MapStone.State.Activated;
						Randomizer.getMapStone();
						RandomizerTrackedDataManager.SetMapstone(this.WorldArea.AreaIdentifier);
						return;
					}
					UI.SeinUI.ShakeMapstones();
					if (this.OnFailAction)
					{
						this.OnFailAction.Perform(null);
						return;
					}
				}
			}
		}
		else if (this.DistanceToSein < this.Radius && !this.OriHasTargets && Characters.Sein.IsOnGround)
		{
			this.Highlight();
			this.CurrentState = MapStone.State.Highlighted;
		}
	}

	// Token: 0x04002B7C RID: 11132
	public Transform OriTarget;

	// Token: 0x04002B7D RID: 11133
	public Color OriHoverColor;

	// Token: 0x04002B7E RID: 11134
	public float Radius = 2f;

	// Token: 0x04002B7F RID: 11135
	private Transform m_transform;

	// Token: 0x04002B80 RID: 11136
	public GameWorldArea WorldArea;

	// Token: 0x04002B81 RID: 11137
	public Texture2D HintTexture;

	// Token: 0x04002B82 RID: 11138
	public MessageProvider HintMessage;

	// Token: 0x04002B83 RID: 11139
	private MessageBox m_hint;

	// Token: 0x04002B84 RID: 11140
	public ActionMethod OriEnterAction;

	// Token: 0x04002B85 RID: 11141
	public ActionMethod OriExitAction;

	// Token: 0x04002B86 RID: 11142
	public ActionMethod OnOpenedAction;

	// Token: 0x04002B87 RID: 11143
	public ActionMethod OnFailAction;

	// Token: 0x04002B88 RID: 11144
	public float OriDuration = 1f;

	// Token: 0x04002B89 RID: 11145
	public MapStone.State CurrentState;

	// Token: 0x02000898 RID: 2200
	public enum State
	{
		// Token: 0x04002B8B RID: 11147
		Normal,
		// Token: 0x04002B8C RID: 11148
		Highlighted,
		// Token: 0x04002B8D RID: 11149
		Activated
	}
}
