using System;
using System.Collections.Generic;
using Core;
using Game;
using UnityEngine;

// Token: 0x0200031B RID: 795
public class SeinDashAttack : CharacterState, ISeinReceiver
{
	// Token: 0x06001057 RID: 4183
	static SeinDashAttack()
	{
		SeinDashAttack.OnDashEvent = delegate()
		{
		};
		SeinDashAttack.OnWallDashEvent = delegate()
		{
		};
	}

	// Token: 0x14000012 RID: 18
	// (add) Token: 0x06001058 RID: 4184
	// (remove) Token: 0x06001059 RID: 4185
	public static event Action OnDashEvent;

	// Token: 0x14000013 RID: 19
	// (add) Token: 0x0600105A RID: 4186
	// (remove) Token: 0x0600105B RID: 4187
	public static event Action OnWallDashEvent;

	// Token: 0x170002A9 RID: 681
	// (get) Token: 0x0600105C RID: 4188
	public bool HasEnoughEnergy
	{
		get
		{
			float num = RandomizerBonus.ChargeDashEfficiency() ? 0.5f : 0f;
			return this.m_sein.Energy.CanAfford(this.EnergyCost - num);
		}
	}

	// Token: 0x0600105D RID: 4189
	public override void Serialize(Archive ar)
	{
		if (ar.Reading)
		{
			this.ReturnToNormal();
		}
	}

	// Token: 0x0600105E RID: 4190
	public override void OnExit()
	{
		this.ReturnToNormal();
		base.OnExit();
	}

	// Token: 0x0600105F RID: 4191
	public void OnDisable()
	{
		this.Exit();
	}

	// Token: 0x06001060 RID: 4192
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

	// Token: 0x06001061 RID: 4193
	public void SpendEnergy()
	{
		float num = RandomizerBonus.ChargeDashEfficiency() ? 0.5f : 0f;
		this.m_sein.Energy.Spend(this.EnergyCost - num);
	}

	// Token: 0x06001062 RID: 4194
	public void RestoreEnergy()
	{
		float num = RandomizerBonus.ChargeDashEfficiency() ? 0.5f : 0f;
		this.m_sein.Energy.Gain(this.EnergyCost - num);
	}

	// Token: 0x06001063 RID: 4195
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		sein.Abilities.Dash = this;
	}

	// Token: 0x06001064 RID: 4196
	public override void UpdateCharacterState()
	{
		this.UpdateState();
	}

	// Token: 0x170002AA RID: 682
	// (get) Token: 0x06001065 RID: 4197
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

	// Token: 0x06001066 RID: 4198
	public void ChangeState(SeinDashAttack.State state)
	{
		this.CurrentState = state;
		this.m_stateCurrentTime = 0f;
		this.m_attackablesIgnore.Clear();
	}

	// Token: 0x170002AB RID: 683
	// (get) Token: 0x06001067 RID: 4199
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

	// Token: 0x06001068 RID: 4200
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

	// Token: 0x06001069 RID: 4201
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

	// Token: 0x0600106A RID: 4202
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

	// Token: 0x0600106B RID: 4203
	public void PerformWallDash()
	{
		this.m_chargeDashAtTarget = false;
		SoundProvider dashSound = (!SeinDashAttack.RainbowDashActivated) ? this.DashSound : this.RainbowDashSound;
		this.PerformDash(this.DashAnimation, dashSound);
		this.ChangeState(SeinDashAttack.State.Dashing);
		this.UpdateDashing();
		SeinDashAttack.OnWallDashEvent();
	}

	// Token: 0x0600106C RID: 4204
	public void PerformDashIntoWall()
	{
		this.m_lastPressTime = 0f;
		this.m_lastDashTime = Time.time;
		this.m_sein.Animation.Play(this.DashIntoWallAnimation, 154, new Func<bool>(this.KeepDashIntoWallAnimationPlaying));
		Sound.Play(this.DashIntoWallSound.GetSound(null), this.m_sein.Position, null);
	}

	// Token: 0x0600106D RID: 4205
	public bool KeepDashIntoWallAnimationPlaying()
	{
		return this.AgainstWall() && this.m_sein.IsOnGround;
	}

	// Token: 0x0600106E RID: 4206
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

	// Token: 0x0600106F RID: 4207
	private bool HasChargeDashSkill()
	{
		return this.m_sein.PlayerAbilities.ChargeDash.HasAbility;
	}

	// Token: 0x06001070 RID: 4208
	private bool HasAirDashSkill()
	{
		return this.m_sein.PlayerAbilities.AirDash.HasAbility;
	}

	// Token: 0x06001071 RID: 4209
	private bool CanChargeDash()
	{
		return this.HasChargeDashSkill() && Core.Input.ChargeJump.Pressed && this.m_chargeJumpWasReleased && !Characters.Sein.Abilities.Swimming.IsSwimming;
	}

	// Token: 0x06001072 RID: 4210
	public void CompleteChargeEffect()
	{
		if (this.m_sein.Abilities.ChargeJumpCharging)
		{
			this.m_sein.Abilities.ChargeJumpCharging.EndCharge();
		}
	}

	// Token: 0x06001073 RID: 4211
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

	// Token: 0x06001074 RID: 4212
	public bool KeepDashAnimationPlaying()
	{
		return !this.m_stopAnimation && !this.m_sein.Abilities.WallSlide.IsOnWall && base.Active;
	}

	// Token: 0x06001075 RID: 4213
	public bool KeepChargeDashAnimationPlaying()
	{
		return this.KeepDashAnimationPlaying();
	}

	// Token: 0x06001076 RID: 4214
	public bool AgainstWall()
	{
		PlatformMovement platformMovement = this.m_sein.PlatformBehaviour.PlatformMovement;
		return (platformMovement.HasWallLeft && this.m_sein.FaceLeft) || (platformMovement.HasWallRight && !this.m_sein.FaceLeft);
	}

	// Token: 0x06001077 RID: 4215
	public bool CanPerformNormalDash()
	{
		return	((this.HasAirDashSkill() || this.m_sein.IsOnGround || (RandomizerBonus.GravitySuit() &&  Characters.Sein.Abilities.Swimming.IsSwimming)) && !this.AgainstWall() && this.DashHasCooledDown && !this.m_hasDashed);
	}

	// Token: 0x170002AC RID: 684
	// (get) Token: 0x06001078 RID: 4216
	private bool DashHasCooledDown
	{
		get
		{
			return Time.time - this.m_lastDashTime > 0.4f;
		}
	}

	// Token: 0x06001079 RID: 4217
	public bool CanPerformDashIntoWall()
	{
		return this.m_sein.IsOnGround && this.AgainstWall() && this.DashHasCooledDown;
	}

	// Token: 0x0600107A RID: 4218
	public bool CanWallDash()
	{
		PlatformMovement platformMovement = this.m_sein.PlatformBehaviour.PlatformMovement;
		return ((platformMovement.HasWallLeft && this.m_sein.Input.Horizontal >= 0f) || (platformMovement.HasWallRight && this.m_sein.Input.Horizontal <= 0f)) && !this.m_sein.IsOnGround && this.m_sein.PlayerAbilities.AirDash.HasAbility;
	}

	// Token: 0x0600107B RID: 4219
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

	// Token: 0x0600107C RID: 4220
	private void ShowNotEnoughEnergy()
	{
		UI.SeinUI.ShakeEnergyOrbBar();
		if (this.NotEnoughEnergySound)
		{
			Sound.Play(this.NotEnoughEnergySound.GetSound(null), base.transform.position, null);
		}
	}

	// Token: 0x0600107D RID: 4221
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

	// Token: 0x0600107E RID: 4222
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

	// Token: 0x0600107F RID: 4223
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

	// Token: 0x06001080 RID: 4224
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

	// Token: 0x06001081 RID: 4225
	public void StopDashing()
	{
		this.m_sein.PlatformBehaviour.PlatformMovement.LocalSpeed = Vector2.zero;
		this.ChangeState(SeinDashAttack.State.Normal);
		this.m_stopAnimation = true;
		this.m_chargeDashAtTarget = false;
	}

	// Token: 0x06001082 RID: 4226
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

	// Token: 0x06001083 RID: 4227
	public void ResetDashLimit()
	{
		this.m_hasDashed = false;
		RandomizerBonus.DoubleAirDashUsed = false;
	}

	// Token: 0x04000F8C RID: 3980
	public AnimationCurve DashSpeedOverTime;

	// Token: 0x04000F8D RID: 3981
	public AnimationCurve ChargeDashSpeedOverTime;

	// Token: 0x04000F8E RID: 3982
	public float DashTime = 0.5f;

	// Token: 0x04000F8F RID: 3983
	public float ChargeDashTime = 0.5f;

	// Token: 0x04000F90 RID: 3984
	public float ChargeTime = 0.2f;

	// Token: 0x04000F91 RID: 3985
	public SoundProvider ChargeSound;

	// Token: 0x04000F92 RID: 3986
	public SoundProvider DoneChargingSound;

	// Token: 0x04000F93 RID: 3987
	public SoundSource ChargedSound;

	// Token: 0x04000F94 RID: 3988
	public SoundProvider UnChargeSound;

	// Token: 0x04000F95 RID: 3989
	public SoundProvider DashSound;

	// Token: 0x04000F96 RID: 3990
	public SoundProvider ChargeDashSound;

	// Token: 0x04000F97 RID: 3991
	public SoundProvider RainbowDashSound;

	// Token: 0x04000F98 RID: 3992
	public SoundProvider DashIntoWallSound;

	// Token: 0x04000F99 RID: 3993
	public GameObject ExplosionEffect;

	// Token: 0x04000F9A RID: 3994
	public SeinDashAttack.State CurrentState;

	// Token: 0x04000F9B RID: 3995
	public float DashDownwardSpeed = 10f;

	// Token: 0x04000F9C RID: 3996
	public float OffGroundSpeed = 15f;

	// Token: 0x04000F9D RID: 3997
	public int Damage = 50;

	// Token: 0x04000F9E RID: 3998
	public float EnergyCost = 1f;

	// Token: 0x04000F9F RID: 3999
	public SoundProvider NotEnoughEnergySound;

	// Token: 0x04000FA0 RID: 4000
	public TextureAnimationWithTransitions DashAnimation;

	// Token: 0x04000FA1 RID: 4001
	public TextureAnimationWithTransitions ChargeDashAnimation;

	// Token: 0x04000FA2 RID: 4002
	public TextureAnimationWithTransitions GlideDashAnimation;

	// Token: 0x04000FA3 RID: 4003
	public TextureAnimationWithTransitions DashIntoWallAnimation;

	// Token: 0x04000FA4 RID: 4004
	public GameObject DashStartEffect;

	// Token: 0x04000FA5 RID: 4005
	public GameObject DashFollowEffect;

	// Token: 0x04000FA6 RID: 4006
	public GameObject DashFollowRainbowEffect;

	// Token: 0x04000FA7 RID: 4007
	private SeinCharacter m_sein;

	// Token: 0x04000FA8 RID: 4008
	private bool m_faceLeft;

	// Token: 0x04000FA9 RID: 4009
	private float m_stateCurrentTime;

	// Token: 0x04000FAA RID: 4010
	private HashSet<IAttackable> m_attackablesIgnore = new HashSet<IAttackable>();

	// Token: 0x04000FAB RID: 4011
	private bool m_stopAnimation;

	// Token: 0x04000FAC RID: 4012
	private float m_lastPressTime;

	// Token: 0x04000FAD RID: 4013
	private float m_lastDashTime;

	// Token: 0x04000FAE RID: 4014
	private bool m_isOnGround;

	// Token: 0x04000FAF RID: 4015
	public static bool RainbowDashActivated;

	// Token: 0x04000FB0 RID: 4016
	private bool m_hasDashed;

	// Token: 0x04000FB1 RID: 4017
	public float ChargeDashTargetMaxDistance = 20f;

	// Token: 0x04000FB2 RID: 4018
	private float m_timeOfLastExplosionEffect;

	// Token: 0x04000FB3 RID: 4019
	private float m_timeWhenDashJumpHappened;

	// Token: 0x04000FB4 RID: 4020
	private bool m_allowNoDecelerationForThisDash;

	// Token: 0x04000FB5 RID: 4021
	private IAttackable m_chargeDashAttackTarget;

	// Token: 0x04000FB6 RID: 4022
	private bool m_hasHitAttackable;

	// Token: 0x04000FB7 RID: 4023
	private bool m_chargeJumpWasReleased = true;

	// Token: 0x04000FB8 RID: 4024
	private IChargeDashAttackable m_lastTarget;

	// Token: 0x04000FB9 RID: 4025
	public float SpriteRotation;

	// Token: 0x04000FBA RID: 4026
	private Vector3 m_chargeDashDirection;

	// Token: 0x04000FBB RID: 4027
	private bool m_chargeDashAtTarget;

	// Token: 0x04000FBC RID: 4028
	private Vector3 m_chargeDashAtTargetPosition;

	// Token: 0x0200031C RID: 796
	public enum State
	{
		// Token: 0x04000FC2 RID: 4034
		Normal,
		// Token: 0x04000FC3 RID: 4035
		Dashing,
		// Token: 0x04000FC4 RID: 4036
		ChargeDashing
	}
}
