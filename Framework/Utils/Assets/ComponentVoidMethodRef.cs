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
			[SerializeField]
			private ComponentMethodRef<object> _methodRef;
		
			public void RunMethod()
			{
				_methodRef.RunMethod();
			}
#if UNITY_EDITOR
			public ComponentMethodRef<object> GetMethodRef()
			{
				return _methodRef;
			}
#endif
		}
	}
}