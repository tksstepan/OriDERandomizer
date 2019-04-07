using System;
using Game;
using UnityEngine;

// Token: 0x02000363 RID: 867
public class SeinLogicCycle : MonoBehaviour
{
	// Token: 0x0600132B RID: 4907
	public void Start()
	{
		this.Sein = Characters.Sein;
	}

	// Token: 0x1700035B RID: 859
	// (get) Token: 0x0600132C RID: 4908
	public SeinMortality Mortality
	{
		get
		{
			return this.Sein.Mortality;
		}
	}

	// Token: 0x1700035C RID: 860
	// (get) Token: 0x0600132D RID: 4909
	public SeinAbilities Abilities
	{
		get
		{
			return this.Sein.Abilities;
		}
	}

	// Token: 0x1700035D RID: 861
	// (get) Token: 0x0600132E RID: 4910
	public PlatformBehaviour PlatformBehaviour
	{
		get
		{
			return this.Sein.PlatformBehaviour;
		}
	}

	// Token: 0x0600132F RID: 4911
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

	// Token: 0x1700035E RID: 862
	// (get) Token: 0x06001330 RID: 4912
	public bool AllowInvincibility
	{
		get
		{
			return true;
		}
	}

	// Token: 0x1700035F RID: 863
	// (get) Token: 0x06001331 RID: 4913
	public bool AllowAirNoDeceleration
	{
		get
		{
			return true;
		}
	}

	// Token: 0x17000360 RID: 864
	// (get) Token: 0x06001332 RID: 4914
	public bool ApplyFrictionToSpeed
	{
		get
		{
			return true;
		}
	}

	// Token: 0x17000361 RID: 865
	// (get) Token: 0x06001333 RID: 4915
	public bool AllowSpiritFlameTargetting
	{
		get
		{
			return this.Sein.PlayerAbilities.SpiritFlame.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsBashing;
		}
	}

	// Token: 0x17000362 RID: 866
	// (get) Token: 0x06001334 RID: 4916
	public bool AllowCrushDetector
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x17000363 RID: 867
	// (get) Token: 0x06001335 RID: 4917
	public bool AllowSpriteRotater
	{
		get
		{
			return true;
		}
	}

	// Token: 0x17000364 RID: 868
	// (get) Token: 0x06001336 RID: 4918
	public bool AllowDamageReciever
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x17000365 RID: 869
	// (get) Token: 0x06001337 RID: 4919
	public bool AllowJumpSustain
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x17000366 RID: 870
	// (get) Token: 0x06001338 RID: 4920
	public bool AllowUpwardsDeceleration
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x17000367 RID: 871
	// (get) Token: 0x06001339 RID: 4921
	public bool AllowForceController
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x17000368 RID: 872
	// (get) Token: 0x0600133A RID: 4922
	public bool AllowGravity
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x17000369 RID: 873
	// (get) Token: 0x0600133B RID: 4923
	public bool AllowGravityToGround
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x1700036A RID: 874
	// (get) Token: 0x0600133C RID: 4924
	public bool AllowSwimming
	{
		get
		{
			return true;
		}
	}

	// Token: 0x1700036B RID: 875
	// (get) Token: 0x0600133D RID: 4925
	public bool AllowDash
	{
		get
		{
			return !RandomizerBonus.Swimming() && !this.Sein.Controller.IsGrabbingLever && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsPushPulling && !this.Sein.Controller.IsAimingGrenade && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsBashing && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.Dash) && this.Sein.Controller.CanMove;
		}
	}

	// Token: 0x1700036C RID: 876
	// (get) Token: 0x0600133E RID: 4926
	public bool AllowGrenade
	{
		get
		{
			return !RandomizerBonus.Swimming() && !this.Sein.Controller.IsGrabbingLever && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsPushPulling && !SeinAbilityRestrictZone.IsInside(SeinAbilityRestrictZoneMode.AllAbilities) && this.Sein.Controller.CanMove && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStandingOnEdge && !this.Sein.Controller.IsDashing;
		}
	}

	// Token: 0x1700036D RID: 877
	// (get) Token: 0x0600133F RID: 4927
	public bool AllowInstantStop
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x1700036E RID: 878
	// (get) Token: 0x06001340 RID: 4928
	public bool AllowLeftRightMovement
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && (!this.Sein.Controller.IsSwimming || !this.Sein.Abilities.Swimming.IsUnderwater);
		}
	}

	// Token: 0x1700036F RID: 879
	// (get) Token: 0x06001341 RID: 4929
	public bool AllowBash
	{
		get
		{
			return this.Sein.PlayerAbilities.Bash.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsPushPulling && !this.Sein.Controller.IsGrabbingLever && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	// Token: 0x17000370 RID: 880
	// (get) Token: 0x06001342 RID: 4930
	public bool AllowLooking
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	// Token: 0x17000371 RID: 881
	// (get) Token: 0x06001343 RID: 4931
	public bool AllowLever
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsPushPulling && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	// Token: 0x17000372 RID: 882
	// (get) Token: 0x06001344 RID: 4932
	public bool AllowFootsteps
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsSwimming;
		}
	}

	// Token: 0x17000373 RID: 883
	// (get) Token: 0x06001345 RID: 4933
	public bool AllowStandardSpiritFlame
	{
		get
		{
			return this.Sein.PlayerAbilities.SpiritFlame.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsBashing;
		}
	}

	// Token: 0x17000374 RID: 884
	// (get) Token: 0x06001346 RID: 4934
	public bool AllowChargeFlame
	{
		get
		{
			return this.Sein.PlayerAbilities.ChargeFlame.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsBashing;
		}
	}

	// Token: 0x17000375 RID: 885
	// (get) Token: 0x06001347 RID: 4935
	public bool AllowWallSlide
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsGliding && !this.Sein.Controller.IsStomping;
		}
	}

	// Token: 0x17000376 RID: 886
	// (get) Token: 0x06001348 RID: 4936
	public bool AllowStomp
	{
		get
		{
			return this.Sein.PlayerAbilities.Stomp.HasAbility && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsGrabbingWall && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	// Token: 0x17000377 RID: 887
	// (get) Token: 0x06001349 RID: 4937
	public bool AllowCarry
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	// Token: 0x17000378 RID: 888
	// (get) Token: 0x0600134A RID: 4938
	public bool AllowFall
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing;
		}
	}

	// Token: 0x17000379 RID: 889
	// (get) Token: 0x0600134B RID: 4939
	public bool AllowGrabBlock
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	// Token: 0x1700037A RID: 890
	// (get) Token: 0x0600134C RID: 4940
	public bool AllowIdle
	{
		get
		{
			return !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsPushPulling;
		}
	}

	// Token: 0x1700037B RID: 891
	// (get) Token: 0x0600134D RID: 4941
	public bool AllowRun
	{
		get
		{
			return !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsPushPulling;
		}
	}

	// Token: 0x1700037C RID: 892
	// (get) Token: 0x0600134E RID: 4942
	public bool AllowCrouching
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade && !this.Sein.Controller.IsDashing;
		}
	}

	// Token: 0x1700037D RID: 893
	// (get) Token: 0x0600134F RID: 4943
	public bool AllowWallGrabbing
	{
		get
		{
			return this.Sein.PlayerAbilities.Climb.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x1700037E RID: 894
	// (get) Token: 0x06001350 RID: 4944
	public bool AllowJumping
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x1700037F RID: 895
	// (get) Token: 0x06001351 RID: 4945
	public bool AllowDoubleJump
	{
		get
		{
			return this.Sein.PlayerAbilities.DoubleJump.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x17000380 RID: 896
	// (get) Token: 0x06001352 RID: 4946
	public bool AllowGliding
	{
		get
		{
			return this.Sein.PlayerAbilities.Glide.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsGrabbingWall && !this.Sein.Controller.IsBashing && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsDashing;
		}
	}

	// Token: 0x17000381 RID: 897
	// (get) Token: 0x06001353 RID: 4947
	public bool AllowWallJump
	{
		get
		{
			return this.Sein.PlayerAbilities.WallJump.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsGliding && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x17000382 RID: 898
	// (get) Token: 0x06001354 RID: 4948
	public bool AllowChargeJumpCharging
	{
		get
		{
			return this.AllowChargeJump || this.AllowDash;
		}
	}

	// Token: 0x17000383 RID: 899
	// (get) Token: 0x06001355 RID: 4949
	public bool AllowChargeJump
	{
		get
		{
			return this.Sein.PlayerAbilities.ChargeJump.HasAbility && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	// Token: 0x17000384 RID: 900
	// (get) Token: 0x06001356 RID: 4950
	public bool AllowWallChargeJump
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	// Token: 0x17000385 RID: 901
	// (get) Token: 0x06001357 RID: 4951
	public bool AllowStandingOnEdge
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsStomping && !this.Sein.Controller.IsPlayingAnimation && !this.Sein.Controller.IsAimingGrenade;
		}
	}

	// Token: 0x17000386 RID: 902
	// (get) Token: 0x06001358 RID: 4952
	public bool AllowPushAgainstWall
	{
		get
		{
			return !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x17000387 RID: 903
	// (get) Token: 0x06001359 RID: 4953
	public bool AllowEdgeClamber
	{
		get
		{
			return !this.Sein.Controller.IsCarrying && !this.Sein.Controller.IsSwimming && !this.Sein.Controller.IsPlayingAnimation;
		}
	}

	// Token: 0x04001212 RID: 4626
	public SeinCharacter Sein;
}
