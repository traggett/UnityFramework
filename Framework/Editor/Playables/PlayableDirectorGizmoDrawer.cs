using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;

namespace Framework
{
	namespace Playables
	{
		public static class PlayableDirectorGizmoDrawer
		{
			[DrawGizmo(GizmoType.Selected | GizmoType.Active)]
			public static void DrawPlayableDirectorSelected(PlayableDirector scr, GizmoType gizmoType)
			{
				DebugGizmos(scr);
			}

			[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Active)]
			public static void DrawPlayableDirectorNotSelected(PlayableDirector scr, GizmoType gizmoType)
			{
				if (TimelineEditor.inspectedDirector != null && TimelineEditor.inspectedDirector == scr)
					DebugGizmos(scr);
			}

			private static void DebugGizmos(PlayableDirector playableDirector)
			{
				if (playableDirector != null && playableDirector.playableGraph.IsValid())
				{
					List<IPlayableBehaviourGizmoDrawer> debugDrawers = TimelineUtils.GetPlayableBehaviours<IPlayableBehaviourGizmoDrawer>(playableDirector.playableGraph);

					foreach (IPlayableBehaviourGizmoDrawer debugDrawer in debugDrawers)
					{
						debugDrawer.DrawGizmos();
					}
				}
			}
		}
	}
}