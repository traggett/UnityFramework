using UnityEngine;

namespace Framework
{
	using Maths;

	namespace AnimationSystem
	{
		public class AnimatedCamera : MonoBehaviour
		{
			#region Private Data
			private Camera _camera;
			private AnimatedCameraSnapshot[] _snapshots;
			private eInterpolation _easeType;
			private float _animationTime;
			private WrapMode _wrapMode;
			private float _currentAnimationT;
			#endregion

			#region MonoBehaviour Calls
			protected void Update()
			{
				if (_snapshots != null)
				{
					if (_snapshots.Length == 1)
					{
						SetFromSnapshot(_snapshots[0]);
					}
					else if (_snapshots.Length > 1)
					{
						_currentAnimationT += Time.deltaTime / _animationTime;

						if (_wrapMode == WrapMode.ClampForever)
						{
							_currentAnimationT = Mathf.Clamp01(_currentAnimationT);
						}
						else if (_wrapMode == WrapMode.Once)
						{
							if (_currentAnimationT > 1.0f)
							{
								_snapshots = null;
							}
						}

						float baseValue = Mathf.Floor(_currentAnimationT);
						float fraction = _currentAnimationT - baseValue;

						if (_wrapMode == WrapMode.PingPong && ((int)baseValue % 2) == 1)
						{
							fraction = 1.0f - fraction;
						}

						float sectonTDist = 1.0f / (_snapshots.Length - 1);
						float sectionT = fraction / sectonTDist;

						int sectionIndex = Mathf.FloorToInt(sectionT);
						sectionT = sectionT - (float)sectionIndex;

						SetFromSnapshots(_snapshots[sectionIndex], _snapshots[sectionIndex + 1], _easeType, sectionT);
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
			
			public void SetAnimation(AnimatedCameraSnapshot[] snapshots, eInterpolation easeType, float time, WrapMode wrapMode)
			{
				//Event if you pass in one snapshot, want to blend up to.
				//Should there be difference between a loop time and a blend time?



				_snapshots = snapshots;
				_easeType = easeType;
				_animationTime = time;
				_wrapMode = wrapMode;

				if (wrapMode == WrapMode.Default)
				{
					wrapMode = WrapMode.PingPong;
				}

				_currentAnimationT = 0.0f;

				if (snapshots.Length > 0 && snapshots[0] != null)
				{
					SetFromSnapshot(snapshots[0]);
				}
			}

			public virtual void SetFromSnapshot(AnimatedCameraSnapshot snapshot)
			{
				this.transform.position = snapshot.transform.position;
				this.transform.rotation = snapshot.transform.rotation;
				GetCamera().fieldOfView = snapshot._fieldOfView;
				GetCamera().rect = snapshot._cameraRect;
			}

			public virtual void SetFromSnapshots(AnimatedCameraSnapshot snapshotFrom, AnimatedCameraSnapshot snapshotTo, eInterpolation easeType, float t)
			{
				this.transform.position = MathUtils.Interpolate(easeType, snapshotFrom.transform.position, snapshotTo.transform.position, t);
				this.transform.rotation = MathUtils.Interpolate(easeType, snapshotFrom.transform.rotation, snapshotTo.transform.rotation, t);
				GetCamera().fieldOfView = MathUtils.Interpolate(easeType, snapshotFrom._fieldOfView, snapshotTo._fieldOfView, t);
				GetCamera().rect = MathUtils.Interpolate(easeType, snapshotFrom._cameraRect, snapshotTo._cameraRect, t);
			}

			public virtual AnimatedCameraSnapshot CreateSnapshot(string name)
			{
				GameObject newObj = new GameObject(name);
				newObj.transform.parent = this.transform.parent;
				AnimatedCameraSnapshot snapshot = newObj.AddComponent<AnimatedCameraSnapshot>();
				return snapshot;
			}
			#endregion
		}
	}
}