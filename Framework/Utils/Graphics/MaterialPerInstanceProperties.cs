using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Framework
{
	using Maths;

	namespace Utils
	{
		public class MaterialPerInstanceProperties : MonoBehaviour
		{
			#region Public Data
			public Renderer _renderer;

			[Serializable]
			public class FloatProperty
			{
				public enum ePropertySource : short
				{
					Constant,
					Range,
					Curve,
				}

				public string _name = "_Value";
				public ePropertySource _source = ePropertySource.Constant;
				public float _value = 0.0f;
				public FloatRange _valueRange = new FloatRange(0f, 1f);
				public AnimationCurve _valueCurve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
			}

			[Serializable]
			public class IntProperty
			{
				public enum ePropertySource : short
				{
					Constant,
					Range,
				}

				public string _name = "_Value";
				public ePropertySource _source = ePropertySource.Constant;
				public int _value = 0;
				public IntRange _valueRange = new IntRange(0, 1);
			}

			[Serializable]
			public class ColorProperty
			{
				public enum ePropertySource : short
				{
					Constant,
					Gradient,
				}

				public string _name = "_Color";
				public ePropertySource _source = ePropertySource.Constant;
				public Color _value = Color.white;
				public Gradient _valueGradient = new Gradient();
			}

			[Serializable]
			public class Vector4Property
			{
				public enum ePropertySource : short
				{
					Constant,
					Range,
					Curves,
				}

				public string _name = "_Value";
				public ePropertySource _source = ePropertySource.Constant;
				public Vector4 _value = Vector4.zero;
				public FloatRange _xValueRange = new FloatRange(0f, 1f);
				public FloatRange _yValueRange = new FloatRange(0f, 1f);
				public FloatRange _zValueRange = new FloatRange(0f, 1f);
				public FloatRange _wValueRange = new FloatRange(0f, 1f);
				public AnimationCurve _xValueCurve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
				public AnimationCurve _yValueCurve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
				public AnimationCurve _zValueCurve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));
				public AnimationCurve _wValueCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
			}

			[Serializable]
			public class TextureProperty
			{
				public enum ePropertySource : short
				{
					Constant,
					Array,
				}

				public string _name = "_Value";
				public ePropertySource _source = ePropertySource.Constant;
				public Texture _value = null;
				public Texture[] _valueArray = new Texture[0];
			}
			
			public FloatProperty[] _floatProperties;
			public IntProperty[] _intProperties;
			public ColorProperty[] _colorProperties;
			public Vector4Property[] _vector4Properties;
			public TextureProperty[] _textureProperties;
			#endregion

			#region MonoBehaviour
			private void OnEnable()
			{
				SetProperties();
			}
			#endregion

			#region Private Functions
			private void SetProperties()
			{
				if (_renderer != null)
				{
					MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

					//Floats
					for (int i=0; i< _floatProperties.Length; i++)
					{
						float value;

						switch (_floatProperties[i]._source)
						{
							case FloatProperty.ePropertySource.Range:
								value = _floatProperties[i]._valueRange.GetRandomValue();
								break;
							case FloatProperty.ePropertySource.Curve:
								value = _floatProperties[i]._valueCurve.Evaluate(Random.value);
								break;
							case FloatProperty.ePropertySource.Constant:
							default:
								value = _floatProperties[i]._value;
								break;

						}

						propertyBlock.SetFloat(_floatProperties[i]._name, value);
					}

					//Ints
					for (int i = 0; i < _intProperties.Length; i++)
					{
						int value;

						switch (_intProperties[i]._source)
						{
							case IntProperty.ePropertySource.Range:
								value = _intProperties[i]._valueRange.GetRandomValue();
								break;
							case IntProperty.ePropertySource.Constant:
							default:
								value = _intProperties[i]._value;
								break;
						}

						propertyBlock.SetInt(_intProperties[i]._name, value);
					}

					//Colors
					for (int i = 0; i < _colorProperties.Length; i++)
					{
						Color value;

						switch (_colorProperties[i]._source)
						{				
							case ColorProperty.ePropertySource.Gradient:
								value = _colorProperties[i]._valueGradient.Evaluate(Random.value);
								break;
							case ColorProperty.ePropertySource.Constant:
							default:
								value = _colorProperties[i]._value;
								break;
						}

						propertyBlock.SetColor(_colorProperties[i]._name, value);
					}

					//Vector4s
					for (int i = 0; i < _vector4Properties.Length; i++)
					{
						Vector4 value;

						switch (_vector4Properties[i]._source)
						{
							case Vector4Property.ePropertySource.Range:
								value.x = _vector4Properties[i]._xValueRange.GetRandomValue();
								value.y = _vector4Properties[i]._yValueRange.GetRandomValue();
								value.z = _vector4Properties[i]._zValueRange.GetRandomValue();
								value.w = _vector4Properties[i]._wValueRange.GetRandomValue();
								break;
							case Vector4Property.ePropertySource.Curves:
								value.x = _vector4Properties[i]._xValueCurve.Evaluate(Random.value);
								value.y = _vector4Properties[i]._yValueCurve.Evaluate(Random.value);
								value.z = _vector4Properties[i]._zValueCurve.Evaluate(Random.value);
								value.w = _vector4Properties[i]._wValueCurve.Evaluate(Random.value);
								break;
							case Vector4Property.ePropertySource.Constant:
							default:
								value = _vector4Properties[i]._value;
								break;
						}

						propertyBlock.SetVector(_vector4Properties[i]._name, value);
					}

					//Textures
					for (int i = 0; i < _textureProperties.Length; i++)
					{
						Texture value;

						switch (_textureProperties[i]._source)
						{
							
							case TextureProperty.ePropertySource.Array:
								if (_textureProperties[i]._valueArray.Length > 0)
									value = _textureProperties[i]._valueArray[Random.Range(0, _textureProperties[i]._valueArray.Length)];
								else
									value = null;
								break;
							case TextureProperty.ePropertySource.Constant:
							default:
								value = _textureProperties[i]._value;
								break;
						}

						propertyBlock.SetTexture(_textureProperties[i]._name, value);
					}

					_renderer.SetPropertyBlock(propertyBlock);
				}
			}
			#endregion
		}
	}
}