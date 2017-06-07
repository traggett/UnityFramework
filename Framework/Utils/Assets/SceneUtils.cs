using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
	namespace Utils
	{
		public static class SceneUtils
		{
			public static T FindInScene<T>(Scene scene) where T : Component
			{
				T component = null;
				
				foreach (GameObject rootObject in scene.GetRootGameObjects())
				{
					component = rootObject.GetComponentInChildren<T>();

					if (component != null)
					{
						break;
					}
				}

				return component;
			}

			public static GameObject FindInScene(Scene scene, string name)
			{
				GameObject gameObject = null;

				foreach (GameObject rootObject in scene.GetRootGameObjects())
				{
					Transform transform = rootObject.transform.Find(name);

					if (transform != null)
					{
						gameObject = transform.gameObject;
						break;
					}
				}

				return gameObject;
			}

			public static T FindInScene<T>(Scene scene, string name) where T : Component
			{
				T component = null;

				foreach (GameObject rootObject in scene.GetRootGameObjects())
				{
					Transform transform = rootObject.transform.Find(name);

					if (transform != null)
					{
						component = transform.gameObject.GetComponent<T>();

						if (component != null)
						{
							break;
						}
					}
				}

				return component;
			}
		}
	}
}