using System;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			[RequireComponent(typeof(GPUAnimator))]
			public class GPUAnimatorBoneTracker: MonoBehaviour
			{
				#region Public Data
				public Mesh _referenceMesh;

				[Serializable]
				public struct TrackedBone
				{
					public string _bone;
					[NonSerialized]
					public int _boneIndex;
					[NonSerialized]
					public Matrix4x4 _inverseBindPose;
					[NonSerialized]
					public Vector3 _currentPosition;
					[NonSerialized]
					public Quaternion _currentRotation;
				}
				public TrackedBone[] _trackedBones;

				[Serializable]
				public struct BoneFollower
				{
					public int _trackedBoneIndex;
					public Transform _transform;
				}
				public BoneFollower[] _boneFollowers;
				#endregion

				#region Private Data
				private GPUAnimator _animator;
				private Matrix4x4[,] _cachedBoneMatrices;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_animator = GetComponent<GPUAnimator>();
					CacheBoneData();
				}

				private void LateUpdate()
				{
					UpdateTrackedBones();
					UpdateBoneFollowers();
				}
				#endregion

				#region Public Interface
				public Vector3 GetBonePosition(string bone)
				{
					for (int i = 0; i < _boneFollowers.Length; i++)
					{
						if (_trackedBones[i]._bone == bone)
						{
							return _trackedBones[i]._currentPosition;
						}
					}

					return this.transform.position;
				}

				public Quaternion GetBoneRotation(string bone)
				{
					for (int i = 0; i < _boneFollowers.Length; i++)
					{
						if (_trackedBones[i]._bone == bone)
						{
							return _trackedBones[i]._currentRotation;
						}
					}

					return this.transform.rotation;
				}
				#endregion

				#region Private Functions
				private void CacheBoneData()
				{
					Texture2D texture = _animator._animations.GetTexture();
					GPUAnimations.Animation[] animations = _animator._animations.GetAnimations();
					string[] boneNames = _animator._animations.GetBoneNames();

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
						//Find bone index!
						_trackedBones[i]._boneIndex = -1;

						for (int j = 0; j < boneNames.Length; j++)
						{
							if (boneNames[j] == _trackedBones[i]._bone)
							{
								_trackedBones[i]._boneIndex = j;
								break;
							}
						}

						if (_trackedBones[i]._boneIndex != -1)
						{
							_trackedBones[i]._inverseBindPose = _referenceMesh.bindposes[_trackedBones[i]._boneIndex].inverse;

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

				private void UpdateTrackedBones()
				{
					if (_trackedBones.Length > 0)
					{
						float curFrameWeight = _animator.GetCurrentAnimationWeight();
						bool lerpingFromPrev = curFrameWeight < 1.0f;

						float curFrame = _animator.GetCurrentAnimationFrame();
						int prevFrame = Mathf.FloorToInt(curFrame);
						int nextFrame = prevFrame + 1;
						float frameLerp = curFrame - prevFrame;

						float prevCurFrame = 0f;
						int prevPrevFrame = 0;
						int prevNextFrame = 0;
						float prevFrameLerp = 0f;

						if (lerpingFromPrev)
						{
							prevCurFrame = _animator.GetPreviousAnimationFrame();
							prevPrevFrame = Mathf.FloorToInt(prevCurFrame);
							prevNextFrame = prevPrevFrame + 1;
							prevFrameLerp = prevCurFrame - prevPrevFrame;
						}

						for (int i = 0; i < _trackedBones.Length; i++)
						{
							CalcBoneTransform(prevFrame, nextFrame, frameLerp, _trackedBones[i]._boneIndex, ref _trackedBones[i]._inverseBindPose, out _trackedBones[i]._currentPosition, out _trackedBones[i]._currentRotation);

							if (lerpingFromPrev)
							{
								CalcBoneTransform(prevPrevFrame, prevNextFrame, prevFrameLerp, _trackedBones[i]._boneIndex, ref _trackedBones[i]._inverseBindPose, out Vector3 prevPosition, out Quaternion prevRotation);

								_trackedBones[i]._currentPosition = Vector3.Lerp(prevPosition, _trackedBones[i]._currentPosition, curFrameWeight);
								_trackedBones[i]._currentRotation = Quaternion.Slerp(prevRotation, _trackedBones[i]._currentRotation, curFrameWeight);
							}

							//Convert to world space
							_trackedBones[i]._currentPosition = this.transform.TransformPoint(_trackedBones[i]._currentPosition);
							_trackedBones[i]._currentRotation = this.transform.rotation * _trackedBones[i]._currentRotation;
						}
					}
				}

				private void CalcBoneTransform(int prevFrame, int nextFrame, float frameLerp, int boneIndex, ref Matrix4x4 inverseBindPose, out Vector3 position, out Quaternion rotation)
				{
					Matrix4x4 prevMatrix = _cachedBoneMatrices[boneIndex, prevFrame] * inverseBindPose;
					Matrix4x4 nextMatrix = _cachedBoneMatrices[boneIndex, nextFrame] * inverseBindPose;

					position = Vector3.Lerp(prevMatrix.MultiplyPoint3x4(Vector3.zero), nextMatrix.MultiplyPoint3x4(Vector3.zero), frameLerp);
					rotation = Quaternion.Slerp(prevMatrix.rotation, nextMatrix.rotation, frameLerp);
				}

				private void UpdateBoneFollowers()
				{
					for (int i = 0; i < _boneFollowers.Length; i++)
					{
						transform.position = _trackedBones[_boneFollowers[i]._trackedBoneIndex]._currentPosition;
						transform.rotation = _trackedBones[_boneFollowers[i]._trackedBoneIndex]._currentRotation;
					}
				}
				#endregion
			}
		}
    }
}
