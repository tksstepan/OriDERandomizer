using System;
using UnityEngine;

public class TitleScreenPressStartLogic : MonoBehaviour
{
	public void FixedUpdate()
	{
		XboxLiveController.Instance.StartPressedOnMainMenu(new Action(this.OnStartPressedCallback));
	}

	public void OnStartPressedCallback()
	{
		GameStateMachine.Instance.SetToTitleScreen();
		this.OnPressed.Perform(null);
	}

	public ActionMethod OnPressed;
}
