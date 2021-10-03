using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Game;

[Serializable]
public class SaveGameController
{
	public int CurrentSlotIndex
	{
		get
		{
			return SaveSlotsManager.CurrentSlotIndex;
		}
	}

	public int CurrentBackupIndex
	{
		get
		{
			return SaveSlotsManager.BackupIndex;
		}
	}

	public bool SaveGameQueried
	{
		get
		{
			return true;
		}
	}

	public void SaveToFile(string filename)
	{
		using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
		{
			this.SaveToWriter(binaryWriter);
		}
	}

	public bool LoadFromFile(string filename)
	{
		bool result;
		using (BinaryReader binaryReader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
		{
			result = this.LoadFromReader(binaryReader);
		}
		return result;
	}

	public byte[] SaveToBytes()
	{
		MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			this.SaveToWriter(binaryWriter);
		}
		return memoryStream.ToArray();
	}

	public void SaveToWriter(BinaryWriter writer)
	{
		SaveSlotsManager.CurrentSaveSlot.SaveToWriter(writer);
		Game.Checkpoint.SaveGameData.SaveToWriter(writer);
	}

	private bool SaveWasOneLifeAndKilled
	{
		get
		{
			SaveSlotInfo currentSaveSlot = SaveSlotsManager.CurrentSaveSlot;
			return currentSaveSlot.Difficulty == DifficultyMode.OneLife && currentSaveSlot.WasKilled;
		}
	}

	public bool LoadFromReader(BinaryReader reader)
	{
		if (!SaveSlotsManager.CurrentSaveSlot.LoadFromReader(reader))
		{
			return false;
		}
		if (!Game.Checkpoint.SaveGameData.LoadFromReader(reader))
		{
			return false;
		}
		if (this.SaveWasOneLifeAndKilled)
		{
			SaveSceneManager.ClearSaveSlotForOneLife(Game.Checkpoint.SaveGameData);
		}
		return true;
	}

	public bool LoadFromBytes(byte[] binary)
	{
		bool result;
		using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(binary)))
		{
			result = this.LoadFromReader(binaryReader);
		}
		return result;
	}

	public bool SaveExists(int slotIndex)
	{
		if (!this.CanPerformLoad())
		{
			return false;
		}
		if (Recorder.Instance && Recorder.Instance.State == Recorder.RecorderState.Playing)
		{
			InputData frameDataOfType = Recorder.Instance.CurrentFrame.GetFrameDataOfType<InputData>();
			return frameDataOfType != null && frameDataOfType.SaveFileExists;
		}
		return File.Exists(this.GetSaveFilePath(slotIndex, -1));
	}

	public bool SaveFileExists
	{
		get
		{
			if (!this.CanPerformLoad())
			{
				return false;
			}
			if (Recorder.Instance && Recorder.Instance.State == Recorder.RecorderState.Playing)
			{
				List<InputData> frameData = Recorder.Instance.CurrentFrame.GetFrameData<InputData>();
				if (frameData != null)
				{
					InputData inputData = frameData[0];
					if (inputData != null)
					{
						return inputData.SaveFileExists;
					}
				}
				return false;
			}
			return File.Exists(this.CurrentSaveFilePath);
		}
	}

	public string CurrentSaveFilePath
	{
		get
		{
			return this.GetSaveFilePath(this.CurrentSlotIndex, -1);
		}
	}

	public string GetSaveFilePath(int slotIndex, int backupIndex = -1)
	{
		if (backupIndex == -1)
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, "saveFile" + slotIndex + ".sav");
		}
		return Path.Combine(OutputFolder.PlayerDataFolderPath, string.Format("saveFile{0}_bkup{1}.sav", slotIndex, backupIndex));
	}

	public void Refresh()
	{
		this.CanPerformLoad();
	}

	public bool PerformLoad()
	{
		if (Recorder.IsPlaying)
		{
			return Recorder.Instance.OnPerformLoad();
		}
		if (!this.CanPerformLoad())
		{
			return false;
		}
		bool result = this.LoadFromFile(this.GetSaveFilePath(this.CurrentSlotIndex, this.CurrentBackupIndex));
		this.RestoreCheckpoint();
		return result;
	}

	public bool PerformLoadWithoutCheckpointRestore()
	{
		if (Recorder.IsPlaying)
		{
			return Recorder.Instance.OnPerformLoad();
		}
		return this.CanPerformLoad() && this.LoadFromFile(this.GetSaveFilePath(this.CurrentSlotIndex, this.CurrentBackupIndex));
	}

	public bool OnLoadComplete(byte[] buffer)
	{
		bool result = this.LoadFromBytes(buffer);
		this.RestoreCheckpoint();
		return result;
	}

	public void PerformSave()
	{
		if (!this.CanPerformSave())
		{
			return;
		}
		Randomizer.OnSave();
		SaveSlotsManager.CurrentSaveSlot.FillData();
		SaveSlotsManager.BackupIndex = -1;
		this.SaveToFile(this.CurrentSaveFilePath);
		if (Recorder.IsRecordering)
		{
			Recorder.Instance.OnPerformSave();
		}
	}

	public bool CanPerformLoad()
	{
		return !GameController.Instance.IsDemo;
	}

	public bool CanPerformSave()
	{
		return !Recorder.IsPlaying && !GameController.Instance.IsDemo;
	}

	public void OnSaveComplete()
	{
	}

	public void RestoreCheckpoint()
	{
		GameController.Instance.IsLoadingGame = true;
		LateStartHook.AddLateStartMethod(new Action(this.RestoreCheckpointPart1));
	}

	public void RestoreCheckpointPart1()
	{
		GameController.Instance.IsLoadingGame = true;
		Game.Checkpoint.SaveGameData.ClearPendingScenes();
		HashSet<SaveSerialize> hashSet = new HashSet<SaveSerialize>();
		hashSet.Add(Scenes.Manager);
		hashSet.Add(GameController.Instance);
		hashSet.Add(SeinWorldState.Instance);
		SaveSceneManager.Master.Load(Game.Checkpoint.SaveGameData.Master, hashSet);
		Scenes.Manager.AutoLoadingUnloading = false;
		GoToSceneController.Instance.StartInScene = MoonGuid.Empty;
		Game.Checkpoint.SaveGameData.ClearPendingScenes();
		Scenes.Manager.MarkLoadingScenesAsCancel();
		if (this.SaveWasOneLifeAndKilled)
		{
			RuntimeSceneMetaData sceneInformation = Scenes.Manager.GetSceneInformation("sunkenGladesRunaway");
			GameController.Instance.RequireInitialValues = true;
			GameStateMachine.Instance.SetToGame();
			DifficultyController.Instance.ChangeDifficulty(DifficultyMode.OneLife);
			GoToSceneController.Instance.StartInScene = sceneInformation.SceneMoonGuid;
			GameController.Instance.IsLoadingGame = false;
			GoToSceneController.Instance.GoToSceneAsync(sceneInformation, new Action(this.OnFinishedLoading), false);
			return;
		}
		InstantLoadScenesController.Instance.OnScenesEnabledCallback = new Action(this.OnFinishedLoading);
		InstantLoadScenesController.Instance.LoadScenesAtPosition(null, true, false);
	}

	public void OnFinishedLoading()
	{
		GameController.Instance.MainMenuCanBeOpened = true;
		UI.Cameras.Current.Controller.PuppetController.Reset();
		GameController.Instance.RestoreCheckpointImmediate();
		Scenes.Manager.MarkActiveScenesAsKeepLoaded();
	}

	public const int MAX_SAVES = 10;

	private float m_startTime;
}
