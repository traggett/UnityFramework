//----------------------------------------------------------------------- 
// <copyright file="ProprietaryMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>A proprietary meta event message.</summary>
    public sealed class ProprietaryMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x7F;
        /// <summary>The data of the event.</summary>
        private byte[] m_data;

        /// <summary>Intializes the proprietary meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="data">The data associated with the event.</param>
        public ProprietaryMetaMidiEvent(long deltaTime, byte[] data) :
            base(deltaTime, MetaId)
        {
            Data = data;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            string baseValue = base.ToString();
            return m_data != null ?
                string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", baseValue, DataToString(m_data)) :
                baseValue;
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            WriteVariableLength(outputStream, m_data != null ? m_data.Length : 0);
            if (m_data != null) outputStream.Write(m_data, 0, m_data.Length);
        }

        /// <summary>Gets or sets the data for the event.</summary>
        public byte[] Data { get { return m_data; } set { m_data = value; } }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new ProprietaryMetaMidiEvent(DeltaTime, DeepClone(Data));
        }
    }
}
