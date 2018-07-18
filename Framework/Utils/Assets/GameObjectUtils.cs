using Framework.Maths;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace Utils
	{
        public static class GameObjectUtils
        {
            public static bool _isShuttingDown = false;

            public static GameObject CreatePrefab(string resourceName)
            {
                GameObject prefabResource = Resources.Load(resourceName) as GameObject;
                Object.DontDestroyOnLoad(prefabResource);
                /*
				if (prefabResource == null)
				{
					throw new System.InvalidOperationException("Unable to load resource: " + resourceName);
				}
				else
				{
					throw new System.InvalidOperationException("Loaded resource: " + resourceName);
				}*/

                GameObject prefab = SafeInstantiate(prefabResource, Vector3.zero, Quaternion.identity) as GameObject;
                return prefab;
            }

            public static T SafeInstantiate<T>(GameObject original, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
            {
                GameObject obj = SafeInstantiate(original, position, rotation);

                if (obj != null)
                {
                    return obj.GetComponent<T>();
                }

                return null;
            }

			public static T SafeInstantiate<T>(GameObject original, Transform parent = null) where T : Component
			{
				GameObject obj = SafeInstantiate(original, parent);

				if (obj != null)
				{
					return obj.GetComponent<T>();
				}

				return null;
			}

            public static GameObject SafeInstantiate(GameObject original, Vector3 position, Quaternion rotation, Transform parent = null)
            {
                if (_isShuttingDown)
                    return null;
                else
                    return GameObject.Instantiate(original, position, rotation) as GameObject;
            }

			public static GameObject SafeInstantiate(GameObject original, Transform parent = null)
			{
				if (_isShuttingDown)
					return null;
				else
					return GameObject.Instantiate(original, parent) as GameObject;
			}

			public static void DeleteChildren(Transform transform, bool immediate = false)
            {
                for (int i = transform.childCount - 1; i >= 0; --i)
                {
                    GameObject child = transform.GetChild(i).gameObject;

                    if (immediate)
                        GameObject.DestroyImmediate(child);
                    else
                        GameObject.Destroy(child);
                }
            }

            public static void ResetTransform(Transform transform)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }

            public static Matrix4x4 GetTransformMatrix(Transform transform)
            {
                return Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            }

            public static Matrix4x4 GetLocalTransformMatrix(Transform transform)
            {
                return Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
            }

            public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
            {
                transform.position = MathUtils.GetTranslationFromMatrix(ref matrix);
                transform.rotation = MathUtils.GetRotationFromMatrix(ref matrix);
                transform.localScale = MathUtils.GetScaleFromMatrix(ref matrix);
            }

            public static void CopyTransform(Transform fromTransform, Transform toTransform, Space space = Space.World)
            {
                switch (space)
                {
                    case Space.Self:
                        {
                            toTransform.localPosition = fromTransform.localPosition;
                            toTransform.localRotation = fromTransform.localRotation;
                            toTransform.localScale = fromTransform.localScale;
                        }
                        break;
                    case Space.World:
                        {
                            toTransform.position = fromTransform.position;
                            toTransform.rotation = fromTransform.rotation;
                            toTransform.localScale = fromTransform.localScale;
                        }
                        break;
                }
            }

            public static void SetLayerRecursively(GameObject obj, int newLayer)
            {
                if (null == obj)
                {
                    return;
                }

                obj.layer = newLayer;

                foreach (Transform child in obj.transform)
                {
                    if (null == child)
                    {
                        continue;
                    }
                    SetLayerRecursively(child.gameObject, newLayer);
                }
            }

            public static bool IsChildOf(Transform obj, Transform parent)
            {
                while (obj.parent != null)
                {
                    if (obj.parent == parent)
                    {
                        return true;
                    }

                    obj = obj.parent;
                }

                return false;
            }

            public static string GetChildFullName(GameObject child, GameObject parent)
            {
                string name = child.name;

                if (child.transform != parent.transform)
                {
                    Transform trans = child.transform.parent;
                    while (trans != null && trans != parent.transform)
                    {
                        name = trans.gameObject.name + "/" + name;
                        trans = trans.parent;
                    }
                }

                return name;
            }

            public static T GetComponent<T>(GameObject gameObject) where T : class
            {
                Component[] components = gameObject.GetComponents(typeof(Component));

                foreach (Component component in components)
                {
                    T typedComponent = component as T;

                    if (typedComponent != null)
                        return typedComponent;
                }

                return null;
            }

            public static T GetComponentInParents<T>(GameObject gameObject, bool includeInactive = false) where T : Component
            {
                while (gameObject != null)
                {
                    Component[] components = gameObject.GetComponentsInParent(typeof(T), includeInactive);

                    if (components != null && components.Length > 0)
                        return (T)components[0];

                    gameObject = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject : null;
                }

                return null;
            }

            public static Transform[] GetChildTransforms(Transform transform)
            {
                List<Transform> children = new List<Transform>();
                AddChildTransforms(transform, ref children);
                return children.ToArray();
            }

            private static void AddChildTransforms(Transform transform, ref List<Transform> transforms)
            {
                transforms.Add(transform);

                foreach (Transform child in transform)
                {
                    AddChildTransforms(child, ref transforms);
                }
            }
        }

	}
}