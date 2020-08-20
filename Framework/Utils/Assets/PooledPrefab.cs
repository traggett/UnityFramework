using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public class PooledPrefab : MonoBehaviour
		{
			public PrefabInstancePool _parentPool;
			public bool _isFree;

			public void Destroy()
			{
				_parentPool.Destroy(this.gameObject);
			}
		}
	}
}