using System;
using System.Collections.Generic;
using CatlikeCoding.TextBox;
using UnityEngine;

// Token: 0x020006EC RID: 1772
public class TransparencyAnimator : BaseAnimator
{
	// Token: 0x06002670 RID: 9840 RVA: 0x0001F95E File Offset: 0x0001DB5E
	static TransparencyAnimator()
	{
		bool[] array = new bool[3];
		array[0] = true;
		array[1] = true;
		TransparencyAnimator.s_disableRenderer = array;
	}

	// Token: 0x06002671 RID: 9841 RVA: 0x000AB958 File Offset: 0x000A9B58
	[ContextMenu("Print out renderer data")]
	public void PrintOutRendererData()
	{
		foreach (TransparencyAnimator.RendererData rendererData in this.m_rendererData)
		{
			if (rendererData.Renderer != null)
			{
			}
		}
	}

	// Token: 0x170005F3 RID: 1523
	// (get) Token: 0x06002672 RID: 9842 RVA: 0x000AB9C0 File Offset: 0x000A9BC0
	private int PropertyId
	{
		get
		{
			if (TransparencyAnimator.s_propIds == null)
			{
				TransparencyAnimator.s_propIds = new int[TransparencyAnimator.s_propNames.Length];
				for (int i = 0; i < TransparencyAnimator.s_propNames.Length; i++)
				{
					TransparencyAnimator.s_propIds[i] = Shader.PropertyToID(TransparencyAnimator.s_propNames[i]);
				}
			}
			return TransparencyAnimator.s_propIds[(int)this.Mode];
		}
	}

	// Token: 0x170005F4 RID: 1524
	// (get) Token: 0x06002673 RID: 9843 RVA: 0x0001F996 File Offset: 0x0001DB96
	private bool UseSharedMaterial
	{
		get
		{
			return (this.IsInScene && !this.m_forceUseRendererMaterial) || !Application.isPlaying;
		}
	}

	// Token: 0x06002674 RID: 9844 RVA: 0x0001F9B9 File Offset: 0x0001DBB9
	public new void Awake()
	{
		this.m_forceUseRendererMaterial = (base.GetComponentInChildren<TextBox>() != null);
		base.Awake();
	}

	// Token: 0x06002675 RID: 9845 RVA: 0x0001F9D3 File Offset: 0x0001DBD3
	private bool CanBeAnimated(Renderer r)
	{
		return !(r.sharedMaterial == null) && r.sharedMaterial.HasProperty("_Color") && r.GetComponent<UberGhostTrail>() == null;
	}

	// Token: 0x06002676 RID: 9846 RVA: 0x0001FA0C File Offset: 0x0001DC0C
	public override void CacheOriginals()
	{
		this.m_rendererData.Clear();
		this.m_renderers.Clear();
		this.AddChild(base.transform);
		if (this.AnimateChildren)
		{
			this.AddChildren(base.transform);
		}
	}

	// Token: 0x06002677 RID: 9847 RVA: 0x000ABA20 File Offset: 0x000A9C20
	private void AddChild(Transform child)
	{
		Renderer component = child.GetComponent<Renderer>();
		if (component && this.CanBeAnimated(component) && !this.m_renderers.Contains(component))
		{
			this.m_rendererData.Add(new TransparencyAnimator.RendererData(component, this.PropertyId));
			this.m_renderers.Add(component);
		}
	}

	// Token: 0x06002678 RID: 9848 RVA: 0x000ABA80 File Offset: 0x000A9C80
	private void AddChildren(Transform childTransform)
	{
		int childCount = childTransform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = childTransform.GetChild(i);
			TransparencyAnimator component = child.GetComponent<TransparencyAnimator>();
			if (component != null)
			{
				this.m_childTransparencyAnimators.Add(component);
			}
			else
			{
				CleverMenuItem component2 = child.GetComponent<CleverMenuItem>();
				if (component2 != null && component2.AnimateColors)
				{
					if (this.m_cleverMenuItems == null)
					{
						this.m_cleverMenuItems = new List<CleverMenuItem>();
					}
					this.m_cleverMenuItems.Add(component2);
				}
				this.AddChild(child);
				this.AddChildren(child);
			}
		}
	}

	// Token: 0x06002679 RID: 9849 RVA: 0x000ABB24 File Offset: 0x000A9D24
	public static void Register(Transform child)
	{
		Transform parent = child.parent;
		while (parent)
		{
			TransparencyAnimator component = parent.GetComponent<TransparencyAnimator>();
			if (component && component.AnimateChildren)
			{
				component.ManuallyRegister(child);
				break;
			}
			parent = parent.parent;
		}
	}

	// Token: 0x0600267A RID: 9850 RVA: 0x000ABB78 File Offset: 0x000A9D78
	private void ManuallyRegister(Transform child)
	{
		if (!base.IsInitialized)
		{
			return;
		}
		TransparencyAnimator component = child.GetComponent<TransparencyAnimator>();
		if (component)
		{
			this.m_childTransparencyAnimators.Add(component);
			return;
		}
		CleverMenuItem component2 = child.GetComponent<CleverMenuItem>();
		if (component2 != null && component2.AnimateColors)
		{
			if (this.m_cleverMenuItems == null)
			{
				this.m_cleverMenuItems = new List<CleverMenuItem>();
			}
			this.m_cleverMenuItems.Add(component2);
			return;
		}
		this.AddChild(child);
		this.AddChildren(child);
		this.ApplyTransparency(true);
	}

	// Token: 0x0600267B RID: 9851 RVA: 0x0001FA44 File Offset: 0x0001DC44
	public override void SampleValue(float value, bool forceSample)
	{
		value = base.TimeToAnimationCurveTime(value);
		this.m_opacity = this.AnimationCurve.Evaluate(value);
		this.ApplyTransparency(false);
	}

	// Token: 0x0600267C RID: 9852 RVA: 0x000ABC08 File Offset: 0x000A9E08
	public void ApplyTransparency(bool force = true)
	{
		float finalOpacity = this.FinalOpacity;
		if (!Mathf.Approximately(this.m_lastFinalOpacity, finalOpacity) || force)
		{
			this.m_lastFinalOpacity = finalOpacity;
			for (int i = 0; i < this.m_rendererData.Count; i++)
			{
				this.m_rendererData[i].SetRendererAlpha((int)this.Mode, this.PropertyId, this.UseSharedMaterial, finalOpacity);
			}
			for (int j = 0; j < this.m_childTransparencyAnimators.Count; j++)
			{
				this.m_childTransparencyAnimators[j].SetParentOpacity(finalOpacity);
			}
			if (this.m_cleverMenuItems != null)
			{
				for (int k = 0; k < this.m_cleverMenuItems.Count; k++)
				{
					this.m_cleverMenuItems[k].SetParentOpacity(finalOpacity);
				}
			}
		}
	}

	// Token: 0x0600267D RID: 9853 RVA: 0x0001FA68 File Offset: 0x0001DC68
	public void SetParentOpacity(float opacity)
	{
		if (!Mathf.Approximately(opacity, this.m_parentOpacity))
		{
			this.m_parentOpacity = opacity;
			if (base.IsInitialized)
			{
				this.ApplyTransparency(true);
			}
		}
	}

	// Token: 0x170005F5 RID: 1525
	// (get) Token: 0x0600267E RID: 9854 RVA: 0x0001FA94 File Offset: 0x0001DC94
	public float FinalOpacity
	{
		get
		{
			return this.m_opacity * this.m_parentOpacity;
		}
	}

	// Token: 0x170005F6 RID: 1526
	// (get) Token: 0x0600267F RID: 9855 RVA: 0x0001FAA3 File Offset: 0x0001DCA3
	public override float Duration
	{
		get
		{
			return base.AnimationCurveTimeToTime(this.AnimationCurve.CurveDuration());
		}
	}

	// Token: 0x06002680 RID: 9856 RVA: 0x000ABCE8 File Offset: 0x000A9EE8
	public override void RestoreToOriginalState()
	{
		this.m_parentOpacity = 1f;
		this.m_opacity = 1f;
		for (int i = 0; i < this.m_childTransparencyAnimators.Count; i++)
		{
			this.m_childTransparencyAnimators[i].RestoreToOriginalState();
		}
		for (int j = 0; j < this.m_rendererData.Count; j++)
		{
			this.m_rendererData[j].SetRendererAlpha((int)this.Mode, this.PropertyId, this.UseSharedMaterial, 1f);
		}
	}

	// Token: 0x170005F7 RID: 1527
	// (get) Token: 0x06002681 RID: 9857 RVA: 0x0001FAB6 File Offset: 0x0001DCB6
	public override bool IsLooping
	{
		get
		{
			return this.AnimationCurve.postWrapMode != WrapMode.ClampForever;
		}
	}

	// Token: 0x06002682 RID: 9858 RVA: 0x000ABD80 File Offset: 0x000A9F80
	public void Reset()
	{
		if (this.m_childTransparencyAnimators != null)
		{
			this.m_childTransparencyAnimators.Clear();
		}
		if (this.m_cleverMenuItems != null)
		{
			this.m_cleverMenuItems.Clear();
		}
		if (this.m_rendererData != null)
		{
			this.m_rendererData.Clear();
		}
		if (this.m_renderers != null)
		{
			this.m_renderers.Clear();
		}
	}

	// Token: 0x04002287 RID: 8839
	private static string[] s_propNames = new string[]
	{
		"_Color",
		"_MaskDissolveColor",
		"_AdditiveLayerColor"
	};

	// Token: 0x04002288 RID: 8840
	private static bool[] s_disableRenderer;

	// Token: 0x04002289 RID: 8841
	private static int[] s_propIds;

	// Token: 0x0400228A RID: 8842
	public AnimationCurve AnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	// Token: 0x0400228B RID: 8843
	public bool AnimateChildren;

	// Token: 0x0400228C RID: 8844
	public TransparencyAnimator.AnimateMode Mode;

	// Token: 0x0400228D RID: 8845
	[PooledSafe]
	private readonly List<TransparencyAnimator.RendererData> m_rendererData = new List<TransparencyAnimator.RendererData>(4);

	// Token: 0x0400228E RID: 8846
	[PooledSafe]
	private readonly List<TransparencyAnimator> m_childTransparencyAnimators = new List<TransparencyAnimator>(4);

	// Token: 0x0400228F RID: 8847
	[PooledSafe]
	private List<CleverMenuItem> m_cleverMenuItems;

	// Token: 0x04002290 RID: 8848
	private bool m_forceUseRendererMaterial;

	// Token: 0x04002291 RID: 8849
	private float m_parentOpacity = 1f;

	// Token: 0x04002292 RID: 8850
	private float m_opacity = 1f;

	// Token: 0x04002293 RID: 8851
	[PooledSafe]
	private readonly HashSet<Renderer> m_renderers = new HashSet<Renderer>();

	// Token: 0x04002294 RID: 8852
	private float m_lastFinalOpacity = 123456792f;

	// Token: 0x020006ED RID: 1773
	public enum AnimateMode
	{
		// Token: 0x04002296 RID: 8854
		Color,
		// Token: 0x04002297 RID: 8855
		Dissolve,
		// Token: 0x04002298 RID: 8856
		Additive
	}

	// Token: 0x020006EE RID: 1774
	private struct RendererData
	{
		// Token: 0x06002683 RID: 9859 RVA: 0x000ABDDC File Offset: 0x000A9FDC
		public RendererData(Renderer renderer, int id)
		{
			this.Renderer = renderer;
			this.OriginalAlpha = renderer.sharedMaterial.GetColor(id).a;
		}

		// Token: 0x06002684 RID: 9860 RVA: 0x000ABE0C File Offset: 0x000AA00C
		public void SetRendererAlpha(int mode, int propertyID, bool useSharedMaterial, float value)
		{
			if (this.Renderer == null || this.Renderer.sharedMaterial == null)
			{
				return;
			}
			if (TransparencyAnimator.s_disableRenderer[mode])
			{
				this.Renderer.enabled = (value > 0.01f);
			}
			float a = value * this.OriginalAlpha;
			Material material = (!useSharedMaterial) ? this.Renderer.material : this.Renderer.sharedMaterial;
			Color color = material.GetColor(propertyID);
			color.a = a;
			material.SetColor(propertyID, color);
		}

		// Token: 0x04002299 RID: 8857
		public readonly float OriginalAlpha;

		// Token: 0x0400229A RID: 8858
		public readonly Renderer Renderer;
	}
}
