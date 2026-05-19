using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Framework
{
	using Framework.Maths.Editor;
	using Maths;

	namespace Graphics
	{
		[CustomEditor(typeof(MaterialPerInstanceProperties), true)]
		public class MaterialPerInstancePropertiesInspector : UnityEditor.Editor
		{
			#region Private Data
			private ReorderableList _propertiesList;
			private MaterialProperty[] _properties;
			private GUIContent[] _propertiesNames;
			#endregion

			#region UnityEditor.Editor
			public void OnEnable()
			{
				_propertiesList = new ReorderableList(new MaterialPerInstanceProperties.IProperty[0], typeof(MaterialPerInstanceProperties.IProperty), false, true, true, false)
				{
					drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnDrawHeader),
					drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawProperty),
					onAddCallback = new ReorderableList.AddCallbackDelegate(OnAddProperty),
					elementHeightCallback = new ReorderableList.ElementHeightCallbackDelegate(OnGetElementHeight),
					showDefaultBackground = true,
				};
			}

			public override void OnInspectorGUI()
			{
				//Drop down list is all these
				MaterialPerInstanceProperties materialPerInstanceProperties = target as MaterialPerInstanceProperties;
				if (materialPerInstanceProperties == null)
					return;

				GUILayout.Label("Per Instance Properties", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_renderer"));

				if (materialPerInstanceProperties._renderer != null)
				{
					_properties = MaterialEditor.GetMaterialProperties(materialPerInstanceProperties._renderer.sharedMaterials);
					_propertiesNames = new GUIContent[_properties.Length];
					for (int i = 0; i < _properties.Length; i++)
						_propertiesNames[i] = new GUIContent(_properties[i].displayName);

					GUILayout.Space(3f);

					_propertiesList.displayAdd = _properties.Length > 0;
					_propertiesList.displayRemove = true;
					_propertiesList.list = GetProperties(materialPerInstanceProperties);
					_propertiesList.DoLayoutList();
					SetProperties(materialPerInstanceProperties, _propertiesList.list);
				}

				serializedObject.ApplyModifiedProperties();
			}
			#endregion

			#region ReorderableList Callbacks
			private void OnAddProperty(ReorderableList list)
			{
				if (_properties.Length > 0)
					_propertiesList.list.Add(GetPerInstanceProperty(_properties[0]));
			}

			private void OnDrawHeader(Rect rect)
			{
				float columnWidth = rect.width /= 2f;
				GUI.Label(rect, "Property Id", EditorStyles.label);
				rect.x += columnWidth;
				GUI.Label(rect, "Value", EditorStyles.label);
			}

			private float OnGetElementHeight(int index)
			{
				float height = EditorGUIUtility.singleLineHeight * 2;

				object property = _propertiesList.list[index];

				if (property is MaterialPerInstanceProperties.Vector4Property)
				{
					MaterialPerInstanceProperties.Vector4Property.ePropertySource source = ((MaterialPerInstanceProperties.Vector4Property)property)._source;

					if (source != MaterialPerInstanceProperties.Vector4Property.ePropertySource.Constant)
						height += EditorGUIUtility.singleLineHeight * 3;
				}
				else if (property is MaterialPerInstanceProperties.TextureProperty)
				{
					MaterialPerInstanceProperties.TextureProperty.ePropertySource source = ((MaterialPerInstanceProperties.TextureProperty)property)._source;

					if (source == MaterialPerInstanceProperties.TextureProperty.ePropertySource.RandomArray)
					{
						//TO DO! show array! get height!
						height += EditorGUIUtility.singleLineHeight * 3;
					}
				}

				return height;
			}

			private void OnDrawProperty(Rect rect, int index, bool selected, bool focused)
			{
				MaterialPerInstanceProperties.IProperty property = (MaterialPerInstanceProperties.IProperty)_propertiesList.list[index];

				Rect nameRect = new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight);
				Rect sourceRect = new Rect(rect.x + nameRect.width, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight);
				Rect valueRect = new Rect(rect.x + nameRect.width, rect.y + nameRect.height, rect.width * 0.5f, EditorGUIUtility.singleLineHeight);

				if (property is MaterialPerInstanceProperties.ColorProperty)
				{
					_propertiesList.list[index] = DrawColorProperty(property, index, nameRect, sourceRect, valueRect);
				}
				else if (property is MaterialPerInstanceProperties.Vector4Property)
				{
					_propertiesList.list[index] = DrawVectorProperty(property, index, nameRect, sourceRect, valueRect);
				}
				else if (property is MaterialPerInstanceProperties.FloatProperty)
				{
					_propertiesList.list[index] = DrawFloatProperty(property, index, nameRect, sourceRect, valueRect);
				}
				else if (property is MaterialPerInstanceProperties.TextureProperty)
				{
					_propertiesList.list[index] = DrawTextureProperty(property, index, nameRect, sourceRect, valueRect);
				}
			}
			#endregion

			#region Private Functions
			private static List<MaterialPerInstanceProperties.IProperty> GetProperties(MaterialPerInstanceProperties materialPerInstanceProperties)
			{
				List<MaterialPerInstanceProperties.IProperty> properties = new List<MaterialPerInstanceProperties.IProperty>();

				properties.AddRange(materialPerInstanceProperties._floatProperties);
				properties.AddRange(materialPerInstanceProperties._colorProperties);
				properties.AddRange(materialPerInstanceProperties._vector4Properties);
				properties.AddRange(materialPerInstanceProperties._textureProperties);

				return properties;
			}

			private static void SetProperties(MaterialPerInstanceProperties materialPerInstanceProperties, IList properties)
			{
				List<MaterialPerInstanceProperties.FloatProperty> floatProperties = new List<MaterialPerInstanceProperties.FloatProperty>();
				List<MaterialPerInstanceProperties.ColorProperty> colorProperties = new List<MaterialPerInstanceProperties.ColorProperty>();
				List<MaterialPerInstanceProperties.Vector4Property> vectorProperties = new List<MaterialPerInstanceProperties.Vector4Property>();
				List<MaterialPerInstanceProperties.TextureProperty> textureProperties = new List<MaterialPerInstanceProperties.TextureProperty>();

				foreach (MaterialPerInstanceProperties.IProperty property in properties)
				{
					if (property is MaterialPerInstanceProperties.FloatProperty)
						floatProperties.Add((MaterialPerInstanceProperties.FloatProperty)property);
					else if (property is MaterialPerInstanceProperties.ColorProperty)
						colorProperties.Add((MaterialPerInstanceProperties.ColorProperty)property);
					else if (property is MaterialPerInstanceProperties.Vector4Property)
						vectorProperties.Add((MaterialPerInstanceProperties.Vector4Property)property);
					else if (property is MaterialPerInstanceProperties.TextureProperty)
						textureProperties.Add((MaterialPerInstanceProperties.TextureProperty)property);
				}

				materialPerInstanceProperties._floatProperties = floatProperties.ToArray();
				materialPerInstanceProperties._colorProperties = colorProperties.ToArray();
				materialPerInstanceProperties._vector4Properties = vectorProperties.ToArray();
				materialPerInstanceProperties._textureProperties = textureProperties.ToArray();
			}

			private static MaterialPerInstanceProperties.IProperty GetPerInstanceProperty(MaterialProperty property)
			{
				MaterialPerInstanceProperties.IProperty perInstanceProperty = null;

				switch (property.type)
				{
					case MaterialProperty.PropType.Color:
						{
							perInstanceProperty = new MaterialPerInstanceProperties.ColorProperty()
							{
								_name = property.name,
								_value = property.colorValue,
								_valueGradient = new Gradient(),
								_source = MaterialPerInstanceProperties.ColorProperty.ePropertySource.Constant
							};
						}
						break;
					case MaterialProperty.PropType.Vector:
						{
							perInstanceProperty = new MaterialPerInstanceProperties.Vector4Property()
							{
								_name = property.name,
								_value = property.vectorValue,
								_xValueCurve = new AnimationCurve(new Keyframe(0, property.vectorValue.x), new Keyframe(1, property.vectorValue.x)),
								_yValueCurve = new AnimationCurve(new Keyframe(0, property.vectorValue.y), new Keyframe(1, property.vectorValue.y)),
								_zValueCurve = new AnimationCurve(new Keyframe(0, property.vectorValue.w), new Keyframe(1, property.vectorValue.w)),
								_wValueCurve = new AnimationCurve(new Keyframe(0, property.vectorValue.z), new Keyframe(1, property.vectorValue.z)),
								_xValueRange = new FloatRange(property.vectorValue.x, property.vectorValue.x),
								_yValueRange = new FloatRange(property.vectorValue.y, property.vectorValue.y),
								_zValueRange = new FloatRange(property.vectorValue.w, property.vectorValue.w),
								_wValueRange = new FloatRange(property.vectorValue.z, property.vectorValue.z),
								_source = MaterialPerInstanceProperties.Vector4Property.ePropertySource.Constant
							};
						}
						break;
					case MaterialProperty.PropType.Float:
						{
							perInstanceProperty = new MaterialPerInstanceProperties.FloatProperty()
							{
								_name = property.name,
								_value = property.floatValue,
								_valueCurve = new AnimationCurve(new Keyframe(0, property.floatValue), new Keyframe(1, property.floatValue)),
								_valueRange = new FloatRange(property.floatValue, property.floatValue),
								_source = MaterialPerInstanceProperties.FloatProperty.ePropertySource.Constant
							};
						}
						break;
					case MaterialProperty.PropType.Texture:
						{
							perInstanceProperty = new MaterialPerInstanceProperties.TextureProperty()
							{
								_name = property.name,
								_value = property.textureValue,
								_valueArray = new Texture[] { property.textureValue },
								_source = MaterialPerInstanceProperties.TextureProperty.ePropertySource.Constant
							};
						}
						break;
				}

				return perInstanceProperty;
			}

			private bool DrawPropertyIdDropdrop(Rect rect, ref MaterialPerInstanceProperties.IProperty property)
			{
				//Draw dropdown - if change check type
				int propIndex = 0;

				for (int i=0; i<_properties.Length; i++)
				{
					if (_properties[i].name == property.GetPropertyId())
					{
						propIndex = i;
						break;
					}
				}

				EditorGUI.BeginChangeCheck();

				propIndex = EditorGUI.Popup(rect, propIndex, _propertiesNames);

				if (EditorGUI.EndChangeCheck())
				{
					property = GetPerInstanceProperty(_properties[propIndex]);
					return true;
				}
				
				return false;
			}			

			private MaterialPerInstanceProperties.IProperty DrawColorProperty(MaterialPerInstanceProperties.IProperty property, int index, Rect nameRect, Rect sourceRect, Rect valueRect)
			{
				MaterialPerInstanceProperties.ColorProperty colorProperty = (MaterialPerInstanceProperties.ColorProperty)property;

				if (DrawPropertyIdDropdrop(nameRect, ref property))
					return property;

				colorProperty._source = (MaterialPerInstanceProperties.ColorProperty.ePropertySource)EditorGUI.EnumPopup(sourceRect, colorProperty._source);

				switch (colorProperty._source)
				{
					case MaterialPerInstanceProperties.ColorProperty.ePropertySource.Constant:
						colorProperty._value = EditorGUI.ColorField(valueRect, colorProperty._value);
						break;
					case MaterialPerInstanceProperties.ColorProperty.ePropertySource.RandomFromGradient:
						colorProperty._valueGradient = EditorGUI.GradientField(valueRect, colorProperty._valueGradient);
						break;
				}

				return colorProperty;
			}

			private MaterialPerInstanceProperties.IProperty DrawVectorProperty(MaterialPerInstanceProperties.IProperty property, int index, Rect nameRect, Rect sourceRect, Rect valueRect)
			{
				MaterialPerInstanceProperties.Vector4Property vectorProperty = (MaterialPerInstanceProperties.Vector4Property)property;

				if (DrawPropertyIdDropdrop(nameRect, ref property))
					return property;

				vectorProperty._source = (MaterialPerInstanceProperties.Vector4Property.ePropertySource)EditorGUI.EnumPopup(sourceRect, vectorProperty._source);

				switch (vectorProperty._source)
				{
					case MaterialPerInstanceProperties.Vector4Property.ePropertySource.Constant:
						vectorProperty._value = EditorGUI.Vector4Field(valueRect, "", vectorProperty._value);
						break;
					case MaterialPerInstanceProperties.Vector4Property.ePropertySource.RandomRange:
						{
							vectorProperty._xValueRange = FloatRangeDrawer.FloatRangeField(valueRect, vectorProperty._xValueRange);
							valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + EditorGUIUtility.singleLineHeight);
							vectorProperty._yValueRange = FloatRangeDrawer.FloatRangeField(valueRect, vectorProperty._yValueRange);
							valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + EditorGUIUtility.singleLineHeight);
							vectorProperty._zValueRange = FloatRangeDrawer.FloatRangeField(valueRect, vectorProperty._zValueRange);
							valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + EditorGUIUtility.singleLineHeight);
							vectorProperty._wValueRange = FloatRangeDrawer.FloatRangeField(valueRect, vectorProperty._wValueRange);
						}
						break;
					case MaterialPerInstanceProperties.Vector4Property.ePropertySource.RandomCurve:
						{
							vectorProperty._xValueCurve = EditorGUI.CurveField(valueRect, vectorProperty._xValueCurve);
							valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + EditorGUIUtility.singleLineHeight);
							vectorProperty._yValueCurve = EditorGUI.CurveField(valueRect, vectorProperty._yValueCurve);
							valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + EditorGUIUtility.singleLineHeight);
							vectorProperty._zValueCurve = EditorGUI.CurveField(valueRect, vectorProperty._zValueCurve);
							valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + EditorGUIUtility.singleLineHeight);
							vectorProperty._wValueCurve = EditorGUI.CurveField(valueRect, vectorProperty._wValueCurve);
						}
						break;
				}

				return vectorProperty;
			}

			private MaterialPerInstanceProperties.IProperty DrawFloatProperty(MaterialPerInstanceProperties.IProperty property, int index, Rect nameRect, Rect sourceRect, Rect valueRect)
			{
				MaterialPerInstanceProperties.FloatProperty floatProperty = (MaterialPerInstanceProperties.FloatProperty)property;

				if (DrawPropertyIdDropdrop(nameRect, ref property))
					return property;

				floatProperty._source = (MaterialPerInstanceProperties.FloatProperty.ePropertySource)EditorGUI.EnumPopup(sourceRect, floatProperty._source);

				switch (floatProperty._source)
				{
					case MaterialPerInstanceProperties.FloatProperty.ePropertySource.Constant:
						floatProperty._value = EditorGUI.FloatField(valueRect, floatProperty._value);
						break;
					case MaterialPerInstanceProperties.FloatProperty.ePropertySource.RandomRange:
						floatProperty._valueRange = FloatRangeDrawer.FloatRangeField(valueRect, floatProperty._valueRange);
						break;
					case MaterialPerInstanceProperties.FloatProperty.ePropertySource.RandomCurve:
						floatProperty._valueCurve = EditorGUI.CurveField(valueRect, floatProperty._valueCurve);
						break;
				}

				return floatProperty;
			}

			private MaterialPerInstanceProperties.IProperty DrawTextureProperty(MaterialPerInstanceProperties.IProperty property, int index, Rect nameRect, Rect sourceRect, Rect valueRect)
			{
				MaterialPerInstanceProperties.TextureProperty texureProperty = (MaterialPerInstanceProperties.TextureProperty)property;

				if (DrawPropertyIdDropdrop(nameRect, ref property))
					return property;

				texureProperty._source = (MaterialPerInstanceProperties.TextureProperty.ePropertySource)EditorGUI.EnumPopup(sourceRect, texureProperty._source);

				switch (texureProperty._source)
				{
					case MaterialPerInstanceProperties.TextureProperty.ePropertySource.Constant:
						texureProperty._value = (Texture)EditorGUI.ObjectField(valueRect, texureProperty._value, typeof(Texture), false);
						break;
					case MaterialPerInstanceProperties.TextureProperty.ePropertySource.RandomArray:
						//TO DO
						//texureProperty._valueCurve = EditorGUI.CurveField(valueRect, texureProperty._valueCurve);
						break;
				}

				return texureProperty;
			}
			#endregion
		}
	}
}