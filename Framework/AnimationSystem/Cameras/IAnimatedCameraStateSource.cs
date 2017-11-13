namespace Framework
{
	namespace AnimationSystem
	{
		public interface IAnimatedCameraStateSource
		{
			AnimatedCameraState GetState();		
#if UNITY_EDITOR
			void SetState(AnimatedCameraState state);
			string GetName();
#endif
		}
	}
}