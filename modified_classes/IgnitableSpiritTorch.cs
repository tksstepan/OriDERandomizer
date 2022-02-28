using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

// Token: 0x0200053E RID: 1342
public class IgnitableSpiritTorch : SaveSerialize
{
	// Token: 0x06001C71 RID: 7281 RVA: 0x00018D15 File Offset: 0x00016F15
	static IgnitableSpiritTorch()
	{
		IgnitableSpiritTorch.OnLightTorchWithGrenadeEvent = delegate()
		{
		};
	}

	// Token: 0x14000022 RID: 34
	// (add) Token: 0x06001C72 RID: 7282 RVA: 0x00018D43 File Offset: 0x00016F43
	// (remove) Token: 0x06001C73 RID: 7283 RVA: 0x00018D5A File Offset: 0x00016F5A
	public static event Action OnLightTorchWithGrenadeEvent;

	// Token: 0x06001C74 RID: 7284 RVA: 0x00018D71 File Offset: 0x00016F71
	public override void Awake()
	{
		base.Awake();
		this.m_transform = base.transform;
		this.UpdateLightSettings();
		IgnitableSpiritTorch.m_all.Add(this);
	}

	// Token: 0x06001C75 RID: 7285 RVA: 0x00018D96 File Offset: 0x00016F96
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

	// Token: 0x06001C76 RID: 7286 RVA: 0x00018DD4 File Offset: 0x00016FD4
	public override void OnDestroy()
	{
		base.OnDestroy();
		IgnitableSpiritTorch.m_all.Remove(this);
	}

	// Token: 0x06001C77 RID: 7287 RVA: 0x00089378 File Offset: 0x00087578
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

	// Token: 0x06001C78 RID: 7288 RVA: 0x000893FC File Offset: 0x000875FC
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

	// Token: 0x17000470 RID: 1136
	// (get) Token: 0x06001C79 RID: 7289 RVA: 0x00018DE8 File Offset: 0x00016FE8
	public Vector3 Position
	{
		get
		{
			return this.m_transform.position;
		}
	}

	// Token: 0x06001C7A RID: 7290 RVA: 0x00018DF5 File Offset: 0x00016FF5
	public override void Serialize(Archive ar)
	{
		ar.Serialize(ref this.m_isLit);
		if (ar.Reading)
		{
			this.UpdateLightSettings();
		}
	}

	// Token: 0x06001C7B RID: 7291 RVA: 0x00089448 File Offset: 0x00087648
	public void FixedUpdate()
	{
		if (!this.m_isLit && Items.LightTorch && Vector3.Distance(Items.LightTorch.Position, this.Position) < this.TouchRadius)
		{
			this.Light(false);
		}
	}

	// Token: 0x0400197F RID: 6527
	private const int GRENADE_IGNITE_RADIUS = 2;

	// Token: 0x04001980 RID: 6528
	private static List<IgnitableSpiritTorch> m_all = new List<IgnitableSpiritTorch>();

	// Token: 0x04001981 RID: 6529
	public ActionSequence OnLitAction;

	// Token: 0x04001982 RID: 6530
	public GameObject LightSource;

	// Token: 0x04001983 RID: 6531
	public float TouchRadius = 2f;

	// Token: 0x04001984 RID: 6532
	private Transform m_transform;

	// Token: 0x04001985 RID: 6533
	private bool m_isLit;

	// Token: 0x04001986 RID: 6534
	public float LitRadius = 5f;

	// Token: 0x04001987 RID: 6535
	public float UnlitRadius = 2f;

	// Token: 0x04001988 RID: 6536
	public BaseAnimator IgniteAnimator;
}
