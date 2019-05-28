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
					public readonly int _fps;
					public readonly WrapMode _wrapMode;
					public readonly AnimationEvent[] _events;

					public Animation(string name, int startOffset, int frameCount, int fps, WrapMode wrapMode, AnimationEvent[] events)
					{
						_name = name;
						_startFrameOffset = startOffset;
						_totalFrames = frameCount;
						_fps = fps;
						_wrapMode = wrapMode;
						_events = events;
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
						int fps = reader.ReadInt32();
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

						animations[i] = new Animation(name, startOffset, totalFrames, fps, wrapMode, events);
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
