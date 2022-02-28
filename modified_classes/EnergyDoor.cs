using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200088D RID: 2189
public class EnergyDoor : SaveSerialize
{
	// Token: 0x06002F79 RID: 12153 RVA: 0x0002663A File Offset: 0x0002483A
	public void OnValidate()
	{
		this.m_transform = base.transform;
	}

	// Token: 0x06002F7A RID: 12154 RVA: 0x00026648 File Offset: 0x00024848
	public override void Awake()
	{
		base.Awake();
	}

	// Token: 0x06002F7B RID: 12155 RVA: 0x000C97CC File Offset: 0x000C79CC
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

	// Token: 0x06002F7C RID: 12156 RVA: 0x000C98CC File Offset: 0x000C7ACC
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

	// Token: 0x06002F7D RID: 12157 RVA: 0x000C996C File Offset: 0x000C7B6C
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

	// Token: 0x06002F7E RID: 12158 RVA: 0x00026650 File Offset: 0x00024850
	public void OnDisable()
	{
		if (this.CurrentState == EnergyDoor.State.Highlighted)
		{
			this.RestoreOrbs();
			this.Unhighlight();
		}
	}

	// Token: 0x06002F7F RID: 12159 RVA: 0x000C99E0 File Offset: 0x000C7BE0
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

	// Token: 0x17000780 RID: 1920
	// (get) Token: 0x06002F80 RID: 12160 RVA: 0x0002666A File Offset: 0x0002486A
	public float DistanceToSein
	{
		get
		{
			return Vector3.Distance(this.m_transform.position, Characters.Sein.Position);
		}
	}

	// Token: 0x17000781 RID: 1921
	// (get) Token: 0x06002F81 RID: 12161 RVA: 0x0004E8C4 File Offset: 0x0004CAC4
	public bool OriHasTargets
	{
		get
		{
			SeinSpiritFlameTargetting spiritFlameTargetting = Characters.Sein.Abilities.SpiritFlameTargetting;
			return spiritFlameTargetting && spiritFlameTargetting.ClosestAttackables.Count > 0;
		}
	}

	// Token: 0x17000782 RID: 1922
	// (get) Token: 0x06002F82 RID: 12162 RVA: 0x00026686 File Offset: 0x00024886
	public bool SeinInRange
	{
		get
		{
			return !this.OriHasTargets && this.DistanceToSein <= this.Radius;
		}
	}

	// Token: 0x06002F83 RID: 12163 RVA: 0x000266A9 File Offset: 0x000248A9
	public void RegisterSlot(EnergyDoorSlot slot)
	{
		this.m_slots.Add(slot);
	}

	// Token: 0x06002F84 RID: 12164 RVA: 0x000C9A6C File Offset: 0x000C7C6C
	public void UpdateSlots()
	{
		foreach (EnergyDoorSlot energyDoorSlot in this.m_slots)
		{
			energyDoorSlot.Refresh();
		}
	}

	// Token: 0x06002F85 RID: 12165 RVA: 0x000C9AC8 File Offset: 0x000C7CC8
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

	// Token: 0x04002AF4 RID: 10996
	public Transform OriTarget;

	// Token: 0x04002AF5 RID: 10997
	[SerializeField]
	[HideInInspector]
	private Transform m_transform;

	// Token: 0x04002AF6 RID: 10998
	private int m_slotsPending;

	// Token: 0x04002AF7 RID: 10999
	private int m_slotsFilled;

	// Token: 0x04002AF8 RID: 11000
	public ActionMethod OnOpenedAction;

	// Token: 0x04002AF9 RID: 11001
	public ActionMethod OnFailAction;

	// Token: 0x04002AFA RID: 11002
	public int AmountOfEnergyRequired;

	// Token: 0x04002AFB RID: 11003
	public int AmountOfEnergyUsed;

	// Token: 0x04002AFC RID: 11004
	public SoundProvider PlaceSlotSoundProvider;

	// Token: 0x04002AFD RID: 11005
	public SoundProvider ActivateSoundProvider;

	// Token: 0x04002AFE RID: 11006
	public SoundProvider RestoreSoundProvider;

	// Token: 0x04002AFF RID: 11007
	public SoundProvider OnOriEnterSoundProvider;

	// Token: 0x04002B00 RID: 11008
	public SoundProvider OnOriExitSoundProvider;

	// Token: 0x04002B01 RID: 11009
	public float OriDuration = 1f;

	// Token: 0x04002B02 RID: 11010
	public float Radius = 10f;

	// Token: 0x04002B03 RID: 11011
	public Texture2D HintTexture;

	// Token: 0x04002B04 RID: 11012
	public MessageProvider HintMessage;

	// Token: 0x04002B05 RID: 11013
	private MessageBox m_hint;

	// Token: 0x04002B06 RID: 11014
	public EnergyDoor.State CurrentState;

	// Token: 0x04002B07 RID: 11015
	private List<EnergyDoorSlot> m_slots = new List<EnergyDoorSlot>();

	// Token: 0x0200088E RID: 2190
	public enum State
	{
		// Token: 0x04002B09 RID: 11017
		Normal,
		// Token: 0x04002B0A RID: 11018
		Highlighted,
		// Token: 0x04002B0B RID: 11019
		Opened
	}
}
