using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Sein.World;

namespace Protogen
{
    public class Inventory
    {
        public HashSet<string> Unlocks = new HashSet<string>{"Free"};

        public int Keystones;

        public int Mapstones;

        public int Health;

        public int Energy;

        public int Acs;

        public bool Contains(Inventory other)
        {
            return Unlocks.IsSupersetOf(other.Unlocks)
                   && other.Acs <= Acs
                   && other.Energy <= Energy
                   && other.Health <= Health
                   && other.Keystones <= Keystones
                   && other.Mapstones <= Mapstones;
        }

        public override string ToString()
        {
            List<string> parts = new List<string>();
            parts.AddRange(this.Unlocks.ToList<string>());

            if (this.Keystones > 0)
            {
                parts.Add("Keystone=" + this.Keystones);
            }

            if (this.Mapstones > 0)
            {
                parts.Add("Mapstone=" + this.Mapstones);
            }

            if (this.Health > 0)
            {
                parts.Add("Health=" + this.Health);
            }

            if (this.Energy > 0)
            {
                parts.Add("Energy=" + this.Energy);
            }

            if (this.Acs > 0)
            {
                parts.Add("Ability=" + this.Acs);
            }

            return String.Join(", ", parts.ToArray());
        }

        public static Inventory operator +(Inventory a, Inventory b)
        {
            return new Inventory{Unlocks = new HashSet<string>(a.Unlocks.Union(b.Unlocks)), Keystones = a.Keystones + b.Keystones, Mapstones = a.Mapstones + b.Mapstones, Health = a.Health + b.Health, Energy = a.Energy + b.Energy, Acs = a.Acs + b.Acs};
        }

        public static Inventory operator -(Inventory a, Inventory b)
        {
            return new Inventory{Unlocks = new HashSet<string>(a.Unlocks.Except(b.Unlocks)), Keystones = a.Keystones - b.Keystones, Mapstones = a.Mapstones - b.Mapstones, Health = a.Health - b.Health, Energy = a.Energy - b.Energy, Acs = a.Acs - b.Acs};
        }

        public static Inventory FromCharacter()
        {
            Inventory currentInventory = new Inventory();
            currentInventory.Health = (int)(Characters.Sein.Mortality.Health.MaxHealth / 4f);
            currentInventory.Energy = (int)Characters.Sein.Energy.Max;
            currentInventory.Keystones = Characters.Sein.Inventory.Keystones;
            currentInventory.Mapstones = Characters.Sein.Inventory.MapStones;
            currentInventory.Acs = Characters.Sein.Inventory.SkillPointsCollected;

            if (Characters.Sein.PlayerAbilities.WallJump.HasAbility)
            {
                currentInventory.Unlocks.Add("WallJump");
            }

            if (Characters.Sein.PlayerAbilities.ChargeFlame.HasAbility)
            {
                currentInventory.Unlocks.Add("ChargeFlame");
                if (currentInventory.Acs >= 3) {
                    currentInventory.Unlocks.Add("ChargeFlameBurn");
                }
            }

            if (Characters.Sein.PlayerAbilities.DoubleJump.HasAbility)
            {
                currentInventory.Unlocks.Add("DoubleJump");
                if (currentInventory.Acs >= 12) {
                    currentInventory.Unlocks.Add("TripleJump");
                }
            }

            if (Characters.Sein.PlayerAbilities.Bash.HasAbility)
            {
                currentInventory.Unlocks.Add("Bash");
                currentInventory.Unlocks.Add("DoubleBash");
            }

            if (Characters.Sein.PlayerAbilities.Stomp.HasAbility)
            {
                currentInventory.Unlocks.Add("Stomp");
            }

            if (Characters.Sein.PlayerAbilities.Glide.HasAbility)
            {
                currentInventory.Unlocks.Add("Glide");
            }

            if (Characters.Sein.PlayerAbilities.Climb.HasAbility)
            {
                currentInventory.Unlocks.Add("Climb");
            }

            if (Characters.Sein.PlayerAbilities.ChargeJump.HasAbility)
            {
                currentInventory.Unlocks.Add("ChargeJump");
            }

            if (Characters.Sein.PlayerAbilities.Grenade.HasAbility)
            {
                currentInventory.Unlocks.Add("Grenade");
            }

            if (Characters.Sein.PlayerAbilities.Dash.HasAbility)
            {
                currentInventory.Unlocks.Add("Dash");
                if (currentInventory.Acs >= 3) {
                    currentInventory.Unlocks.Add("AirDash");
                }
                if (currentInventory.Acs >= 6) {
                    currentInventory.Unlocks.Add("ChargeDash");
                    currentInventory.Unlocks.Add("RocketJump");
                }
            }

            if (Characters.Sein.PlayerAbilities.ChargeJump.HasAbility && Characters.Sein.PlayerAbilities.Climb.HasAbility && Characters.Sein.PlayerAbilities.Grenade.HasAbility)
            {
                currentInventory.Unlocks.Add("GrenadeJump");
            }

            if (Characters.Sein.PlayerAbilities.Bash.HasAbility && Characters.Sein.PlayerAbilities.Grenade.HasAbility)
            {
                currentInventory.Unlocks.Add("BashGrenade");
            }

            // Currently the equivalent of Ability=0, so always expected.
            currentInventory.Unlocks.Add("Rekindle");

            if (currentInventory.Acs >= 3) {
                currentInventory.Unlocks.Add("Regroup");
            }

            if (currentInventory.Acs >= 6) {
                currentInventory.Unlocks.Add("UltraSoulLink");
            }

            if (currentInventory.Acs >= 12) {
                currentInventory.Unlocks.Add("UltraDefense");
            }

            currentInventory.Unlocks.Add("Lure");

            if (Sein.World.Events.WaterPurified)
            {
                currentInventory.Unlocks.Add("Water");
            }

            if (Sein.World.Events.WindRestored)
            {
                currentInventory.Unlocks.Add("Wind");
            }

            if (Keys.GinsoTree)
            {
                currentInventory.Unlocks.Add("GinsoKey");
            }

            if (Keys.ForlornRuins)
            {
                currentInventory.Unlocks.Add("ForlornKey");
            }

            if (Keys.MountHoru)
            {
                currentInventory.Unlocks.Add("HoruKey");
            }

            foreach (GameMapTeleporter teleporter in TeleporterController.Instance.Teleporters)
            {
                if (teleporter.Activated)
                {
                    switch (teleporter.Identifier)
                    {
                    case "spiritTree":
                        currentInventory.Unlocks.Add("TPGrove");
                        break;
                    case "swamp":
                        currentInventory.Unlocks.Add("TPSwamp");
                        break;
                    case "moonGrotto":
                        currentInventory.Unlocks.Add("TPGrotto");
                        break;
                    case "sorrowPass":
                        currentInventory.Unlocks.Add("TPValley");
                        break;
                    case "valleyOfTheWind":
                        currentInventory.Unlocks.Add("TPSorrow");
                        break;
                    case "ginsoTree":
                        currentInventory.Unlocks.Add("TPGinso");
                        break;
                    case "forlorn":
                        currentInventory.Unlocks.Add("TPForlorn");
                        break;
                    case "mountHoru":
                        currentInventory.Unlocks.Add("TPHoru");
                        break;
                    }
                }
            }

            return currentInventory;
        }
    }
}