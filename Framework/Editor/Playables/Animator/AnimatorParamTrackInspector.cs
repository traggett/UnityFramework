using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Timeline;

using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		namespace Editor
		{
			[CustomEditor(typeof(AnimatorParamTrack), true)]
			public class AnimatorParamTrackInspector : UnityEditor.Editor
			{
				private static readonly string kParameterLabel = "Parameter Id";
				private static readonly string kNoParametersLabel ="No Valid Parameters";

				public override void OnInspectorGUI()
				{
					AnimatorParamTrack track = base.target as AnimatorParamTrack;
					Animator animator = GetClipBoundAnimator();

					if (animator != null && animator.runtimeAnimatorController != null)
					{
						AnimatorControllerParameter[] controllerParameters = (animator.runtimeAnimatorController as AnimatorController).parameters;

						List<string> parameters = new List<string>();
						int index = 0;

						for (int i = 0; i < controllerParameters.Length; i++)
						{
							if (controllerParameters[i].type == GetParameterType(track))
							{
								if (controllerParameters[i].name == track._parameterId)
									index = parameters.Count;

								parameters.Add(controllerParameters[i].name);
							}
						}

						if (parameters.Count > 0)
						{
							index = EditorGUILayout.Popup(kParameterLabel, index, parameters.ToArray());
							track._parameterId = parameters[index];
						}
						else
						{
							GUI.enabled = false;
							EditorGUILayout.TextField(kNoParametersLabel);
							GUI.enabled = true;
						}
					}
					else
					{
						GUI.enabled = false;
						EditorGUILayout.TextField(kParameterLabel, track._parameterId);
						GUI.enabled = true;
					}
				}

				private Animator GetClipBoundAnimator()
				{
					PlayableDirector selectedDirector = TimelineEditor.inspectedDirector;
					AnimatorParamTrack track = base.target as AnimatorParamTrack;

					if (selectedDirector != null && track != null)
					{
						ParentBindingTrack parentTrack = track.parent as ParentBindingTrack;

						if (parentTrack != null)
						{
							Object binding = parentTrack.GetEditorBinding(selectedDirector);

							if (binding is GameObject)
							{
								return AnimatorParamTrack.GetAnimatorFromGameObject((GameObject)binding);
							}
							else if (binding is Transform)
							{
								return AnimatorParamTrack.GetAnimatorFromGameObject(((Transform)binding).gameObject);
							}
						}
						else
						{
							return selectedDirector.GetGenericBinding(track) as Animator;
						}
					}

					return null;
				}

				private AnimatorControllerParameterType GetParameterType(AnimatorParamTrack track)
				{
					if (track is AnimatorFloatParamTrack)
						return AnimatorControllerParameterType.Float;

					if (track is AnimatorIntParamTrack)
						return AnimatorControllerParameterType.Int;

					if (track is AnimatorBoolParamTrack)
						return AnimatorControllerParameterType.Bool;

					return AnimatorControllerParameterType.Trigger;
				}
			}
		}
	}
}