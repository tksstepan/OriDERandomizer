using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class EnergyDoor : SaveSerialize
{
	public void OnValidate()
	{
		this.m_transform = base.transform;
	}

	public override void Awake()
	{
		base.Awake();
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
			Characters.Sein.Abilities.SpiritFlame.AddLock("energyDoor");
		}
		Characters.Ori.GetComponent<Rigidbody>().velocity = Vector3.zero;
		Characters.Ori.EnableHoverWobbling = false;
		if (this.m_hint == null)
		{
			this.m_hint = UI.Hints.Show(this.HintMessage, HintLayer.HintZone, 3f);
		}
		if (this.OnOriEnterSoundProvider)
		{
			Sound.Play(this.OnOriEnterSoundProvider.GetSound(null), this.m_transform.position, null);
		}
	}

	public void Unhighlight()
	{
		Characters.Ori.ChangeState(Ori.State.Hovering);
		Characters.Ori.EnableHoverWobbling = true;
		if (Characters.Sein.Abilities.SpiritFlame)
		{
			Characters.Sein.Abilities.SpiritFlame.RemoveLock("energyDoor");
		}
		if (this.m_hint)
		{
			this.m_hint.HideMessageScreen();
		}
		if (this.OnOriExitSoundProvider)
		{
			Sound.Play(this.OnOriExitSoundProvider.GetSound(null), this.m_transform.position, null);
		}
	}

	public void RestoreOrbs()
	{
		if (this.AmountOfEnergyUsed > 0 && this.RestoreSoundProvider)
		{
			Sound.Play(this.RestoreSoundProvider.GetSound(null), this.m_transform.position, null);
		}
		if (Characters.Sein)
		{
			Characters.Sein.Energy.Gain((float)this.AmountOfEnergyUsed);
		}
		this.AmountOfEnergyUsed = 0;
	}

	public void OnDisable()
	{
		if (this.CurrentState == EnergyDoor.State.Highlighted)
		{
			this.RestoreOrbs();
			this.Unhighlight();
		}
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_slotsPending);
		ar.Serialize(ref this.AmountOfEnergyUsed);
		ar.Serialize(ref this.m_slotsFilled);
		if (ar.Reading && this.CurrentState == EnergyDoor.State.Highlighted)
		{
			this.Unhighlight();
			this.CurrentState = EnergyDoor.State.Normal;
		}
		this.CurrentState = (EnergyDoor.State)ar.Serialize((int)this.CurrentState);
		if (ar.Reading && this.CurrentState == EnergyDoor.State.Highlighted)
		{
			this.RestoreOrbs();
			this.CurrentState = EnergyDoor.State.Normal;
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
			return !this.OriHasTargets && this.DistanceToSein <= this.Radius;
		}
	}

	public void RegisterSlot(EnergyDoorSlot slot)
	{
		this.m_slots.Add(slot);
	}

	public void UpdateSlots()
	{
		foreach (EnergyDoorSlot energyDoorSlot in this.m_slots)
		{
			energyDoorSlot.Refresh();
		}
	}

	public void FixedUpdate()
	{
		if (!Characters.Sein)
		{
			return;
		}
		EnergyDoor.State currentState = this.CurrentState;
		if (currentState != EnergyDoor.State.Normal)
		{
			if (currentState == EnergyDoor.State.Highlighted)
			{
				if (!this.SeinInRange)
				{
					this.RestoreOrbs();
					this.Unhighlight();
					this.CurrentState = EnergyDoor.State.Normal;
				}
				if (!Characters.Sein.Controller.CanMove)
				{
					this.RestoreOrbs();
					this.Unhighlight();
					this.CurrentState = EnergyDoor.State.Normal;
					return;
				}
				if (Characters.Sein.Controller.CanMove && !Characters.Sein.IsSuspended && Core.Input.SpiritFlame.OnPressed)
				{
					if (Characters.Sein.Energy.Current < 1f && this.AmountOfEnergyRequired > this.AmountOfEnergyUsed)
					{
						this.OnFailAction.Perform(null);
						Characters.Sein.Energy.NotifyOutOfEnergy();
					}
					if (Characters.Sein.Energy.Current >= 1f && this.AmountOfEnergyUsed < this.AmountOfEnergyRequired)
					{
						this.AmountOfEnergyUsed++;
						Characters.Sein.Energy.Spend(1f);
						this.UpdateSlots();
						if (this.PlaceSlotSoundProvider)
						{
							Sound.Play(this.PlaceSlotSoundProvider.GetSound(null), this.m_transform.position, null);
						}
					}
					if (this.AmountOfEnergyUsed == this.AmountOfEnergyRequired)
					{
						BingoController.OnEnergyDoor(this.MoonGuid);
						this.OnOpenedAction.Perform(null);
						this.Unhighlight();
						this.CurrentState = EnergyDoor.State.Opened;
						if (this.ActivateSoundProvider)
						{
							Sound.Play(this.ActivateSoundProvider.GetSound(null), this.m_transform.position, null);
							return;
						}
					}
				}
			}
		}
		else if (this.SeinInRange && !this.OriHasTargets && Characters.Sein.Controller.CanMove)
		{
			this.Highlight();
			this.CurrentState = EnergyDoor.State.Highlighted;
		}
	}

	public Transform OriTarget;

	[SerializeField]
	[HideInInspector]
	private Transform m_transform;

	private int m_slotsPending;

	private int m_slotsFilled;

	public ActionMethod OnOpenedAction;

	public ActionMethod OnFailAction;

	public int AmountOfEnergyRequired;

	public int AmountOfEnergyUsed;

	public SoundProvider PlaceSlotSoundProvider;

	public SoundProvider ActivateSoundProvider;

	public SoundProvider RestoreSoundProvider;

	public SoundProvider OnOriEnterSoundProvider;

	public SoundProvider OnOriExitSoundProvider;

	public float OriDuration = 1f;

	public float Radius = 10f;

	public Texture2D HintTexture;

	public MessageProvider HintMessage;

	private MessageBox m_hint;

	public EnergyDoor.State CurrentState;

	private List<EnergyDoorSlot> m_slots = new List<EnergyDoorSlot>();

	public enum State
	{
		Normal,
		Highlighted,
		Opened
	}
}
