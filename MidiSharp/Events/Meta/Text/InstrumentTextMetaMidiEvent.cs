//----------------------------------------------------------------------- 
// <copyright file="InstrumentTextMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp.Events.Meta.Text
{
    /// <summary>A instrument name meta event.</summary>
    public sealed class InstrumentTextMetaMidiEvent : BaseTextMetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x4;

        /// <summary>Initialize the instrument meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="text">The text associated with the event.</param>
        public InstrumentTextMetaMidiEvent(long deltaTime, string text) : base(deltaTime, MetaId, text) { }
    }
}
