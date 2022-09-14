using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

public class SeinDashAttack : CharacterState, ISeinReceiver
{
	static SeinDashAttack()
	{
		SeinDashAttack.OnDashEvent = delegate()
		{
		};
		SeinDashAttack.OnWallDashEvent = delegate()
		{
		};
	}

	public static event Action OnDashEvent;

	public static event Action OnWallDashEvent;

	public bool HasEnoughEnergy
	{
		get
		{
			float num = RandomizerBonus.ChargeDashEfficiency() ? 0.5f : 0f;
			return this.m_sein.Energy.CanAfford(this.EnergyCost - num);
		}
	}

	public override void Serialize(Archive ar)
	{
		if (ar.Reading)
		{
			this.ReturnToNormal();
		}
	}

	public override void OnExit()
	{
		this.ReturnToNormal();
		base.OnExit();
	}

	public void OnDisable()
	{
		this.Exit();
	}

	public void ReturnToNormal()
	{
		if (this.CurrentState != SeinDashAttack.State.Normal)
		{
			if (this.CurrentState == SeinDashAttack.State.Dashing)
			{
				this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeedX = (float)((!this.m_faceLeft) ? 1 : -1) * this.DashSpeedOverTime.Evaluate((float)this.DashSpeedOverTime.length);
			}
			if (this.CurrentState == SeinDashAttack.State.ChargeDashing)
			{
				this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeedX = (float)((!this.m_faceLeft) ? 1 : -1) * this.ChargeDashSpeedOverTime.Evaluate((float)this.ChargeDashSpeedOverTime.length);
			}
			UI.Cameras.Current.ChaseTarget.CameraSpeedMultiplier.x = 1f;
			if (this.CurrentState == SeinDashAttack.State.ChargeDashing)
			{
				this.RestoreEnergy();
			}
			this.ChangeState(SeinDashAttack.State.Normal);
		}
	}

	public void SpendEnergy()
	{
		float num = RandomizerBonus.ChargeDashEfficiency() ? 0.5f : 0f;
		this.m_sein.Energy.Spend(this.EnergyCost - num);
	}

	public void RestoreEnergy()
	{
		float num = RandomizerBonus.ChargeDashEfficiency() ? 0.5f : 0f;
		this.m_sein.Energy.Gain(this.EnergyCost - num);
	}

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		sein.Abilities.Dash = this;
	}

	public override void UpdateCharacterState()
	{
		this.UpdateState();
	}

	public bool IsDashingOrChangeDashing
	{
		get
		{
			if (this.CurrentState == SeinDashAttack.State.Dashing)
			{
				return this.m_stateCurrentTime < this.DashTime;
			}
			return this.CurrentState == SeinDashAttack.State.ChargeDashing && this.m_stateCurrentTime < this.ChargeDashTime;
		}
	}

	public void ChangeState(SeinDashAttack.State state)
	{
		this.CurrentState = state;
		this.m_stateCurrentTime = 0f;
		this.m_attackablesIgnore.Clear();
	}

	public IChargeDashAttackable FindClosestAttackable
	{
		get
		{
			IChargeDashAttackable result = null;
			float num = float.MaxValue;
			foreach (IAttackable attackable in Targets.Attackables)
			{
				if (attackable as Component && attackable.CanBeChargeDashed() && attackable is IChargeDashAttackable)
				{
					IChargeDashAttackable chargeDashAttackable = (IChargeDashAttackable)attackable;
					if (UI.Cameras.Current.IsOnScreen(attackable.Position))
					{
						float magnitude = (attackable.Position - this.m_sein.Position).magnitude;
						if (magnitude < num && magnitude < this.ChargeDashTargetMaxDistance)
						{
							result = chargeDashAttackable;
							num = magnitude;
						}
					}
				}
			}
			return result;
		}
	}

	public void AttackNearbyEnemies()
	{
		int i = 0;
		while (i < Targets.Attackables.Count)
		{
			IAttackable attackable = Targets.Attackables[i];
			if (!InstantiateUtility.IsDestroyed(attackable as Component) && !this.m_attackablesIgnore.Contains(attackable) && attackable.CanBeChargeFlamed() && (attackable.Position - this.m_sein.PlatformBehaviour.PlatformMovement.HeadPosition).magnitude <= 3f)
			{
				this.m_attackablesIgnore.Add(attackable);
				Vector3 v = (!this.m_chargeDashAtTarget) ? (((!this.m_faceLeft) ? Vector3.right : Vector3.left) * 3f) : (this.m_chargeDashDirection * 3f);
				new Damage((float)this.Damage, v, this.m_sein.Position, DamageType.ChargeFlame, base.gameObject).DealToComponents(((Component)attackable).gameObject);
				this.m_hasHitAttackable = true;
				if (this.ExplosionEffect && Time.time - this.m_timeOfLastExplosionEffect > 0.1f)
				{
					this.m_timeOfLastExplosionEffect = Time.time;
					InstantiateUtility.Instantiate(this.ExplosionEffect, Vector3.Lerp(base.transform.position, attackable.Position, 0.5f), Quaternion.identity);
					return;
				}
				break;
			}
			else
			{
				i++;
			}
		}
	}

	private void PerformDash(TextureAnimationWithTransitions dashAnimation, SoundProvider dashSound)
	{
		this.m_sein.Mortality.DamageReciever.ResetInviciblity();
		this.m_hasDashed = true;
		if (RandomizerBonus.DoubleAirDash() && !RandomizerBonus.DoubleAirDashUsed)
		{
			this.m_hasDashed = false;
			RandomizerBonus.DoubleAirDashUsed = true;
		}
		this.m_isOnGround = this.m_sein.IsOnGround;
		this.m_lastDashTime = Time.time;
		this.m_lastPressTime = 0f;
		this.SpriteRotation = this.m_sein.PlatformBehaviour.PlatformMovement.GroundAngle;
		this.m_allowNoDecelerationForThisDash = true;
		if (this.m_chargeDashAtTarget)
		{
			this.m_faceLeft = (this.m_chargeDashDirection.x < 0f);
		}
		else if (this.m_sein.PlatformBehaviour.PlatformMovement.HasWallLeft)
		{
			this.m_faceLeft = false;
		}
		else if (this.m_sein.PlatformBehaviour.PlatformMovement.HasWallRight)
		{
			this.m_faceLeft = true;
		}
		else if (this.m_sein.Input.NormalizedHorizontal != 0)
		{
			this.m_faceLeft = (this.m_sein.Input.NormalizedHorizontal < 0);
		}
		else if (!Mathf.Approximately(this.m_sein.Speed.x, 0f))
		{
			this.m_faceLeft = (this.m_sein.Speed.x < 0f);
		}
		else
		{
			this.m_faceLeft = this.m_sein.FaceLeft;
			this.m_allowNoDecelerationForThisDash = false;
		}
		this.m_sein.FaceLeft = this.m_faceLeft;
		this.m_stopAnimation = false;
		if (dashSound)
		{
			Sound.Play(dashSound.GetSound(null), this.m_sein.Position, null);
		}
		this.m_sein.Animation.Play(dashAnimation, 154, new Func<bool>(this.KeepDashAnimationPlaying));
		if (SeinDashAttack.RainbowDashActivated)
		{
			((GameObject)InstantiateUtility.Instantiate(this.DashFollowRainbowEffect, this.m_sein.Position, Quaternion.identity)).transform.parent = this.m_sein.Transform;
		}
		this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeedY = -this.DashDownwardSpeed;
	}

	public void PerformDash()
	{
		this.m_chargeDashAtTarget = false;
		SoundProvider dashSound = (!SeinDashAttack.RainbowDashActivated) ? this.DashSound : this.RainbowDashSound;
		bool isGliding = this.m_sein.Controller.IsGliding;
		this.PerformDash((!isGliding) ? this.DashAnimation : this.GlideDashAnimation, dashSound);
		this.ChangeState(SeinDashAttack.State.Dashing);
		this.UpdateDashing();
		SeinDashAttack.OnDashEvent();
	}

	public void PerformWallDash()
	{
		this.m_chargeDashAtTarget = false;
		SoundProvider dashSound = (!SeinDashAttack.RainbowDashActivated) ? this.DashSound : this.RainbowDashSound;
		this.PerformDash(this.DashAnimation, dashSound);
		this.ChangeState(SeinDashAttack.State.Dashing);
		this.UpdateDashing();
		SeinDashAttack.OnWallDashEvent();
	}

	public void PerformDashIntoWall()
	{
		this.m_lastPressTime = 0f;
		this.m_lastDashTime = Time.time;
		this.m_sein.Animation.Play(this.DashIntoWallAnimation, 154, new Func<bool>(this.KeepDashIntoWallAnimationPlaying));
		Sound.Play(this.DashIntoWallSound.GetSound(null), this.m_sein.Position, null);
	}

	public bool KeepDashIntoWallAnimationPlaying()
	{
		return this.AgainstWall() && this.m_sein.IsOnGround;
	}

	public void PerformChargeDash()
	{
		this.m_hasHitAttackable = false;
		this.m_chargeJumpWasReleased = false;
		this.m_chargeDashAttackTarget = (this.FindClosestAttackable as IAttackable);
		if (this.m_chargeDashAttackTarget != null)
		{
			this.m_chargeDashAtTarget = true;
			this.m_chargeDashDirection = (this.m_chargeDashAttackTarget.Position - this.m_sein.Position).normalized;
			this.m_chargeDashAtTargetPosition = this.m_chargeDashAttackTarget.Position;
		}
		else
		{
			this.m_chargeDashAtTarget = false;
		}
		SoundProvider dashSound = (!SeinDashAttack.RainbowDashActivated) ? this.ChargeDashSound : this.RainbowDashSound;
		this.PerformDash(this.ChargeDashAnimation, dashSound);
		if (this.m_chargeDashAtTarget)
		{
			this.SpriteRotation = Mathf.Atan2(this.m_chargeDashDirection.y, this.m_chargeDashDirection.x) * 57.29578f - (float)((!this.m_faceLeft) ? 0 : 180);
		}
		this.ChangeState(SeinDashAttack.State.ChargeDashing);
		this.CompleteChargeEffect();
		this.UpdateChargeDashing();
	}

	private bool HasChargeDashSkill()
	{
		return this.m_sein.PlayerAbilities.ChargeDash.HasAbility;
	}

	private bool HasAirDashSkill()
	{
		return this.m_sein.PlayerAbilities.AirDash.HasAbility;
	}

	private bool CanChargeDash()
	{
		return this.HasChargeDashSkill() && Core.Input.ChargeJump.Pressed && this.m_chargeJumpWasReleased && !Characters.Sein.Abilities.Swimming.IsSwimming;
	}

	public void CompleteChargeEffect()
	{
		if (this.m_sein.Abilities.ChargeJumpCharging)
		{
			this.m_sein.Abilities.ChargeJumpCharging.EndCharge();
		}
	}

	private void UpdateTargetHighlight(IChargeDashAttackable target)
	{
		if (this.m_lastTarget == target)
		{
			return;
		}
		if (!InstantiateUtility.IsDestroyed(this.m_lastTarget as Component))
		{
			this.m_lastTarget.OnChargeDashDehighlight();
		}
		this.m_lastTarget = target;
		if (!InstantiateUtility.IsDestroyed(this.m_lastTarget as Component))
		{
			this.m_lastTarget.OnChargeDashHighlight();
		}
	}

	public bool KeepDashAnimationPlaying()
	{
		return !this.m_stopAnimation && !this.m_sein.Abilities.WallSlide.IsOnWall && base.Active;
	}

	public bool KeepChargeDashAnimationPlaying()
	{
		return this.KeepDashAnimationPlaying();
	}

	public bool AgainstWall()
	{
		PlatformMovement platformMovement = this.m_sein.PlatformBehaviour.PlatformMovement;
		return (platformMovement.HasWallLeft && this.m_sein.FaceLeft) || (platformMovement.HasWallRight && !this.m_sein.FaceLeft);
	}

	public bool CanPerformNormalDash()
	{
		return	((this.HasAirDashSkill() || this.m_sein.IsOnGround || (RandomizerBonus.GravitySuit() &&  Characters.Sein.Abilities.Swimming.IsSwimming)) && !this.AgainstWall() && this.DashHasCooledDown && !this.m_hasDashed);
	}

	private bool DashHasCooledDown
	{
		get
		{
			return Time.time - this.m_lastDashTime > 0.4f;
		}
	}

	public bool CanPerformDashIntoWall()
	{
		return this.m_sein.IsOnGround && this.AgainstWall() && this.DashHasCooledDown;
	}

	public bool CanWallDash()
	{
		PlatformMovement platformMovement = this.m_sein.PlatformBehaviour.PlatformMovement;
		return ((platformMovement.HasWallLeft && this.m_sein.Input.Horizontal >= 0f) || (platformMovement.HasWallRight && this.m_sein.Input.Horizontal <= 0f)) && !this.m_sein.IsOnGround && this.m_sein.PlayerAbilities.AirDash.HasAbility;
	}

	public void UpdateNormal()
	{
		float num = Time.time - this.m_lastPressTime;
		if (this.m_sein.IsOnGround || (RandomizerBonus.GravitySuit() && Characters.Sein.Abilities.Swimming.IsSwimming))
		{
			this.m_hasDashed = false;
			RandomizerBonus.DoubleAirDashUsed = false;
		}
		if (Core.Input.Glide.Pressed && this.m_timeWhenDashJumpHappened + 5f > Time.time)
		{
			this.m_timeWhenDashJumpHappened = 0f;
			PlatformMovement platformMovement = this.m_sein.PlatformBehaviour.PlatformMovement;
			float num2 = this.OffGroundSpeed - 2f;
			if (Mathf.Abs(platformMovement.LocalSpeedX) > num2)
			{
				platformMovement.LocalSpeedX = Mathf.Sign(platformMovement.LocalSpeedX) * num2;
			}
		}
		IChargeDashAttackable target;
		if (this.CanChargeDash())
		{
			target = this.FindClosestAttackable;
		}
		else
		{
			target = null;
		}
		this.UpdateTargetHighlight(target);
		if (Core.Input.RightShoulder.Pressed && num < 0.15f)
		{
			if (this.CanChargeDash())
			{
				if (this.HasEnoughEnergy)
				{
					this.SpendEnergy();
					this.PerformChargeDash();
					return;
				}
				this.ShowNotEnoughEnergy();
				this.m_lastPressTime = 0f;
				return;
			}
			else
			{
				if (this.CanPerformNormalDash())
				{
					this.PerformDash();
					return;
				}
				if (this.CanWallDash())
				{
					this.PerformWallDash();
					return;
				}
				if (this.CanPerformDashIntoWall())
				{
					this.PerformDashIntoWall();
				}
			}
		}
	}

	private void ShowNotEnoughEnergy()
	{
		UI.SeinUI.ShakeEnergyOrbBar();
		if (this.NotEnoughEnergySound)
		{
			Sound.Play(this.NotEnoughEnergySound.GetSound(null), base.transform.position, null);
		}
	}

	public void UpdateDashing()
	{
		PlatformMovement platformMovement = this.m_sein.PlatformBehaviour.PlatformMovement;
		UI.Cameras.Current.ChaseTarget.CameraSpeedMultiplier.x = Mathf.Clamp01(this.m_stateCurrentTime / this.DashTime);
		float velocity = this.DashSpeedOverTime.Evaluate(this.m_stateCurrentTime);
		velocity *= 1.0f + .2f*RandomizerBonus.Velocity();
		if ((RandomizerBonus.GravitySuit() && Characters.Sein.Abilities.Swimming.IsSwimming))
		{
			Vector2 newSpeed = new Vector2(velocity, 0f);
			platformMovement.LocalSpeed = newSpeed.Rotate(this.m_sein.Abilities.Swimming.SwimAngle);
		}
		else
		{
			platformMovement.LocalSpeedX = (float)((!this.m_faceLeft) ? 1 : -1) * velocity;
		}
		this.m_sein.FaceLeft = this.m_faceLeft;
		if (this.AgainstWall())
		{
			platformMovement.LocalSpeed = Vector2.zero;
		}
		this.SpriteRotation = Mathf.Lerp(this.SpriteRotation, this.m_sein.PlatformBehaviour.PlatformMovement.GroundAngle, 0.2f);
		if (this.m_sein.IsOnGround)
		{
			if (Core.Input.Horizontal > 0f && this.m_faceLeft)
			{
				this.StopDashing();
			}
			if (Core.Input.Horizontal < 0f && !this.m_faceLeft)
			{
				this.StopDashing();
			}
		}
		if (this.m_stateCurrentTime > this.DashTime)
		{
			if (platformMovement.IsOnGround && Core.Input.Horizontal == 0f)
			{
				platformMovement.LocalSpeedX = 0f;
			}
			this.ChangeState(SeinDashAttack.State.Normal);
		}
		if (Core.Input.Jump.OnPressed || Core.Input.Glide.OnPressed)
		{
			platformMovement.LocalSpeedX = ((!this.m_faceLeft) ? this.OffGroundSpeed : (-this.OffGroundSpeed));
			this.m_sein.PlatformBehaviour.AirNoDeceleration.NoDeceleration = this.m_allowNoDecelerationForThisDash;
			this.m_stopAnimation = true;
			this.ChangeState(SeinDashAttack.State.Normal);
			this.m_timeWhenDashJumpHappened = Time.time;
		}
		if (this.RaycastTest() && this.m_isOnGround)
		{
			this.StickOntoGround();
			return;
		}
		this.m_isOnGround = false;
	}

	private void StickOntoGround()
	{
		PlatformMovement platformMovement = this.m_sein.PlatformBehaviour.PlatformMovement;
		Vector3 vector = platformMovement.Position;
		platformMovement.PlaceOnGround(0f, 8f);
		Vector3 vector2 = vector;
		platformMovement.PlaceOnGround(0.5f, 8f);
		Vector3 vector3 = vector;
		vector = vector2;
		if (vector3.y > vector2.y)
		{
			vector = vector3;
		}
		platformMovement.Position = vector;
	}

	public void UpdateChargeDashing()
	{
		PlatformMovement platformMovement = this.m_sein.PlatformBehaviour.PlatformMovement;
		this.AttackNearbyEnemies();
		this.m_sein.Mortality.DamageReciever.MakeInvincibleToEnemies(1f);
		float velocity = this.ChargeDashSpeedOverTime.Evaluate(this.m_stateCurrentTime);
		velocity *= 1.0f + .2f*RandomizerBonus.Velocity();
		if (this.m_chargeDashAtTarget)
		{
			platformMovement.LocalSpeed = this.m_chargeDashDirection * velocity;
		}
		else
		{
			platformMovement.LocalSpeedX = (float)((!this.m_faceLeft) ? 1 : -1) * velocity;
		}
		if (this.m_hasHitAttackable)
		{
			platformMovement.LocalSpeed *= 0.33f;
		}
		this.m_sein.FaceLeft = this.m_faceLeft;
		this.SpriteRotation = Mathf.Lerp(this.SpriteRotation, this.m_sein.PlatformBehaviour.PlatformMovement.GroundAngle, 0.3f);
		if (this.AgainstWall())
		{
			platformMovement.LocalSpeed = Vector2.zero;
		}
		if (this.m_sein.IsOnGround)
		{
			if (Core.Input.Horizontal > 0f && this.m_faceLeft)
			{
				this.StopDashing();
			}
			if (Core.Input.Horizontal < 0f && !this.m_faceLeft)
			{
				this.StopDashing();
			}
		}
		if (this.m_stateCurrentTime > this.ChargeDashTime)
		{
			this.ChangeState(SeinDashAttack.State.Normal);
		}
		if (Core.Input.Jump.OnPressed || Core.Input.Glide.OnPressed)
		{
			platformMovement.LocalSpeedX = ((!this.m_faceLeft) ? this.OffGroundSpeed : (-this.OffGroundSpeed));
			this.m_sein.PlatformBehaviour.AirNoDeceleration.NoDeceleration = true;
			this.m_stopAnimation = true;
			this.ChangeState(SeinDashAttack.State.Normal);
		}
		if (this.RaycastTest() && this.m_isOnGround && !this.m_chargeDashAtTarget)
		{
			this.StickOntoGround();
			return;
		}
		this.m_isOnGround = false;
	}

	public void UpdateState()
	{
		UI.Cameras.Current.ChaseTarget.CameraSpeedMultiplier.x = 1f;
		if (Core.Input.RightShoulder.OnPressed)
		{
			this.m_lastPressTime = Time.time;
		}
		if (Core.Input.ChargeJump.Released)
		{
			this.m_chargeJumpWasReleased = true;
		}
		switch (this.CurrentState)
		{
		case SeinDashAttack.State.Normal:
			this.UpdateNormal();
			break;
		case SeinDashAttack.State.Dashing:
			this.UpdateDashing();
			break;
		case SeinDashAttack.State.ChargeDashing:
			this.UpdateChargeDashing();
			break;
		}
		this.m_stateCurrentTime += Time.deltaTime;
	}

	public void StopDashing()
	{
		this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeed = Vector2.zero;
		this.ChangeState(SeinDashAttack.State.Normal);
		this.m_stopAnimation = true;
		this.m_chargeDashAtTarget = false;
	}

	private bool RaycastTest()
	{
		Vector3 a = Vector3.Cross(this.m_sein.PlatformBehaviour.PlatformMovement.GroundRayNormal, Vector3.forward);
		float num = this.m_sein.Speed.x * Time.deltaTime;
		Vector3 vector = this.m_sein.Position + a * num + Vector3.up;
		Vector3 vector2 = Vector3.down * (1.8f + Mathf.Abs(num));
		Debug.DrawRay(vector, vector2, Color.yellow, 0.5f);
		RaycastHit raycastHit;
		return this.m_sein.Controller.RayTest(vector, vector2, out raycastHit);
	}

	public void ResetDashLimit()
	{
		this.m_hasDashed = false;
		RandomizerBonus.DoubleAirDashUsed = false;
	}

	public AnimationCurve DashSpeedOverTime;

	public AnimationCurve ChargeDashSpeedOverTime;

	public float DashTime = 0.5f;

	public float ChargeDashTime = 0.5f;

	public float ChargeTime = 0.2f;

	public SoundProvider ChargeSound;

	public SoundProvider DoneChargingSound;

	public SoundSource ChargedSound;

	public SoundProvider UnChargeSound;

	public SoundProvider DashSound;

	public SoundProvider ChargeDashSound;

	public SoundProvider RainbowDashSound;

	public SoundProvider DashIntoWallSound;

	public GameObject ExplosionEffect;

	public SeinDashAttack.State CurrentState;

	public float DashDownwardSpeed = 10f;

	public float OffGroundSpeed = 15f;

	public int Damage = 50;

	public float EnergyCost = 1f;

	public SoundProvider NotEnoughEnergySound;

	public TextureAnimationWithTransitions DashAnimation;

	public TextureAnimationWithTransitions ChargeDashAnimation;

	public TextureAnimationWithTransitions GlideDashAnimation;

	public TextureAnimationWithTransitions DashIntoWallAnimation;

	public GameObject DashStartEffect;

	public GameObject DashFollowEffect;

	public GameObject DashFollowRainbowEffect;

	private SeinCharacter m_sein;

	private bool m_faceLeft;

	private float m_stateCurrentTime;

	private HashSet<IAttackable> m_attackablesIgnore = new HashSet<IAttackable>();

	private bool m_stopAnimation;

	private float m_lastPressTime;

	private float m_lastDashTime;

	private bool m_isOnGround;

	public static bool RainbowDashActivated;

	private bool m_hasDashed;

	public float ChargeDashTargetMaxDistance = 20f;

	private float m_timeOfLastExplosionEffect;

	private float m_timeWhenDashJumpHappened;

	private bool m_allowNoDecelerationForThisDash;

	private IAttackable m_chargeDashAttackTarget;

	private bool m_hasHitAttackable;

	private bool m_chargeJumpWasReleased = true;

	private IChargeDashAttackable m_lastTarget;

	public float SpriteRotation;

	private Vector3 m_chargeDashDirection;

	private bool m_chargeDashAtTarget;

	private Vector3 m_chargeDashAtTargetPosition;

	public enum State
	{
		Normal,
		Dashing,
		ChargeDashing
	}
}
