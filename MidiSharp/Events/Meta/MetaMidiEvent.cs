//----------------------------------------------------------------------- 
// <copyright file="MetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>Represents a meta event message.</summary>
    public abstract class MetaMidiEvent : MidiEvent
    {
        /// <summary>The ID of the meta event.</summary>
        private byte m_metaEventID;

        /// <summary>Intializes the meta MIDI event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="metaEventID">The ID of the meta event.</param>
        internal MetaMidiEvent(long deltaTime, byte metaEventID)
            : base(deltaTime)
        {
            m_metaEventID = metaEventID;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t0x{1:X2}", base.ToString(), MetaEventID);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            Validate.NonNull("outputStream", outputStream);
            base.Write(outputStream);
            outputStream.WriteByte(0xFF);
            outputStream.WriteByte(m_metaEventID);
        }

        /// <summary>Gets the ID of this meta event.</summary>
        public byte MetaEventID { get { return m_metaEventID; } }
    }
}
