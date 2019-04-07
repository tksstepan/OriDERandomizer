using System;
using System.Collections.Generic;
using System.IO;
using Core;
using Game;

// Token: 0x0200097F RID: 2431
[Serializable]
public class SaveGameController
{
	// Token: 0x0600352D RID: 13613 RVA: 0x00002517 File Offset: 0x00000717
	public SaveGameController()
	{
	}

	// Token: 0x17000849 RID: 2121
	// (get) Token: 0x0600352E RID: 13614 RVA: 0x00029BF1 File Offset: 0x00027DF1
	public int CurrentSlotIndex
	{
		get
		{
			return SaveSlotsManager.CurrentSlotIndex;
		}
	}

	// Token: 0x1700084A RID: 2122
	// (get) Token: 0x0600352F RID: 13615 RVA: 0x00029BF8 File Offset: 0x00027DF8
	public int CurrentBackupIndex
	{
		get
		{
			return SaveSlotsManager.BackupIndex;
		}
	}

	// Token: 0x1700084B RID: 2123
	// (get) Token: 0x06003530 RID: 13616 RVA: 0x0000424E File Offset: 0x0000244E
	public bool SaveGameQueried
	{
		get
		{
			return true;
		}
	}

	// Token: 0x06003531 RID: 13617 RVA: 0x000D94E4 File Offset: 0x000D76E4
	public void SaveToFile(string filename)
	{
		using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)))
		{
			this.SaveToWriter(binaryWriter);
		}
	}

	// Token: 0x06003532 RID: 13618 RVA: 0x000D9524 File Offset: 0x000D7724
	public bool LoadFromFile(string filename)
	{
		bool result;
		using (BinaryReader binaryReader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
		{
			result = this.LoadFromReader(binaryReader);
		}
		return result;
	}

	// Token: 0x06003533 RID: 13619 RVA: 0x000D9568 File Offset: 0x000D7768
	public byte[] SaveToBytes()
	{
		MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			this.SaveToWriter(binaryWriter);
		}
		return memoryStream.ToArray();
	}

	// Token: 0x06003534 RID: 13620 RVA: 0x00029BFF File Offset: 0x00027DFF
	public void SaveToWriter(BinaryWriter writer)
	{
		SaveSlotsManager.CurrentSaveSlot.SaveToWriter(writer);
		Game.Checkpoint.SaveGameData.SaveToWriter(writer);
	}

	// Token: 0x1700084C RID: 2124
	// (get) Token: 0x06003535 RID: 13621 RVA: 0x000D95AC File Offset: 0x000D77AC
	private bool SaveWasOneLifeAndKilled
	{
		get
		{
			SaveSlotInfo currentSaveSlot = SaveSlotsManager.CurrentSaveSlot;
			return currentSaveSlot.Difficulty == DifficultyMode.OneLife && currentSaveSlot.WasKilled;
		}
	}

	// Token: 0x06003536 RID: 13622 RVA: 0x00029C17 File Offset: 0x00027E17
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

	// Token: 0x06003537 RID: 13623 RVA: 0x000D95D0 File Offset: 0x000D77D0
	public bool LoadFromBytes(byte[] binary)
	{
		bool result;
		using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(binary)))
		{
			result = this.LoadFromReader(binaryReader);
		}
		return result;
	}

	// Token: 0x06003538 RID: 13624 RVA: 0x000D9610 File Offset: 0x000D7810
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

	// Token: 0x1700084D RID: 2125
	// (get) Token: 0x06003539 RID: 13625 RVA: 0x000D966C File Offset: 0x000D786C
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

	// Token: 0x1700084E RID: 2126
	// (get) Token: 0x0600353A RID: 13626 RVA: 0x00029C4A File Offset: 0x00027E4A
	public string CurrentSaveFilePath
	{
		get
		{
			return this.GetSaveFilePath(this.CurrentSlotIndex, -1);
		}
	}

	// Token: 0x0600353B RID: 13627 RVA: 0x000D96D0 File Offset: 0x000D78D0
	public string GetSaveFilePath(int slotIndex, int backupIndex = -1)
	{
		if (backupIndex == -1)
		{
			return Path.Combine(OutputFolder.PlayerDataFolderPath, "saveFile" + slotIndex + ".sav");
		}
		return Path.Combine(OutputFolder.PlayerDataFolderPath, string.Format("saveFile{0}_bkup{1}.sav", slotIndex, backupIndex));
	}

	// Token: 0x0600353C RID: 13628 RVA: 0x00029C59 File Offset: 0x00027E59
	public void Refresh()
	{
		this.CanPerformLoad();
	}

	// Token: 0x0600353D RID: 13629 RVA: 0x00029C62 File Offset: 0x00027E62
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

	// Token: 0x0600353E RID: 13630 RVA: 0x00029C9E File Offset: 0x00027E9E
	public bool PerformLoadWithoutCheckpointRestore()
	{
		if (Recorder.IsPlaying)
		{
			return Recorder.Instance.OnPerformLoad();
		}
		return this.CanPerformLoad() && this.LoadFromFile(this.GetSaveFilePath(this.CurrentSlotIndex, this.CurrentBackupIndex));
	}

	// Token: 0x0600353F RID: 13631 RVA: 0x00029CD4 File Offset: 0x00027ED4
	public bool OnLoadComplete(byte[] buffer)
	{
		bool result = this.LoadFromBytes(buffer);
		this.RestoreCheckpoint();
		return result;
	}

	// Token: 0x06003540 RID: 13632 RVA: 0x000D9724 File Offset: 0x000D7924
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

	// Token: 0x06003541 RID: 13633 RVA: 0x00029CE3 File Offset: 0x00027EE3
	public bool CanPerformLoad()
	{
		return !GameController.Instance.IsDemo;
	}

	// Token: 0x06003542 RID: 13634 RVA: 0x00029CF2 File Offset: 0x00027EF2
	public bool CanPerformSave()
	{
		return !Recorder.IsPlaying && !GameController.Instance.IsDemo;
	}

	// Token: 0x06003543 RID: 13635 RVA: 0x000028FF File Offset: 0x00000AFF
	public void OnSaveComplete()
	{
	}

	// Token: 0x06003544 RID: 13636 RVA: 0x00029D0A File Offset: 0x00027F0A
	public void RestoreCheckpoint()
	{
		GameController.Instance.IsLoadingGame = true;
		LateStartHook.AddLateStartMethod(new Action(this.RestoreCheckpointPart1));
	}

	// Token: 0x06003545 RID: 13637 RVA: 0x000D9774 File Offset: 0x000D7974
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

	// Token: 0x06003546 RID: 13638 RVA: 0x00029D28 File Offset: 0x00027F28
	public void OnFinishedLoading()
	{
		GameController.Instance.MainMenuCanBeOpened = true;
		UI.Cameras.Current.Controller.PuppetController.Reset();
		GameController.Instance.RestoreCheckpointImmediate();
		Scenes.Manager.MarkActiveScenesAsKeepLoaded();
	}

	// Token: 0x04002FC1 RID: 12225
	public const int MAX_SAVES = 10;

	// Token: 0x04002FC2 RID: 12226
	private float m_startTime;
}
