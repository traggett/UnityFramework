using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		public interface ICustomEditorInspector
		{
#if UNITY_EDITOR
			bool RenderObjectProperties(GUIContent label);
#endif
		}
	}
}