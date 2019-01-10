namespace Framework
{
	namespace Playables
	{
		public interface IPlayableBehaviourGizmoDrawer
		{
#if UNITY_EDITOR
			void DrawGizmos();
#endif
		}
	}
}