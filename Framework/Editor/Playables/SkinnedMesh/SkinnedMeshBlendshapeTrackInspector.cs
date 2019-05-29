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
			[CustomEditor(typeof(SkinnedMeshBlendshapeTrack), true)]
			public class SkinnedMeshBlendshapeTrackInspector : UnityEditor.Editor
			{
				private static readonly string kParameterLabel = "Blend Shape";
				private static readonly string kNoParametersLabel ="No Valid Blend Shapes";

				public override void OnInspectorGUI()
				{
					SkinnedMeshBlendshapeTrack track = base.target as SkinnedMeshBlendshapeTrack;
					SkinnedMeshRenderer skinnedMeshRenderer = GetClipBoundSkinnedMeshRenderer();

					if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
					{
						string[] blendShapes = new string[skinnedMeshRenderer.sharedMesh.blendShapeCount];
						int index = Mathf.Min(track._blendShapeIndex, skinnedMeshRenderer.sharedMesh.blendShapeCount);

						for (int i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
						{
							blendShapes[i] = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
						}

						if (skinnedMeshRenderer.sharedMesh.blendShapeCount > 0)
						{
							index = EditorGUILayout.Popup(kParameterLabel, index, blendShapes);
							track._blendShapeIndex = index;
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
						EditorGUILayout.TextField(kParameterLabel, track._blendShapeIndex.ToString());
						GUI.enabled = true;
					}
				}

				private SkinnedMeshRenderer GetClipBoundSkinnedMeshRenderer()
				{
					PlayableDirector selectedDirector = TimelineEditor.inspectedDirector;
					SkinnedMeshBlendshapeTrack track = base.target as SkinnedMeshBlendshapeTrack;

					if (selectedDirector != null && track != null)
					{
						ParentBindingTrack parentTrack = track.parent as ParentBindingTrack;

						if (parentTrack != null)
						{
							Object binding = parentTrack.GetEditorBinding(selectedDirector);

							if (binding is GameObject)
							{
								return SkinnedMeshBlendshapeTrack.GetSkinnedMeshFromGameObject((GameObject)binding);
							}
							else if (binding is Transform)
							{
								return SkinnedMeshBlendshapeTrack.GetSkinnedMeshFromGameObject(((Transform)binding).gameObject);
							}
						}
						else
						{
							return selectedDirector.GetGenericBinding(track) as SkinnedMeshRenderer;
						}
					}

					return null;
				}
			}
		}
	}
}