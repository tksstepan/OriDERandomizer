using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x020008EA RID: 2282
public class SavePedestal : SaveSerialize
{
	// Token: 0x0600320F RID: 12815
	public SavePedestal()
	{
	}

	// Token: 0x06003210 RID: 12816
	static SavePedestal()
	{
	}

	// Token: 0x170007DF RID: 2015
	// (get) Token: 0x06003211 RID: 12817
	public bool IsInside
	{
		get
		{
			return this.CurrentState == SavePedestal.State.Highlighted;
		}
	}

	// Token: 0x06003212 RID: 12818
	public override void Awake()
	{
		base.Awake();
		this.m_transform = base.transform;
		this.m_sceneTeleporter = base.GetComponent<SceneTeleporter>();
		SavePedestal.All.Add(this);
	}

	// Token: 0x06003213 RID: 12819
	public override void OnDestroy()
	{
		base.OnDestroy();
		SavePedestal.All.Remove(this);
	}

	// Token: 0x06003214 RID: 12820
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_hasBeenUsedBefore);
	}

	// Token: 0x170007E0 RID: 2016
	// (get) Token: 0x06003215 RID: 12821
	private bool CanTeleport
	{
		get
		{
			return this.m_sceneTeleporter && TeleporterController.CanTeleport(this.m_sceneTeleporter.Identifier);
		}
	}

	// Token: 0x06003216 RID: 12822
	public void Highlight()
	{
		if (this.OriTarget)
		{
			Characters.Ori.MoveOriToPosition(this.OriTarget.position, this.OriDuration);
		}
		if (Characters.Sein.Abilities.SpiritFlame)
		{
			Characters.Sein.Abilities.SpiritFlame.AddLock("savePedestal");
		}
		Characters.Ori.GetComponent<Rigidbody>().velocity = Vector3.zero;
		Characters.Ori.EnableHoverWobbling = false;
		if (this.OriEnterAction)
		{
			this.OriEnterAction.Perform(null);
		}
		if (this.m_hint == null)
		{
			this.m_hint = UI.Hints.Show(this.SaveAndTeleportHintMessage, HintLayer.HintZone, 3f);
		}
		if (this.OnOriEnter)
		{
			Sound.Play(this.OnOriEnter.GetSound(null), base.transform.position, null);
		}
		if (this.m_sceneTeleporter)
		{
			TeleporterController.Activate(this.m_sceneTeleporter.Identifier);
		}
	}

	// Token: 0x06003217 RID: 12823
	public void Unhighlight()
	{
		this.m_used = false;
		Characters.Ori.ChangeState(Ori.State.Hovering);
		Characters.Ori.EnableHoverWobbling = true;
		if (Characters.Sein.Abilities.SpiritFlame)
		{
			Characters.Sein.Abilities.SpiritFlame.RemoveLock("savePedestal");
		}
		if (this.OriExitAction)
		{
			this.OriExitAction.Perform(null);
		}
		if (this.m_hint)
		{
			this.m_hint.HideMessageScreen();
		}
		if (this.OnOriExit)
		{
			Sound.Play(this.OnOriExit.GetSound(null), base.transform.position, null);
		}
	}

	// Token: 0x170007E1 RID: 2017
	// (get) Token: 0x06003218 RID: 12824
	public bool OriHasTargets
	{
		get
		{
			SeinSpiritFlameTargetting spiritFlameTargetting = Characters.Sein.Abilities.SpiritFlameTargetting;
			return spiritFlameTargetting && spiritFlameTargetting.ClosestAttackables.Count > 0;
		}
	}

	// Token: 0x170007E2 RID: 2018
	// (get) Token: 0x06003219 RID: 12825
	public float DistanceToSein
	{
		get
		{
			return Vector3.Distance(this.m_transform.position, Characters.Sein.Position);
		}
	}

	// Token: 0x0600321A RID: 12826
	public void FixedUpdate()
	{
		if (Characters.Sein == null)
		{
			return;
		}
		if (Characters.Sein.IsSuspended)
		{
			return;
		}
		SavePedestal.State currentState = this.CurrentState;
		if (currentState != SavePedestal.State.Normal)
		{
			if (currentState == SavePedestal.State.Highlighted)
			{
				if ((!Characters.Sein.Controller.IsPlayingAnimation && this.DistanceToSein > this.Radius) || this.OriHasTargets)
				{
					this.Unhighlight();
					this.CurrentState = SavePedestal.State.Normal;
				}
				if (Characters.Sein.Controller.CanMove && Characters.Sein.PlatformBehaviour.PlatformMovement.IsOnGround)
				{
					if (Core.Input.SpiritFlame.OnPressed && !this.m_used)
					{
						this.SaveOnPedestal();
						return;
					}
					if (Core.Input.SoulFlame.OnPressedNotUsed && !Core.Input.Cancel.Used)
					{
						if (this.m_hint)
						{
							this.m_hint.HideMessageScreen();
						}
						Core.Input.SoulFlame.Used = true;
						UI.Menu.ShowSkillTree();
						return;
					}
					if (Core.Input.SpiritFlame.OnPressed && this.m_used)
					{
						if (this.OnSaveSecondTime)
						{
							Sound.Play(this.OnSaveSecondTime.GetSound(null), base.transform.position, null);
							return;
						}
					}
					else if (Core.Input.Bash.OnPressed && WorldMapUI.IsReady)
					{
						if (this.CanTeleport)
						{
							this.TeleportOnPedestal();
							return;
						}
						UI.Hints.Show(this.CantTeleportMessage, HintLayer.Gameplay, 2f);
						return;
					}
				}
			}
		}
		else if (this.DistanceToSein < this.Radius && !this.OriHasTargets)
		{
			this.Highlight();
			this.CurrentState = SavePedestal.State.Highlighted;
		}
	}

	// Token: 0x0600321B RID: 12827
	private void TeleportOnPedestal()
	{
		if (this.m_hint)
		{
			this.m_hint.HideMessageScreen();
		}
		this.MarkAsUsed();
		Characters.Sein.PlatformBehaviour.PlatformMovement.PositionX = base.transform.position.x;
		TeleporterController.Show(this.m_sceneTeleporter.Identifier);
	}

	// Token: 0x0600321C RID: 12828
	public void OnBeginTeleporting()
	{
		if (this.TeleportEffect)
		{
			this.TeleportEffect.gameObject.SetActive(true);
			this.TeleportEffect.Initialize();
			this.TeleportEffect.AnimatorDriver.Restart();
		}
	}

	// Token: 0x0600321D RID: 12829
	public void OnFinishedTeleporting()
	{
		if (this.TeleportEffect)
		{
			this.TeleportEffect.gameObject.SetActive(false);
		}
	}

	// Token: 0x0600321E RID: 12830
	public void MarkAsUsed()
	{
		if (!this.m_hasBeenUsedBefore)
		{
			this.m_hasBeenUsedBefore = true;
			AchievementsLogic.Instance.OnSavePedestalUsedFirstTime();
		}
	}

	// Token: 0x0600321F RID: 12831
	private void SaveOnPedestal()
	{
		if (this.m_hint)
		{
			this.m_hint.HideMessageScreen();
		}
		this.m_used = true;
		this.MarkAsUsed();
		RandomizerStatsManager.OnSave();
		if (Characters.Sein.Abilities.Carry && Characters.Sein.Abilities.Carry.CurrentCarryable != null)
		{
			Characters.Sein.Abilities.Carry.CurrentCarryable.Drop();
		}
		if (this.OnOpenedAction)
		{
			this.OnOpenedAction.Perform(null);
		}
		base.StartCoroutine(this.MoveSeinToCenterSmoothly());
	}

	// Token: 0x06003220 RID: 12832
	public IEnumerator MoveSeinToCenterSmoothly()
	{
		PlatformMovement seinPlatformMovement = Characters.Sein.PlatformBehaviour.PlatformMovement;
		int num;
		for (int i = 0; i < 10; i = num + 1)
		{
			seinPlatformMovement.PositionX = Mathf.Lerp(seinPlatformMovement.PositionX, base.transform.position.x, 0.2f);
			yield return new WaitForFixedUpdate();
			num = i;
		}
		seinPlatformMovement.PositionX = base.transform.position.x;
		yield break;
	}

	// Token: 0x04002D4A RID: 11594
	public static List<SavePedestal> All = new List<SavePedestal>();

	// Token: 0x04002D4B RID: 11595
	public Transform OriTarget;

	// Token: 0x04002D4C RID: 11596
	public float Radius = 2f;

	// Token: 0x04002D4D RID: 11597
	public float OriDuration = 1f;

	// Token: 0x04002D4E RID: 11598
	private Transform m_transform;

	// Token: 0x04002D4F RID: 11599
	private MessageBox m_hint;

	// Token: 0x04002D50 RID: 11600
	public MessageProvider CantTeleportMessage;

	// Token: 0x04002D51 RID: 11601
	public MessageProvider SaveAndTeleportHintMessage;

	// Token: 0x04002D52 RID: 11602
	public SoundProvider OnOriEnter;

	// Token: 0x04002D53 RID: 11603
	public SoundProvider OnOriExit;

	// Token: 0x04002D54 RID: 11604
	public SoundProvider OnSaveSecondTime;

	// Token: 0x04002D55 RID: 11605
	private bool m_hasBeenUsedBefore;

	// Token: 0x04002D56 RID: 11606
	private SceneTeleporter m_sceneTeleporter;

	// Token: 0x04002D57 RID: 11607
	public TimelineSequence TeleportEffect;

	// Token: 0x04002D58 RID: 11608
	public ActionMethod OriEnterAction;

	// Token: 0x04002D59 RID: 11609
	public ActionMethod OriExitAction;

	// Token: 0x04002D5A RID: 11610
	public ActionMethod OnOpenedAction;

	// Token: 0x04002D5B RID: 11611
	private bool m_used;

	// Token: 0x04002D5C RID: 11612
	public SavePedestal.State CurrentState;

	// Token: 0x020008EB RID: 2283
	public enum State
	{
		// Token: 0x04002D5E RID: 11614
		Normal,
		// Token: 0x04002D5F RID: 11615
		Highlighted
	}
}
