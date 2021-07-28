using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Framework
{
    namespace UI
    {
        [CustomEditor(typeof(ProcedualUIFade))]
        [CanEditMultipleObjects]
        public class ProcedualUIFadeEditor : ImageEditor
        {
            private SerializedProperty _direction;
            private SerializedProperty _toColor;
            private SerializedProperty _fromColor;
            private SerializedProperty _sprite;
            private SerializedProperty _material;
            private SerializedProperty _raycastTarget;
            private SerializedProperty _raycastPadding;
            private SerializedProperty _maskable;

            private static readonly GUIContent _fromColorLable = new GUIContent("From Color"); 

            protected override void OnEnable()
            {
                base.OnEnable();

                _sprite = serializedObject.FindProperty("m_Sprite");
                _fromColor = serializedObject.FindProperty("m_Color");
                _toColor = serializedObject.FindProperty("_toColor");
                _direction = serializedObject.FindProperty("_direction");
                _material = serializedObject.FindProperty("m_Material");
                _raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
                _raycastPadding = serializedObject.FindProperty("m_RaycastPadding");
                _maskable = serializedObject.FindProperty("m_Maskable");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(_sprite);
                EditorGUILayout.PropertyField(_fromColor, _fromColorLable);
                EditorGUILayout.PropertyField(_toColor);
                EditorGUILayout.PropertyField(_direction);
                EditorGUILayout.PropertyField(_material);
                EditorGUILayout.PropertyField(_raycastTarget);
                EditorGUILayout.PropertyField(_raycastPadding);
                EditorGUILayout.PropertyField(_maskable);

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
