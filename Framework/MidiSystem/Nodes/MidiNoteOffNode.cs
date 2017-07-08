using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	using NodeGraphSystem;

	namespace MidiSystem
	{
		[NodeCategory("Midi")]
		[Serializable]
		public class MidiNoteOffNode : Node, IValueSource<bool>
		{
			#region Public Data
			public NodeInputField<int> _track = 0;
			public NodeInputField<int> _channel = 16;
			public NodeInputField<int> _note = 0;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return MidiNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<bool>
			public bool GetValue()
			{
				return MidiSequencer.GetNoteOff(_track, _channel, _note);
			}
			#endregion
		}
	}
}