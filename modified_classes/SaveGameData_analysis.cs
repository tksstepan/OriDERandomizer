/*

This class contains a modified load method which logs all saved objects when a
file is loaded. To be used for investigation purposes only (this code should
never be present in a release).

*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public partial class SaveGameData
{
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
		bool logging = RandomizerSettings.Controls.BashDeadzone > 0.9f;
		bool reading = RandomizerSettings.QOL.AbilityMenuOpacity > 0.9f;
		Hashtable DifferentDataMap = new Hashtable();
		if (reading)
		{
			string[] array = File.ReadAllLines("datamap.dat");
			for (int i = 0; i < array.Length; i += 2)
			{
				DifferentDataMap[array[i]] = array[i + 1];
			}
		}
		for (int j = 0; j < num; j++)
		{
			SaveScene saveScene = new SaveScene();
			saveScene.SceneGUID = new MoonGuid(reader.ReadBytes(16));
			if (logging)
			{
				Randomizer.log("SCENE");
				Randomizer.log(saveScene.SceneGUID.ToString());
			}
			this.Scenes.Add(saveScene.SceneGUID, saveScene);
			int num2 = reader.ReadInt32();
			for (int k = 0; k < num2; k++)
			{
				SaveObject saveObject = new SaveObject(new MoonGuid(reader.ReadBytes(16)));
				if (logging)
				{
					Randomizer.log(saveObject.Id.ToString());
				}
				saveObject.Data.ReadMemoryStreamFromBinaryReader(reader);
				if (logging)
				{
					string str = "";
					for (int l = 0; l < saveObject.Data.MemoryStream.GetBuffer().Length; l++)
					{
						str = str + saveObject.Data.MemoryStream.GetBuffer()[l].ToString() + " ";
					}
					Randomizer.log(str);
				}
				if (reading && DifferentDataMap.ContainsKey(saveObject.Id.ToString()))
				{
					saveObject.Data = new Archive();
					string[] array2 = ((string)DifferentDataMap[saveObject.Id.ToString()]).Split(new char[]
					{
						' '
					});
					byte[] bytes = new byte[array2.Length];
					for (int m = 0; m < array2.Length; m++)
					{
						bytes[m] = Convert.ToByte(array2[m]);
					}
					BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes));
					saveObject.Data.MemoryStream.SetLength((long)bytes.Length);
					binaryReader.Read(saveObject.Data.MemoryStream.GetBuffer(), 0, bytes.Length);
				}
				saveScene.SaveObjects.Add(saveObject);
			}
		}
		return true;
	}
}
