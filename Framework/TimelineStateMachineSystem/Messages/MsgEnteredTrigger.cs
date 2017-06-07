using UnityEngine;

namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		public class MsgOnTriggerEnter : IStateMachineMsg
		{
			public Collider _trigger;
			public Collider _collider;

			public MsgOnTriggerEnter(Collider trigger, Collider collider)
			{
				_trigger = trigger;
				_collider = collider;
			}
		}
	}
}