using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		//Implement this on a class if you want to customize how an object is shown when being edited as an JSON object.
		public interface IJSONCustomEditable
		{
#if UNITY_EDITOR
			bool RenderObjectProperties(GUIContent label);
#endif
		}
	}
}