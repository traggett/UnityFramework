using System.Xml;
using System.IO;
using System;

using UnityEngine;

namespace Framework
{
	using Utils;
	using Serialization;

	namespace SaveSystem
	{
		public static class SaveData
		{
			private static SaveDataFile _activeSaveDataFile = null;
			private static Type[] _saveDataBlockTypes = null;
			
			#region Load / Save
			public static bool HasSaveGame(string saveFileId = "")
			{
				return File.Exists(GetSavePath(saveFileId));
			}

			public static void ClearSaveGame()
			{
				_activeSaveDataFile = new SaveDataFile();
			}

			public static void Save(string saveFileId = "")
			{
				if (_activeSaveDataFile != null)
				{
					Serializer.ToFile(_activeSaveDataFile, GetSavePath(saveFileId));
				}
			}

			public static void Load(string saveFileId = "")
			{
				string saveFile = GetSavePath(saveFileId);

				if (File.Exists(saveFile))
				{
					_activeSaveDataFile = Serializer.FromFile<SaveDataFile>(saveFile);
				}	

				if (_activeSaveDataFile == null)
				{
					_activeSaveDataFile = new SaveDataFile();
				}
			}
			#endregion

			public static T Get<T>() where T : SaveDataBlock, new()
			{
				return _activeSaveDataFile.Get<T>();
			}

			public static SaveDataBlock GetByType(Type type)
			{
				return _activeSaveDataFile.GetByType(type);
			}

			public static Type[] GetSaveDataBlockTypes()
			{
				if (_saveDataBlockTypes == null)
				{
					_saveDataBlockTypes = SystemUtils.GetAllSubTypes(typeof(SaveDataBlock));
				}

				return _saveDataBlockTypes;
			}

			private static string GetSavePath(string saveFileId)
			{
				return Application.persistentDataPath + "/SaveGame" + saveFileId + ".sav";
			}
		}
	}
}