//----------------------------------------------------------------------- 
// <copyright file="SMPTEOffsetMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>An SMPTE offset meta event message.</summary>
    public sealed class SMPTEOffsetMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x54;

        /// <summary>Intializes the SMTPE offset meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="hours">Hours for the event.</param>
        /// <param name="minutes">Minutes for the event.</param>
        /// <param name="seconds">Seconds for the event.</param>
        /// <param name="frames">Frames for the event.</param>
        /// <param name="fractionalFrames">Fractional frames for the event.</param>
        public SMPTEOffsetMetaMidiEvent(long deltaTime,
            byte hours, byte minutes, byte seconds, byte frames, byte fractionalFrames) :
            base(deltaTime, MetaId)
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Frames = frames;
            FractionalFrames = fractionalFrames;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0}\t0x{1:X2}\t0x{2:X2}\t0x{3:X2}\t0x{4:X2}\t0x{5:X2}", 
                base.ToString(), Hours, Minutes, Seconds, Frames, FractionalFrames);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(0x05);
            outputStream.WriteByte(Hours);
            outputStream.WriteByte(Minutes);
            outputStream.WriteByte(Seconds);
            outputStream.WriteByte(Frames);
            outputStream.WriteByte(FractionalFrames);
        }

        /// <summary>Gets or sets the hours for the event.</summary>
        public byte Hours { get; set; }

        /// <summary>Gets or sets the minutes for the event.</summary>
        public byte Minutes { get; set; }

        /// <summary>Gets or sets the seconds for the event.</summary>
        public byte Seconds { get; set; }

        /// <summary>Gets or sets the frames for the event.</summary>
        public byte Frames { get; set; }

        /// <summary>Gets or sets the fractional frames for the event.</summary>
        public byte FractionalFrames { get; set; }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new SMPTEOffsetMetaMidiEvent(DeltaTime, Hours, Minutes, Seconds, Frames, FractionalFrames);
        }
    }
}
