using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;

using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		namespace Editor
		{
			[CustomEditor(typeof(MaterialParamTrack), true)]
			public class MaterialParamTrackInspector : UnityEditor.Editor
			{
				private static readonly string kParameterLabel = "Parameter Id";
				private static readonly string kNoParametersLabel ="No Valid Parameters";

				public override void OnInspectorGUI()
				{
					MaterialParamTrack track = base.target as MaterialParamTrack;
					Material material = GetClipBoundMaterial();

					if (material != null)
					{
						MaterialProperty[] properties = MaterialEditor.GetMaterialProperties(new Object[] { material } );
						List<string> parameters = new List<string>();

						int index = 0;

						for (int i = 0; i < properties.Length; i++)
						{
							if (MatchesTrack(track, properties[i]))
							{
								if (properties[i].name == track._parameterId)
									index = parameters.Count;

								parameters.Add(properties[i].name);
							}
						}

						if (properties.Length > 0)
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

				private Material GetClipBoundMaterial()
				{
					PlayableDirector selectedDirector = TimelineEditor.inspectedDirector;
					MaterialParamTrack track = base.target as MaterialParamTrack;

					if (selectedDirector != null && track != null)
					{
						ParentBindingTrack parentTrack = track.parent as ParentBindingTrack;

						if (parentTrack != null)
						{
							Object binding = parentTrack.GetEditorBinding(selectedDirector);

							if (binding is Material material)
							{
								return material;
							}
						}
						else
						{
							return selectedDirector.GetGenericBinding(track) as Material;
						}
					}

					return null;
				}

				private bool MatchesTrack(MaterialParamTrack track, MaterialProperty property)
				{
					if (track is MaterialColorParamTrack)
					{
						return property.type == MaterialProperty.PropType.Color || property.type == MaterialProperty.PropType.Vector;
					}

					if (track is MaterialFloatParamTrack)
					{
						return property.type == MaterialProperty.PropType.Float || property.type == MaterialProperty.PropType.Range;
					}

					return false;
				}
			}
		}
	}
}