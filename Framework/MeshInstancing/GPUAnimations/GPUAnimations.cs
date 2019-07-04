using System.IO;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public class GPUAnimations
			{
				public static int kPixelsPerBoneMatrix = 4;

				public readonly Texture2D _texture;

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
				public readonly string[] _bones;
				
				public struct TrackedBone
				{
					public readonly int _boneIndex;
					public readonly Matrix4x4[] _cachedBoneMatrices;
				}
				public readonly TrackedBone[] _trackedBones;

				public GPUAnimations(Texture2D texture, Animation[] animations, string[] bones, TrackedBone[] trackedBones)
				{
					_texture = texture;
					_animations = animations;
					_bones = bones;
					_trackedBones = trackedBones;
				}

				public static GPUAnimations LoadFromFile(TextAsset file)
				{
					BinaryReader reader = new BinaryReader(new MemoryStream(file.bytes));

					string[] bones = new string[reader.ReadInt32()];
					for (int i = 0; i < bones.Length; i++)
					{
						bones[i] = reader.ReadString();
					}

					int animCount = reader.ReadInt32();

					Animation[] animations = new Animation[animCount];
					for (int i = 0; i < animCount; i++)
					{
						string name = reader.ReadString();
						int startOffset = reader.ReadInt32();
						int totalFrames = reader.ReadInt32();
						float fps = reader.ReadSingle();
						WrapMode wrapMode = (WrapMode)reader.ReadInt32();

						int numEvents = reader.ReadInt32();
						AnimationEvent[] events = new AnimationEvent[numEvents];

						for (int j = 0; j < numEvents; j++)
						{
							events[j] = new AnimationEvent
							{
								time = reader.ReadSingle(),
								functionName = reader.ReadString(),

								stringParameter = reader.ReadString(),
								floatParameter = reader.ReadSingle(),
								intParameter = reader.ReadInt32()
								//TO DO?
								//evnt.objectReferenceParameter = reader.Rea();
							};
						}

						bool hasRootMotion = reader.ReadBoolean();

						Vector3[] rootMotionVelocities = null;
						Vector3[] rootMotionAngularVelocities = null;

						if (hasRootMotion)
						{
							rootMotionVelocities = new Vector3[totalFrames];
							rootMotionAngularVelocities = new Vector3[totalFrames];

							for (int j = 0; j < totalFrames; j++)
							{
								rootMotionVelocities[j] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
								rootMotionAngularVelocities[j] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
							}
						}

						animations[i] = new Animation(name, startOffset, totalFrames, fps, wrapMode, events, hasRootMotion, rootMotionVelocities, rootMotionAngularVelocities);
					}

					//Read texture
					TextureFormat format = (TextureFormat)reader.ReadInt32();
					int textureWidth = reader.ReadInt32();
					int textureHeight = reader.ReadInt32();
					int byteLength = reader.ReadInt32();
					byte[] bytes = new byte[byteLength];
					bytes = reader.ReadBytes(byteLength);
					Texture2D texture = new Texture2D(textureWidth, textureHeight, format, false);
					texture.filterMode = FilterMode.Point;
					texture.LoadRawTextureData(bytes);
					texture.Apply();

					return new GPUAnimations(texture, animations, bones, null);
				}
			}
		}
	}
}
