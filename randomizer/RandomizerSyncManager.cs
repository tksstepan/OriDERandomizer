using System;
using System.Collections.Generic;
using System.Net;
using Game;
using Sein.World;

// Token: 0x020009FF RID: 2559
public static class RandomizerSyncManager
{
	// Token: 0x06003793 RID: 14227
	public static void Initialize()
	{
		RandomizerSyncManager.Countdown = 60 * RandomizerSyncManager.PERIOD;
		RandomizerSyncManager.webClient = new WebClient();
		RandomizerSyncManager.webClient.DownloadStringCompleted += RandomizerSyncManager.RetryOnFail;
		RandomizerSyncManager.getClient = new WebClient();
		RandomizerSyncManager.getClient.DownloadStringCompleted += RandomizerSyncManager.CheckPickups;
		RandomizerSyncManager.SendingPickup = null;
		if (RandomizerSyncManager.UnsavedPickups == null)
		{
			RandomizerSyncManager.UnsavedPickups = new List<RandomizerSyncManager.Pickup>();
		}
		if (RandomizerSyncManager.PickupQueue == null)
		{
			RandomizerSyncManager.PickupQueue = new Queue<RandomizerSyncManager.Pickup>();
		}
		RandomizerSyncManager.LoseOnDeath = new HashSet<string>();
		RandomizerSyncManager.SkillInfos = new List<RandomizerSyncManager.SkillInfoLine>();
		RandomizerSyncManager.EventInfos = new List<RandomizerSyncManager.EventInfoLine>();
		RandomizerSyncManager.UpgradeInfos = new List<RandomizerSyncManager.UpgradeInfoLine>();
		RandomizerSyncManager.TeleportInfos = new List<RandomizerSyncManager.TeleportInfoLine>();
		RandomizerSyncManager.TeleportInfos.Add(new RandomizerSyncManager.TeleportInfoLine("Grove", 0));
		RandomizerSyncManager.TeleportInfos.Add(new RandomizerSyncManager.TeleportInfoLine("Swamp", 1));
		RandomizerSyncManager.TeleportInfos.Add(new RandomizerSyncManager.TeleportInfoLine("Grotto", 2));
		RandomizerSyncManager.TeleportInfos.Add(new RandomizerSyncManager.TeleportInfoLine("Valley", 3));
		RandomizerSyncManager.TeleportInfos.Add(new RandomizerSyncManager.TeleportInfoLine("Forlorn", 4));
		RandomizerSyncManager.TeleportInfos.Add(new RandomizerSyncManager.TeleportInfoLine("Sorrow", 5));
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
		RandomizerSyncManager.EventInfos.Add(new RandomizerSyncManager.EventInfoLine(0, 0, () => Keys.GinsoTree));
		RandomizerSyncManager.EventInfos.Add(new RandomizerSyncManager.EventInfoLine(1, 1, () => Sein.World.Events.WaterPurified));
		RandomizerSyncManager.EventInfos.Add(new RandomizerSyncManager.EventInfoLine(2, 2, () => Keys.ForlornRuins));
		RandomizerSyncManager.EventInfos.Add(new RandomizerSyncManager.EventInfoLine(3, 3, () => Sein.World.Events.WindRestored));
		RandomizerSyncManager.EventInfos.Add(new RandomizerSyncManager.EventInfoLine(4, 4, () => Keys.MountHoru));
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
		if (RandomizerSyncManager.SendingPickup == null && RandomizerSyncManager.PickupQueue.Count > 0 && !RandomizerSyncManager.webClient.IsBusy)
		{
			RandomizerSyncManager.SendingPickup = RandomizerSyncManager.PickupQueue.Dequeue();
			RandomizerSyncManager.webClient.DownloadStringAsync(new Uri(RandomizerSyncManager.SendingPickup.GetURL()));
		}
		RandomizerSyncManager.Countdown--;
		if (RandomizerSyncManager.Countdown <= 0 && !RandomizerSyncManager.getClient.IsBusy)
		{
			RandomizerSyncManager.Countdown = 60 * RandomizerSyncManager.PERIOD;
			RandomizerSyncManager.getClient.DownloadStringAsync(new Uri(RandomizerSyncManager.SERVER_ROOT + Randomizer.SyncId));
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
				if (RandomizerSyncManager.getBit(skill_bf, sfl.bit) && !Characters.Sein.PlayerAbilities.HasAbility(sfl.skill))
				{
					RandomizerSwitch.GivePickup(new RandomizerAction("SK", sfl.id), 0, false);
				}
			}
			int event_bf = int.Parse(array[1]);
			foreach (RandomizerSyncManager.EventInfoLine efl in RandomizerSyncManager.EventInfos)
			{
				if (RandomizerSyncManager.getBit(event_bf, efl.bit) && !efl.checker())
				{
					RandomizerSwitch.GivePickup(new RandomizerAction("EV", efl.id), 0, false);
				}
			}
			int upgrade_bf = int.Parse(array[2]);
			foreach (RandomizerSyncManager.UpgradeInfoLine ufl in RandomizerSyncManager.UpgradeInfos)
			{
				if (ufl.stacks)
				{
					if (RandomizerSyncManager.getTaste(upgrade_bf, ufl.bit) > ufl.counter())
					{
						RandomizerSwitch.GivePickup(new RandomizerAction("RB", ufl.id), 0, false);
					}
				}
				else if (RandomizerSyncManager.getBit(upgrade_bf, ufl.bit) && 1 != ufl.counter())
				{
					RandomizerSwitch.GivePickup(new RandomizerAction("RB", ufl.id), 0, false);
				}
			}
			int bf = int.Parse(array[3]);
			foreach (RandomizerSyncManager.TeleportInfoLine tfl in RandomizerSyncManager.TeleportInfos)
			{
				if (RandomizerSyncManager.getBit(bf, tfl.bit) && !RandomizerSyncManager.isTeleporterActivated(tfl.id))
				{
					RandomizerSwitch.GivePickup(new RandomizerAction("TP", tfl.id), 0, false);
				}
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

	// Token: 0x06003826 RID: 14374
	public static void onDeath()
	{
		RandomizerSyncManager.UnsavedPickups.Clear();
	}

	// Token: 0x06003A18 RID: 14872
	public static void RetryOnFail(object sender, DownloadStringCompletedEventArgs e)
	{
		if (!e.Cancelled && e.Error == null)
		{
			RandomizerSyncManager.SendingPickup = null;
			return;
		}
		if (e.Error.GetType().Name == "WebException")
		{
			HttpStatusCode statusCode = ((HttpWebResponse)((WebException)e.Error).Response).StatusCode;
			if (statusCode == HttpStatusCode.NotAcceptable)
			{
				RandomizerSyncManager.SendingPickup = null;
				return;
			}
			if (statusCode == HttpStatusCode.Gone)
			{
				if (RandomizerSyncManager.SendingPickup.type == "RB")
				{
					RandomizerBonus.UpgradeID(-int.Parse(RandomizerSyncManager.SendingPickup.id));
				}
				RandomizerSyncManager.SendingPickup = null;
			}
		}
		RandomizerSyncManager.webClient.DownloadStringAsync(new Uri(RandomizerSyncManager.SendingPickup.GetURL()));
	}

	// Token: 0x06003A1B RID: 14875
	public static void FoundPickup(RandomizerAction action, int coords)
	{
		RandomizerSyncManager.Pickup p = new RandomizerSyncManager.Pickup(action, coords);
		if (RandomizerSyncManager.LoseOnDeath.Contains(p.type + p.id))
		{
			RandomizerSyncManager.UnsavedPickups.Add(p);
			return;
		}
		RandomizerSyncManager.PickupQueue.Enqueue(p);
	}

	// Token: 0x06003E49 RID: 15945
	public static bool isTeleporterActivated(string identifier)
	{
		foreach (GameMapTeleporter current in TeleporterController.Instance.Teleporters)
		{
			if (current.Identifier == Randomizer.TeleportTable[identifier].ToString())
			{
				return current.Activated;
			}
		}
		return false;
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

	// Token: 0x04003339 RID: 13113
	public static List<RandomizerSyncManager.SkillInfoLine> SkillInfos;

	// Token: 0x0400333A RID: 13114
	public static List<RandomizerSyncManager.EventInfoLine> EventInfos;

	// Token: 0x0400333B RID: 13115
	public static List<RandomizerSyncManager.UpgradeInfoLine> UpgradeInfos;

	// Token: 0x0400347B RID: 13435
	public static HashSet<string> LoseOnDeath;

	// Token: 0x0400347F RID: 13439
	public static RandomizerSyncManager.Pickup SendingPickup;

	// Token: 0x040037E0 RID: 14304
	public static List<RandomizerSyncManager.TeleportInfoLine> TeleportInfos;

	// Token: 0x02000A02 RID: 2562
	public class Pickup
	{
		// Token: 0x06003873 RID: 14451
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			RandomizerSyncManager.Pickup p = (RandomizerSyncManager.Pickup)obj;
			return this.type == p.type && this.id == p.id && this.coords == p.coords;
		}

		// Token: 0x06003874 RID: 14452
		public override int GetHashCode()
		{
			return (this.type + this.id).GetHashCode() ^ this.coords.GetHashCode();
		}

		// Token: 0x06003A1D RID: 14877
		public Pickup(string _type, string _id, int _coords)
		{
			this.type = _type;
			this.id = _id;
			this.coords = _coords;
		}

		// Token: 0x06003A1E RID: 14878
		public Pickup(RandomizerAction action, int _coords)
		{
			this.type = action.Action;
			this.id = ((this.type == "TP") ? ((string)action.Value) : ((int)action.Value).ToString());
			this.coords = _coords;
		}

		// Token: 0x06003A1F RID: 14879
		public string GetURL()
		{
			return string.Concat(new object[]
			{
				RandomizerSyncManager.SERVER_ROOT,
				Randomizer.SyncId,
				"/",
				RandomizerSyncManager.SendingPickup.coords,
				"/",
				RandomizerSyncManager.SendingPickup.type,
				"/",
				RandomizerSyncManager.SendingPickup.id
			});
		}

		// Token: 0x04003295 RID: 12949
		public string id;

		// Token: 0x04003296 RID: 12950
		public string type;

		// Token: 0x04003481 RID: 13441
		public int coords;
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

	// Token: 0x02000A23 RID: 2595
	public class TeleportInfoLine
	{
		// Token: 0x06003E3C RID: 15932
		public TeleportInfoLine(string _id, int _bit)
		{
			this.bit = _bit;
			this.id = _id;
		}

		// Token: 0x06003E3D RID: 15933
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			RandomizerSyncManager.TeleportInfoLine p = (RandomizerSyncManager.TeleportInfoLine)obj;
			return this.bit == p.bit && this.id == p.id;
		}

		// Token: 0x06003E3E RID: 15934
		public override int GetHashCode()
		{
			return this.bit.GetHashCode() ^ this.id.GetHashCode();
		}

		// Token: 0x040037D2 RID: 14290
		public string id;

		// Token: 0x040037D3 RID: 14291
		public int bit;
	}
}
