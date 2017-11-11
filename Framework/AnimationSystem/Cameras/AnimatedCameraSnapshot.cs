using UnityEngine;

namespace Framework
{
	namespace AnimationSystem
	{
		public class AnimatedCameraSnapshot : MonoBehaviour
		{
			#region Public Data
			public AnimatedCameraState _state;
			#endregion

			public AnimatedCameraState GetState()
			{
				if (_state == null)
					_state = ScriptableObject.CreateInstance<AnimatedCameraState>();

				_state._position = this.transform.position;
				_state._rotation = this.transform.rotation;
				return _state;
			}
			
#if UNITY_EDITOR
			public virtual void SetState(AnimatedCameraState state)
			{
				this.transform.position = state._position;
				this.transform.rotation = state._rotation;
				_state = state;
			}
#endif
		}
	}
}