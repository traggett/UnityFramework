using UnityEngine;
using System;

namespace Framework
{
	namespace Utils
	{
		public class PrefabRoot : MonoBehaviour
		{
			public static GameObject GetPrefabRoot(GameObject gameObject)
			{
				if (gameObject != null)
				{
					PrefabRoot root = gameObject.GetComponent<PrefabRoot>();

					if (root == null)
					{
						root = gameObject.GetComponentInParent<PrefabRoot>();
					}

					if (root != null)
					{
						return root.gameObject;
					}
					else
					{
						throw new Exception("No PrefabRoot root component found in prefab - did you forget to add one?");
					}
				}	

				return null;
			}
		}
	}
}
