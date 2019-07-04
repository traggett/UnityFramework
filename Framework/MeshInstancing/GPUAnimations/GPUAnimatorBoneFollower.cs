using UnityEngine;

namespace Framework
{
	using Utils;
	using Maths;
	using System;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public class GPUAnimatorBoneFollower : MonoBehaviour
			{
				#region Public Data
				public GPUAnimatorBase _animator;
				public string _boneName;
				public Transform _targetTransform;
				public TransformFlags _flags = TransformFlags.Translate & TransformFlags.Rotate;
				#endregion

				#region Private Data
				private int _exposedBoneIndex;
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
					_worldBonePosition = Vector3.zero;
					_worldBoneRotation = Quaternion.identity;
					_worldBonePosition = Vector3.one;

					_exposedBoneIndex = GetExposedBoneIndex(_animator.GetRenderer()._animations.GetAnimations(), _boneName);

					if (_exposedBoneIndex == -1)
						this.enabled = false;
				}

				private void UpdateBoneTransform()
				{
					bool followPosition = (_flags & TransformFlags.Translate) != 0;
					bool followRotation = (_flags & TransformFlags.Rotate) != 0;
					bool followScale = (_flags & TransformFlags.Scale) != 0;

					GPUAnimations animations = _animator.GetRenderer()._animations.GetAnimations();

					//Work out local space bone transform
					float curAnimWeight = _animator.GetMainAnimationWeight();
					GetExposedBoneTransform(animations, _exposedBoneIndex, _animator.GetMainAnimationFrame(), _flags, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale);

					if (curAnimWeight < 1.0f)
					{
						GetExposedBoneTransform(animations, _exposedBoneIndex, _animator.GetBackgroundAnimationFrame(), _flags, out Vector3 backgroundLocalPosition, out Quaternion backgroundLocalRotation, out Vector3 backgroundLocalScale);

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
						if ((_flags & TransformFlags.Translate) != 0)
							_targetTransform.position = _worldBonePosition;

						if ((_flags & TransformFlags.Rotate) != 0)
							_targetTransform.rotation = _worldBoneRotation;

						if ((_flags & TransformFlags.Scale) != 0)
							GameObjectUtils.SetTransformWorldScale(_targetTransform, _worldBoneScale);
					}
				}

				private static int GetExposedBoneIndex(GPUAnimations animations, string boneName)
				{
					if (animations != null)
					{
						int boneIndex = -1;

						string[] boneNames = animations._bones;

						for (int i = 0; i < boneNames.Length; i++)
						{
							if (boneNames[i] == boneName)
							{
								boneIndex = i;
								break;
							}
						}

						if (boneIndex != -1)
						{
							for (int i = 0; i < animations._exposedBones.Length; i++)
							{
								if (animations._exposedBones[i]._boneIndex == boneIndex)
								{
									return i;
								}
							}
						}
					}

					return -1;
				}

				private static void GetExposedBoneTransform(GPUAnimations animations, int exposedBoneIndex, float frame, TransformFlags flags, out Vector3 position, out Quaternion rotation, out Vector3 scale)
				{
					int totalSamples = animations._exposedBones[exposedBoneIndex]._cachedBoneMatrices.Length;

					int prevFrame = Mathf.FloorToInt(frame);
					int nextFrame = Math.Min(prevFrame + 1, totalSamples - 1);
					float frameLerp = frame - prevFrame;

					if ((flags & TransformFlags.Translate) != 0)
					{
						Vector3 prevFramePos = animations._exposedBones[exposedBoneIndex]._cachedBoneMatrices[prevFrame].MultiplyPoint3x4(Vector3.zero);
						Vector3 nextFramePos = animations._exposedBones[exposedBoneIndex]._cachedBoneMatrices[nextFrame].MultiplyPoint3x4(Vector3.zero);
						position = Vector3.Lerp(prevFramePos, nextFramePos, frameLerp);
					}
					else
					{
						position = Vector3.zero;
					}

					if ((flags & TransformFlags.Rotate) != 0)
					{
						Quaternion prevRotation = animations._exposedBones[exposedBoneIndex]._cachedBoneMatrices[prevFrame].rotation;
						Quaternion nextRotation = animations._exposedBones[exposedBoneIndex]._cachedBoneMatrices[nextFrame].rotation;
						rotation = Quaternion.Slerp(prevRotation, nextRotation, frameLerp);
					}
					else
					{
						rotation = Quaternion.identity;
					}

					if ((flags & TransformFlags.Scale) != 0)
					{
						Vector3 prevScale = animations._exposedBones[exposedBoneIndex]._cachedBoneMatrices[prevFrame].lossyScale;
						Vector3 nextScale = animations._exposedBones[exposedBoneIndex]._cachedBoneMatrices[nextFrame].lossyScale;
						scale = Vector3.Lerp(prevScale, nextScale, frameLerp);
					}
					else
					{
						scale = Vector3.one;
					}
				}
				#endregion
			}
		}
	}
}
