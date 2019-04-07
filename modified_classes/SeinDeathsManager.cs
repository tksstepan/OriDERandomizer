using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

// Token: 0x0200031E RID: 798
public class SeinDeathsManager : SaveSerialize
{
	// Token: 0x0600108E RID: 4238
	[ContextMenu("Fake a death here")]
	public void FakeADeathHere()
	{
		this.RecordDeath();
	}

	// Token: 0x0600108F RID: 4239
	public override void Awake()
	{
		base.Awake();
		SeinDeathsManager.Instance = this;
		Events.Scheduler.OnGameReset.Add(new Action(this.OnGameReset));
	}

	// Token: 0x06001090 RID: 4240
	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SeinDeathsManager.Instance == this)
		{
			SeinDeathsManager.Instance = null;
		}
		Events.Scheduler.OnGameReset.Remove(new Action(this.OnGameReset));
	}

	// Token: 0x06001091 RID: 4241
	public void OnGameReset()
	{
		this.Deaths.Clear();
	}

	// Token: 0x06001092 RID: 4242
	public override void Serialize(Archive ar)
	{
		if (ar.Reading)
		{
			int num = ar.Serialize(0);
			this.Deaths.Clear();
			for (int i = 0; i < num; i++)
			{
				DeathInformation deathInformation = new DeathInformation();
				deathInformation.Serialize(ar);
				this.Deaths.Add(deathInformation);
			}
			DeathWispsManager.Refresh();
			return;
		}
		int count = this.Deaths.Count;
		ar.Serialize(count);
		for (int j = 0; j < count; j++)
		{
			this.Deaths[j].Serialize(ar);
		}
	}

	// Token: 0x06001093 RID: 4243
	public static void OnDeath()
	{
		Randomizer.OnDeath();
		if (SeinDeathsManager.Instance && DifficultyController.Instance.Difficulty == DifficultyMode.OneLife)
		{
			SeinDeathsManager.Instance.Deaths.Clear();
			SeinDeathsManager.Instance.RecordDeath();
		}
	}

	// Token: 0x06001094 RID: 4244
	public void RecordDeath()
	{
		Vector3 position = Characters.Sein.Position;
		int gameTimeInSeconds = GameController.Instance.GameTimeInSeconds;
		int completionPercentage = GameWorld.Instance.CompletionPercentage;
		int count = this.Deaths.Count;
		this.Deaths.Add(new DeathInformation(position, gameTimeInSeconds, completionPercentage, count));
		SaveSceneManager.Master.Save(Game.Checkpoint.SaveGameData.Master, SeinDeathsManager.Instance);
	}

	// Token: 0x04000FC7 RID: 4039
	public static SeinDeathsManager Instance;

	// Token: 0x04000FC8 RID: 4040
	public List<DeathInformation> Deaths = new List<DeathInformation>();
}
