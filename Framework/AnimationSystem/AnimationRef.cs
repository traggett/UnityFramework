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
			public ComponentRef<IAnimator> _animator;
			public string _animationId = string.Empty;
			#endregion

#if UNITY_EDITOR
			private bool _editorFoldout = true;
#endif

			public static implicit operator string(AnimationRef property)
			{
				return property._animator + ":" + property._animationId;
			}

			public IAnimator GetAnimator()
			{
				return _animator.GetComponent();
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
					
					_animator = SerializationEditorGUILayout.ObjectField(_animator, new GUIContent("Animator"), ref dataChanged);

					IAnimator animator = _animator.GetComponent();

					if (animator != null)
					{
						string[] animationNames = animator.GetAnimationNames();
						int currentIndex = -1;

						for (int i=0; i< animationNames.Length; i++)
						{
							if (animationNames[i] == _animationId)
							{
								currentIndex = i;
								break;
							}
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