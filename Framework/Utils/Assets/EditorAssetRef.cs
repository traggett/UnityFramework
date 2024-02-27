#if UNITY_EDITOR

using System;
using System.IO;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public struct EditorAssetRef<T> where T : UnityEngine.Object
		{
			#region Public Data
			[SerializeField]
			private string _filePath;
			[SerializeField]
			private string _fileGUID;
			#endregion

			#region Private Data
			[NonSerialized]
			public T _editorAsset;
			[NonSerialized]
			public bool _editorCollapsed;
			#endregion

			#region Public Interface
			public static implicit operator T(EditorAssetRef<T> property)
			{
				return property.GetAsset();
			}

			public static implicit operator EditorAssetRef<T>(T asset)
			{
				return new EditorAssetRef<T>(asset);
			}

			public static implicit operator string(EditorAssetRef<T> property)
			{
				if (property.IsValid())
					return Path.GetFileNameWithoutExtension(property._filePath);

				return typeof(T).Name;
			}

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(_filePath);
			}

			public T GetAsset()
			{
				T asset = null;

				if (!string.IsNullOrEmpty(_fileGUID))
				{
					string filepath = AssetDatabase.GUIDToAssetPath(_fileGUID);
					asset = AssetDatabase.LoadAssetAtPath(filepath, typeof(T)) as T;
				}

				if (asset == null && !string.IsNullOrEmpty(_filePath))
				{
					asset = AssetDatabase.LoadAssetAtPath(_filePath, typeof(T)) as T;
				}

				return asset;
			}

			public string GetAssetName()
			{
				string name =  Path.GetFileName(_filePath);

				//Remove file extension
				int index = name.LastIndexOf(".");
				if (index != -1)
				{
					name = name.Substring(0, index);
				}

				return name;
			}
			
			public EditorAssetRef(T asset)
			{
				_editorAsset = asset;
				_editorCollapsed = false;

				if (asset != null)
				{
					_filePath = AssetDatabase.GetAssetPath(asset);
					_fileGUID = AssetDatabase.AssetPathToGUID(_filePath);

				}
				else
				{
					_filePath = null;
					_fileGUID = null;
				}
			}

			public string GetFilePath()
			{
				return _filePath;
			}

			public string GetFileGUID()
			{
				return _fileGUID;
			}
			#endregion
		}
	}
}

#endif