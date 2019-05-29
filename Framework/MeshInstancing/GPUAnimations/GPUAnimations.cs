using System.IO;
using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public class GPUAnimations
			{
				public struct Animation
				{
					public readonly string _name;
					public readonly int _startFrameOffset;
					public readonly int _totalFrames;
					public readonly float _fps;
					public readonly WrapMode _wrapMode;
					public readonly AnimationEvent[] _events;
					public readonly bool _hasRootMotion;
					public readonly Vector3[] _rootMotionVelocities;
					public readonly Vector3[] _rootMotionAngularVelocities;

					public Animation(string name, int startOffset, int frameCount, float fps, WrapMode wrapMode, AnimationEvent[] events, bool hasRootMotion = false, Vector3[] rootMotionVelocities = null, Vector3[] rootMotionAngularVelocities = null)
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
					}
				}

				public readonly Animation[] _animations;
				public readonly int _numBones;
				public readonly Texture2D _texture;

				public GPUAnimations(Animation[] animations, int numBones, Texture2D texture)
				{
					_animations = animations;
					_numBones = numBones;
					_texture = texture;
				}

				public static void CheckForEvents(GameObject gameObject, Animation animation, float prevFrame, float currFrame)
				{
					for (int i = 0; i < animation._events.Length; i++)
					{
						float animationEventFrame = animation._startFrameOffset + (animation._events[i].time * animation._fps);

						if (prevFrame < animationEventFrame && currFrame >= animationEventFrame)
						{
							AnimationUtils.TriggerAnimationEvent(animation._events[i], gameObject);
						}
					}
				}

				public static GPUAnimations LoadFromFile(TextAsset file)
				{
					BinaryReader reader = new BinaryReader(new MemoryStream(file.bytes));

					int numBones = reader.ReadInt32();
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
					TextureFormat format = TextureFormat.RGBAHalf;

					int textureWidth = reader.ReadInt32();
					int textureHeight = reader.ReadInt32();
					int byteLength = reader.ReadInt32();
					byte[] bytes = new byte[byteLength];
					bytes = reader.ReadBytes(byteLength);
					Texture2D texture = new Texture2D(textureWidth, textureHeight, format, false);
					texture.filterMode = FilterMode.Point;
					texture.LoadRawTextureData(bytes);
					texture.Apply();

					return new GPUAnimations(animations, numBones, texture);
				}
			}
		}
	}
}
