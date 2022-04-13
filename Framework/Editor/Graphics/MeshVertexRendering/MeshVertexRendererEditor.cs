using UnityEngine;
using UnityEditor;
using System.IO;

namespace Framework
{
	namespace Graphics
	{
		namespace Editor
		{
			public class MeshVertexRendererEditor : EditorWindow
			{
				protected GameObject _sourceObject;
				protected string _currentFileName;
				
				#region EditorWindow
				[MenuItem("Tools/Mesh Vertex Renderer", false)]
				private static void MakeWindow()
				{
					MeshVertexRendererEditor window = GetWindow(typeof(MeshVertexRendererEditor)) as MeshVertexRendererEditor;
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

					EditorGUILayout.LabelField("Generate Mesh For Vertex Rendering");

					_sourceObject = EditorGUILayout.ObjectField("Source Mesh", _sourceObject, typeof(GameObject), true) as GameObject;

					if (GUILayout.Button("Generate"))
					{
						string path = EditorUtility.SaveFilePanel("Save Mesh Asset", "Assets/", name, "asset");

						if (!string.IsNullOrEmpty(path))
						{
							Mesh sourceMesh = GetMesh(_sourceObject);

							if (sourceMesh == null)
								return;

							//Skinned Mesh
							{
								sourceMesh = GenerateSkinnedMesh(sourceMesh);

								sourceMesh.UploadMeshData(true);
								string assetPath = FileUtil.GetProjectRelativePath(Path.GetFileNameWithoutExtension(path) + "_Skinned.asset");

								AssetDatabase.CreateAsset(sourceMesh, assetPath);
								AssetDatabase.SaveAssets();
							}

							//Rendered mesh
							{
								Mesh mesh = GenerateRenderedMesh(sourceMesh);

								mesh.UploadMeshData(true);
								string assetPath = FileUtil.GetProjectRelativePath(path);

								AssetDatabase.CreateAsset(mesh, assetPath);
								AssetDatabase.SaveAssets();
							}
						}
					}
				}
				#endregion

				protected static Mesh GetMesh(GameObject sourceObject)
				{
					SkinnedMeshRenderer skinnedMesh = sourceObject.GetComponent<SkinnedMeshRenderer>();
					MeshFilter meshFilter = sourceObject.GetComponent<MeshFilter>();

					if (skinnedMesh != null)
					{
						return skinnedMesh.sharedMesh;
					}
					else if (meshFilter != null)
					{
						return meshFilter.sharedMesh;
					}

					return null;
				}

				protected static Mesh GenerateRenderedMesh(Mesh sourceMesh)
				{
					Mesh mesh = new Mesh();

					int sourceMeshVertCount = sourceMesh.vertexCount;

					//Update verts / uvs
					{
						Vector3[] vertices = new Vector3[sourceMeshVertCount * 4];
						Vector2[] uvs = new Vector2[sourceMeshVertCount * 4];

						int[] triangles = new int[sourceMeshVertCount * 6];

						//For each vert in avatar...
						for (int i = 0; i < sourceMeshVertCount; i++)
						{
							int vertOffset = i * 4;

							//Create quad verts
							vertices[vertOffset + 0] = new Vector3(-0.5f, -0.5f, i);
							vertices[vertOffset + 1] = new Vector3(0.5f, -0.5f, i);
							vertices[vertOffset + 2] = new Vector3(-0.5f, 0.5f, i);
							vertices[vertOffset + 3] = new Vector3(0.5f, 0.5f, i);

							//Create UVs
							uvs[vertOffset + 0] = new Vector2(0, 0);
							uvs[vertOffset + 1] = new Vector2(1, 0);
							uvs[vertOffset + 2] = new Vector2(0, 1);
							uvs[vertOffset + 3] = new Vector2(1, 1);

							int indexOffset = i * 6;

							//Work out indices
							triangles[indexOffset + 0] = vertOffset + 0;
							triangles[indexOffset + 1] = vertOffset + 1;
							triangles[indexOffset + 2] = vertOffset + 2;
							triangles[indexOffset + 3] = vertOffset + 2;
							triangles[indexOffset + 4] = vertOffset + 3;
							triangles[indexOffset + 5] = vertOffset + 1;
						}

						mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
						mesh.vertices = vertices;
						mesh.uv = uvs;
						mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
						mesh.bounds = sourceMesh.bounds;
					}

					//Update colors
					if (sourceMesh.colors != null)
					{
						Color[] colors = new Color[sourceMeshVertCount * 4];

						//For each vert in avatar...
						for (int i = 0; i < sourceMeshVertCount; i++)
						{
							int vertOffset = i * 4;

							Color color = sourceMesh.colors[i];

							colors[vertOffset + 0] = color;
							colors[vertOffset + 1] = color;
							colors[vertOffset + 2] = color;
							colors[vertOffset + 3] = color;
						}

						mesh.colors = colors;
					}

					return mesh;
				}

				protected static Mesh GenerateSkinnedMesh(Mesh sourceMesh)
				{
					Mesh mesh = Instantiate(sourceMesh);

					int sourceMeshVertCount = sourceMesh.vertexCount;

					//Update UVs and Indices (change to point topology, uv is info for where vert pixel is)
					{
						float textureSize = MeshVertexRenderer.GetTextureSize(sourceMeshVertCount);
						
						Vector2[] uvs = new Vector2[sourceMeshVertCount];
						int[] indices = new int[sourceMeshVertCount];

						for (int i = 0; i < sourceMeshVertCount; i++)
						{
							//Set mesh uv to be the screen pos for this vert	
							float row = Mathf.Floor(i / textureSize);
							float col = i - (row * textureSize);

							uvs[i].x = (col + 0.5f) / textureSize;
							uvs[i].y = (row + 0.5f) / textureSize;
							indices[i] = i;
						}

						mesh.uv = uvs;
						mesh.SetIndices(indices, MeshTopology.Points, 0);
					}

					return mesh;
				}
			}
		}
	}
}
