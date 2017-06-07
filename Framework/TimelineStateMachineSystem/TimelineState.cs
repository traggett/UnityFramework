using UnityEngine;
using System;

namespace Framework
{
	using TimelineSystem;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public class TimelineState
		{
			#region Public Data			
			public int _stateId = -1;
			public Timeline _timeline = new Timeline();

			//Editor properties
			public Vector2 _editorPosition = Vector2.zero;
			public bool _editorAutoDescription = true;
			public string _editorDescription = string.Empty;
			public bool _editorAutoColor = true;
			public Color _editorColor = Color.gray;
#if DEBUG
			[NonSerialized]
			public TimelineStateMachine _debugParentStateMachine;
#endif
			#endregion

#if UNITY_EDITOR
			public string GetDescription()
			{
				string label = null;

				if (_editorAutoDescription)
				{
					foreach (Event evnt in _timeline._events)
					{
						string eventDescription = evnt.GetEditorDescription();

						if (!string.IsNullOrEmpty(eventDescription))
						{
							if (label == null)
							{
								label = eventDescription;
							}
							else
							{
								label += "\n" + eventDescription;
							}
						}
					}
				}

				if (string.IsNullOrEmpty(label))
				{
					label = _editorDescription;
				}

				return label;
			}
#endif


		}
	}
}