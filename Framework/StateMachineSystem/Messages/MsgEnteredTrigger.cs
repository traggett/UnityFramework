using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
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