//----------------------------------------------------------------------- 
// <copyright file="EndOfTrackMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>An end of track meta event message.</summary>
    public sealed class EndOfTrackMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x2F;

        /// <summary>Intializes the end of track meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        public EndOfTrackMetaMidiEvent(long deltaTime) : base(deltaTime, MetaId) { }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(0x00);
        }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new EndOfTrackMetaMidiEvent(DeltaTime);
        }
    }
}
