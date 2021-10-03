using System;
using System.Collections.Generic;
using CatlikeCoding.TextBox;
using UnityEngine;

public class TransparencyAnimator : BaseAnimator
{
	static TransparencyAnimator()
	{
		bool[] array = new bool[3];
		array[0] = true;
		array[1] = true;
		TransparencyAnimator.s_disableRenderer = array;
	}

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

	private bool UseSharedMaterial
	{
		get
		{
			return (this.IsInScene && !this.m_forceUseRendererMaterial) || !Application.isPlaying;
		}
	}

	public new void Awake()
	{
		this.m_forceUseRendererMaterial = (base.GetComponentInChildren<TextBox>() != null);
		base.Awake();
	}

	private bool CanBeAnimated(Renderer r)
	{
		return !(r.sharedMaterial == null) && r.sharedMaterial.HasProperty("_Color") && r.GetComponent<UberGhostTrail>() == null;
	}

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

	private void AddChild(Transform child)
	{
		Renderer component = child.GetComponent<Renderer>();
		if (component && this.CanBeAnimated(component) && !this.m_renderers.Contains(component))
		{
			this.m_rendererData.Add(new TransparencyAnimator.RendererData(component, this.PropertyId));
			this.m_renderers.Add(component);
		}
	}

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

	public override void SampleValue(float value, bool forceSample)
	{
		value = base.TimeToAnimationCurveTime(value);
		this.m_opacity = this.AnimationCurve.Evaluate(value);
		this.ApplyTransparency(false);
	}

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

	public float FinalOpacity
	{
		get
		{
			return this.m_opacity * this.m_parentOpacity;
		}
	}

	public override float Duration
	{
		get
		{
			return base.AnimationCurveTimeToTime(this.AnimationCurve.CurveDuration());
		}
	}

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

	public override bool IsLooping
	{
		get
		{
			return this.AnimationCurve.postWrapMode != WrapMode.ClampForever;
		}
	}

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

	private static string[] s_propNames = new string[]
	{
		"_Color",
		"_MaskDissolveColor",
		"_AdditiveLayerColor"
	};

	private static bool[] s_disableRenderer;

	private static int[] s_propIds;

	public AnimationCurve AnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public bool AnimateChildren;

	public TransparencyAnimator.AnimateMode Mode;

	[PooledSafe]
	private readonly List<TransparencyAnimator.RendererData> m_rendererData = new List<TransparencyAnimator.RendererData>(4);

	[PooledSafe]
	private readonly List<TransparencyAnimator> m_childTransparencyAnimators = new List<TransparencyAnimator>(4);

	[PooledSafe]
	private List<CleverMenuItem> m_cleverMenuItems;

	private bool m_forceUseRendererMaterial;

	private float m_parentOpacity = 1f;

	private float m_opacity = 1f;

	[PooledSafe]
	private readonly HashSet<Renderer> m_renderers = new HashSet<Renderer>();

	private float m_lastFinalOpacity = 123456792f;

	public enum AnimateMode
	{
		Color,
		Dissolve,
		Additive
	}

	private struct RendererData
	{
		public RendererData(Renderer renderer, int id)
		{
			this.Renderer = renderer;
			this.OriginalAlpha = renderer.sharedMaterial.GetColor(id).a;
		}

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

		public readonly float OriginalAlpha;

		public readonly Renderer Renderer;
	}
}
