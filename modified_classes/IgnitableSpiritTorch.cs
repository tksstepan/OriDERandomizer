using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class IgnitableSpiritTorch : SaveSerialize
{
	static IgnitableSpiritTorch()
	{
		IgnitableSpiritTorch.OnLightTorchWithGrenadeEvent = delegate()
		{
		};
	}

	public static event Action OnLightTorchWithGrenadeEvent;

	public override void Awake()
	{
		base.Awake();
		this.m_transform = base.transform;
		this.UpdateLightSettings();
		IgnitableSpiritTorch.m_all.Add(this);
	}

	public void UpdateLightSettings()
	{
		if (this.m_isLit)
		{
			this.LightSource.GetComponent<SpiritLightRadialVisualAffector>().Radius = this.LitRadius;
		}
		else
		{
			this.LightSource.GetComponent<SpiritLightRadialVisualAffector>().Radius = this.UnlitRadius;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		IgnitableSpiritTorch.m_all.Remove(this);
	}

	public static IgnitableSpiritTorch IgniteAnyTorchesNearPosition(Vector3 position)
	{
		foreach (IgnitableSpiritTorch ignitableSpiritTorch in IgnitableSpiritTorch.m_all)
		{
			if (!ignitableSpiritTorch.m_isLit && Vector3.Distance(ignitableSpiritTorch.Position, position) < 2f)
			{
				ignitableSpiritTorch.Light(true);
				return ignitableSpiritTorch;
			}
		}
		return null;
	}

	public void Light(bool byGrenade)
	{
		BingoController.OnLanternLit(this.MoonGuid, byGrenade);
		this.m_isLit = true;
		if (this.OnLitAction)
		{
			this.OnLitAction.Perform(null);
		}
		this.UpdateLightSettings();
		if (byGrenade)
		{
			IgnitableSpiritTorch.OnLightTorchWithGrenadeEvent();
		}
	}

	public Vector3 Position
	{
		get
		{
			return this.m_transform.position;
		}
	}

	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_isLit);
		if (ar.Reading)
		{
			this.UpdateLightSettings();
		}
	}

	public void FixedUpdate()
	{
		if (!this.m_isLit && Items.LightTorch && Vector3.Distance(Items.LightTorch.Position, this.Position) < this.TouchRadius)
		{
			this.Light(false);
		}
	}

	private const int GRENADE_IGNITE_RADIUS = 2;

	private static List<IgnitableSpiritTorch> m_all = new List<IgnitableSpiritTorch>();

	public ActionSequence OnLitAction;

	public GameObject LightSource;

	public float TouchRadius = 2f;

	private Transform m_transform;

	private bool m_isLit;

	public float LitRadius = 5f;

	public float UnlitRadius = 2f;

	public BaseAnimator IgniteAnimator;
}
