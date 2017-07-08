using UnityEngine;
using System;

namespace Framework
{
	using StateMachineSystem;
	using TimelineSystem;
	using Utils;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventSetMaterialColor : Event, IStateMachineEvent
		{
			#region Public Data
			public MaterialRef _material;
			public string _propertyName = "_Color";
			public Color _color = Color.black;
			public float _duration = 0.0f;
			#endregion

			private Color _origColor;


			#region Event
			public override float GetDuration()
			{
				return _duration;
			}

#if UNITY_EDITOR
			public override Color GetColor()
			{
				return new Color(_color.r, _color.g, _color.b);
			}

			public override string GetEditorDescription()
			{
				return "Set Material Color ";
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachine stateMachine)
			{
				Material material = _material;

				if (material != null)
				{
					if (_duration > 0.0f)
					{
						_origColor = material.GetColor(_propertyName);
						return eEventTriggerReturn.EventOngoing;
					}
					else
					{
						material.SetColor(_propertyName, _color);
					}
				}

				return eEventTriggerReturn.EventFinished;
			}

			public eEventTriggerReturn Update(StateMachine stateMachine, float eventTime)
			{
				float lerp = eventTime / _duration;
				Color color = Color.Lerp(_origColor, _color, lerp);
				Material material = _material;
				material.SetColor(_propertyName, color);

				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachine stateMachine)
			{
				Material material = _material;
				material.SetColor(_propertyName, _color);
			}

#if UNITY_EDITOR
			public EditorStateLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}
