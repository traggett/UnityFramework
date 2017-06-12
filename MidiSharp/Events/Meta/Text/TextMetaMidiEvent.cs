//----------------------------------------------------------------------- 
// <copyright file="TextMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp.Events.Meta.Text
{
    /// <summary>A text meta event.</summary>
    public sealed class TextMetaMidiEvent : BaseTextMetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x1;

        /// <summary>Initialize the text meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="text">The text associated with the event.</param>
        public TextMetaMidiEvent(long deltaTime, string text) : base(deltaTime, MetaId, text) { }
    }
}