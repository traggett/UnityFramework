using System;
using System.IO;
using System.Runtime.InteropServices;

using UnityEngine;

using MidiJack;
using MidiSharp;
using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Voice;
using MidiSharp.Events.Voice.Note;

namespace Framework
{
	namespace MidiSystem
	{
		public static class MidiSequencer
		{
			#region Private Data
			private static readonly int kMaxTracks = 4;
			private static MidiTrackState[] _midiTracks;

			// Last update frame number
			private static int _lastFrame;

			private static MidiSequence _playingSequence;
			private static readonly int kDefaultTempo = 500000;
			
			private static long[] _trackTicker = new long[kMaxTracks];
			private static int[] _trackEventIndex = new int[kMaxTracks];
			private static double[] _trackPulsesPerSecond = new double[kMaxTracks];
			#endregion

			#region Internal Interface
			internal static void Init()
			{
				_midiTracks = new MidiTrackState[kMaxTracks];
				for (int i = 0; i < kMaxTracks; i++)
					_midiTracks[i] = new MidiTrackState();

				//PlayMidiSequence(LoadMidiFile("C:/Projects/MissingInActon/unity/Assets/Game/Resources/Test.midi"));
			}

			internal static void Update()
			{
				// Update the note state array.
				UpdateNoteStates();

				//Update playing midi
				UpdatePlayingSequence(Time.deltaTime);

				UpdateLiveInput();
			}
			#endregion

			#region Public Interface

			#region State Querires
			public static bool GetNoteOn(int track, int channel, int key)
			{
				return _midiTracks[track]._channelStates[channel]._noteArray[key] > 1.0f;
			}

			public static bool GetNoteOff(int track, int channel, int key)
			{
				return _midiTracks[track]._channelStates[channel]._noteArray[key] < 0.0f;
			}

			public static float GetNoteVelocity(int track, int channel, int key)
			{
				return _midiTracks[track]._channelStates[channel]._noteArray[key];
			}

			public static float GetCCLevel(int track, int channel, int id)
			{
				return _midiTracks[track]._channelStates[channel]._ccArray[id];
			}
			#endregion

			#region Midi File Playback
			public static MidiSequence LoadMidiFile(string fileName)
			{
				MidiSequence sequence = null;

				try
				{
					using (Stream inputStream = File.OpenRead(fileName))
					{
						sequence = MidiSequence.Open(inputStream);
					}
				}
				catch (Exception exc)
				{
					Debug.LogError("Error: " + exc.Message);
				}

				return sequence;
			}

			public static void PlayMidiSequence(MidiSequence sequence)
			{
				if (sequence != null)
				{
					_playingSequence = sequence;
					int trackId = 0;
					foreach (MidiTrack track in _playingSequence.Tracks)
					{
						_trackTicker[trackId] = 0;
						_trackEventIndex[trackId] = 0;
						SetTrackTempo(trackId, kDefaultTempo);
						trackId++;
					}
				}
			}
			#endregion

			#endregion

			#region Native Plugin Interface
			[DllImport("MidiJackPlugin", EntryPoint = "MidiJackDequeueIncomingData")]
			public static extern ulong DequeueIncomingData();
			#endregion

			#region Private Methods
			private static bool ShouldTrackBeUpdatedByInputSource(ref MidiTrackState track, uint source)
			{
				return track._source == MidiTrackState.eSource.AnyLiveSource || (track._source == MidiTrackState.eSource.LiveSource && track._sourceAddress == source);
			}
			
			private static void OnNoteOn(int track, int channelNumber, int note, byte velocity)
			{
				float floatVelocity = 1.0f / 127 * velocity + 1;
				_midiTracks[track]._channelStates[channelNumber]._noteArray[note] = floatVelocity + 1.0f;
				_midiTracks[track]._channelStates[MidiTrackState.kNumberOfChannels - 1]._noteArray[note] = floatVelocity + 1.0f;
			}

			private static void OnNoteOff(int track, int channelNumber, int note)
			{
				_midiTracks[track]._channelStates[channelNumber]._noteArray[note] = -1.0f;
				_midiTracks[track]._channelStates[MidiTrackState.kNumberOfChannels - 1]._noteArray[note] = -1.0f;
			}

			private static void OnCC(int track, int channelNumber, int id, byte level)
			{
				float floatLevel = level / 127f;
				_midiTracks[track]._channelStates[channelNumber]._ccArray[id] = floatLevel;
				_midiTracks[track]._channelStates[MidiTrackState.kNumberOfChannels - 1]._ccArray[id] = floatLevel;
			}

			private static void UpdateNoteStates()
			{
				// Note state array
				// X<0    : Released on this frame
				// X=0    : Off
				// 0<X<=1 : On (X represents velocity)
				// 1<X<=2 : Triggered on this frame
				//          (X-1 represents velocity)

				for (int i = 0; i < kMaxTracks; i++)
				{
					for (int j = 0; j < MidiTrackState.kNumberOfChannels; j++)
					{
						for (int k = 0; k < MidiTrackState.kNumberOfNotes; k++)
						{
							if (_midiTracks[i]._channelStates[j]._noteArray[k] < 0.0f)
								_midiTracks[i]._channelStates[j]._noteArray[k] = 0.0f;
							else if (_midiTracks[i]._channelStates[j]._noteArray[k] > 1.0f)
								_midiTracks[i]._channelStates[j]._noteArray[k] = 1.0f - _midiTracks[i]._channelStates[i]._noteArray[k];
						}
					}
				}
			}

			private static void UpdateLiveInput()
			{
				// Process the message queue.
				while (true)
				{
					// Pop from the queue.
					ulong data = DequeueIncomingData();
					if (data == 0) break;

					// Parse the message.
					MidiMessage message = new MidiMessage(data);
					
					// Split the first byte.
					int statusCode = message.status >> 4;
					int channelNumber = message.status & 0xf;

					// Note on message?
					if (statusCode == 9)
					{
						for (int i = 0; i < kMaxTracks; i++)
						{
							if (ShouldTrackBeUpdatedByInputSource(ref _midiTracks[i], message.source))
							{
								OnNoteOn(i, channelNumber, message.data1, message.data2);
							}
						}
					}

					// Note off message?
					if (statusCode == 8 || (statusCode == 9 && message.data2 == 0))
					{
						for (int i = 0; i < kMaxTracks; i++)
						{
							if (ShouldTrackBeUpdatedByInputSource(ref _midiTracks[i], message.source))
							{
								OnNoteOff(i, channelNumber, message.data1);
							}
						}
					}

					// CC message?
					if (statusCode == 0xb)
					{
						for (int i = 0; i < kMaxTracks; i++)
						{
							if (ShouldTrackBeUpdatedByInputSource(ref _midiTracks[i], message.source))
							{
								OnCC(i, channelNumber, message.data1, message.data2);
							}
						}
					}

				}
			}

			#region Midi File Playback
			private static void UpdatePlayingSequence(double deltaTime)
			{
				if (_playingSequence != null)
				{
					int trackId = 0;
					foreach (MidiTrack track in _playingSequence.Tracks)
					{
						long pulsesThisFrame = (long)Math.Round(deltaTime * _trackPulsesPerSecond[trackId]);
						_trackTicker[trackId] += pulsesThisFrame;

						UpdateCurrentTrackEvent(track, trackId);

						trackId++;
					}
				}
			}

			private static void SetTrackTempo(int track, int tempo)
			{
				//BPM = 60,000,000/tempo so BPSec is 1,000,000/temp
				double beatsPerSecond = 1000000d / tempo;
				//Pulses Per Quarter Note
				double ppqn = _playingSequence.TicksPerBeatOrFrame;
				//Pulse Length = 60/(BPM * PPQN) so Pulses per second is (beatsPerSecond * PPQN)
				_trackPulsesPerSecond[track] = (beatsPerSecond * ppqn);
			}

			private static void UpdateCurrentTrackEvent(MidiTrack track, int trackId)
			{
				if (_trackEventIndex[trackId] < track.Events.Count)
				{
					MidiEvent evnt = track.Events[_trackEventIndex[trackId]];

					if (_trackTicker[trackId] >= evnt.DeltaTime)
					{
						_trackEventIndex[trackId]++;
						_trackTicker[trackId] -= evnt.DeltaTime;

						TriggerEvent(evnt, trackId);

						UpdateCurrentTrackEvent(track, trackId);
					}
				}
			}

			private static void TriggerEvent(MidiEvent evnt, int track)
			{
				if (evnt is TempoMetaMidiEvent)
				{
					TempoMetaMidiEvent tempoEvent = (TempoMetaMidiEvent)evnt;
					SetTrackTempo(track, tempoEvent.Value);
				}
				else if (evnt is EndOfTrackMetaMidiEvent)
				{
					//Loop??
					_trackEventIndex[track] = 0;
				}
				else if (evnt is ControllerVoiceMidiEvent)
				{
					ControllerVoiceMidiEvent ccEvent = (ControllerVoiceMidiEvent)evnt;
					OnCC(track, ccEvent.Channel, ccEvent.Number, ccEvent.Value);
				}
				else if (evnt is OnNoteVoiceMidiEvent)
				{
					OnNoteVoiceMidiEvent voiceEvent = (OnNoteVoiceMidiEvent)evnt;
					OnNoteOn(track, voiceEvent.Channel, voiceEvent.Note, voiceEvent.Velocity);
				}
				else if (evnt is OffNoteVoiceMidiEvent)
				{
					OffNoteVoiceMidiEvent voiceEvent = (OffNoteVoiceMidiEvent)evnt;
					OnNoteOff(track, voiceEvent.Channel, voiceEvent.Note);
				}
			}
			#endregion

			#endregion
		}
	}
}