using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ImprovedShotCombo : ShotCombo
{
	public float CurrentCooldownTime
	{
		get
		{
			return Mathf.Min(this.CooldownTimeForCompletedCombo + this.m_lastRealShotTime, this.MaximumComboCooldownTime) + this.m_spamPenaltyAmount;
		}
	}

	public bool InCooldownBufferWindow
	{
		get
		{
			return (this.CurrentCooldownTime - this.m_comboExecutionTimer) <= this.CooldownBufferWindow;
		}
	}

	public float CurrentResetTime
	{
		get
		{
			return Mathf.Min(this.CooldownTimeForIncompleteCombo + this.m_lastRealShotTime, this.MaximumComboResetTime);
		}
	}

	public bool InResetBufferWindow
	{
		get
		{
			return (this.CurrentResetTime - this.m_comboExecutionTimer) <= this.ResetBufferWindow;
		}
	}

	public void PenalizeSpam()
	{
		this.m_spamPenaltyAmount = Mathf.Min(this.m_spamPenaltyAmount + this.AdditionalPenaltyForSpam, this.MaximumPenaltyForSpam);
		Randomizer.log("ImprovedShotCombo (spam) real " + this.m_comboExecutionTimer + " / pen " + this.m_spamPenaltyAmount + " / cooldown " + this.CurrentCooldownTime);
	}

	public void OnShootInput()
	{
		if (this.m_currentBuffer == null)
		{
			this.m_currentBuffer = new Buffer(this.NumberOfShotsPerCombo);
			this.m_currentBuffer.Add();
			Randomizer.log("ImprovedShotCombo (combo:new)");
			return;
		}

		if (this.m_currentBuffer.CanAdd && !this.InResetBufferWindow)
		{
			this.m_currentBuffer.Add();
			Randomizer.log("ImprovedShotCombo (combo:add) time " + this.m_currentBuffer.EffectiveTime + " / real " + this.m_comboExecutionTimer + " / full " + !this.m_currentBuffer.CanAdd);
			return;
		}

		// there's a current buffer, but you EITHER can't add to it OR are in the reset buffer
		// if you can't add to it, you're EITHER blocked on cooldown OR in the cooldown buffer
		if (this.m_currentBuffer.CanAdd && this.InResetBufferWindow)
		{
			if (this.m_pendingBuffer == null)
			{
				this.m_pendingBuffer = new Buffer(this.NumberOfShotsPerCombo);
				this.m_pendingBuffer.Add();
				Randomizer.log("ImprovedShotCombo (buffer:reset) time 0 / real " + this.m_comboExecutionTimer + " / reset " + this.CurrentResetTime);
			}
			else if (this.m_pendingBuffer.CanAdd)
			{
				this.m_pendingBuffer.Add();
				Randomizer.log("ImprovedShotCombo (buffer:reset) time " + this.m_pendingBuffer.EffectiveTime + " / real " + this.m_comboExecutionTimer + " / full " + !this.m_pendingBuffer.CanAdd);
			}

			return;
		}

		this.PenalizeSpam();

		if (this.InCooldownBufferWindow)
		{
			if (this.m_pendingBuffer == null)
			{
				this.m_pendingBuffer = new Buffer(this.NumberOfShotsPerCombo);
				this.m_pendingBuffer.Add();
				Randomizer.log("ImprovedShotCombo (buffer:cooldown) time 0 / real " + this.m_comboExecutionTimer + " / cooldown " + this.CurrentCooldownTime);
			}
			else if (this.m_pendingBuffer.CanAdd)
			{
				this.m_pendingBuffer.Add();
				Randomizer.log("ImprovedShotCombo (buffer:cooldown) time " + this.m_pendingBuffer.EffectiveTime + " / real " + this.m_comboExecutionTimer + " / full " + !this.m_pendingBuffer.CanAdd);
			}
		}
	}

	public void ResetCombo()
	{
		Randomizer.log("ImprovedShotCombo (reset) hasBuffer " + (this.m_pendingBuffer != null));
		this.CanShoot = true;
		this.CurrentShot = 0;
		this.m_lastRealShotTime = 0f;
		this.m_comboExecutionTimer = 0f;
		this.m_spamPenaltyAmount = 0f;
		this.m_currentBuffer = this.m_pendingBuffer;
		this.m_pendingBuffer = null;
	}

	public void Update(float dt)
	{
		Randomizer.log("ImprovedShotCombo ----- FRAME -----");

		if (this.CurrentShot == this.NumberOfShotsPerCombo)
		{
			this.m_comboExecutionTimer += Mathf.Round(dt * 120f);
			Randomizer.log("ImprovedShotCombo (cooldown) timer " + this.m_comboExecutionTimer + " / cooldown " + this.CurrentCooldownTime);
			if (this.m_comboExecutionTimer >= this.CurrentCooldownTime)
			{
				this.ResetCombo();
			}
		}
		else if (this.CurrentShot == 0)
		{
			Randomizer.log("ImprovedShotCombo (idle)");
			this.CanShoot = true;
		}
		else
		{
			this.m_comboExecutionTimer += Mathf.Round(dt * 120f);
			this.CanShoot = true;
			Randomizer.log("ImprovedShotCombo (combo) shot " + this.CurrentShot + " / num " + this.NumberOfShotsPerCombo + " / timer " + this.m_comboExecutionTimer + " / reset " + this.CurrentResetTime);
			if (this.m_comboExecutionTimer >= this.CurrentResetTime)
			{
				this.ResetCombo();
			}
		}

		if (this.m_currentBuffer != null)
		{
			this.m_currentBuffer.NumShots = this.NumberOfShotsPerCombo;
			this.m_currentBuffer.Update(dt);
		}
		if (this.m_pendingBuffer != null)
		{
			this.m_pendingBuffer.NumShots = this.NumberOfShotsPerCombo;
			this.m_pendingBuffer.Update(dt);
		}
	}

	public bool ProcessBuffer()
	{
		if (!this.CanShoot || this.m_currentBuffer == null || this.m_currentBuffer.IsEmpty)
		{
			return false;
		}

		float speedupTime = Mathf.Max(this.m_comboExecutionTimer - this.BufferPlaybackSpeedupThreshold, 0f) * 2f;
		float effectiveTime = Mathf.Min(this.m_comboExecutionTimer, this.BufferPlaybackSpeedupThreshold) + speedupTime;
		Randomizer.log("ImprovedShotCombo (process) eff " + effectiveTime + " / next " + this.m_currentBuffer.NextShotTime + " / real " + this.m_comboExecutionTimer);

		if (effectiveTime >= this.m_currentBuffer.NextShotTime)
		{
			this.m_lastRealShotTime = this.m_comboExecutionTimer;
			++this.CurrentShot;
			this.CanShoot = false;
			this.m_currentBuffer.Remove();
			Randomizer.log("ImprovedShotCombo (shoot) shot " + this.CurrentShot + " / num " + this.NumberOfShotsPerCombo + " / real " + this.m_comboExecutionTimer);
			return true;
		}

		return false;
	}

	public void SetQuickFlame(bool enabled)
	{
		this.CooldownTimeForCompletedCombo = 60f;
		this.CooldownTimeForIncompleteCombo = 36f;

		if (enabled)
		{
			this.NumberOfShotsPerCombo = 3;
			this.MaximumComboCooldownTime = 84f;
			this.MaximumPenaltyForSpam = 6f;
		}
		else
		{
			this.NumberOfShotsPerCombo = 2;
			this.MaximumComboCooldownTime = 72f;
			this.MaximumPenaltyForSpam = 8f;
		}
	}

	private float m_lastRealShotTime = 0f;

	private float m_comboExecutionTimer = 0f;

	private float m_spamPenaltyAmount = 0f;

	// times expressed in number of frames at 120 fps (for plausible future-proofing)
	public float MaximumComboCooldownTime = 72f;

	public float MaximumComboResetTime = 48f;

	public float AdditionalPenaltyForSpam = 2f;

	public float MaximumPenaltyForSpam = 8f;

	public float CooldownBufferWindow = 11f;

	public float ResetBufferWindow = 5f;

	public float BufferPlaybackSpeedupThreshold = 24f;

	private class Buffer
	{
		public Buffer(int numShots)
		{
			this.NumShots = numShots;
		}

		public void Update(float dt)
		{
			this.m_comboTimer += Mathf.Round(dt * 120f);
		}

		public bool IsEmpty
		{
			get
			{
				return this.m_currentShotRemove == this.m_shotTimes.Count;
			}
		}

		public bool CanAdd
		{
			get
			{
				return this.m_currentShotAdd < this.NumShots;
			}
		}

		public void Add()
		{
			++this.m_currentShotAdd;
			this.m_shotTimes.Add(this.m_comboTimer);
		}

		public float NextShotTime
		{
			get
			{
				if (this.m_currentShotRemove >= this.m_currentShotAdd)
				{
					return Mathf.Infinity;
				}

				return this.m_shotTimes[this.m_currentShotRemove];
			}
		}

		public float EffectiveTime
		{
			get
			{
				return this.m_comboTimer;
			}
		}

		public void Remove()
		{
			++this.m_currentShotRemove;
		}

		public int NumShots = 0;

		private int m_currentShotAdd = 0;

		private int m_currentShotRemove = 0;

		private List<float> m_shotTimes = new List<float>();

		private float m_comboTimer = 0f;
	}

	private Buffer m_currentBuffer = null;

	private Buffer m_pendingBuffer = null;
}