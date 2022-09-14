using System;
using Game;
using UnityEngine;

public class PlayerAbilities : SaveSerialize, ISeinReceiver
{
	public CharacterAbility[] Abilities { get; private set; }

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

	public void SetAllAbilitys(bool abilityEnabled)
	{
		foreach (CharacterAbility characterAbility in this.Abilities)
		{
			characterAbility.HasAbility = abilityEnabled;
		}
		this.m_sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public override void Serialize(Archive ar)
	{
		try
		{
			foreach (CharacterAbility characterAbility in this.Abilities)
			{
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

	public void SetAbility(AbilityType ability, bool value)
	{
		switch (ability)
		{
		case AbilityType.Bash:
			this.Bash.HasAbility = value;
			break;
		case AbilityType.ChargeFlame:
			this.ChargeFlame.HasAbility = value;
			break;
		case AbilityType.WallJump:
			this.WallJump.HasAbility = value;
			break;
		case AbilityType.Stomp:
			this.Stomp.HasAbility = value;
			break;
		case AbilityType.DoubleJump:
			this.DoubleJump.HasAbility = value;
			break;
		case AbilityType.ChargeJump:
			this.ChargeJump.HasAbility = value;
			break;
		case AbilityType.Magnet:
			this.Magnet.HasAbility = value;
			break;
		case AbilityType.UltraMagnet:
			this.UltraMagnet.HasAbility = value;
			break;
		case AbilityType.Climb:
			this.Climb.HasAbility = value;
			break;
		case AbilityType.Glide:
			this.Glide.HasAbility = value;
			break;
		case AbilityType.SpiritFlame:
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
		    if(value) 
				foreach(var area in GameWorld.Instance.RuntimeAreas) 
					area.DiscoverAllAreas();
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
			this.Dash.HasAbility = value;
			break;
		case AbilityType.Grenade:
			this.Grenade.HasAbility = value;
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

	public void SetReferenceToSein(SeinCharacter sein)
	{
		this.m_sein = sein;
		this.m_sein.PlayerAbilities = this;
	}

	public CharacterAbility Bash;

	public CharacterAbility ChargeFlame;

	public CharacterAbility WallJump;

	public CharacterAbility Stomp;

	public CharacterAbility DoubleJump;

	public CharacterAbility ChargeJump;

	public CharacterAbility Magnet;

	public CharacterAbility UltraMagnet;

	public CharacterAbility Climb;

	public CharacterAbility Glide;

	public CharacterAbility SpiritFlame;

	public CharacterAbility RapidFire;

	public CharacterAbility SoulEfficiency;

	public CharacterAbility WaterBreath;

	public CharacterAbility ChargeFlameBlast;

	public CharacterAbility ChargeFlameBurn;

	public CharacterAbility DoubleJumpUpgrade;

	public CharacterAbility BashBuff;

	public CharacterAbility UltraDefense;

	public CharacterAbility HealthEfficiency;

	public CharacterAbility Sense;

	public CharacterAbility StompUpgrade;

	public CharacterAbility QuickFlame;

	public CharacterAbility MapMarkers;

	public CharacterAbility EnergyEfficiency;

	public CharacterAbility HealthMarkers;

	public CharacterAbility EnergyMarkers;

	public CharacterAbility AbilityMarkers;

	public CharacterAbility Rekindle;

	public CharacterAbility Regroup;

	public CharacterAbility ChargeFlameEfficiency;

	public CharacterAbility UltraSoulFlame;

	public CharacterAbility SoulFlameEfficiency;

	public CharacterAbility SplitFlameUpgrade;

	public CharacterAbility SparkFlame;

	public CharacterAbility CinderFlame;

	public CharacterAbility UltraSplitFlame;

	public CharacterAbility Grenade;

	public CharacterAbility Dash;

	public CharacterAbility GrenadeUpgrade;

	public CharacterAbility ChargeDash;

	public CharacterAbility AirDash;

	public CharacterAbility GrenadeEfficiency;

	public ActionMethod GainAbilityAction;

	private SeinCharacter m_sein;
}
