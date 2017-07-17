using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
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