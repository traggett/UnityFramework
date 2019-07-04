namespace Framework
{
	using Maths;

	namespace AnimationSystem
	{
		public interface IAnimator
		{
			#region Public Interface
			void Play(int channel, string animName, eWrapMode wrapMode = eWrapMode.Default, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine, float weight = 1.0f, bool queued = false);

			void Stop(int channel, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine);

			void StopAll();

			void SetAnimationTime(int channel, string animName, float time);

			void SetAnimationSpeed(int channel, string animName, float speed);

			void SetAnimationWeight(int channel, string animName, float weight);

			bool IsPlaying(int channel, string animName);

			float GetAnimationLength(string animName);

			float GetAnimationTime(int channel, string animName);

			float GetAnimationSpeed(int channel, string animName);

			float GetAnimationWeight(int channel, string animName);

			bool DoesAnimationExist(string animName);

#if UNITY_EDITOR
			string[] GetAnimationNames();
#endif
			#endregion
		}
	}
}