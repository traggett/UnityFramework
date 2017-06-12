//----------------------------------------------------------------------- 
// <copyright file="ManufacturerId.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp
{
    /// <summary>IDs of MIDI-related manufacturers.</summary>
    public enum ManufacturerId : byte
    {
        /// <summary>Sequential Circuits</summary>
        SequentialCircuits = 1,
        /// <summary>Big Briar</summary>
        BigBriar = 2,
        /// <summary>Octave</summary>
        Octave = 3,
        /// <summary>Moog</summary>
        Moog = 4,
        /// <summary>Passport Designs</summary>
        PassportDesigns = 5,
        /// <summary>Lexicon</summary>
        Lexicon = 6,
        /// <summary>Kurzweil</summary>
        Kurzweil = 7,
        /// <summary>Fender</summary>
        Fender = 8,
        /// <summary>Gulbransen</summary>
        Gulbransen = 9,
        /// <summary>DeltaLabs</summary>
        DeltaLabs = 0x0A,
        /// <summary>SoundComp</summary>
        SoundComp = 0x0B,
        /// <summary>General Electro</summary>
        GeneralElectro = 0x0C,
        /// <summary>Techmar</summary>
        Techmar = 0x0D,
        /// <summary>Matthews Research</summary>
        MatthewsResearch = 0x0E,
        /// <summary>Oberheim</summary>
        Oberheim = 0x10,
        /// <summary>PAIA</summary>
        PAIA = 0x11,
        /// <summary>Simmons</summary>
        Simmons = 0x12,
        /// <summary>DigiDesign</summary>
        DigiDesign = 0x13,
        /// <summary>Fairlight</summary>
        Fairlight = 0x14,
        /// <summary>Peavey</summary>
        Peavey = 0x1B,
        /// <summary>J.L. Cooper</summary>
        JLCooper = 0x15,
        /// <summary>Lowery</summary>
        Lowery = 0x16,
        /// <summary>Lin</summary>
        Lin = 0x17,
        /// <summary>Emu</summary>
        Emu = 0x18,
        /// <summary>Bon Tempi</summary>
        BonTempi = 0x20,
        /// <summary>SIEL</summary>
        SIEL = 0x21,
        /// <summary>Synthe Axe</summary>
        SyntheAxe = 0x23,
        /// <summary>Hohner</summary>
        Hohner = 0x24,
        /// <summary>Crumar</summary>
        Crumar = 0x25,
        /// <summary>Solton</summary>
        Solton = 0x26,
        /// <summary>Jellinghaus Ms</summary>
        JellinghausMs = 0x27,
        /// <summary>CTS</summary>
        CTS = 0x28,
        /// <summary>PPG</summary>
        PPG = 0x29,
        /// <summary>Elka</summary>
        Elka = 0x2F,
        /// <summary>Cheetah</summary>
        Cheetah = 0x36,
        /// <summary>Waldorf</summary>
        Waldorf = 0x3E,
        /// <summary>Kawai</summary>
        Kawai = 0x40,
        /// <summary>Roland</summary>
        Roland = 0x41,
        /// <summary>Korg</summary>
        Korg = 0x42,
        /// <summary>Yamaha</summary>
        Yamaha = 0x43,
        /// <summary>Casio</summary>
        Casio = 0x44,
        /// <summary>Akai</summary>
        Akai = 0x45,

        /// <summary>This ID is for educational or development use only.</summary>
        EducationalUse = 0x7d
    }
}
