using System;
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
				[Flags]
				public enum Flags
				{
					Position = 1,
					Rotation = 2,
					Scale = 4,
				}
				public Flags _flags = Flags.Position & Flags.Rotation;
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
					
					if (_animator.GetRenderer() != null)
					{
						Initialise();
					}
					else
					{
						_animator._onInitialise += Initialise;
					}
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
				private void Initialise()
				{
					_boneTracking = _animator.GetRenderer().GetComponent<GPUAnimatorRendererBoneTracking>();
					_boneIndex = -1;

					if (_boneTracking != null)
					{
						_boneIndex = _boneTracking.GetBoneIndex(_boneName);
					}

					if (_boneIndex == -1)
						this.enabled = false;
				}

				private void UpdateBoneTransform()
				{
					float curAnimWeight = _animator.GetCurrentAnimationWeight();
					Matrix4x4 inverseBindPose = _boneTracking.GetInvBindPose(_boneIndex);

					CalcBoneTransform(_animator.GetCurrentAnimationFrame(), ref inverseBindPose, out _currentBonePosition, out _currentBoneRotation, out _currentBoneScale);

					if (curAnimWeight < 1.0f)
					{
						CalcBoneTransform(_animator.GetPreviousAnimationFrame(), ref inverseBindPose, out Vector3 prevPosition, out Quaternion prevRotation, out Vector3 prevScale);

						_currentBonePosition = Vector3.Lerp(prevPosition, _currentBonePosition, curAnimWeight);
						_currentBoneRotation = Quaternion.Slerp(prevRotation, _currentBoneRotation, curAnimWeight);
						_currentBoneScale = Vector3.Lerp(prevScale, _currentBoneScale, curAnimWeight);
					}

					//Convert to world space
					_currentBonePosition = _animator.transform.TransformPoint(_currentBonePosition);
					_currentBoneRotation = _animator.transform.rotation * _currentBoneRotation;

					UpdateTargetTransform();
				
				}

				private void CalcBoneTransform(float frame, ref Matrix4x4 inverseBindPose, out Vector3 position, out Quaternion rotation, out Vector3 scale)
				{
					int prevFrame = Mathf.FloorToInt(frame);
					int nextFrame = prevFrame + 1;
					float frameLerp = frame - prevFrame;

					//TO DO! improve and get scale working
					Matrix4x4 prevMatrix = _boneTracking.GetBoneMatrix(_boneIndex, prevFrame) * inverseBindPose;
					Matrix4x4 nextMatrix = _boneTracking.GetBoneMatrix(_boneIndex, nextFrame) * inverseBindPose;

					if ((_flags & Flags.Position) != 0)
						position = Vector3.Lerp(prevMatrix.MultiplyPoint3x4(Vector3.zero), nextMatrix.MultiplyPoint3x4(Vector3.zero), frameLerp);
					else
						position = Vector3.zero;

					if ((_flags & Flags.Rotation) != 0)
						rotation = Quaternion.Slerp(prevMatrix.rotation, nextMatrix.rotation, frameLerp);
					else
						rotation = Quaternion.identity;

					if ((_flags & Flags.Scale) != 0)
						scale = Vector3.one;
					else
						scale = Vector3.one;
				}

				private void UpdateTargetTransform()
				{
					if (_targetTransform != null)
					{
						if ((_flags & Flags.Position) != 0)
							_targetTransform.position = _currentBonePosition;

						if ((_flags & Flags.Rotation) != 0)
							_targetTransform.rotation = _currentBoneRotation;

						if ((_flags & Flags.Scale) != 0)
							_targetTransform.localScale = _currentBoneScale;
					}
				}
				#endregion
			}
		}
	}
}
