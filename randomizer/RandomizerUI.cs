using System;
using System.Collections.Generic;
using CatlikeCoding.TextBox;
using Core;
using Game;
using UnityEngine;

public class RandomizerUI : MonoBehaviour
{
	public static void Initialize()
	{
		RandomizerUI.Instance = new GameObject("randomizerUI").AddComponent<RandomizerUI>();
	}

	public void Awake()
	{
		// deactivating the existing prefab before cloning it prevents the clone from doing Awake/Start/etc.
		// this gives us a chance to do setup before things get locked in
		bool wasActive = UI.MessageController.HintMessage.activeSelf;
		UI.MessageController.HintMessage.SetActive(false);
		GameObject obj = (GameObject)InstantiateUtility.Instantiate(UI.MessageController.HintMessage);
		UI.MessageController.HintMessage.SetActive(wasActive);
		Message.SideNotificationPrefab = obj;

		// nb: removing the DestroyOnRestoreCheckpoint component allows notifs to stick around through S&Q
		obj.name = "pickupMessage";
		obj.transform.parent = this.transform;
		UnityEngine.Object.Destroy(obj.GetComponent<DestroyOnRestoreCheckpoint>());
		UnityEngine.Object.Destroy(obj.GetComponent<SoundSource>());

		// position adjustment on the textbox moves it forward (-Z) so that the text + bg render on top of the world map
		MessageBox messageBox = obj.GetComponentInChildren<MessageBox>();
		messageBox.transform.localScale *= 0.9f;
		messageBox.TextBox.transform.localPosition = new Vector3(0f, 0f, -1f);
		messageBox.TextBox.alignment = AlignmentMode.Left;
		messageBox.TextBox.horizontalAnchor = HorizontalAnchorMode.Left;
		messageBox.TextBox.verticalAnchor = VerticalAnchorMode.Top;

		messageBox.Visibility.TransitionInDuration = 0.2f;
		messageBox.Visibility.TransitionOutDuration = 0.15f;

		ScaleToTextBox scaleToTextBox = messageBox.GetComponentInChildren<ScaleToTextBox>();			
		scaleToTextBox.TopLeftPadding = new Vector2(0.5f, 0.15f);
		scaleToTextBox.BottomRightPadding = new Vector2(0.5f, 0.15f);
	}

	public void OnGUI()
	{

		if (DebugMenuB.DebugControlsEnabled && Characters.Sein != null && Characters.Sein.Active)
		{
			GUIStyle infoStyle = new GUIStyle();
			infoStyle.fontSize = 16;
			infoStyle.alignment = TextAnchor.LowerLeft;
			infoStyle.normal.textColor = Color.white;
			Camera camera = UI.Cameras.Current.Camera;
			Vector2 cursorPosition = Core.Input.CursorPosition;
			Vector2 cursorWorldPos = camera.ViewportToWorldPoint(new Vector3(cursorPosition.x, cursorPosition.y, -camera.transform.position.z));
			string text = string.Format("Ori (World) X: {0} / Y: {1}\nCursor (World) X {2} / Y: {3}", new object[]
			{
				Characters.Sein.Position.x,
				Characters.Sein.Position.y,
				cursorWorldPos.x,
				cursorWorldPos.y
			});
			GUI.Label(new Rect(4f, GameSettings.Instance.Resolution.y - 54f, 200f, 50f), text, infoStyle);
		}
	}

	public void Update()
	{
		if (this.m_sideNotificationsDisplaying.Count == 0 && this.m_sideNotificationsAwaiting.Count == 0)
		{
			return;
		}

		bool updateDisplay = false;

		// message objects destroy themselves automatically when their time elapses, so we have to clean up after them
		int i = 0;
		while (i < this.m_sideNotificationsDisplaying.Count)
		{
			if (!this.m_sideNotificationsDisplaying[i].MessageBox)
			{
				this.m_sideNotificationsDisplaying.RemoveAt(i);
				updateDisplay = true;
			}
			else
			{
				i++;
			}
		}

		while (this.m_sideNotificationsAwaiting.Count > 0 && (this.m_sideNotificationsDisplaying.Count < 5 || this.m_extendedAltTShown))
		{
			Message nextMessage = this.m_sideNotificationsAwaiting.Dequeue();
			this.m_sideNotificationsDisplaying.Add(nextMessage);
			this.m_recentSideNotifications.Enqueue(nextMessage);
			updateDisplay = true;
		}

		if (this.m_sideNotificationsDisplaying.Count > 5)
		{
			for (int j = 0; j < this.m_sideNotificationsDisplaying.Count - 5; j++)
			{
				if (this.m_sideNotificationsDisplaying[j].MessageBox != null)
				{
					this.m_sideNotificationsDisplaying[j].MessageBox.Visibility.HideMessageScreenImmediately();
				}
			}

			this.m_sideNotificationsDisplaying.RemoveRange(0, this.m_sideNotificationsDisplaying.Count - 5);
		}

		while (this.m_recentSideNotifications.Count > 5)
		{
			this.m_recentSideNotifications.Dequeue();
		}

		if (updateDisplay)
		{
			float nextY = 2.2f;
			foreach (Message displayingMessage in this.m_sideNotificationsDisplaying)
			{
				if (!displayingMessage.MessageBox)
				{
					displayingMessage.Instantiate();
				}

				displayingMessage.MessageBox.transform.position = new Vector3(-5.7f, nextY, 0f);
				float scaledHeight = displayingMessage.MessageBox.TextBox.boundsTop - displayingMessage.MessageBox.TextBox.boundsBottom;
				scaledHeight *= displayingMessage.MessageBox.TextBox.transform.lossyScale.y;
				nextY -= (scaledHeight + 0.35f);
			}
		}
	}

	public void FixedUpdate()
	{
		bool queueEnabled = RandomizerSettings.Customization.MultiplePickupMessages;
		bool alwaysShowLastFive = RandomizerSettings.Customization.AlwaysShowLastFivePickups;

		// in any case where "hold alt+T" would show nothing, replay last message OnPressed to preserve snappy response
		if (RandomizerRebinding.ReplayMessage.OnPressed && (!queueEnabled || alwaysShowLastFive || this.m_recentSideNotifications.Count == 0))
		{
			Randomizer.playLastMessage();
		}

		if (!queueEnabled)
		{
			return;
		}

		if (alwaysShowLastFive)
		{
			this.m_extendedAltTShown = true;
		}

		if (RandomizerRebinding.ReplayMessage.Pressed)
		{
			this.m_timeAltTHeld += Time.deltaTime;

			if (this.m_timeAltTHeld >= 0.2f && !this.m_extendedAltTShown)
			{
				foreach (Message displayingMessage in this.m_sideNotificationsDisplaying)
				{
					if (!this.m_recentSideNotifications.Contains(displayingMessage))
					{
						displayingMessage.MessageBox.Visibility.HideMessageScreenImmediately();
					}
				}

				this.m_sideNotificationsDisplaying.Clear();
				this.m_sideNotificationsDisplaying.AddRange(this.m_recentSideNotifications.ToArray());
				this.m_extendedAltTShown = true;

				float nextY = 2.2f;
				foreach (Message displayingMessage in this.m_sideNotificationsDisplaying)
				{
					if (!displayingMessage.MessageBox)
					{
						displayingMessage.Instantiate();
					}

					displayingMessage.MessageBox.transform.position = new Vector3(-5.7f, nextY, 0f);
					float scaledHeight = displayingMessage.MessageBox.TextBox.boundsTop - displayingMessage.MessageBox.TextBox.boundsBottom;
					scaledHeight *= displayingMessage.MessageBox.TextBox.transform.lossyScale.y;
					nextY -= (scaledHeight + 0.35f);
				}
			}
		}

		if (this.m_extendedAltTShown)
		{
			foreach (Message displayingMessage in this.m_sideNotificationsDisplaying)
			{
				displayingMessage.MessageBox.Visibility.ResetWaitDuration();
			}
		}

		// only replay message OnReleased if we know that OnPressed would not have handled it (i.e. "hold alt+T" would show something)
		if (RandomizerRebinding.ReplayMessage.OnReleased && !this.m_extendedAltTShown && this.m_recentSideNotifications.Count > 0)
		{
			Randomizer.playLastMessage();
		}

		if (RandomizerRebinding.ReplayMessage.Released)
		{
			this.m_timeAltTHeld = 0f;

			if (this.m_extendedAltTShown && !alwaysShowLastFive)
			{
				this.m_extendedAltTShown = false;

				foreach (Message displayingMessage in this.m_sideNotificationsDisplaying)
				{
					displayingMessage.MessageBox.SetWaitDuration(3f);
					displayingMessage.MessageBox.Visibility.ResetWaitDuration();
				}
			}
		}
	}

	public void QueueSideNotification(string message, float duration = 5f)
	{
		this.m_sideNotificationsAwaiting.Enqueue(new Message{ MessageString = message, Duration = duration });
	}

	public void ClearRecentNotifications()
	{
		this.m_recentSideNotifications.Clear();

		foreach (Message displayingMessage in this.m_sideNotificationsDisplaying)
		{
			if (displayingMessage.MessageBox)
			{
				displayingMessage.MessageBox.SetWaitDuration(3f);
				displayingMessage.MessageBox.Visibility.ResetWaitDuration();
			}
		}
	}

	public static RandomizerUI Instance;

	private List<Message> m_sideNotificationsDisplaying = new List<Message>();

	private Queue<Message> m_sideNotificationsAwaiting = new Queue<Message>();

	private Queue<Message> m_recentSideNotifications = new Queue<Message>();

	private float m_timeAltTHeld;

	private bool m_extendedAltTShown;

	private class Message
	{
		public void Instantiate()
		{
			GameObject obj = (GameObject)InstantiateUtility.Instantiate(Message.SideNotificationPrefab);
			obj.transform.parent = Message.SideNotificationPrefab.transform.parent;
			obj.SetActive(true);

			this.MessageBox = obj.GetComponentInChildren<MessageBox>();
			RandomizerMessageProvider messageProvider = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
			messageProvider.SetMessage(this.MessageString);
			this.MessageBox.SetMessageProvider(messageProvider);

			if (this.MessageBox.Visibility)
			{
				this.MessageBox.SetWaitDuration(this.Duration);
			}
		}

		public static GameObject SideNotificationPrefab;

		public string MessageString;

		public float Duration = 5f;

		public MessageBox MessageBox;
	}
}