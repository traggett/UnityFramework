using UnityEngine;

namespace Framework
{
	using Maths;
	using System.Collections.Generic;

	namespace AnimationSystem
	{
		public class AnimatedCamera : MonoBehaviour
		{
			public class Animation
			{
				public AnimatedCameraSnapshot[] _snapshots;
				public float _duration;
				public eInterpolation _easeType;
				public WrapMode _wrapMode;
				public float _animationT;
				public float _weight;
			}

			#region Private Data
			private Camera _camera;
			private List<Animation> _animations = new List<Animation>();
			private float _currentAnimationBlendTime;
			protected eInterpolation _currentEaseType;
			#endregion

			#region MonoBehaviour Calls
			protected virtual void Update()
			{
				if (_animations.Count > 0)
				{
					float deltaTime = Time.deltaTime;

					_animations[_animations.Count - 1]._weight += deltaTime;

					if (_animations[_animations.Count - 1]._weight >= 1.0f)
					{
						_animations[_animations.Count - 1]._weight = 1.0f;
						//Stop all other animations
						_animations = new List<Animation>() { _animations[_animations.Count - 1] };
					}

					foreach (Animation animation in _animations)
					{
						UpdateAnimation(animation, deltaTime);
					}
				}
			}
			#endregion

			#region Public Functions
			public Camera GetCamera()
			{
				if (_camera == null)
					_camera = GetComponentInChildren<Camera>();

				return _camera;
			}
			
			public void SetAnimation(Animation animation, eInterpolation easeType, float blendTime)
			{
				_currentAnimationBlendTime = blendTime;
				_currentEaseType = easeType;
				animation._animationT = 0.0f;
				if (blendTime > 0.0f)
				{
					animation._weight = 0.0f;
					_animations.Add(animation);
				}			
				else
				{
					animation._weight = 1.0f;
					_animations = new List<Animation>() { animation };
				}	
			}

			public virtual void SetFromSnapshot(AnimatedCameraSnapshot snapshot, float weight = 1.0f)
			{
				this.transform.position = MathUtils.Interpolate(_currentEaseType, this.transform.position, snapshot.transform.position, weight);
				this.transform.rotation = MathUtils.Interpolate(_currentEaseType, this.transform.rotation, snapshot.transform.rotation, weight);
				GetCamera().fieldOfView = MathUtils.Interpolate(_currentEaseType, GetCamera().fieldOfView, snapshot._fieldOfView, weight);
				GetCamera().rect = MathUtils.Interpolate(_currentEaseType, GetCamera().rect, snapshot._cameraRect, weight);
			}

			public virtual void SetFromSnapshots(AnimatedCameraSnapshot snapshotFrom, AnimatedCameraSnapshot snapshotTo, eInterpolation easeType, float t, float weight = 1.0f)
			{
				this.transform.position = MathUtils.Interpolate(_currentEaseType, this.transform.position, MathUtils.Interpolate(easeType, snapshotFrom.transform.position, snapshotTo.transform.position, t), weight);
				this.transform.rotation = MathUtils.Interpolate(_currentEaseType, this.transform.rotation, MathUtils.Interpolate(easeType, snapshotFrom.transform.rotation, snapshotTo.transform.rotation, t), weight);
				GetCamera().fieldOfView = MathUtils.Interpolate(_currentEaseType, GetCamera().fieldOfView, MathUtils.Interpolate(easeType, snapshotFrom._fieldOfView, snapshotTo._fieldOfView, t), weight);
				GetCamera().rect = MathUtils.Interpolate(_currentEaseType, GetCamera().rect, MathUtils.Interpolate(easeType, snapshotFrom._cameraRect, snapshotTo._cameraRect, t), weight);
			}

			public virtual AnimatedCameraSnapshot CreateSnapshot(string name)
			{
				GameObject newObj = new GameObject(name);
				newObj.transform.parent = this.transform.parent;
				AnimatedCameraSnapshot snapshot = newObj.AddComponent<AnimatedCameraSnapshot>();
				return snapshot;
			}
			#endregion

			private void UpdateAnimation(Animation animation, float deltaTime)
			{
				if (animation._snapshots.Length == 1)
				{
					SetFromSnapshot(animation._snapshots[0], animation._weight);
				}
				else if (animation._snapshots.Length > 1)
				{
					animation._animationT += deltaTime / animation._duration;

					if (animation._wrapMode == WrapMode.ClampForever)
					{
						animation._animationT = Mathf.Clamp01(animation._animationT);
					}
					else if (animation._wrapMode == WrapMode.Once)
					{
						if (animation._animationT > 1.0f)
						{
							animation._snapshots = null;
						}
					}

					float baseValue = Mathf.Floor(animation._animationT);
					float fraction = animation._animationT - baseValue;

					if (animation._wrapMode == WrapMode.PingPong && ((int)baseValue % 2) == 1)
					{
						fraction = 1.0f - fraction;
					}

					float sectonTDist = 1.0f / (animation._snapshots.Length - 1);
					float sectionT = fraction / sectonTDist;

					int sectionIndex = Mathf.FloorToInt(sectionT);
					sectionT = sectionT - (float)sectionIndex;

					SetFromSnapshots(animation._snapshots[sectionIndex], animation._snapshots[sectionIndex + 1], animation._easeType, sectionT, animation._weight);
				}
			}
		}
	}
}