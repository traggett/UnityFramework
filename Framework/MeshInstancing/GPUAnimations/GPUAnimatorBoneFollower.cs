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

					bool followPosition = (_flags & Flags.Position) != 0;
					bool followRotation = (_flags & Flags.Rotation) != 0;
					bool followScale = (_flags & Flags.Scale) != 0;

					//Work out local space bone transform
					float curAnimWeight = _animator.GetMainAnimationWeight();
					_boneTracking.GetBoneTransform(_boneIndex, _animator.GetMainAnimationFrame(), followPosition, followRotation, followScale, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale);

					if (curAnimWeight < 1.0f)
					{
						_boneTracking.GetBoneTransform(_boneIndex, _animator.GetBackgroundAnimationFrame(), followPosition, followRotation, followScale, out Vector3 backgroundLocalPosition, out Quaternion backgroundLocalRotation, out Vector3 backgroundLocalScale);

						if (followPosition)
							localPosition = Vector3.Lerp(backgroundLocalPosition, localPosition, curAnimWeight);
						if (followRotation)
							localRotation = Quaternion.Slerp(backgroundLocalRotation, localRotation, curAnimWeight);
						if (followScale)
							localScale = Vector3.Lerp(backgroundLocalScale, localScale, curAnimWeight);
					}

					//Convert to world space
					if (followPosition)
						_worldBonePosition = _animator.transform.TransformPoint(localPosition);
					if (followRotation)
						_worldBoneRotation = _animator.transform.rotation * localRotation;
					if (followScale)
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
