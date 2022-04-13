using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Utils;
	using Serialization;

	namespace AnimationSystem
	{
		[Serializable]
		public sealed class AnimationRef : ICustomEditorInspector
		{
			#region Public Data		
			public ComponentRef<Animation> _animation;
			public string _animationId = string.Empty;
			#endregion

#if UNITY_EDITOR
			private bool _editorFoldout = true;
#endif

			public static implicit operator string(AnimationRef property)
			{
				return property._animation + ":" + property._animationId;
			}

			public Animation GetAnimator()
			{
				return _animation.GetComponent();
			}

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				if (label == null)
					label = new GUIContent();

				label.text += " (" + this + ")";

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);

				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;
					
					_animation = SerializationEditorGUILayout.ObjectField(_animation, new GUIContent("Animator"), ref dataChanged);

					Animation animator = _animation.GetComponent();

					if (animator != null)
					{
						int currentIndex = -1;
						int i = 0;
						string[] animationNames = new string[animator.GetClipCount()];

						foreach (AnimationClip clip in animator)
						{
							animationNames[i] = clip.name;

							if (clip.name == _animationId)
							{
								currentIndex = i;
							}

							i++;
						}

						int index = EditorGUILayout.Popup("Animation", currentIndex == -1 ? 0 : currentIndex, animationNames);

						if (currentIndex != index)
						{
							_animationId = animationNames[index];
							dataChanged = true;
						}
					}

					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion
		}
	}
}