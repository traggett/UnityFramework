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
		public class EventSetParticleSystemEmit : Event, ITimelineStateEvent
		{
			#region Public Data
			public ComponentRef<ParticleSystem> _particleSystem;
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
			public eEventTriggerReturn Trigger(StateMachineComponent stateMachine)
			{
				ParticleSystem particleSystem = _particleSystem;

				if (particleSystem != null)
				{
					ParticleSystem.EmissionModule emission = particleSystem.emission;
					emission.enabled = _emission;
				}

				return eEventTriggerReturn.EventFinished;
			}

			public eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime)
			{
				return eEventTriggerReturn.EventOngoing;
			}

			public void End(StateMachineComponent stateMachine)
			{
			}

#if UNITY_EDITOR
			public StateMachineEditorLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}
