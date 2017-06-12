//----------------------------------------------------------------------- 
// <copyright file="ChannelPrefixMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>A channel prefix meta event message.</summary>
    public sealed class ChannelPrefixMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x20;
        /// <summary>The prefix for the event.</summary>
        private byte m_prefix;

        /// <summary>Intializes the channel prefix event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="prefix">The prefix for the event.</param>
        public ChannelPrefixMetaMidiEvent(long deltaTime, byte prefix)
            : base(deltaTime, MetaId)
        {
            Prefix = prefix;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t0x{1:X2}", base.ToString(), m_prefix);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(0x01);
            outputStream.WriteByte((byte)m_prefix);
        }

        /// <summary>Gets or sets the prefix for the event.</summary>
        public byte Prefix
        {
            get { return m_prefix; }
            set { Validate.SetIfInRange("Prefix", ref m_prefix, value, 0, 0x7F); }
        }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new ChannelPrefixMetaMidiEvent(DeltaTime, Prefix);
        }
    }
}
