using System;
using Game;
using UnityEngine;

public class SeinLogicCycle : MonoBehaviour
{
	public void Start()
	{
		this.Sein = Characters.Sein;
	}

	public SeinMortality Mortality
	{
		get
		{
			return this.Sein.Mortality;
		}
	}

	public SeinAbilities Abilities
	{
		get
		{
			return this.Sein.Abilities;
		}
	}

	public PlatformBehaviour PlatformBehaviour
	{
		get
		{
			return this.Sein.PlatformBehaviour;
		}
	}

	public void FixedUpdate()
	{
		if (this.Sein.IsSuspended)
		{
			return;
		}
		SeinAbilities abilities = this.Abilities;
		this.PlatformBehaviour.Gravity.SetStateActive(this.AllowGravity);
		this.PlatformBehaviour.GravityToGround.SetStateActive(this.AllowGravityToGround);
		this.PlatformBehaviour.InstantStop.SetStateActive(this.AllowInstantStop);
		this.PlatformBehaviour.LeftRightMovement.SetStateActive(this.AllowLeftRightMovement);
		this.PlatformBehaviour.AirNoDeceleration.SetStateActive(this.AllowAirNoDeceleration);
		this.PlatformBehaviour.ApplyFrictionToSpeed.SetStateActive(this.ApplyFrictionToSpeed);
		abilities.StandardSpiritFlame.SetStateActive(this.AllowStandardSpiritFlame);
		abilities.Bash.SetStateActive(this.AllowBash);
		abilities.LookUp.SetStateActive(this.AllowLooking);
		abilities.Lever.SetStateActive(this.AllowLever);
		abilities.Footsteps.SetStateActive(this.AllowFootsteps);
		abilities.SpiritFlameTargetting.SetStateActive(this.AllowSpiritFlameTargetting);
		abilities.ChargeFlame.SetStateActive(this.AllowChargeFlame);
		abilities.WallSlide.SetStateActive(this.AllowWallSlide);
		abilities.Stomp.SetStateActive(this.AllowStomp);
		abilities.Carry.SetStateActive(this.AllowCarry);
		abilities.Fall.SetStateActive(this.AllowFall);
		abilities.GrabBlock.SetStateActive(this.AllowGrabBlock);
		abilities.Idle.SetStateActive(this.AllowIdle);
		abilities.Run.SetStateActive(this.AllowRun);
		abilities.Crouch.SetStateActive(this.AllowCrouching);
		abilities.GrabWall.SetStateActive(this.AllowWallGrabbing);
		abilities.Jump.SetStateActive(this.AllowJumping);
		abilities.DoubleJump.SetStateActive(this.AllowDoubleJump);
		abilities.Glide.SetStateActive(this.AllowGliding);
		abilities.WallJump.SetStateActive(this.AllowWallJump);
		abilities.ChargeJumpCharging.SetStateActive(this.AllowChargeJumpCharging);
		abilities.ChargeJump.SetStateActive(this.AllowChargeJump);
		abilities.WallChargeJump.SetStateActive(this.AllowWallChargeJump);
		abilities.StandingOnEdge.SetStateActive(this.AllowStandingOnEdge);
		abilities.PushAgainstWall.SetStateActive(this.AllowPushAgainstWall);
		abilities.EdgeClamber.SetStateActive(this.AllowEdgeClamber);
		this.Mortality.CrushDetector.SetStateActive(this.AllowCrushDetector);
		this.PlatformBehaviour.Visuals.SpriteRotater.SetStateActive(this.AllowSpriteRotater);
		this.Mortality.DamageReciever.SetStateActive(this.AllowDamageReciever);
		abilities.Invincibility.SetStateActive(this.AllowInvincibility);
		this.PlatformBehaviour.JumpSustain.SetStateActive(this.AllowJumpSustain);
		this.PlatformBehaviour.UpwardsDeceleration.SetStateActive(this.AllowUpwardsDeceleration);
		this.Sein.ForceController.SetStateActive(this.AllowForceController);
		abilities.Swimming.SetStateActive(this.AllowSwimming);
		abilities.Dash.SetStateActive(this.AllowDash);
		abilities.Grenade.SetStateActive(this.AllowGrenade);
		this.Sein.SoulFlame.SetStateActive(true);
		CharacterState.UpdateCharacterState(this.Mortality.CrushDetector);
		CharacterState.UpdateCharacterState(this.Mortality.DamageReciever);
		CharacterState.UpdateCharacterState(this.PlatformBehaviour.Gravity);
		CharacterState.UpdateCharacterState(this.PlatformBehaviour.GravityToGround);
		CharacterState.UpdateCharacterState(this.PlatformBehaviour.InstantStop);
		CharacterState.UpdateCharacterState(this.Abilities.Carry);
		CharacterState.UpdateCharacterState(this.Abilities.GrabBlock);
		CharacterState.UpdateCharacterState(this.Abilities.SpiritFlameTargetting);
		CharacterState.UpdateCharacterState(this.Abilities.SpiritFlame);
		CharacterState.UpdateCharacterState(this.Abilities.ChargeFlame);
		CharacterState.UpdateCharacterState(this.Abilities.StandardSpiritFlame);
		CharacterState.UpdateCharacterState(this.Abilities.IceSpiritFlame);
		CharacterState.UpdateCharacterState(this.Abilities.StandingOnEdge);
		CharacterState.UpdateCharacterState(this.Abilities.Glide);
		CharacterState.UpdateCharacterState(this.Abilities.Bash);
		CharacterState.UpdateCharacterState(this.Abilities.WallJump);
		CharacterState.UpdateCharacterState(this.Abilities.EdgeClamber);
		CharacterState.UpdateCharacterState(this.Abilities.DoubleJump);
		CharacterState.UpdateCharacterState(this.Abilities.ChargeJumpCharging);
		CharacterState.UpdateCharacterState(this.Abilities.ChargeJump);
		CharacterState.UpdateCharacterState(this.Abilities.WallChargeJump);
		CharacterState.UpdateCharacterState(this.Abilities.Jump);
		CharacterState.UpdateCharacterState(this.Abilities.Fall);
		CharacterState.UpdateCharacterState(this.Abilities.PushAgainstWall);
		CharacterState.UpdateCharacterState(this.PlatformBehaviour.AirNoDeceleration);
		CharacterState.UpdateCharacterState(this.PlatformBehaviour.ApplyFrictionToSpeed);
		CharacterState.UpdateCharacterState(this.Abilities.Crouch);
		CharacterState.UpdateCharacterState(this.Abilities.Invincibility);
		CharacterState.UpdateCharacterState(this.Abilities.Run);
		CharacterState.UpdateCharacterState(this.Abilities.Idle);
		CharacterState.UpdateCharacterState(this.Abilities.LookUp);
		CharacterState.UpdateCharacterState(this.Abilities.GrabWall);
		CharacterState.UpdateCharacterState(this.Abilities.Footsteps);
		CharacterState.UpdateCharacterState(this.Sein.Abilities.Lever);
		CharacterState.UpdateCharacterState(this.PlatformBehaviour.JumpSustain);
		CharacterState.UpdateCharacterState(this.PlatformBehaviour.UpwardsDeceleration);
		CharacterState.UpdateCharacterState(this.Sein.ForceController);
		CharacterState.UpdateCharacterState(this.Abilities.WallSlide);
		CharacterState.UpdateCharacterState(this.Abilities.Stomp);
		CharacterState.UpdateCharacterState(this.Abilities.Swimming);
		CharacterState.UpdateCharacterState(this.PlatformBehaviour.Visuals.SpriteRotater);
		CharacterState.UpdateCharacterState(this.Sein.SoulFlame);
		CharacterState.UpdateCharacterState(this.Abilities.Dash);
		CharacterState.UpdateCharacterState(this.Abilities.Grenade);
		this.Sein.Controller.HandleOffscreenIssue();
	}

	public bool AllowInvincibility
	{
		get
		{
			return true;
		}
	}

	public bool AllowAirNoDeceleration
	{
		get
		{
			return true;
		}
	}

	public bool ApplyFrictionToSpeed
	{
		get
		{
			return true;
		}
	}

	public bool AllowSpiritFlameTargetting
	{
		get
		{
			return this.Sein.PlayerAbilities.SpiritFlame.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsBashing;
		}
	}

	public bool AllowCrushDetector
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowSpriteRotater
	{
		get
		{
			return true;
		}
	}

	public bool AllowDamageReciever
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowJumpSustain
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowUpwardsDeceleration
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowForceController
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowGravity
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowGravityToGround
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowSwimming
	{
		get
		{
			return true;
		}
	}

	public bool AllowDash
	{
		get
		{
			return !RandomizerBonus.Swimming() && !this.Sein.Controller.IsGrabbingLever && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsPushPulling && !this.Sein.Controller.IsAimingGrenade && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsBashing && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.Dash) && this.Sein.Controller.CanMove;
		}
	}

	public bool AllowGrenade
	{
		get
		{
			return !RandomizerBonus.Swimming() && !this.Sein.Controller.IsGrabbingLever && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsPushPulling && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && this.Sein.Controller.CanMove && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStandingOnEdge && !this.Sein.Controller.IsDashing;
		}
	}

	public bool AllowInstantStop
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowLeftRightMovement
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && (!this.Sein.Controller.IsSwimming || !this.Sein.Abilities.Swimming.IsUnderwater);
		}
	}

	public bool AllowBash
	{
		get
		{
			return this.Sein.PlayerAbilities.Bash.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsPushPulling && !this.Sein.Controller.IsGrabbingLever && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	public bool AllowLooking
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	public bool AllowLever
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsPushPulling && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	public bool AllowFootsteps
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsSwimming;
		}
	}

	public bool AllowStandardSpiritFlame
	{
		get
		{
			return this.Sein.PlayerAbilities.SpiritFlame.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsBashing;
		}
	}

	public bool AllowChargeFlame
	{
		get
		{
			return this.Sein.PlayerAbilities.ChargeFlame.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsBashing;
		}
	}

	public bool AllowWallSlide
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsGliding && !this.Sein.Controller.IsStomping;
		}
	}

	public bool AllowStomp
	{
		get
		{
			return this.Sein.PlayerAbilities.Stomp.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsGrabbingWall && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	public bool AllowCarry
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	public bool AllowFall
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing;
		}
	}

	public bool AllowGrabBlock
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	public bool AllowIdle
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsPushPulling;
		}
	}

	public bool AllowRun
	{
		get
		{
			return !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsPushPulling;
		}
	}

	public bool AllowCrouching
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade && !this.Sein.Controller.IsDashing;
		}
	}

	public bool AllowWallGrabbing
	{
		get
		{
			return this.Sein.PlayerAbilities.Climb.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowJumping
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowDoubleJump
	{
		get
		{
			return this.Sein.PlayerAbilities.DoubleJump.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowGliding
	{
		get
		{
			return this.Sein.PlayerAbilities.Glide.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsGrabbingWall && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsDashing;
		}
	}

	public bool AllowWallJump
	{
		get
		{
			return this.Sein.PlayerAbilities.WallJump.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsGliding && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowChargeJumpCharging
	{
		get
		{
			return this.AllowChargeJump || this.AllowDash;
		}
	}

	public bool AllowChargeJump
	{
		get
		{
			return this.Sein.PlayerAbilities.ChargeJump.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	public bool AllowWallChargeJump
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	public bool AllowStandingOnEdge
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	public bool AllowPushAgainstWall
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public bool AllowEdgeClamber
	{
		get
		{
			return !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	public SeinCharacter Sein;
}
