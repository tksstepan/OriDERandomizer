using System;
using Core;
using Game;
using Sein.World;
using UnityEngine;

public class SeinSwimming : CharacterState, ISeinReceiver
{
	public void ChangeState(SeinSwimming.State state)
	{
		if (this.CurrentState == SeinSwimming.State.SwimMovingUnderwater && this.UnderwaterSwimmingSoundProvider)
		{
			this.UnderwaterSwimmingSoundProvider.StopAndFadeOut(0.3f);
		}
		this.CurrentState = state;
	}

	public bool IsUpsideDown
	{
		get
		{
			return Vector3.Dot(MoonMath.Angle.VectorFromAngle(this.SwimAngle), (!this.m_sein.Controller.FaceLeft) ? Vector3.left : Vector3.right) > Mathf.Cos(0.87266463f);
		}
	}

	public float RemainingBreath { get; set; }

	public bool HasUnlimitedBreathingUnderwater
	{
		get
		{
			return this.m_sein.PlayerAbilities.WaterBreath.HasAbility;
		}
	}

	public PlatformMovement PlatformMovement
	{
		get
		{
			return this.m_sein.PlatformBehaviour.PlatformMovement;
		}
	}

	public CharacterLeftRightMovement LeftRightMovement
	{
		get
		{
			return this.m_sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	public CharacterGravity Gravity
	{
		get
		{
			return this.m_sein.PlatformBehaviour.Gravity;
		}
	}

	public bool IsSwimming
	{
		get
		{
			return this.CurrentState > SeinSwimming.State.OutOfWater;
		}
	}

	private float WaterSurfacePositionY
	{
		get
		{
			return this.m_currentWater.Bounds.yMax;
		}
	}

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

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.Abilities.Swimming = this;
	}

	public bool IsSuspended { get; set; }

	public bool IsUnderwater
	{
		get
		{
			return this.CurrentState == SeinSwimming.State.SwimMovingUnderwater || this.CurrentState == SeinSwimming.State.SwimIdleUnderwater;
		}
	}

	public void HideBreathingUI()
	{
		for (int i = 0; i < this.m_breathingUIAnimators.Length; i++)
		{
			this.m_breathingUIAnimators[i].ContinueBackward();
		}
	}

	public void ShowBreathingUI()
	{
		for (int i = 0; i < this.m_breathingUIAnimators.Length; i++)
		{
			this.m_breathingUIAnimators[i].ContinueForward();
		}
	}

	public override void Awake()
	{
		base.Awake();
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
		this.m_breathingUIAnimators = this.BreathingUI.GetComponentsInChildren<LegacyAnimator>();
	}

	public void RestoreBreath()
	{
		this.RemainingBreath = this.Breath;
	}

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

	public void Start()
	{
		this.LeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += this.ModifyHorizontalPlatformMovementSettings;
		this.Gravity.ModifyGravityPlatformMovementSettingsEvent += this.ModifyGravityPlatformMovementSettings;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		this.LeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= this.ModifyHorizontalPlatformMovementSettings;
		this.Gravity.ModifyGravityPlatformMovementSettingsEvent -= this.ModifyGravityPlatformMovementSettings;
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

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

	public void OnRestoreCheckpoint()
	{
		this.RestoreBreath();
	}

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

	public void GetOutOfWater()
	{
		Sound.Play(this.OutOfWaterSoundProvider.GetSound(null), this.m_sein.transform.position, null);
		InstantiateUtility.Instantiate(this.WaterSplashPrefab, this.m_sein.transform.position, Quaternion.identity);
		this.ChangeState(SeinSwimming.State.OutOfWater);
		this.RemainingBreath = this.Breath;
	}

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

	public void RemoveUnderwaterSounds()
	{
		if (this.m_ambienceLayer != null)
		{
			Ambience.RemoveAmbienceLayer(this.m_ambienceLayer);
			this.m_ambienceLayer = null;
			this.UnderwaterMixerSnapshot.FadeOut();
		}
	}

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

	public void OnDisable()
	{
		this.RemoveUnderwaterSounds();
	}

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

	public void HorizontalFlip()
	{
		this.m_swimMovingTime = 0f;
		this.m_boostAnimationRemainingTime = 0f;
		this.SwimAngle += 180f;
		this.m_sein.Controller.FaceLeft = !this.m_sein.Controller.FaceLeft;
		this.m_sein.PlatformBehaviour.Visuals.Animation.Play(this.Animations.SwimFlipHorizontalAnimation, 10, new Func<bool>(this.ShouldSwimUnderwaterAnimationPlay));
	}

	public void VerticalFlip()
	{
		this.m_boostAnimationRemainingTime = 0f;
		this.m_swimMovingTime = 0f;
		this.m_sein.Controller.FaceLeft = !this.m_sein.Controller.FaceLeft;
		this.m_sein.PlatformBehaviour.Visuals.Animation.Play(this.Animations.SwimFlipVerticalAnimation, 10, new Func<bool>(this.ShouldSwimUnderwaterAnimationPlay));
	}

	public void HorizontalVerticalFlip()
	{
		this.m_swimMovingTime = 0f;
		this.m_boostAnimationRemainingTime = 0f;
		this.SwimAngle += 180f;
		this.m_sein.PlatformBehaviour.Visuals.Animation.Play(this.Animations.SwimFlipHorizontalVerticalAnimation, 10, new Func<bool>(this.ShouldSwimUnderwaterAnimationPlay));
	}

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

	public void ApplySwimmingUnderwaterStuff()
	{
		if (this.m_ambienceLayer == null)
		{
			this.m_ambienceLayer = new Ambience.Layer(this.SwimmingUnderwaterAmbience, 0.7f, 0.7f, 5);
			Ambience.AddAmbienceLayer(this.m_ambienceLayer);
			this.UnderwaterMixerSnapshot.FadeIn();
		}
	}

	public Vector2 GetAxisInput()
	{
		if (!this.m_sein.Controller.CanMove)
		{
			return Vector2.zero;
		}

		if (this.m_sein.Input.Axis.magnitude > 0.3f)
		{
			return this.m_sein.Input.Axis;
		}

		if (RandomizerSettings.SwimmingMouseAim)
		{
			Vector2 oriScreenPos = UI.Cameras.Current.Camera.WorldToScreenPoint(this.PlatformMovement.Position);
			Vector2 oriUIPos = UI.Cameras.System.GUICamera.Camera.ScreenToWorldPoint(oriScreenPos);
			Vector2 cursorAxis = Core.Input.CursorPositionUI - oriUIPos;

			if (cursorAxis.magnitude > 0.5f)
			{
				return cursorAxis;
			}
		}

		return Vector2.zero;
	}

	public void UpdateSwimMovingUnderwaterState()
	{
		this.UpdateDrowning();
		if (this.UnderwaterSwimmingSoundProvider && !this.UnderwaterSwimmingSoundProvider.IsPlaying)
		{
			this.UnderwaterSwimmingSoundProvider.Play();
		}
		this.m_sein.PlatformBehaviour.PlatformMovement.ForceKeepInAir = true;
		Vector2 vector = this.GetAxisInput();
		this.m_swimAccelerationTime += 2f * Time.deltaTime;
		Vector2 vector2 = Vector3.down * this.MaxFallSpeed;
		if (vector != Vector2.zero)
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

	public void UpdateSwimIdleUnderwaterState()
	{
		this.UpdateDrowning();
		Vector2 vector = this.GetAxisInput();
		this.m_swimAccelerationTime += Time.deltaTime;
		if (vector != Vector2.zero)
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

	public bool CanJump()
	{
		return this.CurrentState == SeinSwimming.State.SwimmingOnSurface || this.CurrentState == SeinSwimming.State.SwimMovingUnderwater;
	}

	public void SurfaceSwimJump()
	{
		this.PlatformMovement.LocalSpeedY = this.JumpOutOfWaterSpeed * RandomizerBonus.Jumpscale;
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

	public bool ShouldSwimUnderwaterAnimationPlay()
	{
		return this.CurrentState == SeinSwimming.State.SwimMovingUnderwater;
	}

	public bool ShouldIdleUnderwaterAnimationPlay()
	{
		return this.CurrentState == SeinSwimming.State.SwimIdleUnderwater;
	}

	public bool ShouldSwimSurfaceAnimationPlay()
	{
		return this.CurrentState == SeinSwimming.State.SwimmingOnSurface;
	}

	public bool ShouldJumpOutOfWaterAnimationIdleKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && (!this.m_sein.Controller.CanMove || this.m_sein.Input.NormalizedHorizontal == 0) && (!this.IsSwimming || !this.PlatformMovement.Falling);
	}

	public bool ShouldJumpOutOfWaterAnimationMovingKeepPlaying()
	{
		return this.PlatformMovement.IsInAir && (!this.m_sein.Controller.CanMove || this.m_sein.Input.NormalizedHorizontal != 0) && (!this.IsSwimming || !this.PlatformMovement.Falling);
	}

	public SoundProvider SwimmingUnderwaterAmbience;

	public MixerSnapshot UnderwaterMixerSnapshot;

	public SeinSwimming.State CurrentState;

	public SeinSwimming.SwimmingAnimations Animations;

	public float Breath = 3f;

	public GameObject BreathingUI;

	public float DiveUnderwaterSpeed = 3f;

	public float DurationBetweenDrowningDamage = 1f;

	public SoundProvider InWaterSoundProvider;

	public SoundProvider BashIntoWaterSoundProvider;

	public SoundProvider StompIntoWaterSoundProvider;

	public float JumpOutOfWaterSpeed = 20f;

	public SoundProvider OutOfWaterSoundProvider;

	public float SkipSurfaceSpeedIn = 20f;

	public float SkipSurfaceSpeedOut = 10f;

	public SoundSource SurfaceSwimmingSoundProvider;

	public SoundSource UnderwaterSwimmingSoundProvider;

	public SoundProvider EmergeHighBreathSoundProvider;

	public SoundProvider EmergeMedBreathSoundProvider;

	public SoundProvider EmergeLowBreathSoundProvider;

	public SoundProvider BoostSwimsoundProvider;

	public float SwimGravity = 13f;

	public float SwimSpeed = 6f;

	public AnimationCurve SwimSpeedBoostCurve;

	public float BoostPeakTime = 0.2f;

	private float m_boostTime;

	public float BoostDuration;

	private bool m_isBoosting;

	public float SwimAngle;

	public float SwimAngleDeltaLimit = 100f;

	private float m_swimMovingTime;

	private float m_swimIdleTime;

	private float m_swimAccelerationTime;

	public float SwimUpwardsGravity = 13f;

	public HorizontalPlatformMovementSettings.SpeedMultiplierSet SwimmingOnSurfaceHorizontalSpeed;

	public GameObject WaterSplashPrefab;

	private WaterZone m_currentWater;

	private float m_drowningDelay;

	private SeinCharacter m_sein;

	private LegacyAnimator[] m_breathingUIAnimators;

	public float DrownDamage = 5f;

	private Ambience.Layer m_ambienceLayer;

	public bool CanHorizontalSwimJump;

	public float Deceleration = 10f;

	public float MaxFallSpeed = 4f;

	public float BashTime = 1f;

	public float SmoothAngleDelta;

	public AnimationCurve AccelerationOverTime;

	private float m_boostAnimationRemainingTime;

	public bool CanJumpUnderwater;

	public bool HoldAToSwimLoop;

	public float SwimJumpSpeedDelta = 100f;

	[Serializable]
	public class MovingAndIdleAnimationPair
	{
		public TextureAnimationWithTransitions Idle;

		public TextureAnimationWithTransitions Moving;
	}

	public enum State
	{
		OutOfWater,
		SwimmingOnSurface,
		SwimMovingUnderwater,
		SwimIdleUnderwater
	}

	[Serializable]
	public class SwimmingAnimations
	{
		public SeinSwimming.MovingAndIdleAnimationPair JumpOutOfWater;

		public SeinSwimming.MovingAndIdleAnimationPair SwimSurface;

		public TextureAnimationWithTransitions[] SwimHorizontal;

		public TextureAnimationWithTransitions[] SwimJumpLeft;

		public AnimationCurve AnimationFromBend;

		public TextureAnimationWithTransitions SwimIdle;

		public TextureAnimationWithTransitions SwimMiddleToIdleClockwise;

		public TextureAnimationWithTransitions SwimMiddleToIdleAntiClockwise;

		public TextureAnimationWithTransitions SwimIdleToSwimMiddle;

		public TextureAnimationWithTransitions SwimFlipHorizontalAnimation;

		public TextureAnimationWithTransitions SwimFlipVerticalAnimation;

		public TextureAnimationWithTransitions SwimFlipHorizontalVerticalAnimation;
	}
}
