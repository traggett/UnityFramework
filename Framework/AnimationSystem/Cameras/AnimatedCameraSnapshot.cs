using UnityEngine;

namespace Framework
{
	namespace AnimationSystem
	{
		public class AnimatedCameraSnapshot : MonoBehaviour, IAnimatedCameraStateSource
		{
			#region Public Data
			[SerializeField]
			private AnimatedCameraState _state;
			#endregion

#if UNITY_EDITOR
			private void Update()
			{
				//Update position / rotation
				GetState();
			}
#endif

			#region IAnimatedCameraStateSource
			public AnimatedCameraState GetState()
			{
				if (_state == null)
					_state = new AnimatedCameraState();

				_state._position = this.transform.position;
				_state._rotation = this.transform.rotation;
				return _state;
			}
			
#if UNITY_EDITOR
			public void SetState(AnimatedCameraState state)
			{
				if (state != null)
				{
					this.transform.position = state._position;
					this.transform.rotation = state._rotation;
				}
				_state = state;
			}

			public string GetName()
			{
				return gameObject.name;
			}
#endif
			#endregion
		}
	}
}