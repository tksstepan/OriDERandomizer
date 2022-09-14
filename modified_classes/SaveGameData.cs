using System;
using System.Collections.Generic;
using System.IO;

public class SaveGameData
{
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

	public SaveScene Master
	{
		get
		{
			return this.InsertScene(MoonGuid.Empty);
		}
	}

	public SaveScene GetScene(MoonGuid sceneGuid)
	{
		SaveScene result;
		if (this.Scenes.TryGetValue(sceneGuid, out result))
		{
			return result;
		}
		return null;
	}

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

	public bool SceneExists(MoonGuid sceneGUID)
	{
		return this.Scenes.ContainsKey(sceneGUID);
	}

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

	public void ClearPendingScenes()
	{
		this.PendingScenes.Clear();
	}

	public void ClearAllData()
	{
		this.Scenes.Clear();
		this.PendingScenes.Clear();
	}

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

	
	public const int DATA_VERSION = 1;

	private const string FILE_FORMAT_STRING = "SaveGameData";

	public readonly Dictionary<MoonGuid, SaveScene> Scenes = new Dictionary<MoonGuid, SaveScene>();

	public readonly Dictionary<MoonGuid, SaveScene> PendingScenes = new Dictionary<MoonGuid, SaveScene>();

	public static int CurrentSaveFileVersion = -1;
}
