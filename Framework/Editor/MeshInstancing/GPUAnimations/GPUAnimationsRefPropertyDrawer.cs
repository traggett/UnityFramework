using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils.Editor;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			namespace Editor
			{
				[CustomPropertyDrawer(typeof(GPUAnimationsRef))]
				public class GPUAnimationsRefPropertyDrawer : PropertyDrawer
				{
					public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
					{
						EditorGUI.BeginProperty(position, label, property);

						SerializedProperty assetProp = property.FindPropertyRelative("_asset");

						EditorGUI.BeginChangeCheck();

						EditorGUI.ObjectField(position, assetProp, label);

						if (EditorGUI.EndChangeCheck())
						{
							GPUAnimationsRef animationTexture = SerializedPropertyUtils.GetSerializedPropertyValue<GPUAnimationsRef>(property);
							animationTexture.UnloadTexture();
							SerializedPropertyUtils.SetSerializedPropertyValue(property, animationTexture);
						}

						EditorGUI.EndProperty();
					}

					public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
					{
						return EditorGUIUtility.singleLineHeight;
					}
				}
			}
		}
	}
}