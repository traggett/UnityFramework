using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine;

namespace Framework
{
	namespace Playables
	{
		namespace Editor
		{
			public static class TimelineEditorUtils
			{
				//Until Unity makes the SupportsChildTracks public, have to hack our way around creating child tracks
				public static T CreateChildTrack<T>(TrackAsset parent, string name) where T : TrackAsset
				{
					return (T)CreateChildTrack(parent, name, typeof(T));
				}

				public static TrackAsset CreateChildTrack(TrackAsset parent, string name, Type type)
				{
					TrackAsset newTrack = null;

					if (parent != null && parent.timelineAsset != null)
					{
						newTrack = parent.timelineAsset.CreateTrack(type, null, name);

						if (newTrack != null)
						{
							//Set as a child in parent track
							{
								SerializedObject parentSO = new SerializedObject(parent);

								SerializedProperty childrenProp = parentSO.FindProperty("m_Children");
								childrenProp.arraySize = childrenProp.arraySize + 1;
								SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(childrenProp.arraySize - 1);
								childProp.objectReferenceValue = newTrack;

								parentSO.ApplyModifiedProperties();
							}

							//Mark parent on new track
							{
								SerializedObject childSO = new SerializedObject(newTrack);

								SerializedProperty parentProp = childSO.FindProperty("m_Parent");
								parentProp.objectReferenceValue = parent;

								childSO.ApplyModifiedProperties();
							}

							//Remove from timeline root tracks
							{
								SerializedObject timelineSO = new SerializedObject(parent.timelineAsset);
								SerializedProperty tracksProp = timelineSO.FindProperty("m_Tracks");

								List<UnityEngine.Object> tracks = new List<UnityEngine.Object>();

								for (int i = 0; i < tracksProp.arraySize; i++)
								{
									SerializedProperty trackProp = tracksProp.GetArrayElementAtIndex(i);

									if (trackProp.objectReferenceValue != newTrack)
									{
										tracks.Add(trackProp.objectReferenceValue);
									}
								}

								tracksProp.arraySize = tracks.Count;
								for (int i = 0; i < tracksProp.arraySize; i++)
								{
									SerializedProperty trackProp = tracksProp.GetArrayElementAtIndex(i);
									trackProp.objectReferenceValue = tracks[i];
								}

								timelineSO.ApplyModifiedProperties();
							}

							//Refresh the window to show new track as child
							ShowTimelineInEditorWindow(parent.timelineAsset);
						}
					}

					return newTrack;
				}
				
				public static void ShowTimelineInEditorWindow(TimelineAsset timelineAsset)
				{
					//Add new track via reflection (puke)
					Assembly assembly = Assembly.GetAssembly(typeof(TimelineEditor));
					Type timelineWindowType = assembly.GetType("UnityEditor.Timeline.TimelineWindow");
					//UnityEditor.Timeline.TimelineWindow
					EditorWindow timelineWindow = EditorWindow.GetWindow(timelineWindowType);
					//AddTrack(Type type, TrackAsset parent = null, string name = null);
					MethodInfo methodInfo = timelineWindowType.GetMethod("AddTrack", new Type[] { typeof(Type), typeof(TrackAsset), typeof(string) });

					MethodInfo[] methj = timelineWindowType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic);

					UnityEngine.Object previousSelectionObject = Selection.activeObject;

					//Have to set timeline to null and then back to this timeline to show changes grr...
					methodInfo = timelineWindowType.GetMethod("SetTimeline", new Type[] { typeof(TimelineAsset) });

					methodInfo.Invoke(timelineWindow, new object[] { null });
					methodInfo.Invoke(timelineWindow, new object[] { timelineAsset });

					//Also need to reselect whatever timeline object was previously selected as above clears it
					Selection.activeObject = previousSelectionObject;
				}
			}
		}
	}
}