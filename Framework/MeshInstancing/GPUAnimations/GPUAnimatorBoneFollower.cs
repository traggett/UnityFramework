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
				public TransformFlags _flags = TransformFlags.Translate & TransformFlags.Rotate;
				#endregion

				#region Private Data
				private bool _initialised;
				private int _exposedBoneIndex;
				private int _exposedBoneNumSamples;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					if (_animator != null)
					{
						if (_animator.IsInitialised())
						{
							Initialise();
						}
						else
						{
							_animator._onInitialise += Initialise;
						}
					}
				}

				private void LateUpdate()
				{
					if (_initialised)
						UpdateBoneTransform();
				}
				#endregion

				#region Private Functions
				private void Initialise()
				{
					_initialised = true;
					
					GPUAnimations animations = _animator.GetRenderer()._animations.GetAnimations();

					_exposedBoneIndex = GetExposedBoneIndex(animations, _boneName);
					_exposedBoneNumSamples = animations._exposedBones[_exposedBoneIndex]._cachedBonePositions.Length;

					if (_exposedBoneIndex == -1)
						this.enabled = false;
				}

				private void UpdateBoneTransform()
				{
					bool followPosition = (_flags & TransformFlags.Translate) != 0;
					bool followRotation = (_flags & TransformFlags.Rotate) != 0;
					bool followScale = (_flags & TransformFlags.Scale) != 0;

					Vector3 localPosition;
					Quaternion localRotation;
					Vector3 localScale;

					//Work out local space bone transform
					{
						GPUAnimations animations = _animator.GetRenderer()._animations.GetAnimations();

						float curAnimWeight = _animator.GetMainAnimationWeight();
						GetFrameInfo(_animator.GetMainAnimationFrame(), out int prevFrame, out int nextFrame, out float frameLerp);
						animations._exposedBones[_exposedBoneIndex].GetBoneTransform(prevFrame, nextFrame, frameLerp, followPosition, followRotation, followScale, out localPosition, out localRotation, out localScale);

						if (curAnimWeight < 1.0f)
						{
							GetFrameInfo(_animator.GetBackgroundAnimationFrame(), out prevFrame, out nextFrame, out frameLerp);
							animations._exposedBones[_exposedBoneIndex].GetBoneTransform(prevFrame, nextFrame, frameLerp, followPosition, followRotation, followScale, out Vector3 backgroundLocalPosition, out Quaternion backgroundLocalRotation, out Vector3 backgroundLocalScale);

							if (followPosition)
								localPosition = Vector3.Lerp(backgroundLocalPosition, localPosition, curAnimWeight);
							if (followRotation)
								localRotation = Quaternion.Slerp(backgroundLocalRotation, localRotation, curAnimWeight);
							if (followScale)
								localScale = Vector3.Lerp(backgroundLocalScale, localScale, curAnimWeight);
						}
					}
					
					//Set world transform from this
					{
						if (followPosition && followRotation)
						{
							this.transform.SetPositionAndRotation(_animator.transform.TransformPoint(localPosition), _animator.transform.rotation * localRotation);
						}
						else if (followPosition)
						{
							this.transform.position = _animator.transform.TransformPoint(localPosition);
						}
						else if (followRotation)
						{
							this.transform.rotation = _animator.transform.rotation * localRotation;
						}

						if (followScale)
						{
							GameObjectUtils.SetTransformWorldScale(this.transform, Vector3.Scale(_animator.transform.lossyScale, localScale));
						}
					}
				}
				
				private static int GetExposedBoneIndex(GPUAnimations animations, string boneName)
				{
					if (animations != null)
					{
						string[] boneNames = animations._bones;

						for (int i = 0; i < boneNames.Length; i++)
						{
							if (boneNames[i] == boneName)
							{
								for (int j = 0; j < animations._exposedBones.Length; j++)
								{
									if (animations._exposedBones[j]._boneIndex == i)
									{
										return j;
									}
								}

								UnityEngine.Debug.LogError("Bone '" + boneName + "' isn't exposed in GPU Animations Asset");
								return -1;
							}
						}
					}

					return -1;
				}
				
				private void GetFrameInfo(float frame, out int prevFrame, out int nextFrame, out float frameLerp)
				{
					prevFrame = Mathf.FloorToInt(frame);
					nextFrame = Math.Min(prevFrame + 1, _exposedBoneNumSamples - 1);
					frameLerp = frame - prevFrame;
				}
				#endregion
			}
		}
	}
}
