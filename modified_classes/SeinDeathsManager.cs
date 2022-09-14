using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class SeinDeathsManager : SaveSerialize
{
	[ContextMenu("Fake a death here")]
	public void FakeADeathHere()
	{
		this.RecordDeath();
	}

	public override void Awake()
	{
		base.Awake();
		SeinDeathsManager.Instance = this;
		Events.Scheduler.OnGameReset.Add(new Action(this.OnGameReset));
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SeinDeathsManager.Instance == this)
		{
			SeinDeathsManager.Instance = null;
		}
		Events.Scheduler.OnGameReset.Remove(new Action(this.OnGameReset));
	}

	public void OnGameReset()
	{
		this.Deaths.Clear();
	}

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

	public static void OnDeath()
	{
		Randomizer.OnDeath();
		if (SeinDeathsManager.Instance && DifficultyController.Instance.Difficulty == DifficultyMode.OneLife)
		{
			SeinDeathsManager.Instance.Deaths.Clear();
			SeinDeathsManager.Instance.RecordDeath();
		}
	}

	public void RecordDeath()
	{
		Vector3 position = Characters.Sein.Position;
		int gameTimeInSeconds = GameController.Instance.GameTimeInSeconds;
		int completionPercentage = GameWorld.Instance.CompletionPercentage;
		int count = this.Deaths.Count;
		this.Deaths.Add(new DeathInformation(position, gameTimeInSeconds, completionPercentage, count));
		SaveSceneManager.Master.Save(Game.Checkpoint.SaveGameData.Master, SeinDeathsManager.Instance);
	}

	public static SeinDeathsManager Instance;

	public List<DeathInformation> Deaths = new List<DeathInformation>();
}
