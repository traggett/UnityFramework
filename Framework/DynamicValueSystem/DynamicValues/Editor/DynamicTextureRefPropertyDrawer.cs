using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicTextureRef))]
			public class DynamicTextureRefPropertyDrawer : DynamicValuePropertyDrawer<Texture>
			{
			}
		}
	}
}