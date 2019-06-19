using System;
using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public class GPUAnimatorBoneFollower : MonoBehaviour
			{
				#region Public Data
				[Flags]
				public enum Flags
				{
					Position = 1,
					Rotation = 2,
					Scale = 4,
				}
				public GPUAnimatorBase _animator;
				public string _boneName;
				public Transform _targetTransform;
				public Flags _flags = Flags.Position & Flags.Rotation;
				#endregion

				#region Private Data
				private GPUAnimatorRendererBoneTracking _boneTracking;
				private int _boneIndex;
				private Vector3 _worldBonePosition;
				private Quaternion _worldBoneRotation;
				private Vector3 _worldBoneScale;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					if (_animator != null)
					{
						if (_animator.GetRenderer() != null)
						{
							Initialise();
						}
						else
						{
							_animator._onInitialise += Initialise;
						}
					}
					else
					{
						this.enabled = false;
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
					return _worldBonePosition;
				}

				public Quaternion GetBoneWorldRotation()
				{
					return _worldBoneRotation;
				}

				public Vector3 GetBoneLossyScale()
				{
					return _worldBoneScale;
				}
				#endregion

				#region Private Functions
				private void Initialise()
				{
					_boneTracking = _animator.GetRenderer().GetComponent<GPUAnimatorRendererBoneTracking>();
					_boneIndex = -1;
					_worldBonePosition = Vector3.zero;
					_worldBoneRotation = Quaternion.identity;
					_worldBonePosition = Vector3.one;

					if (_boneTracking != null)
					{
						_boneIndex = _boneTracking.GetBoneIndex(_boneName);
					}

					if (_boneIndex == -1)
						this.enabled = false;
				}

				private void UpdateBoneTransform()
				{
					if (_boneTracking == null)
						return;

					//Work out local space bone transform
					float curAnimWeight = _animator.GetMainAnimationWeight();
					_boneTracking.GetBoneTransform(_boneIndex, _animator.GetMainAnimationFrame(), _flags, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale);

					if (curAnimWeight < 1.0f)
					{
						_boneTracking.GetBoneTransform(_boneIndex, _animator.GetBackgroundAnimationFrame(), _flags, out Vector3 prevLocalPosition, out Quaternion prevLocalRotation, out Vector3 prevLocalScale);

						if ((_flags & Flags.Position) != 0)
							localPosition = Vector3.Lerp(prevLocalPosition, localPosition, curAnimWeight);
						if ((_flags & Flags.Rotation) != 0)
							localRotation = Quaternion.Slerp(prevLocalRotation, localRotation, curAnimWeight);
						if ((_flags & Flags.Scale) != 0)
							localScale = Vector3.Lerp(prevLocalScale, localScale, curAnimWeight);
					}

					//Convert to world space
					if ((_flags & Flags.Position) != 0)
						_worldBonePosition = _animator.transform.TransformPoint(localPosition);
					if ((_flags & Flags.Rotation) != 0)
						_worldBoneRotation = _animator.transform.rotation * localRotation;
					if ((_flags & Flags.Scale) != 0)
						_worldBoneScale = Vector3.Scale(_animator.transform.lossyScale, localScale);

					UpdateTargetTransform();
				}
				
				private void UpdateTargetTransform()
				{
					if (_targetTransform != null)
					{
						if ((_flags & Flags.Position) != 0)
							_targetTransform.position = _worldBonePosition;

						if ((_flags & Flags.Rotation) != 0)
							_targetTransform.rotation = _worldBoneRotation;

						if ((_flags & Flags.Scale) != 0)
							GameObjectUtils.SetTransformWorldScale(_targetTransform, _worldBoneScale);
					}
				}
				#endregion
			}
		}
	}
}
