using System;
using System.Collections;
using System.Diagnostics;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000315 RID: 789
public class SeinDamageReciever : CharacterState, IDamageReciever, ISeinReceiver, IProjectileDetonatable
{
	// Token: 0x1700029E RID: 670
	// (get) Token: 0x06001026 RID: 4134 RVA: 0x0000E223 File Offset: 0x0000C423
	public CharacterLeftRightMovement CharacterLeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	// Token: 0x1700029F RID: 671
	// (get) Token: 0x06001027 RID: 4135 RVA: 0x0000E235 File Offset: 0x0000C435
	public CharacterGravity CharacterGravity
	{
		get
		{
			return this.Sein.PlatformBehaviour.Gravity;
		}
	}

	// Token: 0x170002A0 RID: 672
	// (get) Token: 0x06001028 RID: 4136 RVA: 0x0000E247 File Offset: 0x0000C447
	public CharacterInstantStop CharacterInstantStop
	{
		get
		{
			return this.Sein.PlatformBehaviour.InstantStop;
		}
	}

	// Token: 0x170002A1 RID: 673
	// (get) Token: 0x06001029 RID: 4137 RVA: 0x0000E259 File Offset: 0x0000C459
	public SeinHealthController HealthController
	{
		get
		{
			return this.Sein.Mortality.Health;
		}
	}

	// Token: 0x170002A2 RID: 674
	// (get) Token: 0x0600102A RID: 4138 RVA: 0x0000E26B File Offset: 0x0000C46B
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x170002A3 RID: 675
	// (get) Token: 0x0600102B RID: 4139 RVA: 0x0000E27D File Offset: 0x0000C47D
	public Renderer Sprite
	{
		get
		{
			return this.Sein.PlatformBehaviour.Visuals.SpriteRenderer;
		}
	}

	// Token: 0x0600102C RID: 4140 RVA: 0x000610D8 File Offset: 0x0005F2D8
	public void Start()
	{
		this.CharacterGravity.ModifyGravityPlatformMovementSettingsEvent += new Action<GravityPlatformMovementSettings>(this.ModifyGravityPlatformMovementSettings);
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += new Action<HorizontalPlatformMovementSettings>(this.ModifyHorizontalPlatformMovementSettings);
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x0600102D RID: 4141 RVA: 0x0006112C File Offset: 0x0005F32C
	public new void OnDestroy()
	{
		base.OnDestroy();
		this.CharacterGravity.ModifyGravityPlatformMovementSettingsEvent -= new Action<GravityPlatformMovementSettings>(this.ModifyGravityPlatformMovementSettings);
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= new Action<HorizontalPlatformMovementSettings>(this.ModifyHorizontalPlatformMovementSettings);
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x0600102E RID: 4142 RVA: 0x0000E294 File Offset: 0x0000C494
	public override void OnEnter()
	{
		this.CharacterInstantStop.Active = false;
	}

	// Token: 0x0600102F RID: 4143 RVA: 0x0000E2A2 File Offset: 0x0000C4A2
	public override void OnExit()
	{
		this.CharacterInstantStop.Active = true;
	}

	// Token: 0x06001030 RID: 4144
	public void OnRecieveDamage(Damage damage)
	{
		if (RandomizerBonusSkill.Invincible)
			return;
		if (damage.Amount < 9000f || damage.Type != DamageType.Water)
		{
			if (this.IsImmortal)
			{
				return;
			}
			if (!this.Sein.Controller.CanMove)
			{
				return;
			}
			if (damage.Type == DamageType.SpiritFlameSplatter || damage.Type == DamageType.LevelUp)
			{
				return;
			}
		}
		damage.SetAmount(Mathf.Round(damage.Amount * Randomizer.DamageModifier));
		bool flag = this.m_invincibleTimeRemaining > 0f;
		bool flag2 = this.m_invincibleToEnemiesTimeRemaining > 0f;
		if (this.Sein.Abilities.Stomp && this.Sein.Abilities.Stomp.Logic.CurrentState == this.Sein.Abilities.Stomp.State.StompDown)
		{
			flag = true;
		}
		if (!this.Sein.gameObject.activeInHierarchy)
		{
			return;
		}
		if (flag && damage.Amount < 100f && damage.Type != DamageType.Drowning)
		{
			damage.SetAmount(0f);
		}
		if (flag2 && damage.Amount < 100f && (damage.Type == DamageType.Enemy || damage.Type == DamageType.Projectile || damage.Type == DamageType.SlugSpike))
		{
			damage.SetAmount(0f);
		}
		if (damage.Amount == 0f)
		{
			return;
		}
		if (damage.Amount < 100f)
		{
			DifficultyMode difficulty = DifficultyController.Instance.Difficulty;
			if (difficulty != DifficultyMode.Easy)
			{
				if (difficulty == DifficultyMode.Hard)
				{
					damage.SetAmount(damage.Amount * 2f);
					if (damage.Amount < 8f)
					{
						damage.SetAmount(8f);
					}
				}
			}
			else if (damage.Type != DamageType.Lava && damage.Type != DamageType.Spikes)
			{
				damage.SetAmount(damage.Amount / 2f);
			}
			else
			{
				int num = Mathf.RoundToInt(damage.Amount / 4f);
				if (num > 3)
				{
					num = Mathf.FloorToInt((float)(num - 3) * 0.5f) + 3;
				}
				damage.SetAmount((float)(num * 4));
			}
		}
		if (Randomizer.OHKO)
		{
			damage.SetAmount(damage.Amount * 100f);
		}
		UI.Vignette.SeinHurt.Restart();
		SoundDescriptor soundForDamage = ((damage.Amount >= this.BadlyHurtAmount) ? this.SeinBadlyHurtSound : this.SeinHurtSound).GetSoundForDamage(damage);
		if (soundForDamage != null)
		{
			SoundPlayer soundPlayer = Sound.Play(soundForDamage, this.PlatformMovement.Position, null);
			if (soundPlayer)
			{
				soundPlayer.AttachTo = this.Sein.PlatformBehaviour.transform;
			}
		}
		int num2 = Mathf.CeilToInt(damage.Amount / 4f);
		damage.SetAmount((float)num2);
		if (damage.Amount < 1000f && this.Sein.PlayerAbilities.UltraDefense.HasAbility)
		{
			damage.SetAmount((float)Mathf.RoundToInt((float)num2 * 0.8f));
		}
		Attacking.DamageDisplayText.Create(damage, this.Sein.transform);
		damage.SetAmount((float)(num2 * 4));
		if (damage.Amount < 1000f && this.Sein.PlayerAbilities.UltraDefense.HasAbility)
		{
			damage.SetAmount((float)(Mathf.FloorToInt((float)(num2 * 2) * 0.8f) * 2));
		}
		int num3 = Mathf.RoundToInt(damage.Amount);
		if ((float)num3 >= this.HealthController.Amount)
		{
			this.Sein.Mortality.Health.TakeDamage(num3);
			this.OnKill(damage);
			return;
		}
		this.Sein.Mortality.Health.TakeDamage(num3);
		if (damage.Type != DamageType.Drowning)
		{
			this.MakeInvincible(1f);
			base.StartCoroutine(this.FlashSprite());
			if (this.HurtEffect)
			{
				GameObject expr_3BA = (GameObject)InstantiateUtility.Instantiate(this.HurtEffect);
				expr_3BA.transform.position = base.transform.position;
				Vector3 vector = this.PlatformMovement.LocalSpeed.normalized + damage.Force.normalized;
				float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				expr_3BA.transform.rotation = Quaternion.Euler(0f, 0f, z);
			}
			base.Active = true;
			if (this.Sein.Abilities.GrabWall)
			{
				this.Sein.Abilities.GrabWall.Exit();
			}
			if (this.Sein.Abilities.Dash)
			{
				this.Sein.Abilities.Dash.Exit();
			}
			this.PlatformMovement.LocalSpeed = ((damage.Force.x <= 0f) ? new Vector2(-this.HurtSpeed.x, this.HurtSpeed.y) : this.HurtSpeed);
			this.m_hurtTimeRemaining = this.HurtDuration;
			this.Sein.PlatformBehaviour.Visuals.Animation.Play(this.HurtAnimation, 140, new Func<bool>(this.ShouldHurtAnimationKeepPlaying));
			return;
		}
		base.StartCoroutine(this.FlashSprite());
	}

	// Token: 0x06001031 RID: 4145 RVA: 0x0000E2B0 File Offset: 0x0000C4B0
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
	}

	// Token: 0x06001032 RID: 4146 RVA: 0x000616AC File Offset: 0x0005F8AC
	public override void UpdateCharacterState()
	{
		if (this.Sein.IsSuspended)
		{
			return;
		}
		this.m_hurtTimeRemaining -= Time.deltaTime;
		this.m_invincibleTimeRemaining -= Time.deltaTime;
		this.m_invincibleToEnemiesTimeRemaining -= Time.deltaTime;
		if (this.m_hurtTimeRemaining < 0f)
		{
			this.m_hurtTimeRemaining = 0f;
		}
		if (this.m_invincibleTimeRemaining < 0f)
		{
			this.m_invincibleTimeRemaining = 0f;
		}
		if (this.m_invincibleToEnemiesTimeRemaining < 0f)
		{
			this.m_invincibleToEnemiesTimeRemaining = 0f;
		}
		if (base.Active && this.m_hurtTimeRemaining == 0f)
		{
			base.Active = false;
		}
	}

	// Token: 0x06001033 RID: 4147 RVA: 0x0000E2B9 File Offset: 0x0000C4B9
	public void ModifyHorizontalPlatformMovementSettings(HorizontalPlatformMovementSettings settings)
	{
		if (base.Active)
		{
			settings.Ground.ApplySpeedMultiplier(this.MoveSpeed);
			settings.Air.ApplySpeedMultiplier(this.MoveSpeed);
		}
	}

	// Token: 0x06001034 RID: 4148 RVA: 0x0000E2E8 File Offset: 0x0000C4E8
	public void ModifyGravityPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (base.Active)
		{
			settings.GravityStrength *= this.GravityMultiplier;
		}
	}

	// Token: 0x06001035 RID: 4149 RVA: 0x0000E308 File Offset: 0x0000C508
	public void MakeInvincible(float duration)
	{
		this.m_invincibleTimeRemaining = Mathf.Max(this.m_invincibleTimeRemaining, duration);
	}

	// Token: 0x06001036 RID: 4150 RVA: 0x0000E31C File Offset: 0x0000C51C
	public void MakeInvincibleToEnemies(float duration)
	{
		this.m_invincibleToEnemiesTimeRemaining = Mathf.Max(this.m_invincibleToEnemiesTimeRemaining, duration);
	}

	// Token: 0x06001037 RID: 4151 RVA: 0x0000E330 File Offset: 0x0000C530
	public void ResetInviciblity()
	{
		this.m_invincibleTimeRemaining = 0f;
		this.m_invincibleToEnemiesTimeRemaining = 0f;
	}

	// Token: 0x06001038 RID: 4152 RVA: 0x00061774 File Offset: 0x0005F974
	public void OnRestoreCheckpoint()
	{
		this.SpriteMaterialTintColor(new Color(0f, 0f, 0f, 0f));
		CameraFrustumOptimizer.ForceUpdate();
		if (this.m_died)
		{
			this.m_died = false;
			this.Sein.Active = true;
			this.Sein.GetComponent<GoThroughPlatformHandler>().UpdateColliders();
			this.Sein.Mortality.Health.OnRespawn();
			if (WorldMapLogic.Instance.MapEnabledArea.FindFaceAtPositionFaster(Characters.Sein.Position) != null)
			{
				GameController.Instance.SaveGameController.PerformSave();
			}
		}
	}

	// Token: 0x06001039 RID: 4153 RVA: 0x00061818 File Offset: 0x0005FA18
	[DebuggerHidden]
	public IEnumerator FlashSprite()
	{
		SeinDamageReciever.<FlashSprite>c__Iterator12 <FlashSprite>c__Iterator = new SeinDamageReciever.<FlashSprite>c__Iterator12();
		<FlashSprite>c__Iterator.<>f__this = this;
		return <FlashSprite>c__Iterator;
	}

	// Token: 0x0600103A RID: 4154 RVA: 0x0000E348 File Offset: 0x0000C548
	public void SpriteMaterialTintColor(Color color)
	{
		if (this.Sprite)
		{
			this.Sprite.sharedMaterial.SetColor(ShaderProperties.TintColor, color);
		}
	}

	// Token: 0x0600103B RID: 4155 RVA: 0x0000E370 File Offset: 0x0000C570
	public void OnEnable()
	{
		this.SpriteMaterialTintColor(new Color(0f, 0f, 0f, 0f));
		this.m_invincibleTimeRemaining = 0f;
		this.m_invincibleToEnemiesTimeRemaining = 0f;
	}

	// Token: 0x170002A4 RID: 676
	// (get) Token: 0x0600103C RID: 4156 RVA: 0x0000E3A7 File Offset: 0x0000C5A7
	public bool IsInvinsible
	{
		get
		{
			return this.m_invincibleTimeRemaining > 0f;
		}
	}

	// Token: 0x0600103D RID: 4157 RVA: 0x0000E3B6 File Offset: 0x0000C5B6
	public bool ShouldHurtAnimationKeepPlaying()
	{
		return !this.PlatformMovement.IsOnGround;
	}

	// Token: 0x0600103E RID: 4158 RVA: 0x00061834 File Offset: 0x0005FA34
	public void OnKill(Damage damage)
	{
		if (!this.Sein.Active)
		{
			return;
		}
		this.m_died = true;
		SoundDescriptor soundForDamage = this.SeinDeathSound.GetSoundForDamage(damage);
		if (soundForDamage != null)
		{
			SoundPlayer soundPlayer = Sound.Play(soundForDamage, this.PlatformMovement.Position, null);
			if (soundPlayer)
			{
				soundPlayer.AttachTo = this.Sein.PlatformBehaviour.transform;
			}
		}
		Utility.DisableLate(this.Sein);
		SeinDeathCounter.Count++;
		SeinDeathsManager.OnDeath();
		GameController.Instance.ResumeGameplay();
		if (this.DeathEffectProvider)
		{
			this.InstantiateDeathEffect(damage);
		}
		Events.Scheduler.OnPlayerDeath.Call();
		if (DifficultyController.Instance.Difficulty == DifficultyMode.OneLife)
		{
			SaveSlotsManager.CurrentSaveSlot.WasKilled = true;
			GameController.Instance.SaveGameController.PerformSave();
			SaveSlotBackupsManager.DeleteAllBackups(SaveSlotsManager.CurrentSlotIndex);
		}
		GameController.Instance.StartCoroutine(this.OnKillRoutine());
	}

	// Token: 0x0600103F RID: 4159 RVA: 0x00061934 File Offset: 0x0005FB34
	private void InstantiateDeathEffect(Damage damage)
	{
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.DeathEffectProvider.Prefab(new DamageContext(damage)));
		damage.DealToComponents(gameObject);
		Transform transform = this.Sein.PlatformBehaviour.Visuals.SpriteMirror.transform;
		gameObject.transform.localPosition = transform.position;
		gameObject.transform.localScale = transform.localScale;
		gameObject.transform.localRotation = transform.localRotation;
	}

	// Token: 0x06001040 RID: 4160 RVA: 0x000619B8 File Offset: 0x0005FBB8
	[DebuggerHidden]
	public IEnumerator OnKillRoutine()
	{
		SeinDamageReciever.<OnKillRoutine>c__Iterator13 <OnKillRoutine>c__Iterator = new SeinDamageReciever.<OnKillRoutine>c__Iterator13();
		<OnKillRoutine>c__Iterator.<>f__this = this;
		return <OnKillRoutine>c__Iterator;
	}

	// Token: 0x06001041 RID: 4161 RVA: 0x0000E3C6 File Offset: 0x0000C5C6
	public void OnKillFadeInComplete()
	{
		GameController.Instance.RestoreCheckpoint(null);
	}

	// Token: 0x06001042 RID: 4162 RVA: 0x0000E3D3 File Offset: 0x0000C5D3
	public bool CanDetonateProjectiles()
	{
		return this.m_invincibleToEnemiesTimeRemaining == 0f;
	}

	// Token: 0x06001043 RID: 4163 RVA: 0x0000E3E2 File Offset: 0x0000C5E2
	public override void Serialize(Archive ar)
	{
		base.Serialize(ar);
		ar.Serialize(ref this.m_serializationFiller);
	}

	// Token: 0x04000F68 RID: 3944
	public SeinCharacter Sein;

	// Token: 0x04000F69 RID: 3945
	public TextureAnimationWithTransitions HurtAnimation;

	// Token: 0x04000F6A RID: 3946
	public DamageBasedSoundProvider SeinDeathSound;

	// Token: 0x04000F6B RID: 3947
	public DamageBasedSoundProvider SeinHurtSound;

	// Token: 0x04000F6C RID: 3948
	public DamageBasedSoundProvider SeinBadlyHurtSound;

	// Token: 0x04000F6D RID: 3949
	public float BadlyHurtAmount = 4f;

	// Token: 0x04000F6E RID: 3950
	public bool IsImmortal;

	// Token: 0x04000F6F RID: 3951
	public float HurtDropPickupSpeed = 20f;

	// Token: 0x04000F70 RID: 3952
	private float m_invincibleTimeRemaining;

	// Token: 0x04000F71 RID: 3953
	private float m_invincibleToEnemiesTimeRemaining;

	// Token: 0x04000F72 RID: 3954
	private bool m_died;

	// Token: 0x04000F73 RID: 3955
	public GameObject GameOverScreen;

	// Token: 0x04000F74 RID: 3956
	public float HurtDuration = 0.25f;

	// Token: 0x04000F75 RID: 3957
	public Vector2 HurtSpeed = new Vector2(6f, 9f);

	// Token: 0x04000F76 RID: 3958
	public HorizontalPlatformMovementSettings.SpeedMultiplierSet MoveSpeed = new HorizontalPlatformMovementSettings.SpeedMultiplierSet
	{
		AccelerationMultiplier = 0f,
		DeceelerationMultiplier = 0f,
		MaxSpeedMultiplier = 1f
	};

	// Token: 0x04000F77 RID: 3959
	public float GravityMultiplier = 2f;

	// Token: 0x04000F78 RID: 3960
	public GameObject HurtEffect;

	// Token: 0x04000F79 RID: 3961
	public GameObject HurtDropPickup;

	// Token: 0x04000F7A RID: 3962
	private float m_hurtTimeRemaining;

	// Token: 0x04000F7B RID: 3963
	public GameObject KillFader;

	// Token: 0x04000F7C RID: 3964
	public float DeathDuration;

	// Token: 0x04000F7D RID: 3965
	public float OneLifeDeathDuration = 2f;

	// Token: 0x04000F7E RID: 3966
	public float SpawnDuration;

	// Token: 0x04000F7F RID: 3967
	public float DeathFadeInDuration = 0.05f;

	// Token: 0x04000F80 RID: 3968
	public float DeathFadeOutDuration = 1f;

	// Token: 0x04000F81 RID: 3969
	public DamageBasedPrefabProvider DeathEffectProvider;

	// Token: 0x04000F82 RID: 3970
	private int m_serializationFiller;
}
