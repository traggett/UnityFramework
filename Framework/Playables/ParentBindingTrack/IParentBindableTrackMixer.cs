namespace Framework
{
	namespace Playables
	{
		public interface IParentBindableTrackMixer
		{
			void SetParentBinding(object playerData);
			void ClearParentBinding();
		}
	}
}