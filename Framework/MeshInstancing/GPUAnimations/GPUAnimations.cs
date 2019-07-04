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
					public readonly Matrix4x4[] _cachedBoneMatrices;

					public ExposedBone(int boneIndex, Matrix4x4[] boneMatrices)
					{
						_boneIndex = boneIndex;
						_cachedBoneMatrices = boneMatrices;
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
