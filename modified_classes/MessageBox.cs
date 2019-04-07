using System;
using System.Collections.Generic;
using System.Linq;
using CatlikeCoding.TextBox;
using Game;
using UnityEngine;

// Token: 0x0200056F RID: 1391
[ExecuteInEditMode]
public class MessageBox : MonoBehaviour
{
	// Token: 0x14000023 RID: 35
	// (add) Token: 0x06001D4C RID: 7500 RVA: 0x000195CC File Offset: 0x000177CC
	// (remove) Token: 0x06001D4D RID: 7501 RVA: 0x000195E5 File Offset: 0x000177E5
	public event Action OnMessageScreenHide = delegate()
	{
	};

	// Token: 0x14000024 RID: 36
	// (add) Token: 0x06001D4E RID: 7502 RVA: 0x000195FE File Offset: 0x000177FE
	// (remove) Token: 0x06001D4F RID: 7503 RVA: 0x00019617 File Offset: 0x00017817
	public event Action OnNextMessage = delegate()
	{
	};

	// Token: 0x06001D50 RID: 7504 RVA: 0x0008BD20 File Offset: 0x00089F20
	public HashSet<ISuspendable> GetSuspendables()
	{
		HashSet<ISuspendable> hashSet = new HashSet<ISuspendable>();
		foreach (ISuspendable item in base.GetComponentsInChildren(typeof(ISuspendable)))
		{
			hashSet.Add(item);
		}
		return hashSet;
	}

	// Token: 0x06001D51 RID: 7505 RVA: 0x00019630 File Offset: 0x00017830
	public void OverrideLanuage(Language language)
	{
		this.m_language = language;
		this.m_forceLanguage = true;
	}

	// Token: 0x06001D52 RID: 7506 RVA: 0x0008BD6C File Offset: 0x00089F6C
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

	// Token: 0x06001D53 RID: 7507 RVA: 0x00019640 File Offset: 0x00017840
	public void SetAvatarArray(GameObject[] avatarPrefabs)
	{
		this.m_avatarPrefabs = avatarPrefabs;
	}

	// Token: 0x06001D54 RID: 7508 RVA: 0x00019649 File Offset: 0x00017849
	public void HideMessageScreen()
	{
		this.Visibility.HideMessageScreen();
		this.OnMessageScreenHide();
	}

	// Token: 0x06001D55 RID: 7509 RVA: 0x0008BE14 File Offset: 0x0008A014
	public void Awake()
	{
		if (Application.isPlaying)
		{
			Events.Scheduler.OnGameLanguageChange.Add(new Action(this.RefreshText));
			Events.Scheduler.OnGameControlSchemeChange.Add(new Action(this.RefreshText));
		}
	}

	// Token: 0x06001D56 RID: 7510 RVA: 0x0008BE64 File Offset: 0x0008A064
	public void OnDestroy()
	{
		if (Application.isPlaying)
		{
			Events.Scheduler.OnGameLanguageChange.Remove(new Action(this.RefreshText));
			Events.Scheduler.OnGameControlSchemeChange.Remove(new Action(this.RefreshText));
		}
	}

	// Token: 0x06001D57 RID: 7511 RVA: 0x00019661 File Offset: 0x00017861
	public void Start()
	{
		this.RefreshText();
		if (this.WriteOutTextBox)
		{
			this.WriteOutTextBox.GoToStart();
		}
	}

	// Token: 0x06001D58 RID: 7512 RVA: 0x00019681 File Offset: 0x00017881
	public void Update()
	{
		if (this.m_previousOverrideText != this.OverrideText)
		{
			this.m_previousOverrideText = this.OverrideText;
			this.RefreshText();
		}
	}

	// Token: 0x06001D59 RID: 7513 RVA: 0x000196AB File Offset: 0x000178AB
	public void RemoveMessageFade()
	{
		this.SetMessageFade(999999f);
	}

	// Token: 0x06001D5A RID: 7514 RVA: 0x0008BEB4 File Offset: 0x0008A0B4
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

	// Token: 0x06001D5B RID: 7515 RVA: 0x0008BF38 File Offset: 0x0008A138
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

	// Token: 0x06001D5C RID: 7516
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

	// Token: 0x06001D5D RID: 7517 RVA: 0x000196B8 File Offset: 0x000178B8
	public void OnEnable()
	{
		if (!Application.isPlaying)
		{
			this.RemoveMessageFade();
		}
	}

	// Token: 0x06001D5E RID: 7518 RVA: 0x000196CA File Offset: 0x000178CA
	public void SetMessageProvider(MessageProvider messageProvider)
	{
		this.MessageProvider = messageProvider;
		this.RefreshText();
	}

	// Token: 0x17000493 RID: 1171
	// (get) Token: 0x06001D5F RID: 7519 RVA: 0x000196D9 File Offset: 0x000178D9
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

	// Token: 0x06001D60 RID: 7520 RVA: 0x000196F0 File Offset: 0x000178F0
	public void SetWaitDuration(float duration)
	{
		this.Visibility.WaitDuration = duration;
	}

	// Token: 0x17000494 RID: 1172
	// (get) Token: 0x06001D61 RID: 7521 RVA: 0x000196FE File Offset: 0x000178FE
	public EmotionType CurrentEmotion
	{
		get
		{
			return this.m_currentMessage.Emotion;
		}
	}

	// Token: 0x17000495 RID: 1173
	// (get) Token: 0x06001D62 RID: 7522 RVA: 0x0001970B File Offset: 0x0001790B
	public SoundProvider CurrentMessageSound
	{
		get
		{
			return this.m_currentMessage.Sound;
		}
	}

	// Token: 0x06001D63 RID: 7523 RVA: 0x00019718 File Offset: 0x00017918
	public void FinishWriting()
	{
		if (this.WriteOutTextBox)
		{
			this.WriteOutTextBox.AnimatorDriver.GoToEnd();
		}
	}

	// Token: 0x17000496 RID: 1174
	// (get) Token: 0x06001D64 RID: 7524 RVA: 0x0001973A File Offset: 0x0001793A
	public bool IsLastMessage
	{
		get
		{
			return this.m_messageDescriptors == null || this.MessageIndex == this.m_messageDescriptors.Length - 1;
		}
	}

	// Token: 0x17000497 RID: 1175
	// (get) Token: 0x06001D65 RID: 7525 RVA: 0x0001975B File Offset: 0x0001795B
	public bool FinishedWriting
	{
		get
		{
			return this.WriteOutTextBox == null || this.WriteOutTextBox.AtEnd;
		}
	}

	// Token: 0x06001D66 RID: 7526 RVA: 0x0008C1B4 File Offset: 0x0008A3B4
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

	// Token: 0x04001A99 RID: 6809
	public const float WaitTimeBetweenMessages = 0.3f;

	// Token: 0x04001A9A RID: 6810
	public MessageBoxLanguageStyles LanguageStyles;

	// Token: 0x04001A9B RID: 6811
	public WriteOutTextBox WriteOutTextBox;

	// Token: 0x04001A9C RID: 6812
	public MessageBoxVisibility Visibility;

	// Token: 0x04001A9D RID: 6813
	public TextBox TextBox;

	// Token: 0x04001A9E RID: 6814
	public Transform Avatar;

	// Token: 0x04001A9F RID: 6815
	public int MessageIndex;

	// Token: 0x04001AA0 RID: 6816
	public MessageProvider MessageProvider;

	// Token: 0x04001AA1 RID: 6817
	public AnimationCurve ScaleOverLetterCount = AnimationCurve.Linear(0f, 1f, 150f, 1f);

	// Token: 0x04001AA2 RID: 6818
	private float m_remainingWaitTime;

	// Token: 0x04001AA3 RID: 6819
	private GameObject m_avatar;

	// Token: 0x04001AA4 RID: 6820
	private GameObject[] m_avatarPrefabs;

	// Token: 0x04001AA5 RID: 6821
	public BaseAnimator NextMessageAnimator;

	// Token: 0x04001AA6 RID: 6822
	public bool FormatText = true;

	// Token: 0x04001AA7 RID: 6823
	private bool m_forceLanguage;

	// Token: 0x04001AA8 RID: 6824
	private Language m_language;

	// Token: 0x04001AA9 RID: 6825
	public float FadeSpread = 5f;

	// Token: 0x04001AAA RID: 6826
	public string OverrideText;

	// Token: 0x04001AAB RID: 6827
	private string m_previousOverrideText = string.Empty;

	// Token: 0x04001AAC RID: 6828
	private MessageDescriptor[] m_messageDescriptors;

	// Token: 0x04001AAD RID: 6829
	private MessageDescriptor m_currentMessage;
}
