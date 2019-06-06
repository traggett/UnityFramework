using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			[RequireComponent(typeof(GPUAnimatorBase))]
			public class GPUAnimatorBoneFollower : MonoBehaviour
			{
				#region Public Data
				public string _boneName;
				public Transform _targetTransform;
				#endregion

				#region Private Data
				private GPUAnimatorBase _animator;
				private GPUAnimatorRendererBoneTracking _boneTracking;
				private int _boneIndex;
				private Vector3 _currentBonePosition;
				private Quaternion _currentBoneRotation;
				private Vector3 _currentBoneScale;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_animator = GetComponent<GPUAnimatorBase>();

					//TO DO! need to hook into on init in animator base
					_boneTracking = _animator._renderer.GetComponent<GPUAnimatorRendererBoneTracking>();
					_boneIndex = -1;

					if (_boneTracking != null)
					{
						_boneIndex = _boneTracking.GetBoneIndex(_boneName);
					}

					if (_boneIndex == -1)
						this.enabled = false;
				}

				private void LateUpdate()
				{
					UpdateBoneTransform();
				}
				#endregion

				#region Public Interface
				public Vector3 GetBoneWorldPosition()
				{
					return _currentBonePosition;
				}

				public Quaternion GetBoneWorldRotation()
				{
					return _currentBoneRotation;
				}
				#endregion

				#region Private Functions
				private void UpdateBoneTransform()
				{
					float curFrameWeight = _animator.GetCurrentAnimationWeight();
					bool lerpingFromPrev = curFrameWeight < 1.0f;

					float curFrame = _animator.GetCurrentAnimationFrame();
					int prevFrame = Mathf.FloorToInt(curFrame);
					int nextFrame = prevFrame + 1;
					float frameLerp = curFrame - prevFrame;

					Matrix4x4 inverseBindPose = _boneTracking.GetInvBindPose(_boneIndex);

					CalcBoneTransform(prevFrame, nextFrame, frameLerp, _boneIndex, ref inverseBindPose, out _currentBonePosition, out _currentBoneRotation, out _currentBoneScale);

					if (lerpingFromPrev)
					{
						float prevCurFrame = _animator.GetPreviousAnimationFrame();
						int prevPrevFrame = Mathf.FloorToInt(prevCurFrame);
						int prevNextFrame = prevPrevFrame + 1;
						float prevFrameLerp = prevCurFrame - prevPrevFrame;

						CalcBoneTransform(prevPrevFrame, prevNextFrame, prevFrameLerp, _boneIndex, ref inverseBindPose, out Vector3 prevPosition, out Quaternion prevRotation, out Vector3 prevScale);

						_currentBonePosition = Vector3.Lerp(prevPosition, _currentBonePosition, curFrameWeight);
						_currentBoneRotation = Quaternion.Slerp(prevRotation, _currentBoneRotation, curFrameWeight);
						_currentBoneScale = Vector3.Lerp(prevScale, _currentBoneScale, curFrameWeight);
					}

					//Convert to world space
					_currentBonePosition = _animator.transform.TransformPoint(_currentBonePosition);
					_currentBoneRotation = _animator.transform.rotation * _currentBoneRotation;

					UpdateTargetTransform();
				
				}

				private void CalcBoneTransform(int prevFrame, int nextFrame, float frameLerp, int boneIndex, ref Matrix4x4 inverseBindPose, out Vector3 position, out Quaternion rotation, out Vector3 scale)
				{
					//TO DO! improve and get scale working
					Matrix4x4 prevMatrix = _boneTracking.GetBoneMatrix(boneIndex, prevFrame) * inverseBindPose;
					Matrix4x4 nextMatrix = _boneTracking.GetBoneMatrix(boneIndex, nextFrame) * inverseBindPose;

					position = Vector3.Lerp(prevMatrix.MultiplyPoint3x4(Vector3.zero), nextMatrix.MultiplyPoint3x4(Vector3.zero), frameLerp);
					rotation = Quaternion.Slerp(prevMatrix.rotation, nextMatrix.rotation, frameLerp);
					scale = Vector3.one;
				}

				private void UpdateTargetTransform()
				{
					if (_targetTransform != null)
					{
						_targetTransform.position = _currentBonePosition;
						_targetTransform.rotation = _currentBoneRotation;
						_targetTransform.localScale = _currentBoneScale;
					}
				}
				#endregion
			}
		}
	}
}
