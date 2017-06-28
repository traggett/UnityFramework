using UnityEngine;

namespace Framework
{
	namespace AnimationSystem
	{
		public class AnimatedCameraSnapshot : MonoBehaviour
		{
			#region Public Data
			public Rect _cameraRect = new Rect(0,0,1,1);
			public float _fieldOfView = 60.0f;
			#endregion

#if UNITY_EDITOR
			public virtual void SetFromCamera(AnimatedCamera camera)
			{
				this.transform.position = camera.transform.position;
				this.transform.rotation = camera.transform.rotation;
				_cameraRect = camera.GetCamera().rect;
				_fieldOfView = camera.GetCamera().fieldOfView;
			}
#endif
		}
	}
}