using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class SavePedestal : SaveSerialize
{
	public bool IsInside
	{
		get
		{
			return this.CurrentState == SavePedestal.State.Highlighted;
		}
	}

	public override void Awake()
	{
		base.Awake();
		this.m_transform = base.transform;
		this.m_sceneTeleporter = base.GetComponent<SceneTeleporter>();
		SavePedestal.All.Add(this);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		SavePedestal.All.Remove(this);
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_hasBeenUsedBefore);
	}

	private bool CanTeleport
	{
		get
		{
			return this.m_sceneTeleporter && TeleporterController.CanTeleport(this.m_sceneTeleporter.Identifier);
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

	public bool OriHasTargets
	{
		get
		{
			SeinSpiritFlameTargetting spiritFlameTargetting = Characters.Sein.Abilities.SpiritFlameTargetting;
			return spiritFlameTargetting && spiritFlameTargetting.ClosestAttackables.Count > 0;
		}
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

	public void OnBeginTeleporting()
	{
		if (this.TeleportEffect)
		{
			this.TeleportEffect.gameObject.SetActive(true);
			this.TeleportEffect.Initialize();
			this.TeleportEffect.AnimatorDriver.Restart();
		}
	}

	public void OnFinishedTeleporting()
	{
		if (this.TeleportEffect)
		{
			this.TeleportEffect.gameObject.SetActive(false);
		}
	}

	public void MarkAsUsed()
	{
		if (!this.m_hasBeenUsedBefore)
		{
			this.m_hasBeenUsedBefore = true;
			AchievementsLogic.Instance.OnSavePedestalUsedFirstTime();
		}
	}

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

	public static List<SavePedestal> All = new List<SavePedestal>();

	public Transform OriTarget;

	public float Radius = 2f;

	public float OriDuration = 1f;

	private Transform m_transform;

	private MessageBox m_hint;

	public MessageProvider CantTeleportMessage;

	public MessageProvider SaveAndTeleportHintMessage;

	public SoundProvider OnOriEnter;

	public SoundProvider OnOriExit;

	public SoundProvider OnSaveSecondTime;

	private bool m_hasBeenUsedBefore;

	private SceneTeleporter m_sceneTeleporter;

	public TimelineSequence TeleportEffect;

	public ActionMethod OriEnterAction;

	public ActionMethod OriExitAction;

	public ActionMethod OnOpenedAction;

	private bool m_used;

	public SavePedestal.State CurrentState;

	public enum State
	{
		Normal,
		Highlighted
	}
}
