//----------------------------------------------------------------------- 
// <copyright file="CopyrightTextMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp.Events.Meta.Text
{
    /// <summary>A copyright meta event.</summary>
    public sealed class CopyrightTextMetaMidiEvent : BaseTextMetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x2;

        /// <summary>Initialize the copyright meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="text">The text associated with the event.</param>
        public CopyrightTextMetaMidiEvent(long deltaTime, string text) : base(deltaTime, MetaId, text) { }
    }
}
