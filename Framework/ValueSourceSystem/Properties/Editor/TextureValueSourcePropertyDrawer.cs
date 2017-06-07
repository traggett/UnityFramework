using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(TextureValueSource))]
			public class TextureValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Texture>
			{
			}
		}
	}
}