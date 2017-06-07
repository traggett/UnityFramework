

namespace Engine
{
	namespace JSON
	{
		//Implement this on a class if you want to do any custom conversion to and from JSON. 
		public interface IJSONConversionCallbackReceiver
		{
			void OnConvertToJSONElement(JSONElement node);
			void OnConvertFromJSONElement(JSONElement node);
		}
	}
}