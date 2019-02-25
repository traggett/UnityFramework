using System.Collections.Generic;
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
				public readonly int _startFrameOffset;
				public readonly int _totalFrames;
				public readonly int _fps;
				public readonly WrapMode _wrapMode;

				public Animation(string name, int startOffset, int frameCount, int fps, WrapMode wrapMode)
				{
					_name = name;
					_startFrameOffset = startOffset;
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

			public static AnimationTexture LoadFromFile(TextAsset file)
			{
				BinaryReader reader = new BinaryReader(new MemoryStream(file.bytes));

				//Read animation infos
				int numBones = reader.ReadInt32();
				int animCount = reader.ReadInt32();
				

				Animation[] animations = new Animation[animCount];
				for (int i=0; i<animCount; i++)
				{
					string name = reader.ReadString();
					int startOffset = reader.ReadInt32();
					int totalFrames = reader.ReadInt32();
					int fps = reader.ReadInt32();
					WrapMode wrapMode = (WrapMode)reader.ReadInt32();

					animations[i] = new Animation(name, startOffset, totalFrames, fps, wrapMode);
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

			public static Mesh AddExtraMeshData(Mesh mesh, int bonesPerVertex = 4)
			{
				Color[] colors = new Color[mesh.vertexCount];
				List<Vector4> uv2 = new List<Vector4>();

				for (int i = 0; i != mesh.vertexCount; ++i)
				{
					BoneWeight weight = mesh.boneWeights[i];

					colors[i].r = weight.weight0;
					colors[i].g = weight.weight1;
					colors[i].b = weight.weight2;
					colors[i].a = weight.weight3;

					Vector4 boneIds;

					boneIds.x = weight.boneIndex0;
					boneIds.y = weight.boneIndex1;
					boneIds.z = weight.boneIndex2;
					boneIds.w = weight.boneIndex3;
					
					if (bonesPerVertex == 3)
					{
						float rate = 1.0f / (weight.boneIndex0 + weight.boneIndex1 + weight.boneIndex2);
						colors[i].r = colors[i].r * rate;
						colors[i].g = colors[i].g * rate;
						colors[i].b = colors[i].b * rate;
						colors[i].a = -0.1f;
					}
					else if (bonesPerVertex == 2)
					{
						float rate = 1.0f / (weight.boneIndex0 + weight.boneIndex1);
						colors[i].r = colors[i].r * rate;
						colors[i].g = colors[i].g * rate;
						colors[i].b = -0.1f;
						colors[i].a = -0.1f;
					}
					else if (bonesPerVertex == 1)
					{
						colors[i].r = 1.0f;
						colors[i].g = -0.1f;
						colors[i].b = -0.1f;
						colors[i].a = -0.1f;
					}

					uv2.Add(boneIds);
				}
				mesh.colors = colors;
				mesh.SetUVs(2, uv2);
				mesh.UploadMeshData(false);

				return mesh;
			}
		}
	}
}
