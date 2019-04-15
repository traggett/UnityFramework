using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public static class AnimationUtils
		{
			public static void TriggerAnimationEvent(AnimationEvent animationEvent, GameObject gameObject)
			{
				//Try parameterless first
				try
				{
					gameObject.SendMessage(animationEvent.functionName, animationEvent.messageOptions);
				}
				catch
				{
					//Then with string parameter
					try
					{
						gameObject.SendMessage(animationEvent.functionName, animationEvent.stringParameter, animationEvent.messageOptions);
					}
					catch
					{
						//Then with float parameter
						try
						{
							gameObject.SendMessage(animationEvent.functionName, animationEvent.floatParameter, animationEvent.messageOptions);
						}
						catch
						{
							//Then with int parameter
							try
							{
								gameObject.SendMessage(animationEvent.functionName, animationEvent.intParameter, animationEvent.messageOptions);
							}
							catch
							{
								//Then with object parameter
								gameObject.SendMessage(animationEvent.functionName, animationEvent.objectReferenceParameter, animationEvent.messageOptions);
							}
						}
					}
				}
			}
		}
	}
}