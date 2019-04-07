using System;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000916 RID: 2326
public class SeinDoorHandler : SaveSerialize, ISeinReceiver
{
	// Token: 0x0600330D RID: 13069
	public SeinDoorHandler()
	{
	}

	// Token: 0x17000803 RID: 2051
	// (get) Token: 0x0600330E RID: 13070
	// (set) Token: 0x0600330F RID: 13071
	public bool IsOverlappingDoor { get; private set; }

	// Token: 0x06003310 RID: 13072
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
	}

	// Token: 0x06003311 RID: 13073
	public void OnDoorOverlap(Door door)
	{
		if (this.m_enterDoorHint == null)
		{
			if (Characters.Sein.Controller.CanMove)
			{
				this.m_enterDoorHint = UI.Hints.Show((!door.OverrideEnterDoorMessage) ? this.EnterDoorMessage : door.OverrideEnterDoorMessage, HintLayer.Gameplay, 1f);
			}
		}
		else
		{
			this.m_enterDoorHint.Visibility.ResetWaitDuration();
		}
		this.m_isOverlappingDoor = true;
		if (this.Sein.Controller.CanMove && Core.Input.Up.OnPressed && this.Sein.PlatformBehaviour.PlatformMovement.IsOnGround && !this.Sein.Controller.IsBashing && !UI.MainMenuVisible)
		{
			this.EnterIntoDoor(door);
		}
	}

	// Token: 0x06003312 RID: 13074
	public void EnterIntoDoor(Door door)
	{
		if (this.m_enterDoorHint)
		{
			this.m_enterDoorHint.HideMessageScreen();
		}
		this.m_createCheckpoint = door.CreateCheckpoint;
		this.m_targetDoor = null;
		foreach (SceneManagerScene sceneManagerScene in Scenes.Manager.ActiveScenes)
		{
			if (sceneManagerScene.SceneRoot)
			{
				foreach (Door door2 in sceneManagerScene.SceneRoot.SceneRootData.Doors)
				{
					if (door2 != null && door2.name == door.OtherDoorName && door2 != door)
					{
						this.m_targetDoor = door2;
					}
				}
			}
		}
		if (this.m_targetDoor == null)
		{
			return;
		}
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.EnterDoorAnimationPrefab);
		gameObject.transform.position = this.Sein.Position;
		if (Characters.Sein.Controller.FaceLeft)
		{
			gameObject.transform.localScale = Vector3.Scale(new Vector3(-1f, 1f, 1f), gameObject.transform.localScale);
		}
		if (door.EnterDoorAction)
		{
			door.EnterDoorAction.Perform(null);
		}
		Utility.DisableLate(this.Sein);
		UI.Fader.Fade(0.5f, 0.05f, 0.2f, new Action(this.OnFadedToBlack), null);
	}

	// Token: 0x06003313 RID: 13075
	public void OnFadedToBlack()
	{
		Vector3 position = this.Sein.Position;
		if (this.m_targetDoor)
		{
			position = this.m_targetDoor.transform.position;
		}
		if (Randomizer.Entrance)
		{
			Randomizer.EnterDoor(Characters.Sein.Position);
		}
		else
		{
			this.Sein.Position = position;
		}
		CameraPivotZone.InstantUpdate();
		Scenes.Manager.UpdatePosition();
		Scenes.Manager.UnloadScenesAtPosition(true);
		Scenes.Manager.EnableDisabledScenesAtPosition(false);
		this.Sein.gameObject.SetActive(true);
		UI.Cameras.Current.MoveCameraToTargetInstantly(true);
		this.Sein.PlatformBehaviour.PlatformMovement.PlaceOnGround(0.5f, 0f);
		UI.Cameras.Current.MoveCameraToTargetInstantly(true);
		if (Characters.Ori)
		{
			Characters.Ori.MoveOriBackToPlayer();
		}
		if (this.m_createCheckpoint)
		{
			GameController.Instance.CreateCheckpoint();
			GameController.Instance.PerformSaveGameSequence();
		}
		LateStartHook.AddLateStartMethod(new Action(this.OnGoneThroughDoor));
	}

	// Token: 0x06003314 RID: 13076
	public void OnGoneThroughDoor()
	{
		if (this.m_targetDoor != null && this.m_targetDoor.ComeOutOfDoorAction)
		{
			this.m_targetDoor.ComeOutOfDoorAction.Perform(null);
		}
		this.m_targetDoor = null;
		CameraFrustumOptimizer.ForceUpdate();
	}

	// Token: 0x06003315 RID: 13077
	public void FixedUpdate()
	{
		this.IsOverlappingDoor = this.m_isOverlappingDoor;
		this.m_isOverlappingDoor = false;
		bool isSuspended = this.Sein.IsSuspended;
	}

	// Token: 0x06003316 RID: 13078
	public override void Serialize(Archive ar)
	{
	}

	// Token: 0x04002E13 RID: 11795
	public SeinCharacter Sein;

	// Token: 0x04002E14 RID: 11796
	public GameObject EnterDoorAnimationPrefab;

	// Token: 0x04002E15 RID: 11797
	public MessageProvider EnterDoorMessage;

	// Token: 0x04002E16 RID: 11798
	private MessageBox m_enterDoorHint;

	// Token: 0x04002E17 RID: 11799
	private bool m_createCheckpoint;

	// Token: 0x04002E18 RID: 11800
	private bool m_isOverlappingDoor;

	// Token: 0x04002E19 RID: 11801
	private Door m_targetDoor;
}
