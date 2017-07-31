using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Framework
{
	using UnityEngine;
	using Utils;
	using Utils.Editor;

	namespace DynamicValueSystem
	{
		namespace Editor
		{
			public abstract class DynamicValuePropertyDrawer<T> : PropertyDrawer
			{
				public enum eEdtiorType
				{
					Static,
					Source,
				}

				private struct MemeberData
				{
					public DynamicValue<T>.eSourceType _sourceType;
					public FieldInfo _fieldInfo;
					public int _index;

					public MemeberData(DynamicValue<T>.eSourceType sourceType, FieldInfo fieldInfo, int index)
					{
						_sourceType = sourceType;
						_fieldInfo = fieldInfo;
						_index = index;
					}
				}		

				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					SerializedProperty sourceTypeProperty = property.FindPropertyRelative("_sourceType");
					SerializedProperty sourceObjectProp = property.FindPropertyRelative("_sourceObject");
					SerializedProperty sourceObjectMemberNameProp = property.FindPropertyRelative("_sourceObjectMemberName");
					SerializedProperty sourceObjectMemberIndexProp = property.FindPropertyRelative("_sourceObjectMemberIndex");
					SerializedProperty valueProperty = property.FindPropertyRelative("_value");
					
					Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

					sourceTypeProperty.isExpanded = EditorGUI.Foldout(foldoutPosition, sourceTypeProperty.isExpanded, label != null ? label.text : property.displayName);
					
					if (sourceTypeProperty.isExpanded)
					{
						int origIndent = EditorGUI.indentLevel;
						EditorGUI.indentLevel++;
						
						eEdtiorType sourceType = (DynamicValue<T>.eSourceType)sourceTypeProperty.intValue == DynamicValue<T>.eSourceType.Static ? eEdtiorType.Static : eEdtiorType.Source;
						bool tempOverrideType = AllowDragComponentToSetAsSource() && sourceType == eEdtiorType.Static && EditorUtils.GetDraggingComponent<MonoBehaviour>() != null;
						if (tempOverrideType)
						{
							sourceType = eEdtiorType.Source;
						}

						EditorGUI.BeginChangeCheck();

						Rect typePosition = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
						eEdtiorType edtiorType = (eEdtiorType)EditorGUI.EnumPopup(typePosition, "Source Type", sourceType);

						if (EditorGUI.EndChangeCheck())
						{
							sourceObjectProp.objectReferenceValue = null;
							sourceTypeProperty.intValue = Convert.ToInt32(edtiorType);
						}

						Rect valuePosition = new Rect(position.x, typePosition.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

						switch (sourceType)
						{
							case eEdtiorType.Source:
								{
									DrawSourceObjectField(sourceObjectProp, sourceTypeProperty, sourceObjectMemberNameProp, sourceObjectMemberIndexProp, valuePosition);
								}
								break;
							case eEdtiorType.Static:
								{
									DrawStaticValueField(valuePosition, valueProperty);
								}
								break;
						}

						EditorGUI.indentLevel = origIndent;
					}

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					SerializedProperty sourceTypeProperty = property.FindPropertyRelative("_sourceType");
					SerializedProperty valueProperty = property.FindPropertyRelative("_value");

					float height = EditorGUIUtility.singleLineHeight;

					if (sourceTypeProperty.isExpanded)
					{
						height += EditorGUIUtility.singleLineHeight;

						eEdtiorType sourceType = (DynamicValue<T>.eSourceType)sourceTypeProperty.intValue == DynamicValue<T>.eSourceType.Static ? eEdtiorType.Static : eEdtiorType.Source;

						switch (sourceType)
						{
							case eEdtiorType.Source: height += EditorGUIUtility.singleLineHeight * 3; break;
							case eEdtiorType.Static: height += GetStaticValueFieldHeight(valueProperty); break;
						}
					}

					return height;
				}

				public virtual void DrawStaticValueField(Rect position, SerializedProperty valueProperty)
				{
					EditorGUI.PropertyField(position, valueProperty, new GUIContent("Value"));
				}

				public virtual float GetStaticValueFieldHeight(SerializedProperty valueProperty)
				{
					return EditorGUIUtility.singleLineHeight;
				}

				protected virtual bool AllowDragComponentToSetAsSource()
				{
					return true;
				}

				private void DrawSourceObjectField(SerializedProperty sourceObjectProp, SerializedProperty sourceTypeProperty, SerializedProperty sourceObjectMemberNameProp, SerializedProperty sourceObjectMemberIndexProp, Rect valuePosition)
				{
					Component currentComponent = sourceObjectProp.objectReferenceValue as Component;

					float height;
					Component selectedComponent = EditorUtils.ComponentField<MonoBehaviour>(new GUIContent("Source Component"), valuePosition, currentComponent, out height);

					if (currentComponent != selectedComponent)
					{
						currentComponent = selectedComponent;
						sourceObjectProp.objectReferenceValue = selectedComponent;
						sourceObjectMemberIndexProp.intValue = -1;

						if (currentComponent == null)
							sourceTypeProperty.intValue = Convert.ToInt32(DynamicValue<T>.eSourceType.SourceObject);
					}

					valuePosition.y += height;

					if (currentComponent != null)
					{
						height += DrawObjectDropDown(currentComponent, valuePosition, sourceTypeProperty, sourceObjectMemberNameProp, sourceObjectMemberIndexProp);
					}
				}

				private float DrawObjectDropDown(object obj, Rect valuePosition, SerializedProperty sourceTypeProperty, SerializedProperty sourceObjectMemberNameProp, SerializedProperty sourceObjectMemberIndexProp)
				{
					List<GUIContent> memberLabels = new List<GUIContent>();
					List<MemeberData> memeberInfo = new List<MemeberData>();

					int index = 0;

					//If the object itself is an IValueSource<T> then it can be selected
					if (SystemUtils.IsTypeOf(typeof(IValueSource<T>), obj.GetType()))
					{
						memberLabels.Add(new GUIContent(".this"));
						memeberInfo.Add(new MemeberData(DynamicValue<T>.eSourceType.SourceObject, null, -1));

						if (sourceObjectMemberIndexProp.intValue == -1 && string.IsNullOrEmpty(sourceObjectMemberNameProp.stringValue))
							index = 0;
					}

					//If the object is a dynamic value source container then add its value sources to the list
					if (SystemUtils.IsTypeOf(typeof(IValueSourceContainer), obj.GetType()))
					{
						IValueSourceContainer valueSourceContainer = (IValueSourceContainer)obj;

						for (int i = 0; i < valueSourceContainer.GetNumberOfValueSource(); i++)
						{
							if (SystemUtils.IsTypeOf(typeof(IValueSource<T>), valueSourceContainer.GetValueSource(i).GetType()))
							{
								//Ideally be able to get value source name as well? just for editor?
								memberLabels.Add(new GUIContent(valueSourceContainer.GetValueSourceName(i).ToString()));
								memeberInfo.Add(new MemeberData(DynamicValue<T>.eSourceType.SourceDynamicMember, null, i));

								if (sourceObjectMemberIndexProp.intValue == i)
									index = memeberInfo.Count - 1;
							}
						}
					
					}

					//Finally add all public fields that are of type IValueSource<T>
					FieldInfo[] fields = DynamicValue<T>.GetDynamicValueFields(obj);

					foreach (FieldInfo field in fields)
					{
						memberLabels.Add(new GUIContent("." + field.Name));
						memeberInfo.Add(new MemeberData(DynamicValue<T>.eSourceType.SourceMember, field, -1));

						if (sourceObjectMemberNameProp.stringValue == field.Name)
							index = memeberInfo.Count - 1;
					}
					
					//Warn if there are no valid options for the object
					if (memeberInfo.Count == 0)
					{
						EditorGUI.LabelField(valuePosition, new GUIContent("Component Property"), new GUIContent("No public IValueSource<" + SystemUtils.GetTypeName(typeof(T)) + ">" + " members!"));
					}
					else
					{
						bool valueChanged = index != sourceTypeProperty.intValue;

						EditorGUI.BeginChangeCheck();
						index = EditorGUI.Popup(valuePosition, new GUIContent("Component Property"), index, memberLabels.ToArray());
						valueChanged |= EditorGUI.EndChangeCheck();

						if (valueChanged)
						{
							sourceTypeProperty.intValue = Convert.ToInt32(memeberInfo[index]._sourceType);
							sourceObjectMemberNameProp.stringValue = memeberInfo[index]._fieldInfo != null ? memeberInfo[index]._fieldInfo.Name : null;
							sourceObjectMemberIndexProp.intValue = memeberInfo[index]._index;
						}
					}

					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}