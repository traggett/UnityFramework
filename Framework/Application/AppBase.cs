using UnityEngine;

namespace Framework
{
	using StateMachineSystem;
	using Utils;
	
	public abstract class AppBase : MonoBehaviour
	{
		#region Unity Messages
		void Awake()
		{
			
		}

		void Update()
		{
			UpdateApp();
		}

		void LateUpdate()
		{
			LateUpdateApp();
		}

		void OnApplicationQuit()
		{
			Resources.UnloadUnusedAssets();
			GameObjectUtils._isShuttingDown = true;
		}
		#endregion

		protected void InitApp()
		{
			
		}

		protected void UpdateApp()
		{
			
		}

		protected void LateUpdateApp()
		{
			StateMachine.ClearMessages();
		}
	}
}