using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public class GPUAnimations
			{
				#region Public Data
				public static int kPixelsPerBoneMatrix = 4;

				public readonly Texture2D _texture;
				public readonly string[] _bones;

				public struct Animation
				{
					public readonly string _name;
					public readonly int _startFrameOffset;
					public readonly int _totalFrames;
					public readonly float _fps;
					public readonly float _length;
					public readonly WrapMode _wrapMode;
					public readonly AnimationEvent[] _events;
					public readonly bool _hasRootMotion;
					public readonly Vector3[] _rootMotionVelocities;
					public readonly Vector3[] _rootMotionAngularVelocities;

					public Animation(string name, int startOffset, int frameCount, float fps, WrapMode wrapMode, AnimationEvent[] events, bool hasRootMotion, Vector3[] rootMotionVelocities, Vector3[] rootMotionAngularVelocities)
					{
						_name = name;
						_startFrameOffset = startOffset;
						_totalFrames = frameCount;
						_fps = fps;
						_wrapMode = wrapMode;
						_events = events;
						_hasRootMotion = hasRootMotion;
						_rootMotionVelocities = rootMotionVelocities;
						_rootMotionAngularVelocities = rootMotionAngularVelocities;
						_length = _totalFrames / _fps;
					}

					public static readonly Animation kInvalid = new Animation(string.Empty, 0, 0, 0f, WrapMode.Default, new AnimationEvent[0], false, new Vector3[0], new Vector3[0]);
				}

				public readonly Animation[] _animations;
				
				public struct ExposedBone
				{
					public readonly int _boneIndex;
					public readonly Vector3[] _cachedBonePositions;
					public readonly Quaternion[] _cachedBoneRotations;
					public readonly Vector3[] _cachedBoneScales;

					public ExposedBone(int boneIndex, Vector3[] bonePositions, Quaternion[] boneRotations, Vector3[] boneScales)
					{
						_boneIndex = boneIndex;
						_cachedBonePositions = bonePositions;
						_cachedBoneRotations = boneRotations;
						_cachedBoneScales = boneScales;
					}

					public void GetBoneTransform(int prevFrame, int nextFrame, float frameLerp, bool usePosition, bool useRotation, bool useScale, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
					{
						if (usePosition)
							localPosition = Vector3.Lerp(_cachedBonePositions[prevFrame], _cachedBonePositions[nextFrame], frameLerp);
						else
							localPosition = Vector3.zero;

						if (useRotation)
							localRotation = Quaternion.Slerp(_cachedBoneRotations[prevFrame], _cachedBoneRotations[nextFrame], frameLerp);
						else
							localRotation = Quaternion.identity;

						if (useScale)
							localScale = Vector3.Lerp(_cachedBoneScales[prevFrame], _cachedBoneScales[nextFrame], frameLerp);
						else
							localScale = Vector3.one;
					}
				}

				public readonly ExposedBone[] _exposedBones;
				#endregion

				#region Public Interface
				public GPUAnimations(Texture2D texture, Animation[] animations, string[] bones, ExposedBone[] exposedBones)
				{
					_texture = texture;
					_animations = animations;
					_bones = bones;
					_exposedBones = exposedBones;
				}
				#endregion
			}
		}
	}
}
