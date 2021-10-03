using System;
using Core;
using Game;
using UnityEngine;

public class SeinDoorHandler : SaveSerialize, ISeinReceiver
{
	public bool IsOverlappingDoor { get; private set; }

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
	}

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

	public void OnGoneThroughDoor()
	{
		if (this.m_targetDoor != null && this.m_targetDoor.ComeOutOfDoorAction)
		{
			this.m_targetDoor.ComeOutOfDoorAction.Perform(null);
		}
		this.m_targetDoor = null;
		CameraFrustumOptimizer.ForceUpdate();
	}

	public void FixedUpdate()
	{
		this.IsOverlappingDoor = this.m_isOverlappingDoor;
		this.m_isOverlappingDoor = false;
		bool isSuspended = this.Sein.IsSuspended;
	}

	public override void Serialize(Archive ar)
	{
	}

	public SeinCharacter Sein;

	public GameObject EnterDoorAnimationPrefab;

	public MessageProvider EnterDoorMessage;

	private MessageBox m_enterDoorHint;

	private bool m_createCheckpoint;

	private bool m_isOverlappingDoor;

	private Door m_targetDoor;
}
