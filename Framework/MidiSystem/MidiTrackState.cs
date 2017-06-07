namespace Framework
{
	namespace MidiSystem
	{
		public sealed class MidiTrackState
		{
			public static readonly int kNumberOfChannels = 17; //(16 Channels + 1 for all combined)
			public static readonly int kNumberOfNotes = 128;
			public static readonly int kNumberOfCCs = 128;

			public class ChannelState
			{
				public float[] _noteArray;
				public float[] _ccArray;

				public ChannelState()
				{
					_noteArray = new float[kNumberOfNotes];
					_ccArray = new float[kNumberOfCCs];
				}
			}

			public ChannelState[] _channelStates;

			public enum eSource
			{
				None = 0,
				LiveSource,
				AnyLiveSource,
			}
			public eSource _source;
			public uint _sourceAddress;

			public MidiTrackState()
			{
				 _channelStates = new ChannelState[kNumberOfChannels];
				for (int i = 0; i < kNumberOfChannels; i++)
					_channelStates[i] = new ChannelState();

				_source = eSource.AnyLiveSource;
			}

			
		}
	}
}