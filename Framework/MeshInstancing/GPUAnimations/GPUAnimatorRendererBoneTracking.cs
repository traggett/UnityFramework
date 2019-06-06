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
				}
				public TrackedBone[] _trackedBones = new TrackedBone[0];
				#endregion

				#region Private Data
				private GPUAnimatorRenderer _renderer;
				private Matrix4x4[,] _cachedBoneMatrices;
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
					string[] boneNames = _renderer._animationTexture.GetBoneNames();

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
					return _cachedBoneMatrices[boneIndex, frame];
				}
				#endregion

				#region Private Functions
				private void CacheBoneData()
				{
					Texture2D texture = _renderer._animationTexture.GetTexture();
					GPUAnimations.Animation[] animations = _renderer._animationTexture.GetAnimations();
					string[] boneNames = _renderer._animationTexture.GetBoneNames();

					int numBones = boneNames.Length;
					int totalFrames = 0;

					for (int i = 0; i < animations.Length; i++)
					{
						totalFrames += animations[i]._totalFrames;
					}

					_cachedBoneMatrices = new Matrix4x4[numBones, totalFrames];


					int framesPerRow = texture.width / GPUAnimations.kPixelsPerBoneMatrix;

					for (int i = 0; i < _trackedBones.Length; i++)
					{
						_trackedBones[i]._boneIndex = GetBoneIndex(_trackedBones[i]._bone);
						
						if (_trackedBones[i]._boneIndex != -1)
						{
							_trackedBones[i]._inverseBindPose = _renderer._mesh.bindposes[_trackedBones[i]._boneIndex].inverse;

							int textureFrame = 0;

							foreach (GPUAnimations.Animation animation in animations)
							{
								for (; textureFrame < animation._startFrameOffset + animation._totalFrames; textureFrame++)
								{
									//what row is our frame?
									int row = textureFrame / framesPerRow;

									//what col is our frame?
									int col = textureFrame - (row * framesPerRow);

									//Work out pixel coords for the frame
									int pixelX = (col * GPUAnimations.kPixelsPerBoneMatrix);
									int pixelY = (row * numBones) + _trackedBones[i]._boneIndex;

									Color[] pixels = texture.GetPixels(pixelX, pixelY, 4, 1, 0);

									_cachedBoneMatrices[_trackedBones[i]._boneIndex, textureFrame] = CalcMatrixFromPixels(pixels);
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
