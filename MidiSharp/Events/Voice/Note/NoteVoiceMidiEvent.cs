//----------------------------------------------------------------------- 
// <copyright file="NoteVoiceMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Voice.Note
{
    /// <summary>Represents a voice category message that deals with notes.</summary>
    public abstract class NoteVoiceMidiEvent : VoiceMidiEvent
    {
        /// <summary>The MIDI note to modify (0x0 to 0x7F).</summary>
        private byte m_note;

        /// <summary>Intializes the note voice MIDI event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="category">The category identifier (0x0 through 0xF) for this voice event.</param>
        /// <param name="channel">The channel (0x0 through 0xF) for this voice event.</param>
        /// <param name="note">The MIDI note to modify (0x0 to 0x7F).</param>
        internal NoteVoiceMidiEvent(long deltaTime, byte category, byte channel, byte note) :
            base(deltaTime, category, channel)
        {
            Note = note;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0}\t{1}",
                base.ToString(),
                Channel == (byte)SpecialChannel.Percussion && Enum.IsDefined(typeof(GeneralMidiPercussion), m_note) ?
                    ((GeneralMidiPercussion)m_note).ToString() : GetNoteName(m_note));
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(m_note);
        }

        /// <summary>Gets or sets the MIDI note (0x0 to 0x7F).</summary>
        public byte Note
        {
            get { return m_note; }
            set { Validate.SetIfInRange("Note", ref m_note, value, 0, 127); }
        }

        /// <summary>The first parameter as sent in the MIDI message.</summary>
        internal sealed override byte Parameter1 { get { return m_note; } }

        /// <summary>Create a complete note (both on and off messages).</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="channel">The channel (0x0 through 0xF) for this voice event.</param>
        /// <param name="note">The name of the MIDI note to sound ("C0" to "G10").</param>
        /// <param name="velocity">The velocity of the note (0x0 to 0x7F).</param>
        /// <param name="duration">The duration of the note.</param>
        public static MidiEvent[] Complete(
            long deltaTime, byte channel, string note, byte velocity, long duration)
        {
            return Complete(deltaTime, channel, GetNoteValue(note), velocity, duration);
        }

        /// <summary>Create a complete note (both on and off messages).</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="percussion">The percussion instrument to sound.</param>
        /// <param name="velocity">The velocity of the note (0x0 to 0x7F).</param>
        /// <param name="duration">The duration of the note.</param>
        /// <remarks>Channel 10 (internally 0x9) is assumed.</remarks>
        public static MidiEvent[] Complete(
            long deltaTime, GeneralMidiPercussion percussion, byte velocity, long duration)
        {
            return Complete(deltaTime, (byte)SpecialChannel.Percussion, GetNoteValue(percussion), velocity, duration);
        }

        /// <summary>Create a complete note (both on and off messages) with a specified duration.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="channel">The channel (0x0 through 0xF) for this voice event.</param>
        /// <param name="note">The MIDI note to sound (0x0 to 0x7F).</param>
        /// <param name="velocity">The velocity of the note (0x0 to 0x7F).</param>
        /// <param name="duration">The duration of the note.</param>
        public static MidiEvent[] Complete(
            long deltaTime, byte channel, byte note, byte velocity, long duration)
        {
            return new MidiEvent[2] {
                new OnNoteVoiceMidiEvent(deltaTime, channel, note, velocity),
                new OffNoteVoiceMidiEvent(duration, channel, note, velocity)
            };
        }
    }
}
