using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace Framework
{
	namespace Utils
	{
		public class SceneIndexer : MonoBehaviour
		{
			[System.Serializable]
			public class ObjectData
			{
				public GameObject _gameObject;
				public int _sceneObjectID;
			}
			[HideInInspector]
			public ObjectData[] _sceneObjects;
			private Dictionary<int, GameObject> _lookupTable = null;

			void Awake()
			{
				this.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
			}

			public static GameObject GetObject(Scene scene, int sceneObjectID)
			{
				GameObject obj = null;

				if (sceneObjectID != -1)
				{
					SceneIndexer indexer = SceneUtils.FindInScene<SceneIndexer>(scene);

					if (indexer != null)
					{
						obj = indexer.GetObject(sceneObjectID);
					}
				}			

				return obj;
			}

			public GameObject GetObject(int sceneObjectID)
			{
				GameObject obj = null;

				if (sceneObjectID != -1)
				{
					if (_lookupTable == null)
					{
						BuildLookUpTable();
					}

					if (!_lookupTable.TryGetValue(sceneObjectID, out obj))
					{
						BuildLookUpTable();
					}
				}

				return obj;
			}

#if UNITY_EDITOR
			public static int GetIdentifier(GameObject gameObject)
			{
				if (gameObject == null)
				{
					return -1;
				}
				
				SerializedObject serializedObject = new SerializedObject(gameObject);
				PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
				inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);
				SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");   //note the misspelling!

				return localIdProp.intValue;
			}

			public void CacheSceneObjects()
			{
				Scene scene = this.gameObject.scene;

				List<ObjectData> sceneObjects = new List<ObjectData>();

				//Find object by id in scene
				foreach (GameObject rootObject in scene.GetRootGameObjects())
				{
					ObjectData objData = new ObjectData();
					objData._gameObject = rootObject;
					objData._sceneObjectID = GetIdentifier(rootObject);
					sceneObjects.Add(objData);

					AddChildren(rootObject, ref sceneObjects);
				}

				_sceneObjects = sceneObjects.ToArray();
				_lookupTable = null;
			}

			private void AddChildren(GameObject parent, ref List<ObjectData> sceneObjects)
			{
				foreach (Transform child in parent.transform)
				{
					ObjectData objData = new ObjectData();
					objData._gameObject = child.gameObject;
					objData._sceneObjectID = GetIdentifier(child.gameObject);

					sceneObjects.Add(objData);

					AddChildren(child.gameObject, ref sceneObjects);
				}
			}
#endif

			private void BuildLookUpTable()
			{
				_lookupTable = new Dictionary<int, GameObject>();

				foreach (ObjectData data in _sceneObjects)
				{
					if (data._sceneObjectID != 0 && data._sceneObjectID != -1)
					{
						_lookupTable[data._sceneObjectID] = data._gameObject;
					}
				}
			}
		}
	}
}
