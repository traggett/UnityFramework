using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using StateMachineSystem;
	using Maths;
	using System;
	using TimelineSystem;
	using Utils;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory("Animation")]
		public class EventTransformGameObject : Event, ITimelineStateEvent, ICustomEditorInspector
		{
			#region Public Data
			public enum eMoveType
			{
				To,
				From,
			}
			[Flags]
			public enum eTransformFlag
			{
				Translate = 1,
				Rotate = 2,
				Scale = 4,
			}
			public enum eTargetType
			{
				LocalDelta,
				LocalTarget,
				WorldDelta,
				WorldTarget,
				Transform,
			}
			
			public GameObjectRef _gameObject;
			public eTransformFlag _transformFlags = eTransformFlag.Translate;
			public eMoveType _moveType = eMoveType.To;
			public eTargetType _targetType = eTargetType.LocalDelta;
			public InterpolationType _easeType = InterpolationType.Linear;
			public float _duration = 0.0f;
			public GameObjectRef _targetTransform;
			public Vector3 _targetPosition = Vector3.zero;
			public Quaternion _targetRotation = Quaternion.identity;
			public Vector3 _targetScale = Vector3.one;
			#endregion

			#region Private Data
			private Vector3 _runTimeStartPosition;
			private Vector3 _runTimeTargetPosition;
			private Quaternion _runTimeStartRotation;
			private Quaternion _runTimeTargetRotation;
			private Vector3 _runTimeStartScale;
			private Vector3 _runTimeTargetScale;

			private Transform _transform;
			private Transform _runTimeTargetTransform;
			#endregion

			#region Event
			public override float GetDuration()
			{
				return _duration;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(162f / 255f, 219f / 255f, 115f / 255f);
			}

			public override string GetEditorDescription()
			{
				return "(<b>" + _gameObject + "</b>) " + GetTypeString() + GetActionString() + "(<b>" + GetTargetString() + "</b>)";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				GameObject gameObject = _gameObject.GetGameObject();

				if (gameObject != null)
				{
					_transform = gameObject.transform;

					FindTargets(stateMachine);
					UpdateTransform(0.0f);

					return eEventTriggerReturn.EventOngoing;
				}

				return eEventTriggerReturn.EventFinished;
			}

			public eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime)
			{
				float lerp = eventTime / _duration;
				UpdateTransform(lerp);

				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachineComponent stateMachine)
			{
				UpdateTransform(1.0f);
			}

#if UNITY_EDITOR
			public StateMachineEditorLink[] GetEditorLinks() { return null; }
#endif
			#endregion

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;
				
				_gameObject = SerializationEditorGUILayout.ObjectField(_gameObject, "Game Object", ref dataChanged);
				_transformFlags = SerializationEditorGUILayout.ObjectField(_transformFlags, "Transform Flags", ref dataChanged);
				_moveType = SerializationEditorGUILayout.ObjectField(_moveType, "Move Type", ref dataChanged);
				_easeType = SerializationEditorGUILayout.ObjectField(_easeType, "Ease Type", ref dataChanged);

				EditorGUI.BeginChangeCheck();
				_duration = EditorGUILayout.FloatField("Duration", _duration);
				dataChanged |= EditorGUI.EndChangeCheck();

				bool targetChanged = false;
				_targetType = SerializationEditorGUILayout.ObjectField(_targetType, "Target Type", ref targetChanged);
				if  (targetChanged)
				{
					dataChanged = true;

					_targetTransform = new GameObjectRef();
					_targetPosition = Vector3.zero;
					_targetRotation = Quaternion.identity;
					_targetScale = Vector3.one;
				}

				switch (_targetType)
				{
					case eTargetType.Transform:
						{
							_targetTransform = SerializationEditorGUILayout.ObjectField(_targetTransform, new GUIContent("Target Type"), ref dataChanged);
						}
						break;
					case eTargetType.LocalDelta:
					case eTargetType.LocalTarget:
					case eTargetType.WorldDelta:
					case eTargetType.WorldTarget:
						{
							if ((_transformFlags & eTransformFlag.Translate) != 0)
							{
								EditorGUI.BeginChangeCheck();
								_targetPosition = EditorGUILayout.Vector3Field("Translation", _targetPosition);
								dataChanged |= EditorGUI.EndChangeCheck();
							}
							if ((_transformFlags & eTransformFlag.Rotate) != 0)
							{
								EditorGUI.BeginChangeCheck();
								_targetRotation.eulerAngles = EditorGUILayout.Vector3Field("Rotation", _targetRotation.eulerAngles);
								dataChanged |= EditorGUI.EndChangeCheck();
							}
							if ((_transformFlags & eTransformFlag.Scale) != 0)
							{
								EditorGUI.BeginChangeCheck();
								_targetScale = EditorGUILayout.Vector3Field("Scale", _targetScale);
								dataChanged |= EditorGUI.EndChangeCheck();
							}
						}
						break;
				}


				return dataChanged;
			}
#endif
			#endregion

			#region Private Functions
			private void FindTargets(StateMachineComponent stateMachine)
			{
				if ((_transformFlags & eTransformFlag.Translate) != 0)
					FindStartPosition(stateMachine);

				if ((_transformFlags & eTransformFlag.Rotate) != 0)
					FindStartRotation(stateMachine);

				if ((_transformFlags & eTransformFlag.Scale) != 0)
					FindStartScale(stateMachine);
			}

			private void UpdateTransform(float lerp)
			{
				if ((_transformFlags & eTransformFlag.Translate) != 0)
					UpdateTargetPosition(lerp);

				if ((_transformFlags & eTransformFlag.Rotate) != 0)
					UpdateTargetRotation(lerp);

				if ((_transformFlags & eTransformFlag.Scale) != 0)
					UpdateTargetScale(lerp);
			}

			private void FindStartPosition(StateMachineComponent stateMachine)
			{
				switch (_targetType)
				{
					case eTargetType.LocalDelta:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartPosition = _transform.localPosition;
										_runTimeTargetPosition = _transform.localPosition + _targetPosition;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartPosition = _transform.localPosition + _targetPosition;
										_runTimeTargetPosition = _transform.localPosition;
									}
									break;
							}
						}
						break;
					case eTargetType.WorldDelta:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartPosition = _transform.position;
										_runTimeTargetPosition = _transform.position + _targetPosition;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartPosition = _transform.position + _targetPosition;
										_runTimeTargetPosition = _transform.position;
									}
									break;
							}
						}
						break;
					case eTargetType.LocalTarget:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartPosition = _transform.localPosition;
										_runTimeTargetPosition = _targetPosition;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartPosition = _targetPosition;
										_runTimeTargetPosition = _transform.localPosition;
									}
									break;
							}
						}
						break;
					case eTargetType.WorldTarget:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartPosition = _transform.position;
										_runTimeTargetPosition = _targetPosition;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartPosition = _targetPosition;
										_runTimeTargetPosition = _transform.position;
									}
									break;
							}
						}
						break;
					case eTargetType.Transform:
						{
							Transform transform = _runTimeTargetTransform != null ? _runTimeTargetTransform : _targetTransform.GetGameObject().transform;

							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartPosition = _transform.position;
										_runTimeTargetTransform = transform;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartPosition = transform.position;
										_runTimeTargetPosition = _transform.position;
									}
									break;
							}
						}
						break;
				}
			}

			private void FindStartRotation(StateMachineComponent stateMachine)
			{
				switch (_targetType)
				{
					case eTargetType.LocalDelta:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartRotation = _transform.localRotation;
										_runTimeTargetRotation = _transform.localRotation * _targetRotation;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartRotation = _transform.localRotation * _targetRotation;
										_runTimeTargetRotation = _transform.localRotation;
									}
									break;
							}
						}
						break;
					case eTargetType.WorldDelta:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartRotation = _transform.rotation;
										_runTimeTargetRotation = _transform.rotation * _targetRotation;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartRotation = _transform.rotation * _targetRotation;
										_runTimeTargetRotation = _transform.rotation;
									}
									break;
							}
						}
						break;
					case eTargetType.LocalTarget:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartRotation = _transform.localRotation;
										_runTimeTargetRotation = _targetRotation;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartRotation = _targetRotation;
										_runTimeTargetRotation = _transform.localRotation;
									}
									break;
							}
						}
						break;
					case eTargetType.WorldTarget:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartRotation = _transform.rotation;
										_runTimeTargetRotation = _targetRotation;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartRotation = _targetRotation;
										_runTimeTargetRotation = _transform.rotation;
									}
									break;
							}
						}
						break;
					case eTargetType.Transform:
						{
							Transform transform = _runTimeTargetTransform != null ? _runTimeTargetTransform : _targetTransform.GetGameObject().transform;

							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartRotation = _transform.rotation;
										_runTimeTargetTransform = transform;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartRotation = transform.rotation;
										_runTimeTargetRotation = _transform.rotation;
									}
									break;
							}
						}
						break;
				}
			}

			private void FindStartScale(StateMachineComponent stateMachine)
			{
				switch (_targetType)
				{
					case eTargetType.WorldDelta:
					case eTargetType.LocalDelta:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartScale = _transform.localScale;
										_runTimeTargetScale = _transform.localScale + _targetScale;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartScale = _transform.localScale + _targetScale;
										_runTimeTargetScale = _transform.localScale;
									}
									break;
							}
						}
						break;
					case eTargetType.WorldTarget:
					case eTargetType.LocalTarget:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartScale = _transform.localScale;
										_runTimeTargetScale = _targetScale;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartScale = _targetScale;
										_runTimeTargetScale = _transform.localScale;
									}
									break;
							}
						}
						break;
					case eTargetType.Transform:
						{
							Transform transform = _runTimeTargetTransform != null ? _runTimeTargetTransform : _targetTransform.GetGameObject().transform;

							switch (_moveType)
							{
								case eMoveType.To:
									{
										_runTimeStartScale = _transform.localScale;
										_runTimeTargetTransform = transform;
									}
									break;
								case eMoveType.From:
									{
										_runTimeStartScale = transform.localScale;
										_runTimeTargetScale = _transform.localScale;
									}
									break;
							}
						}
						break;
				}
			}			

			private void UpdateTargetPosition(float lerp)
			{
				switch (_targetType)
				{
					case eTargetType.LocalTarget:
					case eTargetType.LocalDelta:
						{
							_transform.localPosition = MathUtils.Interpolate(_easeType, _runTimeStartPosition, _runTimeTargetPosition, lerp);
						}
						break;
					case eTargetType.WorldTarget:
					case eTargetType.WorldDelta:
						{
							_transform.position = MathUtils.Interpolate(_easeType, _runTimeStartPosition, _runTimeTargetPosition, lerp);
						}
						break;
					case eTargetType.Transform:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_transform.position = MathUtils.Interpolate(_easeType, _runTimeStartPosition, _runTimeTargetTransform.position, lerp);
									}
									break;
								case eMoveType.From:
									{
										_transform.position = MathUtils.Interpolate(_easeType, _runTimeStartPosition, _runTimeTargetPosition, lerp);
									}
									break;
							}
						}
						break;
				}
			}

			private void UpdateTargetRotation(float lerp)
			{
				switch (_targetType)
				{
					case eTargetType.LocalTarget:
					case eTargetType.LocalDelta:
						{
							_transform.localRotation = MathUtils.Interpolate(_easeType, _runTimeStartRotation, _runTimeTargetRotation, lerp);
						}
						break;
					case eTargetType.WorldTarget:
					case eTargetType.WorldDelta:
						{
							_transform.rotation = MathUtils.Interpolate(_easeType, _runTimeStartRotation, _runTimeTargetRotation, lerp);
						}
						break;
					case eTargetType.Transform:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_transform.rotation = MathUtils.Interpolate(_easeType, _runTimeStartRotation, _runTimeTargetTransform.rotation, lerp);
									}
									break;
								case eMoveType.From:
									{
										_transform.rotation = MathUtils.Interpolate(_easeType, _runTimeStartRotation, _runTimeTargetRotation, lerp);
									}
									break;
							}
						}
						break;
				}
			}

			private void UpdateTargetScale(float lerp)
			{
				switch (_targetType)
				{
					case eTargetType.LocalTarget:
					case eTargetType.LocalDelta:
					case eTargetType.WorldTarget:
					case eTargetType.WorldDelta:
						{
							_transform.localScale = MathUtils.Interpolate(_easeType, _runTimeStartScale, _runTimeTargetScale, lerp);
						}
						break;
					case eTargetType.Transform:
						{
							switch (_moveType)
							{
								case eMoveType.To:
									{
										_transform.localScale = MathUtils.Interpolate(_easeType, _runTimeStartScale, _runTimeTargetTransform.localScale, lerp);
									}
									break;
								case eMoveType.From:
									{
										_transform.localScale = MathUtils.Interpolate(_easeType, _runTimeStartScale, _runTimeTargetScale, lerp);
									}
									break;
							}
						}
						break;
				}
			}

#if UNITY_EDITOR
			private string GetActionString()
			{
				string text = "";

				switch (_moveType)
				{
					case eMoveType.To: text = " to "; break;
					case eMoveType.From: text = " from "; break;
				}

				return text;
			}

			private string GetTypeString()
			{
				string text = " Transform";

				bool instant = _duration <= 0.0f;

				if ((_transformFlags & eTransformFlag.Translate) == eTransformFlag.Translate)
					text = instant ? " Set Position" : " Move";
				else if((_transformFlags & eTransformFlag.Rotate) == eTransformFlag.Rotate)
					text = instant ? " Set Rotation" : " Rotate";
				else if((_transformFlags & eTransformFlag.Scale) == eTransformFlag.Scale)
					text = instant ? " Set Scale" : " Scale";

				return text;
			}

			private string GetTargetString()
			{
				string text = "";

				switch (_targetType)
				{				
					case eTargetType.Transform: text = _targetTransform; break;
					case eTargetType.LocalDelta:
					case eTargetType.LocalTarget:
					case eTargetType.WorldDelta:
					case eTargetType.WorldTarget:
						{
							if ((_transformFlags & eTransformFlag.Translate) == eTransformFlag.Translate)
								text = _targetPosition.ToString();
							else if ((_transformFlags & eTransformFlag.Rotate) == eTransformFlag.Rotate)
								text = _targetRotation.ToString();
							else if((_transformFlags & eTransformFlag.Scale) == eTransformFlag.Scale)
								text = _targetScale.ToString();
							else
								text = "Transform";
						}
						break;
				}

				return text;
			}
#endif
			#endregion
		}
	}
}
