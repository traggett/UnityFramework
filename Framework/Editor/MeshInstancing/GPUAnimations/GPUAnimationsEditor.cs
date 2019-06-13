using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Framework.Editor;
	using UnityEditor.Animations;
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			namespace Editor
			{
				public sealed class GPUAnimationsEditor : UpdatedEditorWindow
				{
					#region Private Data
					private bool _working;

					private GameObject _evaluatedObject;
					private SkinnedMeshRenderer[] _skinnedMeshes;
					private int _skinnedMeshIndex;
					private bool _useAnimator;
					[SerializeField]
					private AnimationClip[] _animations;

					private Mesh _mesh;
					private TEXCOORD _boneIdChanel = TEXCOORD.TEXCOORD3;
					private TEXCOORD _boneWeightChannel = TEXCOORD.TEXCOORD4;
					
					private static int[] kAllowedTextureSizes = { 64, 128, 256, 512, 1024, 2048, 4098 };
					#endregion

					#region EditorWindow
					[MenuItem("GPU Skinning/Animation Texture Generator", false)]
					private static void MakeWindow()
					{
						GPUAnimationsEditor window = GetWindow(typeof(GPUAnimationsEditor)) as GPUAnimationsEditor;
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

						EditorGUILayout.Separator();

						GUILayout.BeginVertical();
						{
							EditorGUILayout.LabelField("Generate Animation Texture", EditorStyles.largeLabel);
							EditorGUILayout.Separator();

							GameObject prefab = EditorGUILayout.ObjectField("Asset to Evaluate", _evaluatedObject, typeof(GameObject), true) as GameObject;
							if (prefab != _evaluatedObject)
							{
								_evaluatedObject = prefab;
								_skinnedMeshes = prefab != null ? prefab.GetComponentsInChildren<SkinnedMeshRenderer>() : new SkinnedMeshRenderer[0];
								_skinnedMeshIndex = 0;
							}

							if (_evaluatedObject != null && _skinnedMeshes != null && _skinnedMeshes.Length > 0)
							{
								string[] skinnedMeshes = new string[_skinnedMeshes.Length];

								for (int i = 0; i < skinnedMeshes.Length; i++)
								{
									skinnedMeshes[i] = _skinnedMeshes[i].gameObject.name;
								}

								_skinnedMeshIndex = EditorGUILayout.Popup("Skinned Mesh", _skinnedMeshIndex, skinnedMeshes);
							}

							_useAnimator = EditorGUILayout.Toggle("Sample Animator", _useAnimator);

							if (!_useAnimator)
							{
								//Draw list showing animation clip, 
								SerializedProperty animationsProperty = so.FindProperty("_animations");
								EditorGUILayout.PropertyField(animationsProperty, true);
								so.ApplyModifiedProperties();
							}

							if (_working)
							{
								EditorGUILayout.LabelField("Generating Animation Texture", EditorStyles.helpBox);
							}
							else if (_skinnedMeshes != null && (_useAnimator || (_animations != null && _animations.Length > 0)))
							{
								if (GUILayout.Button("Generate"))
								{
									string path = EditorUtility.SaveFilePanelInProject("Save Animation Texture", Path.GetFileNameWithoutExtension(""), "bytes", "Please enter a file name to save the animation texture to");

									if (!string.IsNullOrEmpty(path))
									{
										Run(BakeAnimationTexture(path));
									}
								}
							}
						}
						GUILayout.EndVertical();

						EditorGUILayout.Separator();
						EditorGUILayout.Separator();
						EditorGUILayout.Separator();

						GUILayout.BeginVertical();
						{
							EditorGUILayout.LabelField("Generate Animation Texture Mesh", EditorStyles.largeLabel);
							EditorGUILayout.Separator();

							_mesh = EditorGUILayout.ObjectField("Mesh", _mesh, typeof(Mesh), true) as Mesh;

							_boneIdChanel = (TEXCOORD)EditorGUILayout.EnumPopup("Bone IDs UV Channel", _boneIdChanel);
							_boneWeightChannel = (TEXCOORD)EditorGUILayout.EnumPopup("Bone Weights UV Channel", _boneWeightChannel);

							if (_mesh != null)
							{
								if (GUILayout.Button("Generate Animated Texture Ready Mesh"))
								{
									string path = EditorUtility.SaveFilePanel("Save Mesh Asset", "Assets/", name, "asset");

									if (!string.IsNullOrEmpty(path))
									{
										CreateMeshForAnimations(_mesh, _boneIdChanel, _boneWeightChannel, path);
									}
								}
							}
						}
						GUILayout.EndVertical();
					}
					#endregion

					#region Private Functions
					private IEnumerator BakeAnimationTexture(string path)
					{
						_working = true;

						GameObject gameObject = Instantiate(_evaluatedObject);

						SkinnedMeshRenderer[] skinnedMeshes = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
						SkinnedMeshRenderer skinnedMesh = skinnedMeshes[_skinnedMeshIndex];
						AnimationClip[] animationClips = _animations;
						Animator animator = null;
						string[] animationStateNames = null;

						//If using animator then get clips and state names from controller
						if (_useAnimator)
						{
							animator = GameObjectUtils.GetComponent<Animator>(gameObject, true);

							if (animator == null)
							{
								DestroyImmediate(gameObject);
								_working = false;
								yield break;
							}
								

							GetAnimationClipsFromAnimator(animator, out animationClips, out animationStateNames);

							if (animationClips.Length == 0)
							{
								DestroyImmediate(gameObject);
								_working = false;
								yield break;
							}

							AnimatorUtility.DeoptimizeTransformHierarchy(animator.gameObject);
							animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
						}

						Transform[] bones = skinnedMesh.bones;
						Matrix4x4[] bindposes = skinnedMesh.sharedMesh.bindposes;

						int numBones = bones.Length;

						if (numBones == 0)
						{
							DestroyImmediate(gameObject);
							_working = false;
							yield break;
						}

						string[] boneNames = new string[numBones];

						for (int i=0; i<numBones; i++)
						{
							boneNames[i] = bones[i].gameObject.name;
						}

						GPUAnimations.Animation[] animations = new GPUAnimations.Animation[animationClips.Length];

						//3d array - animation / frame / bones
						Matrix4x4[][][] boneWorldMatrix = new Matrix4x4[animations.Length][][];

						Matrix4x4 rootMat = gameObject.transform.worldToLocalMatrix;
						int startOffset = 0;

						for (int animIndex = 0; animIndex < animations.Length; animIndex++)
						{
							AnimationClip clip = animationClips[animIndex];
							bool hasRootMotion = animator != null && clip.hasMotionCurves;

							string name = clip.name;
							int totalFrames = Mathf.Max(Mathf.FloorToInt(clip.length * clip.frameRate), 1);
							int totalSamples = totalFrames + 1;
							WrapMode wrapMode = clip.wrapMode;
							AnimationEvent[] events = clip.events;

							//Sample animation
							boneWorldMatrix[animIndex] = new Matrix4x4[totalSamples][];

							Vector3[] rootMotionVelocities = null;
							Vector3[] rootMotionAngularVelocities = null;

							if (hasRootMotion)
							{
								rootMotionVelocities = new Vector3[totalSamples];
								rootMotionAngularVelocities = new Vector3[totalSamples];
							}

							//Needed to prevent first frame pop?
							if (animator != null)
							{
								animator.Play(animationStateNames[animIndex]);
								animator.Update(0f);
								yield return null;
								animator.Play(animationStateNames[animIndex], 0, 0f);
								animator.Update(0f);
								yield return null;
								animator.Play(animationStateNames[animIndex], 0, 0f);
								animator.Update(0f);
								yield return null;
							}
							
							for (int i = 0; i < totalSamples; i++)
							{
								float normalisedTime = i / (float)totalFrames;
								
								//Sample animation using legacy system
								if (animator == null)
								{
									bool wasLegacy = clip.legacy;
									clip.legacy = true;
									clip.SampleAnimation(gameObject, normalisedTime * clip.length);
									clip.legacy = wasLegacy;
								}
								//Using animator, update animator to progress forward with animation
								else
								{
									animator.Play(animationStateNames[animIndex], 0, normalisedTime);
									animator.Update(0f);
									yield return null;
								}

								//Save bone matrices
								boneWorldMatrix[animIndex][i] = new Matrix4x4[numBones];

								for (int boneIndex = 0; boneIndex < numBones; boneIndex++)
								{
									boneWorldMatrix[animIndex][i][boneIndex] = rootMat * bones[boneIndex].localToWorldMatrix * bindposes[boneIndex];
								}

								//Save root motion velocities
								if (hasRootMotion)
								{
									rootMotionVelocities[i] = animator.velocity;
									rootMotionAngularVelocities[i] = animator.angularVelocity;
								}
							}

							//Create animation
							animations[animIndex] = new GPUAnimations.Animation(name, startOffset, totalFrames, clip.frameRate, wrapMode, events, hasRootMotion, rootMotionVelocities, rootMotionAngularVelocities);
							startOffset += totalSamples;
						}

						//Create and save texture! Work out width!
						int textureSize;

						if (!CalculateTextureSize(boneWorldMatrix, out textureSize))
						{
							DestroyImmediate(gameObject);
							_working = false;
							yield break;
						}

						Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBAHalf, false);
						texture.filterMode = FilterMode.Point;

						//Loop through animations / frames / bones setting pixels for each bone matrix
						int pixelx = 0;
						int pixely = 0;

						//Foreach animation
						for (int animIndex = 0; animIndex < boneWorldMatrix.Length; animIndex++)
						{
							//For each frame
							for (int j = 0; j < boneWorldMatrix[animIndex].Length; j++)
							{
								//Convert all frame bone matrices to colors
								Color[] matrixPixels = ConvertMatricesToColor(boneWorldMatrix[animIndex][j]);
								texture.SetPixels(pixelx, pixely, 4, numBones, matrixPixels);

								//Shift to next frame position
								pixelx += 4;

								//If less than 4 pixels from edge, move to next row
								if (textureSize - pixelx < 4)
								{
									pixelx = 0;
									pixely += numBones;
								}
							}
						}

						texture.Apply();

						GPUAnimations animationTexture = new GPUAnimations(animations, boneNames, texture);

						SaveAnimationTexture(animationTexture, path);

						DestroyImmediate(gameObject);
						_working = false;
						yield break;
					}

					private static void SaveAnimationTexture(GPUAnimations animationTexture, string fileName)
					{
						string directory = Path.GetDirectoryName(fileName);
						if (!Directory.Exists(directory))
						{
							Directory.CreateDirectory(directory);
						}

						FileStream file = File.Open(fileName, FileMode.Create);
						BinaryWriter writer = new BinaryWriter(file);

						writer.Write(animationTexture._bones.Length);
						for (int i = 0; i < animationTexture._bones.Length; i++)
						{
							writer.Write(animationTexture._bones[i]);
						}

						writer.Write(animationTexture._animations.Length);

						for (int i = 0; i < animationTexture._animations.Length; i++)
						{
							writer.Write(animationTexture._animations[i]._name);
							writer.Write(animationTexture._animations[i]._startFrameOffset);
							writer.Write(animationTexture._animations[i]._totalFrames);
							writer.Write(animationTexture._animations[i]._fps);
							writer.Write((int)animationTexture._animations[i]._wrapMode);

							writer.Write(animationTexture._animations[i]._events.Length);

							for (int j = 0; j < animationTexture._animations[i]._events.Length; j++)
							{
								writer.Write(animationTexture._animations[i]._events[j].time);
								writer.Write(animationTexture._animations[i]._events[j].functionName);
								writer.Write(animationTexture._animations[i]._events[j].stringParameter);
								writer.Write(animationTexture._animations[i]._events[j].floatParameter);
								writer.Write(animationTexture._animations[i]._events[j].intParameter);
								//TO DO?
								//writer.Write(animationTexture._animations[i]._events[j].objectReferenceParameter);
							}

							writer.Write(animationTexture._animations[i]._hasRootMotion);

							if (animationTexture._animations[i]._hasRootMotion)
							{
								for (int j = 0; j < animationTexture._animations[i]._totalFrames; j++)
								{
									writer.Write(animationTexture._animations[i]._rootMotionVelocities[j].x);
									writer.Write(animationTexture._animations[i]._rootMotionVelocities[j].y);
									writer.Write(animationTexture._animations[i]._rootMotionVelocities[j].z);

									writer.Write(animationTexture._animations[i]._rootMotionAngularVelocities[j].x);
									writer.Write(animationTexture._animations[i]._rootMotionAngularVelocities[j].y);
									writer.Write(animationTexture._animations[i]._rootMotionAngularVelocities[j].z);
								}
							}
						}

						//Write texture!
						byte[] bytes = animationTexture._texture.GetRawTextureData();

						writer.Write((int)animationTexture._texture.format);
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

					private static bool CheckTextureSize(int textureSize, int numBones, int totalFrames)
					{
						int numBonesPerHeight = textureSize / numBones;
						int numMatriciesPerWidth = textureSize / GPUAnimations.kPixelsPerBoneMatrix;

						return numBonesPerHeight * numMatriciesPerWidth >= totalFrames;
					}

					private static bool CalculateTextureSize(Matrix4x4[][][] boneWorldMatrix, out int textureSize)
					{
						int numBones = boneWorldMatrix[0][0].Length;
						int numFrames = 0;

						for (int i = 0; i < boneWorldMatrix.Length; i++)
						{
							numFrames += boneWorldMatrix[i].Length;
						}

						int totalPixels = numFrames * numBones * GPUAnimations.kPixelsPerBoneMatrix;
						
						int textureSizeIndex = 0;
						
						while (textureSizeIndex < kAllowedTextureSizes.Length)
						{
							textureSize = kAllowedTextureSizes[textureSizeIndex];

							if (CheckTextureSize(textureSize, numBones, numFrames))
							{
								return true;
							}

							textureSizeIndex++;

						}

						textureSize = -1;
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
					
					private static void GetAnimationClipsFromAnimator(Animator animator, out AnimationClip[] animationClips, out string[] animationStateNames)
					{
						AnimatorController animatorController = (AnimatorController)animator.runtimeAnimatorController;
						AnimatorControllerLayer controllerLayer = animatorController.layers[0];

						List<AnimationClip> clips = new List<AnimationClip>();
						List<string> stateNames = new List<string>();

						foreach (ChildAnimatorState state in controllerLayer.stateMachine.states)
						{
							AnimationClip clip = state.state.motion as AnimationClip;

							if (clip != null && !clips.Contains(clip))
							{
								clips.Add(clip);
								stateNames.Add(state.state.name);
							}
						}

						animationClips = clips.ToArray();
						animationStateNames = stateNames.ToArray();
					}

					private static void CreateMeshForAnimations(Mesh sourceMesh, TEXCOORD boneIdChannel, TEXCOORD boneWeightChannel, string path)
					{
						Mesh mesh = CreateMeshWithExtraData(sourceMesh, boneIdChannel, boneWeightChannel);
						mesh.UploadMeshData(true);

						string assetPath = FileUtil.GetProjectRelativePath(path);

						AssetDatabase.CreateAsset(mesh, assetPath);
						AssetDatabase.SaveAssets();
					}

					private static Mesh CreateMeshWithExtraData(Mesh sourceMesh, TEXCOORD boneIdsUVChanel, TEXCOORD boneWeightsUVChanel, int bonesPerVertex = 4)
					{
						Mesh mesh = Instantiate(sourceMesh);

						//Add bone IDs and weights as texture data
						{
							List<Vector4> boneIdsData = new List<Vector4>();
							List<Vector4> boneWeightsData = new List<Vector4>();

							for (int i = 0; i != mesh.vertexCount; ++i)
							{
								BoneWeight weight = mesh.boneWeights[i];

								Vector4 boneIds;

								boneIds.x = weight.boneIndex0;
								boneIds.y = weight.boneIndex1;
								boneIds.z = weight.boneIndex2;
								boneIds.w = weight.boneIndex3;

								boneIdsData.Add(boneIds);

								Vector4 boneWeights;

								boneWeights.x = weight.weight0;
								boneWeights.y = weight.weight1;
								boneWeights.z = weight.weight2;
								boneWeights.w = weight.weight3;

								if (bonesPerVertex == 3)
								{
									float rate = 1.0f / (weight.boneIndex0 + weight.boneIndex1 + weight.boneIndex2);
									boneWeights.x = boneWeights.x * rate;
									boneWeights.y = boneWeights.y * rate;
									boneWeights.z = boneWeights.z * rate;
									boneWeights.w = -0.1f;
								}
								else if (bonesPerVertex == 2)
								{
									float rate = 1.0f / (weight.boneIndex0 + weight.boneIndex1);
									boneWeights.x = boneWeights.x * rate;
									boneWeights.y = boneWeights.y * rate;
									boneWeights.z = -0.1f;
									boneWeights.w = -0.1f;
								}
								else if (bonesPerVertex == 1)
								{
									boneWeights.x = 1.0f;
									boneWeights.y = -0.1f;
									boneWeights.z = -0.1f;
									boneWeights.w = -0.1f;
								}

								boneWeightsData.Add(boneWeights);
							}

							mesh.SetUVs((int)boneIdsUVChanel, boneIdsData);
							mesh.SetUVs((int)boneWeightsUVChanel, boneWeightsData);
						}

						return mesh;
					}
					#endregion
				}
			}
		}
	}
}
