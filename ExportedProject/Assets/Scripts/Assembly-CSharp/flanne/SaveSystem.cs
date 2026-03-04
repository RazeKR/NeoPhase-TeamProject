using System;
using System.IO;
using UnityEngine;

namespace flanne
{
	public static class SaveSystem
	{
		public static SaveData data;

		public static void Load()
		{
			string path = Application.persistentDataPath + "/gamedata.json";
			if (File.Exists(path))
			{
				string json = File.ReadAllText(path);
				try
				{
					data = JsonUtility.FromJson<SaveData>(json);
					return;
				}
				catch (Exception)
				{
					string path2 = Application.persistentDataPath + "/gamedata_backup.json";
					if (File.Exists(path2))
					{
						string json2 = File.ReadAllText(path2);
						try
						{
							data = JsonUtility.FromJson<SaveData>(json2);
							return;
						}
						catch (Exception)
						{
							data = new SaveData();
							return;
						}
					}
					data = new SaveData();
					return;
				}
			}
			data = new SaveData();
		}

		public static void Save()
		{
			string contents = JsonUtility.ToJson(data);
			File.WriteAllText(Application.persistentDataPath + "/gamedata.json", contents);
			File.WriteAllText(Application.persistentDataPath + "/gamedata_backup.json", contents);
		}
	}
}
