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
					public Matrix4x4 _inverseBindPose;
					[NonSerialized]
					public Matrix4x4[] _cachedBoneMatrices;
				}
				public TrackedBone[] _trackedBones = new TrackedBone[0];
				#endregion

				#region Private Data
				private GPUAnimatorRenderer _renderer;
				private int _totalFrames;
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

				public Matrix4x4 GetInvBindPose(int boneIndex)
				{
					for (int i=0; i<_trackedBones.Length; i++)
					{
						if (_trackedBones[i]._boneIndex == boneIndex)
							return _trackedBones[i]._inverseBindPose;
					}

					return Matrix4x4.identity;
				}

				public Matrix4x4 GetBoneMatrix(int boneIndex, int frame)
				{
					if (frame < _totalFrames)
					{
						for (int i = 0; i < _trackedBones.Length; i++)
						{
							if (_trackedBones[i]._boneIndex == boneIndex)
							{
								return _trackedBones[i]._cachedBoneMatrices[frame];
							}
						}
					}
					
					return Matrix4x4.identity;
				}
				#endregion

				#region Private Functions
				private void CacheBoneData()
				{
					GPUAnimations animations = _renderer._animationTexture.GetAnimations();
					
					int numBones = animations._bones.Length;
					_totalFrames = 0;

					for (int i = 0; i < animations._animations.Length; i++)
					{
						_totalFrames += animations._animations[i]._totalFrames;
					}
					
					int framesPerRow = animations._texture.width / GPUAnimations.kPixelsPerBoneMatrix;

					for (int i = 0; i < _trackedBones.Length; i++)
					{
						_trackedBones[i]._boneIndex = GetBoneIndex(_trackedBones[i]._bone);
						
						if (_trackedBones[i]._boneIndex != -1)
						{
							_trackedBones[i]._cachedBoneMatrices = new Matrix4x4[_totalFrames];

							_trackedBones[i]._inverseBindPose = _renderer._mesh.bindposes[_trackedBones[i]._boneIndex].inverse;

							int textureFrame = 0;

							for (int j = 0; j < animations._animations.Length; j++)
							{
								for (; textureFrame < animations._animations[j]._startFrameOffset + animations._animations[j]._totalFrames; textureFrame++)
								{
									//what row is our frame?
									int row = textureFrame / framesPerRow;

									//what col is our frame?
									int col = textureFrame - (row * framesPerRow);

									//Work out pixel coords for the frame
									int pixelX = (col * GPUAnimations.kPixelsPerBoneMatrix);
									int pixelY = (row * numBones) + _trackedBones[i]._boneIndex;

									Color[] pixels = animations._texture.GetPixels(pixelX, pixelY, 4, 1, 0);

									_trackedBones[i]._cachedBoneMatrices[textureFrame] = CalcMatrixFromPixels(pixels);
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
				#endregion
			}
		}
    }
}
