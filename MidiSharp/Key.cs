//----------------------------------------------------------------------- 
// <copyright file="Key.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp
{
	/// <summary>The number of sharps or flats in the key signature.</summary>
	public enum Key : sbyte
    {
        /// <summary>Key has 7 flats.</summary>
        Flat7 = -7,
        /// <summary>Key has 6 flats.</summary>
        Flat6 = -6,
		/// <summary>Key has 5 flats.</summary>
		Flat5 = -5,
		/// <summary>Key has 4 flats.</summary>
        Flat4 = -4,
        /// <summary>Key has 3 flats.</summary>
        Flat3 = -3,
        /// <summary>Key has 2 flats.</summary>
        Flat2 = -2,
        /// <summary>Key has 1 flat.</summary>
        Flat1 = -1,
		/// <summary>Key has no sharps or flats.</summary>
		NoFlatsOrSharps = 0,
		/// <summary>Key has 1 sharp.</summary>
		Sharp1 = 1,
		/// <summary>Key has 2 sharps.</summary>
		Sharp2 = 2,
		/// <summary>Key has 3 sharps.</summary>
		Sharp3 = 3,
		/// <summary>Key has 4 sharps.</summary>
		Sharp4 = 4,
		/// <summary>Key has 5 sharps.</summary>
		Sharp5 = 5,
		/// <summary>Key has 6 sharps.</summary>
		Sharp6 = 6,
		/// <summary>Key has 7 sharps.</summary>
		Sharp7 = 7,
	}
}
