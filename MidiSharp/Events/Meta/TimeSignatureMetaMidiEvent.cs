//----------------------------------------------------------------------- 
// <copyright file="TimeSignatureMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>A time signature meta event message.</summary>
    public sealed class TimeSignatureMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x58;

        /// <summary>Intializes the time signature meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="numerator">Numerator of the time signature.</param>
        /// <param name="denominator">Negative power of two, denominator of time signature.</param>
        /// <param name="midiClocksPerClick">The number of MIDI clocks per metronome click.</param>
        /// <param name="numberOfNotated32nds">The number of notated 32nd notes per MIDI quarter note.</param>
        public TimeSignatureMetaMidiEvent(long deltaTime,
            byte numerator, byte denominator, byte midiClocksPerClick, byte numberOfNotated32nds) :
            base(deltaTime, MetaId)
        {
            Numerator = numerator;
            Denominator = denominator;
            MidiClocksPerClick = midiClocksPerClick;
            NumberOfNotated32nds = numberOfNotated32nds;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0}\t{1:X2}\t{2:X2}\t{3:X2}\t{4:X2}",
                base.ToString(), Numerator, Denominator, MidiClocksPerClick, NumberOfNotated32nds);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(0x04);
            outputStream.WriteByte(Numerator);
            outputStream.WriteByte(Denominator);
            outputStream.WriteByte(MidiClocksPerClick);
            outputStream.WriteByte(NumberOfNotated32nds);
        }

        /// <summary>Gets or sets the numerator for the event.</summary>
        public byte Numerator { get; set; }

        /// <summary>Gets or sets the denominator for the event.</summary>
        public byte Denominator { get; set; }

        /// <summary>Gets or sets the MIDI clocks per click for the event.</summary>
        public byte MidiClocksPerClick { get; set; }

        /// <summary>Gets or sets the number of notated 32 notes per MIDI quarter note for the event.</summary>
        public byte NumberOfNotated32nds { get; set; }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new TimeSignatureMetaMidiEvent(DeltaTime, Numerator, Denominator, MidiClocksPerClick, NumberOfNotated32nds);
        }
    }
}
