using System;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public class MaterialPerInstanceProperties : MonoBehaviour
		{
			#region Public Data
			public Renderer _renderer;

			[Serializable]
			public struct Property
			{
				public enum eType
				{
					Float,
					Int,
					Color,
					Vector,
					Texture,
				}
				public eType _type;
				public string _name;

				public float _floatValue;
				public int _intValue;
				public Vector4 _vectorValue;
				public Texture _textureValue;

				public AnimationCurve _valueCurveX;
				public AnimationCurve _valueCurveY;
				public AnimationCurve _valueCurveZ;
				public AnimationCurve _valueCurveW;
				public Gradient _gradientCurve;

				public float GetFloatValue()
				{
					return _floatValue;
				}

				public int GetIntValue()
				{
					return _intValue;
				}

				public Color GetColorValue()
				{
					return _vectorValue;
				}

				public Vector4 GetVectorValue()
				{
					return _vectorValue;
				}

				public Texture GetTextureValue()
				{
					return _textureValue;
				}
			}
			
			public Property[] _properties;
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

					for (int i=0; i<_properties.Length; i++)
					{
						switch (_properties[i]._type)
						{
							case Property.eType.Float:
								propertyBlock.SetFloat(_properties[i]._name, _properties[i].GetFloatValue());
								break;
							case Property.eType.Int:
								propertyBlock.SetInt(_properties[i]._name, _properties[i].GetIntValue());
								break;
							case Property.eType.Color:
								propertyBlock.SetColor(_properties[i]._name, _properties[i].GetColorValue());
								break;
							case Property.eType.Vector:
								propertyBlock.SetVector(_properties[i]._name, _properties[i].GetVectorValue());
								break;
							case Property.eType.Texture:
								propertyBlock.SetTexture(_properties[i]._name, _properties[i].GetTextureValue());
								break;
						}
					}

					GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
				}
			}
			#endregion
		}
	}
}