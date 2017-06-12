//----------------------------------------------------------------------- 
// <copyright file="OnNoteVoiceMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Voice.Note
{
    /// <summary>Midi event to start playing a note.</summary>
    public sealed class OnNoteVoiceMidiEvent : NoteVoiceMidiEvent
    {
        /// <summary>The category status byte for NoteOn messages.</summary>
        internal const byte CategoryId = 0x9;
        /// <summary>The velocity of the note (0x0 to 0x7F).</summary>
        private byte m_velocity;

        /// <summary>Initialize the NoteOn MIDI event message.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="channel">The channel (0x0 through 0xF) for this voice event.</param>
        /// <param name="note">The name of the MIDI note to sound ("C0" to "G10").</param>
        /// <param name="velocity">The velocity of the note (0x0 to 0x7F).</param>
        public OnNoteVoiceMidiEvent(long deltaTime, byte channel, string note, byte velocity) :
            this(deltaTime, channel, GetNoteValue(note), velocity)
        {
        }

        /// <summary>Initialize the NoteOn MIDI event message.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="percussion">The percussion instrument to sound.</param>
        /// <param name="velocity">The velocity of the note (0x0 to 0x7F).</param>
        /// <remarks>Channel 10 (internally 0x9) is assumed.</remarks>
        public OnNoteVoiceMidiEvent(long deltaTime, GeneralMidiPercussion percussion, byte velocity) :
            this(deltaTime, (byte)SpecialChannel.Percussion, GetNoteValue(percussion), velocity)
        {
        }

        /// <summary>Initialize the NoteOn MIDI event message.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="channel">The channel (0x0 through 0xF) for this voice event.</param>
        /// <param name="note">The MIDI note to sound (0x0 to 0x7F).</param>
        /// <param name="velocity">The velocity of the note (0x0 to 0x7F).</param>
        public OnNoteVoiceMidiEvent(long deltaTime, byte channel, byte note, byte velocity) :
            base(deltaTime, CategoryId, channel, note)
        {
            Velocity = velocity;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t0x{1:X2}", base.ToString(), m_velocity); 
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            // Write out the base event information followed by this event's data
            base.Write(outputStream);
            outputStream.WriteByte(m_velocity);
        }

        /// <summary>Gets or sets the velocity of the note (0x0 to 0x7F).</summary>
        public byte Velocity
        {
            get { return m_velocity; }
            set { Validate.SetIfInRange("Velocity", ref m_velocity, value, 0, 127); }
        }

        /// <summary>The second parameter as sent in the MIDI message.</summary>
        internal override byte Parameter2 { get { return m_velocity; } }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new OnNoteVoiceMidiEvent(DeltaTime, Channel, Note, Velocity);
        }
    }
}