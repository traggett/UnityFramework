using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Utils;

	namespace ValueSourceSystem
	{
		[Serializable]
		public class MaterialValueSource : ValueSource<MaterialRefProperty>
		{
		}
	}
}