//----------------------------------------------------------------------- 
// <copyright file="SystemExclusiveMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events
{
    /// <summary>A system exclusive MIDI event.</summary>
    public sealed class SystemExclusiveMidiEvent : MidiEvent
    {
        /// <summary>Intializes the meta MIDI event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="data">The data for the event.</param>
        public SystemExclusiveMidiEvent(long deltaTime, byte[] data)
            : base(deltaTime)
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
            outputStream.WriteByte(0xF0);
            WriteVariableLength(outputStream, 1 + (Data != null ? Data.Length : 0)); // "1+" for the F7 at the end
            if (Data != null) outputStream.Write(Data, 0, Data.Length);
            outputStream.WriteByte(0xF7);
        }

        /// <summary>Gets or sets the data for this event.</summary>
        public byte[] Data { get; set; }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new SystemExclusiveMidiEvent(DeltaTime, Data != null ? (byte[])Data.Clone() : null);
        }
    }
}
