using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public class PooledPrefab : MonoBehaviour
		{
			private PrefabInstancePool _parentPool;

			public bool _isFree;

			public void SetParentPool(PrefabInstancePool pool)
			{
				_parentPool = pool;
			}

			public void Destroy()
			{
				_parentPool.Destroy(this.gameObject);
			}

			public static void DestroyChildPrefabs(Transform parent)
			{
				for (int i = 0; i < parent.childCount;)
				{
					PooledPrefab prefab = parent.GetChild(i).GetComponent<PooledPrefab>();
					if (prefab == null || !prefab._parentPool.Destroy(prefab.gameObject))
					{
						i++;
					}
				}
			}
		}
	}
}