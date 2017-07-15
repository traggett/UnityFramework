using System;
using System.Reflection;

using UnityEngine;

namespace Framework
{

	namespace Utils
	{
		[Serializable]
		public struct ComponentMethodRef<T>
		{
			#region Public Data
			public ComponentRef<Component> _object;
			public string _methodName;
			#endregion

#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorFoldout;
#endif

			public static implicit operator string(ComponentMethodRef<T> property)
			{
				return property._object + "." + property._methodName + "()";
			}

			public T RunMethod()
			{
				T returnObj = default(T);

				Component component = _object.GetComponent();

				if (component != null)
				{
					MethodInfo method = component.GetType().GetMethod(_methodName);

					if (method != null)
					{
						returnObj = (T)method.Invoke(component, null);
					}
				}

				return returnObj;
			}
		}
	}
}