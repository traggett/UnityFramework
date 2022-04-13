﻿using System.IO;
using UnityEngine;

namespace Framework
{
	namespace Graphics
	{
		namespace MeshInstancing
		{
			namespace GPUAnimations
			{
				public static class GPUAnimationsIO
				{
					#region Public Interface
					public static GPUAnimations Read(BinaryReader reader)
					{
						string[] bones = new string[reader.ReadInt32()];
						for (int i = 0; i < bones.Length; i++)
						{
							bones[i] = reader.ReadString();
						}

						int animCount = reader.ReadInt32();
						GPUAnimations.Animation[] animations = new GPUAnimations.Animation[animCount];

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

							animations[i] = new GPUAnimations.Animation(name, startOffset, totalFrames, fps, wrapMode, events, hasRootMotion, rootMotionVelocities, rootMotionAngularVelocities);
						}

						int exposedBoneCount = reader.ReadInt32();
						GPUAnimations.ExposedBone[] exposedBones = new GPUAnimations.ExposedBone[exposedBoneCount];

						for (int i = 0; i < exposedBoneCount; i++)
						{
							int boneIndex = reader.ReadInt32();
							int numSamples = reader.ReadInt32();
							Vector3[] bonePositions = new Vector3[numSamples];
							Quaternion[] boneRotations = new Quaternion[numSamples];
							Vector3[] boneScales = new Vector3[numSamples];

							for (int j = 0; j < numSamples; j++)
							{
								bonePositions[j] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
								boneRotations[j] = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
								boneScales[j] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
							}

							exposedBones[i] = new GPUAnimations.ExposedBone(boneIndex, bonePositions, boneRotations, boneScales);
						}


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

						return new GPUAnimations(texture, animations, bones, exposedBones);
					}

					public static void Write(GPUAnimations animations, BinaryWriter writer)
					{
						writer.Write(animations._bones.Length);
						for (int i = 0; i < animations._bones.Length; i++)
						{
							writer.Write(animations._bones[i]);
						}

						writer.Write(animations._animations.Length);

						for (int i = 0; i < animations._animations.Length; i++)
						{
							writer.Write(animations._animations[i]._name);
							writer.Write(animations._animations[i]._startFrameOffset);
							writer.Write(animations._animations[i]._totalFrames);
							writer.Write(animations._animations[i]._fps);
							writer.Write((int)animations._animations[i]._wrapMode);

							writer.Write(animations._animations[i]._events.Length);

							for (int j = 0; j < animations._animations[i]._events.Length; j++)
							{
								writer.Write(animations._animations[i]._events[j].time);
								writer.Write(animations._animations[i]._events[j].functionName);
								writer.Write(animations._animations[i]._events[j].stringParameter);
								writer.Write(animations._animations[i]._events[j].floatParameter);
								writer.Write(animations._animations[i]._events[j].intParameter);
								//TO DO?
								//writer.Write(animationTexture._animations[i]._events[j].objectReferenceParameter);
							}

							writer.Write(animations._animations[i]._hasRootMotion);

							if (animations._animations[i]._hasRootMotion)
							{
								for (int j = 0; j < animations._animations[i]._totalFrames; j++)
								{
									writer.Write(animations._animations[i]._rootMotionVelocities[j].x);
									writer.Write(animations._animations[i]._rootMotionVelocities[j].y);
									writer.Write(animations._animations[i]._rootMotionVelocities[j].z);

									writer.Write(animations._animations[i]._rootMotionAngularVelocities[j].x);
									writer.Write(animations._animations[i]._rootMotionAngularVelocities[j].y);
									writer.Write(animations._animations[i]._rootMotionAngularVelocities[j].z);
								}
							}
						}

						writer.Write(animations._exposedBones.Length);

						for (int i = 0; i < animations._exposedBones.Length; i++)
						{
							writer.Write(animations._exposedBones[i]._boneIndex);

							writer.Write(animations._exposedBones[i]._cachedBonePositions.Length);

							for (int j = 0; j < animations._exposedBones[i]._cachedBonePositions.Length; j++)
							{
								writer.Write(animations._exposedBones[i]._cachedBonePositions[j].x);
								writer.Write(animations._exposedBones[i]._cachedBonePositions[j].y);
								writer.Write(animations._exposedBones[i]._cachedBonePositions[j].z);

								writer.Write(animations._exposedBones[i]._cachedBoneRotations[j].x);
								writer.Write(animations._exposedBones[i]._cachedBoneRotations[j].y);
								writer.Write(animations._exposedBones[i]._cachedBoneRotations[j].z);
								writer.Write(animations._exposedBones[i]._cachedBoneRotations[j].w);

								writer.Write(animations._exposedBones[i]._cachedBoneScales[j].x);
								writer.Write(animations._exposedBones[i]._cachedBoneScales[j].y);
								writer.Write(animations._exposedBones[i]._cachedBoneScales[j].z);
							}
						}


						byte[] bytes = animations._texture.GetRawTextureData();

						writer.Write((int)animations._texture.format);
						writer.Write(animations._texture.width);
						writer.Write(animations._texture.height);
						writer.Write(bytes.Length);
						writer.Write(bytes);
					}
					#endregion
				}
			}
		}
	}
}
