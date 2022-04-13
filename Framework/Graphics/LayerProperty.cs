using System;
using UnityEngine;
namespace Framework
{
	namespace Graphics
	{
		[Serializable]
		public struct LayerProperty
		{
			#region Serialized Data
			[SerializeField]
			private int _layer;	
			#endregion

			public LayerProperty(int layer = -1)
			{
				_layer = layer;
			}

			public static implicit operator int(LayerProperty property)
			{
				return property._layer;
			}
			
			public static implicit operator LayerProperty(int value)
			{
				return new LayerProperty(value);
			}

			public int GetLayerMask()
			{
				return LayerMask.GetMask(LayerMask.LayerToName(_layer));
			}
		}
	}
}