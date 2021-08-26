using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class SeinStandardSpiritFlameAbility : CharacterState, ISeinReceiver
{
	public SpiritFlame CurrentSpiritFlame
	{
		get
		{
			return this.GetStandardSpiritFlame(this.OriLevel);
		}
	}

	public int OriLevel
	{
		get
		{
			return this.m_sein.PlayerAbilities.OriStrength;
		}
	}

	public bool LockShootingSpiritFlame
	{
		get
		{
			return this.m_sein.Abilities.SpiritFlame.LockShootingSpiritFlame;
		}
	}

	public int MaxTargets
	{
		get
		{
			return this.m_sein.PlayerAbilities.SplitFlameTargets;
		}
	}

	private bool ProcessAutofire(bool pressed, bool held, bool released)
	{
		if (pressed)
		{
			this.m_lastAutofire = Mathf.Round(Time.time * 120f);
		}

		bool autofire = RandomizerSettings.HoldAutofire && held;

		if (this.m_sein.Abilities.ChargeFlame && this.m_sein.Abilities.ChargeFlame.IsCharging)
		{
			autofire = false;
		}

		if (autofire)
		{
			float scaledTime = Mathf.Round(Time.time * 120f);
			if (scaledTime - this.m_lastAutofire >= 12f)
			{
				this.m_lastAutofire = scaledTime;
				return true;
			}
			else
			{
				this.UpdateTargetting();
			}
		}

		return false;
	}

	public override void UpdateCharacterState()
	{
		if (this.m_sein.Controller.InputLocked)
		{
			return;
		}
		if (SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities))
		{
			return;
		}

		bool pressed = Core.Input.SpiritFlame.OnPressed && !Core.Input.SpiritFlame.Used;
		bool held = Core.Input.SpiritFlame.Pressed && Core.Input.SpiritFlame.WasPressed;
		bool released = Core.Input.SpiritFlame.Released;

		if (this.ProcessAutofire(pressed, held, released))
		{
			pressed = true;
		}

		if (pressed)
		{
			if (Characters.Ori == null)
			{
				return;
			}

			this.m_timeOfBeforeLastShot = this.m_timeOfLastShot;
			this.m_timeOfLastShot = Mathf.Round(Time.time * 120f);
		}

		if (released)
		{
			this.UpdateTargetting();
		}

		if (this.m_sein.PlayerAbilities.RapidFire.HasAbility)
		{
			this.ProcessRapidFire(pressed);
		}
		else if (RandomizerSettings.ImprovedSpiritFlame)
		{
			this.ProcessImprovedSpiritFlame(pressed);
		}
		else
		{
			this.ProcessBaseSpiritFlame(pressed);
		}
	}

	private void ProcessRapidFire(bool pressed)
	{
		float scaledTime = Mathf.Round(Time.time * 120f);
		
		if (this.m_isSpamming)
		{
			if (scaledTime - this.m_timeOfLastSpam >= 18f)
			{
				this.m_timeOfLastSpam = scaledTime;
				pressed = true;
			}
			else
			{
				pressed = false;
			}
			if (scaledTime - this.m_timeOfLastShot > 24f)
			{
				this.m_isSpamming = false;
			}
		}
		else if (pressed && scaledTime - this.m_timeOfBeforeLastShot <= 24f)
		{
			this.m_timeOfLastSpam = scaledTime;
			this.m_isSpamming = true;
		}

		if (pressed)
		{
			Characters.Ori.ShootAnimation.Restart();
			if (!this.LockShootingSpiritFlame)
			{
				SpiritFlame currentSpiritFlame = this.CurrentSpiritFlame;
				this.m_sein.Abilities.SpiritFlame.ThrowSpiritFlames(currentSpiritFlame);
				Core.Input.SpiritFlame.Used = true;
			}
		}
	}

	private void ProcessImprovedSpiritFlame(bool pressed)
	{
		this.m_improvedShotCombo.SetQuickFlame(this.m_sein.PlayerAbilities.QuickFlame.HasAbility);
		this.m_improvedShotCombo.Update(Time.deltaTime);

		if (pressed)
		{
			this.m_improvedShotCombo.OnShootInput();
			Characters.Ori.ShootAnimation.Restart();
			Core.Input.SpiritFlame.Used = true;
		}

		if (this.m_improvedShotCombo.ProcessBuffer() && !this.LockShootingSpiritFlame)
		{
			Randomizer.log("SpiritFlame.Used");
			SpiritFlame currentSpiritFlame = this.CurrentSpiritFlame;
			this.m_sein.Abilities.SpiritFlame.ThrowSpiritFlames(currentSpiritFlame);
		}
	}

	private void ProcessBaseSpiritFlame(bool pressed)
	{
		this.StandardSpiritFlameShotCombo.UseShotDelay = false;
		this.StandardSpiritFlameShotCombo.Update(Time.deltaTime);

		if (pressed)
		{
			Characters.Ori.ShootAnimation.Restart();
			if (this.StandardSpiritFlameShotCombo.CanShoot && !this.LockShootingSpiritFlame)
			{
				this.StandardSpiritFlameShotCombo.NumberOfShotsPerCombo = ((!this.m_sein.PlayerAbilities.QuickFlame.HasAbility) ? 2 : 3);
				SpiritFlame currentSpiritFlame = this.CurrentSpiritFlame;
				this.m_sein.Abilities.SpiritFlame.ThrowSpiritFlames(currentSpiritFlame);
				this.StandardSpiritFlameShotCombo.Shoot();
				Core.Input.SpiritFlame.Used = true;
			}
		}
	}

	public SpiritFlame GetStandardSpiritFlame(int index)
	{
		if (index < 0)
		{
			index = 0;
		}
		if (index >= this.StandardSpiritFlames.Length)
		{
			index = this.StandardSpiritFlames.Length - 1;
		}
		return this.StandardSpiritFlames[index];
	}

	public List<ISpiritFlameAttackable> ClosestAttackables
	{
		get
		{
			return this.m_sein.Abilities.SpiritFlameTargetting.ClosestAttackables;
		}
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.Abilities.StandardSpiritFlame = this;
	}

	public void UpdateTargetting()
	{
		this.m_sein.Abilities.SpiritFlameTargetting.MaxNumberOfTargets = (float)this.MaxTargets;
		this.m_sein.Abilities.SpiritFlameTargetting.Range = this.SpiritFlameRange;
	}

	public ShotCombo StandardSpiritFlameShotCombo = new ShotCombo();

	private ImprovedShotCombo m_improvedShotCombo = new ImprovedShotCombo();

	public SeinStandardSpiritFlameAbility.PoisonSettings Poison = new SeinStandardSpiritFlameAbility.PoisonSettings();

	public SpiritFlame[] StandardSpiritFlames;

	public float SpiritFlameRange = 8f;

	public bool CanDamageOverTime;

	private SeinCharacter m_sein;

	private float m_timeOfLastShot;

	private float m_timeOfBeforeLastShot;

	private bool m_isSpamming;

	private float m_timeOfLastSpam;

	public float SpamShotSpeed = 10f;

	private float m_lastAutofire = 0f;

	[Serializable]
	public class PoisonSettings
	{
		public float DamageAmount = 4f;

		public int DamageDuration = 4;

		public GameObject PoisonEffect;
	}
}
