using System;
using Core;
using Game;
using UnityEngine;

public class DoorWithSlots : SaveSerialize
{
	public DoorWithSlots()
	{
	}

	public void OnValidate()
	{
		this.m_transform = base.transform;
	}

	public override void Awake()
	{
		base.Awake();
		this.m_opensOnLeftSide = (this.m_transform.TransformPoint(Vector3.right).x < this.m_transform.position.x);
	}

	public void Highlight()
	{
		if (this.OriTarget)
		{
			Characters.Ori.MoveOriToPosition(this.OriTarget.position, this.OriDuration);
		}
		else
		{
			Characters.Ori.MoveOriToPosition(this.m_transform.position, this.OriDuration);
		}
		if (Characters.Sein.Abilities.SpiritFlame)
		{
			Characters.Sein.Abilities.SpiritFlame.AddLock("doorWithSlots");
		}
		Characters.Ori.GetComponent<Rigidbody>().velocity = Vector3.zero;
		Characters.Ori.EnableHoverWobbling = false;
		Characters.Ori.InsideDoor = true;
		if (this.m_hint == null)
		{
			this.m_hint = UI.Hints.Show(this.HintMessage, HintLayer.HintZone, 600f);
		}
		if (this.OnOriEnterSoundProvider)
		{
			Sound.Play(this.OnOriEnterSoundProvider.GetSound(null), this.m_transform.position, null);
		}
		Randomizer.Keysanity.ApplyKeystoneCount(this.MoonGuid, this.NumberOfOrbsUsed);
	}

	public void Unhighlight()
	{
		Characters.Ori.ChangeState(Ori.State.Hovering);
		Characters.Ori.EnableHoverWobbling = true;
		Characters.Ori.InsideDoor = false;
		if (Characters.Sein.Abilities.SpiritFlame)
		{
			Characters.Sein.Abilities.SpiritFlame.RemoveLock("doorWithSlots");
		}
		if (this.m_hint)
		{
			this.m_hint.HideMessageScreen();
		}
		if (this.OnOriExitSoundProvider)
		{
			Sound.Play(this.OnOriExitSoundProvider.GetSound(null), this.m_transform.position, null);
		}
		Randomizer.Keysanity.ResetKeystoneCount();
	}

	public void RestoreOrbs()
	{
		if (this.NumberOfOrbsUsed > 0 && this.RestoreLeafsSoundProvider)
		{
			Sound.Play(this.RestoreLeafsSoundProvider.GetSound(null), this.m_transform.position, null);
		}
		Characters.Sein.Inventory.CollectKeystones(this.NumberOfOrbsUsed);
		this.NumberOfOrbsUsed = 0;
		Randomizer.Keysanity.ResetKeystoneCount();
	}

	public void OnDisable()
	{
		if (!Characters.Sein)
		{
			return;
		}
		if (this.CurrentState == DoorWithSlots.State.Highlighted)
		{
			this.RestoreOrbs();
			this.Unhighlight();
		}
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_slotsPending);
		ar.Serialize(ref this.NumberOfOrbsUsed);
		ar.Serialize(ref this.m_slotsFilled);
		if (ar.Reading && this.CurrentState == DoorWithSlots.State.Highlighted)
		{
			this.Unhighlight();
			this.CurrentState = DoorWithSlots.State.Normal;
		}
		this.CurrentState = (DoorWithSlots.State)ar.Serialize((int)this.CurrentState);
		if (ar.Reading && this.CurrentState == DoorWithSlots.State.Highlighted)
		{
			this.RestoreOrbs();
			this.CurrentState = DoorWithSlots.State.Normal;
		}
		if (this.m_openDoorSound)
		{
			this.m_openDoorSound.FadeOut(0.5f, true);
			UberPoolManager.Instance.RemoveOnDestroyed(this.m_openDoorSound.gameObject);
			this.m_openDoorSound = null;
		}
		if (ar.Reading && this.CurrentState == DoorWithSlots.State.Opened)
		{
			this.m_checkItOpened = true;
		}
	}

	public float DistanceToSein
	{
		get
		{
			return Vector3.Distance(this.m_transform.position, Characters.Sein.Position);
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

	public bool SeinInRange
	{
		get
		{
			return !this.OriHasTargets && this.DistanceToSein <= this.Radius && (Randomizer.OpenMode || ((!this.m_opensOnLeftSide || this.m_transform.position.x >= Characters.Sein.Position.x) && (this.m_opensOnLeftSide || this.m_transform.position.x <= Characters.Sein.Position.x)));
		}
	}

	public void FixedUpdate()
	{
		switch (this.CurrentState)
		{
		case DoorWithSlots.State.Normal:
			if (this.SeinInRange && !this.OriHasTargets && Characters.Sein.Controller.CanMove)
			{
				this.Highlight();
				this.CurrentState = DoorWithSlots.State.Highlighted;
				return;
			}
			break;
		case DoorWithSlots.State.Highlighted:
			if (!this.SeinInRange)
			{
				this.RestoreOrbs();
				this.Unhighlight();
				this.CurrentState = DoorWithSlots.State.Normal;
			}
			if (!Characters.Sein.Controller.CanMove)
			{
				this.RestoreOrbs();
				this.Unhighlight();
				this.CurrentState = DoorWithSlots.State.Normal;
				return;
			}
			if (Characters.Sein.Controller.CanMove && !Characters.Sein.IsSuspended && Core.Input.SpiritFlame.OnPressed)
			{
				if (Characters.Sein.Inventory.Keystones == 0 && this.NumberOfOrbsRequired > this.NumberOfOrbsUsed)
				{
					this.OnFailAction.Perform(null);
					UI.SeinUI.ShakeKeystones();
					if (this.NotEnoughLeafsSoundProvider)
					{
						Sound.Play(this.NotEnoughLeafsSoundProvider.GetSound(null), this.m_transform.position, null);
					}
				}
				if (Characters.Sein.Inventory.Keystones > 0 && this.NumberOfOrbsUsed < this.NumberOfOrbsRequired)
				{
					this.NumberOfOrbsUsed++;
					Characters.Sein.Inventory.SpendKeystones(1);
					if (this.PlaceLeafSoundSoundProvider)
					{
						Sound.Play(this.PlaceLeafSoundSoundProvider.GetSound(null), this.m_transform.position, null);
					}
				}
				if (this.NumberOfOrbsUsed == this.NumberOfOrbsRequired)
				{
					this.OnOpenedAction.Perform(null);
					this.Unhighlight();
					BingoController.OnKSDoor(this.MoonGuid);
					this.CurrentState = DoorWithSlots.State.Opened;
					if (this.OpenDoorSoundProvider)
					{
						this.m_openDoorSound = Sound.Play(this.OpenDoorSoundProvider.GetSound(null), this.m_transform.position, delegate()
						{
							this.m_openDoorSound = null;
						});
						this.m_openDoorSound.PauseOnSuspend = true;
						return;
					}
				}
			}
			break;
		case DoorWithSlots.State.Opened:
			if (this.m_checkItOpened)
			{
				this.m_checkItOpened = false;
				this.MakeSureItsAtEnd(base.transform.FindChild("doorPieces/doorLeft"));
				this.MakeSureItsAtEnd(base.transform.FindChild("doorPieces/doorRight"));
			}
			break;
		default:
			return;
		}
	}

	private void MakeSureItsAtEnd(Transform c)
	{
		if (c == null)
		{
			return;
		}
		LegacyTranslateAnimator component = c.GetComponent<LegacyTranslateAnimator>();
		if (component.CurrentTime <= 0f && component.Stopped)
		{
			component.StopAndSampleAtEnd();
		}
	}

	public Transform OriTarget;

	public Color OriHoverColor;

	[SerializeField]
	[HideInInspector]
	private Transform m_transform;

	private int m_slotsPending;

	private int m_slotsFilled;

	public ActionMethod OnOpenedAction;

	public ActionMethod OnFailAction;

	public int NumberOfOrbsRequired;

	public int NumberOfOrbsUsed;

	public SoundProvider PlaceLeafSoundSoundProvider;

	public SoundProvider NotEnoughLeafsSoundProvider;

	public SoundProvider OpenDoorSoundProvider;

	public SoundProvider RestoreLeafsSoundProvider;

	public SoundProvider OnOriEnterSoundProvider;

	public SoundProvider OnOriExitSoundProvider;

	public float OriDuration = 1f;

	public float Radius = 10f;

	public MessageProvider HintMessage;

	public CameraShakeAsset DoorKeyInsertShake;

	public ControllerShakeAsset DoorKeyInsertControllerShake;

	private MessageBox m_hint;

	private bool m_opensOnLeftSide;

	public DoorWithSlots.State CurrentState;

	private bool m_checkItOpened;

	private SoundPlayer m_openDoorSound;

	public enum State
	{
		Normal,
		Highlighted,
		Opened
	}
}
