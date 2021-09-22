using System;
using UnityEngine;

public class BashAttackCritical : Suspendable, IPooled
{
	public void OnPoolSpawned()
	{
		this.CurrentState = BashAttackCritical.State.Charging;
		this.m_stateCurrentTime = 0f;
		this.m_suspended = false;
	}

	public void ChangeState(BashAttackCritical.State state)
	{
		this.CurrentState = state;
		this.m_stateCurrentTime = 0f;
	}

	public void UpdateState()
	{
		switch (this.CurrentState)
		{
		case BashAttackCritical.State.Charging:
			this.UpdateChargingState();
			break;
		case BashAttackCritical.State.Critical:
			this.UpdateCriticalState();
			break;
		case BashAttackCritical.State.Failed:
			this.UpdateFailedState();
			break;
		}
		this.m_stateCurrentTime += Time.deltaTime;
	}

	private void UpdateFailedState()
	{
		base.transform.localScale = this.m_localScale;
		base.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MaskTexture", new Vector2(0.5f, 0f));
		if (this.m_stateCurrentTime > this.FailedDuration)
		{
			this.ChangeState(BashAttackCritical.State.Finished);
		}
	}

	private void UpdateCriticalState()
	{
		base.transform.localScale = this.m_localScale + Vector3.one * Mathf.Sin(this.m_stateCurrentTime * 6.2831855f / this.ShakePeriod) * this.ShakeAmount;
		base.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MaskTexture", new Vector2(0.5f * (float)(Mathf.RoundToInt(this.m_stateCurrentTime * 15f) % 2), 0f));
		float criticalDuration = this.CriticalDuration;
		if (RandomizerSettings.Controls.LongerBashAimTime)
		{
			criticalDuration += 3.3f;
		}
		if (this.m_stateCurrentTime > criticalDuration)
		{
			this.ChangeState(BashAttackCritical.State.Failed);
		}
	}

	private void UpdateChargingState()
	{
		base.transform.localScale = this.m_localScale;
		float num = this.m_stateCurrentTime / this.ChargingDuration;
		base.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MaskTexture", new Vector2(0.5f - num * 0.5f, 0f));
		if (this.m_stateCurrentTime > this.ChargingDuration)
		{
			this.ChangeState(BashAttackCritical.State.Critical);
		}
	}

	public new void Awake()
	{
		base.Awake();
		this.m_localScale = base.transform.localScale;
	}

	public override bool IsSuspended
	{
		get
		{
			return this.m_suspended;
		}
		set
		{
			this.m_suspended = value;
		}
	}

	public void FixedUpdate()
	{
		if (this.IsSuspended)
		{
			return;
		}
		this.UpdateState();
	}

	public float ChargingDuration;

	public float CriticalDuration;

	public float FailedDuration;

	public float ShakePeriod = 0.2f;

	public float ShakeAmount = 0.5f;

	private Vector3 m_localScale;

	public BashAttackCritical.State CurrentState;

	private bool m_suspended;

	private float m_stateCurrentTime;

	public Texture2D BashAttackArrow;

	public Texture2D RedirectArrow;

	public enum State
	{
		Charging,
		Critical,
		Failed,
		Finished
	}
}
