using System;
using UnityEngine;

[Serializable]
public class Kickback
{
	public float KickbackDuration
	{
		get
		{
			return this.KickbackCurve[this.KickbackCurve.length - 1].time;
		}
	}

	public Vector2 KickbackDirection { get; private set; }

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

	public Vector2 KickbackVector
	{
		get
		{
			return this.CurrentKickbackSpeed * this.KickbackDirection;
		}
	}

	public void ApplyKickback(float kickbackMultiplier)
	{
		this.m_kickbackMultiplier = kickbackMultiplier;
		this.m_kickbackTimeRemaining = this.KickbackDuration;
	}

	public void ApplyKickback(float kickbackMultiplier, Vector2 kickbackDirection)
	{
		this.ApplyKickback(kickbackMultiplier);
		this.KickbackDirection = kickbackDirection.normalized;
	}

	public void AdvanceTime()
	{
		this.m_kickbackTimeRemaining -= RandomizerBonusSkill.TimeScale(Time.deltaTime);
	}

	public void Stop()
	{
		this.m_kickbackTimeRemaining = 0f;
	}

	public AnimationCurve KickbackCurve;

	private float m_kickbackTimeRemaining;

	private float m_kickbackMultiplier;
}
