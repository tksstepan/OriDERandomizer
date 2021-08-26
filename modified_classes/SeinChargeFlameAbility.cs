using System;
using System.Collections.Generic;
using Core;
using fsm;
using Game;
using UnityEngine;

// Token: 0x02000023 RID: 35
public class SeinChargeFlameAbility : CharacterState, ISeinReceiver
{
	// Token: 0x060000CC RID: 204
	public SeinChargeFlameAbility()
	{
	}

	// Token: 0x060000CD RID: 205
	static SeinChargeFlameAbility()
	{
	}

	// Token: 0x17000022 RID: 34
	// (get) Token: 0x060000CE RID: 206
	public float ChargeDuration
	{
		get
		{
			return this.ChargeFlameSettings.ChargeDuration;
		}
	}

	// Token: 0x17000023 RID: 35
	// (get) Token: 0x060000CF RID: 207
	public bool HasEnoughEnergy
	{
		get
		{
			return this.m_sein.Energy.CanAfford(this.m_sein.PlayerAbilities.ChargeFlameEfficiency.HasAbility ? 0f : 0.5f);
		}
	}

	// Token: 0x060000D0 RID: 208
	public void SpendEnergy()
	{
		this.m_sein.Energy.Spend(this.m_sein.PlayerAbilities.ChargeFlameEfficiency.HasAbility ? 0f : 0.5f);
	}

	// Token: 0x060000D1 RID: 209
	public void RestoreEnergy()
	{
		this.m_sein.Energy.Gain(this.m_sein.PlayerAbilities.ChargeFlameEfficiency.HasAbility ? 0f : 0.5f);
	}

	// Token: 0x060000D2 RID: 210
	public override void Awake()
	{
		base.Awake();
		this.State.Start = new State
		{
			UpdateStateEvent = new Action(this.UpdateStartState),
			OnEnterEvent = new Action(this.OnEnterStartState)
		};
		this.State.Precharging = new State
		{
			UpdateStateEvent = new Action(this.UpdatePrechargingState)
		};
		this.State.Charging = new State
		{
			UpdateStateEvent = new Action(this.UpdateChargingState)
		};
		this.State.Charged = new State
		{
			UpdateStateEvent = new Action(this.UpdateChargedState)
		};
		this.Logic.RegisterStates(new IState[]
		{
			this.State.Start,
			this.State.Precharging,
			this.State.Charging,
			this.State.Charged
		});
		this.Logic.ChangeState(this.State.Start);
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x060000D3 RID: 211
	public void OnRestoreCheckpoint()
	{
		if (this.m_chargeFlameChargeEffect)
		{
			InstantiateUtility.Destroy(this.m_chargeFlameChargeEffect);
		}
		if (this.CurrentChargingSound())
		{
			this.CurrentChargingSound().StopAndFadeOut(0.5f);
		}
		this.Logic.ChangeState(this.State.Start);
	}

	// Token: 0x060000D4 RID: 212
	public void OnEnterStartState()
	{
		if (this.m_chargeFlameChargeEffect)
		{
			InstantiateUtility.Destroy(this.m_chargeFlameChargeEffect);
		}
	}

	// Token: 0x060000D5 RID: 213
	public void UpdateStartState()
	{
		if (this.m_chargeFlameChargeEffect)
		{
			InstantiateUtility.Destroy(this.m_chargeFlameChargeEffect);
		}
		if (this.m_sein.Controller.IsBashing)
		{
			return;
		}

		bool pressed = this.ChargeFlameButton.OnPressed && !this.ChargeFlameButton.Used;

		if (RandomizerSettings.HoldAutofire && !Core.Input.ChargeJump.Pressed)
		{
			pressed = false;
		}

		if (pressed && this.m_sein.PlayerAbilities.ChargeFlame.HasAbility && !this.m_sein.Controller.InputLocked && !this.m_sein.Abilities.SpiritFlame.LockShootingSpiritFlame)
		{
			this.Logic.ChangeState(this.State.Precharging);
		}
	}

	// Token: 0x060000D6 RID: 214
	public void UpdatePrechargingState()
	{
		if (this.Logic.CurrentStateTime > 0.3f)
		{
			this.m_chargeFlameChargeEffect = (GameObject)InstantiateUtility.Instantiate(this.ChargeFlameSettings.ChargeFlameChargeEffectPrefab);
			this.m_chargeFlameChargeEffect.transform.position = Characters.Ori.transform.position;
			this.m_chargeFlameChargeEffect.transform.parent = Characters.Ori.transform;
			this.m_chargeFlameChargeEffect.GetComponentsInChildren<LegacyAnimator>(SeinChargeFlameAbility.s_legacyAnimatorList);
			for (int i = 0; i < SeinChargeFlameAbility.s_legacyAnimatorList.Count; i++)
			{
				SeinChargeFlameAbility.s_legacyAnimatorList[i].Speed = 1f / this.ChargeDuration;
			}
			SeinChargeFlameAbility.s_legacyAnimatorList.Clear();
			if (this.CurrentChargingSound())
			{
				this.CurrentChargingSound().Play();
			}
			this.Logic.ChangeState(this.State.Charging);
			return;
		}
		if (this.ChargeFlameButton.Released)
		{
			this.Logic.ChangeState(this.State.Start);
			return;
		}
		if (this.m_sein.Abilities.SpiritFlame.LockShootingSpiritFlame)
		{
			this.Logic.ChangeState(this.State.Start);
			return;
		}
		if (this.m_sein.Controller.InputLocked)
		{
			this.Logic.ChangeState(this.State.Start);
		}
	}

	// Token: 0x060000D7 RID: 215
	public void UpdateChargingState()
	{
		if (this.ChargeFlameButton.Released || this.m_sein.Controller.InputLocked || this.m_sein.Abilities.SpiritFlame.LockShootingSpiritFlame)
		{
			if (this.CurrentChargingSound())
			{
				this.CurrentChargingSound().StopAndFadeOut(0.5f);
			}
			this.Logic.ChangeState(this.State.Start);
			return;
		}
		if (this.Logic.CurrentStateTime >= this.ChargeDuration)
		{
			if (this.HasEnoughEnergy)
			{
				this.Logic.ChangeState(this.State.Charged);
				this.SpendEnergy();
				return;
			}
			if (this.CurrentChargingSound())
			{
				this.CurrentChargingSound().StopAndFadeOut(0.5f);
			}
			this.Logic.ChangeState(this.State.Start);
			UI.SeinUI.ShakeEnergyOrbBar();
			if (this.NotEnoughEnergySound)
			{
				Sound.Play(this.NotEnoughEnergySound.GetSound(null), base.transform.position, null);
			}
		}
	}

	// Token: 0x060000D8 RID: 216
	public void ReleaseChargeBurst()
	{
		if (this.CurrentChargingSound())
		{
			this.CurrentChargingSound().StopAndFadeOut(0.5f);
		}
		if (this.m_sein.PlayerAbilities.ChargeFlameBlast.HasAbility)
		{
			InstantiateUtility.Instantiate(this.ChargeFlameSettings.ChargeFlameBurstC, Characters.Ori.Position, Quaternion.identity);
		}
		else if (this.m_sein.PlayerAbilities.ChargeFlameBurn.HasAbility)
		{
			InstantiateUtility.Instantiate(this.ChargeFlameSettings.ChargeFlameBurstB, Characters.Ori.Position, Quaternion.identity);
		}
		else
		{
			InstantiateUtility.Instantiate(this.ChargeFlameSettings.ChargeFlameBurstA, Characters.Ori.Position, Quaternion.identity);
		}
		this.Logic.ChangeState(this.State.Start);
	}

	// Token: 0x060000D9 RID: 217
	public void UpdateChargedState()
	{
		if (this.ChargeFlameButton.Released)
		{
			this.ReleaseChargeBurst();
			return;
		}
		if (Core.Input.SoulFlame.OnPressed)
		{
			Core.Input.SoulFlame.Used = true;
			if (this.CurrentChargingSound())
			{
				this.CurrentChargingSound().StopAndFadeOut(0.5f);
			}
			this.Logic.ChangeState(this.State.Start);
			this.RestoreEnergy();
			UI.SeinUI.ShakeEnergyOrbBar();
		}
	}

	// Token: 0x17000024 RID: 36
	// (get) Token: 0x060000DA RID: 218
	public Core.Input.InputButtonProcessor ChargeFlameButton
	{
		get
		{
			return Core.Input.SpiritFlame;
		}
	}

	// Token: 0x17000025 RID: 37
	// (get) Token: 0x060000DB RID: 219
	public bool IsCharging
	{
		get
		{
			return this.Logic.CurrentState != this.State.Start;
		}
	}

	// Token: 0x060000DC RID: 220
	public override void UpdateCharacterState()
	{
		this.Logic.UpdateState(Time.deltaTime);
	}

	// Token: 0x060000DD RID: 221
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.Abilities.ChargeFlame = this;
	}

	// Token: 0x060000DE RID: 222
	public override void OnExit()
	{
		if (this.Logic.CurrentState == this.State.Precharging)
		{
			this.Logic.ChangeState(this.State.Start);
		}
		if (this.Logic.CurrentState == this.State.Charging)
		{
			if (this.CurrentChargingSound())
			{
				this.CurrentChargingSound().StopAndFadeOut(0.5f);
			}
			this.Logic.ChangeState(this.State.Start);
		}
		if (this.Logic.CurrentState == this.State.Charged)
		{
			this.ReleaseChargeBurst();
		}
		base.OnExit();
	}

	// Token: 0x060000DF RID: 223
	private SoundSource CurrentChargingSound()
	{
		if (this.m_sein.PlayerAbilities.ChargeFlameBlast.HasAbility)
		{
			return this.ChargingSoundLevelC;
		}
		if (this.m_sein.PlayerAbilities.ChargeFlameBurn.HasAbility)
		{
			return this.ChargingSoundLevelB;
		}
		return this.ChargingSoundLevelA;
	}

	// Token: 0x040000FF RID: 255
	public SoundSource ChargingSoundLevelA;

	// Token: 0x04000100 RID: 256
	public SoundSource ChargingSoundLevelB;

	// Token: 0x04000101 RID: 257
	public SoundSource ChargingSoundLevelC;

	// Token: 0x04000102 RID: 258
	public AchievementAsset KillEnemiesSimultaneouslyAchievement;

	// Token: 0x04000103 RID: 259
	public SoundProvider NotEnoughEnergySound;

	// Token: 0x04000104 RID: 260
	public SeinChargeFlameAbility.ChargeFlameDefinitions ChargeFlameSettings;

	// Token: 0x04000105 RID: 261
	public SeinChargeFlameAbility.States State = new SeinChargeFlameAbility.States();

	// Token: 0x04000106 RID: 262
	private StateMachine Logic = new StateMachine();

	// Token: 0x04000107 RID: 263
	private GameObject m_chargeFlameChargeEffect;

	// Token: 0x04000108 RID: 264
	public float EnergyCost = 1f;

	// Token: 0x04000109 RID: 265
	private static readonly List<LegacyAnimator> s_legacyAnimatorList = new List<LegacyAnimator>();

	// Token: 0x0400010A RID: 266
	private SeinCharacter m_sein;

	// Token: 0x02000024 RID: 36
	[Serializable]
	public class ChargeFlameDefinitions
	{
		// Token: 0x060000E0 RID: 224
		public ChargeFlameDefinitions()
		{
		}

		// Token: 0x0400010B RID: 267
		public float ChargeDuration = 1f;

		// Token: 0x0400010C RID: 268
		public GameObject ChargeFlameBurstA;

		// Token: 0x0400010D RID: 269
		public GameObject ChargeFlameBurstB;

		// Token: 0x0400010E RID: 270
		public GameObject ChargeFlameBurstC;

		// Token: 0x0400010F RID: 271
		public GameObject ChargeFlameChargeEffectPrefab;
	}

	// Token: 0x02000025 RID: 37
	public class States
	{
		// Token: 0x060000E1 RID: 225
		public States()
		{
		}

		// Token: 0x04000110 RID: 272
		public State Start;

		// Token: 0x04000111 RID: 273
		public State Precharging;

		// Token: 0x04000112 RID: 274
		public State Charging;

		// Token: 0x04000113 RID: 275
		public State Charged;
	}
}
