using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using Framework.Utils;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Framework
{
	namespace Playables
	{
		namespace Editor
		{
			[CustomEditor(typeof(PlayableDirectorBindings))]
			public class PlayableDirectorBindingsInspector : UnityEditor.Editor
			{
				private PlayableAsset _cachedAsset;

				[InitializeOnLoadMethod]
				static public void Init()
				{
					EditorSceneManager.sceneSaving += OnSceneSaving;
				}

				public override void OnInspectorGUI()
				{
					PlayableDirectorBindings assetSwitcher = target as PlayableDirectorBindings;
					if (assetSwitcher == null)
						return;

					PlayableDirector playableDirector = assetSwitcher.GetComponent<PlayableDirector>();
					if (playableDirector == null)
						return;

					if (Application.isPlaying)
						return;

					SerializedObject playableDirectorSO = new SerializedObject(playableDirector);

					SerializedProperty assetProp = playableDirectorSO.FindProperty("m_PlayableAsset");
					SerializedProperty sceneBindingsProp = playableDirectorSO.FindProperty("m_SceneBindings");
					SerializedProperty exposedReferencesProp = playableDirectorSO.FindProperty("m_ExposedReferences");
					exposedReferencesProp = exposedReferencesProp.FindPropertyRelative("m_References");

					if (_cachedAsset != playableDirector.playableAsset)
					{
						_cachedAsset = playableDirector.playableAsset;

						//Clear bindings
						sceneBindingsProp.arraySize = 0;
						exposedReferencesProp.arraySize = 0;
						playableDirectorSO.ApplyModifiedProperties();

						//Load bindings from component
						assetSwitcher.SetupBindingsForAsset(playableDirector.playableAsset);
						EditorSceneManager.MarkSceneDirty(playableDirector.gameObject.scene);
					}

					EditorGUILayout.LabelField("Configured Playable Assets", EditorStyles.boldLabel);

					//Draw assets that have binding for (show asset, switch to button, clear button)
					SerializedProperty assetDataArrayProp = serializedObject.FindProperty("_playableAssetData");

					assetDataArrayProp.isExpanded = EditorGUILayout.Foldout(assetDataArrayProp.isExpanded, new GUIContent("Playable Asset Bindings"), toggleOnLabelClick: true);

					if (assetDataArrayProp.isExpanded)
					{
						EditorGUI.indentLevel++;
						EditorGUILayout.BeginVertical();

						for (int i=0; i<assetDataArrayProp.arraySize; i++)
						{
							SerializedProperty assetDataProp = assetDataArrayProp.GetArrayElementAtIndex(i).FindPropertyRelative("_asset");

							EditorGUILayout.BeginHorizontal();

							GUI.enabled = false;
							EditorGUILayout.PropertyField(assetDataProp, GUIContent.none);
							GUI.enabled = true;

							if (GUILayout.Button("Load"))
							{
								_cachedAsset = assetDataProp.objectReferenceValue as PlayableAsset;

								//Clear bindings
								sceneBindingsProp.arraySize = 0;
								exposedReferencesProp.arraySize = 0;

								//Set playable assets
								assetProp.objectReferenceValue = _cachedAsset;

								playableDirectorSO.ApplyModifiedProperties();

								//Load bindings from component
								assetSwitcher.SetupBindingsForAsset(_cachedAsset);
							}

							if (GUILayout.Button("Clear"))
							{
								_cachedAsset = null;

								//If its the current asset, clear it
								if (assetProp.objectReferenceValue == assetDataProp.objectReferenceValue)
								{
									assetProp.objectReferenceValue = null;
									playableDirectorSO.ApplyModifiedProperties();
								}

								//Remove from array
								assetDataArrayProp.DeleteArrayElementAtIndex(i);
								i--;
								serializedObject.ApplyModifiedProperties();
							}

							EditorGUILayout.EndHorizontal();
						}

						EditorGUILayout.EndVertical();
						EditorGUI.indentLevel--;
					}

					//Sync binding from director - this should happen whenever bindings change or on update not on inspector
					if (playableDirector.playableAsset != null)
					{
						SyncBindings(assetSwitcher);
					}
				}

				public static void OnSceneSaving(Scene scene, string path)
				{
					PlayableDirectorBindings[] bindings = SceneUtils.FindAllInScene<PlayableDirectorBindings>(scene, true);

					foreach (PlayableDirectorBindings binding in bindings)
					{
						SyncBindings(binding);
					}
				}

				private static void SyncBindings(PlayableDirectorBindings bindings)
				{
					PlayableDirector playableDirector = bindings.GetComponent<PlayableDirector>();
					if (playableDirector == null || playableDirector.playableAsset == null)
						return;

					SerializedObject playableDirectorSO = new SerializedObject(playableDirector);

					SerializedProperty sceneBindingsProp = playableDirectorSO.FindProperty("m_SceneBindings");
					SerializedProperty exposedReferencesProp = playableDirectorSO.FindProperty("m_ExposedReferences.m_References");

					//Get scene bindings
					List<PlayableDirectorBindings.GenericBinding> sceneBindings = new List<PlayableDirectorBindings.GenericBinding>();

					for (int i = 0; i < sceneBindingsProp.arraySize; i++)
					{
						SerializedProperty binding = sceneBindingsProp.GetArrayElementAtIndex(i);
						SerializedProperty key = binding.FindPropertyRelative("key");
						SerializedProperty value = binding.FindPropertyRelative("value");

						PlayableDirectorBindings.GenericBinding genericBinding = new PlayableDirectorBindings.GenericBinding
						{
							_key = key.objectReferenceValue,
							_value = value.objectReferenceValue
						};

						sceneBindings.Add(genericBinding);
					}

					//Get exposed refs
					List<PlayableDirectorBindings.ExposedReference> exposedReferences = new List<PlayableDirectorBindings.ExposedReference>();

					for (int i = 0; i < exposedReferencesProp.arraySize; i++)
					{
						SerializedProperty exposedReferenceProp = exposedReferencesProp.GetArrayElementAtIndex(i);
						exposedReferenceProp.NextVisible(true);
						string key = exposedReferenceProp.stringValue;
						exposedReferenceProp.NextVisible(true);
						Object value = exposedReferenceProp.objectReferenceValue;

						PlayableDirectorBindings.ExposedReference exposedReference = new PlayableDirectorBindings.ExposedReference
						{
							_key = key,
							_value = value
						};

						exposedReferences.Add(exposedReference);
					}

					//Update data
					bool foundData = false;

					if (bindings._playableAssetData != null)
					{
						for (int i = 0; i < bindings._playableAssetData.Length; i++)
						{
							if (bindings._playableAssetData[i]._asset == playableDirector.playableAsset)
							{
								bindings._playableAssetData[i]._sceneBindings = sceneBindings.ToArray();
								bindings._playableAssetData[i]._exposedReferences = exposedReferences.ToArray();
								foundData = true;
								break;
							}
						}
					}

					if (!foundData)
					{
						PlayableDirectorBindings.PlayableAssetData playableAssetData = new PlayableDirectorBindings.PlayableAssetData();
						playableAssetData._asset = playableDirector.playableAsset;
						playableAssetData._sceneBindings = sceneBindings.ToArray();
						playableAssetData._exposedReferences = exposedReferences.ToArray();

						ArrayUtils.Add(ref bindings._playableAssetData, playableAssetData);
					}
				}
			}
		}
	}
}