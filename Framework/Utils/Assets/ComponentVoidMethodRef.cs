using System;
using System.Reflection;

using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		//Cant have generic type of <void> so have to manually overrider return type in its own class
		[Serializable]
		public struct ComponentVoidMethodRef
		{
			#region Public Data
			public ComponentMethodRef<object> _methodRef;
			#endregion
		
			public void RunMethod()
			{
				Component component = _methodRef._object.GetComponent();

				if (component != null)
				{
					MethodInfo method = component.GetType().GetMethod(_methodRef._methodName);

					if (method != null)
					{
						method.Invoke(component, null);
					}
				}
			}
		}
	}
}