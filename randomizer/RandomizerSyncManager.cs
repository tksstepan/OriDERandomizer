using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using Game;
using Sein.World;
using UnityEngine;

public static class RandomizerSyncManager
{
	public static void Initialize()
	{
		Countdown = 60;
		webClient = new WebClient();
		webClient.DownloadStringCompleted += RetryOnFail;
		getClient = new WebClient();
		getClient.UploadValuesCompleted += CheckPickups;
		if (CurrentSignals == null)
			CurrentSignals = new HashSet<String>();
		if (PickupQueue == null)
			PickupQueue = new Queue<Pickup>();
		SeedSent = false;
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
		TeleportInfos.Add(new TeleportInfoLine("Blackroot", 8));
		TeleportInfos.Add(new TeleportInfoLine("Glades", 9));
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
				NameValueCollection nvc = new NameValueCollection();
				Vector3 pos = Characters.Sein.Position;
				nvc["x"] = pos.x.ToString();
				nvc["y"] = pos.y.ToString();
				for(int i = 0; i < 8; i++) {
					nvc["seen_" + i.ToString()] = fixInt(Characters.Sein.Inventory.GetRandomizerItem(1560+i));
					nvc["have_" + i.ToString()] = fixInt(Characters.Sein.Inventory.GetRandomizerItem(930+i));
				}
				Uri uri = new Uri(RootUrl + "/tick/"); 
				getClient.UploadValuesAsync(uri, nvc);
			}
		} catch(Exception e) {
			Randomizer.LogError("RSM.Update: " + e.Message);
		}

	}

	public static void UploadSeed()
	{
		try
		{
			string[] array = File.ReadAllLines(Randomizer.SeedFilePath);
			array[0] = array[0].Replace(',', '|');
			NameValueCollection nvc = new NameValueCollection();
			nvc.Set("seed", string.Join(",", array).Replace("#",""));
			nvc.Set("version", Randomizer.VERSION);
			var client = new WebClient();
			client.UploadValuesAsync(new Uri(RootUrl + "/setSeed"), nvc);
			SeedSent = true;			
		} catch(Exception e) {
			Randomizer.LogError("UploadSeed: " + e.Message);
		}
	}

	public static bool getBit(int bf, int bit)
	{
		return 1 == (bf >> bit & 1);
	}

	public static int getTaste(int bf, int taste)
	{
		return bf >> 2 * taste & 3;
	}

	public static void CheckPickups(object sender, UploadValuesCompletedEventArgs e)
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
				if(!Characters.Sein)
					return;
				string[] array = System.Text.Encoding.UTF8.GetString(e.Result).Split(new char[]
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
							if(id >= 900) {
								if(id < 910) {
									int tree = id-899;
									string treeName =  RandomizerTrackedDataManager.Trees[tree];
									if(RandomizerTrackedDataManager.SetTree(tree))
										Randomizer.showHint(treeName +  " tree (activated by teammate)");
								} else if(id < 922) {
									string relicZone = RandomizerTrackedDataManager.Zones[id-911];
									if(RandomizerTrackedDataManager.SetRelic(relicZone))
										Randomizer.showHint("#" + relicZone + " relic# (found by teammate)", 300);
								}
							} else if(!RandomizerBonusSkill.UnlockedBonusSkills.ContainsValue(id) && cnt > 0) {
								RandomizerBonus.UpgradeID(id);
							}
						} else if(RandomizerBonus.UpgradeCount(id) < cnt) {
							RandomizerBonus.UpgradeID(id);
						} else if(!PickupQueue.Where((Pickup p) => p.type == "RB" && p.id == splitpair[0]).Any() && RandomizerBonus.UpgradeCount(id) > cnt) {
							RandomizerBonus.UpgradeID(-id);
						}
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
							if(!RandomizerBonusSkill.UnlockCreditWarp(text.Substring(4)))
							{
								Randomizer.Print(text.Substring(4), 10, false, true, false, false);
								RandomizerStatsManager.WriteStatsFile();
							}
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
			Randomizer.LogError("CheckPickups threw error: " + e2.Message);
		}
	}

	public static void RetryOnFail(object sender, DownloadStringCompletedEventArgs e)
	{
		int ln = 0;
		try
		{
			if(SendingPickup == null)
			{
				Randomizer.log("Error: no sending pickup found!");
				return;
			}
			ln = 1;
			if (e.Cancelled || e.Error != null)
			{
				ln = 2;
				if (e.Error is WebException we && we.Response != null)
				{
					ln = 3;
					HttpStatusCode statusCode = ((HttpWebResponse)we.Response).StatusCode;
					ln = 4;
					if (statusCode == HttpStatusCode.Gone) {
						ln = 5;
						if (SendingPickup.type == "RB")
						{
							ln = 6;
							RandomizerBonus.UpgradeID(-int.Parse(SendingPickup.id));
						}
					} else if (statusCode != HttpStatusCode.NotAcceptable) {
						webClient.DownloadStringAsync(SendingPickup.GetURL());
						return;
					}
					SendingPickup = null;
					return;
				}
				if(e.Error != null)
					Randomizer.log($"RetryOnFail (ln: {ln}) got responseless excpetion: {e}");
			}
			SendingPickup = null;
		} catch(Exception ee) {
			Randomizer.LogError($"RetryOnFail: {ee.Message}, e: {e}, ln {ln}");
			if(ee.Message == "Object reference not set to an instance of an object") {
				Randomizer.printInfo("Strange Network Error! Ping Eiko in the ori discord if you see this");
				SendingPickup = null;
			}
		}
	}

	public static void FoundPickup(RandomizerAction action, int coords)
	{
		try {
			Pickup pickup = new Pickup(action, coords);
			PickupQueue.Enqueue(pickup);
		} catch(Exception e) {
			Randomizer.LogError("FoundPickup: " + e.Message);
		}
	}

	public static void FoundTP(string identifier) {
		if(!Randomizer.Sync)
			return;
		try
		{
			if(TPIds.ContainsKey(identifier) && !isTeleporterActivated(identifier, false))
				FoundPickup(TPIds[identifier], -1);
		}
		catch (Exception e)
		{
			Randomizer.LogError("FoundTP: " + e.Message);
		}
	}

	public static bool isTeleporterActivated(string identifier)
	{
		return isTeleporterActivated(identifier, true);
	}

	public static bool isTeleporterActivated(string identifier, bool translate)
	{
		if(translate)
			identifier = Randomizer.TeleportTable[identifier].ToString();
		try{
			if(Characters.Sein && Characters.Sein.Inventory)
			{
				if(identifier == "ginsoTree" && Characters.Sein.Inventory.GetRandomizerItem(1024) == 1)
					return true;
				if(identifier == "forlorn" && Characters.Sein.Inventory.GetRandomizerItem(1025) == 1)
					return true;
				if(identifier == "mountHoru" && Characters.Sein.Inventory.GetRandomizerItem(1026) == 1)
					return true;			
			}
			foreach (GameMapTeleporter gameMapTeleporter in TeleporterController.Instance.Teleporters)
			{
				if (gameMapTeleporter.Identifier == identifier)
				{
					return gameMapTeleporter.Activated;
				}
			}
		}
		catch (Exception e)
		{
			Randomizer.LogError("IsTPActive: " + identifier + " " + e.Message +". Not criticial unless repeating.");
		}
		return false;
	}

	public static Pickup SendingPickup;

	public static string RootUrl;

	public static int Countdown;

	public static int PERIOD = 1;

	public static WebClient webClient;

	public static WebClient getClient;

	public static List<SkillInfoLine> SkillInfos;

	public static List<EventInfoLine> EventInfos;

	public static List<TeleportInfoLine> TeleportInfos;

	public static int ChaosTimeoutCounter = 0;

	public static Queue<Pickup> PickupQueue;

	public static bool SeedSent;

	public static HashSet<string> CurrentSignals;

	public static bool NetworkFree {
		get { return Randomizer.SyncId == "" || (PickupQueue.Count == 0 && SendingPickup == null && !webClient.IsBusy); }
	}

	public static string fixInt(int stupidFuckingSignedInt) {
		if(stupidFuckingSignedInt < 0) {
			var unsignedVer = BitConverter.ToUInt32(BitConverter.GetBytes(stupidFuckingSignedInt), 0);
			return unsignedVer.ToString();
		}
		return stupidFuckingSignedInt.ToString();
	}

	public static Dictionary<string, RandomizerAction> TPIds = new Dictionary<string, RandomizerAction>() {
		{"swamp", new RandomizerAction("TP", "Swamp")},
		{"sorrowPass", new RandomizerAction("TP", "Valley")},
		{"moonGrotto", new RandomizerAction("TP", "Grotto")},
		{"valleyOfTheWind", new RandomizerAction("TP", "Sorrow")},
		{"spiritTree", new RandomizerAction("TP", "Grove")},
		{"ginsoTree", new RandomizerAction("TP", "Ginso")},
		{"forlorn", new RandomizerAction("TP", "Forlorn")},
		{"mountHoru", new RandomizerAction("TP", "Horu")},
		{"mangroveFalls", new RandomizerAction("TP", "Blackroot")},
	};

	public class Pickup
	{
		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			Pickup pickup = (Pickup)obj;
			return this.type == pickup.type && this.id == pickup.id && this.coords == pickup.coords;
		}

		public override int GetHashCode()
		{
			return (this.type + this.id).GetHashCode() ^ this.coords.GetHashCode();
		}

		public Pickup(string _type, string _id, int _coords)
		{
			this.type = _type;
			this.id = _id;
			this.coords = _coords;
		}

		public Pickup(RandomizerAction action, int _coords)
		{
			this.type = action.Action;
			this.id = (Randomizer.StringKeyPickupTypes.Contains(this.type) ? ((string)action.Value) : ((int)action.Value).ToString());
			this.coords = _coords;
		}

		public Uri GetURL()
		{
			string cleaned_id = this.id.Replace("#","");
			if(cleaned_id.Contains("\\"))
				cleaned_id = cleaned_id.Split('\\')[0];
			string url = RootUrl + "/found/" + this.coords + "/" + this.type + "/" + cleaned_id;
			url += "?zone=" + RandomizerStatsManager.CurrentZone();

			return new Uri(url);
		}

		public string id;

		public string type;

		public int coords;
		private string urlGarbage;
	}

	public class SkillInfoLine
	{
		public SkillInfoLine(int _id, int _bit, AbilityType _skill)
		{
			this.bit = _bit;
			this.id = _id;
			this.skill = _skill;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			SkillInfoLine skillInfoLine = (SkillInfoLine)obj;
			return this.bit == skillInfoLine.bit && this.id == skillInfoLine.id && this.skill == skillInfoLine.skill;
		}

		public override int GetHashCode()
		{
			return this.skill.GetHashCode() ^ this.id.GetHashCode() ^ this.bit.GetHashCode();
		}

		public int id;
		public int bit;
		public AbilityType skill;
	}

	public delegate int UpgradeCounter();

	public class UpgradeInfoLine
	{
		public UpgradeInfoLine(int _id, int _bit, bool _stacks, UpgradeCounter _counter)
		{
			this.bit = _bit;
			this.id = _id;
			this.stacks = _stacks;
			this.counter = _counter;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			UpgradeInfoLine upgradeInfoLine = (UpgradeInfoLine)obj;
			return this.bit == upgradeInfoLine.bit && this.id == upgradeInfoLine.id;
		}

		public override int GetHashCode()
		{
			return this.bit.GetHashCode() ^ this.id.GetHashCode();
		}

		public int id;

		public int bit;

		public bool stacks;

		public UpgradeCounter counter;
	}

	public delegate bool EventChecker();

	public class EventInfoLine
	{
		public EventInfoLine(int _id, int _bit, EventChecker _checker)
		{
			this.bit = _bit;
			this.id = _id;
			this.checker = _checker;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			EventInfoLine eventInfoLine = (EventInfoLine)obj;
			return this.bit == eventInfoLine.bit && this.id == eventInfoLine.id;
		}

		public override int GetHashCode()
		{
			return this.bit.GetHashCode() ^ this.id.GetHashCode();
		}

		public int id;

		public EventChecker checker;

		public int bit;
	}

	public class TeleportInfoLine
	{
		public TeleportInfoLine(string _id, int _bit)
		{
			this.bit = _bit;
			this.id = _id;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || base.GetType() != obj.GetType())
			{
				return false;
			}
			TeleportInfoLine teleportInfoLine = (TeleportInfoLine)obj;
			return this.bit == teleportInfoLine.bit && this.id == teleportInfoLine.id;
		}

		public override int GetHashCode()
		{
			return this.bit.GetHashCode() ^ this.id.GetHashCode();
		}

		public string id;

		public int bit;
	}
}