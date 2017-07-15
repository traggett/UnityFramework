using System;
using System.Collections;

using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public struct CoroutineRef
		{
			public ComponentMethodRef<IEnumerator> _methodRef;

			public IEnumerator RunCoroutine()
			{
				MonoBehaviour component = _methodRef._object.GetComponent() as MonoBehaviour;

				if (component != null)
				{
					yield return component.StartCoroutine(_methodRef.RunMethod());
				}

				yield break;
			}
		}
	}
}