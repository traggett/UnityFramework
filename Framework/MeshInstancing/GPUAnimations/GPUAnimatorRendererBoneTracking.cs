using System;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			[RequireComponent(typeof(GPUAnimatorRenderer))]
			public class GPUAnimatorRendererBoneTracking : MonoBehaviour
			{
				#region Public Data
				[Serializable]
				public struct TrackedBone
				{
					public string _bone;
					[NonSerialized]
					public int _boneIndex;
					[NonSerialized]
					public Matrix4x4[] _cachedBoneMatrices;
				}
				public TrackedBone[] _trackedBones = new TrackedBone[0];
				#endregion

				#region Private Data
				private GPUAnimatorRenderer _renderer;
				private int _totalSamples;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_renderer = GetComponent<GPUAnimatorRenderer>();
					CacheBoneData();
				}
				#endregion

				#region Public Interface
				public int GetBoneIndex(string boneName)
				{
					string[] boneNames = _renderer._animationTexture.GetAnimations()._bones;

					for (int i = 0; i < boneNames.Length; i++)
					{
						if (boneNames[i] == boneName)
						{
							return i;
						}
					}

					return -1;
				}

				public bool GetBoneTransform(int boneIndex, float frame, GPUAnimatorBoneFollower.Flags flags, out Vector3 position, out Quaternion rotation, out Vector3 scale)
				{
					int trackedBoneIndex = GetTrackedBoneIndex(boneIndex);

					if (trackedBoneIndex != -1)
					{
						int prevFrame = Mathf.FloorToInt(frame);
						int nextFrame = Math.Min(prevFrame + 1, _totalSamples - 1);
						float frameLerp = frame - prevFrame;
						
						if ((flags & GPUAnimatorBoneFollower.Flags.Position) != 0)
						{
							Vector3 prevFramePos = _trackedBones[trackedBoneIndex]._cachedBoneMatrices[prevFrame].MultiplyPoint3x4(Vector3.zero);
							Vector3 nextFramePos = _trackedBones[trackedBoneIndex]._cachedBoneMatrices[nextFrame].MultiplyPoint3x4(Vector3.zero);
							position = Vector3.Lerp(prevFramePos, nextFramePos, frameLerp);
						}
						else
						{
							position = Vector3.zero;
						}

						if ((flags & GPUAnimatorBoneFollower.Flags.Rotation) != 0)
						{
							Vector3 prevForward = _trackedBones[trackedBoneIndex]._cachedBoneMatrices[prevFrame].MultiplyVector(Vector3.forward);
							Vector3 prevUp = _trackedBones[trackedBoneIndex]._cachedBoneMatrices[prevFrame].MultiplyVector(Vector3.up);

							Vector3 nextForward = _trackedBones[trackedBoneIndex]._cachedBoneMatrices[nextFrame].MultiplyVector(Vector3.forward);
							Vector3 nextUp = _trackedBones[trackedBoneIndex]._cachedBoneMatrices[nextFrame].MultiplyVector(Vector3.up);

							Vector3 forward = Vector3.Lerp(prevForward, nextForward, frameLerp);
							Vector3 up = Vector3.Lerp(prevUp, nextUp, frameLerp);

							rotation = Quaternion.LookRotation(forward, up);
						}				
						else
						{
							rotation = Quaternion.identity;
						}
						
						if ((flags & GPUAnimatorBoneFollower.Flags.Scale) != 0)
						{
							Vector3 prevScale = _trackedBones[trackedBoneIndex]._cachedBoneMatrices[prevFrame].lossyScale;
							Vector3 nextScale = _trackedBones[trackedBoneIndex]._cachedBoneMatrices[nextFrame].lossyScale;
							scale = Vector3.Lerp(prevScale, nextScale, frameLerp);
						}
						else
						{
							scale = Vector3.one;
						}

						return true;
					}
					else
					{
						position = Vector3.zero;
						rotation = Quaternion.identity;
						scale = Vector3.one;

						return false;
					}
				}
				#endregion

				#region Private Functions
				private void CacheBoneData()
				{
					GPUAnimations animations = _renderer._animationTexture.GetAnimations();
					
					int numBones = animations._bones.Length;
					_totalSamples = 0;

					for (int i = 0; i < animations._animations.Length; i++)
					{
						_totalSamples += animations._animations[i]._totalFrames + 1;
					}
					
					int framesPerRow = animations._texture.width / GPUAnimations.kPixelsPerBoneMatrix;

					for (int i = 0; i < _trackedBones.Length; i++)
					{
						_trackedBones[i]._boneIndex = GetBoneIndex(_trackedBones[i]._bone);
						
						if (_trackedBones[i]._boneIndex != -1)
						{
							_trackedBones[i]._cachedBoneMatrices = new Matrix4x4[_totalSamples];

							Matrix4x4 inverseBindPose = _renderer._mesh.bindposes[_trackedBones[i]._boneIndex].inverse;

							int textureFrame = 0;

							for (int j = 0; j < animations._animations.Length; j++)
							{
								for (; textureFrame < animations._animations[j]._startFrameOffset + animations._animations[j]._totalFrames + 1; textureFrame++)
								{
									//what row is our frame?
									int row = textureFrame / framesPerRow;

									//what col is our frame?
									int col = textureFrame - (row * framesPerRow);

									//Work out pixel coords for the frame
									int pixelX = (col * GPUAnimations.kPixelsPerBoneMatrix);
									int pixelY = (row * numBones) + _trackedBones[i]._boneIndex;

									Color[] pixels = animations._texture.GetPixels(pixelX, pixelY, 4, 1, 0);

									_trackedBones[i]._cachedBoneMatrices[textureFrame] = CalcMatrixFromPixels(pixels) * inverseBindPose;
								}
							}
						}
					}
				}
				
				private static Matrix4x4 CalcMatrixFromPixels(Color[] pixels)
				{
					Matrix4x4 matrix = new Matrix4x4();

					matrix.SetRow(0, pixels[0]);
					matrix.SetRow(1, pixels[1]);
					matrix.SetRow(2, pixels[2]);
					matrix.SetRow(3, pixels[3]);

					return matrix;
				}

				private int GetTrackedBoneIndex(int boneIndex)
				{
					for (int i = 0; i < _trackedBones.Length; i++)
					{
						if (_trackedBones[i]._boneIndex == boneIndex)
						{
							return i;
						}
					}

					return -1;
				}
				#endregion
			}
		}
    }
}
