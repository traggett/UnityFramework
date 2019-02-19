using System.IO;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		public class AnimationTexture
		{
			public struct Animation
			{
				public readonly string _name;
				public readonly int _totalFrames;
				public readonly int _fps;
				public readonly WrapMode _wrapMode;

				public Animation(string name, int frameCount, int fps, WrapMode wrapMode)
				{
					_name = name;
					_totalFrames = frameCount;
					_fps = fps;
					_wrapMode = wrapMode;
				}
			}

			public readonly Animation[] _animations;
			public readonly int _numBones;
			public readonly Texture2D _texture;

			public AnimationTexture(Animation[] animations, int numBones, Texture2D texture)
			{
				_animations = animations;
				_numBones = numBones;
				_texture = texture;
			}

			public static AnimationTexture ReadAnimationTexture(TextAsset file)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(file.bytes));

				//Read animation infos
				int numBones = reader.ReadInt32();
				int animCount = reader.ReadInt32();

				Animation[] animations = new Animation[animCount];

				for (int i=0; i<animCount; i++)
				{
					string name = reader.ReadString();
					int totalFrames = reader.ReadInt32();
					int fps = reader.ReadInt32();
					WrapMode wrapMode = (WrapMode)reader.ReadInt32();

					animations[i] = new Animation(name, totalFrames, fps, wrapMode);
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

				return new AnimationTexture(animations, numBones, texture);
			}
		}
	}
}
