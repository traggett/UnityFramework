//----------------------------------------------------------------------- 
// <copyright file="MarkerTextMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp.Events.Meta.Text
{
    /// <summary>A marker name meta event.</summary>
    public sealed class MarkerTextMetaMidiEvent : BaseTextMetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x6;

        /// <summary>Initialize the marker meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="text">The text associated with the event.</param>
        public MarkerTextMetaMidiEvent(long deltaTime, string text) : base(deltaTime, MetaId, text) { }
    }
}
