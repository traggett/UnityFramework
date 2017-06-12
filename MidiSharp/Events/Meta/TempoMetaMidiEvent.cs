//----------------------------------------------------------------------- 
// <copyright file="TempoMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>A tempo meta event message.</summary>
    public sealed class TempoMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x51;
        /// <summary>The tempo for the event.</summary>
        private int m_tempo;

        /// <summary>Intializes the tempo meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="value">The tempo for the event.</param>
        public TempoMetaMidiEvent(long deltaTime, int value)
            : base(deltaTime, MetaId)
        {
            Value = value;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", base.ToString(), m_tempo);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(0x03);
            outputStream.WriteByte((byte)((m_tempo & 0xFF0000) >> 16));
            outputStream.WriteByte((byte)((m_tempo & 0x00FF00) >> 8));
            outputStream.WriteByte((byte)((m_tempo & 0x0000FF)));
        }

        /// <summary>Gets or sets the tempo for the event.</summary>
        public int Value
        {
            get { return m_tempo; }
            set { Validate.SetIfInRange("Value", ref m_tempo, value, 0, 0xFFFFFF); }
        }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new TempoMetaMidiEvent(DeltaTime, Value);
        }
    }
}
