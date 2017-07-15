using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		//This should only be used by Classes, not structs!
		public interface ICustomEditorInspector
		{
#if UNITY_EDITOR
			//This should only be called by SerializationEditorGUILayout - that way can deal with rendering properties for a null object
			bool RenderObjectProperties(GUIContent label);
#endif
		}
	}
}