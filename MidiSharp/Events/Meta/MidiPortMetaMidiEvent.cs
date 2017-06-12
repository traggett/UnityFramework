//----------------------------------------------------------------------- 
// <copyright file="MidiPortMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>A MIDI port meta event message.</summary>
    public sealed class MidiPortMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x21;
        /// <summary>The port for the event.</summary>
        private byte m_port;

        /// <summary>Intializes the MIDI port event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="port">The port for the event.</param>
        public MidiPortMetaMidiEvent(long deltaTime, byte port)
            : base(deltaTime, MetaId)
        {
            Port = port;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t0x{1:X2}", base.ToString(), m_port);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(0x01);
            outputStream.WriteByte((byte)m_port);
        }

        /// <summary>Gets or sets the port for the event.</summary>
        public byte Port
        {
            get { return m_port; }
            set { Validate.SetIfInRange("Port", ref m_port, value, 0, 0x7F); }
        }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new MidiPortMetaMidiEvent(DeltaTime, Port);
        }
    }
}