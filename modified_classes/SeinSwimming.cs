using System;
using Core;
using Game;
using Sein.World;
using UnityEngine;

// Token: 0x02000037 RID: 55
public class SeinSwimming : CharacterState, ISeinReceiver
{
	// Token: 0x0600014D RID: 333
	public void ChangeState(SeinSwimming.State state)
	{
		if (this.CurrentState == SeinSwimming.State.SwimMovingUnderwater && this.UnderwaterSwimmingSoundProvider)
		{
			this.UnderwaterSwimmingSoundProvider.StopAndFadeOut(0.3f);
		}
		this.CurrentState = state;
	}

	// Token: 0x1700003B RID: 59
	// (get) Token: 0x0600014E RID: 334
	public bool IsUpsideDown
	{
		get
		{
			return Vector3.Dot(MoonMath.Angle.VectorFromAngle(this.SwimAngle), (!this.m_sein.Controller.FaceLeft) ? Vector3.left : Vector3.right) > Mathf.Cos(0.87266463f);
		}
	}

	// Token: 0x1700003C RID: 60
	// (get) Token: 0x0600014F RID: 335
	// (set) Token: 0x06000150 RID: 336
	public float RemainingBreath { get; set; }

	// Token: 0x1700003D RID: 61
	// (get) Token: 0x06000151 RID: 337
	public bool HasUnlimitedBreathingUnderwater
	{
		get
		{
			return this.m_sein.PlayerAbilities.WaterBreath.HasAbility;
		}
	}

	// Token: 0x1700003E RID: 62
	// (get) Token: 0x06000152 RID: 338
	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.m_sein.PlatformBehaviour.PlatformMovement;
		}
	}

	// Token: 0x1700003F RID: 63
	// (get) Token: 0x06000153 RID: 339
	public CharacterLeftRightMovement LeftRightMovement
	{
		get
		{
			return this.m_sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	// Token: 0x17000040 RID: 64
	// (get) Token: 0x06000154 RID: 340
	public CharacterGravity Gravity
	{
		get
		{
			return this.m_sein.PlatformBehaviour.Gravity;
		}
	}

	// Token: 0x17000041 RID: 65
	// (get) Token: 0x06000155 RID: 341
	public bool IsSwimming
	{
		get
		{
			return this.CurrentState > SeinSwimming.State.OutOfWater;
		}
	}

	// Token: 0x17000042 RID: 66
	// (get) Token: 0x06000156 RID: 342
	private float WaterSurfacePositionY
	{
		get
		{
			return this.m_currentWater.Bounds.yMax;
		}
	}

	// Token: 0x17000043 RID: 67
	// (get) Token: 0x06000157 RID: 343
	public Rect WaterSurfaceBound
	{
		get
		{
			Rect result = new Rect(this.m_currentWater.Bounds);
			result.yMin = result.yMax - 0.5f;
			result.yMax += ((!this.m_sein.PlatformBehaviour.PlatformMovement.IsOnGround) ? 0.5f : 0f);
			return result;
		}
	}

	// Token: 0x06000158 RID: 344
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.Abilities.Swimming = this;
	}

	// Token: 0x17000044 RID: 68
	// (get) Token: 0x06000159 RID: 345
	// (set) Token: 0x0600015A RID: 346
	public bool IsSuspended { get; set; }

	// Token: 0x17000045 RID: 69
	// (get) Token: 0x0600015B RID: 347
	public bool IsUnderwater
	{
		get
		{
			return this.CurrentState == SeinSwimming.State.SwimMovingUnderwater || this.CurrentState == SeinSwimming.State.SwimIdleUnderwater;
		}
	}

	// Token: 0x0600015C RID: 348
	public void HideBreathingUI()
	{
		for (int i = 0; i < this.m_breathingUIAnimators.Length; i++)
		{
			this.m_breathingUIAnimators[i].ContinueBackward();
		}
	}

	// Token: 0x0600015D RID: 349
	public void ShowBreathingUI()
	{
		for (int i = 0; i < this.m_breathingUIAnimators.Length; i++)
		{
			this.m_breathingUIAnimators[i].ContinueForward();
		}
	}

	// Token: 0x0600015E RID: 350
	public override void Awake()
	{
		base.Awake();
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
		this.m_breathingUIAnimators = this.BreathingUI.GetComponentsInChildren<LegacyAnimator>();
	}

	// Token: 0x0600015F RID: 351
	public void RestoreBreath()
	{
		this.RemainingBreath = this.Breath;
	}

	// Token: 0x06000160 RID: 352
	public void UpdateDrowning()
	{
		if (!Sein.World.Events.WaterPurified && this.CurrentState != SeinSwimming.State.OutOfWater)
		{
			this.RemainingBreath = 0f;
			this.HideBreathingUI();
		}
		if (this.HasUnlimitedBreathingUnderwater && Sein.World.Events.WaterPurified)
		{
			return;
		}
		if (this.m_sein.Controller.IsBashing)
		{
			return;
		}
		if (this.RemainingBreath > 0f)
		{
			this.RemainingBreath -= Time.deltaTime;
		}
		if (this.RemainingBreath <= 0f)
		{
			this.RemainingBreath = 0f;
			if (this.m_drowningDelay < 0f)
			{
				new Damage(this.DrownDamage, Vector2.zero, base.transform.position, DamageType.Drowning, base.gameObject).DealToComponents(Characters.Sein.Mortality.DamageReciever.gameObject);
				this.m_drowningDelay = this.DurationBetweenDrowningDamage;
			}
		}
	}

	// Token: 0x06000161 RID: 353
	public void Start()
	{
		this.LeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += this.ModifyHorizontalPlatformMovementSettings;
		this.Gravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyGravityPlatformMovementSettings;
	}

	// Token: 0x06000162 RID: 354
	public override void OnDestroy()
	{
		base.OnDestroy();
		this.LeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= this.ModifyHorizontalPlatformMovementSettings;
		this.Gravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyGravityPlatformMovementSettings;
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x06000163 RID: 355
	public override void Serialize(Archive ar)
	{
		this.CurrentState = (SeinSwimming.State)ar.Serialize((int)this.CurrentState);
		ar.Serialize(ref this.m_drowningDelay);
		this.RemainingBreath = ar.Serialize(this.RemainingBreath);
		ar.Serialize(ref this.m_swimIdleTime);
		ar.Serialize(ref this.m_swimMovingTime);
		ar.Serialize(ref this.SwimAngle);
		ar.Serialize(ref this.SmoothAngleDelta);
	}

	// Token: 0x06000164 RID: 356
	public void OnRestoreCheckpoint()
	{
		this.RestoreBreath();
	}

	// Token: 0x06000165 RID: 357
	public void ModifyHorizontalPlatformMovementSettings(HorizontalPlatformMovementSettings settings)
	{
		SeinSwimming.State currentState = this.CurrentState;
		if (currentState == SeinSwimming.State.SwimmingOnSurface)
		{
			settings.Air.ApplySpeedMultiplier(this.SwimmingOnSurfaceHorizontalSpeed);
			settings.Ground.ApplySpeedMultiplier(this.SwimmingOnSurfaceHorizontalSpeed);
			return;
		}
		if (currentState - SeinSwimming.State.SwimMovingUnderwater > 1)
		{
			return;
		}
		settings.Air.Acceleration = 0f;
		settings.Air.Decceleration = 0f;
		settings.Air.MaxSpeed = float.PositiveInfinity;
		settings.Ground.Acceleration = 0f;
		settings.Ground.Decceleration = 0f;
		settings.Ground.MaxSpeed = float.PositiveInfinity;
	}

	// Token: 0x06000166 RID: 358
	public void ModifyGravityPlatformMovementSettings(GravityPlatformMovementSettings settings)
	{
		if (this.CurrentState == SeinSwimming.State.SwimmingOnSurface)
		{
			settings.GravityStrength = 0f;
			settings.MaxFallSpeed = 0f;
		}
		if (this.CurrentState == SeinSwimming.State.SwimMovingUnderwater || this.CurrentState == SeinSwimming.State.SwimIdleUnderwater)
		{
			settings.GravityStrength = 0f;
		}
	}

	// Token: 0x06000167 RID: 359
	public override void UpdateCharacterState()
	{
		if (this.m_drowningDelay >= 0f)
		{
			this.m_drowningDelay -= Time.deltaTime;
		}
		switch (this.CurrentState)
		{
		case SeinSwimming.State.OutOfWater:
			this.UpdateOutOfWaterState();
			return;
		case SeinSwimming.State.SwimmingOnSurface:
			this.UpdateSwimmingOnSurfaceState();
			return;
		case SeinSwimming.State.SwimMovingUnderwater:
			this.UpdateSwimMovingUnderwaterState();
			return;
		case SeinSwimming.State.SwimIdleUnderwater:
			this.UpdateSwimIdleUnderwaterState();
			return;
		default:
			return;
		}
	}

	// Token: 0x06000168 RID: 360
	public void GetOutOfWater()
	{
		Sound.Play(this.OutOfWaterSoundProvider.GetSound(null), this.m_sein.transform.position, null);
		InstantiateUtility.Instantiate(this.WaterSplashPrefab, this.m_sein.transform.position, Quaternion.identity);
		this.ChangeState(SeinSwimming.State.OutOfWater);
		this.RemainingBreath = this.Breath;
	}

	// Token: 0x06000169 RID: 361
	public void SwimUnderwater()
	{
		this.ChangeState(SeinSwimming.State.SwimMovingUnderwater);
		this.SwimAngle = 270f;
		this.m_swimIdleTime = 0f;
		this.m_swimMovingTime = 0f;
		this.m_swimAccelerationTime = 0f;
		Sound.Play(this.InWaterSoundProvider.GetSound(null), this.m_sein.transform.position, null);
		if (this.m_sein.Abilities.Bash != null && this.m_sein.Abilities.Bash.IsBashing)
		{
			Sound.Play(this.BashIntoWaterSoundProvider.GetSound(null), this.m_sein.transform.position, null);
		}
		if (this.m_sein.Abilities.Stomp && this.m_sein.Abilities.Stomp.IsStomping)
		{
			Sound.Play(this.StompIntoWaterSoundProvider.GetSound(null), this.m_sein.transform.position, null);
		}
		InstantiateUtility.Instantiate(this.WaterSplashPrefab, this.m_sein.transform.position, Quaternion.identity);
		if (!this.HasUnlimitedBreathingUnderwater)
		{
			this.RemainingBreath = this.Breath;
			this.ShowBreathingUI();
		}
	}

	// Token: 0x0600016A RID: 362
	public void RemoveUnderwaterSounds()
	{
		if (this.m_ambienceLayer != null)
		{
			Ambience.RemoveAmbienceLayer(this.m_ambienceLayer);
			this.m_ambienceLayer = null;
			this.UnderwaterMixerSnapshot.FadeOut();
		}
	}

	// Token: 0x0600016B RID: 363
	public void UpdateOutOfWaterState()
	{
		Vector3 headPosition = this.m_sein.PlatformBehaviour.PlatformMovement.HeadPosition;
		this.RemoveUnderwaterSounds();
		int i = 0;
		while (i < Zones.WaterZones.Count)
		{
			WaterZone waterZone = Zones.WaterZones[i];
			if (waterZone.Bounds.Contains(headPosition))
			{
				this.m_currentWater = waterZone;
				this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeedX *= 0.5f;
				if (Mathf.Abs(this.PlatformMovement.LocalSpeedY) <= this.SkipSurfaceSpeedIn && this.WaterSurfaceBound.Contains(this.PlatformMovement.Position))
				{
					this.SwimOnSurface();
					return;
				}
				if (this.PlatformMovement.LocalSpeedY < 0f)
				{
					this.SwimUnderwater();
					this.PlatformMovement.LocalSpeedY *= 0.8f;
					return;
				}
				this.m_currentWater = null;
				return;
			}
			else
			{
				i++;
			}
		}
	}

	// Token: 0x0600016C RID: 364
	public void SwimOnSurface()
	{
		this.PlatformMovement.PositionY = this.WaterSurfacePositionY;
		this.PlatformMovement.LocalSpeedY = 0f;
		this.ChangeState(SeinSwimming.State.SwimmingOnSurface);
		if (this.m_sein.Abilities.Carry && this.m_sein.Abilities.Carry.IsCarrying)
		{
			Damage damage = new Damage(1000f, (this.m_sein.transform.position - base.transform.position).normalized, base.transform.position, DamageType.Water, base.gameObject);
			this.m_sein.Mortality.DamageReciever.OnRecieveDamage(damage);
		}
		Sound.Play(this.OutOfWaterSoundProvider.GetSound(null), this.m_sein.transform.position, null);
		InstantiateUtility.Instantiate(this.WaterSplashPrefab, this.m_sein.transform.position, Quaternion.identity);
		this.RestoreBreath();
		this.HideBreathingUI();
	}

	// Token: 0x0600016D RID: 365
	public void OnDisable()
	{
		this.RemoveUnderwaterSounds();
	}

	// Token: 0x0600016E RID: 366
	public void UpdateSwimmingOnSurfaceState()
	{
		if (!Sein.World.Events.WaterPurified)
		{
			this.UpdateDrowning();
		}
		this.RemoveUnderwaterSounds();
		if (this.m_currentWater == null)
		{
			this.GetOutOfWater();
			return;
		}
		Vector2 point = this.m_sein.PlatformBehaviour.PlatformMovement.Position;
		if (this.WaterSurfaceBound.Contains(point))
		{
			this.PlatformMovement.Ground.IsOn = false;
			this.PlatformMovement.GroundNormal = Vector3.up;
			this.PlatformMovement.PositionY = this.WaterSurfacePositionY;
			this.PlatformMovement.LocalSpeedY = 0f;
			this.PlatformMovement.LocalSpeed *= RandomizerBonusSkill.ExtremeSpeed; 

			this.m_sein.PlatformBehaviour.Visuals.Animation.PlayLoop((this.m_sein.Input.NormalizedHorizontal != 0) ? this.Animations.SwimSurface.Moving : this.Animations.SwimSurface.Idle, 9, new Func<bool>(this.ShouldSwimSurfaceAnimationPlay), false);
			if (this.SurfaceSwimmingSoundProvider && !this.SurfaceSwimmingSoundProvider.IsPlaying && this.m_sein.Input.NormalizedHorizontal != 0)
			{
				this.SurfaceSwimmingSoundProvider.Play();
			}
			if (this.m_sein.Controller.CanMove && !this.m_sein.Controller.IsBashing)
			{
				if (this.m_sein.Input.Down.Pressed)
				{
					this.SwimUnderwater();
					this.PlatformMovement.LocalSpeedY = -this.DiveUnderwaterSpeed;
				}
				if (Core.Input.Jump.OnPressed)
				{
					this.SurfaceSwimJump();
				}
			}
			return;
		}
		this.GetOutOfWater();
	}

	// Token: 0x0600016F RID: 367
	public void HorizontalFlip()
	{
		this.m_swimMovingTime = 0f;
		this.m_boostAnimationRemainingTime = 0f;
		this.SwimAngle += 180f;
		this.m_sein.Controller.FaceLeft = !this.m_sein.Controller.FaceLeft;
		this.m_sein.PlatformBehaviour.Visuals.Animation.Play(this.Animations.SwimFlipHorizontalAnimation, 10, new Func<bool>(this.ShouldSwimUnderwaterAnimationPlay));
	}

	// Token: 0x06000170 RID: 368
	public void VerticalFlip()
	{
		this.m_boostAnimationRemainingTime = 0f;
		this.m_swimMovingTime = 0f;
		this.m_sein.Controller.FaceLeft = !this.m_sein.Controller.FaceLeft;
		this.m_sein.PlatformBehaviour.Visuals.Animation.Play(this.Animations.SwimFlipVerticalAnimation, 10, new Func<bool>(this.ShouldSwimUnderwaterAnimationPlay));
	}

	// Token: 0x06000171 RID: 369
	public void HorizontalVerticalFlip()
	{
		this.m_swimMovingTime = 0f;
		this.m_boostAnimationRemainingTime = 0f;
		this.SwimAngle += 180f;
		this.m_sein.PlatformBehaviour.Visuals.Animation.Play(this.Animations.SwimFlipHorizontalVerticalAnimation, 10, new Func<bool>(this.ShouldSwimUnderwaterAnimationPlay));
	}

	// Token: 0x06000172 RID: 370
	public void OnBash(float angle)
	{
		if (this.IsUnderwater)
		{
			angle += 90f;
			this.SwimAngle = angle;
			this.m_sein.Controller.FaceLeft = (MoonMath.Angle.VectorFromAngle(angle).x < 0f);
			this.m_swimAccelerationTime = -this.BashTime;
			this.ChangeState(SeinSwimming.State.SwimIdleUnderwater);
		}
	}

	// Token: 0x06000173 RID: 371
	public void ApplySwimmingUnderwaterStuff()
	{
		if (this.m_ambienceLayer == null)
		{
			this.m_ambienceLayer = new Ambience.Layer(this.SwimmingUnderwaterAmbience, 0.7f, 0.7f, 5);
			Ambience.AddAmbienceLayer(this.m_ambienceLayer);
			this.UnderwaterMixerSnapshot.FadeIn();
		}
	}

	// Token: 0x06000174 RID: 372
	public void UpdateSwimMovingUnderwaterState()
	{
		this.UpdateDrowning();
		if (this.UnderwaterSwimmingSoundProvider && !this.UnderwaterSwimmingSoundProvider.IsPlaying)
		{
			this.UnderwaterSwimmingSoundProvider.Play();
		}
		this.m_sein.PlatformBehaviour.PlatformMovement.ForceKeepInAir = true;
		Vector2 vector = (!this.m_sein.Controller.CanMove) ? Vector2.zero : this.m_sein.Input.Axis;
		this.m_swimAccelerationTime += 2f * Time.deltaTime;
		Vector2 vector2 = Vector3.down * this.MaxFallSpeed;
		if (vector.magnitude > 0.3f)
		{
			this.m_swimIdleTime = 0f;
			vector.Normalize();
			float swimAngle = this.SwimAngle;
			Vector2 v = MoonMath.Angle.VectorFromAngle(this.SwimAngle);
			if (Vector3.Dot(-vector, v) > Mathf.Cos(1.04719758f))
			{
				if (this.IsUpsideDown)
				{
					this.HorizontalVerticalFlip();
				}
				else
				{
					this.HorizontalFlip();
				}
			}
			else
			{
				float target = MoonMath.Angle.AngleFromVector(vector);
				this.SwimAngle = Mathf.MoveTowardsAngle(this.SwimAngle, target, this.SwimAngleDeltaLimit * Time.deltaTime);
				vector = MoonMath.Angle.VectorFromAngle(this.SwimAngle);
				vector2 = vector * this.SwimSpeed;
				if (this.m_sein.Controller.CanMove && RandomizerSettings.IsSwimBoosting())
				{
					this.m_isBoosting = true;
					this.m_boostTime = Mathf.Min(this.m_boostTime, this.BoostPeakTime);
				}
				if (this.m_sein.Controller.CanMove && RandomizerSettings.SwimBoostPressed() && this.m_boostAnimationRemainingTime <= 0f && this.BoostSwimsoundProvider)
				{
					Sound.Play(this.BoostSwimsoundProvider.GetSound(null), base.transform.position, null);
					this.m_boostAnimationRemainingTime = 0.6666667f;
				}
				if (this.m_isBoosting)
				{
					this.m_boostTime += Time.deltaTime / this.BoostDuration;
					vector2 *= this.SwimSpeedBoostCurve.Evaluate(this.m_boostTime);
				}
				if (this.m_isBoosting && this.m_boostTime > this.BoostDuration)
				{
					this.m_isBoosting = false;
					this.m_boostTime = 0f;
				}
			}
			float b = MoonMath.Angle.AngleSubtract(this.SwimAngle, swimAngle) / Time.deltaTime;
			this.SmoothAngleDelta = Mathf.Lerp(this.SmoothAngleDelta, b, 0.1f);
		}
		else
		{
			if (this.m_swimAccelerationTime > 0f)
			{
				this.m_swimAccelerationTime = 0f;
			}
			if (this.m_isBoosting)
			{
				this.m_isBoosting = false;
				this.m_boostTime = 0f;
				this.m_boostAnimationRemainingTime = 0f;
			}
			if (this.m_swimIdleTime > 0.1f)
			{
				this.m_swimMovingTime = 0f;
				if (this.m_swimAccelerationTime > 0f)
				{
					this.m_swimAccelerationTime = 0f;
				}
				if (this.IsUpsideDown)
				{
					this.VerticalFlip();
				}
				bool faceLeft = this.m_sein.Controller.FaceLeft;
				float target2 = (float)((!faceLeft) ? 0 : 180);
				if (MoonMath.Angle.AngleSubtract(this.SwimAngle, target2) > 0f)
				{
					this.m_sein.PlatformBehaviour.Visuals.Animation.Play(faceLeft ? this.Animations.SwimMiddleToIdleClockwise : this.Animations.SwimMiddleToIdleAntiClockwise, 10, new Func<bool>(this.ShouldIdleUnderwaterAnimationPlay));
				}
				else
				{
					this.m_sein.PlatformBehaviour.Visuals.Animation.Play((!faceLeft) ? this.Animations.SwimMiddleToIdleClockwise : this.Animations.SwimMiddleToIdleAntiClockwise, 10, new Func<bool>(this.ShouldIdleUnderwaterAnimationPlay));
				}
				this.ChangeState(SeinSwimming.State.SwimIdleUnderwater);
			}
			this.m_swimIdleTime += Time.deltaTime;
		}
		this.PlatformMovement.LocalSpeed = Vector3.Lerp(this.PlatformMovement.LocalSpeed, vector2, this.AccelerationOverTime.Evaluate(this.m_swimAccelerationTime));
		this.PlatformMovement.LocalSpeed *= RandomizerBonusSkill.ExtremeSpeed; 
		if (this.IsUpsideDown && Math.Abs(this.SmoothAngleDelta) < 10f)
		{
			this.VerticalFlip();
		}
		this.ApplySwimmingUnderwaterStuff();
		if (this.m_boostAnimationRemainingTime > 0f)
		{
			this.m_boostAnimationRemainingTime -= Time.deltaTime;
			int min = Mathf.RoundToInt(this.Animations.AnimationFromBend.Evaluate(this.SmoothAngleDelta * (float)((!this.m_sein.Controller.FaceLeft) ? -1 : 1)) * (float)(this.Animations.SwimJumpLeft.Length - 1));
			int num = Mathf.Clamp(0, min, this.Animations.SwimJumpLeft.Length - 1);
			this.m_sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.Animations.SwimJumpLeft[num], 9, new Func<bool>(this.ShouldSwimUnderwaterAnimationPlay), true);
		}
		else
		{
			int min2 = Mathf.RoundToInt(this.Animations.AnimationFromBend.Evaluate(this.SmoothAngleDelta * (float)((!this.m_sein.Controller.FaceLeft) ? -1 : 1)) * (float)(this.Animations.SwimHorizontal.Length - 1));
			int num2 = Mathf.Clamp(0, min2, this.Animations.SwimHorizontal.Length - 1);
			this.m_sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.Animations.SwimHorizontal[num2], 9, new Func<bool>(this.ShouldSwimUnderwaterAnimationPlay), true);
		}
		this.HandleLeavingWater();
	}

	// Token: 0x06000175 RID: 373
	public void UpdateSwimIdleUnderwaterState()
	{
		this.UpdateDrowning();
		Vector2 vector = (!this.m_sein.Controller.CanMove) ? Vector2.zero : this.m_sein.Input.Axis;
		this.m_swimAccelerationTime += Time.deltaTime;
		if (vector.magnitude > 0.3f)
		{
			if (this.m_swimAccelerationTime > 0f)
			{
				this.m_swimAccelerationTime = 0f;
			}
			this.m_swimIdleTime = 0f;
			this.ChangeState(SeinSwimming.State.SwimMovingUnderwater);
		}
		else
		{
			float target = (float)((!this.m_sein.Controller.FaceLeft) ? 0 : 180);
			this.SwimAngle = Mathf.MoveTowardsAngle(this.SwimAngle, target, this.SwimAngleDeltaLimit * Time.deltaTime);
			this.m_sein.PlatformBehaviour.Visuals.Animation.PlayLoop(this.Animations.SwimIdle, 9, new Func<bool>(this.ShouldIdleUnderwaterAnimationPlay), true);
		}
		this.PlatformMovement.LocalSpeed = Vector3.Lerp(this.PlatformMovement.LocalSpeed, Vector3.down * this.MaxFallSpeed, this.AccelerationOverTime.Evaluate(this.m_swimAccelerationTime));
		this.ApplySwimmingUnderwaterStuff();
		this.HandleLeavingWater();
	}

	// Token: 0x06000176 RID: 374
	public void HandleLeavingWater()
	{
		Vector3 position = this.m_sein.PlatformBehaviour.PlatformMovement.Position;
		for (int i = 0; i < Zones.WaterZones.Count; i++)
		{
			WaterZone waterZone = Zones.WaterZones[i];
			if (waterZone.Bounds.Contains(position))
			{
				this.m_currentWater = waterZone;
				return;
			}
		}
		if (this.RemainingBreath / this.Breath > 0.5f)
		{
			if (this.EmergeHighBreathSoundProvider)
			{
				Sound.Play(this.EmergeHighBreathSoundProvider.GetSound(null), base.transform.position, null);
			}
		}
		else if (this.RemainingBreath / this.Breath > 0.15f)
		{
			if (this.EmergeMedBreathSoundProvider)
			{
				Sound.Play(this.EmergeMedBreathSoundProvider.GetSound(null), base.transform.position, null);
			}
		}
		else if (this.EmergeLowBreathSoundProvider)
		{
			Sound.Play(this.EmergeLowBreathSoundProvider.GetSound(null), base.transform.position, null);
		}
		this.RestoreBreath();
		this.HideBreathingUI();
		if (this.m_currentWater.HasTopSurface && this.WaterSurfaceBound.Contains(this.PlatformMovement.Position))
		{
			this.SwimOnSurface();
			return;
		}
		this.GetOutOfWater();
	}

	// Token: 0x06000177 RID: 375
	public bool CanJump()
	{
		return this.CurrentState == SeinSwimming.State.SwimmingOnSurface || this.CurrentState == SeinSwimming.State.SwimMovingUnderwater;
	}

	// Token: 0x06000178 RID: 376
	public void SurfaceSwimJump()
	{
		this.PlatformMovement.LocalSpeedY = this.JumpOutOfWaterSpeed;
		if (this.m_sein.Input.NormalizedHorizontal == 0)
		{
			this.m_sein.PlatformBehaviour.Visuals.Animation.Play(this.Animations.JumpOutOfWater.Idle, 10, new Func<bool>(this.ShouldJumpOutOfWaterAnimationIdleKeepPlaying));
		}
		else
		{
			this.m_sein.PlatformBehaviour.Visuals.Animation.Play(this.Animations.JumpOutOfWater.Moving, 10, new Func<bool>(this.ShouldJumpOutOfWaterAnimationMovingKeepPlaying));
		}
		this.m_sein.ResetAirLimits();
		this.GetOutOfWater();
	}

	// Token: 0x06000179 RID: 377
	public bool ShouldSwimUnderwaterAnimationPlay()
	{
		return this.CurrentState == SeinSwimming.State.SwimMovingUnderwater;
	}

	// Token: 0x0600017A RID: 378
	public bool ShouldIdleUnderwaterAnimationPlay()
	{
		return this.CurrentState == SeinSwimming.State.SwimIdleUnderwater;
	}

	// Token: 0x0600017B RID: 379
	public bool ShouldSwimSurfaceAnimationPlay()
	{
		return this.CurrentState == SeinSwimming.State.SwimmingOnSurface;
	}

	// Token: 0x0600017C RID: 380
	public bool ShouldJumpOutOfWaterAnimationIdleKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && (!this.m_sein.Controller.CanMove || this.m_sein.Input.NormalizedHorizontal == 0) && (!this.IsSwimming || !this.PlatformMovement.Falling);
	}

	// Token: 0x0600017D RID: 381
	public bool ShouldJumpOutOfWaterAnimationMovingKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && (!this.m_sein.Controller.CanMove || this.m_sein.Input.NormalizedHorizontal != 0) && (!this.IsSwimming || !this.PlatformMovement.Falling);
	}

	// Token: 0x0400018E RID: 398
	public SoundProvider SwimmingUnderwaterAmbience;

	// Token: 0x0400018F RID: 399
	public MixerSnapshot UnderwaterMixerSnapshot;

	// Token: 0x04000190 RID: 400
	public SeinSwimming.State CurrentState;

	// Token: 0x04000191 RID: 401
	public SeinSwimming.SwimmingAnimations Animations;

	// Token: 0x04000192 RID: 402
	public float Breath = 3f;

	// Token: 0x04000193 RID: 403
	public GameObject BreathingUI;

	// Token: 0x04000194 RID: 404
	public float DiveUnderwaterSpeed = 3f;

	// Token: 0x04000195 RID: 405
	public float DurationBetweenDrowningDamage = 1f;

	// Token: 0x04000196 RID: 406
	public SoundProvider InWaterSoundProvider;

	// Token: 0x04000197 RID: 407
	public SoundProvider BashIntoWaterSoundProvider;

	// Token: 0x04000198 RID: 408
	public SoundProvider StompIntoWaterSoundProvider;

	// Token: 0x04000199 RID: 409
	public float JumpOutOfWaterSpeed = 20f;

	// Token: 0x0400019A RID: 410
	public SoundProvider OutOfWaterSoundProvider;

	// Token: 0x0400019B RID: 411
	public float SkipSurfaceSpeedIn = 20f;

	// Token: 0x0400019C RID: 412
	public float SkipSurfaceSpeedOut = 10f;

	// Token: 0x0400019D RID: 413
	public SoundSource SurfaceSwimmingSoundProvider;

	// Token: 0x0400019E RID: 414
	public SoundSource UnderwaterSwimmingSoundProvider;

	// Token: 0x0400019F RID: 415
	public SoundProvider EmergeHighBreathSoundProvider;

	// Token: 0x040001A0 RID: 416
	public SoundProvider EmergeMedBreathSoundProvider;

	// Token: 0x040001A1 RID: 417
	public SoundProvider EmergeLowBreathSoundProvider;

	// Token: 0x040001A2 RID: 418
	public SoundProvider BoostSwimsoundProvider;

	// Token: 0x040001A3 RID: 419
	public float SwimGravity = 13f;

	// Token: 0x040001A4 RID: 420
	public float SwimSpeed = 6f;

	// Token: 0x040001A5 RID: 421
	public AnimationCurve SwimSpeedBoostCurve;

	// Token: 0x040001A6 RID: 422
	public float BoostPeakTime = 0.2f;

	// Token: 0x040001A7 RID: 423
	private float m_boostTime;

	// Token: 0x040001A8 RID: 424
	public float BoostDuration;

	// Token: 0x040001A9 RID: 425
	private bool m_isBoosting;

	// Token: 0x040001AA RID: 426
	public float SwimAngle;

	// Token: 0x040001AB RID: 427
	public float SwimAngleDeltaLimit = 100f;

	// Token: 0x040001AC RID: 428
	private float m_swimMovingTime;

	// Token: 0x040001AD RID: 429
	private float m_swimIdleTime;

	// Token: 0x040001AE RID: 430
	private float m_swimAccelerationTime;

	// Token: 0x040001AF RID: 431
	public float SwimUpwardsGravity = 13f;

	// Token: 0x040001B0 RID: 432
	public HorizontalPlatformMovementSettings.SpeedMultiplierSet SwimmingOnSurfaceHorizontalSpeed;

	// Token: 0x040001B1 RID: 433
	public GameObject WaterSplashPrefab;

	// Token: 0x040001B2 RID: 434
	private WaterZone m_currentWater;

	// Token: 0x040001B3 RID: 435
	private float m_drowningDelay;

	// Token: 0x040001B4 RID: 436
	private SeinCharacter m_sein;

	// Token: 0x040001B5 RID: 437
	private LegacyAnimator[] m_breathingUIAnimators;

	// Token: 0x040001B6 RID: 438
	public float DrownDamage = 5f;

	// Token: 0x040001B7 RID: 439
	private Ambience.Layer m_ambienceLayer;

	// Token: 0x040001B8 RID: 440
	public bool CanHorizontalSwimJump;

	// Token: 0x040001B9 RID: 441
	public float Deceleration = 10f;

	// Token: 0x040001BA RID: 442
	public float MaxFallSpeed = 4f;

	// Token: 0x040001BB RID: 443
	public float BashTime = 1f;

	// Token: 0x040001BC RID: 444
	public float SmoothAngleDelta;

	// Token: 0x040001BD RID: 445
	public AnimationCurve AccelerationOverTime;

	// Token: 0x040001BE RID: 446
	private float m_boostAnimationRemainingTime;

	// Token: 0x040001BF RID: 447
	public bool CanJumpUnderwater;

	// Token: 0x040001C0 RID: 448
	public bool HoldAToSwimLoop;

	// Token: 0x040001C1 RID: 449
	public float SwimJumpSpeedDelta = 100f;

	// Token: 0x02000038 RID: 56
	[Serializable]
	public class MovingAndIdleAnimationPair
	{
		// Token: 0x040001C4 RID: 452
		public TextureAnimationWithTransitions Idle;

		// Token: 0x040001C5 RID: 453
		public TextureAnimationWithTransitions Moving;
	}

	// Token: 0x02000039 RID: 57
	public enum State
	{
		// Token: 0x040001C7 RID: 455
		OutOfWater,
		// Token: 0x040001C8 RID: 456
		SwimmingOnSurface,
		// Token: 0x040001C9 RID: 457
		SwimMovingUnderwater,
		// Token: 0x040001CA RID: 458
		SwimIdleUnderwater
	}

	// Token: 0x0200003A RID: 58
	[Serializable]
	public class SwimmingAnimations
	{
		// Token: 0x040001CB RID: 459
		public SeinSwimming.MovingAndIdleAnimationPair JumpOutOfWater;

		// Token: 0x040001CC RID: 460
		public SeinSwimming.MovingAndIdleAnimationPair SwimSurface;

		// Token: 0x040001CD RID: 461
		public TextureAnimationWithTransitions[] SwimHorizontal;

		// Token: 0x040001CE RID: 462
		public TextureAnimationWithTransitions[] SwimJumpLeft;

		// Token: 0x040001CF RID: 463
		public AnimationCurve AnimationFromBend;

		// Token: 0x040001D0 RID: 464
		public TextureAnimationWithTransitions SwimIdle;

		// Token: 0x040001D1 RID: 465
		public TextureAnimationWithTransitions SwimMiddleToIdleClockwise;

		// Token: 0x040001D2 RID: 466
		public TextureAnimationWithTransitions SwimMiddleToIdleAntiClockwise;

		// Token: 0x040001D3 RID: 467
		public TextureAnimationWithTransitions SwimIdleToSwimMiddle;

		// Token: 0x040001D4 RID: 468
		public TextureAnimationWithTransitions SwimFlipHorizontalAnimation;

		// Token: 0x040001D5 RID: 469
		public TextureAnimationWithTransitions SwimFlipVerticalAnimation;

		// Token: 0x040001D6 RID: 470
		public TextureAnimationWithTransitions SwimFlipHorizontalVerticalAnimation;
	}
}
