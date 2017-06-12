//----------------------------------------------------------------------- 
// <copyright file="VoiceMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Voice
{
    /// <summary>Represents a voice category message.</summary>
    public abstract class VoiceMidiEvent : MidiEvent
    {
        /// <summary>The status identifier (0x0 through 0xF) for this voice event.</summary>
        private byte m_category;
        /// <summary>The channel (0x0 through 0xF) for this voice event.</summary>
        private byte m_channel;

        /// <summary>Intializes the voice MIDI event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="category">The category identifier (0x0 through 0xF) for this voice event.</param>
        /// <param name="channel">The channel (0x0 through 0xF) for this voice event.</param>
        internal VoiceMidiEvent(long deltaTime, byte category, byte channel)
            : base(deltaTime)
        {
            Validate.SetIfInRange("category", ref m_category, category, 0x0, 0xF);
            Channel = channel;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t0x{1:X1}", base.ToString(), Channel);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(Status);
        }

        /// <summary>Gets the status identifier (0x0 through 0xF) for this voice event.</summary>
        public byte Category { get { return m_category; } }
        /// <summary>Gets or sets the channel (0x0 through 0xF) for this voice event.</summary>
        public byte Channel
        {
            get { return m_channel; }
            set { Validate.SetIfInRange("Channel", ref m_channel, value, 0x0, 0xF); }
        }
        /// <summary>Gets the status byte for the event message (combination of category and channel).</summary>
        public byte Status { get { return (byte)((((int)m_category) << 4) | m_channel); } } // The command is the upper 4 bits and the channel is the lower 4.
        /// <summary>Gets the Dword that represents this event as a MIDI event message.</summary>
        internal int Message { get { return Status | (Parameter1 << 8) | (Parameter2 << 16); } }
        /// <summary>The first parameter as sent in the MIDI message.</summary>
        internal abstract byte Parameter1 { get; }
        /// <summary>The second parameter as sent in the MIDI message.</summary>
        internal abstract byte Parameter2 { get; }
    }
}
