using UnityEngine;

namespace Framework
{
	using StateMachineSystem;
	using MidiSystem;
	using Utils;

	public abstract class AppBase : MonoBehaviour
	{
		#region MonoBehaviour Calls
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
			MidiSequencer.Init();
		}

		protected void UpdateApp()
		{
			MidiSequencer.Update();
		}

		protected void LateUpdateApp()
		{
			StateMachineComponent.ClearMessages();
		}
	}
}