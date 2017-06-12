//----------------------------------------------------------------------- 
// <copyright file="Format.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp
{
    /// <summary>MIDI sequence format.</summary>
    public enum Format
    {
        /// <summary>
        /// Format 0 as described in the MIDI specification.  The sequence should contain only one track
        /// with all of the events for the whole song.
        /// </summary>
        Zero = 0,
        /// <summary>
        /// Format 1 as described in the MIDI specification.  The sequence should contain at least two
        /// tracks, with song information such as song title and tempo in the first track and with subsequent
        /// tracks containing the music data relevant to the track.
        /// </summary>
        One = 1,
        /// <summary>
        /// Format 2 as described in the MIDI specification.  The sequence contains multiple tracks
        /// which may not be played together.
        /// </summary>
        Two = 2
    }
}