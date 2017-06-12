//----------------------------------------------------------------------- 
// <copyright file="UnknownMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>An unknown meta event message.</summary>
    public sealed class UnknownMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>Intializes the meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="metaEventID">The event ID for this meta event.</param>
        /// <param name="data">The data associated with the event.</param>
        public UnknownMetaMidiEvent(long deltaTime, byte metaEventID, byte[] data) :
            base(deltaTime, metaEventID)
        {
            Data = data;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            string baseValue = base.ToString();
            return Data != null ?
                string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", baseValue, DataToString(Data)) :
                baseValue;
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            WriteVariableLength(outputStream, Data != null ? Data.Length : 0);
            if (Data != null) outputStream.Write(Data, 0, Data.Length);
        }

        /// <summary>Gets or sets the data for the event.</summary>
        public byte[] Data { get; set; }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new UnknownMetaMidiEvent(DeltaTime, MetaEventID, DeepClone(Data));
        }
    }
}
