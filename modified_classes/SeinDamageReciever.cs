using System;
using System.Collections;
using System.Diagnostics;
using Core;
using Game;
using UnityEngine;

public class SeinDamageReciever : CharacterState, IDamageReciever, ISeinReceiver, IProjectileDetonatable
{
	public CharacterLeftRightMovement CharacterLeftRightMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	public CharacterGravity CharacterGravity
	{
		get
		{
			return this.Sein.PlatformBehaviour.Gravity;
		}
	}

	public CharacterInstantStop CharacterInstantStop
	{
		get
		{
			return this.Sein.PlatformBehaviour.InstantStop;
		}
	}

	public SeinHealthController HealthController
	{
		get
		{
			return this.Sein.Mortality.Health;
		}
	}

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.Sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public Renderer Sprite
	{
		get
		{
			return this.Sein.PlatformBehaviour.Visuals.SpriteRenderer;
		}
	}

	public void Start()
	{
		this.CharacterGravity.ModifyGravityPlatformMovementSettingsEvent += new Action<GravityPlatformMovementSettings>(this.ModifyGravityPlatformMovementSettings);
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += new Action<HorizontalPlatformMovementSettings>(this.ModifyHorizontalPlatformMovementSettings);
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	public new void OnDestroy()
	{
		base.OnDestroy();
		this.CharacterGravity.ModifyGravityPlatformMovementSettingsEvent -= new Action<GravityPlatformMovementSettings>(this.ModifyGravityPlatformMovementSettings);
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= new Action<HorizontalPlatformMovementSettings>(this.ModifyHorizontalPlatformMovementSettings);
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

	public override void OnEnter()
	{
		this.CharacterInstantStop.Active = false;
	}

	public override void OnExit()
	{
		this.CharacterInstantStop.Active = true;
	}

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

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.Sein = sein;
	}

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

	public void ModifyHorizontalPlatformMovementSettings(HorizontalPlatformMovementSettings settings)
	{
		if (base.Active)
		{
			settings.Ground.ApplySpeedMultiplier(this.MoveSpeed);
			settings.Air.ApplySpeedMultiplier(this.MoveSpeed);
		}
	}

	public void ModifyGravityPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (base.Active)
		{
			settings.GravityStrength *= this.GravityMultiplier;
		}
	}

	public void MakeInvincible(float duration)
	{
		this.m_invincibleTimeRemaining = Mathf.Max(this.m_invincibleTimeRemaining, duration);
	}

	public void MakeInvincibleToEnemies(float duration)
	{
		this.m_invincibleToEnemiesTimeRemaining = Mathf.Max(this.m_invincibleToEnemiesTimeRemaining, duration);
	}

	public void ResetInviciblity()
	{
		this.m_invincibleTimeRemaining = 0f;
		this.m_invincibleToEnemiesTimeRemaining = 0f;
	}

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
		RandomizerBonusSkill.UpdateDrain();
	}

	public IEnumerator FlashSprite()
	{
		yield return new WaitForFixedUpdate();
		for (int i = 0; i < 8; i++)
		{
			this.SpriteMaterialTintColor(Color.red);
			yield return new WaitForSeconds(0.05f);
			this.SpriteMaterialTintColor(new Color(0f, 0f, 0f, 0f));
			yield return new WaitForSeconds(0.05f);
		}
		yield break;
	}

	public void SpriteMaterialTintColor(Color color)
	{
		if (this.Sprite)
		{
			this.Sprite.sharedMaterial.SetColor(ShaderProperties.TintColor, color);
		}
	}

	public void OnEnable()
	{
		this.SpriteMaterialTintColor(new Color(0f, 0f, 0f, 0f));
		this.m_invincibleTimeRemaining = 0f;
		this.m_invincibleToEnemiesTimeRemaining = 0f;
	}

	public bool IsInvinsible
	{
		get
		{
			return this.m_invincibleTimeRemaining > 0f;
		}
	}

	public bool ShouldHurtAnimationKeepPlaying()
	{
		return !this.PlatformMovement.IsOnGround;
	}

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

	private void InstantiateDeathEffect(Damage damage)
	{
		GameObject gameObject = (GameObject)InstantiateUtility.Instantiate(this.DeathEffectProvider.Prefab(new DamageContext(damage)));
		damage.DealToComponents(gameObject);
		Transform transform = this.Sein.PlatformBehaviour.Visuals.SpriteMirror.transform;
		gameObject.transform.localPosition = transform.position;
		gameObject.transform.localScale = transform.localScale;
		gameObject.transform.localRotation = transform.localRotation;
	}

	public IEnumerator OnKillRoutine()
	{
		float deathDuration = this.DeathDuration;
		for (float t = 0f; t < deathDuration; t += ((!this.Sein.IsSuspended) ? Time.deltaTime : 0f))
		{
			if (Characters.Sein == null)
			{
				yield break;
			}
			yield return new WaitForFixedUpdate();
		}
		if (DifficultyController.Instance.Difficulty == DifficultyMode.OneLife)
		{
			InstantiateUtility.Instantiate(this.GameOverScreen, Vector3.zero, Quaternion.identity);
		}
		else
		{
			UI.Fader.Fade(this.DeathFadeInDuration, 0f, this.DeathFadeOutDuration, new Action(this.OnKillFadeInComplete), null);
		}
		yield return new WaitForSeconds(this.DeathFadeInDuration);
		SeinDeathCounter.SendTelemetryData();
		yield break;
	}

	public void OnKillFadeInComplete()
	{
		GameController.Instance.RestoreCheckpoint(null);
	}

	public bool CanDetonateProjectiles()
	{
		return this.m_invincibleToEnemiesTimeRemaining == 0f;
	}

	public override void Serialize(Archive ar)
	{
		base.Serialize(ar);
		ar.Serialize(ref this.m_serializationFiller);
	}

	public SeinCharacter Sein;

	public TextureAnimationWithTransitions HurtAnimation;

	public DamageBasedSoundProvider SeinDeathSound;

	public DamageBasedSoundProvider SeinHurtSound;

	public DamageBasedSoundProvider SeinBadlyHurtSound;

	public float BadlyHurtAmount = 4f;

	public bool IsImmortal;

	public float HurtDropPickupSpeed = 20f;

	private float m_invincibleTimeRemaining;

	private float m_invincibleToEnemiesTimeRemaining;

	private bool m_died;

	public GameObject GameOverScreen;

	public float HurtDuration = 0.25f;

	public Vector2 HurtSpeed = new Vector2(6f, 9f);

	public HorizontalPlatformMovementSettings.SpeedMultiplierSet MoveSpeed = new HorizontalPlatformMovementSettings.SpeedMultiplierSet
	{
		AccelerationMultiplier = 0f,
		DeceelerationMultiplier = 0f,
		MaxSpeedMultiplier = 1f
	};

	public float GravityMultiplier = 2f;

	public GameObject HurtEffect;

	public GameObject HurtDropPickup;

	private float m_hurtTimeRemaining;

	public GameObject KillFader;

	public float DeathDuration;

	public float OneLifeDeathDuration = 2f;

	public float SpawnDuration;

	public float DeathFadeInDuration = 0.05f;

	public float DeathFadeOutDuration = 1f;

	public DamageBasedPrefabProvider DeathEffectProvider;

	private int m_serializationFiller;
}
