using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Paths
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(ExposedPathPosition))]
			public class ExposedPathPositionPropertyDrawer : PathPositionPropertyDrawer
			{
				protected override Path GetPath(SerializedProperty pathProp)
				{
					return pathProp.exposedReferenceValue as Path;
				}

				protected override PathNode GetPathNode(SerializedProperty nodeProp)
				{
					return nodeProp.exposedReferenceValue as PathNode;
				}

				protected override void SetPathNode(SerializedProperty nodeProp, PathNode node)
				{
					nodeProp.exposedReferenceValue = node;
				}

				protected IExposedPropertyTable GetExposedPropertyTable(SerializedProperty property)
				{
					Object context = property.serializedObject.context;
					return context as IExposedPropertyTable;
				}
			}
		}
	}
}