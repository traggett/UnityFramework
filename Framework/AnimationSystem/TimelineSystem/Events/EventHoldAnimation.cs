using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Framework
{
	using Serialization;
	using TimelineSystem;

	namespace AnimationSystem
	{
		namespace TimelineSystem
		{
			[Serializable]
			[EventCategory("Animation")]
			public class EventHoldAnimation : Event
			{
				public enum eHoldType
				{
					FirstFrame,
					LastFrame,
					Time
				}

				#region Public Data
				public int _channel = 0;
				public AnimationRef _animation = new AnimationRef();
				public eHoldType _holdType = eHoldType.FirstFrame;
				public float _holdTime = 1.0f;
				#endregion

				#region ICustomEditable
#if UNITY_EDITOR
				public bool RenderProperty(GUIContent label)
				{
					bool dataChanged = false;

					_animation = SerializationEditorGUILayout.ObjectField(_animation, new GUIContent("Animation"), ref dataChanged);
					_holdType = SerializationEditorGUILayout.ObjectField(_holdType, "Play Mode", ref dataChanged);

					if (_holdType == eHoldType.Time)
					{
						EditorGUI.BeginChangeCheck();
						_holdTime = EditorGUILayout.FloatField("Hold Time", _holdTime);
						dataChanged |= EditorGUI.EndChangeCheck();
					}

					return dataChanged;
				}
#endif
				#endregion

				#region Event
				public override void Trigger()
				{
					IAnimator target = _animation.GetAnimator();

					if (target != null)
					{
						target.Play(_channel, _animation._animationId);

						switch (_holdType)
						{
							case eHoldType.FirstFrame:
								target.SetAnimationTime(_channel, _animation._animationId, 0.0f);
								break;
							case eHoldType.LastFrame:
								target.SetAnimationTime(_channel, _animation._animationId, target.GetAnimationLength(_animation._animationId) - 0.01f);
								break;
							case eHoldType.Time:
								target.SetAnimationTime(_channel, _animation._animationId, _holdTime);
								break;
						}

						target.SetAnimationSpeed(_channel, _animation._animationId, 0.0f);
					}
				}

				public override float GetDuration()
				{
					return _holdTime;
				}

#if UNITY_EDITOR
				public override Color GetEditorColor()
				{
					return new Color(115.0f / 255.0f, 204.0f / 255.0f, 111.0f / 255.0f);
				}

				public override string GetEditorDescription()
				{
					string label = "<b>(" + _animation._animator + ")</b> ";

					switch (_holdType)
					{
						case eHoldType.FirstFrame:
							label += "Hold First Frame";
							break;
						case eHoldType.LastFrame:
							label += "Hold Last Frame";
							break;
						case eHoldType.Time:
							label += "Hold Animation";
							break;
					}

					label += " (<b>" + _animation._animationId + "</b>)";

					return label;
				}
#endif
				#endregion
			}
		}
	}
}