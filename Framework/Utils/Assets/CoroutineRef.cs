using System;
using System.Collections;

using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		[Serializable]
		public sealed class CoroutineRef : ComponentMethodRef<IEnumerator>
		{
			public IEnumerator RunCoroutine()
			{
				MonoBehaviour component = _object.GetComponent() as MonoBehaviour;

				if (component != null)
				{
					yield return component.StartCoroutine(RunMethod());
				}

				yield break;
			}
		}
	}
}