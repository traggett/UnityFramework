using System;

using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public struct StreamingAssetRef<T> : ISerializationCallbackReceiver where T : UnityEngine.Object
		{
			#region Public Data
			[SerializeField]
			private string _filePath;
			[SerializeField]
			private string _fileGUID;
			#endregion

			#region Private Data
#if UNITY_EDITOR
			[NonSerialized]
			public T _editorAsset;
			[NonSerialized]
			public bool _editorCollapsed;
#endif
			#endregion

			#region Public Interface
#if UNITY_EDITOR
			public static implicit operator StreamingAssetRef<T>(T asset)
			{
				return new StreamingAssetRef<T>(asset);
			}
#endif

			public static implicit operator string(StreamingAssetRef<T> property)
			{
				if (property.IsValid())
					return Path.GetFileNameWithoutExtension(property._filePath);

				return typeof(T).Name;
			}

			public bool IsValid()
			{
				return !string.IsNullOrEmpty(_filePath);
			}
			
			public string GetStreamingPath()
			{
				return AssetUtils.GetStreamingAssetPath(_filePath);
			}

			public string GetAssetName()
			{
				if (!string.IsNullOrEmpty(_filePath))
				{
					string name = Path.GetFileName(_filePath);

					//Remove file extension
					int index = name.LastIndexOf(".");
					if (index != -1)
					{
						name = name.Substring(0, index);
					}

					return name;
				}

				return string.Empty;
			}

#if UNITY_EDITOR
			public StreamingAssetRef(T asset)
			{
				_editorAsset = asset;
				_editorCollapsed = false;

				if (asset != null && AssetUtils.IsStreamingAsset(asset))
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

			public StreamingAssetRef(string assetPath)
			{
				_editorAsset = null;
				_editorCollapsed = false;

				_filePath = assetPath;
				_fileGUID = AssetDatabase.AssetPathToGUID(assetPath);
			}

			public string GetFilePath()
			{
				return _filePath;
			}

			public string GetFileGUID()
			{
				return _fileGUID;
			}
#endif
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
#if UNITY_EDITOR
				_editorAsset = FindEditorAsset();
#endif
			}
			#endregion

#if UNITY_EDITOR
			private T FindEditorAsset()
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
#endif
		}
	}
}