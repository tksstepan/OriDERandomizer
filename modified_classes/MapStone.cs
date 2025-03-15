using System;
using Core;
using Game;
using UnityEngine;

public class MapStone : SaveSerialize
{
	public override void Awake()
	{
		base.Awake();
		this.m_transform = base.transform;
	}

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

	public void Start()
	{
		if (this.WorldArea == null)
		{
			this.FindWorldArea();
		}
	}

	public bool OriHasTargets
	{
		get
		{
			SeinSpiritFlameTargetting spiritFlameTargetting = Characters.Sein.Abilities.SpiritFlameTargetting;
			return spiritFlameTargetting && spiritFlameTargetting.ClosestAttackables.Count > 0;
		}
	}

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
		BingoController.OnTouchMapstone();
		if (this.m_hint == null)
		{
			this.m_hint = UI.Hints.Show(this.HintMessage, HintLayer.HintZone, 3f);
		}
		if (this.OriEnterAction)
		{
			this.OriEnterAction.Perform(null);
		}
	}

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

	public void OnDisable()
	{
		if (this.CurrentState == MapStone.State.Highlighted)
		{
			this.CurrentState = MapStone.State.Normal;
			this.Unhighlight();
		}
	}

	public bool Activated
	{
		get
		{
			return this.CurrentState == MapStone.State.Activated;
		}
	}

	public override void Serialize(Archive ar)
	{
		this.CurrentState = (MapStone.State)ar.Serialize((int)this.CurrentState);
	}

	public float DistanceToSein
	{
		get
		{
			return Vector3.Distance(this.m_transform.position, Characters.Sein.Position);
		}
	}

	public void FixedUpdate()
	{
		MapStone.State currentState = this.CurrentState;
		if (currentState != MapStone.State.Activated && RandomizerLocationManager.IsPickupCollected(this.MoonGuid))
		{
			if (currentState == MapStone.State.Highlighted)
			{
				this.Unhighlight();
			}

			if (this.OnOpenedAction)
			{
				this.OnOpenedAction.PerformInstantly(null);
			}

			this.CurrentState = MapStone.State.Activated;
			return;
		}

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
						RandomizerLocationManager.GivePickup(this.MoonGuid);
						GameWorld.Instance.CurrentArea.DirtyCompletionAmount();
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

	public Transform OriTarget;

	public Color OriHoverColor;

	public float Radius = 2f;

	private Transform m_transform;

	public GameWorldArea WorldArea;

	public Texture2D HintTexture;

	public MessageProvider HintMessage;

	private MessageBox m_hint;

	public ActionMethod OriEnterAction;

	public ActionMethod OriExitAction;

	public ActionMethod OnOpenedAction;

	public ActionMethod OnFailAction;

	public float OriDuration = 1f;

	public MapStone.State CurrentState;

	public enum State
	{
		Normal,
		Highlighted,
		Activated
	}
}
