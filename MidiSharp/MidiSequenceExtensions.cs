//----------------------------------------------------------------------- 
// <copyright file="MidiSequenceExtensions.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Meta.Text;
using MidiSharp.Events.Voice;
using MidiSharp.Events.Voice.Note;
using System.Linq;

namespace MidiSharp
{
    /// <summary>Common manipulations of MidiSequences.</summary>
    public static class MidiSequenceExtensions
    {
        /// <summary>Transposes a MIDI sequence up/down the specified number of half-steps.</summary>
        /// <param name="sequence">The sequence to be transposed.</param>
        /// <param name="steps">The number of steps up(+) or down(-) to transpose the sequence.</param>
        public static void Transpose(this MidiSequence sequence, int steps)
        {
            // Transpose the sequence; do not transpose drum tracks
            Transpose(sequence, steps, false);
        }

        /// <summary>Transposes a MIDI sequence up/down the specified number of half-steps.</summary>
        /// <param name="sequence">The sequence to be transposed.</param>
        /// <param name="steps">The number of steps up(+) or down(-) to transpose the sequence.</param>
        /// <param name="includeDrums">Whether drum tracks should also be transposed.</param>
        /// <remarks>If the step value is too large or too small, notes may wrap.</remarks>
        public static void Transpose(this MidiSequence sequence, int steps, bool includeDrums)
        {
            Validate.NonNull("sequence", sequence);

            // Modify each track
            foreach (MidiTrack track in sequence) {
                // Modify each event
                foreach (MidiEvent ev in track.Events) {
                    // If the event is not a voice MIDI event but the channel is the
                    // drum channel and the user has chosen not to include drums in the
                    // transposition (which makes sense), skip this event.
                    NoteVoiceMidiEvent nvme = ev as NoteVoiceMidiEvent;
                    if (nvme == null ||
                        (!includeDrums && nvme.Channel == (byte)SpecialChannel.Percussion))
                        continue;

                    // If the event is a NoteOn, NoteOff, or Aftertouch, shift the note
                    // according to the supplied number of steps.
                    nvme.Note = (byte)((nvme.Note + steps) % 128);
                }
            }
        }

        /// <summary>Trims a MIDI file to a specified length.</summary>
        /// <param name="sequence">The sequence to be copied and trimmed.</param>
        /// <param name="totalTime">The requested time length of the new MIDI sequence.</param>
        /// <returns>A MIDI sequence with only those events that fell before the requested time limit.</returns>
        public static MidiSequence Trim(this MidiSequence sequence, long totalTime)
        {
            Validate.NonNull("sequence", sequence);
            Validate.InRange("totalTime", totalTime, 0, long.MaxValue);

            // Create a new sequence to mimic the old
            MidiSequence newSequence = new MidiSequence(sequence.Format, sequence.Division);

            // Copy each track up to the specified time limit
            foreach (MidiTrack track in sequence) {
                // Create a new track in the new sequence to match the old track in the old sequence
                MidiTrack newTrack = newSequence.Tracks.AddNewTrack();

                // Convert all times in the old track to deltas
                track.Events.ConvertDeltasToTotals();

                // Copy over all events that fell before the specified time
                for (int i = 0; i < track.Events.Count && track.Events[i].DeltaTime < totalTime; i++) {
                    newTrack.Events.Add(track.Events[i].DeepClone());
                }

                // Convert all times back (on both new and old tracks; the new one inherited the totals)
                track.Events.ConvertTotalsToDeltas();
                newTrack.Events.ConvertTotalsToDeltas();

                // If the new track lacks an end of track, add one
                if (!newTrack.HasEndOfTrack) {
                    newTrack.Events.Add(new EndOfTrackMetaMidiEvent(0));
                }
            }

            // Return the new sequence
            return newSequence;
        }

        /// <summary>Converts a MIDI sequence from its current format to the specified format.</summary>
        /// <param name="sequence">The sequence to be converted.</param>
        /// <param name="format">The format to which we want to convert the sequence.</param>
        /// <returns>The converted sequence.</returns>
        /// <remarks>
        /// This may or may not return the same sequence as passed in.
        /// Regardless, the reference passed in should not be used after this call as the old
        /// sequence could be unusable if a different reference was returned.
        /// </remarks>
        public static MidiSequence Convert(this MidiSequence sequence, Format format)
        {
            return Convert(sequence, format, FormatConversionOption.None);
        }

        /// <summary>Converts the MIDI sequence into a new one with the desired format.</summary>
        /// <param name="sequence">The sequence to be converted.</param>
        /// <param name="format">The format to which we want to convert the sequence.</param>
        /// <param name="options">Options used when doing the conversion.</param>
        /// <returns>The new, converted sequence.</returns>
        public static MidiSequence Convert(this MidiSequence sequence, Format format, FormatConversionOption options)
        {
            Validate.NonNull("sequence", sequence);
            Validate.InRange("format", (int)format, (int)Format.Zero, (int)Format.Two);

            if (sequence.Format == format) {
                // If the desired format is the same as the original, just return a copy.
                // No transformation is necessary.
                sequence = new MidiSequence(sequence);
            }
            else if (format != Format.Zero || sequence.Tracks.Count == 1) {
                // If the desired format is is not 0 or there's only one track, just copy the sequence with a different format number.
                // If it's not zero, then multiple tracks are acceptable, so no transformation is necessary.
                // Or if there's only one track, then there's no possible transformation to be done.
                var newSequence = new MidiSequence(format, sequence.Division);
                foreach (MidiTrack t in sequence) {
                    newSequence.Tracks.Add(new MidiTrack(t));
                }
                sequence = newSequence;
            }
            else {
                // Now the harder cases, converting to format 0.  We need to combine all tracks into 1,
                // as format 0 requires that there only be a single track with all of the events for the song.
                sequence = new MidiSequence(sequence);

                // Iterate through all events in all tracks and change deltaTimes to actual times.
                // We'll then be able to sort based on time and change them back to deltas later.
                foreach (MidiTrack track in sequence) {
                    track.Events.ConvertDeltasToTotals();
                }

                // Add all events to new track (except for end of track markers!)
                int trackNumber = 0;
                MidiTrack newTrack = new MidiTrack();
                foreach (MidiTrack track in sequence) {
                    foreach (MidiEvent midiEvent in track.Events) {
                        // If this event has a channel, and if we're storing tracks as channels, copy to it
                        if ((options & FormatConversionOption.CopyTrackToChannel) > 0 && trackNumber >= 0 && trackNumber <= 0xF) {
                            var vme = midiEvent as VoiceMidiEvent;
                            if (vme != null) {
                                vme.Channel = (byte)trackNumber;
                            }
                        }

                        // Add all events, except for end of track markers (we'll add our own)
                        if (!(midiEvent is EndOfTrackMetaMidiEvent)) {
                            newTrack.Events.Add(midiEvent);
                        }
                    }
                    trackNumber++;
                }

                // Sort the events by total time, then convert back to delta time,
                // and top things off with an end-of-track marker.
                newTrack.Events.Sort((x, y) => x.DeltaTime.CompareTo(y.DeltaTime));
                newTrack.Events.ConvertTotalsToDeltas();
                newTrack.Events.Add(new EndOfTrackMetaMidiEvent(0));

                // We now have all of the combined events in newTrack.  Clear out the sequence, replacing all the tracks
                // with this new one.
                sequence.Tracks.Clear();
                sequence.Tracks.Add(newTrack);
            }

            return sequence;
        }

        /// <summary>Options used when performing a format conversion.</summary>
        public enum FormatConversionOption
        {
            /// <summary>No special formatting.</summary>
            None,

            /// <summary>
            /// Uses the number of the track as the channel for all events on that track.
            /// Only valid if the number of the track is a valid track number.
            /// </summary>
            CopyTrackToChannel
        }

        /// <summary>Gets whether the sequence contains any lyric events.</summary>
        /// <param name="sequence">The sequence to examine.</param>
        /// <returns>true if the sequence contains any lyric events; otherwise, false.</returns>
        public static bool HasLyrics(this MidiSequence sequence)
        {
            Validate.NonNull("sequence", sequence);
            return sequence.SelectMany(t => t.Events).Any(e => e is LyricTextMetaMidiEvent);
        }
    }
}
