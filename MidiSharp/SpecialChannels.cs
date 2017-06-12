//----------------------------------------------------------------------- 
// <copyright file="SpecialChannels.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp
{
	/// <summary>Defines channels reserved for special purposes.</summary>
	public enum SpecialChannel : byte
	{
		/// <summary>General MIDI percussion channel</summary>
		Percussion = 9 // Channel 10 (1-based) is reserved for the percussion map
	}
}