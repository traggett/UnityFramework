using UnityEngine;
using System;

namespace Framework
{
	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public struct TimelineStateRefProperty
		{
			[SerializeField]
			private TextAsset _file;
			[SerializeField]
			private int _timelineId;

			public TimelineStateRefProperty(TextAsset file=null, int TimelineId=-1)
			{
				_file = file;
				_timelineId = TimelineId;
			}

			public TimelineState LoadTimelineState(GameObject sourceObject = null)
			{
				if (_file != null)
				{
					TimelineStateMachine stateMachine = TimelineStateMachine.FromTextAsset(_file, sourceObject);
					return stateMachine.GetTimelineState(_timelineId);
				}

				return null;
			}

			public bool IsValid()
			{
				return _file != null;
			}
			
			public TextAsset GetFile()
			{
				return _file;
			}
		}
	}
}