using System;
using System.Collections;
using System.IO;
using Core;
using Game;
using Sein.World;
using UnityEngine;

// Token: 0x020009EF RID: 2543
public static class Randomizer
{
	// Token: 0x0600373A RID: 14138
	public static void initialize()
	{
		Randomizer.OHKO = false;
		Randomizer.ZeroXP = false;
		Randomizer.Sync = false;
		Randomizer.BonusActive = true;
		Randomizer.GiveAbility = false;
		Randomizer.Chaos = false;
		Randomizer.ChaosVerbose = false;
		Randomizer.Returning = false;
		RandomizerChaosManager.initialize();
		RandomizerSyncManager.Initialize();
		Randomizer.DamageModifier = 1f;
		Randomizer.Table = new Hashtable();
		Randomizer.GridFactor = 4.0;
		Randomizer.Message = "Good luck on your rando!";
		Randomizer.MessageProvider = (RandomizerMessageProvider)ScriptableObject.CreateInstance(typeof(RandomizerMessageProvider));
		Randomizer.ProgressiveMapStones = true;
		Randomizer.ForceTrees = false;
		Randomizer.CluesMode = false;
		Randomizer.SeedMeta = "";
		Randomizer.MistySim = new WorldEvents();
		Randomizer.MistySim.MoonGuid = new MoonGuid(1061758509, 1206015992, 824243626, -2026069462);
		Randomizer.TeleportTable = new Hashtable();
		Randomizer.TeleportTable["Forlorn"] = "forlorn";
		Randomizer.TeleportTable["Grotto"] = "moonGrotto";
		Randomizer.TeleportTable["Sorrow"] = "valleyOfTheWind";
		Randomizer.TeleportTable["Grove"] = "spiritTree";
		Randomizer.TeleportTable["Swamp"] = "swamp";
		Randomizer.TeleportTable["Valley"] = "sorrowPass";
		Randomizer.ColorShift = false;
		Randomizer.MessageQueue = new Queue();
		Randomizer.MessageQueueTime = 0;
		RandomizerRebinding.ParseRebinding();
		if (File.Exists("randomizer.dat"))
		{
			string[] array = File.ReadAllLines("randomizer.dat");
			string[] array3 = array[0].Split(new char[]
			{
				'|'
			})[0].Split(new char[]
			{
				','
			});
			Randomizer.SeedMeta = array[0];
			foreach (string text in array3)
			{
				if (text.ToLower() == "ohko")
				{
					Randomizer.OHKO = true;
				}
				if (text.ToLower().StartsWith("sync"))
				{
					Randomizer.Sync = true;
					Randomizer.SyncId = text.Substring(4);
				}
				if (text.ToLower() == "0xp")
				{
					Randomizer.ZeroXP = true;
				}
				if (text.ToLower() == "nobonus")
				{
					Randomizer.BonusActive = false;
				}
				if (text.ToLower() == "nonprogressivemapstones")
				{
					Randomizer.ProgressiveMapStones = false;
				}
				if (text.ToLower() == "forcetrees")
				{
					Randomizer.ForceTrees = true;
				}
				if (text.ToLower() == "clues")
				{
					Randomizer.CluesMode = true;
					RandomizerClues.initialize();
				}
			}
			for (int i = 1; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(new char[]
				{
					'|'
				});
				int num;
				int.TryParse(array2[0], out num);
				if (array2[1] == "TP")
				{
					Randomizer.Table[num] = new RandomizerAction(array2[1], array2[2]);
				}
				else
				{
					int num2;
					int.TryParse(array2[2], out num2);
					Randomizer.Table[num] = new RandomizerAction(array2[1], num2);
					if (Randomizer.CluesMode && array2[1] == "EV" && num2 % 2 == 0)
					{
						RandomizerClues.AddClue(array2[3], num2 / 2);
					}
				}
			}
		}
	}

	// Token: 0x0600373B RID: 14139
	public static void getPickup()
	{
		Randomizer.getPickup(Characters.Sein.Position);
	}

	// Token: 0x0600373C RID: 14140
	public static void returnToStart()
	{
		if (Characters.Sein.Abilities.Carry.IsCarrying || !Characters.Sein.Controller.CanMove || !Characters.Sein.Active)
		{
			return;
		}
		if (Items.NightBerry != null)
		{
			Items.NightBerry.transform.position = new Vector3(-755f, -400f);
		}
		Randomizer.Returning = true;
		Characters.Sein.Position = new Vector3(189f, -210f);
		int value = World.Events.Find(Randomizer.MistySim).Value;
		if (value != 1 && value != 8)
		{
			World.Events.Find(Randomizer.MistySim).Value = 10;
		}
	}

	// Token: 0x0600373D RID: 14141
	public static void getEvent(int ID)
	{
		RandomizerBonus.CollectPickup();
		if (Randomizer.ColorShift)
		{
			Randomizer.changeColor();
		}
		RandomizerSwitch.GivePickup((RandomizerAction)Randomizer.Table[ID * 4], ID * 4, true);
	}

	// Token: 0x0600373E RID: 14142
	public static void showHint(string message)
	{
		Randomizer.Message = message;
		Randomizer.MessageQueue.Enqueue(message);
	}

	// Token: 0x0600373F RID: 14143
	public static void playLastMessage()
	{
		Randomizer.MessageQueue.Enqueue(Randomizer.Message);
	}

	// Token: 0x06003740 RID: 14144
	public static void log(string message)
	{
		StreamWriter streamWriter = File.AppendText("randomizer.log");
		streamWriter.WriteLine(message);
		streamWriter.Flush();
		streamWriter.Dispose();
	}

	// Token: 0x06003741 RID: 14145
	public static bool WindRestored()
	{
		return Sein.World.Events.WindRestored && Scenes.Manager.CurrentScene != null && Scenes.Manager.CurrentScene.Scene != "forlornRuinsResurrection" && Scenes.Manager.CurrentScene.Scene != "forlornRuinsRotatingLaserFlipped";
	}

	// Token: 0x06003742 RID: 14146
	public static void getSkill()
	{
		Characters.Sein.Inventory.SkillPointsCollected += 134217728;
		Randomizer.getPickup();
	}

	// Token: 0x06003743 RID: 14147
	public static void hintAndLog(float x, float y)
	{
		string message = ((int)x).ToString() + " " + ((int)y).ToString();
		Randomizer.showHint(message);
		Randomizer.log(message);
	}

	// Token: 0x06003744 RID: 14148
	public static void getPickup(Vector3 position)
	{
		RandomizerBonus.CollectPickup();
		if (Randomizer.ColorShift)
		{
			Randomizer.changeColor();
		}
		int num = (int)(Math.Floor((double)((int)position.x) / Randomizer.GridFactor) * Randomizer.GridFactor) * 10000 + (int)(Math.Floor((double)((int)position.y) / Randomizer.GridFactor) * Randomizer.GridFactor);
		if (Randomizer.Table.ContainsKey(num))
		{
			RandomizerSwitch.GivePickup((RandomizerAction)Randomizer.Table[num], num, true);
			return;
		}
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (Randomizer.Table.ContainsKey(num + (int)Randomizer.GridFactor * (10000 * i + j)))
				{
					RandomizerSwitch.GivePickup((RandomizerAction)Randomizer.Table[num + (int)Randomizer.GridFactor * (10000 * i + j)], num, true);
					return;
				}
			}
		}
		for (int k = -2; k <= 2; k += 4)
		{
			for (int l = -1; l <= 1; l++)
			{
				if (Randomizer.Table.ContainsKey(num + (int)Randomizer.GridFactor * (10000 * k + l)))
				{
					RandomizerSwitch.GivePickup((RandomizerAction)Randomizer.Table[num + (int)Randomizer.GridFactor * (10000 * k + l)], num, true);
					return;
				}
			}
		}
		Randomizer.showHint("Error finding pickup at " + ((int)position.x).ToString() + ", " + ((int)position.y).ToString());
	}

	// Token: 0x06003745 RID: 14149
	public static void Update()
	{
		Randomizer.UpdateMessages();
		if (Characters.Sein && !Characters.Sein.IsSuspended)
		{
			Characters.Sein.Mortality.Health.GainHealth((float)RandomizerBonus.HealthRegeneration() * (Characters.Sein.PlayerAbilities.HealthEfficiency.HasAbility ? 0.0016f : 0.0008f));
			Characters.Sein.Energy.Gain((float)RandomizerBonus.EnergyRegeneration() * (Characters.Sein.PlayerAbilities.EnergyEfficiency.HasAbility ? 0.0003f : 0.0002f));
			if (Randomizer.ForceTrees && Scenes.Manager.CurrentScene != null && Scenes.Manager.CurrentScene.Scene == "catAndMouseResurrectionRoom" && RandomizerBonus.SkillTreeProgression() < 10)
			{
				Characters.Sein.Position = new Vector3(20f, 105f);
			}
			if (Randomizer.Chaos)
			{
				RandomizerChaosManager.Update();
			}
			if (Randomizer.Sync)
			{
				RandomizerSyncManager.Update();
			}
			if (Randomizer.Returning)
			{
				Characters.Sein.Position = new Vector3(189f, -215f);
				Randomizer.Returning = false;
			}
		}
		if (MoonInput.GetKey(KeyCode.LeftAlt) || MoonInput.GetKey(KeyCode.RightAlt))
		{
			if (MoonInput.GetKeyDown(RandomizerRebinding.ReplayMessage))
			{
				Randomizer.playLastMessage();
				return;
			}
			if (MoonInput.GetKeyDown(RandomizerRebinding.ReturnToStart) && Characters.Sein)
			{
				Randomizer.returnToStart();
				return;
			}
			if (MoonInput.GetKeyDown(RandomizerRebinding.ReloadSeed))
			{
				Randomizer.initialize();
				Randomizer.showSeedInfo();
				return;
			}
			if (MoonInput.GetKeyDown(RandomizerRebinding.ShowProgress) && Characters.Sein)
			{
				Randomizer.showProgress();
				return;
			}
			if (MoonInput.GetKeyDown(RandomizerRebinding.ColorShift))
			{
				string obj = "Color shift enabled";
				if (Randomizer.ColorShift)
				{
					obj = "Color shift disabled";
				}
				Randomizer.ColorShift = !Randomizer.ColorShift;
				Randomizer.MessageQueue.Enqueue(obj);
			}
			if (MoonInput.GetKeyDown(RandomizerRebinding.ToggleChaos) && Characters.Sein)
			{
				if (Randomizer.Chaos)
				{
					Randomizer.showChaosMessage("Chaos deactivated");
					Randomizer.Chaos = false;
					RandomizerChaosManager.ClearEffects();
					return;
				}
				Randomizer.showChaosMessage("Chaos activated");
				Randomizer.Chaos = true;
				return;
			}
			else if (MoonInput.GetKeyDown(RandomizerRebinding.ChaosVerbosity) && Randomizer.Chaos)
			{
				Randomizer.ChaosVerbose = !Randomizer.ChaosVerbose;
				if (Randomizer.ChaosVerbose)
				{
					Randomizer.showChaosMessage("Chaos messages enabled");
					return;
				}
				Randomizer.showChaosMessage("Chaos messages disabled");
				return;
			}
			else if (MoonInput.GetKeyDown(RandomizerRebinding.ForceChaosEffect) && Randomizer.Chaos && Characters.Sein)
			{
				RandomizerChaosManager.SpawnEffect();
				return;
			}
		}
	}

	// Token: 0x06003746 RID: 14150
	public static void showChaosEffect(string message)
	{
		if (Randomizer.ChaosVerbose)
		{
			Randomizer.MessageQueue.Enqueue(message);
		}
	}

	// Token: 0x06003747 RID: 14151
	public static void showChaosMessage(string message)
	{
		Randomizer.MessageQueue.Enqueue(message);
	}

	// Token: 0x06003748 RID: 14152
	public static void getMapStone()
	{
		if (!Randomizer.ProgressiveMapStones)
		{
			Randomizer.getPickup();
			return;
		}
		RandomizerBonus.CollectPickup();
		if (Randomizer.ColorShift)
		{
			Randomizer.changeColor();
		}
		Characters.Sein.Inventory.SkillPointsCollected += 8388608;
		RandomizerSwitch.GivePickup((RandomizerAction)Randomizer.Table[20 + RandomizerBonus.MapStoneProgression() * 4], 20 + RandomizerBonus.MapStoneProgression() * 4, true);
	}

	// Token: 0x06003749 RID: 14153
	public static void showProgress()
	{
		string text = "";
		if (RandomizerBonus.SkillTreeProgression() < 10)
		{
			text = text + "Trees (" + RandomizerBonus.SkillTreeProgression().ToString() + "/10)  ";
		}
		else
		{
			text += "$Trees (10/10)$  ";
		}
		text = text + "Maps (" + RandomizerBonus.MapStoneProgression().ToString() + "/9)  ";
		text = text + "Total (" + RandomizerBonus.GetPickupCount().ToString() + "/248)\n";
		if (Randomizer.CluesMode)
		{
			text += RandomizerClues.GetClues();
		}
		else
		{
			if (Keys.GinsoTree)
			{
				text += "*WV (3/3)*  ";
			}
			else
			{
				text = text + " *WV* (" + RandomizerBonus.WaterVeinShards().ToString() + "/3)  ";
			}
			if (Keys.ForlornRuins)
			{
				text += "#GS (3/3)#  ";
			}
			else
			{
				text = text + "#GS# (" + RandomizerBonus.GumonSealShards().ToString() + "/3)  ";
			}
			if (Keys.MountHoru)
			{
				text += "@SS (3/3)@";
			}
			else
			{
				text = text + " @SS@ (" + RandomizerBonus.SunstoneShards().ToString() + "/3)";
			}
		}
		Randomizer.MessageQueue.Enqueue(text);
	}

	// Token: 0x0600374A RID: 14154
	public static void showSeedInfo()
	{
		string obj = "v2.1 - seed loaded: " + Randomizer.SeedMeta;
		Randomizer.MessageQueue.Enqueue(obj);
	}

	// Token: 0x0600374B RID: 14155
	public static void changeColor()
	{
		if (Characters.Sein)
		{
			Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = new Color(FixedRandom.Values[0], FixedRandom.Values[1], FixedRandom.Values[2], 0.5f);
		}
	}

	// Token: 0x0600374C RID: 14156
	public static void UpdateMessages()
	{
		if (Randomizer.MessageQueueTime == 0)
		{
			if (Randomizer.MessageQueue.Count == 0)
			{
				return;
			}
			Randomizer.MessageProvider.SetMessage((string)Randomizer.MessageQueue.Dequeue());
			UI.Hints.Show(Randomizer.MessageProvider, HintLayer.GameSaved, 3f);
			Randomizer.MessageQueueTime = 60;
		}
		Randomizer.MessageQueueTime--;
	}

	// Token: 0x0400322B RID: 12843
	public static Hashtable Table;

	// Token: 0x0400322C RID: 12844
	public static bool GiveAbility;

	// Token: 0x0400322D RID: 12845
	public static double GridFactor;

	// Token: 0x0400322E RID: 12846
	public static RandomizerMessageProvider MessageProvider;

	// Token: 0x0400322F RID: 12847
	public static bool OHKO;

	// Token: 0x04003230 RID: 12848
	public static bool ZeroXP;

	// Token: 0x04003231 RID: 12849
	public static bool BonusActive;

	// Token: 0x04003232 RID: 12850
	public static string Message;

	// Token: 0x04003233 RID: 12851
	public static bool Chaos;

	// Token: 0x04003234 RID: 12852
	public static bool ChaosVerbose;

	// Token: 0x04003235 RID: 12853
	public static float DamageModifier;

	// Token: 0x04003236 RID: 12854
	public static bool ProgressiveMapStones;

	// Token: 0x04003237 RID: 12855
	public static bool ForceTrees;

	// Token: 0x04003238 RID: 12856
	public static string SeedMeta;

	// Token: 0x04003239 RID: 12857
	public static Hashtable TeleportTable;

	// Token: 0x0400323A RID: 12858
	public static WorldEvents MistySim;

	// Token: 0x0400323B RID: 12859
	public static bool Returning;

	// Token: 0x0400323C RID: 12860
	public static bool CluesMode;

	// Token: 0x0400323D RID: 12861
	public static bool ColorShift;

	// Token: 0x0400323E RID: 12862
	public static Queue MessageQueue;

	// Token: 0x0400323F RID: 12863
	public static int MessageQueueTime;

	// Token: 0x04003240 RID: 12864
	public static bool Sync;

	// Token: 0x0400353A RID: 13626
	public static string SyncId;
}
