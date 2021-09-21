using System;
using Game;

public static class AbilityDebugMenuItems
{
	public static void SetAllAbilities(bool enabled)
	{
		Characters.Sein.PlayerAbilities.SetAllAbilitys(enabled);
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static void ToggleAbilities()
	{
		AbilityDebugMenuItems.SetAllAbilities(!AbilityDebugMenuItems.AllAbilitiesGetter());
	}

	public static void AllAbilitiesSetter(bool newValue)
	{
		AbilityDebugMenuItems.ToggleAbilities();
	}

	public static bool AllAbilitiesGetter()
	{
		return Characters.Sein.PlayerAbilities.Bash.HasAbility;
	}

	public static void SpiritFlameSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.SpiritFlame.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
		Characters.Ori.gameObject.SetActive(newValue);
	}

	public static bool SpiritFlameGetter()
	{
		return Characters.Sein.PlayerAbilities.SpiritFlame.HasAbility;
	}

	public static void ChargeFlameSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.ChargeFlame.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool ChargeFlameGetter()
	{
		return Characters.Sein.PlayerAbilities.ChargeFlame.HasAbility;
	}

	public static void ClimbSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Climb.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool ClimbGetter()
	{
		return !(Characters.Sein == null) && Characters.Sein.PlayerAbilities.Climb.HasAbility;
	}

	public static void ChargeJumpSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.ChargeJump.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool ChargeJumpGetter()
	{
		return !(Characters.Sein == null) && Characters.Sein.PlayerAbilities.ChargeJump.HasAbility;
	}

	public static void StompSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Stomp.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool StompGetter()
	{
		return !(Characters.Sein == null) && Characters.Sein.PlayerAbilities.Stomp.HasAbility;
	}

	public static void WallJumpSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.WallJump.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool WallJumpGetter()
	{
		return !(Characters.Sein == null) && Characters.Sein.PlayerAbilities.WallJump.HasAbility;
	}

	public static void DoubleJumpSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.DoubleJump.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool DoubleJumpGetter()
	{
		return !(Characters.Sein == null) && Characters.Sein.PlayerAbilities.DoubleJump.HasAbility;
	}

	public static bool BashGetter()
	{
		return !(Characters.Sein == null) && Characters.Sein.PlayerAbilities.Bash.HasAbility;
	}

	public static void BashSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Bash.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool GlideGetter()
	{
		return Characters.Sein.Abilities.Glide != null;
	}

	public static void GlideSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Glide.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static void DashSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Dash.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool DashGetter()
	{
		return Characters.Sein.PlayerAbilities.Dash.HasAbility;
	}

	public static void GrenadeSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Grenade.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool GrenadeGetter()
	{
		return Characters.Sein.PlayerAbilities.Grenade.HasAbility;
	}

	public static bool WaterBreathGetter()
	{
		return Characters.Sein.PlayerAbilities.WaterBreath.HasAbility;
	}

	public static void WaterBreathSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.WaterBreath.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool MagnetGetter()
	{
		return Characters.Sein.PlayerAbilities.Magnet.HasAbility;
	}

	public static void MagnetSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Magnet.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool UltraMagnetGetter()
	{
		return Characters.Sein.PlayerAbilities.UltraMagnet.HasAbility;
	}

	public static void UltraMagnetSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.UltraMagnet.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool RapidFireGetter()
	{
		return Characters.Sein.PlayerAbilities.RapidFire.HasAbility;
	}

	public static void RapidFireSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.RapidFire.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool SoulEfficiencyGetter()
	{
		return Characters.Sein.PlayerAbilities.SoulEfficiency.HasAbility;
	}

	public static void SoulEfficiencySetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.SoulEfficiency.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool ChargeFlameBlastGetter()
	{
		return Characters.Sein.PlayerAbilities.ChargeFlameBlast.HasAbility;
	}

	public static void ChargeFlameBlastSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.ChargeFlameBlast.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool DoubleJumpUpgradeGetter()
	{
		return Characters.Sein.PlayerAbilities.DoubleJumpUpgrade.HasAbility;
	}

	public static void DoubleJumpUpgradeSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.DoubleJumpUpgrade.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool BashUpgradeGetter()
	{
		return Characters.Sein.PlayerAbilities.BashBuff.HasAbility;
	}

	public static void BashUpgradeSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.BashBuff.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool UltraDefenseGetter()
	{
		return Characters.Sein.PlayerAbilities.UltraDefense.HasAbility;
	}

	public static void UltraDefenseSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.UltraDefense.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool HealthEfficiencyGetter()
	{
		return Characters.Sein.PlayerAbilities.HealthEfficiency.HasAbility;
	}

	public static void HealthEfficiencySetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.HealthEfficiency.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool SenseGetter()
	{
		return Characters.Sein.PlayerAbilities.Sense.HasAbility;
	}

	public static void SenseSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Sense.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool StompUpgradeGetter()
	{
		return Characters.Sein.PlayerAbilities.StompUpgrade.HasAbility;
	}

	public static void StompUpgradeSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.StompUpgrade.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool QuickFlameGetter()
	{
		return Characters.Sein.PlayerAbilities.QuickFlame.HasAbility;
	}

	public static void QuickFlameSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.QuickFlame.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool SparkFlameGetter()
	{
		return Characters.Sein.PlayerAbilities.SparkFlame.HasAbility;
	}

	public static void SparkFlameSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.SparkFlame.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool SplitFlameUpgradeGetter()
	{
		return Characters.Sein.PlayerAbilities.SplitFlameUpgrade.HasAbility;
	}

	public static void SplitFlameUpgradeSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.SplitFlameUpgrade.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool CinderFlameGetter()
	{
		return Characters.Sein.PlayerAbilities.CinderFlame.HasAbility;
	}

	public static void CinderFlameSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.CinderFlame.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool UltraSplitFlameGetter()
	{
		return Characters.Sein.PlayerAbilities.UltraSplitFlame.HasAbility;
	}

	public static void UltraSplitFlameSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.UltraSplitFlame.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool GrenadeUpgradeGetter()
	{
		return Characters.Sein.PlayerAbilities.GrenadeUpgrade.HasAbility;
	}

	public static void GrenadeUpgradeSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.GrenadeUpgrade.HasAbility = newValue;
	}

	public static bool ChargeDashGetter()
	{
		return Characters.Sein.PlayerAbilities.ChargeDash.HasAbility;
	}

	public static void ChargeDashSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.ChargeDash.HasAbility = newValue;
	}

	public static bool AirDashGetter()
	{
		return Characters.Sein.PlayerAbilities.AirDash.HasAbility;
	}

	public static void AirDashSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.AirDash.HasAbility = newValue;
	}

	public static bool GrenadeEfficiencyGetter()
	{
		return Characters.Sein.PlayerAbilities.GrenadeEfficiency.HasAbility;
	}

	public static void GrenadeEfficiencySetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.GrenadeEfficiency.HasAbility = newValue;
	}

	public static bool MapMarkersGetter()
	{
		return Characters.Sein.PlayerAbilities.MapMarkers.HasAbility;
	}

	public static void MapMarkersSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.MapMarkers.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool EnergyEfficiencyGetter()
	{
		return Characters.Sein.PlayerAbilities.EnergyEfficiency.HasAbility;
	}

	public static void EnergyEfficiencySetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.EnergyEfficiency.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool HealthMarkersGetter()
	{
		return Characters.Sein.PlayerAbilities.HealthMarkers.HasAbility;
	}

	public static void HealthMarkersSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.HealthMarkers.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool EnergyMarkersGetter()
	{
		return Characters.Sein.PlayerAbilities.EnergyMarkers.HasAbility;
	}

	public static void EnergyMarkersSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.EnergyMarkers.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool AbilityMarkersGetter()
	{
		return Characters.Sein.PlayerAbilities.AbilityMarkers.HasAbility;
	}

	public static void AbilityMarkersSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.AbilityMarkers.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool RekindleGetter()
	{
		return Characters.Sein.PlayerAbilities.Rekindle.HasAbility;
	}

	public static void RekindleSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Rekindle.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool RegroupGetter()
	{
		return Characters.Sein.PlayerAbilities.Regroup.HasAbility;
	}

	public static void RegroupSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.Regroup.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool ChargeFlameEfficiencyGetter()
	{
		return Characters.Sein.PlayerAbilities.ChargeFlameEfficiency.HasAbility;
	}

	public static void ChargeFlameEfficiencySetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.ChargeFlameEfficiency.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool SoulFlameEfficiencyGetter()
	{
		return Characters.Sein.PlayerAbilities.SoulFlameEfficiency.HasAbility;
	}

	public static void SoulFlameEfficiencySetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.ChargeFlameEfficiency.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool UltraSoulFlameGetter()
	{
		return Characters.Sein.PlayerAbilities.UltraSoulFlame.HasAbility;
	}

	public static void UltraSoulFlameSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.UltraSoulFlame.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool ChargeFlameBurnGetter()
	{
		return Characters.Sein.PlayerAbilities.ChargeFlameBurn.HasAbility;
	}

	public static void ChargeFlameBurnSetter(bool newValue)
	{
		Characters.Sein.PlayerAbilities.ChargeFlameBurn.HasAbility = newValue;
		Characters.Sein.Prefabs.EnsureRightPrefabsAreThereForAbilities();
	}

	public static bool RemoveAllSkillsAndAbilities()
	{
		AbilityDebugMenuItems.RemoveAllSkills();
		AbilityDebugMenuItems.RemoveAllAbilities();
		return true;
	}

	public static bool RemoveAllSkills()
	{
		AbilityDebugMenuItems.SpiritFlameSetter(false);
		AbilityDebugMenuItems.WallJumpSetter(false);
		AbilityDebugMenuItems.ChargeFlameSetter(false);
		AbilityDebugMenuItems.DoubleJumpSetter(false);
		AbilityDebugMenuItems.BashSetter(false);
		AbilityDebugMenuItems.StompSetter(false);
		AbilityDebugMenuItems.GlideSetter(false);
		AbilityDebugMenuItems.ClimbSetter(false);
		AbilityDebugMenuItems.ChargeJumpSetter(false);
		AbilityDebugMenuItems.DashSetter(false);
		AbilityDebugMenuItems.GrenadeSetter(false);
		return true;
	}

	public static bool RemoveAllAbilities()
	{
		AbilityDebugMenuItems.RemoveAllBlueAbilities();
		AbilityDebugMenuItems.RemoveAllPurpleAbilities();
		AbilityDebugMenuItems.RemoveAllRedAbilities();
		return true;
	}

	public static bool RemoveAllBlueAbilities()
	{
		AbilityDebugMenuItems.RekindleSetter(false);
		AbilityDebugMenuItems.RegroupSetter(false);
		AbilityDebugMenuItems.ChargeFlameEfficiencySetter(false);
		AbilityDebugMenuItems.AirDashSetter(false);
		AbilityDebugMenuItems.UltraSoulFlameSetter(false);
		AbilityDebugMenuItems.ChargeDashSetter(false);
		AbilityDebugMenuItems.WaterBreathSetter(false);
		AbilityDebugMenuItems.SoulFlameEfficiencySetter(false);
		AbilityDebugMenuItems.DoubleJumpUpgradeSetter(false);
		AbilityDebugMenuItems.UltraDefenseSetter(false);
		return true;
	}

	public static bool RemoveAllPurpleAbilities()
	{
		AbilityDebugMenuItems.MagnetSetter(false);
		AbilityDebugMenuItems.MapMarkersSetter(false);
		AbilityDebugMenuItems.HealthEfficiencySetter(false);
		AbilityDebugMenuItems.UltraMagnetSetter(false);
		AbilityDebugMenuItems.EnergyEfficiencySetter(false);
		AbilityDebugMenuItems.AbilityMarkersSetter(false);
		AbilityDebugMenuItems.SoulEfficiencySetter(false);
		AbilityDebugMenuItems.HealthMarkersSetter(false);
		AbilityDebugMenuItems.EnergyMarkersSetter(false);
		AbilityDebugMenuItems.SenseSetter(false);
		return true;
	}

	public static bool RemoveAllRedAbilities()
	{
		AbilityDebugMenuItems.QuickFlameSetter(false);
		AbilityDebugMenuItems.SparkFlameSetter(false);
		AbilityDebugMenuItems.ChargeFlameBurnSetter(false);
		AbilityDebugMenuItems.SplitFlameUpgradeSetter(false);
		AbilityDebugMenuItems.GrenadeUpgradeSetter(false);
		AbilityDebugMenuItems.CinderFlameSetter(false);
		AbilityDebugMenuItems.StompUpgradeSetter(false);
		AbilityDebugMenuItems.RapidFireSetter(false);
		AbilityDebugMenuItems.ChargeFlameBlastSetter(false);
		AbilityDebugMenuItems.UltraSplitFlameSetter(false);

		// also remove these deprecated abilities just in case
		AbilityDebugMenuItems.BashUpgradeSetter(false);
		AbilityDebugMenuItems.GrenadeEfficiencySetter(false);
		return true;
	}
}
