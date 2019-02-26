using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils.Editor;

	namespace MeshInstancing
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(AnimationTextureRef))]
			public class AnimationTextureRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					
					SerializedProperty assetProp = property.FindPropertyRelative("_asset");

					EditorGUI.BeginChangeCheck();

					EditorGUI.ObjectField(position, assetProp, label);

					if (EditorGUI.EndChangeCheck())
					{
						AnimationTextureRef animationTexture = SerializedPropertyUtils.GetSerializedPropertyValue<AnimationTextureRef>(property);
						animationTexture.UnloadTexture();
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