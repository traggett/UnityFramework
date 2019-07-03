using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Framework
{
	using Editor;

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
					private int _numBonesPerVertex = 4;

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
							EditorGUILayout.LabelField("Generate GPU Animation Texture", EditorStyles.largeLabel);
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

							Animator animator = GameObjectUtils.GetComponent<Animator>(_evaluatedObject, true);

							if (animator != null)
							{
								_useAnimator = EditorGUILayout.Toggle("Use Animator", _useAnimator);
							}
							else
							{
								_useAnimator = false;
							}

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
							EditorGUILayout.LabelField("Generate GPU Animated Mesh", EditorStyles.largeLabel);
							EditorGUILayout.Separator();

							_mesh = EditorGUILayout.ObjectField("Mesh", _mesh, typeof(Mesh), true) as Mesh;

							_boneIdChanel = (TEXCOORD)EditorGUILayout.EnumPopup("Bone IDs UV Channel", _boneIdChanel);
							_boneWeightChannel = (TEXCOORD)EditorGUILayout.EnumPopup("Bone Weights UV Channel", _boneWeightChannel);
							_numBonesPerVertex = EditorGUILayout.IntSlider(new GUIContent("Bones Per Vertex"), _numBonesPerVertex, 1, 4);

							if (_mesh != null)
							{
								if (GUILayout.Button("Generate Animated Texture Ready Mesh"))
								{
									string path = EditorUtility.SaveFilePanel("Save Mesh Asset", "Assets/", name, "asset");

									if (!string.IsNullOrEmpty(path))
									{
										CreateMeshForAnimations(_mesh, _boneIdChanel, _boneWeightChannel, _numBonesPerVertex, path);
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
						AnimationClip[] animationClips = _animations;
						Animator animator = null;
						string[] animationStateNames = null;
						int[] animationStateLayers = null;

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


							GetAnimationClipsFromAnimator(animator, out animationClips, out animationStateNames, out animationStateLayers);

							if (animationClips.Length == 0)
							{
								DestroyImmediate(gameObject);
								_working = false;
								yield break;
							}

							AnimatorUtility.DeoptimizeTransformHierarchy(animator.gameObject);
							animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
						}

						SkinnedMeshRenderer[] skinnedMeshes = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
						SkinnedMeshRenderer skinnedMesh = skinnedMeshes[_skinnedMeshIndex];
						Matrix4x4 rootMat = gameObject.transform.worldToLocalMatrix;
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
						Vector3[] origBonePositons = new Vector3[numBones];
						Quaternion[] origBoneRotations = new Quaternion[numBones];
						Vector3[] origBoneScales = new Vector3[numBones];

						GPUAnimations.Animation[] animations = new GPUAnimations.Animation[animationClips.Length];

						//3d array - animation / frame / bones
						Matrix4x4[][][] boneMatricies = new Matrix4x4[animations.Length + 1][][];

						//Insert dummy one frame animation at start which is just the reference pose
						boneMatricies[0] = new Matrix4x4[1][];
						boneMatricies[0][0] = new Matrix4x4[numBones];

						for (int i=0; i<numBones; i++)
						{
							boneNames[i] = bones[i].gameObject.name;
							origBonePositons[i] = bones[i].localPosition;
							origBoneRotations[i] = bones[i].localRotation;
							origBoneScales[i] = bones[i].localScale;

							//Save reference pose bone matrix
							boneMatricies[0][0][i] = rootMat * bones[i].localToWorldMatrix * bindposes[i];
						}
						
						//Start after first frame (as this frame is our reference pose)
						int startOffset = 1;

						for (int animIndex = 0; animIndex < animations.Length; animIndex++)
						{
							AnimationClip clip = animationClips[animIndex];
							bool hasRootMotion = _useAnimator && clip.hasMotionCurves;

							string name = clip.name;
							int totalFrames = Mathf.Max(Mathf.FloorToInt(clip.length * clip.frameRate), 1);
							int totalSamples = totalFrames + 1;
							WrapMode wrapMode = clip.wrapMode;
							AnimationEvent[] events = clip.events;

							//Sample animation
							boneMatricies[animIndex + 1] = new Matrix4x4[totalSamples][];

							Vector3[] rootMotionVelocities = null;
							Vector3[] rootMotionAngularVelocities = null;

							if (hasRootMotion)
							{
								rootMotionVelocities = new Vector3[totalSamples];
								rootMotionAngularVelocities = new Vector3[totalSamples];
							}

							//Reset all bone transforms before sampling animations
							for (int i = 0; i < numBones; i++)
							{
								bones[i].localPosition = origBonePositons[i];
								bones[i].localRotation = origBoneRotations[i];
								bones[i].localScale = origBoneScales[i];
							}

							//If using an animator, start playing animation with correct layer weights set
							if (_useAnimator)
							{
								for (int layer=1; layer < animator.layerCount; layer++)
								{
									animator.SetLayerWeight(animIndex, layer == animationStateLayers[animIndex] ? 1.0f : 0.0f);
								}
								
								animator.Play(animationStateNames[animIndex], animationStateLayers[animIndex]);

								//Needed to prevent first frame pop?
								{
									animator.Update(0f);
									yield return null;
									animator.Play(animationStateNames[animIndex], animationStateLayers[animIndex]);
									animator.Update(0f);
									yield return null;
									animator.Play(animationStateNames[animIndex], animationStateLayers[animIndex]);
									animator.Update(0f);
									yield return null;
								}
							}
							
							for (int i = 0; i < totalSamples; i++)
							{
								float normalisedTime = i / (float)totalFrames;
								
								//Using animator, update animator to progress forward with animation
								if (_useAnimator)
								{
									for (int layer = 1; layer < animator.layerCount; layer++)
									{
										animator.SetLayerWeight(layer, 0.0f);
									}

									animator.SetLayerWeight(animationStateLayers[animIndex], 1.0f);
									float layerWeight = animator.GetLayerWeight(animationStateLayers[animIndex]);

									animator.Play(animationStateNames[animIndex], animationStateLayers[animIndex], normalisedTime);
									animator.Update(0f);

									layerWeight = animator.GetLayerWeight(animationStateLayers[animIndex]);

									//Wait for end of frame
									yield return null;
								}
								//Sample animation using legacy system
								else
								{
									bool wasLegacy = clip.legacy;
									clip.legacy = true;
									clip.SampleAnimation(gameObject, normalisedTime * clip.length);
									clip.legacy = wasLegacy;
								}

								//Save bone matrices
								boneMatricies[animIndex + 1][i] = new Matrix4x4[numBones];

								for (int boneIndex = 0; boneIndex < numBones; boneIndex++)
								{
									boneMatricies[animIndex + 1][i][boneIndex] = rootMat * bones[boneIndex].localToWorldMatrix * bindposes[boneIndex];
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

						//Create and save texture
						if (!CalculateTextureSize(boneMatricies, out int textureSize))
						{
							DestroyImmediate(gameObject);
							_working = false;
							yield break;
						}

						Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBAHalf, false)
						{
							filterMode = FilterMode.Point
						};

						//Loop through animations / frames / bones setting pixels for each bone matrix
						int pixelx = 0;
						int pixely = 0;

						//Foreach animation
						for (int animIndex = 0; animIndex < boneMatricies.Length; animIndex++)
						{
							//For each frame
							for (int j = 0; j < boneMatricies[animIndex].Length; j++)
							{
								//Convert all frame bone matrices to colors
								Color[] matrixPixels = ConvertMatricesToColor(boneMatricies[animIndex][j]);
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
					
					private static void GetAnimationClipsFromAnimator(Animator animator, out AnimationClip[] animationClips, out string[] animationStateNames, out int[] animationStateLayers)
					{
						AnimatorController animatorController = (AnimatorController)animator.runtimeAnimatorController;

						List<AnimationClip> clips = new List<AnimationClip>();
						List<string> stateNames = new List<string>();
						List<int> stateLayers = new List<int>();

						for (int i=0; i< animatorController.layers.Length; i++)
						{
							GetAnimationClipsFromStatemachine(i, animatorController.layers[i].stateMachine, ref clips, ref stateNames, ref stateLayers);
						}

						animationClips = clips.ToArray();
						animationStateNames = stateNames.ToArray();
						animationStateLayers = stateLayers.ToArray();
					}

					private static void GetAnimationClipsFromStatemachine(int layer, AnimatorStateMachine stateMachine, ref List<AnimationClip> clips, ref List<string> stateNames, ref List<int> stateLayers)
					{
						foreach (ChildAnimatorState state in stateMachine.states)
						{
							AnimationClip clip = state.state.motion as AnimationClip;

							if (clip != null && !clips.Contains(clip))
							{
								clips.Add(clip);
								stateNames.Add(state.state.name);
								stateLayers.Add(layer);
							}
						}

						foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
						{
							GetAnimationClipsFromStatemachine(layer, childStateMachine.stateMachine, ref clips, ref stateNames, ref stateLayers);
						}
					}

					private static void CreateMeshForAnimations(Mesh sourceMesh, TEXCOORD boneIdChannel, TEXCOORD boneWeightChannel, int bonesPerVertex, string path)
					{
						Mesh mesh = CreateMeshWithExtraData(sourceMesh, boneIdChannel, boneWeightChannel, bonesPerVertex);
						mesh.UploadMeshData(true);

						string assetPath = FileUtil.GetProjectRelativePath(path);

						AssetDatabase.CreateAsset(mesh, assetPath);
						AssetDatabase.SaveAssets();
					}

					private static Mesh CreateMeshWithExtraData(Mesh sourceMesh, TEXCOORD boneIdsUVChanel, TEXCOORD boneWeightsUVChanel, int bonesPerVertex)
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
