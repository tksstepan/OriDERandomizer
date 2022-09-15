using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class SaveSceneManager : MonoBehaviour
{
	public SaveSceneManager()
	{
	}

	// Note: this type is marked as 'beforefieldinit'.
	static SaveSceneManager()
	{
	}

	public static SaveSceneManager FromTransform(Transform transform)
	{
		SceneRoot sceneRoot = SceneRoot.FindFromTransform(transform);
		if (sceneRoot)
		{
			return sceneRoot.SaveSceneManager;
		}
		return null;
	}

	public void ReleaseNullReferences()
	{
		for (int i = 0; i < this.SaveData.Count; i++)
		{
			SaveSceneManager.SaveId saveId = this.SaveData[i];
			if (saveId.SaveObject == null)
			{
				saveId.SaveObject = null;
			}
		}
	}

	[ContextMenu("Print info")]
	public void PrintInfo()
	{
	}

	public void RegisterGameObject(GameObject go)
	{
		go.GetComponentsInChildren<SaveSerialize>(SaveSceneManager.s_saveSerializeList);
		for (int i = 0; i < SaveSceneManager.s_saveSerializeList.Count; i++)
		{
			SaveSceneManager.s_saveSerializeList[i].RegisterToSaveSceneManager(this);
		}
		SaveSceneManager.s_saveSerializeList.Clear();
	}

	public void UnregisterGameObject(GameObject go)
	{
		go.GetComponentsInChildren<SaveSerialize>(SaveSceneManager.s_saveSerializeList);
		for (int i = 0; i < SaveSceneManager.s_saveSerializeList.Count; i++)
		{
			SaveSceneManager.s_saveSerializableHashSet.Add(SaveSceneManager.s_saveSerializeList[i]);
		}
		this.SaveData.RemoveAll((SaveSceneManager.SaveId a) => SaveSceneManager.s_saveSerializableHashSet.Contains(a.Save));
		SaveSceneManager.s_saveSerializeList.Clear();
		SaveSceneManager.s_saveSerializableHashSet.Clear();
	}

	public ISerializable IdToSaveSerialize(MoonGuid id)
	{
		if (id == null)
		{
			return null;
		}
		for (int i = 0; i < this.SaveData.Count; i++)
		{
			SaveSceneManager.SaveId saveId = this.SaveData[i];
			if (saveId.Id == id)
			{
				return saveId.Save;
			}
		}
		return null;
	}

	public MoonGuid SaveSerializeToId(ISerializable saveSerialize)
	{
		if (saveSerialize == null)
		{
			return null;
		}
		for (int i = 0; i < this.SaveData.Count; i++)
		{
			SaveSceneManager.SaveId saveId = this.SaveData[i];
			if (saveId.Save == saveSerialize)
			{
				return saveId.Id;
			}
		}
		return MoonGuid.Empty;
	}

	public bool SaveSerializeIsRegistered(ISerializable serializable)
	{
		for (int i = 0; i < this.SaveData.Count; i++)
		{
			SaveSceneManager.SaveId saveId = this.SaveData[i];
			if (saveId.Save == serializable)
			{
				return true;
			}
		}
		return false;
	}

	public void AddSaveObject(ISerializable saveSerialize, MoonGuid guid)
	{
		SaveSceneManager.SaveId item = new SaveSceneManager.SaveId
		{
			Id = guid,
			Save = saveSerialize
		};
		this.SaveData.RemoveAll((SaveSceneManager.SaveId a) => a.Id == guid);
		this.SaveData.Add(item);
	}

	public static void RemoveSaveDataFromMaster(GameObject go)
	{
		go.GetComponentsInChildren<SaveSerialize>(SaveSceneManager.s_saveSerializeList);
		for (int i = 0; i < SaveSceneManager.s_saveSerializeList.Count; i++)
		{
			SaveSerialize saveSerialize = SaveSceneManager.s_saveSerializeList[i];
			MoonGuid moonGUID = MoonGuid.Empty;
			foreach (SaveSceneManager.SaveId saveId in SaveSceneManager.Master.SaveData)
			{
				if (saveId.Save == saveSerialize)
				{
					moonGUID = saveId.Id;
				}
			}
			if (moonGUID != MoonGuid.Empty)
			{
				Game.Checkpoint.SaveGameData.Master.SaveObjects.RemoveAll((SaveObject a) => a.Id == moonGUID);
			}
		}
		SaveSceneManager.s_saveSerializeList.Clear();
	}

	public void Save(SaveScene saveScene)
	{
		saveScene.SaveObjects.Clear();
		for (int i = 0; i < this.SaveData.Count; i++)
		{
			SaveSceneManager.SaveId saveId = this.SaveData[i];
			try
			{
				if (saveId.Save as Component != null)
				{
					SaveObject item = new SaveObject(saveId.Id);
					item.Data.WriteMode();
					saveId.Save.Serialize(item.Data);
					saveScene.SaveObjects.Add(item);
				}
			}
			catch (Exception ex)
			{
			}
		}
	}

	public void SaveWithoutClearing(SaveScene saveScene)
	{
		this.m_saveCache.Clear();
		for (int i = 0; i < saveScene.SaveObjects.Count; i++)
		{
			this.m_saveCache.Add(saveScene.SaveObjects[i].Id, saveScene.SaveObjects[i].Data);
		}
		for (int j = 0; j < this.SaveData.Count; j++)
		{
			SaveSceneManager.SaveId saveId = this.SaveData[j];
			try
			{
				if (saveId.Save as Component != null)
				{
					Archive archive;
					if (this.m_saveCache.TryGetValue(saveId.Id, out archive))
					{
						archive.WriteMode();
						saveId.Save.Serialize(archive);
					}
					else
					{
						SaveObject item = new SaveObject(saveId.Id);
						item.Data.WriteMode();
						saveId.Save.Serialize(item.Data);
						saveScene.SaveObjects.Add(item);
					}
				}
			}
			catch (Exception ex)
			{
			}
		}
		this.m_saveCache.Clear();
	}

	public void Save(SaveScene saveScene, ISerializable serializable)
	{
		MoonGuid moonGuid = this.SaveSerializeToId(serializable);
		bool flag = false;
		for (int i = 0; i < saveScene.SaveObjects.Count; i++)
		{
			if (moonGuid == saveScene.SaveObjects[i].Id)
			{
				Archive data = saveScene.SaveObjects[i].Data;
				data.WriteMode();
				serializable.Serialize(data);
				flag = true;
			}
		}
		if (!flag)
		{
			SaveObject item = new SaveObject(moonGuid);
			saveScene.SaveObjects.Add(item);
			Archive data2 = item.Data;
			data2.WriteMode();
			serializable.Serialize(data2);
		}
	}

	public void Load(SaveScene saveScene, HashSet<SaveSerialize> objects)
	{
		for (int i = 0; i < saveScene.SaveObjects.Count; i++)
		{
			SaveObject saveObject = saveScene.SaveObjects[i];
			ISerializable serializable = this.IdToSaveSerialize(saveObject.Id);
			try
			{
				SaveSerialize saveSerialize = serializable as SaveSerialize;
				if (saveSerialize != null && objects.Contains(saveSerialize))
				{
					saveObject.Data.ReadMode();
					serializable.Serialize(saveObject.Data);
				}
			}
			catch (Exception ex)
			{
			}
		}
		if (bootstrapHook != null) {
			try
			{
				bootstrapHook(sceneRoot);
			}
			catch (Exception ex)
			{
				Randomizer.log("Bootstrap exception: "+ ex.ToString());
			}
		}
	}

	public void Load(SaveScene saveScene)
	{
		for (int i = 0; i < saveScene.SaveObjects.Count; i++)
		{
			SaveObject saveObject = saveScene.SaveObjects[i];
			ISerializable serializable = this.IdToSaveSerialize(saveObject.Id);
			try
			{
				if (serializable as Component)
				{
					saveObject.Data.ReadMode();
					serializable.Serialize(saveObject.Data);
				}
			}
			catch (Exception ex)
			{
			}
		}
		if (bootstrapHook != null) {
			try
			{
				bootstrapHook(sceneRoot);
			}
			catch (Exception ex)
			{
				Randomizer.log("Bootstrap exception: "+ ex.ToString());
			}
		}
	}

	public void AddChildSaveSerializables()
	{
		this.SaveData.Clear();
		try
		{
			this.RegisterGameObject(base.gameObject);
		}
		catch (Exception ex)
		{
		}
	}

	public static void ClearSaveSlotForOneLife(SaveGameData data)
	{
		SaveObject item = default(SaveObject);
		if (SeinDeathsManager.Instance)
		{
			item = data.Master.SaveObjects.Find((SaveObject a) => a.Id == SeinDeathsManager.Instance.MoonGuid);
		}
		data.PendingScenes.Clear();
		data.Scenes.Clear();
		SaveScene master = data.Master;
		master.SaveObjects.Add(item);
	}

	public static SaveSceneManager Master;

	public List<SaveSceneManager.SaveId> SaveData = new List<SaveSceneManager.SaveId>();

	private static readonly List<SaveSerialize> s_saveSerializeList = new List<SaveSerialize>();

	private static readonly HashSet<ISerializable> s_saveSerializableHashSet = new HashSet<ISerializable>();

	private Dictionary<MoonGuid, Archive> m_saveCache = new Dictionary<MoonGuid, Archive>();

	[Serializable]
	public class SaveId
	{
		public SaveId()
		{
		}

		public ISerializable Save
		{
			get
			{
				return (ISerializable)this.SaveObject;
			}
			set
			{
				this.SaveObject = (UnityEngine.Object)value;
			}
		}

		public MoonGuid Id;

		public UnityEngine.Object SaveObject;
	}
		
	public Action<SceneRoot> bootstrapHook;
	
	public SceneRoot sceneRoot;
}
