//----------------------------------------------------------------------- 
// <copyright file="ChannelPressureVoiceMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Voice
{
    /// <summary>MIDI event to apply pressure to a channel's currently playing notes.</summary>
    public sealed class ChannelPressureVoiceMidiEvent : VoiceMidiEvent
    {
        /// <summary>The category status byte for ChannelPressure messages.</summary>
        internal const byte CategoryId = 0xD;
        /// <summary>The amount of pressure to be applied (0x0 to 0x7F).</summary>
        private byte m_pressure;

        /// <summary>Initialize the Controller MIDI event message.</summary>
        /// <param name="deltaTime">The delta-time since the previous message.</param>
        /// <param name="channel">The channel to which to write the message (0 through 15).</param>
        /// <param name="pressure">The pressure to be applied.</param>
        public ChannelPressureVoiceMidiEvent(long deltaTime, byte channel, byte pressure) :
            base(deltaTime, CategoryId, channel)
        {
            Validate.SetIfInRange("pressure", ref m_pressure, pressure, 0, 127); 
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t0x{1:X2}", base.ToString(), m_pressure);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(m_pressure);
        }

        /// <summary>Gets or sets the amount pressure to be applied (0x0 to 0x7F).</summary>
        public byte Pressure
        {
            get { return m_pressure; }
            set { Validate.SetIfInRange("Pressure", ref m_pressure, value, 0, 127); }
        }

        /// <summary>The first parameter as sent in the MIDI message.</summary>
        internal override byte Parameter1 { get { return m_pressure; } }
        /// <summary>The second parameter as sent in the MIDI message.</summary>
        internal override byte Parameter2 { get { return 0; } }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new ChannelPressureVoiceMidiEvent(DeltaTime, Channel, Pressure);
        }
    }
}
