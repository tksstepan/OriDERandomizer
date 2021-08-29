using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x02000329 RID: 809
public class SeinGrenadeAttack : CharacterState, ISeinReceiver
{
	// Token: 0x06001138 RID: 4408
	public SeinGrenadeAttack()
	{
	}

	// Token: 0x170002DF RID: 735
	// (get) Token: 0x06001139 RID: 4409
	private bool IsGrabbingWall
	{
		get
		{
			return this.m_sein.Controller.IsGrabbingWall;
		}
	}

	// Token: 0x170002E0 RID: 736
	// (get) Token: 0x0600113A RID: 4410
	private bool IsInAir
	{
		get
		{
			return !this.m_isAiming;
		}
	}

	// Token: 0x0600113B RID: 4411
	private void ResetAimToDefault()
	{
		this.SetAimVelocity(new Vector2(14f, 16f));
	}

	// Token: 0x0600113C RID: 4412
	private int PickAnimationIndex(int length)
	{
		return Mathf.Clamp(Mathf.FloorToInt(((!this.IsGrabbingWall) ? Mathf.InverseLerp(this.MinAimGroundAnimationAngle, this.MaxAimGroundAnimationAngle, this.m_animationAimAngle) : Mathf.InverseLerp(this.MinAimWallAnimationAngle, this.MaxAimWallAnimationAngle, this.m_animationAimAngle)) * (float)length), 0, length - 1);
	}

	// Token: 0x0600113D RID: 4413
	private float IndexToAnimationAngle(int index, int length)
	{
		float t = (float)index / (float)length;
		if (this.IsGrabbingWall)
		{
			return Mathf.Lerp(this.MinAimWallAnimationAngle, this.MaxAimWallAnimationAngle, t);
		}
		return Mathf.Lerp(this.MinAimGroundAnimationAngle, this.MaxAimGroundAnimationAngle, t);
	}

	// Token: 0x0600113E RID: 4414
	private TextureAnimationWithTransitions PickAnimation(TextureAnimationWithTransitions[] animations)
	{
		int num = this.PickAnimationIndex(animations.Length);
		return animations[num];
	}

	// Token: 0x170002E1 RID: 737
	// (get) Token: 0x0600113F RID: 4415
	private float EnergyCostFinal
	{
		get
		{
			return 0f;
		}
	}

	// Token: 0x06001140 RID: 4416
	private bool HasGrenadeEfficiencySkill()
	{
		return this.m_sein.PlayerAbilities.GrenadeEfficiency.HasAbility;
	}

	// Token: 0x170002E2 RID: 738
	// (get) Token: 0x06001141 RID: 4417
	private bool HasEnoughEnergy
	{
		get
		{
			return this.m_sein.Energy.CanAfford(this.EnergyCostFinal);
		}
	}

	// Token: 0x06001142 RID: 4418
	private void SpendEnergy()
	{
		this.m_sein.Energy.Spend(this.EnergyCostFinal);
	}

	// Token: 0x06001143 RID: 4419
	private void RestoreEnergy()
	{
		this.m_sein.Energy.Gain(this.EnergyCostFinal);
	}

	// Token: 0x06001144 RID: 4420
	public void Start()
	{
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += this.ModifyHorizontalPlatformMovementSettings;
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x06001145 RID: 4421
	public override void OnDestroy()
	{
		base.OnDestroy();
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= this.ModifyHorizontalPlatformMovementSettings;
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

	// Token: 0x06001146 RID: 4422
	public void OnRestoreCheckpoint()
	{
		this.CancelAiming();
	}

	// Token: 0x170002E3 RID: 739
	// (get) Token: 0x06001147 RID: 4423
	public CharacterLeftRightMovement CharacterLeftRightMovement
	{
		get
		{
			return this.m_sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	// Token: 0x170002E4 RID: 740
	// (get) Token: 0x06001148 RID: 4424
	public CharacterGravity CharacterGravity
	{
		get
		{
			return this.m_sein.PlatformBehaviour.Gravity;
		}
	}

	// Token: 0x06001149 RID: 4425
	private void ModifyHorizontalPlatformMovementSettings(HorizontalPlatformMovementSettings settings)
	{
		if (this.m_isAiming)
		{
			settings.Ground.Acceleration = 0f;
			settings.Ground.MaxSpeed = 0f;
		}
	}

	// Token: 0x0600114A RID: 4426
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		sein.Abilities.Grenade = this;
	}

	// Token: 0x0600114B RID: 4427
	public override void UpdateCharacterState()
	{
		if (this.m_sein.IsSuspended)
		{
			return;
		}
		if (this.m_sein.Controller.InputLocked)
		{
			return;
		}
		if (SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities))
		{
			return;
		}
		base.UpdateCharacterState();
		if (this.m_isAiming)
		{
			this.UpdateAiming();
			return;
		}
		this.UpdateNormal();
	}

	// Token: 0x0600114C RID: 4428
	private bool HasGrenadeUpgrade()
	{
		return this.m_sein.PlayerAbilities.GrenadeUpgrade.HasAbility;
	}

	// Token: 0x170002E5 RID: 741
	// (get) Token: 0x0600114D RID: 4429
	private Vector3 GrenadeSpawnPosition
	{
		get
		{
			return this.m_sein.Position;
		}
	}

	// Token: 0x0600114E RID: 4430
	private SpiritGrenade SpawnGrenade(Vector2 velocity)
	{
		this.RefreshListOfQuickSpiritGrenades();
		if (this.m_spiritGrenades.Count >= this.MaxSpamGrenades)
		{
			this.m_spiritGrenades[0].Explode();
			this.m_spiritGrenades.RemoveAt(0);
		}
		SpiritGrenade component = ((GameObject)InstantiateUtility.Instantiate((!this.HasGrenadeUpgrade()) ? this.Grenade : this.GrenadeUpgraded, this.GrenadeSpawnPosition, Quaternion.identity)).GetComponent<SpiritGrenade>();
		component.SetTrajectory(velocity);
		this.m_spiritGrenades.Add(component);
		if (this.m_autoTarget as Component != null)
		{
			component.Duration = this.TimeToTarget(velocity, this.m_autoTarget) + 0.2f;
			this.m_autoTarget = null;
		}
		return component;
	}

	// Token: 0x0600114F RID: 4431
	private void RefreshListOfQuickSpiritGrenades()
	{
		this.m_spiritGrenades.RemoveAll((SpiritGrenade a) => a == null);
	}

	// Token: 0x170002E6 RID: 742
	// (get) Token: 0x06001150 RID: 4432
	public bool IsAiming
	{
		get
		{
			return this.m_isAiming;
		}
	}

	// Token: 0x170002E7 RID: 743
	// (get) Token: 0x06001151 RID: 4433
	public bool CanAim
	{
		get
		{
			return !this.m_sein.PlatformBehaviour.PlatformMovement.MovingHorizontally && (this.m_sein.IsOnGround || this.IsGrabbingWall);
		}
	}

	// Token: 0x06001152 RID: 4434
	public void PlayAimAnimation()
	{
		this.m_sein.Animation.PlayLoop(this.PickAnimation((!this.IsGrabbingWall) ? this.AimingAnimations : this.WallAimingAnimations), 154, new Func<bool>(this.KeepPlayingAimAnimation), true);
	}

	// Token: 0x06001153 RID: 4435
	public void PlayThrowAnimation()
	{
		if (Mathf.Approximately(Mathf.Abs(this.m_rawAimOffset.x), this.QuickThrowSpeed.x) && Mathf.Approximately(this.m_rawAimOffset.y, this.QuickThrowSpeed.y))
		{
			this.m_sein.Animation.Play((!this.IsGrabbingWall) ? this.QuickThrow.IdleThrowAnimation : this.QuickThrow.WallThrowAnimation, 154, new Func<bool>(this.KeepPlayingThrowAnimation));
			return;
		}
		this.m_sein.Animation.Play(this.PickAnimation((!this.IsGrabbingWall) ? this.ThrowAnimations : this.WallThrowAnimations), 154, new Func<bool>(this.KeepPlayingThrowAnimation));
	}

	// Token: 0x06001154 RID: 4436
	public void PlayThrowSound()
	{
		Sound.Play(this.ThrowGrenadeSound.GetSound(null), base.transform.position, null);
	}

	// Token: 0x170002E8 RID: 744
	// (get) Token: 0x06001155 RID: 4437
	public float GrenadeGravity
	{
		get
		{
			return this.Trajectory.Gravity;
		}
	}

	// Token: 0x06001156 RID: 4438
	public void UpdateAiming()
	{
		if (Core.Input.LeftShoulder.Released)
		{
			this.m_lockPressingInputTime = 0.64f;
			this.SpawnGrenade(this.m_rawAimOffset);
			this.PlayThrowAnimation();
			this.EndAiming();
			this.PlayThrowSound();
			return;
		}
		if (Core.Input.Jump.OnPressed || Core.Input.Cancel.OnPressed || !this.CanAim)
		{
			this.CancelAiming();
			return;
		}
		this.m_sein.Speed = Vector2.zero;
		if (RandomizerRebinding.ResetGrenadeAim.OnPressed)
		{
			this.ResetAimToDefault();
		}
		Vector2 axis = Core.Input.Axis;
		if (!RandomizerSettings.FastGrenadeAim)
		{
			Vector2 b = this.AimSpeed.Evaluate(axis.magnitude) * axis.normalized * RandomizerSettings.GrenadeAimSpeed;
			if (b.magnitude > 0f)
			{
				this.m_autoAim = false;
			}
			this.m_rawAimOffset += b;
		}
		else
		{
			float greater = Math.Max(Math.Abs(axis.x), Math.Abs(axis.y));
			if (greater > 0f)
			{
				this.m_rawAimOffset = axis * axis.sqrMagnitude * Math.Min(Math.Abs(UI.Cameras.Current.OffsetController.Offset.z), this.MaxAimDistance) / greater + Vector2.up * this.CursorSpeedYOffset;
			}
			else
			{
				this.m_rawAimOffset = Vector2.up * this.CursorSpeedYOffset;
			}
			this.m_autoAim = false;
		}
		if (this.m_autoAim)
		{
			this.AutoTarget();
		}
		else
		{
			this.m_autoTarget = null;
		}
		this.ClampAim();
		if (Core.Input.CursorMoved)
		{
			Vector2 v = UI.Cameras.Current.Camera.WorldToScreenPoint(base.transform.position);
			Vector2 b2 = UI.Cameras.System.GUICamera.ScreenToWorldPoint(v);
			this.m_rawAimOffset = (Core.Input.CursorPositionUI - b2) * this.CursorSpeedMultiplier + Vector2.up * this.CursorSpeedYOffset;
			this.m_autoAim = false;
			this.ClampAim();
		}
		this.m_aimOffset = Vector2.Lerp(this.m_rawAimOffset, this.m_aimOffset, 0.5f);
		if (!this.m_sein.Controller.IsGrabbingWall)
		{
			if (this.m_lockAimAnimationRemainingTime <= 0f)
			{
				bool faceLeft = this.m_faceLeft;
				this.m_faceLeft = (this.m_aimOffset.x < 0f);
				if (faceLeft != this.m_faceLeft)
				{
					this.m_lockAimAnimationRemainingTime = 0.17f;
					this.m_animationAimAngle = 90f;
					Sound.Play(this.TurnAroundAimingSound.GetSound(null), base.transform.position, null);
				}
			}
			this.m_sein.FaceLeft = this.m_faceLeft;
		}
		this.UpdateTrajectory();
		if (this.m_lockAimAnimationRemainingTime > 0f)
		{
			this.m_lockAimAnimationRemainingTime -= Time.deltaTime;
		}
		if (this.m_lockAimAnimationRemainingTime <= 0f)
		{
			Vector3 v2 = this.m_aimOffset.normalized;
			if (this.m_aimOffset.y > 0f)
			{
				float num = this.m_aimOffset.y / this.GrenadeGravity;
				float d = this.m_aimOffset.y * num + 0.5f * this.GrenadeGravity * num * num;
				v2 = (this.m_aimOffset.x * num * Vector3.right + d * Vector3.up).normalized;
			}
			v2.x = Mathf.Abs(v2.x);
			float target = MoonMath.Angle.AngleFromVector(v2);
			this.m_animationAimAngle = Mathf.MoveTowardsAngle(this.m_animationAimAngle, target, 90f * Time.deltaTime * 2f);
			this.PlayAimAnimation();
		}
		if (this.m_grenadeAiming)
		{
			SpriteAnimatorWithTransitions animator = this.m_sein.Animation.Animator;
			TextureAnimation currentAnimation = animator.CurrentAnimation;
			if (currentAnimation.AnimationMetaData)
			{
				this.PositionGrenadeAiming(currentAnimation.AnimationMetaData, (int)animator.TextureAnimator.Frame);
				return;
			}
			if (this.IsGrabbingWall)
			{
				this.PositionGrenadeAiming(this.WallAimingMetaData, this.PickAnimationIndex(this.WallAimingAnimations.Length));
				return;
			}
			this.PositionGrenadeAiming(this.AimingMetaData, this.PickAnimationIndex(this.AimingAnimations.Length));
		}
	}

	// Token: 0x06001157 RID: 4439
	private void PositionGrenadeAiming(AnimationMetaData metaData, int frame)
	{
		AnimationMetaData.AnimationData animationData = metaData.FindData("#grenade");
		if (animationData != null)
		{
			Vector3 positionAtFrame = animationData.GetPositionAtFrame(frame);
			this.m_grenadeAiming.transform.position = this.m_sein.PlatformBehaviour.Visuals.Sprite.transform.TransformPoint(positionAtFrame);
		}
	}

	// Token: 0x06001158 RID: 4440
	public void EndAiming()
	{
		this.m_lockAimAnimationRemainingTime = 0f;
		this.m_isAiming = false;
		if (this.m_sein.Abilities.GrabWall)
		{
			this.m_sein.Abilities.GrabWall.LockVerticalMovement = false;
		}
		if (this.m_grenadeAiming)
		{
			this.m_grenadeAiming.GetComponent<TransparencyAnimator>().AnimatorDriver.ContinueBackwards();
		}
		this.Trajectory.HideTrajectory();
		if (this.AimingSound)
		{
			this.AimingSound.Stop();
		}
	}

	// Token: 0x06001159 RID: 4441
	private void ClampAim()
	{
		this.m_rawAimOffset.x = Mathf.Clamp(this.m_rawAimOffset.x, -this.MaxAimDistance, this.MaxAimDistance);
		if (this.IsGrabbingWall)
		{
			this.m_rawAimOffset.x = ((!this.m_faceLeft) ? Mathf.Min(0f, this.m_rawAimOffset.x) : Mathf.Max(0f, this.m_rawAimOffset.x));
		}
		float num = (this.m_rawAimOffset.y <= 0f) ? this.MinAimDistanceDown : this.MinAimDistanceUp;
		float num2 = this.MinAimDistanceHorizontal / num;
		this.m_rawAimOffset.y = this.m_rawAimOffset.y * num2;
		if (this.m_rawAimOffset.magnitude < this.MinAimDistanceHorizontal)
		{
			this.m_rawAimOffset = this.m_rawAimOffset.normalized * this.MinAimDistanceHorizontal;
		}
		this.m_rawAimOffset.y = this.m_rawAimOffset.y / num2;
		this.m_rawAimOffset.y = Mathf.Clamp(this.m_rawAimOffset.y, (!this.IsGrabbingWall) ? this.MinAimVertical : this.MinAimVerticalWall, this.MaxAimVertical);
	}

	// Token: 0x0600115A RID: 4442
	public void UpdateTrajectory()
	{
		this.Trajectory.StartPosition = this.GrenadeSpawnPosition;
		this.Trajectory.InitialVelocity = this.m_aimOffset;
	}

	// Token: 0x0600115B RID: 4443
	public float TimeToTarget(Vector2 velocity, IAttackable target)
	{
		return Mathf.Abs(target.Position.x - this.GrenadeSpawnPosition.x) / Mathf.Abs(velocity.x);
	}

	// Token: 0x0600115C RID: 4444
	public bool WillRayHitEnemy(Vector2 initialVelocity, IAttackable target)
	{
		Vector3 vector = this.GrenadeSpawnPosition;
		Vector3 a = initialVelocity;
		Vector3 vector2 = vector;
		float grenadeGravity = this.GrenadeGravity;
		float num = 0f;
		float num2 = this.TimeToTarget(initialVelocity, target);
		while (num < num2)
		{
			for (int i = 0; i < 2; i++)
			{
				vector += a * 0.01666667f;
				a += Vector3.down * grenadeGravity * 0.01666667f;
				num += 0.01666667f;
			}
			Vector3 vector3 = vector - vector2;
			RaycastHit raycastHit;
			if (Physics.SphereCast(vector2, 0.5f, vector3.normalized, out raycastHit, vector3.magnitude))
			{
				break;
			}
			vector2 = vector;
		}
		return Vector3.Distance(vector2, target.Position) <= 4f;
	}

	// Token: 0x0600115D RID: 4445
	public bool CompareAnimations(TextureAnimationWithTransitions current, TextureAnimationWithTransitions[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == current)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600115E RID: 4446
	public Func<bool> AnimationRule(SeinGrenadeAttack.FastThrowAnimationRule.AnimationRule rule)
	{
		if (rule == SeinGrenadeAttack.FastThrowAnimationRule.AnimationRule.InAir)
		{
			return new Func<bool>(this.KeepPlayingAirThrowAnimation);
		}
		if (rule != SeinGrenadeAttack.FastThrowAnimationRule.AnimationRule.OnGround)
		{
			return null;
		}
		return new Func<bool>(this.KeepPlayingGroundThrowAnimation);
	}

	// Token: 0x0600115F RID: 4447
	public void PlayFastThrowAnimation()
	{
		TextureAnimation currentAnimation = this.m_sein.PlatformBehaviour.Visuals.Animation.Animator.CurrentAnimation;
		TextureAnimationWithTransitions currentTextureAnimationTransitions = this.m_sein.PlatformBehaviour.Visuals.Animation.Animator.CurrentTextureAnimationTransitions;
		foreach (SeinGrenadeAttack.FastThrowAnimationRule fastThrowAnimationRule in this.FastThrowAnimations)
		{
			if (fastThrowAnimationRule.Animations.Contains(currentAnimation))
			{
				this.m_sein.Animation.Play(fastThrowAnimationRule.ThrowAnimation, 10, this.AnimationRule(fastThrowAnimationRule.PlayRule));
				return;
			}
		}
		foreach (SeinGrenadeAttack.FastThrowAnimationRule fastThrowAnimationRule2 in this.FastThrowAnimations)
		{
			if (fastThrowAnimationRule2.AnimationsWithTransitions.Contains(currentTextureAnimationTransitions))
			{
				this.m_sein.Animation.Play(fastThrowAnimationRule2.ThrowAnimation, 10, this.AnimationRule(fastThrowAnimationRule2.PlayRule));
				break;
			}
		}
	}

	// Token: 0x06001160 RID: 4448
	public bool KeepPlayingAirThrowAnimation()
	{
		return this.m_sein.PlatformBehaviour.PlatformMovement.IsInAir;
	}

	// Token: 0x06001161 RID: 4449
	public bool KeepPlayingGroundThrowAnimation()
	{
		return this.m_sein.PlatformBehaviour.PlatformMovement.IsOnGround;
	}

	// Token: 0x06001162 RID: 4450
	public void UpdateNormal()
	{
		if (RandomizerRebinding.ResetGrenadeAim.OnPressed)
		{
			this.ResetAimToDefault();
		}
		this.m_lockPressingInputTime -= Time.deltaTime;
		this.m_autoTarget = null;
		if (Core.Input.LeftShoulder.OnPressed && this.m_lockPressingInputTime <= 0f)
		{
			this.m_inputPressed = true;
		}
		if (Core.Input.LeftShoulder.Released)
		{
			this.m_inputPressed = false;
		}
		this.RefreshListOfQuickSpiritGrenades();
		if (Core.Input.LeftShoulder.Pressed && this.m_lockPressingInputTime <= 0f && this.HasEnoughEnergy && this.CanAim)
		{
			this.m_inputPressed = false;
			this.SpendEnergy();
			this.BeginAiming();
			this.UpdateTrajectory();
			this.Trajectory.ShowTrajectory();
		}
		if (this.m_inputPressed)
		{
			if (!this.HasEnoughEnergy)
			{
				this.m_inputPressed = false;
				UI.SeinUI.ShakeEnergyOrbBar();
				if (this.NotEnoughEnergySound)
				{
					Sound.Play(this.NotEnoughEnergySound.GetSound(null), base.transform.position, null);
				}
				this.m_sein.Animation.Play(this.PickAnimation((!this.IsGrabbingWall) ? this.NotEnoughEnergyThrowAnimations : this.NotEnoughEnergyWallThrowAnimations), 154, new Func<bool>(this.KeepPlayingNotEnoughEnergyAnimation));
				if (this.CanAim)
				{
					Vector3 b = (!this.IsGrabbingWall) ? new Vector2(-0.5f, 0.1f) : new Vector2(-0.8f, -0.13f);
					if (this.m_sein.FaceLeft)
					{
						b.x *= -1f;
					}
					InstantiateUtility.Instantiate(this.GrenadeFailEffect, this.m_sein.Position + b, Quaternion.identity);
				}
				this.m_lockPressingInputTime = 0.2f;
				return;
			}
			if (!this.CanAim)
			{
				this.m_autoTarget = this.FindAutoAttackable;
				if (this.m_autoTarget != null)
				{
					this.m_inputPressed = false;
					this.m_lockPressingInputTime = 0.2f;
					this.SpawnGrenade(this.VelocityToAimAtTarget(this.m_autoTarget)).Bashable = false;
					this.SpendEnergy();
					this.PlayFastThrowAnimation();
					this.PlayThrowSound();
					this.ResetAimToDefault();
				}
				else
				{
					this.m_inputPressed = false;
					this.m_lockPressingInputTime = 0.2f;
					Vector2 quickThrowSpeed = this.QuickThrowSpeed;
					if (this.m_sein.FaceLeft)
					{
						quickThrowSpeed.x *= -1f;
					}
					this.SpawnGrenade(quickThrowSpeed).Bashable = false;
					this.SpendEnergy();
					this.PlayFastThrowAnimation();
					this.PlayThrowSound();
					this.ResetAimToDefault();
				}
				if (this.m_sein.Abilities.Glide)
				{
					this.m_sein.Abilities.Glide.LockGliding(0.2f);
					this.m_sein.Abilities.Glide.IsGliding = false;
				}
			}
		}
	}

	// Token: 0x06001163 RID: 4451
	public bool KeepPlayingAimAnimation()
	{
		return this.m_isAiming;
	}

	// Token: 0x06001164 RID: 4452
	public bool KeepPlayingThrowAnimation()
	{
		return !this.m_sein.PlatformBehaviour.PlatformMovement.MovingHorizontally;
	}

	// Token: 0x06001165 RID: 4453
	public bool KeepPlayingNotEnoughEnergyAnimation()
	{
		return this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeed == Vector2.zero;
	}

	// Token: 0x06001166 RID: 4454
	public void BeginAiming()
	{
		this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeed = Vector2.zero;
		if (this.IsGrabbingWall)
		{
			if (!this.m_lastAimWasOnWall)
			{
				this.ResetAimToDefault();
			}
			this.m_lastAimWasOnWall = true;
			this.m_animationAimAngle = this.IndexToAnimationAngle(8, this.WallAimingAnimations.Length);
			this.m_lockAimAnimationRemainingTime = 0.3667f;
		}
		else
		{
			if (this.m_lastAimWasOnWall)
			{
				this.ResetAimToDefault();
			}
			this.m_lastAimWasOnWall = false;
			this.m_animationAimAngle = this.IndexToAnimationAngle(8, this.AimingAnimations.Length);
			this.m_lockAimAnimationRemainingTime = 0.1f;
		}
		this.m_isAiming = true;
		this.m_faceLeft = this.m_sein.FaceLeft;
		this.m_rawAimOffset.x = Mathf.Abs(this.m_rawAimOffset.x) * (float)((!this.m_sein.FaceLeft) ? 1 : -1);
		if (this.IsGrabbingWall)
		{
			this.m_rawAimOffset.x = this.m_rawAimOffset.x * -1f;
		}
		this.ClampAim();
		this.m_aimOffset = this.m_rawAimOffset;
		this.m_autoAim = true;
		this.AutoTarget();
		if (this.m_sein.Abilities.GrabWall)
		{
			this.m_sein.Abilities.GrabWall.LockVerticalMovement = true;
		}
		this.m_grenadeAiming = (GameObject)InstantiateUtility.Instantiate(this.GrenadeAiming);
		Sound.Play(this.StartAimingSound.GetSound(null), base.transform.position, null);
		if (this.AimingSound)
		{
			this.AimingSound.Play();
		}
		this.PlayAimAnimation();
	}

	// Token: 0x170002E9 RID: 745
	// (get) Token: 0x06001167 RID: 4455
	public IAttackable FindAutoAttackable
	{
		get
		{
			IAttackable result = null;
			int num = 0;
			float num2 = float.MaxValue;
			foreach (IAttackable attackable in Targets.Attackables)
			{
				if (attackable as Component && attackable.CanBeGrenaded() && attackable is EntityTargetting && UI.Cameras.Current.IsOnScreen(attackable.Position))
				{
					Vector2 vector = attackable.Position - this.m_sein.Position;
					float magnitude = vector.magnitude;
					int num3 = (!this.m_sein.FaceLeft) ? 1 : -1;
					if (this.IsGrabbingWall)
					{
						num3 *= -1;
					}
					int num4 = (!(((EntityTargetting)attackable).Entity is Enemy)) ? 0 : 1;
					if (magnitude > this.AutoAim.MinDistance && magnitude < this.AutoAim.MaxDistance && num3 == (int)Mathf.Sign(vector.x) && (num < num4 || (num == num4 && magnitude < num2)))
					{
						Vector2 initialVelocity = this.VelocityToAimAtTarget(attackable);
						if (this.WillRayHitEnemy(initialVelocity, attackable))
						{
							result = attackable;
							num2 = magnitude;
							num = num4;
						}
					}
				}
			}
			return result;
		}
	}

	// Token: 0x06001168 RID: 4456
	public void AutoTarget()
	{
		this.m_autoTarget = this.FindAutoAttackable;
		if (this.m_autoTarget as Component != null)
		{
			this.SetAimVelocity(this.VelocityToAimAtTarget(this.m_autoTarget));
		}
	}

	// Token: 0x06001169 RID: 4457
	private void SetAimVelocity(Vector2 aim)
	{
		this.m_aimOffset = aim;
		this.m_rawAimOffset = aim;
	}

	// Token: 0x0600116A RID: 4458
	public Vector2 VelocityToAimAtTarget(IAttackable attackable)
	{
		Vector2 vector = attackable.Position - this.m_sein.Position;
		float num = (!this.IsInAir) ? (this.AutoAim.Speed + Mathf.Abs(vector.x) * this.AutoAim.SpeedPerXDistance + Mathf.Max(0f, vector.y) * this.AutoAim.SpeedPerYDistance) : this.AutoAim.InAirSpeed;
		float num2 = vector.magnitude / num;
		return new Vector2(vector.x / num2, vector.y / num2 + this.GrenadeGravity * num2 * 0.5f);
	}

	// Token: 0x0600116B RID: 4459
	public override void OnExit()
	{
		base.OnExit();
		this.CancelAiming();
	}

	// Token: 0x0600116C RID: 4460
	public void CancelAiming()
	{
		if (this.m_isAiming)
		{
			this.RestoreEnergy();
			this.EndAiming();
			Sound.Play(this.StopAimingSound.GetSound(null), base.transform.position, null);
		}
	}

	// Token: 0x0400103B RID: 4155
	public GameObject Grenade;

	// Token: 0x0400103C RID: 4156
	public GameObject GrenadeUpgraded;

	// Token: 0x0400103D RID: 4157
	public GameObject GrenadeAiming;

	// Token: 0x0400103E RID: 4158
	private GameObject m_grenadeAiming;

	// Token: 0x0400103F RID: 4159
	public SeinGrenadeTrajectory Trajectory;

	// Token: 0x04001040 RID: 4160
	public AnimationCurve AimSpeed;

	// Token: 0x04001041 RID: 4161
	public float MaxAimDistance;

	// Token: 0x04001042 RID: 4162
	public float MinAimDistanceUp;

	// Token: 0x04001043 RID: 4163
	public float MinAimDistanceDown;

	// Token: 0x04001044 RID: 4164
	public float MinAimDistanceHorizontal;

	// Token: 0x04001045 RID: 4165
	public float MaxAimVertical = 50f;

	// Token: 0x04001046 RID: 4166
	public float MinAimVertical = 2f;

	// Token: 0x04001047 RID: 4167
	public float MinAimVerticalWall = -30f;

	// Token: 0x04001048 RID: 4168
	public int MaxSpamGrenades = 3;

	// Token: 0x04001049 RID: 4169
	public float EnergyCost = 1f;

	// Token: 0x0400104A RID: 4170
	public SoundProvider NotEnoughEnergySound;

	// Token: 0x0400104B RID: 4171
	public SoundProvider TurnAroundAimingSound;

	// Token: 0x0400104C RID: 4172
	public SoundProvider ThrowGrenadeSound;

	// Token: 0x0400104D RID: 4173
	public SoundProvider StopAimingSound;

	// Token: 0x0400104E RID: 4174
	public SoundProvider StartAimingSound;

	// Token: 0x0400104F RID: 4175
	public SoundSource AimingSound;

	// Token: 0x04001050 RID: 4176
	public Vector2 QuickThrowSpeed = new Vector2(14f, 16f);

	// Token: 0x04001051 RID: 4177
	public GameObject GrenadeFailEffect;

	// Token: 0x04001052 RID: 4178
	public float AimAnimationAngleOffset = 5f;

	// Token: 0x04001053 RID: 4179
	public float CursorSpeedMultiplier = 1f;

	// Token: 0x04001054 RID: 4180
	public float CursorSpeedYOffset = 12f;

	// Token: 0x04001055 RID: 4181
	private float m_lockPressingInputTime;

	// Token: 0x04001056 RID: 4182
	private Vector2 m_rawAimOffset = new Vector2(14f, 16f);

	// Token: 0x04001057 RID: 4183
	private SeinCharacter m_sein;

	// Token: 0x04001058 RID: 4184
	private bool m_isAiming;

	// Token: 0x04001059 RID: 4185
	private Vector2 m_aimOffset;

	// Token: 0x0400105A RID: 4186
	private List<SpiritGrenade> m_spiritGrenades = new List<SpiritGrenade>();

	// Token: 0x0400105B RID: 4187
	private float m_animationAimAngle;

	// Token: 0x0400105C RID: 4188
	private bool m_lastAimWasOnWall;

	// Token: 0x0400105D RID: 4189
	public TextureAnimationWithTransitions[] AimingAnimations;

	// Token: 0x0400105E RID: 4190
	public TextureAnimationWithTransitions[] ThrowAnimations;

	// Token: 0x0400105F RID: 4191
	public TextureAnimationWithTransitions[] WallAimingAnimations;

	// Token: 0x04001060 RID: 4192
	public TextureAnimationWithTransitions[] WallThrowAnimations;

	// Token: 0x04001061 RID: 4193
	public TextureAnimationWithTransitions[] NotEnoughEnergyThrowAnimations;

	// Token: 0x04001062 RID: 4194
	public TextureAnimationWithTransitions[] NotEnoughEnergyWallThrowAnimations;

	// Token: 0x04001063 RID: 4195
	public SeinGrenadeAttack.QuickThrowAnimations QuickThrow;

	// Token: 0x04001064 RID: 4196
	public AnimationMetaData WallAimingMetaData;

	// Token: 0x04001065 RID: 4197
	public AnimationMetaData AimingMetaData;

	// Token: 0x04001066 RID: 4198
	private float m_lockAimAnimationRemainingTime;

	// Token: 0x04001067 RID: 4199
	private bool m_faceLeft;

	// Token: 0x04001068 RID: 4200
	public float MaxAimWallAnimationAngle = 85f;

	// Token: 0x04001069 RID: 4201
	public float MinAimWallAnimationAngle = -80f;

	// Token: 0x0400106A RID: 4202
	public float MaxAimGroundAnimationAngle = 90f;

	// Token: 0x0400106B RID: 4203
	public float MinAimGroundAnimationAngle = -30f;

	// Token: 0x0400106C RID: 4204
	private bool m_inputPressed;

	// Token: 0x0400106D RID: 4205
	public List<SeinGrenadeAttack.FastThrowAnimationRule> FastThrowAnimations;

	// Token: 0x0400106E RID: 4206
	private bool m_autoAim;

	// Token: 0x0400106F RID: 4207
	private IAttackable m_autoTarget;

	// Token: 0x04001070 RID: 4208
	public SeinGrenadeAttack.AutoAimSettings AutoAim;

	// Token: 0x0200032A RID: 810
	[Serializable]
	public class QuickThrowAnimations
	{
		// Token: 0x0600116E RID: 4462
		public QuickThrowAnimations()
		{
		}

		// Token: 0x04001072 RID: 4210
		public TextureAnimationWithTransitions FallIdleThrowAnimation;

		// Token: 0x04001073 RID: 4211
		public TextureAnimationWithTransitions FallThrowAnimation;

		// Token: 0x04001074 RID: 4212
		public TextureAnimationWithTransitions RunThrowAnimation;

		// Token: 0x04001075 RID: 4213
		public TextureAnimationWithTransitions JogThrowAnimation;

		// Token: 0x04001076 RID: 4214
		public TextureAnimationWithTransitions WalkThrowAnimation;

		// Token: 0x04001077 RID: 4215
		public TextureAnimationWithTransitions JumpThrowAnimation;

		// Token: 0x04001078 RID: 4216
		public TextureAnimationWithTransitions JumpIdleThrowAnimation;

		// Token: 0x04001079 RID: 4217
		public TextureAnimationWithTransitions IdleThrowAnimation;

		// Token: 0x0400107A RID: 4218
		public TextureAnimationWithTransitions WallThrowAnimation;
	}

	// Token: 0x0200032B RID: 811
	[Serializable]
	public class FastThrowAnimationRule
	{
		// Token: 0x0600116F RID: 4463
		public FastThrowAnimationRule()
		{
		}

		// Token: 0x0400107B RID: 4219
		public TextureAnimationWithTransitions ThrowAnimation;

		// Token: 0x0400107C RID: 4220
		public List<TextureAnimationWithTransitions> AnimationsWithTransitions;

		// Token: 0x0400107D RID: 4221
		public List<TextureAnimation> Animations;

		// Token: 0x0400107E RID: 4222
		public SeinGrenadeAttack.FastThrowAnimationRule.AnimationRule PlayRule;

		// Token: 0x0200032C RID: 812
		public enum AnimationRule
		{
			// Token: 0x04001080 RID: 4224
			InAir,
			// Token: 0x04001081 RID: 4225
			OnGround
		}
	}

	// Token: 0x0200032D RID: 813
	[Serializable]
	public class AutoAimSettings
	{
		// Token: 0x06001170 RID: 4464
		public AutoAimSettings()
		{
		}

		// Token: 0x04001082 RID: 4226
		public float MaxDistance = 30f;

		// Token: 0x04001083 RID: 4227
		public float MinDistance = 2f;

		// Token: 0x04001084 RID: 4228
		public float Speed = 5f;

		// Token: 0x04001085 RID: 4229
		public float SpeedPerXDistance = 0.7f;

		// Token: 0x04001086 RID: 4230
		public float SpeedPerYDistance = 2f;

		// Token: 0x04001087 RID: 4231
		public float InAirSpeed = 30f;
	}
}
