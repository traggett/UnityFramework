//----------------------------------------------------------------------- 
// <copyright file="SequenceNumberMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>A sequence number meta event message.</summary>
    public sealed class SequenceNumberMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x0;
        /// <summary>The sequence number for the event.</summary>
        private int m_number;

        /// <summary>Intializes the sequence number meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="number">The sequence number for the event.</param>
        public SequenceNumberMetaMidiEvent(long deltaTime, int number)
            : base(deltaTime, MetaId)
        {
            Number = number;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", base.ToString(), m_number);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(0x02);
            outputStream.WriteByte((byte)((m_number & 0xFF00) >> 8));
            outputStream.WriteByte((byte)(m_number & 0xFF));
        }

        /// <summary>Gets or sets the sequence number for the event.</summary>
        public int Number
        {
            get { return m_number; }
            set { Validate.SetIfInRange("Number", ref m_number, value, 0, 0xFFFF); }
        }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new SequenceNumberMetaMidiEvent(DeltaTime, Number);
        }
    }
}
