//----------------------------------------------------------------------- 
// <copyright file="PitchWheelVoiceMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Voice
{
    /// <summary>MIDI event to modify the pitch of all notes played on the channel.</summary>
    public sealed class PitchWheelVoiceMidiEvent : VoiceMidiEvent
    {
        /// <summary>The category status byte for PitchWheel messages.</summary>
        internal const byte CategoryId = 0xE;
        /// <summary>The upper 7-bits of the wheel position..</summary>
        private byte m_upperBits;
        /// <summary>The lower 7-bits of the wheel position..</summary>
        private byte m_lowerBits;

        /// <summary>Initialize the Controller MIDI event message.</summary>
        /// <param name="deltaTime">The delta-time since the previous message.</param>
        /// <param name="channel">The channel to which to write the message (0 through 15).</param>
        /// <param name="steps">The amount of pitch change to apply.</param>
        public PitchWheelVoiceMidiEvent(long deltaTime, byte channel, PitchWheelStep steps) :
            this(deltaTime, channel, (int)steps)
        {
        }

        /// <summary>Initialize the Controller MIDI event message.</summary>
        /// <param name="deltaTime">The delta-time since the previous message.</param>
        /// <param name="channel">The channel to which to write the message (0 through 15).</param>
        /// <param name="upperBits">The upper 7 bits of the position.</param>
        /// <param name="lowerBits">The lower 7 bits of the position.</param>
        public PitchWheelVoiceMidiEvent(long deltaTime, byte channel, byte upperBits, byte lowerBits) :
            base(deltaTime, CategoryId, channel)
        {
            UpperBits = upperBits;
            LowerBits = lowerBits;
        }

        /// <summary>Initialize the Controller MIDI event message.</summary>
        /// <param name="deltaTime">The delta-time since the previous message.</param>
        /// <param name="channel">The channel to which to write the message (0 through 15).</param>
        /// <param name="position">The position of the wheel.</param>
        public PitchWheelVoiceMidiEvent(long deltaTime, byte channel, int position) :
            base(deltaTime, CategoryId, channel)
        {
            // Store the data (validation is implicit)
            Position = position;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", base.ToString(), Position);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(Parameter1);
            outputStream.WriteByte(Parameter2);
        }

        /// <summary>Gets or sets the upper 7 bits of the position.</summary>
        public byte UpperBits
        {
            get { return m_upperBits; }
            set { Validate.SetIfInRange("UpperBits", ref m_upperBits, value, 0x0, 0x7F); }
        }

        /// <summary>Gets or sets the lower 7 bits of the position.</summary>
        public byte LowerBits
        {
            get { return m_lowerBits; }
            set { Validate.SetIfInRange("LowerBits", ref m_lowerBits, value, 0x0, 0x7F); }
        }

        /// <summary>Gets or sets the wheel position.</summary>
        public int Position
        {
            get { return CombineBytesTo14Bits(m_upperBits, m_lowerBits); }
            set
            {
                Validate.InRange("Position", value, 0, 0x3FFF);
                Split14BitsToBytes(value, out m_upperBits, out m_lowerBits);
            }
        }

        /// <summary>The first parameter as sent in the MIDI message.</summary>
        internal override byte Parameter1 { get { return (byte)((Position & 0xFF00) >> 8); } }
        /// <summary>The second parameter as sent in the MIDI message.</summary>
        internal override byte Parameter2 { get { return (byte)(Position & 0xFF); } }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new PitchWheelVoiceMidiEvent(DeltaTime, Channel, Position);
        }
    }
}
