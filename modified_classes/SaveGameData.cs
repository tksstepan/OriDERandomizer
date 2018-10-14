using System;
using System.Collections.Generic;
using System.IO;

// Token: 0x0200060C RID: 1548
public class SaveGameData
{
	// Token: 0x0600213D RID: 8509
	public SaveGameData()
	{
	}

	// Token: 0x0600213E RID: 8510
	static SaveGameData()
	{
	}

	// Token: 0x0600213F RID: 8511
	public void SaveToWriter(BinaryWriter writer)
	{
		SaveGameData.CurrentSaveFileVersion = 1;
		writer.Write("SaveGameData");
		writer.Write(1);
		writer.Write(this.Scenes.Count);
		foreach (SaveScene saveScene in this.Scenes.Values)
		{
			writer.Write(saveScene.SceneGUID.ToByteArray());
			writer.Write(saveScene.SaveObjects.Count);
			foreach (SaveObject saveObject in saveScene.SaveObjects)
			{
				writer.Write(saveObject.Id.ToByteArray());
				saveObject.Data.WriteMemoryStreamToBinaryWriter(writer);
			}
		}
		((IDisposable)writer).Dispose();
	}

	// Token: 0x06002140 RID: 8512
	public bool LoadFromReader(BinaryReader reader)
	{
		this.Scenes.Clear();
		this.PendingScenes.Clear();
		if (reader.ReadString() != "SaveGameData")
		{
			return false;
		}
		SaveGameData.CurrentSaveFileVersion = reader.ReadInt32();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			SaveScene saveScene = new SaveScene();
			saveScene.SceneGUID = new MoonGuid(reader.ReadBytes(16));
			this.Scenes.Add(saveScene.SceneGUID, saveScene);
			int num2 = reader.ReadInt32();
			for (int j = 0; j < num2; j++)
			{
				SaveObject item = new SaveObject(new MoonGuid(reader.ReadBytes(16)));
				item.Data.ReadMemoryStreamFromBinaryReader(reader);
				saveScene.SaveObjects.Add(item);
			}
		}
		return true;
	}

	// Token: 0x17000536 RID: 1334
	// (get) Token: 0x06002141 RID: 8513
	public SaveScene Master
	{
		get
		{
			return this.InsertScene(MoonGuid.Empty);
		}
	}

	// Token: 0x06002142 RID: 8514
	public SaveScene GetScene(MoonGuid sceneGuid)
	{
		SaveScene result;
		if (this.Scenes.TryGetValue(sceneGuid, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06002143 RID: 8515
	public SaveScene InsertScene(MoonGuid sceneGuid)
	{
		SaveScene saveScene;
		if (this.Scenes.TryGetValue(sceneGuid, out saveScene))
		{
			return saveScene;
		}
		saveScene = new SaveScene
		{
			SceneGUID = sceneGuid
		};
		this.Scenes.Add(saveScene.SceneGUID, saveScene);
		return saveScene;
	}

	// Token: 0x06002144 RID: 8516
	public SaveScene InsertPendingScene(MoonGuid sceneGUID)
	{
		SaveScene saveScene;
		if (this.PendingScenes.TryGetValue(sceneGUID, out saveScene))
		{
			return saveScene;
		}
		saveScene = new SaveScene
		{
			SceneGUID = sceneGUID
		};
		this.PendingScenes.Add(saveScene.SceneGUID, saveScene);
		return saveScene;
	}

	// Token: 0x06002145 RID: 8517
	public bool SceneExists(MoonGuid sceneGUID)
	{
		return this.Scenes.ContainsKey(sceneGUID);
	}

	// Token: 0x06002146 RID: 8518
	public void ApplyPendingScenes()
	{
		foreach (SaveScene saveScene in this.PendingScenes.Values)
		{
			if (this.SceneExists(saveScene.SceneGUID))
			{
				this.Scenes.Remove(saveScene.SceneGUID);
			}
			this.Scenes.Add(saveScene.SceneGUID, saveScene);
		}
		this.ClearPendingScenes();
	}

	// Token: 0x06002147 RID: 8519
	public void ClearPendingScenes()
	{
		this.PendingScenes.Clear();
	}

	// Token: 0x06002148 RID: 8520
	public void ClearAllData()
	{
		this.Scenes.Clear();
		this.PendingScenes.Clear();
	}

	// Token: 0x06003898 RID: 14488
	public void LoadCustomData(ArrayList data)
	{
		SaveScene saveScene = new SaveScene();
		saveScene.SceneGUID = (MoonGuid)data[0];
		this.Scenes.Add(saveScene.SceneGUID, saveScene);
		for (int i = 1; i < data.Count; i++)
		{
			SaveObject saveObject = new SaveObject((MoonGuid)((object[])data[i])[0]);
			byte[] array = (byte[])((object[])data[i])[1];
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(array));
			int num = array.Length;
			saveObject.Data.MemoryStream.SetLength((long)num);
			binaryReader.Read(saveObject.Data.MemoryStream.GetBuffer(), 0, num);
			saveScene.SaveObjects.Add(saveObject);
		}
	}

	
	// Token: 0x04001D3F RID: 7487
	public const int DATA_VERSION = 1;

	// Token: 0x04001D40 RID: 7488
	private const string FILE_FORMAT_STRING = "SaveGameData";

	// Token: 0x04001D41 RID: 7489
	public readonly Dictionary<MoonGuid, SaveScene> Scenes = new Dictionary<MoonGuid, SaveScene>();

	// Token: 0x04001D42 RID: 7490
	public readonly Dictionary<MoonGuid, SaveScene> PendingScenes = new Dictionary<MoonGuid, SaveScene>();

	// Token: 0x04001D43 RID: 7491
	public static int CurrentSaveFileVersion = -1;
}
