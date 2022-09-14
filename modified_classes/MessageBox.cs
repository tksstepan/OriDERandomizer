using System;
using System.Collections.Generic;
using System.Linq;
using CatlikeCoding.TextBox;
using Game;
using UnityEngine;

[ExecuteInEditMode]
public class MessageBox : MonoBehaviour
{
	public event Action OnMessageScreenHide = delegate()
	{
	};

	public event Action OnNextMessage = delegate()
	{
	};

	public HashSet<ISuspendable> GetSuspendables()
	{
		HashSet<ISuspendable> hashSet = new HashSet<ISuspendable>();
		foreach (ISuspendable item in base.GetComponentsInChildren(typeof(ISuspendable)))
		{
			hashSet.Add(item);
		}
		return hashSet;
	}

	public void OverrideLanuage(Language language)
	{
		this.m_language = language;
		this.m_forceLanguage = true;
	}

	public void SetAvatar(GameObject avatarPrefab)
	{
		if (this.m_avatar)
		{
			InstantiateUtility.Destroy(this.m_avatar);
			this.m_avatar = null;
		}
		if (avatarPrefab)
		{
			this.m_avatar = UnityEngine.Object.Instantiate<GameObject>(avatarPrefab);
			this.m_avatar.transform.parent = this.Avatar;
			this.m_avatar.transform.localPosition = Vector3.zero;
			this.m_avatar.transform.localRotation = avatarPrefab.transform.localRotation;
			this.m_avatar.transform.localScale = avatarPrefab.transform.localScale;
		}
	}

	public void SetAvatarArray(GameObject[] avatarPrefabs)
	{
		this.m_avatarPrefabs = avatarPrefabs;
	}

	public void HideMessageScreen()
	{
		this.Visibility.HideMessageScreen();
		this.OnMessageScreenHide();
	}

	public void Awake()
	{
		if (Application.isPlaying)
		{
			Events.Scheduler.OnGameLanguageChange.Add(new Action(this.RefreshText));
			Events.Scheduler.OnGameControlSchemeChange.Add(new Action(this.RefreshText));
		}
	}

	public void OnDestroy()
	{
		if (Application.isPlaying)
		{
			Events.Scheduler.OnGameLanguageChange.Remove(new Action(this.RefreshText));
			Events.Scheduler.OnGameControlSchemeChange.Remove(new Action(this.RefreshText));
		}
	}

	public void Start()
	{
		this.RefreshText();
		if (this.WriteOutTextBox)
		{
			this.WriteOutTextBox.GoToStart();
		}
	}

	public void Update()
	{
		if (this.m_previousOverrideText != this.OverrideText)
		{
			this.m_previousOverrideText = this.OverrideText;
			this.RefreshText();
		}
	}

	public void RemoveMessageFade()
	{
		this.SetMessageFade(999999f);
	}

	public void SetMessageFade(float time)
	{
		if (this.TextBox.textRenderers != null)
		{
			foreach (TextRenderer textRenderer in this.TextBox.textRenderers)
			{
				MoonTextMeshRenderer moonTextMeshRenderer = textRenderer as MoonTextMeshRenderer;
				if (moonTextMeshRenderer != null)
				{
					Renderer component = moonTextMeshRenderer.GetComponent<Renderer>();
					if (component)
					{
						float val = time / this.FadeSpread;
						UberShaderAPI.SetFloat(component, val, "_TxtTime", true);
					}
				}
			}
		}
	}

	public void SetMessage(MessageDescriptor messageDescriptor)
	{
		this.MessageProvider = null;
		this.m_messageDescriptors = null;
		this.m_currentMessage = messageDescriptor;
		if (this.FormatText)
		{
			string text = MessageParserUtility.ProcessString(this.m_currentMessage.Message);
			this.TextBox.SetText(text);
		}
		else
		{
			this.TextBox.SetText(this.m_currentMessage.Message);
		}
		this.RefreshText();
	}

	public void RefreshText()
	{
		if (this.m_forceLanguage)
		{
			this.TextBox.SetStyleCollection(this.LanguageStyles.GetStyle(this.m_language));
		}
		else
		{
			this.TextBox.SetStyleCollection(this.LanguageStyles.Current);
		}
		if (this.MessageProvider)
		{
			this.m_messageDescriptors = this.MessageProvider.GetMessages().ToArray<MessageDescriptor>();
			this.MessageIndex = Mathf.Clamp(this.MessageIndex, 0, this.m_messageDescriptors.Length);
			this.m_currentMessage = this.m_messageDescriptors[this.MessageIndex];
			string text = this.m_currentMessage.Message;
			if (text.StartsWith("ALIGNLEFT"))
			{
				this.TextBox.alignment = AlignmentMode.Left;
				text = text.Substring(9);
			}
			else if (text.StartsWith("ALIGNRIGHT"))
			{
				this.TextBox.alignment = AlignmentMode.Right;
				text = text.Substring(10);
			}
			if (text.StartsWith("ANCHORTOP"))
			{
				this.TextBox.verticalAnchor = VerticalAnchorMode.Top;
				text = text.Substring(9);
			} else if  (text.StartsWith("ANCHORBOT"))
			{
				this.TextBox.verticalAnchor = VerticalAnchorMode.Bottom;
				text = text.Substring(9);
			} 
			if (text.StartsWith("ANCHORLEFT"))
			{
				this.TextBox.horizontalAnchor = HorizontalAnchorMode.Left;
				text = text.Substring(10);
			} else if  (text.StartsWith("ANCHORRIGHT"))
			{
				this.TextBox.horizontalAnchor = HorizontalAnchorMode.Right;
				text = text.Substring(11);
			} 
			if (text.StartsWith("PADDING"))
			{
				Queue<string> p = new Queue<string>(text.Split(new char[]{'_'}));
				p.Dequeue();
				this.TextBox.paddingBottom = float.Parse(p.Dequeue());
				this.TextBox.paddingLeft = float.Parse(p.Dequeue());
				this.TextBox.paddingRight = float.Parse(p.Dequeue());
				this.TextBox.paddingTop = float.Parse(p.Dequeue());
				text = string.Join("_", p.ToArray());
			}
			if (text.StartsWith("PARAMS"))
			{
				Queue<string> p = new Queue<string>(text.Split(new char[]{'_'}));
				p.Dequeue();
				this.TextBox.maxHeight = float.Parse(p.Dequeue());
				this.TextBox.width = float.Parse(p.Dequeue());
				this.TextBox.TabSize = float.Parse(p.Dequeue());
				text = string.Join("_", p.ToArray());
			}
			if (text.StartsWith("SHOWINFO"))
			{
				text = string.Concat(new string[]
				{
					text.Substring(8),
					"\nHeight: ",
					this.TextBox.maxHeight.ToString(),
					" width: ",
					this.TextBox.width.ToString(),
					"TabSize ",
					this.TextBox.size.ToString(),
					"\n Anchors ",
					this.TextBox.horizontalAnchor.ToString(),
					" ",
					this.TextBox.verticalAnchor.ToString(),
					"\nPadding: ",
					this.TextBox.paddingBottom.ToString(),
					"/",
					this.TextBox.paddingLeft.ToString(),
					"/",
					this.TextBox.paddingRight.ToString(),
					"/",
					this.TextBox.paddingTop.ToString()
				});
			}
			if (this.FormatText)
			{
				text = MessageParserUtility.ProcessString(text);
				this.TextBox.SetText(text);
			}
			else
			{
				this.TextBox.SetText(text);
			}
		}
		else if (this.OverrideText != string.Empty)
		{
			if (this.FormatText)
			{
				this.TextBox.SetText(MessageParserUtility.ProcessString(this.OverrideText));
			}
			else
			{
				this.TextBox.SetText(this.OverrideText);
			}
		}
		this.TextBox.CreateRendersIfThereAreNone();
		TextRenderer[] textRenderers = this.TextBox.textRenderers;
		for (int i = 0; i < textRenderers.Length; i++)
		{
			MoonTextMeshRenderer moonTextMeshRenderer = textRenderers[i] as MoonTextMeshRenderer;
			if (moonTextMeshRenderer)
			{
				moonTextMeshRenderer.FadeSpread = this.FadeSpread;
			}
		}
		this.TextBox.size = this.ScaleOverLetterCount.Evaluate((float)TextBoxExtended.CountLetters(this.TextBox));
		this.TextBox.RenderText();
		if (this.WriteOutTextBox)
		{
			this.WriteOutTextBox.OnTextChange();
		}
		else
		{
			this.RemoveMessageFade();
		}
		if (this.m_avatarPrefabs != null)
		{
			this.SetAvatar(this.m_avatarPrefabs[this.MessageIndex]);
		}
		if (!Application.isPlaying)
		{
			this.RemoveMessageFade();
		}
	}

	public void OnEnable()
	{
		if (!Application.isPlaying)
		{
			this.RemoveMessageFade();
		}
	}

	public void SetMessageProvider(MessageProvider messageProvider)
	{
		this.MessageProvider = messageProvider;
		this.RefreshText();
	}

	public int MessageCount
	{
		get
		{
			if (this.m_messageDescriptors == null)
			{
				return 1;
			}
			return this.m_messageDescriptors.Length;
		}
	}

	public void SetWaitDuration(float duration)
	{
		this.Visibility.WaitDuration = duration;
	}

	public EmotionType CurrentEmotion
	{
		get
		{
			return this.m_currentMessage.Emotion;
		}
	}

	public SoundProvider CurrentMessageSound
	{
		get
		{
			return this.m_currentMessage.Sound;
		}
	}

	public void FinishWriting()
	{
		if (this.WriteOutTextBox)
		{
			this.WriteOutTextBox.AnimatorDriver.GoToEnd();
		}
	}

	public bool IsLastMessage
	{
		get
		{
			return this.m_messageDescriptors == null || this.MessageIndex == this.m_messageDescriptors.Length - 1;
		}
	}

	public bool FinishedWriting
	{
		get
		{
			return this.WriteOutTextBox == null || this.WriteOutTextBox.AtEnd;
		}
	}

	public void NextMessage()
	{
		this.MessageIndex++;
		this.RefreshText();
		if (this.WriteOutTextBox)
		{
			this.WriteOutTextBox.GoToStart();
		}
		this.OnNextMessage();
		if (this.NextMessageAnimator)
		{
			this.NextMessageAnimator.AnimatorDriver.Restart();
		}
	}

	public const float WaitTimeBetweenMessages = 0.3f;

	public MessageBoxLanguageStyles LanguageStyles;

	public WriteOutTextBox WriteOutTextBox;

	public MessageBoxVisibility Visibility;

	public TextBox TextBox;

	public Transform Avatar;

	public int MessageIndex;

	public MessageProvider MessageProvider;

	public AnimationCurve ScaleOverLetterCount = AnimationCurve.Linear(0f, 1f, 150f, 1f);

	private float m_remainingWaitTime;

	private GameObject m_avatar;

	private GameObject[] m_avatarPrefabs;

	public BaseAnimator NextMessageAnimator;

	public bool FormatText = true;

	private bool m_forceLanguage;

	private Language m_language;

	public float FadeSpread = 5f;

	public string OverrideText;

	private string m_previousOverrideText = string.Empty;

	private MessageDescriptor[] m_messageDescriptors;

	private MessageDescriptor m_currentMessage;
}
