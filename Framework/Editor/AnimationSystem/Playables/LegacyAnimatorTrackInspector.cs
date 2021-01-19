using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace Framework
{
	using Playables.Editor;
	using Utils;

	namespace AnimationSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(LegacyAnimatorTrack))]
			[CanEditMultipleObjects]
			public class LegacyAnimatorTrackInspector : UnityEditor.Editor
			{
				private SerializedProperty _channelProperty;
				private ReorderableList _channelTracks;
				

				public void OnEnable()
				{
					_channelProperty = this.serializedObject.FindProperty("_animationChannel");

					_channelTracks = new ReorderableList(new TrackAsset[0], typeof(TrackAsset), false, true, true, false)
					{
						drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawSubTrack),
						drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnDrawHeader),
						onAddCallback = new ReorderableList.AddCallbackDelegate(OnAddChannel),
						showDefaultBackground = true,
						index = 0,
						elementHeight = 20f
					};
				}

				public override void OnInspectorGUI()
				{
					foreach (Object target in base.targets)
					{
						LegacyAnimatorTrack track = target as LegacyAnimatorTrack;
						if (track == null)
							break;

						if (!(track.parent is TimelineAsset))
							break;

						GUILayout.Label(track.name, EditorStyles.boldLabel);

						EditorGUILayout.PropertyField(_channelProperty, new GUIContent("Base Animation Channel"));

						EditorGUILayout.Separator();

						GUILayout.Label(new GUIContent("Additional Animation Layers"), EditorStyles.boldLabel);

						IEnumerable<TrackAsset> childTracks = track.GetChildTracks();
						_channelTracks.list = new List<TrackAsset>(childTracks);
						_channelTracks.DoLayoutList();
						_channelTracks.index = -1;
					}

					serializedObject.ApplyModifiedProperties();
				}

				private void OnAddChannel(ReorderableList list)
				{
					foreach (Object target in base.targets)
					{
						AddChanelToTrack(target as LegacyAnimatorTrack);
					}
				}

				protected virtual void AddChanelToTrack(LegacyAnimatorTrack animatorTrack)
				{
					if (animatorTrack != null)
					{
						//Work out next free channel to add
						int channel = _channelProperty.intValue + 1;

						foreach (LegacyAnimatorTrack track in animatorTrack.GetChildTracks())
						{
							if (track != null)
							{
								channel = Mathf.Max(channel, track._animationChannel + 1);
							}
						}

						LegacyAnimatorTrack newTrack = TimelineEditorUtils.CreateChildTrack<LegacyAnimatorTrack>(animatorTrack, "Channel " + channel);
						newTrack._animationChannel = channel;
					}
				}

				protected virtual void OnDrawHeader(Rect rect)
				{
					float columnWidth = rect.width /= 3f;
					GUI.Label(rect, "Channel", EditorStyles.label);
					rect.x += columnWidth;
					GUI.Label(rect, "Duration", EditorStyles.label);
					rect.x += columnWidth;
					GUI.Label(rect, "Clips", EditorStyles.label);
				}

				protected virtual void OnDrawSubTrack(Rect rect, int index, bool selected, bool focused)
				{ 
					float columnWidth = rect.width / 3f;
					LegacyAnimatorTrack track = _channelTracks.list[index] as LegacyAnimatorTrack;

					if (track != null)
					{
						rect.width = columnWidth;
						GUI.Label(rect, track._animationChannel.ToString(), EditorStyles.label);
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