using UnityEngine;

namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		[RequireComponent(typeof(Collider))]
		public class TriggerMsgSender : MonoBehaviour
		{
			private Collider _collider;

			void Awake()
			{
				_collider = GetComponent<Collider>();
			}


			void OnTriggerEnter(Collider collider)
			{
				StateMachine.TriggerMessage(new MsgOnTriggerEnter(_collider, collider));
			}
		}
	}
}