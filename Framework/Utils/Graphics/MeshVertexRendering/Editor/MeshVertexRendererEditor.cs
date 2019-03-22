using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public class MeshVertexRendererEditor : EditorWindow
			{
				protected GameObject _sourceObject;
				protected string _currentFileName;

				#region EditorWindow
				[MenuItem("Tools/Mesh Vertex Renderer", false)]
				static void MakeWindow()
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
							SaveMesh(_sourceObject, path);
						}
					}
				}
				#endregion

				private static void SaveMesh(GameObject sourceObject, string fileName)
				{
					Mesh mesh = new Mesh();
					Mesh sourceMesh = null;
					int sourceMeshVertCount = 0;

					SkinnedMeshRenderer skinnedMesh = sourceObject.GetComponent<SkinnedMeshRenderer>();
					MeshFilter meshFilter = sourceObject.GetComponent<MeshFilter>();

					if (skinnedMesh != null)
					{
						sourceMesh = skinnedMesh.sharedMesh;
					}
					else if (meshFilter != null)
					{
						sourceMesh = meshFilter.sharedMesh;
					}

					sourceMeshVertCount = sourceMesh.vertexCount;

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
					//mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
					mesh.UploadMeshData(true);

					string path = FileUtil.GetProjectRelativePath(fileName);

					AssetDatabase.CreateAsset(mesh, path);
					AssetDatabase.SaveAssets();
				}
			}
		}
	}
}
