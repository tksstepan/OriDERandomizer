using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000881 RID: 2177
public class DoorWithSlots : SaveSerialize
{
	// Token: 0x06002F4F RID: 12111
	public DoorWithSlots()
	{
	}

	// Token: 0x06002F50 RID: 12112
	public void OnValidate()
	{
		this.m_transform = base.transform;
	}

	// Token: 0x06002F51 RID: 12113
	public override void Awake()
	{
		base.Awake();
		this.m_opensOnLeftSide = (this.m_transform.TransformPoint(Vector3.right).x < this.m_transform.position.x);
	}

	// Token: 0x06002F52 RID: 12114
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
	}

	// Token: 0x06002F53 RID: 12115
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
	}

	// Token: 0x06002F54 RID: 12116
	public void RestoreOrbs()
	{
		if (this.NumberOfOrbsUsed > 0 && this.RestoreLeafsSoundProvider)
		{
			Sound.Play(this.RestoreLeafsSoundProvider.GetSound(null), this.m_transform.position, null);
		}
		Characters.Sein.Inventory.CollectKeystones(this.NumberOfOrbsUsed);
		this.NumberOfOrbsUsed = 0;
	}

	// Token: 0x06002F55 RID: 12117
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

	// Token: 0x06002F56 RID: 12118
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

	// Token: 0x1700077D RID: 1917
	// (get) Token: 0x06002F57 RID: 12119
	public float DistanceToSein
	{
		get
		{
			return Vector3.Distance(this.m_transform.position, Characters.Sein.Position);
		}
	}

	// Token: 0x1700077E RID: 1918
	// (get) Token: 0x06002F58 RID: 12120
	public bool OriHasTargets
	{
		get
		{
			SeinSpiritFlameTargetting spiritFlameTargetting = Characters.Sein.Abilities.SpiritFlameTargetting;
			return spiritFlameTargetting && spiritFlameTargetting.ClosestAttackables.Count > 0;
		}
	}

	// Token: 0x1700077F RID: 1919
	// (get) Token: 0x06002F59 RID: 12121
	public bool SeinInRange
	{
		get
		{
			return !this.OriHasTargets && this.DistanceToSein <= this.Radius && (Randomizer.OpenMode || ((!this.m_opensOnLeftSide || this.m_transform.position.x >= Characters.Sein.Position.x) && (this.m_opensOnLeftSide || this.m_transform.position.x <= Characters.Sein.Position.x)));
		}
	}

	// Token: 0x06002F5A RID: 12122
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

	// Token: 0x06002F5B RID: 12123
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

	// Token: 0x04002AD4 RID: 10964
	public Transform OriTarget;

	// Token: 0x04002AD5 RID: 10965
	public Color OriHoverColor;

	// Token: 0x04002AD6 RID: 10966
	[SerializeField]
	[HideInInspector]
	private Transform m_transform;

	// Token: 0x04002AD7 RID: 10967
	private int m_slotsPending;

	// Token: 0x04002AD8 RID: 10968
	private int m_slotsFilled;

	// Token: 0x04002AD9 RID: 10969
	public ActionMethod OnOpenedAction;

	// Token: 0x04002ADA RID: 10970
	public ActionMethod OnFailAction;

	// Token: 0x04002ADB RID: 10971
	public int NumberOfOrbsRequired;

	// Token: 0x04002ADC RID: 10972
	public int NumberOfOrbsUsed;

	// Token: 0x04002ADD RID: 10973
	public SoundProvider PlaceLeafSoundSoundProvider;

	// Token: 0x04002ADE RID: 10974
	public SoundProvider NotEnoughLeafsSoundProvider;

	// Token: 0x04002ADF RID: 10975
	public SoundProvider OpenDoorSoundProvider;

	// Token: 0x04002AE0 RID: 10976
	public SoundProvider RestoreLeafsSoundProvider;

	// Token: 0x04002AE1 RID: 10977
	public SoundProvider OnOriEnterSoundProvider;

	// Token: 0x04002AE2 RID: 10978
	public SoundProvider OnOriExitSoundProvider;

	// Token: 0x04002AE3 RID: 10979
	public float OriDuration = 1f;

	// Token: 0x04002AE4 RID: 10980
	public float Radius = 10f;

	// Token: 0x04002AE5 RID: 10981
	public MessageProvider HintMessage;

	// Token: 0x04002AE6 RID: 10982
	public CameraShakeAsset DoorKeyInsertShake;

	// Token: 0x04002AE7 RID: 10983
	public ControllerShakeAsset DoorKeyInsertControllerShake;

	// Token: 0x04002AE8 RID: 10984
	private MessageBox m_hint;

	// Token: 0x04002AE9 RID: 10985
	private bool m_opensOnLeftSide;

	// Token: 0x04002AEA RID: 10986
	public DoorWithSlots.State CurrentState;

	// Token: 0x04002AEB RID: 10987
	private bool m_checkItOpened;

	// Token: 0x04002AEC RID: 10988
	private SoundPlayer m_openDoorSound;

	// Token: 0x02000882 RID: 2178
	public enum State
	{
		// Token: 0x04002AEE RID: 10990
		Normal,
		// Token: 0x04002AEF RID: 10991
		Highlighted,
		// Token: 0x04002AF0 RID: 10992
		Opened
	}
}
