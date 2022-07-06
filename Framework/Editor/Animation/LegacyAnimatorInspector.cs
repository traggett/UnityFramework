using UnityEditor;
using UnityEngine;

namespace Framework
{

	namespace Animations
	{
		namespace Editor
		{
			[CustomEditor(typeof(LegacyAnimator), true)]
			public sealed class LegacyAnimatorInspector : UnityEditor.Editor
			{
				private GUIStyle _style;
				private Animation _animation;
				private bool _foldout;

				private void OnEnable() 
				{
					_animation = ((LegacyAnimator)target).GetComponent<Animation>();
				}

				public override void OnInspectorGUI()
				{
					if (_style == null)
					{
						_style = new GUIStyle(EditorStyles.textArea);
						_style.fixedHeight = 0f;
						_style.richText = true;
					}

					DrawDefaultInspector();

					_foldout = EditorGUILayout.BeginFoldoutHeaderGroup(_foldout, "Current Animation State Info");
					if (_foldout)
					{
						GUI.enabled = false;
						EditorGUILayout.TextArea(DebugLogInfo(_animation), _style);	
						GUI.enabled = true;
					}
					EditorGUILayout.EndFoldoutHeaderGroup();				
				}
						
				private static string DebugLogInfo(Animation animation)
				{
					string info = "";

					foreach (AnimationState state in animation)
					{
						info += "clip:<b>" + state.name + "</b>    enabled:<b>" + state.enabled + "</b>    weight:<b>" + state.weight + "</b>    time:<b>" + state.time + "</b>    layer:<b>" + state.layer + "</b>";
						info += "\n";
					}

					return info;
				}
			}
		}
	}
}