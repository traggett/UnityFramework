using UnityEngine;
using System;


namespace Framework
{
	using Maths;

	namespace AnimationSystem
	{
		public class AnimatedCameraState : ScriptableObject
		{
			[HideInInspector]
			public Vector3 _position;
			[HideInInspector]
			public Quaternion _rotation;
			public float _fieldOfView = 60.0f;
			public Rect _cameraRect = new Rect(0, 0, 1, 1);
			public UnityEngine.Object _extraData;

			public virtual AnimatedCameraState InterpolateTo(AnimatedCamera camera, AnimatedCameraState to, eInterpolation ease, float t)
			{
				AnimatedCameraState state = CreateInstance<AnimatedCameraState>();

				state._position = MathUtils.Interpolate(ease, _position, to._position, t);
				state._rotation = MathUtils.Interpolate(ease, _rotation, to._rotation, t);
				state._fieldOfView = MathUtils.Interpolate(ease, _fieldOfView, to._fieldOfView, t);
				state._cameraRect = MathUtils.Interpolate(ease, _cameraRect, to._cameraRect, t);

				return state;
			}
		}
	}
}