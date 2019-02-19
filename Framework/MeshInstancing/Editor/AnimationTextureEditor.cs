using System.IO;
using UnityEngine;
using UnityEditor;

namespace Framework
{
	using System.Collections.Generic;
	using Utils;

	namespace MeshInstancing
	{
		namespace Editor
		{
			public class AnimationTextureEditor : EditorWindow
			{
				private GameObject _animatorObject;
				private SkinnedMeshRenderer[] _skinnedMeshes;
				private int _skinnedMeshIndex;
				[SerializeField]
				private AnimationClip[] _animations;
				private string _currentFileName;
				private int _fps = 15;

				private static int[] kAllowedTextureSizes = { 64, 128, 256, 512, 1024, 2048 };

				#region EditorWindow
				[MenuItem("GPU Skinning/Animation Texture Generator", false)]
				static void MakeWindow()
				{
					AnimationTextureEditor window = GetWindow(typeof(AnimationTextureEditor)) as AnimationTextureEditor;
				}

				private void OnGUI()
				{
					ScriptableObject target = this;
					SerializedObject so = new SerializedObject(target);

					GUI.skin.label.richText = true;
					GUILayout.BeginHorizontal();
					{
						GUILayout.FlexibleSpace();
					}
					GUILayout.EndHorizontal();


					EditorGUILayout.LabelField("Generate Animation Texture");

					GameObject prefab = EditorGUILayout.ObjectField("Asset to Evaluate", _animatorObject, typeof(GameObject), true) as GameObject;
					if (prefab != _animatorObject)
					{
						_animatorObject = prefab;
						_skinnedMeshes = prefab != null ? prefab.GetComponentsInChildren<SkinnedMeshRenderer>() : new SkinnedMeshRenderer[0];
						_skinnedMeshIndex = 0;
					}

					if (_skinnedMeshes != null && _skinnedMeshes.Length > 0)
					{
						string[] skinnedMeshes = new string[_skinnedMeshes.Length];
						
						for (int i = 0; i < skinnedMeshes.Length; i++)
						{
							skinnedMeshes[i] = _skinnedMeshes[i].gameObject.name;
						}

						_skinnedMeshIndex = EditorGUILayout.Popup("Skinned Mesh", _skinnedMeshIndex, skinnedMeshes);
					}

					_fps = EditorGUILayout.IntField("FPS", _fps);
					_fps = Mathf.Clamp(_fps, 1, 120);

					//Draw list showing animation clip, 
					SerializedProperty animationsProperty = so.FindProperty("_animations");
					EditorGUILayout.PropertyField(animationsProperty, true);
					so.ApplyModifiedProperties();

					if (_skinnedMeshes != null && _animations != null && _animations.Length > 0)
					{
						if (GUILayout.Button("Generate"))
						{
							string path = EditorUtility.SaveFilePanelInProject("Save Animation Texture", Path.GetFileNameWithoutExtension(_currentFileName), "bytes", "Please enter a file name to save the animation texture to");

							if (!string.IsNullOrEmpty(path))
							{
								_currentFileName = path;

								GameObject sampleObject = Instantiate(_animatorObject);

								SkinnedMeshRenderer[] skinnedMeshes = sampleObject.GetComponentsInChildren<SkinnedMeshRenderer>();
								SkinnedMeshRenderer skinnedMesh = skinnedMeshes[_skinnedMeshIndex];
								Transform[] bones = skinnedMesh.bones;
								Matrix4x4[] bindPoses = skinnedMesh.sharedMesh.bindposes;

								AnimationTexture animationTexture = CreateAnimationTexture(sampleObject, bones, bindPoses, _animations, _fps);
								SaveAnimationTexture(animationTexture, _currentFileName);

								DestroyImmediate(sampleObject);
							}
						}
					}
				}
				#endregion

				private static AnimationTexture CreateAnimationTexture(GameObject gameObject, Transform[] bones, Matrix4x4[] bindposes, AnimationClip[] animationClips, int bakeFPS)
				{
					int numBones = bones.Length;
					AnimationTexture.Animation[] animations = new AnimationTexture.Animation[animationClips.Length];

					//3d array - animation / frame / bones
					Matrix4x4[][][] boneWorldMatrix = new Matrix4x4[animations.Length][][];

					Transform root = bones[0];
					while (root.parent != null)
					{
						root = root.parent;
					}
					Matrix4x4 rootMat = root.worldToLocalMatrix;

					for (int i = 0; i < animations.Length; i++)
					{
						string name = animationClips[i].name;
						int totalFrames = (int)(animationClips[i].length * bakeFPS + 0.5f) + 1;
						totalFrames = Mathf.Max(totalFrames, 1);
						int fps = bakeFPS;
						WrapMode wrapMode = animationClips[i].wrapMode;

						animations[i] = new AnimationTexture.Animation(name, totalFrames, fps, wrapMode);

						//Sample animation
						boneWorldMatrix[i] = new Matrix4x4[totalFrames][];

						for (int j = 0; j < totalFrames; j++)
						{
							//Sample animation
							animationClips[i].legacy = true;
							animationClips[i].SampleAnimation(gameObject, (float)bakeFPS * j);
							animationClips[i].legacy = false;
							//Save bone matrices
							boneWorldMatrix[i][j] = new Matrix4x4[numBones];

							for (int k = 0; k < numBones; k++)
							{
								boneWorldMatrix[i][j][k] = rootMat * bones[k].localToWorldMatrix * bindposes[k];
							}
						}
					}

					//Create and save texture! Work out width!
					TextureFormat format = TextureFormat.RGBAHalf;
					int textureSize;

					if (!CalculateTextureSize(boneWorldMatrix, out textureSize))
					{
						return null;
					}

					Texture2D texture = new Texture2D(textureSize, textureSize, format, false);
					texture.filterMode = FilterMode.Point;

					//Loop through animations / frames / bones setting pixels for each bone matrix
					int pixelx = 0;
					int pixely = 0;

					for (int i = 0; i < boneWorldMatrix.Length; i++)
					{
						for (int j = 0; j < boneWorldMatrix[i].Length; j++)
						{
							Color[] matrixPixels = ConvertMatricesToColor(boneWorldMatrix[i][j]);
							texture.SetPixels(pixelx, pixely, 4, numBones, matrixPixels);

							pixelx += 4;

							if (pixelx + 4 > textureSize)
							{
								pixelx = 0;
								pixely += numBones;
							}
						}
					}

					texture.Apply();

					return new AnimationTexture(animations, numBones, texture);
				}

				private static void SaveAnimationTexture(AnimationTexture animationTexture, string fileName)
				{
					string directory = Path.GetDirectoryName(fileName);
					if (!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					FileStream file = File.Open(fileName, FileMode.Create);
					BinaryWriter writer = new BinaryWriter(file);

					writer.Write(animationTexture._numBones);
					writer.Write(animationTexture._animations.Length);

					for (int i = 0; i < animationTexture._animations.Length; i++)
					{
						writer.Write(animationTexture._animations[i]._name);
						writer.Write(animationTexture._animations[i]._totalFrames);
						writer.Write(animationTexture._animations[i]._fps);
						writer.Write((int)animationTexture._animations[i]._wrapMode);
					}

					//Write texture!
					byte[] bytes = animationTexture._texture.GetRawTextureData();
					writer.Write(animationTexture._texture.width);
					writer.Write(animationTexture._texture.height);
					writer.Write(bytes.Length);
					writer.Write(bytes);

					file.Close();

#if UNITY_EDITOR
					//Refresh the saved asset
					AssetUtils.RefreshAsset(fileName);
#endif
				}

				private static bool CalculateTextureSize(Matrix4x4[][][] boneWorldMatrix, out int textureSize)
				{
					int numBones = boneWorldMatrix[0][0].Length;
					int numFrames = 0;

					for (int i = 0; i < boneWorldMatrix.Length; i++)
					{
						numFrames += boneWorldMatrix[i].Length;
					}

					//Work out how many frames are needed to match the height of bone pixels (four pixels per bone wide
					int numFramesToMakeASquare = Mathf.CeilToInt(numBones / 4f);

					int width = 0;

					if (numFramesToMakeASquare >= numFrames)
					{
						width = numFramesToMakeASquare * 4;
					}
					//Work out what square is needed
					else
					{
						int pow = 2;

						while (true)
						{
							float frameCount = numFramesToMakeASquare * Mathf.Pow(2, pow);

							if (frameCount >= numFrames)
							{
								width = pow * numFramesToMakeASquare * 4;
								break;
							}

							pow++;
						}
					}

					for (int i = 0; i < kAllowedTextureSizes.Length; i++)
					{
						if (width <= kAllowedTextureSizes[i])
						{
							textureSize = kAllowedTextureSizes[i];
							return true;
						}

					}

					textureSize = 0;
					return false;
				}

				private static Color[] ConvertMatricesToColor(Matrix4x4[] boneMatrices)
				{
					Color[] colors = new Color[4 * boneMatrices.Length];

					int index = 0;

					for (int i = 0; i < boneMatrices.Length; i++)
					{
						colors[index++] = boneMatrices[i].GetRow(0);
						colors[index++] = boneMatrices[i].GetRow(1);
						colors[index++] = boneMatrices[i].GetRow(2);
						colors[index++] = boneMatrices[i].GetRow(3);
					}

					return colors;
				}
			}
		}
	}
}
