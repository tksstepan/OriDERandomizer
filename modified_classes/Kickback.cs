using System;
using UnityEngine;

// Token: 0x02000436 RID: 1078
[Serializable]
public class Kickback
{
	// Token: 0x17000417 RID: 1047
	// (get) Token: 0x06001809 RID: 6153 RVA: 0x0007C110 File Offset: 0x0007A310
	public float KickbackDuration
	{
		get
		{
			return this.KickbackCurve[this.KickbackCurve.length - 1].time;
		}
	}

	// Token: 0x17000418 RID: 1048
	// (get) Token: 0x0600180A RID: 6154 RVA: 0x000153BC File Offset: 0x000135BC
	// (set) Token: 0x0600180B RID: 6155 RVA: 0x000153C4 File Offset: 0x000135C4
	public Vector2 KickbackDirection { get; private set; }

	// Token: 0x17000419 RID: 1049
	// (get) Token: 0x0600180C RID: 6156 RVA: 0x000153CD File Offset: 0x000135CD
	public float CurrentKickbackSpeed
	{
		get
		{
			if (this.m_kickbackTimeRemaining <= 0f)
			{
				return 0f;
			}
			return this.m_kickbackMultiplier * this.KickbackCurve.Evaluate(this.KickbackDuration - this.m_kickbackTimeRemaining);
		}
	}

	// Token: 0x1700041A RID: 1050
	// (get) Token: 0x0600180D RID: 6157 RVA: 0x00015404 File Offset: 0x00013604
	public Vector2 KickbackVector
	{
		get
		{
			return this.CurrentKickbackSpeed * this.KickbackDirection;
		}
	}

	// Token: 0x0600180E RID: 6158 RVA: 0x00015417 File Offset: 0x00013617
	public void ApplyKickback(float kickbackMultiplier)
	{
		this.m_kickbackMultiplier = kickbackMultiplier;
		this.m_kickbackTimeRemaining = this.KickbackDuration;
	}

	// Token: 0x0600180F RID: 6159 RVA: 0x0001542C File Offset: 0x0001362C
	public void ApplyKickback(float kickbackMultiplier, Vector2 kickbackDirection)
	{
		this.ApplyKickback(kickbackMultiplier);
		this.KickbackDirection = kickbackDirection.normalized;
	}

	// Token: 0x06001810 RID: 6160
	public void AdvanceTime()
	{
		this.m_kickbackTimeRemaining -= RandomizerBonusSkill.TimeScale(Time.deltaTime);
	}

	// Token: 0x06001811 RID: 6161 RVA: 0x00015456 File Offset: 0x00013656
	public void Stop()
	{
		this.m_kickbackTimeRemaining = 0f;
	}

	// Token: 0x0400151F RID: 5407
	public AnimationCurve KickbackCurve;

	// Token: 0x04001520 RID: 5408
	private float m_kickbackTimeRemaining;

	// Token: 0x04001521 RID: 5409
	private float m_kickbackMultiplier;
}
