using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Editor;

	namespace Utils
	{
		namespace Editor
		{
			public sealed class AssetBundleBuilderWindow : UpdatedEditorWindow
			{
				private BuildAssetBundleOptions _bundleOptions = BuildAssetBundleOptions.None;
				private BuildTarget _buildTarget = BuildTarget.StandaloneWindows;
				private string _exportFolder = "StreamingAssets";

				//OPtionall choose AssetBundleBuild?




				//Points to folder bundles will go
				//Choose platform
				#region EditorWindow
				[MenuItem("Window/Asset Bundle Tools", false)]
				private static void MakeWindow()
				{
					AssetBundleBuilderWindow window = GetWindow(typeof(AssetBundleBuilderWindow)) as AssetBundleBuilderWindow;
				}

				private void OnGUI()
				{
					GUILayout.BeginVertical();
					{
						_bundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumPopup("Bundle Options", _bundleOptions);
						_buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", _buildTarget);

						if (GUILayout.Button("Build Asset Bundles"))
						{
							_exportFolder = AssetUtils.GetAppllicationPath(EditorUtility.SaveFolderPanel("Asset Bundle Folder", _exportFolder, ""));

							if (!string.IsNullOrEmpty(_exportFolder))
							{
								BuildPipeline.BuildAssetBundles(_exportFolder, _bundleOptions, _buildTarget);
							}
						}
					}
					GUILayout.EndVertical();
				}
				#endregion
			}
		}
	}
}