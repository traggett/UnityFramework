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

			public interface IProperty
			{
				string GetPropertyId();
			}

			[Serializable]
			public class FloatProperty : IProperty
			{
				public enum ePropertySource : short
				{
					Constant,
					RandomRange,
					RandomCurve,
				}

				public string _name = "_Value";
				public ePropertySource _source = ePropertySource.Constant;
				public float _value = 0.0f;
				public FloatRange _valueRange = new FloatRange(0f, 1f);
				public AnimationCurve _valueCurve = new AnimationCurve(new Keyframe(0f,0f), new Keyframe(1f,1f));

				public string GetPropertyId() { return _name; }
			}

			[Serializable]
			public class ColorProperty : IProperty
			{
				public enum ePropertySource : short
				{
					Constant,
					RandomFromGradient,
				}

				public string _name = "_Color";
				public ePropertySource _source = ePropertySource.Constant;
				public Color _value = Color.white;
				public Gradient _valueGradient = new Gradient();

				public string GetPropertyId() { return _name; }
			}

			[Serializable]
			public class Vector4Property : IProperty
			{
				public enum ePropertySource : short
				{
					Constant,
					RandomRange,
					RandomCurve,
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

				public string GetPropertyId() { return _name; }
			}

			[Serializable]
			public class TextureProperty : IProperty
			{
				public enum ePropertySource : short
				{
					Constant,
					RandomArray,
				}

				public string _name = "_Value";
				public ePropertySource _source = ePropertySource.Constant;
				public Texture _value = null;
				public Texture[] _valueArray = new Texture[0];

				public string GetPropertyId() { return _name; }
			}
			
			public FloatProperty[] _floatProperties;
			public ColorProperty[] _colorProperties;
			public Vector4Property[] _vector4Properties;
			public TextureProperty[] _textureProperties;
			#endregion

			#region MonoBehaviour
			private void OnEnable()
			{
				UpdateProperties();
			}
			#endregion

			#region Public Interface
			public MaterialPropertyBlock GetPropertyBlock()
			{
				MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

				if (_renderer != null)
				{
					//Floats
					for (int i = 0; i < _floatProperties.Length; i++)
					{
						float value;

						switch (_floatProperties[i]._source)
						{
							case FloatProperty.ePropertySource.RandomRange:
								value = _floatProperties[i]._valueRange.GetRandomValue();
								break;
							case FloatProperty.ePropertySource.RandomCurve:
								value = _floatProperties[i]._valueCurve.Evaluate(Random.value);
								break;
							case FloatProperty.ePropertySource.Constant:
							default:
								value = _floatProperties[i]._value;
								break;

						}

						propertyBlock.SetFloat(_floatProperties[i].GetPropertyId(), value);
					}

					//Colors
					for (int i = 0; i < _colorProperties.Length; i++)
					{
						Color value;

						switch (_colorProperties[i]._source)
						{
							case ColorProperty.ePropertySource.RandomFromGradient:
								value = _colorProperties[i]._valueGradient.Evaluate(Random.value);
								break;
							case ColorProperty.ePropertySource.Constant:
							default:
								value = _colorProperties[i]._value;
								break;
						}

						propertyBlock.SetColor(_colorProperties[i].GetPropertyId(), value);
					}

					//Vector4s
					for (int i = 0; i < _vector4Properties.Length; i++)
					{
						Vector4 value;

						switch (_vector4Properties[i]._source)
						{
							case Vector4Property.ePropertySource.RandomRange:
								value.x = _vector4Properties[i]._xValueRange.GetRandomValue();
								value.y = _vector4Properties[i]._yValueRange.GetRandomValue();
								value.z = _vector4Properties[i]._zValueRange.GetRandomValue();
								value.w = _vector4Properties[i]._wValueRange.GetRandomValue();
								break;
							case Vector4Property.ePropertySource.RandomCurve:
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

						propertyBlock.SetVector(_vector4Properties[i].GetPropertyId(), value);
					}

					//Textures
					for (int i = 0; i < _textureProperties.Length; i++)
					{
						Texture value;

						switch (_textureProperties[i]._source)
						{

							case TextureProperty.ePropertySource.RandomArray:
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

						propertyBlock.SetTexture(_textureProperties[i].GetPropertyId(), value);
					}
				}

				return propertyBlock;
			}

			public void UpdateProperties()
			{
				if (_renderer != null)
				{
					MaterialPropertyBlock propertyBlock = GetPropertyBlock();
					_renderer.SetPropertyBlock(propertyBlock);
				}
			}
			#endregion
		}
	}
}