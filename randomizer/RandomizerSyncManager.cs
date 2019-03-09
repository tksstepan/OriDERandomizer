using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using Game;
using Sein.World;
using UnityEngine;

// Token: 0x020009FF RID: 2559
public static class RandomizerSyncManager
{
	public static void Initialize()
	{
		Countdown = 60;
		webClient = new WebClient();
		webClient.DownloadStringCompleted += RetryOnFail;
		getClient = new WebClient();
		getClient.DownloadStringCompleted += CheckPickups;
		DoCreditWarp = false;
		if (JustFound == null)
			JustFound = new HashSet<String>();
		if (CurrentSignals == null)
			CurrentSignals = new HashSet<String>();
		if (PickupQueue == null)
			PickupQueue = new Queue<Pickup>();
		SeedSent = false;
		Hints = new Dictionary<int, int>();
		SkillInfos = new List<SkillInfoLine>();
		EventInfos = new List<EventInfoLine>();
		TeleportInfos = new List<TeleportInfoLine>();
		TeleportInfos.Add(new TeleportInfoLine("Grove", 0));
		TeleportInfos.Add(new TeleportInfoLine("Swamp", 1));
		TeleportInfos.Add(new TeleportInfoLine("Grotto", 2));
		TeleportInfos.Add(new TeleportInfoLine("Valley", 3));
		TeleportInfos.Add(new TeleportInfoLine("Forlorn", 4));
		TeleportInfos.Add(new TeleportInfoLine("Sorrow", 5));
		TeleportInfos.Add(new TeleportInfoLine("Ginso", 6));
		TeleportInfos.Add(new TeleportInfoLine("Horu", 7));
		SkillInfos.Add(new SkillInfoLine(0, 0, AbilityType.Bash));
		SkillInfos.Add(new SkillInfoLine(2, 1, AbilityType.ChargeFlame));
		SkillInfos.Add(new SkillInfoLine(3, 2, AbilityType.WallJump));
		SkillInfos.Add(new SkillInfoLine(4, 3, AbilityType.Stomp));
		SkillInfos.Add(new SkillInfoLine(5, 4, AbilityType.DoubleJump));
		SkillInfos.Add(new SkillInfoLine(8, 5, AbilityType.ChargeJump));
		SkillInfos.Add(new SkillInfoLine(12, 6, AbilityType.Climb));
		SkillInfos.Add(new SkillInfoLine(14, 7, AbilityType.Glide));
		SkillInfos.Add(new SkillInfoLine(50, 8, AbilityType.Dash));
		SkillInfos.Add(new SkillInfoLine(51, 9, AbilityType.Grenade));
		EventInfos.Add(new EventInfoLine(0, 0, () => Keys.GinsoTree));
		EventInfos.Add(new EventInfoLine(1, 1, () => Sein.World.Events.WaterPurified));
		EventInfos.Add(new EventInfoLine(2, 2, () => Keys.ForlornRuins));
		EventInfos.Add(new EventInfoLine(3, 3, () => Sein.World.Events.WindRestored));
		EventInfos.Add(new EventInfoLine(4, 4, () => Keys.MountHoru));
		if(Randomizer.SyncId != "") {
			string[] parts = Randomizer.SyncId.Split('.');
			RootUrl = "http://orirandov3.appspot.com/netcode/game/" + parts[0] + "/player/" + parts[1]; 
		}
	}

	// Token: 0x06003794 RID: 14228
	public static void Update()
	{
		try
		{
			if (SendingPickup == null && PickupQueue.Count > 0 && !webClient.IsBusy)
			{
				SendingPickup = PickupQueue.Dequeue();
				webClient.DownloadStringAsync(SendingPickup.GetURL());
			}
			else if (Randomizer.SyncId != "" && !SeedSent)
			{
				UploadSeed();
			}
			Countdown--;
			ChaosTimeoutCounter--;
			if (ChaosTimeoutCounter < 0)
			{
				RandomizerChaosManager.ClearEffects();
				ChaosTimeoutCounter = 216000;
			}
			if (Countdown <= 0 && !getClient.IsBusy)
			{
				Countdown = 60 * PERIOD;
				Vector3 pos = Characters.Sein.Position;
				Uri uri = new Uri(RootUrl + "/tick/" + pos.x.ToString() + "," + pos.y.ToString()); 
				getClient.DownloadStringAsync(uri);
			}
		} catch(Exception e) {
			Randomizer.LogError("RSM.Update: " + e.Message);
		}

	}

	public static void UploadSeed()
	{
		try
		{
			string[] array = File.ReadAllLines("randomizer.dat");
			array[0] = array[0].Replace(',', '|');
			NameValueCollection nvc = new NameValueCollection();
			nvc.Set("seed", string.Join(",", array).Replace("#",""));
			nvc.Set("version", BingoController.BINGO_VERSION);
			var client = new WebClient();
			client.UploadValuesAsync(new Uri(RootUrl + "/setSeed"), nvc);
			SeedSent = true;			
		} catch(Exception e) {
			Randomizer.LogError("UploadSeed: " + e.Message);
		}
	}

	// Token: 0x06003795 RID: 14229
	static RandomizerSyncManager()
	{
	}

	// Token: 0x06003796 RID: 14230
	public static bool getBit(int bf, int bit)
	{
		return 1 == (bf >> bit & 1);
	}

	// Token: 0x06003797 RID: 14231
	public static int getTaste(int bf, int taste)
	{
		return bf >> 2 * taste & 3;
	}

	// Token: 0x06003798 RID: 14232
	public static void CheckPickups(object sender, DownloadStringCompletedEventArgs e)
	{
		try
		{
			if (e.Error != null)
			{
				if(e.Error is System.NullReferenceException)
					return;
				Randomizer.LogError("CheckPickups got error: " + e.Error.ToString());
			}
			if (!e.Cancelled && e.Error == null)
			{
				string[] array = e.Result.Split(new char[]
				{
					','
				});
				int bf = int.Parse(array[0]);
				foreach (SkillInfoLine skillInfoLine in SkillInfos)
				{
					if (getBit(bf, skillInfoLine.bit) && !Characters.Sein.PlayerAbilities.HasAbility(skillInfoLine.skill))
					{
						RandomizerSwitch.GivePickup(new RandomizerAction("SK", skillInfoLine.id), 0, false);
					}
				}
				int bf2 = int.Parse(array[1]);
				foreach (EventInfoLine eventInfoLine in EventInfos)
				{
					if (getBit(bf2, eventInfoLine.bit) && !eventInfoLine.checker())
					{
						RandomizerSwitch.GivePickup(new RandomizerAction("EV", eventInfoLine.id), 0, false);
					}
				}
				int bf4 = int.Parse(array[2]);
				foreach (TeleportInfoLine teleportInfoLine in TeleportInfos)
				{
					if (getBit(bf4, teleportInfoLine.bit) && !isTeleporterActivated(teleportInfoLine.id))
					{
						RandomizerSwitch.GivePickup(new RandomizerAction("TP", teleportInfoLine.id), 0, false);
					}
				}
				if(array[3] != "")
					{
					string[] upgrades = array[3].Split(';');
					foreach(string rawUpgrade in upgrades)
					{
						string[] splitpair = rawUpgrade.Split('x');
						int id = int.Parse(splitpair[0]);
						int cnt = int.Parse(splitpair[1]);
						if(id >= 100) {
							if(!RandomizerBonusSkill.UnlockedBonusSkills.ContainsValue(id) && cnt > 0)
								RandomizerBonus.UpgradeID(id);
						} else if(RandomizerBonus.UpgradeCount(id) < cnt) {
							RandomizerBonus.UpgradeID(id);
						} else if(!JustFound.Contains("RB"+splitpair[0]) && RandomizerBonus.UpgradeCount(id) > cnt) {
							RandomizerBonus.UpgradeID(-id);
						}
					}
				}
				if(array[4] != "")
					{
					string[] hints = array[4].Split(';');
					foreach(string rawHint in hints)
					{
						string[] splitpair = rawHint.Split(':');
						int coords = int.Parse(splitpair[0]);
						int player = int.Parse(splitpair[1]);
						Hints[coords] = player;
					}
				}
				if (array.Length > 5)
				{
					foreach (string text in array[5].Split(new char[] { '|' }))
					{
						if(CurrentSignals.Contains(text))
							continue;
						if (text == "stop")
						{
							RandomizerChaosManager.ClearEffects();
						}
						else if (text.StartsWith("msg:"))
						{
							Randomizer.printInfo(text.Substring(4), 360);
						}
						else if (text.StartsWith("win:"))
						{
							Randomizer.Print(text.Substring(4)+"\n (Press Alt+R to warp to credits)", 10, false, true, false, false);
							Randomizer.CanWarp = 10;
							DoCreditWarp = true;
							RandomizerStatsManager.WriteStatsFile();
						}
						else if (text.StartsWith("pickup:"))
						{
							string[] parts = text.Substring(7).Split(new char[] { '|' });
							RandomizerAction action;
							if(Randomizer.StringKeyPickupTypes.Contains(parts[0])) {
								 action = new RandomizerAction(parts[0], parts[1]);
							} else {
								int pickup_id;
								int.TryParse(parts[1], out pickup_id);
								action = new RandomizerAction(parts[0], pickup_id);
							}
							RandomizerSwitch.GivePickup(action, 0, false);
						}
						else if (text == "spawnChaos")
						{
							Randomizer.ChaosVerbose = true;
							RandomizerChaosManager.SpawnEffect();
							ChaosTimeoutCounter = 3600;
						}
						var client = new WebClient();
						client.DownloadStringAsync(new Uri(RootUrl + "/callback/" + text));
						CurrentSignals.Add(text);
					}
				} else {
					CurrentSignals.Clear();
				}
				return;
			}
			if (e.Error.GetType().Name == "WebException" && ((HttpWebResponse)((WebException)e.Error).Response).StatusCode == HttpStatusCode.PreconditionFailed)
			{
				if(Randomizer.SyncMode == 1)
					Randomizer.printInfo("Co-op server error, try reloading the seed (Alt+L)");
				else
					Randomizer.LogError("Co-op server error, try reloading the seed (Alt+L)");
				return;
			}
		}
		catch (Exception e2)
		{
			Randomizer.LogError("FoundPickup threw error: " + e2.Message);
		}
	}

	// Token: 0x0600379B RID: 14235
	public static void RetryOnFail(object sender, DownloadStringCompletedEventArgs e)
	{
		try
		{
			if (e.Cancelled || e.Error != null)
			{
				if (e.Error.GetType().Name == "WebException")
				{
					HttpStatusCode statusCode = ((HttpWebResponse)((WebException)e.Error).Response).StatusCode;
					if (statusCode == HttpStatusCode.Gone) {
						if (SendingPickup.type == "RB")
						{
							RandomizerBonus.UpgradeID(-int.Parse(SendingPickup.id));
						}
					} else if (statusCode != HttpStatusCode.NotAcceptable) {
						webClient.DownloadStringAsync(SendingPickup.GetURL());
						return;
					}
				}
			}
			JustFound.Remove(SendingPickup.type+SendingPickup.id);
			SendingPickup = null;
		} catch(Exception ee) {
			Randomizer.LogError("RetryOnFail: " + ee.Message);
		}
	}

	// Token: 0x0600379C RID: 14236
	public static void FoundPickup(RandomizerAction action, int coords)
	{
		try {
			Pickup pickup = new Pickup(action, coords);
			if(pickup.type == "HN") {
				string[] hintParts = pickup.id.Split('-');
				string name = hintParts[0];
				string type = hintParts[1];
				string owner = hintParts[2];
				string hintText = type + " for " + owner;
				if(Hints.ContainsKey(coords)) {
					if(Hints[coords] > 0) {
						Randomizer.showHint("$" + owner + " found "+ name + " here$");
					} else {
						Randomizer.showHint(hintText);
					}
				} else {
					Randomizer.showHint("@" + hintText + "@");
				}
			}
			JustFound.Add(pickup.type+pickup.id);
			PickupQueue.Enqueue(pickup);	
		} catch(Exception e) {
			Randomizer.LogError("FoundPickup: " + e.Message);
		}
	}

	// Token: 0x0600379D RID: 14237
	public static bool isTeleporterActivated(string identifier)
	{
		if(identifier == "Ginso" && Characters.Sein.Inventory.GetRandomizerItem(1024) == 1)
			return true;
		if(identifier == "Forlorn" && Characters.Sein.Inventory.GetRandomizerItem(1025) == 1)
			return true;
		if(identifier == "Horu" && Characters.Sein.Inventory.GetRandomizerItem(1026) == 1)
			return true;


		foreach (GameMapTeleporter gameMapTeleporter in TeleporterController.Instance.Teleporters)
		{
			if (gameMapTeleporter.Identifier == Randomizer.TeleportTable[identifier].ToString())
			{
				return gameMapTeleporter.Activated;
			}
		}
		return false;
	}

	public static Pickup SendingPickup;

	public static string RootUrl;

	// Token: 0x0400326A RID: 12906
	public static int Countdown;

	// Token: 0x0400326B RID: 12907
	public static int PERIOD = 1;

	// Token: 0x0400326C RID: 12908
	public static WebClient webClient;

	// Token: 0x04003270 RID: 12912
	public static WebClient getClient;

	// Token: 0x04003272 RID: 12914
	public static List<SkillInfoLine> SkillInfos;

	// Token: 0x04003273 RID: 12915
	public static List<EventInfoLine> EventInfos;

	// Token: 0x04003276 RID: 12918
	public static List<TeleportInfoLine> TeleportInfos;

	// Token: 0x04003277 RID: 12919
	public static int ChaosTimeoutCounter = 0;

	public static HashSet<string> JustFound;

	// Token: 0x04003278 RID: 12920
	public static Queue<Pickup> PickupQueue;

	public static bool SeedSent;

	public static Dictionary<int, int> Hints;

	public static HashSet<string> CurrentSignals;

	public static bool DoCreditWarp;

	// Token: 0x02000A00 RID: 2560
	public class Pickup
	{
		// Token: 0x0600379E RID: 14238
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			Pickup pickup = (Pickup)obj;
			return this.type == pickup.type && this.id == pickup.id && this.coords == pickup.coords;
		}

		// Token: 0x0600379F RID: 14239
		public override int GetHashCode()
		{
			return (this.type + this.id).GetHashCode() ^ this.coords.GetHashCode();
		}

		// Token: 0x060037A0 RID: 14240
		public Pickup(string _type, string _id, int _coords)
		{
			this.type = _type;
			this.id = _id;
			this.coords = _coords;
		}

		// Token: 0x060037A1 RID: 14241
		public Pickup(RandomizerAction action, int _coords)
		{
			this.type = action.Action;
			this.id = (Randomizer.StringKeyPickupTypes.Contains(this.type) ? ((string)action.Value) : ((int)action.Value).ToString());
			this.coords = _coords;
		}

		// Token: 0x060037A2 RID: 14242
		public Uri GetURL()
		{
			string cleaned_id = this.id.Replace("#","");
			if(cleaned_id.Contains("\\"))
				cleaned_id = cleaned_id.Split('\\')[0];
			string url = RootUrl + "/found/" + this.coords + "/" + this.type + "/" + cleaned_id;
			url += "?zone=" + RandomizerStatsManager.CurrentZone();

			return new Uri(url);
		}

		// Token: 0x0400327B RID: 12923
		public string id;

		// Token: 0x0400327C RID: 12924
		public string type;

		// Token: 0x0400327D RID: 12925
		public int coords;
	}

	// Token: 0x02000A01 RID: 2561
	public class SkillInfoLine
	{
		// Token: 0x060037A3 RID: 14243
		public SkillInfoLine(int _id, int _bit, AbilityType _skill)
		{
			this.bit = _bit;
			this.id = _id;
			this.skill = _skill;
		}

		// Token: 0x060037A4 RID: 14244
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			SkillInfoLine skillInfoLine = (SkillInfoLine)obj;
			return this.bit == skillInfoLine.bit && this.id == skillInfoLine.id && this.skill == skillInfoLine.skill;
		}

		// Token: 0x060037A5 RID: 14245
		public override int GetHashCode()
		{
			return this.skill.GetHashCode() ^ this.id.GetHashCode() ^ this.bit.GetHashCode();
		}

		public int id;
		public int bit;
		public AbilityType skill;
	}

	// Token: 0x02000A02 RID: 2562
	// (Invoke) Token: 0x060037A7 RID: 14247
	public delegate int UpgradeCounter();

	// Token: 0x02000A03 RID: 2563
	public class UpgradeInfoLine
	{
		// Token: 0x060037AA RID: 14250
		public UpgradeInfoLine(int _id, int _bit, bool _stacks, UpgradeCounter _counter)
		{
			this.bit = _bit;
			this.id = _id;
			this.stacks = _stacks;
			this.counter = _counter;
		}

		// Token: 0x060037AB RID: 14251
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			UpgradeInfoLine upgradeInfoLine = (UpgradeInfoLine)obj;
			return this.bit == upgradeInfoLine.bit && this.id == upgradeInfoLine.id;
		}

		// Token: 0x060037AC RID: 14252
		public override int GetHashCode()
		{
			return this.bit.GetHashCode() ^ this.id.GetHashCode();
		}

		// Token: 0x04003281 RID: 12929
		public int id;

		// Token: 0x04003282 RID: 12930
		public int bit;

		// Token: 0x04003283 RID: 12931
		public bool stacks;

		// Token: 0x04003284 RID: 12932
		public UpgradeCounter counter;
	}

	// Token: 0x02000A04 RID: 2564
	// (Invoke) Token: 0x060037AE RID: 14254
	public delegate bool EventChecker();

	// Token: 0x02000A05 RID: 2565
	public class EventInfoLine
	{
		// Token: 0x060037B1 RID: 14257
		public EventInfoLine(int _id, int _bit, EventChecker _checker)
		{
			this.bit = _bit;
			this.id = _id;
			this.checker = _checker;
		}

		// Token: 0x060037B2 RID: 14258
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			EventInfoLine eventInfoLine = (EventInfoLine)obj;
			return this.bit == eventInfoLine.bit && this.id == eventInfoLine.id;
		}

		// Token: 0x060037B3 RID: 14259
		public override int GetHashCode()
		{
			return this.bit.GetHashCode() ^ this.id.GetHashCode();
		}

		// Token: 0x04003285 RID: 12933
		public int id;

		// Token: 0x04003286 RID: 12934
		public EventChecker checker;

		// Token: 0x04003287 RID: 12935
		public int bit;
	}

	// Token: 0x02000A06 RID: 2566
	public class TeleportInfoLine
	{
		// Token: 0x060037B4 RID: 14260
		public TeleportInfoLine(string _id, int _bit)
		{
			this.bit = _bit;
			this.id = _id;
		}

		// Token: 0x060037B5 RID: 14261
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			TeleportInfoLine teleportInfoLine = (TeleportInfoLine)obj;
			return this.bit == teleportInfoLine.bit && this.id == teleportInfoLine.id;
		}

		// Token: 0x060037B6 RID: 14262
		public override int GetHashCode()
		{
			return this.bit.GetHashCode() ^ this.id.GetHashCode();
		}

		// Token: 0x04003288 RID: 12936
		public string id;

		// Token: 0x04003289 RID: 12937
		public int bit;
	}
}
