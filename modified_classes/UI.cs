using System;
using UnityEngine;

namespace Game
{
	public static class UI
	{
		public static MessageControllerB MessageController
		{
			get
			{
				UI.LoadMessageController();
				return UI.m_messageController;
			}
		}

		public static void LoadMessageController()
		{
			if (UI.m_messageController == null)
			{
				UI.m_messageController = (Resources.Load("MessageControllerB") as GameObject).GetComponent<MessageControllerB>();
			}
		}

		public static MenuScreenManager Menu
		{
			get
			{
				return UI.m_sMenu;
			}
			set
			{
				UI.m_sMenu = value;
			}
		}

		public static bool MainMenuVisible
		{
			get
			{
				return UI.m_sMenu != null && (UI.m_sMenu.MainMenuVisible || UI.m_sMenu.ResumeScreenVisible);
			}
		}

		public static bool MainMenuExists
		{
			get
			{
				return UI.m_sMenu != null;
			}
		}

		public static bool IsInventoryVisible()
		{
			return UI.MainMenuVisible && UI.m_sMenu.IsInventoryVisible();
		}

		private static MessageControllerB m_messageController;

		public static FaderB Fader;

		public static SeinUI SeinUI;

		private static MenuScreenManager m_sMenu;

		public static Vignette Vignette;

		public static class Cameras
		{
			public static CameraSystem System;

			public static GameplayCamera Current;

			public static CameraManager Manager;
		}

		public static class Hints
		{
			public static Vector3 HintPosition
			{
				get
				{
					return OnScreenPositions.TopCenter;
				}
			}

			public static void HideExistingHint()
			{
				UI.Hints.HideExistingHint(false);
			}

			private static bool LayerShouldShow(HintLayer layer)
			{
				return !UI.Hints.m_currentHint || layer >= UI.Hints.m_currentLayer;
			}

			public static MessageBox Show(MessageProvider messageProvider, HintLayer layer, float duration = 3f)
			{
				if (messageProvider == null)
				{
					return null;
				}
				if (UI.MessageController.AnyAbilityPickupStoryMessagesVisible)
				{
					return null;
				}
				if (UI.Hints.LayerShouldShow(layer))
				{
					UI.Hints.HideExistingHint(true);
					UI.Hints.m_currentLayer = layer;
					if (ShorterHintZone.IsInside)
					{
						duration = 1f;
					}
					if (layer == HintLayer.Randomizer)
					{
						UI.Hints.m_currentHint = UI.MessageController.ShowHintMessage(messageProvider, new Vector3(UI.Hints.HintPosition.x, UI.Hints.HintPosition.y, -7f), duration);
					}
					else
					{
						UI.Hints.m_currentHint = UI.MessageController.ShowHintMessage(messageProvider, UI.Hints.HintPosition, duration);
					}
					return UI.Hints.m_currentHint;
				}
				return null;
			}

			public static bool IsShowingHint
			{
				get
				{
					return UI.Hints.m_currentHint;
				}
			}

			public static void HideExistingHint(bool force)
			{
				if (UI.Hints.m_currentLayer == HintLayer.Randomizer && !force)
				{
					return;
				}

				if (UI.Hints.m_currentHint)
				{
					UI.Hints.m_currentHint.Visibility.HideMessageScreenImmediately();
					UI.Hints.m_currentHint = null;
				}
			}

			private static MessageBox m_currentHint;

			private static HintLayer m_currentLayer;

			private static bool m_showHints;
		}
	}
}
