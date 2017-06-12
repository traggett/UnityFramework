//----------------------------------------------------------------------- 
// <copyright file="ProgramChangeVoiceMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Voice
{
    /// <summary>MIDI event to select an instrument for the channel by assigning a patch number.</summary>
    public sealed class ProgramChangeVoiceMidiEvent : VoiceMidiEvent
    {
        /// <summary>The category status byte for ProgramChange messages.</summary>
        internal const byte CategoryId = 0xC;
        /// <summary>The number of the program to which to change (0x0 to 0x7F).</summary>
        private byte m_number;

        /// <summary>Initialize the ProgramChange MIDI event message.</summary>
        /// <param name="deltaTime">The delta-time since the previous message.</param>
        /// <param name="channel">The channel to which to write the message (0 through 15).</param>
        /// <param name="number">The instrument to which to change.</param>
        public ProgramChangeVoiceMidiEvent(long deltaTime, byte channel, GeneralMidiInstrument number) :
            this(deltaTime, channel, (byte)number)
        {
        }

        /// <summary>Initialize the ProgramChange MIDI event message.</summary>
        /// <param name="deltaTime">The delta-time since the previous message.</param>
        /// <param name="channel">The channel to which to write the message (0 through 15).</param>
        /// <param name="number">The number of the program to which to change.</param>
        public ProgramChangeVoiceMidiEvent(long deltaTime, byte channel, byte number) :
            base(deltaTime, CategoryId, channel)
        {
            Validate.SetIfInRange("number", ref m_number, number, 0, 127);
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return Enum.IsDefined(typeof(GeneralMidiInstrument), m_number) ?
                string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", base.ToString(), (GeneralMidiInstrument)m_number) :
                string.Format(CultureInfo.InvariantCulture, "{0}\t0x{1:X2}", base.ToString(), m_number);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(m_number);
        }

        /// <summary>Gets or sets the number of the program to which to change (0x0 to 0x7F).</summary>
        public byte Number
        {
            get { return m_number; }
            set { Validate.SetIfInRange("Number", ref m_number, value, 0, 127); }
        }

        /// <summary>The first parameter as sent in the MIDI message.</summary>
        internal override byte Parameter1 { get { return m_number; } }
        /// <summary>The second parameter as sent in the MIDI message.</summary>
        internal override byte Parameter2 { get { return 0; } }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new ProgramChangeVoiceMidiEvent(DeltaTime, Channel, Number);
        }
    }
}
