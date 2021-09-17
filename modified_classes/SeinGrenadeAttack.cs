using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class SeinGrenadeAttack : CharacterState, ISeinReceiver
{
	private bool IsGrabbingWall
	{
		get
		{
			return this.m_sein.Controller.IsGrabbingWall;
		}
	}

	private bool IsInAir
	{
		get
		{
			return !this.m_isAiming;
		}
	}

	private void ResetAimToDefault()
	{
		this.SetAimVelocity(new Vector2(14f, 16f));
	}

	private int PickAnimationIndex(int length)
	{
		return Mathf.Clamp(Mathf.FloorToInt(((!this.IsGrabbingWall) ? Mathf.InverseLerp(this.MinAimGroundAnimationAngle, this.MaxAimGroundAnimationAngle, this.m_animationAimAngle) : Mathf.InverseLerp(this.MinAimWallAnimationAngle, this.MaxAimWallAnimationAngle, this.m_animationAimAngle)) * (float)length), 0, length - 1);
	}

	private float IndexToAnimationAngle(int index, int length)
	{
		float t = (float)index / (float)length;
		if (this.IsGrabbingWall)
		{
			return Mathf.Lerp(this.MinAimWallAnimationAngle, this.MaxAimWallAnimationAngle, t);
		}
		return Mathf.Lerp(this.MinAimGroundAnimationAngle, this.MaxAimGroundAnimationAngle, t);
	}

	private TextureAnimationWithTransitions PickAnimation(TextureAnimationWithTransitions[] animations)
	{
		int num = this.PickAnimationIndex(animations.Length);
		return animations[num];
	}

	private float EnergyCostFinal
	{
		get
		{
			return 0f;
		}
	}

	private bool HasGrenadeEfficiencySkill()
	{
		return this.m_sein.PlayerAbilities.GrenadeEfficiency.HasAbility;
	}

	private bool HasEnoughEnergy
	{
		get
		{
			return this.m_sein.Energy.CanAfford(this.EnergyCostFinal);
		}
	}

	private void SpendEnergy()
	{
		this.m_sein.Energy.Spend(this.EnergyCostFinal);
	}

	private void RestoreEnergy()
	{
		this.m_sein.Energy.Gain(this.EnergyCostFinal);
	}

	public void Start()
	{
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent += this.ModifyHorizontalPlatformMovementSettings;
		Game.Checkpoint.Events.OnPostRestore.Add(new Action(this.OnRestoreCheckpoint));
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		this.CharacterLeftRightMovement.ModifyHorizontalPlatformMovementSettingsEvent -= this.ModifyHorizontalPlatformMovementSettings;
		Game.Checkpoint.Events.OnPostRestore.Remove(new Action(this.OnRestoreCheckpoint));
	}

	public void OnRestoreCheckpoint()
	{
		this.CancelAiming();
	}

	public CharacterLeftRightMovement CharacterLeftRightMovement
	{
		get
		{
			return this.m_sein.PlatformBehaviour.LeftRightMovement;
		}
	}

	public CharacterGravity CharacterGravity
	{
		get
		{
			return this.m_sein.PlatformBehaviour.Gravity;
		}
	}

	private void ModifyHorizontalPlatformMovementSettings(HorizontalPlatformMovementSettings settings)
	{
		if (this.m_isAiming)
		{
			settings.Ground.Acceleration = 0f;
			settings.Ground.MaxSpeed = 0f;
		}
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		sein.Abilities.Grenade = this;
	}

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

	private bool HasGrenadeUpgrade()
	{
		return this.m_sein.PlayerAbilities.GrenadeUpgrade.HasAbility;
	}

	private Vector3 GrenadeSpawnPosition
	{
		get
		{
			return this.m_sein.Position;
		}
	}

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

	private void RefreshListOfQuickSpiritGrenades()
	{
		this.m_spiritGrenades.RemoveAll((SpiritGrenade a) => a == null);
	}

	public bool IsAiming
	{
		get
		{
			return this.m_isAiming;
		}
	}

	public bool CanAim
	{
		get
		{
			return !this.m_sein.PlatformBehaviour.PlatformMovement.MovingHorizontally && (this.m_sein.IsOnGround || this.IsGrabbingWall);
		}
	}

	public void PlayAimAnimation()
	{
		this.m_sein.Animation.PlayLoop(this.PickAnimation((!this.IsGrabbingWall) ? this.AimingAnimations : this.WallAimingAnimations), 154, new Func<bool>(this.KeepPlayingAimAnimation), true);
	}

	public void PlayThrowAnimation()
	{
		if (Mathf.Approximately(Mathf.Abs(this.m_rawAimOffset.x), this.QuickThrowSpeed.x) && Mathf.Approximately(this.m_rawAimOffset.y, this.QuickThrowSpeed.y))
		{
			this.m_sein.Animation.Play((!this.IsGrabbingWall) ? this.QuickThrow.IdleThrowAnimation : this.QuickThrow.WallThrowAnimation, 154, new Func<bool>(this.KeepPlayingThrowAnimation));
			return;
		}
		this.m_sein.Animation.Play(this.PickAnimation((!this.IsGrabbingWall) ? this.ThrowAnimations : this.WallThrowAnimations), 154, new Func<bool>(this.KeepPlayingThrowAnimation));
	}

	public void PlayThrowSound()
	{
		Sound.Play(this.ThrowGrenadeSound.GetSound(null), base.transform.position, null);
	}

	public float GrenadeGravity
	{
		get
		{
			return this.Trajectory.Gravity;
		}
	}

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
		if (!RandomizerSettings.Controls.FastGrenadeAim)
		{
			Vector2 b = this.AimSpeed.Evaluate(axis.magnitude) * axis.normalized * RandomizerSettings.Controls.GrenadeAimSpeed;
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

	private void PositionGrenadeAiming(AnimationMetaData metaData, int frame)
	{
		AnimationMetaData.AnimationData animationData = metaData.FindData("#grenade");
		if (animationData != null)
		{
			Vector3 positionAtFrame = animationData.GetPositionAtFrame(frame);
			this.m_grenadeAiming.transform.position = this.m_sein.PlatformBehaviour.Visuals.Sprite.transform.TransformPoint(positionAtFrame);
		}
	}

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

	public void UpdateTrajectory()
	{
		this.Trajectory.StartPosition = this.GrenadeSpawnPosition;
		this.Trajectory.InitialVelocity = this.m_aimOffset;
	}

	public float TimeToTarget(Vector2 velocity, IAttackable target)
	{
		return Mathf.Abs(target.Position.x - this.GrenadeSpawnPosition.x) / Mathf.Abs(velocity.x);
	}

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

	public bool KeepPlayingAirThrowAnimation()
	{
		return this.m_sein.PlatformBehaviour.PlatformMovement.IsInAir;
	}

	public bool KeepPlayingGroundThrowAnimation()
	{
		return this.m_sein.PlatformBehaviour.PlatformMovement.IsOnGround;
	}

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

	public bool KeepPlayingAimAnimation()
	{
		return this.m_isAiming;
	}

	public bool KeepPlayingThrowAnimation()
	{
		return !this.m_sein.PlatformBehaviour.PlatformMovement.MovingHorizontally;
	}

	public bool KeepPlayingNotEnoughEnergyAnimation()
	{
		return this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeed == Vector2.zero;
	}

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

	public void AutoTarget()
	{
		this.m_autoTarget = this.FindAutoAttackable;
		if (this.m_autoTarget as Component != null)
		{
			this.SetAimVelocity(this.VelocityToAimAtTarget(this.m_autoTarget));
		}
	}

	private void SetAimVelocity(Vector2 aim)
	{
		this.m_aimOffset = aim;
		this.m_rawAimOffset = aim;
	}

	public Vector2 VelocityToAimAtTarget(IAttackable attackable)
	{
		Vector2 vector = attackable.Position - this.m_sein.Position;
		float num = (!this.IsInAir) ? (this.AutoAim.Speed + Mathf.Abs(vector.x) * this.AutoAim.SpeedPerXDistance + Mathf.Max(0f, vector.y) * this.AutoAim.SpeedPerYDistance) : this.AutoAim.InAirSpeed;
		float num2 = vector.magnitude / num;
		return new Vector2(vector.x / num2, vector.y / num2 + this.GrenadeGravity * num2 * 0.5f);
	}

	public override void OnExit()
	{
		base.OnExit();
		this.CancelAiming();
	}

	public void CancelAiming()
	{
		if (this.m_isAiming)
		{
			this.RestoreEnergy();
			this.EndAiming();
			Sound.Play(this.StopAimingSound.GetSound(null), base.transform.position, null);
		}
	}

	public GameObject Grenade;

	public GameObject GrenadeUpgraded;

	public GameObject GrenadeAiming;

	private GameObject m_grenadeAiming;

	public SeinGrenadeTrajectory Trajectory;

	public AnimationCurve AimSpeed;

	public float MaxAimDistance;

	public float MinAimDistanceUp;

	public float MinAimDistanceDown;

	public float MinAimDistanceHorizontal;

	public float MaxAimVertical = 50f;

	public float MinAimVertical = 2f;

	public float MinAimVerticalWall = -30f;

	public int MaxSpamGrenades = 3;

	public float EnergyCost = 1f;

	public SoundProvider NotEnoughEnergySound;

	public SoundProvider TurnAroundAimingSound;

	public SoundProvider ThrowGrenadeSound;

	public SoundProvider StopAimingSound;

	public SoundProvider StartAimingSound;

	public SoundSource AimingSound;

	public Vector2 QuickThrowSpeed = new Vector2(14f, 16f);

	public GameObject GrenadeFailEffect;

	public float AimAnimationAngleOffset = 5f;

	public float CursorSpeedMultiplier = 1f;

	public float CursorSpeedYOffset = 12f;

	private float m_lockPressingInputTime;

	private Vector2 m_rawAimOffset = new Vector2(14f, 16f);

	private SeinCharacter m_sein;

	private bool m_isAiming;

	private Vector2 m_aimOffset;

	private List<SpiritGrenade> m_spiritGrenades = new List<SpiritGrenade>();

	private float m_animationAimAngle;

	private bool m_lastAimWasOnWall;

	public TextureAnimationWithTransitions[] AimingAnimations;

	public TextureAnimationWithTransitions[] ThrowAnimations;

	public TextureAnimationWithTransitions[] WallAimingAnimations;

	public TextureAnimationWithTransitions[] WallThrowAnimations;

	public TextureAnimationWithTransitions[] NotEnoughEnergyThrowAnimations;

	public TextureAnimationWithTransitions[] NotEnoughEnergyWallThrowAnimations;

	public SeinGrenadeAttack.QuickThrowAnimations QuickThrow;

	public AnimationMetaData WallAimingMetaData;

	public AnimationMetaData AimingMetaData;

	private float m_lockAimAnimationRemainingTime;

	private bool m_faceLeft;

	public float MaxAimWallAnimationAngle = 85f;

	public float MinAimWallAnimationAngle = -80f;

	public float MaxAimGroundAnimationAngle = 90f;

	public float MinAimGroundAnimationAngle = -30f;

	private bool m_inputPressed;

	public List<SeinGrenadeAttack.FastThrowAnimationRule> FastThrowAnimations;

	private bool m_autoAim;

	private IAttackable m_autoTarget;

	public SeinGrenadeAttack.AutoAimSettings AutoAim;

	[Serializable]
	public class QuickThrowAnimations
	{
		public TextureAnimationWithTransitions FallIdleThrowAnimation;

		public TextureAnimationWithTransitions FallThrowAnimation;

		public TextureAnimationWithTransitions RunThrowAnimation;

		public TextureAnimationWithTransitions JogThrowAnimation;

		public TextureAnimationWithTransitions WalkThrowAnimation;

		public TextureAnimationWithTransitions JumpThrowAnimation;

		public TextureAnimationWithTransitions JumpIdleThrowAnimation;

		public TextureAnimationWithTransitions IdleThrowAnimation;

		public TextureAnimationWithTransitions WallThrowAnimation;
	}

	[Serializable]
	public class FastThrowAnimationRule
	{
		public TextureAnimationWithTransitions ThrowAnimation;

		public List<TextureAnimationWithTransitions> AnimationsWithTransitions;

		public List<TextureAnimation> Animations;

		public SeinGrenadeAttack.FastThrowAnimationRule.AnimationRule PlayRule;

		public enum AnimationRule
		{
			InAir,
			OnGround
		}
	}

	[Serializable]
	public class AutoAimSettings
	{
		public float MaxDistance = 30f;

		public float MinDistance = 2f;

		public float Speed = 5f;

		public float SpeedPerXDistance = 0.7f;

		public float SpeedPerYDistance = 2f;

		public float InAirSpeed = 30f;
	}
}
