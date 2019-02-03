using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace Framework
{
	using System;
	using Utils;

	namespace Playables
	{
		namespace Editor
		{
			public class ParentBindingTrackInspector : UnityEditor.Editor
			{
				protected ReorderableList _channelTracks;

				public void OnEnable()
				{
					_channelTracks = new ReorderableList(new TrackAsset[0], typeof(TrackAsset), false, true, true, false)
					{
						drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawSubTrack),
						drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnDrawHeader),
						onAddCallback = new ReorderableList.AddCallbackDelegate(OnAddTrack),
						showDefaultBackground = true,
						index = 0,
						elementHeight = 20f
					};
				}

				public override void OnInspectorGUI()
				{
					ParentBindingTrack track = target as ParentBindingTrack;
					if (track == null)
						return;

					GUILayout.Label(track.name, EditorStyles.boldLabel);

					DrawDefaultInspector();

					IEnumerable<TrackAsset> childTracks = track.GetChildTracks();

					GUILayout.Label("Child Tracks", EditorStyles.boldLabel);
					GUILayout.Space(3f);
					_channelTracks.list = new List<TrackAsset>(childTracks);
					_channelTracks.DoLayoutList();
					_channelTracks.index = -1;

					track.EnsureMasterClipExists();
				}

				private void OnAddTrack(ReorderableList list)
				{
					GenericMenu menu = new GenericMenu();

					Type[] types = SystemUtils.GetAllSubTypes(typeof(IParentBindableTrack));

					foreach (Type type in types)
					{
						menu.AddItem(new GUIContent("Add " + type.Name + " Subtrack"), false, CreateSubTrack, type);
					}

					menu.ShowAsContext();
				}

				private void CreateSubTrack(object data)
				{
					Type type = data as Type;
					TimelineEditorUtils.CreateChildTrack(base.target as TrackAsset, type.Name, type);
				}

				protected virtual void OnDrawHeader(Rect rect)
				{
					float columnWidth = rect.width /= 3f;
					GUI.Label(rect, "Name", EditorStyles.label);
					rect.x += columnWidth;
					GUI.Label(rect, "Duration", EditorStyles.label);
					rect.x += columnWidth;
					GUI.Label(rect, "Clips", EditorStyles.label);
				}

				protected virtual void OnDrawSubTrack(Rect rect, int index, bool selected, bool focused)
				{
					float columnWidth = rect.width / 3f;
					TrackAsset track = _channelTracks.list[index] as TrackAsset;

					if (track != null)
					{
						rect.width = columnWidth;
						GUI.Label(rect, track.name, EditorStyles.label);
						rect.x += columnWidth;
						GUI.Label(rect, track.duration.ToString(), EditorStyles.label);
						rect.x += columnWidth;
						GUI.Label(rect, ArrayUtils.GetCount(track.GetClips()).ToString(), EditorStyles.label);
					}
				}
			}
		}
	}
}