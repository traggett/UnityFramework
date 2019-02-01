using UnityEditor;

namespace Framework
{
	namespace Playables
	{
		namespace Editor
		{
			[CustomEditor(typeof(PrefabInstanceTrack))]
			[CanEditMultipleObjects]
			public class PrefabInstanceTrackInspector : ParentBindingTrackInspector
			{

			}
		}
	}
}