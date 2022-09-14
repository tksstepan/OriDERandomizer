using System;
using Game;
using UnityEngine;

public class MessageControllerB : MonoBehaviour
{
	public bool AnyAbilityPickupStoryMessagesVisible
	{
		get
		{
			return this.m_currentMessageBox;
		}
	}

	public GameObject ShowMessageBox(GameObject messageBoxPrefab, MessageProvider messageProvider, Vector3 position, float duration = 3f)
	{
		if (messageProvider == null)
		{
			return null;
		}
		if (SeinUI.DebugHideUI)
		{
			return null;
		}
		GameObject expr_25 = InstantiateUtility.Instantiate(messageBoxPrefab, position, Quaternion.identity) as GameObject;
		MessageBox componentInChildren = expr_25.GetComponentInChildren<MessageBox>();
		if (componentInChildren.Visibility)
		{
			componentInChildren.SetWaitDuration(duration);
		}
		componentInChildren.SetMessageProvider(messageProvider);
		return expr_25;
	}

	public MessageBox ShowHintMessage(MessageProvider messageProvider, Vector3 position, float duration = 3f)
	{
		GameObject gameObject = this.ShowMessageBox(this.HintMessage, messageProvider, position, duration);
		return (!gameObject) ? null : gameObject.GetComponentInChildren<MessageBox>();
	}

	public MessageBox ShowMessageBoxB(GameObject messageBoxPrefab, MessageProvider messageProvider, Vector3 position, float duration = 3f)
	{
		if (!Characters.Sein.IsSuspended)
		{
			return null;
		}
		GameObject gameObject = this.ShowMessageBox(messageBoxPrefab, messageProvider, position, duration);
		if (gameObject)
		{
			return gameObject.GetComponentInChildren<MessageBox>();
		}
		return null;
	}

	public MessageBox ShowAreaMessage(MessageProvider messageProvider)
	{
		this.m_currentMessageBox = this.ShowMessageBoxB(this.AreaMessage, messageProvider, Vector3.zero, 3f);
		return this.m_currentMessageBox;
	}

	public MessageBox ShowAbilityMessage(MessageProvider messageProvider, GameObject avatar)
	{
		UI.Hints.HideExistingHint();
		MessageBox messageBox = this.ShowMessageBoxB(this.AbilityMessage, messageProvider, new Vector3(0f, 2f), float.PositiveInfinity);
		if (messageBox && avatar)
		{
			messageBox.SetAvatar(avatar);
		}
		this.m_currentMessageBox = messageBox;
		return messageBox;
	}

	public MessageBox ShowPickupMessage(MessageProvider messageProvider, GameObject avatar)
	{
		UI.Hints.HideExistingHint();
		MessageBox messageBox = this.ShowMessageBoxB(this.PickupMessage, messageProvider, new Vector3(0f, 2f), float.PositiveInfinity);
		if (messageBox && avatar)
		{
			messageBox.SetAvatar(avatar);
		}
		this.m_currentMessageBox = messageBox;
		return messageBox;
	}

	public MessageBox ShowStoryMessage(MessageProvider messageProvider)
	{
		UI.Hints.HideExistingHint();
		MessageBox messageBox = this.ShowMessageBoxB(this.StoryMessage, messageProvider, Vector3.zero, float.PositiveInfinity);
		this.m_currentMessageBox = messageBox;
		return messageBox;
	}

	public MessageBox ShowHelpMessage(MessageProvider messageProvider, GameObject avatar)
	{
		UI.Hints.HideExistingHint();
		MessageBox messageBox = this.ShowMessageBoxB(this.HelpMessage, messageProvider, Vector3.zero, float.PositiveInfinity);
		if (messageBox && avatar)
		{
			messageBox.SetAvatar(avatar);
		}
		return messageBox;
	}

	public GameObject ShowSpiritTreeTextMessage(MessageProvider messageProvider, Vector3 position)
	{
		return this.ShowMessageBox(this.SpiritTreeText, messageProvider, position, 0f);
	}

	public float DefaultDuration;

	public GameObject AreaMessage;

	public GameObject AbilityMessage;

	public GameObject HintMessage;

	public GameObject PickupMessage;

	public GameObject StoryMessage;

	public GameObject HelpMessage;

	public GameObject SpiritTreeText;

	private MessageBox m_currentMessageBox;
}
