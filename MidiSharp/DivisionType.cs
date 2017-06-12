//----------------------------------------------------------------------- 
// <copyright file="DivisionType.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiSharp
{
    /// <summary>The division type for a MIDI sequence.</summary>
    public enum DivisionType
    {
        /// <summary>A division measured in the number of ticks per beat (e.g. quarter note).</summary>
        TicksPerBeat,
        /// <summary>A division measured in the number of frames per second.</summary>
        FramesPerSecond
    }
}
