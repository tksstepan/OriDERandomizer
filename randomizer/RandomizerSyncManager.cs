using System;
using System.Collections.Generic;
using System.Net;
using Game;

// Token: 0x020009FF RID: 2559
public static class RandomizerSyncManager
{
	// Token: 0x06003793 RID: 14227
	public static void Initialize()
	{
		RandomizerSyncManager.Countdown = 60 * RandomizerSyncManager.PERIOD;
		RandomizerSyncManager.webClient = new WebClient();
		RandomizerSyncManager.getClient = new WebClient();
		RandomizerSyncManager.UnsavedPickups = new List<RandomizerSyncManager.Pickup>();
		RandomizerSyncManager.PickupQueue = new Queue<RandomizerSyncManager.Pickup>();
		RandomizerSyncManager.getClient.DownloadStringCompleted += RandomizerSyncManager.CheckPickups;
		RandomizerSyncManager.LoseOnDeath = new HashSet<RandomizerSyncManager.Pickup>();
		RandomizerSyncManager.SkillInfos = new List<RandomizerSyncManager.SkillInfoLine>();
		RandomizerSyncManager.EventInfos = new List<RandomizerSyncManager.EventInfoLine>();
		RandomizerSyncManager.UpgradeInfos = new List<RandomizerSyncManager.UpgradeInfoLine>();
		foreach (int i in new int[]
		{
			6,
			13,
			15,
			17,
			19,
			21
		})
		{
			RandomizerSyncManager.LoseOnDeath.Add(new RandomizerSyncManager.Pickup("upgrade", i.ToString()));
		}
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(0, 0, AbilityType.Bash));
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(2, 1, AbilityType.ChargeFlame));
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(3, 2, AbilityType.WallJump));
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(4, 3, AbilityType.Stomp));
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(5, 4, AbilityType.DoubleJump));
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(8, 5, AbilityType.ChargeJump));
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(12, 6, AbilityType.Climb));
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(14, 7, AbilityType.Glide));
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(50, 8, AbilityType.Dash));
		RandomizerSyncManager.SkillInfos.Add(new RandomizerSyncManager.SkillInfoLine(51, 9, AbilityType.Grenade));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(17, 0, true, () => RandomizerBonus.WaterVeinShards()));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(19, 1, true, () => RandomizerBonus.GumonSealShards()));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(21, 2, true, () => RandomizerBonus.SunstoneShards()));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(6, 3, true, () => RandomizerBonus.SpiritFlameLevel()));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(13, 4, true, () => RandomizerBonus.HealthRegeneration()));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(15, 5, true, () => RandomizerBonus.EnergyRegeneration()));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(8, 12, false, delegate
		{
			if (!RandomizerBonus.ExplosionPower())
			{
				return 0;
			}
			return 1;
		}));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(9, 13, false, delegate
		{
			if (!RandomizerBonus.ExpEfficiency())
			{
				return 0;
			}
			return 1;
		}));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(10, 14, false, delegate
		{
			if (!RandomizerBonus.DoubleAirDash())
			{
				return 0;
			}
			return 1;
		}));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(11, 15, false, delegate
		{
			if (!RandomizerBonus.ChargeDashEfficiency())
			{
				return 0;
			}
			return 1;
		}));
		RandomizerSyncManager.UpgradeInfos.Add(new RandomizerSyncManager.UpgradeInfoLine(12, 16, false, delegate
		{
			if (!RandomizerBonus.DoubleJumpUpgrade())
			{
				return 0;
			}
			return 1;
		}));
	}

	// Token: 0x06003794 RID: 14228
	public static void Update()
	{
		if (RandomizerSyncManager.PickupQueue.Count > 0 && !RandomizerSyncManager.webClient.IsBusy)
		{
			RandomizerSyncManager.Pickup pickup = RandomizerSyncManager.PickupQueue.Dequeue();
			string uriString = string.Concat(new object[]
			{
				RandomizerSyncManager.SERVER_ROOT,
				Randomizer.SyncId,
				"/",
				pickup.type,
				"/",
				pickup.id
			});
			RandomizerSyncManager.webClient.DownloadStringAsync(new Uri(uriString));
		}
		RandomizerSyncManager.Countdown--;
		if (RandomizerSyncManager.Countdown <= 0 && !RandomizerSyncManager.getClient.IsBusy)
		{
			RandomizerSyncManager.Countdown = 60 * RandomizerSyncManager.PERIOD;
			RandomizerSyncManager.getClient.DownloadStringAsync(new Uri(RandomizerSyncManager.SERVER_ROOT + Randomizer.SyncId.ToString()));
		}
	}

	// Token: 0x06003795 RID: 14229
	static RandomizerSyncManager()
	{
	}

	// Token: 0x06003797 RID: 14231
	public static bool getBit(int bf, int bit)
	{
		return 1 == (bf >> bit & 1);
	}

	// Token: 0x06003798 RID: 14232
	public static int getTaste(int bf, int taste)
	{
		return bf >> 2 * taste & 3;
	}

	// Token: 0x0600379B RID: 14235
	public static void CheckPickups(object sender, DownloadStringCompletedEventArgs e)
	{
		if (!e.Cancelled && e.Error == null)
		{
			string[] array = e.Result.Split(new char[]
			{
				','
			});
			int skill_bf = int.Parse(array[0]);
			foreach (RandomizerSyncManager.SkillInfoLine sfl in RandomizerSyncManager.SkillInfos)
			{
				bool bit = RandomizerSyncManager.getBit(skill_bf, sfl.bit);
				if (bit != Characters.Sein.PlayerAbilities.HasAbility(sfl.skill))
				{
					if (bit)
					{
						RandomizerSwitch.AbilityPickup(sfl.id);
					}
					else
					{
						RandomizerSyncManager.PickupQueue.Enqueue(new RandomizerSyncManager.Pickup("skill", sfl.id.ToString()));
					}
				}
			}
			int event_bf = int.Parse(array[1]);
			foreach (RandomizerSyncManager.EventInfoLine efl in RandomizerSyncManager.EventInfos)
			{
				bool bit2 = RandomizerSyncManager.getBit(event_bf, efl.bit);
				if (bit2 != efl.checker())
				{
					if (bit2)
					{
						RandomizerSwitch.EventPickup(efl.id);
					}
					else
					{
						RandomizerSyncManager.PickupQueue.Enqueue(new RandomizerSyncManager.Pickup("event", efl.id.ToString()));
					}
				}
			}
			int upgrade_bf = int.Parse(array[2]);
			foreach (RandomizerSyncManager.UpgradeInfoLine ufl in RandomizerSyncManager.UpgradeInfos)
			{
				if (ufl.stacks)
				{
					int taste = RandomizerSyncManager.getTaste(upgrade_bf, ufl.bit);
					if (taste != ufl.counter())
					{
						if (taste > ufl.counter())
						{
							RandomizerBonus.UpgradeID(ufl.id);
						}
						else if (!RandomizerSyncManager.UnsavedPickups.Contains(new RandomizerSyncManager.Pickup("upgrade", ufl.id.ToString())))
						{
							RandomizerSyncManager.UnsavedPickups.Add(new RandomizerSyncManager.Pickup("upgrade", ufl.id.ToString()));
						}
					}
				}
				else
				{
					bool bit3 = RandomizerSyncManager.getBit(upgrade_bf, ufl.bit);
					if (bit3 != (1 == ufl.counter()))
					{
						if (bit3)
						{
							RandomizerBonus.UpgradeID(ufl.id);
						}
						else
						{
							RandomizerSyncManager.PickupQueue.Enqueue(new RandomizerSyncManager.Pickup("upgrade", ufl.id.ToString()));
						}
					}
				}
			}
			int bf = int.Parse(array[3]);
			if (RandomizerSyncManager.getBit(bf, 0))
			{
				TeleporterController.Activate(Randomizer.TeleportTable["Grove"].ToString());
			}
			if (RandomizerSyncManager.getBit(bf, 1))
			{
				TeleporterController.Activate(Randomizer.TeleportTable["Swamp"].ToString());
			}
			if (RandomizerSyncManager.getBit(bf, 2))
			{
				TeleporterController.Activate(Randomizer.TeleportTable["Grotto"].ToString());
			}
			if (RandomizerSyncManager.getBit(bf, 3))
			{
				TeleporterController.Activate(Randomizer.TeleportTable["Valley"].ToString());
			}
			if (RandomizerSyncManager.getBit(bf, 4))
			{
				TeleporterController.Activate(Randomizer.TeleportTable["Forlorn"].ToString());
			}
			if (RandomizerSyncManager.getBit(bf, 5))
			{
				TeleporterController.Activate(Randomizer.TeleportTable["Sorrow"].ToString());
			}
			return;
		}
	}

	// Token: 0x060037AA RID: 14250
	public static void onSave()
	{
		foreach (RandomizerSyncManager.Pickup p in RandomizerSyncManager.UnsavedPickups)
		{
			RandomizerSyncManager.PickupQueue.Enqueue(p);
		}
		RandomizerSyncManager.UnsavedPickups.Clear();
	}

	// Token: 0x060037C2 RID: 14274
	public static void FoundPickup(string type, string id)
	{
		RandomizerSyncManager.Pickup p = new RandomizerSyncManager.Pickup(type, id);
		if (RandomizerSyncManager.LoseOnDeath.Contains(p))
		{
			RandomizerSyncManager.UnsavedPickups.Add(p);
			return;
		}
		RandomizerSyncManager.PickupQueue.Enqueue(p);
	}

	// Token: 0x06003826 RID: 14374
	public static void onDeath()
	{
		RandomizerSyncManager.UnsavedPickups.Clear();
	}

	// Token: 0x04003268 RID: 12904
	public static int Countdown;

	// Token: 0x04003269 RID: 12905
	public static int PERIOD = 1;

	// Token: 0x0400326A RID: 12906
	public static WebClient webClient;

	// Token: 0x0400326B RID: 12907
	public static string SERVER_ROOT = "http://orirandocoopserver.appspot.com/";

	// Token: 0x0400326C RID: 12908
	public static string lastRaw;

	// Token: 0x0400326D RID: 12909
	public static bool canRun;

	// Token: 0x0400326E RID: 12910
	public static WebClient getClient;

	// Token: 0x0400329E RID: 12958
	public static List<RandomizerSyncManager.Pickup> UnsavedPickups;

	// Token: 0x0400329F RID: 12959
	public static Queue<RandomizerSyncManager.Pickup> PickupQueue;

	// Token: 0x04003308 RID: 13064
	public static HashSet<RandomizerSyncManager.Pickup> LoseOnDeath;

	// Token: 0x04003339 RID: 13113
	public static List<RandomizerSyncManager.SkillInfoLine> SkillInfos;

	// Token: 0x0400333A RID: 13114
	public static List<RandomizerSyncManager.EventInfoLine> EventInfos;

	// Token: 0x0400333B RID: 13115
	public static List<RandomizerSyncManager.UpgradeInfoLine> UpgradeInfos;

	// Token: 0x02000A02 RID: 2562
	public class Pickup
	{
		// Token: 0x060037C5 RID: 14277
		public Pickup(string _type, string _id)
		{
			this.type = _type;
			this.id = _id;
		}

		// Token: 0x06003873 RID: 14451
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			RandomizerSyncManager.Pickup p = (RandomizerSyncManager.Pickup)obj;
			return this.type == p.type && this.id == p.id;
		}

		// Token: 0x06003874 RID: 14452
		public override int GetHashCode()
		{
			return this.type.GetHashCode() ^ this.id.GetHashCode();
		}

		// Token: 0x04003295 RID: 12949
		public string id;

		// Token: 0x04003296 RID: 12950
		public string type;
	}

	// Token: 0x02000A0A RID: 2570
	public class SkillInfoLine
	{
		// Token: 0x06003881 RID: 14465
		public SkillInfoLine(int _id, int _bit, AbilityType _skill)
		{
			this.bit = _bit;
			this.id = _id;
			this.skill = _skill;
		}

		// Token: 0x06003882 RID: 14466
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			RandomizerSyncManager.SkillInfoLine p = (RandomizerSyncManager.SkillInfoLine)obj;
			return this.bit == p.bit && this.id == p.id && this.skill == p.skill;
		}

		// Token: 0x06003883 RID: 14467
		public override int GetHashCode()
		{
			return this.skill.GetHashCode() ^ this.id.GetHashCode() ^ this.bit.GetHashCode();
		}

		// Token: 0x04003319 RID: 13081
		public int id;

		// Token: 0x0400331A RID: 13082
		public int bit;

		// Token: 0x0400331B RID: 13083
		public AbilityType skill;
	}

	// Token: 0x02000A0B RID: 2571
	// (Invoke) Token: 0x06003885 RID: 14469
	public delegate int UpgradeCounter();

	// Token: 0x02000A0C RID: 2572
	public class UpgradeInfoLine
	{
		// Token: 0x06003888 RID: 14472
		public UpgradeInfoLine(int _id, int _bit, bool _stacks, RandomizerSyncManager.UpgradeCounter _counter)
		{
			this.bit = _bit;
			this.id = _id;
			this.stacks = _stacks;
			this.counter = _counter;
		}

		// Token: 0x06003889 RID: 14473
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			RandomizerSyncManager.UpgradeInfoLine p = (RandomizerSyncManager.UpgradeInfoLine)obj;
			return this.bit == p.bit && this.id == p.id;
		}

		// Token: 0x0600388A RID: 14474
		public override int GetHashCode()
		{
			return this.bit.GetHashCode() ^ this.id.GetHashCode();
		}

		// Token: 0x0400331C RID: 13084
		public int id;

		// Token: 0x0400331D RID: 13085
		public int bit;

		// Token: 0x0400331E RID: 13086
		public bool stacks;

		// Token: 0x0400331F RID: 13087
		public RandomizerSyncManager.UpgradeCounter counter;
	}

	// Token: 0x02000A0D RID: 2573
	// (Invoke) Token: 0x0600388C RID: 14476
	public delegate bool EventChecker();

	// Token: 0x02000A0E RID: 2574
	public class EventInfoLine
	{
		// Token: 0x0600388F RID: 14479
		public EventInfoLine(int _id, int _bit, RandomizerSyncManager.EventChecker _checker)
		{
			this.bit = _bit;
			this.id = _id;
			this.checker = _checker;
		}

		// Token: 0x06003890 RID: 14480
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			RandomizerSyncManager.EventInfoLine p = (RandomizerSyncManager.EventInfoLine)obj;
			return this.bit == p.bit && this.id == p.id;
		}

		// Token: 0x06003891 RID: 14481
		public override int GetHashCode()
		{
			return this.bit.GetHashCode() ^ this.id.GetHashCode();
		}

		// Token: 0x04003320 RID: 13088
		public int id;

		// Token: 0x04003321 RID: 13089
		public RandomizerSyncManager.EventChecker checker;

		// Token: 0x04003322 RID: 13090
		public int bit;
	}
}
