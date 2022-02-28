using System;
using Game;
using UnityEngine;

// Token: 0x02000308 RID: 776
public class PlayerAbilities : SaveSerialize, ISeinReceiver
{
	// Token: 0x1700026D RID: 621
	// (get) Token: 0x06000F7D RID: 3965 RVA: 0x0000D945 File Offset: 0x0000BB45
	// (set) Token: 0x06000F7E RID: 3966 RVA: 0x0000D94D File Offset: 0x0000BB4D
	public CharacterAbility[] Abilities
	{
		get;
		private set;
	}

	// Token: 0x1700026E RID: 622
	// (get) Token: 0x06000F7F RID: 3967 RVA: 0x0000D956 File Offset: 0x0000BB56
	public int OriStrength
	{
		get
		{
			if (this.UltraSplitFlame.HasAbility)
			{
				return 3;
			}
			if (this.CinderFlame.HasAbility)
			{
				return 2;
			}
			if (this.SparkFlame.HasAbility)
			{
				return 1;
			}
			return 0;
		}
	}

	// Token: 0x1700026F RID: 623
	// (get) Token: 0x06000F80 RID: 3968 RVA: 0x0000D986 File Offset: 0x0000BB86
	public int SplitFlameTargets
	{
		get
		{
			if (this.UltraSplitFlame.HasAbility)
			{
				return 4 + RandomizerBonus.SpiritFlameLevel();
			}
			if (this.SplitFlameUpgrade.HasAbility)
			{
				return 2 + RandomizerBonus.SpiritFlameLevel();
			}
			return 1 + RandomizerBonus.SpiritFlameLevel();
		}
	}

	// Token: 0x17000270 RID: 624
	// (get) Token: 0x06000F81 RID: 3969
	public float AttractionDistance
	{
		get
		{
			if (Characters.Sein.PlayerAbilities.UltraMagnet.HasAbility)
			{
				return 200f;
			}
			if (Characters.Sein.PlayerAbilities.Magnet.HasAbility)
			{
				return 8f;
			}
			return 0f;
		}
	}

	// Token: 0x06000F82 RID: 3970 RVA: 0x0005DCF4 File Offset: 0x0005BEF4
	public new void Awake()
	{
		base.Awake();
		this.Abilities = new CharacterAbility[]
		{
			this.Bash,
			this.ChargeFlame,
			this.WallJump,
			this.Stomp,
			this.DoubleJump,
			this.ChargeJump,
			this.Magnet,
			this.UltraMagnet,
			this.Climb,
			this.Glide,
			this.SpiritFlame,
			this.RapidFire,
			this.SoulEfficiency,
			this.WaterBreath,
			this.ChargeFlameBlast,
			this.ChargeFlameBurn,
			this.DoubleJumpUpgrade,
			this.BashBuff,
			this.UltraDefense,
			this.HealthEfficiency,
			this.Sense,
			this.StompUpgrade,
			this.QuickFlame,
			this.MapMarkers,
			this.EnergyEfficiency,
			this.HealthMarkers,
			this.EnergyMarkers,
			this.AbilityMarkers,
			this.Rekindle,
			this.Regroup,
			this.ChargeFlameEfficiency,
			this.UltraSoulFlame,
			this.SoulFlameEfficiency,
			this.SplitFlameUpgrade,
			this.SparkFlame,
			this.CinderFlame,
			this.UltraSplitFlame,
			this.Dash,
			this.Grenade,
			this.GrenadeUpgrade,
			this.ChargeDash,
			this.AirDash,
			this.GrenadeEfficiency
		};
	}

	// Token: 0x06000F83 RID: 3971 RVA: 0x0005DEBC File Offset: 0x0005C0BC
	public void SetAllAbilitys(bool abilityEnabled)
	{
		CharacterAbility[] abilities = this.Abilities;
		for (int i = 0; i < abilities.Length; i++)
		{
			CharacterAbility characterAbility = abilities[i];
			characterAbility.HasAbility = abilityEnabled;
		}
		this.m_sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	// Token: 0x06000F84 RID: 3972 RVA: 0x0005DF00 File Offset: 0x0005C100
	public override void Serialize(Archive ar)
	{
		try
		{
			CharacterAbility[] abilities = this.Abilities;
			for (int i = 0; i < abilities.Length; i++)
			{
				CharacterAbility characterAbility = abilities[i];
				ar.Serialize(ref characterAbility.HasAbility);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		if (ar.Reading)
		{
			this.m_sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
		}
	}

	// Token: 0x06000F85 RID: 3973 RVA: 0x0005DF74 File Offset: 0x0005C174
	public void SetAbility(AbilityType ability, bool value)
	{
		switch (ability)
		{
		case AbilityType.Bash:
			if (Randomizer.GiveAbility)
			{
				this.Bash.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(4);
			}
			break;
		case AbilityType.ChargeFlame:
			if (Randomizer.GiveAbility)
			{
				this.ChargeFlame.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(2);
			}
			break;
		case AbilityType.WallJump:
			if (Randomizer.GiveAbility)
			{
				this.WallJump.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(1);
			}
			break;
		case AbilityType.Stomp:
			if (Randomizer.GiveAbility)
			{
				this.Stomp.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(5);
			}
			break;
		case AbilityType.DoubleJump:
			if (Randomizer.GiveAbility)
			{
				this.DoubleJump.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(3);
			}
			break;
		case AbilityType.ChargeJump:
			if (Randomizer.GiveAbility)
			{
				this.ChargeJump.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(8);
			}
			break;
		case AbilityType.Magnet:
			this.Magnet.HasAbility = value;
			break;
		case AbilityType.UltraMagnet:
			this.UltraMagnet.HasAbility = value;
			break;
		case AbilityType.Climb:
			if (Randomizer.GiveAbility)
			{
				this.Climb.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(7);
			}
			break;
		case AbilityType.Glide:
			if (Randomizer.GiveAbility)
			{
				this.Glide.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(6);
			}
			break;
		case AbilityType.SpiritFlame:
			RandomizerTrackedDataManager.SetTree(0);
			this.SpiritFlame.HasAbility = value;
			Characters.Ori.MoveOriToPlayer();
			break;
		case AbilityType.RapidFlame:
			this.RapidFire.HasAbility = value;
			break;
		case AbilityType.SplitFlameUpgrade:
			this.SplitFlameUpgrade.HasAbility = value;
			break;
		case AbilityType.SoulEfficiency:
			this.SoulEfficiency.HasAbility = value;
			break;
		case AbilityType.WaterBreath:
			this.WaterBreath.HasAbility = value;
			break;
		case AbilityType.ChargeFlameBlast:
			this.ChargeFlameBlast.HasAbility = value;
			break;
		case AbilityType.ChargeFlameBurn:
			this.ChargeFlameBurn.HasAbility = value;
			break;
		case AbilityType.DoubleJumpUpgrade:
			this.DoubleJumpUpgrade.HasAbility = value;
			break;
		case AbilityType.BashBuff:
			this.BashBuff.HasAbility = value;
			break;
		case AbilityType.UltraDefense:
			this.UltraDefense.HasAbility = value;
			break;
		case AbilityType.HealthEfficiency:
			this.HealthEfficiency.HasAbility = value;
			break;
		case AbilityType.Sense:
			this.Sense.HasAbility = value;
			break;
		case AbilityType.UltraStomp:
			this.StompUpgrade.HasAbility = value;
			break;
		case AbilityType.SparkFlame:
			this.SparkFlame.HasAbility = value;
			break;
		case AbilityType.QuickFlame:
			this.QuickFlame.HasAbility = value;
			break;
		case AbilityType.MapMarkers:
			this.MapMarkers.HasAbility = value;
			break;
		case AbilityType.EnergyEfficiency:
			this.EnergyEfficiency.HasAbility = value;
			break;
		case AbilityType.HealthMarkers:
			this.HealthMarkers.HasAbility = value;
			break;
		case AbilityType.EnergyMarkers:
			this.EnergyMarkers.HasAbility = value;
			break;
		case AbilityType.AbilityMarkers:
			this.AbilityMarkers.HasAbility = value;
			break;
		case AbilityType.Rekindle:
			this.Rekindle.HasAbility = value;
			break;
		case AbilityType.Regroup:
			this.Regroup.HasAbility = value;
			break;
		case AbilityType.ChargeFlameEfficiency:
			this.ChargeFlameEfficiency.HasAbility = value;
			break;
		case AbilityType.UltraSoulFlame:
			this.UltraSoulFlame.HasAbility = value;
			break;
		case AbilityType.SoulFlameEfficiency:
			this.SoulFlameEfficiency.HasAbility = value;
			break;
		case AbilityType.CinderFlame:
			this.CinderFlame.HasAbility = value;
			break;
		case AbilityType.UltraSplitFlame:
			this.UltraSplitFlame.HasAbility = value;
			break;
		case AbilityType.Dash:
			if (Randomizer.GiveAbility)
			{
				this.Dash.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(10);
			}
			break;
		case AbilityType.Grenade:
			if (Randomizer.GiveAbility)
			{
				this.Grenade.HasAbility = value;
			}
			else
			{
				Randomizer.getSkill(9);
			}
			break;
		case AbilityType.GrenadeUpgrade:
			this.GrenadeUpgrade.HasAbility = value;
			break;
		case AbilityType.ChargeDash:
			this.ChargeDash.HasAbility = value;
			break;
		case AbilityType.AirDash:
			this.AirDash.HasAbility = value;
			break;
		case AbilityType.GrenadeEfficiency:
			this.GrenadeEfficiency.HasAbility = value;
			break;
		}
		this.m_sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	// Token: 0x06000F86 RID: 3974 RVA: 0x0005E3EC File Offset: 0x0005C5EC
	public bool HasAbility(AbilityType ability)
	{
		switch (ability)
		{
		case AbilityType.Bash:
			return this.Bash.HasAbility;
		case AbilityType.ChargeFlame:
			return this.ChargeFlame.HasAbility;
		case AbilityType.WallJump:
			return this.WallJump.HasAbility;
		case AbilityType.Stomp:
			return this.Stomp.HasAbility;
		case AbilityType.DoubleJump:
			return this.DoubleJump.HasAbility;
		case AbilityType.ChargeJump:
			return this.ChargeJump.HasAbility;
		case AbilityType.Magnet:
			return this.Magnet.HasAbility;
		case AbilityType.UltraMagnet:
			return this.UltraMagnet.HasAbility;
		case AbilityType.Climb:
			return this.Climb.HasAbility;
		case AbilityType.Glide:
			return this.Glide.HasAbility;
		case AbilityType.SpiritFlame:
			return this.SpiritFlame.HasAbility;
		case AbilityType.RapidFlame:
			return this.RapidFire.HasAbility;
		case AbilityType.SplitFlameUpgrade:
			return this.SplitFlameUpgrade.HasAbility;
		case AbilityType.SoulEfficiency:
			return this.SoulEfficiency.HasAbility;
		case AbilityType.WaterBreath:
			return this.WaterBreath.HasAbility;
		case AbilityType.ChargeFlameBlast:
			return this.ChargeFlameBlast.HasAbility;
		case AbilityType.ChargeFlameBurn:
			return this.ChargeFlameBurn.HasAbility;
		case AbilityType.DoubleJumpUpgrade:
			return this.DoubleJumpUpgrade.HasAbility;
		case AbilityType.BashBuff:
			return this.BashBuff.HasAbility;
		case AbilityType.UltraDefense:
			return this.UltraDefense.HasAbility;
		case AbilityType.HealthEfficiency:
			return this.HealthEfficiency.HasAbility;
		case AbilityType.Sense:
			return this.Sense.HasAbility;
		case AbilityType.UltraStomp:
			return this.StompUpgrade.HasAbility;
		case AbilityType.SparkFlame:
			return this.SparkFlame.HasAbility;
		case AbilityType.QuickFlame:
			return this.QuickFlame.HasAbility;
		case AbilityType.MapMarkers:
			return this.MapMarkers.HasAbility;
		case AbilityType.EnergyEfficiency:
			return this.EnergyEfficiency.HasAbility;
		case AbilityType.HealthMarkers:
			return this.HealthMarkers.HasAbility;
		case AbilityType.EnergyMarkers:
			return this.EnergyMarkers.HasAbility;
		case AbilityType.AbilityMarkers:
			return this.AbilityMarkers.HasAbility;
		case AbilityType.Rekindle:
			return this.Rekindle.HasAbility;
		case AbilityType.Regroup:
			return this.Regroup.HasAbility;
		case AbilityType.ChargeFlameEfficiency:
			return this.ChargeFlameEfficiency.HasAbility;
		case AbilityType.UltraSoulFlame:
			return this.UltraSoulFlame.HasAbility;
		case AbilityType.SoulFlameEfficiency:
			return this.SoulFlameEfficiency.HasAbility;
		case AbilityType.CinderFlame:
			return this.CinderFlame.HasAbility;
		case AbilityType.UltraSplitFlame:
			return this.UltraSplitFlame.HasAbility;
		case AbilityType.Dash:
			return this.Dash.HasAbility;
		case AbilityType.Grenade:
			return this.Grenade.HasAbility;
		case AbilityType.GrenadeUpgrade:
			return this.GrenadeUpgrade.HasAbility;
		case AbilityType.ChargeDash:
			return this.ChargeDash.HasAbility;
		case AbilityType.AirDash:
			return this.AirDash.HasAbility;
		case AbilityType.GrenadeEfficiency:
			return this.GrenadeEfficiency.HasAbility;
		}
		return false;
	}

	// Token: 0x06000F87 RID: 3975 RVA: 0x0000D9B9 File Offset: 0x0000BBB9
	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.PlayerAbilities = this;
	}

	// Token: 0x04000EB9 RID: 3769
	public CharacterAbility Bash;

	// Token: 0x04000EBA RID: 3770
	public CharacterAbility ChargeFlame;

	// Token: 0x04000EBB RID: 3771
	public CharacterAbility WallJump;

	// Token: 0x04000EBC RID: 3772
	public CharacterAbility Stomp;

	// Token: 0x04000EBD RID: 3773
	public CharacterAbility DoubleJump;

	// Token: 0x04000EBE RID: 3774
	public CharacterAbility ChargeJump;

	// Token: 0x04000EBF RID: 3775
	public CharacterAbility Magnet;

	// Token: 0x04000EC0 RID: 3776
	public CharacterAbility UltraMagnet;

	// Token: 0x04000EC1 RID: 3777
	public CharacterAbility Climb;

	// Token: 0x04000EC2 RID: 3778
	public CharacterAbility Glide;

	// Token: 0x04000EC3 RID: 3779
	public CharacterAbility SpiritFlame;

	// Token: 0x04000EC4 RID: 3780
	public CharacterAbility RapidFire;

	// Token: 0x04000EC5 RID: 3781
	public CharacterAbility SoulEfficiency;

	// Token: 0x04000EC6 RID: 3782
	public CharacterAbility WaterBreath;

	// Token: 0x04000EC7 RID: 3783
	public CharacterAbility ChargeFlameBlast;

	// Token: 0x04000EC8 RID: 3784
	public CharacterAbility ChargeFlameBurn;

	// Token: 0x04000EC9 RID: 3785
	public CharacterAbility DoubleJumpUpgrade;

	// Token: 0x04000ECA RID: 3786
	public CharacterAbility BashBuff;

	// Token: 0x04000ECB RID: 3787
	public CharacterAbility UltraDefense;

	// Token: 0x04000ECC RID: 3788
	public CharacterAbility HealthEfficiency;

	// Token: 0x04000ECD RID: 3789
	public CharacterAbility Sense;

	// Token: 0x04000ECE RID: 3790
	public CharacterAbility StompUpgrade;

	// Token: 0x04000ECF RID: 3791
	public CharacterAbility QuickFlame;

	// Token: 0x04000ED0 RID: 3792
	public CharacterAbility MapMarkers;

	// Token: 0x04000ED1 RID: 3793
	public CharacterAbility EnergyEfficiency;

	// Token: 0x04000ED2 RID: 3794
	public CharacterAbility HealthMarkers;

	// Token: 0x04000ED3 RID: 3795
	public CharacterAbility EnergyMarkers;

	// Token: 0x04000ED4 RID: 3796
	public CharacterAbility AbilityMarkers;

	// Token: 0x04000ED5 RID: 3797
	public CharacterAbility Rekindle;

	// Token: 0x04000ED6 RID: 3798
	public CharacterAbility Regroup;

	// Token: 0x04000ED7 RID: 3799
	public CharacterAbility ChargeFlameEfficiency;

	// Token: 0x04000ED8 RID: 3800
	public CharacterAbility UltraSoulFlame;

	// Token: 0x04000ED9 RID: 3801
	public CharacterAbility SoulFlameEfficiency;

	// Token: 0x04000EDA RID: 3802
	public CharacterAbility SplitFlameUpgrade;

	// Token: 0x04000EDB RID: 3803
	public CharacterAbility SparkFlame;

	// Token: 0x04000EDC RID: 3804
	public CharacterAbility CinderFlame;

	// Token: 0x04000EDD RID: 3805
	public CharacterAbility UltraSplitFlame;

	// Token: 0x04000EDE RID: 3806
	public CharacterAbility Grenade;

	// Token: 0x04000EDF RID: 3807
	public CharacterAbility Dash;

	// Token: 0x04000EE0 RID: 3808
	public CharacterAbility GrenadeUpgrade;

	// Token: 0x04000EE1 RID: 3809
	public CharacterAbility ChargeDash;

	// Token: 0x04000EE2 RID: 3810
	public CharacterAbility AirDash;

	// Token: 0x04000EE3 RID: 3811
	public CharacterAbility GrenadeEfficiency;

	// Token: 0x04000EE4 RID: 3812
	public ActionMethod GainAbilityAction;

	// Token: 0x04000EE5 RID: 3813
	private SeinCharacter m_sein;
}
