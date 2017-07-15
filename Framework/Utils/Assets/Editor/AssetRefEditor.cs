using System;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(AssetRef<>), "PropertyField")]
			public static class AssetRefEditor
			{
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
				{
					Type assetType = SystemUtils.GetGenericImplementationType(typeof(AssetRef<>), obj.GetType());

					if (assetType != null)
					{
						MethodInfo genericFieldMethod = typeof(AssetRefEditor).GetMethod("AssetRefField", BindingFlags.Static | BindingFlags.NonPublic);
						MethodInfo typedFieldMethod = genericFieldMethod.MakeGenericMethod(assetType);

						if (typedFieldMethod != null)
						{
							object[] args = new object[] { obj, label, dataChanged };
							obj = typedFieldMethod.Invoke(null, args);

							if ((bool)args[2])
								dataChanged = true;
						}
					}

					return obj;
				}
				#endregion

				private static AssetRef<T> AssetRefField<T>(AssetRef<T> assetRef, GUIContent label, ref bool dataChanged) where T : UnityEngine.Object
				{
					if (label == null)
						label = new GUIContent();

					label.text += " (" + assetRef + ")";

					bool foldOut = EditorGUILayout.Foldout(assetRef._editorFoldout, label);

					if (foldOut != assetRef._editorFoldout)
					{
						assetRef._editorFoldout = foldOut;
						dataChanged = true;
					}

					if (foldOut)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;

						T asset = EditorGUILayout.ObjectField("File", assetRef._editorAsset, typeof(T), false) as T;

						//If asset changed update GUIDS
						if (assetRef._editorAsset != asset)
						{
							assetRef = new AssetRef<T>(asset);
							assetRef._editorFoldout = foldOut;
							dataChanged = true;
						}

						EditorGUI.indentLevel = origIndent;
					}

					return assetRef;
				}

			}
		}
	}
}