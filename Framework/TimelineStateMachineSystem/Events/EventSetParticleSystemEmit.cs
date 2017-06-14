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
		public class EventSetParticleSystemEmit : Event, IStateMachineEvent
		{
			#region Public Data
			public ComponentRef<ParticleSystem> _particleSystem = new ComponentRef<ParticleSystem>();
			public bool _emission = false;
			#endregion

			#region Event
#if UNITY_EDITOR
			public override Color GetColor()
			{
				return Color.blue;
			}

			public override string GetEditorDescription()
			{
				return "Set ParticleSystem Emit: " + _emission;
			}
#endif
			#endregion

			#region IStateMachineSystemEvent
			public eEventTriggerReturn Trigger(StateMachine stateMachine)
			{
				ParticleSystem particleSystem = _particleSystem;

				if (particleSystem != null)
				{
					ParticleSystem.EmissionModule emission = particleSystem.emission;
					emission.enabled = _emission;
				}

				return eEventTriggerReturn.EventFinished;
			}

			public eEventTriggerReturn Update(StateMachine stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachine stateMachine)
			{
			}

#if UNITY_EDITOR
			public EditorStateLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}
