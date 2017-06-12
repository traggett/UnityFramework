//----------------------------------------------------------------------- 
// <copyright file="ControllerVoiceMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Voice
{
    /// <summary>
    /// MIDI event to modify the tone with data from a pedal, lever, or other device; 
    /// also used for miscellaneous controls such as volume and bank select.
    /// </summary>
    public sealed class ControllerVoiceMidiEvent : VoiceMidiEvent
    {
        /// <summary>The category status byte for Controller messages.</summary>
        internal const byte CategoryId = 0xB;
        /// <summary>The type of controller message (0x0 to 0x7F).</summary>
        private byte m_number;
        /// <summary>The value of the controller message (0x0 to 0x7F).</summary>
        private byte m_value;

        /// <summary>Initialize the Controller MIDI event message.</summary>
        /// <param name="deltaTime">The delta-time since the previous message.</param>
        /// <param name="channel">The channel to which to write the message (0 through 15).</param>
        /// <param name="number">The type of controller message to be written.</param>
        /// <param name="value">The value of the controller message.</param>
        public ControllerVoiceMidiEvent(long deltaTime, byte channel, Controller number, byte value) :
            this(deltaTime, channel, (byte)number, value)
        {
        }

        /// <summary>Initialize the Controller MIDI event message.</summary>
        /// <param name="deltaTime">The delta-time since the previous message.</param>
        /// <param name="channel">The channel to which to write the message (0 through 15).</param>
        /// <param name="number">The type of controller message to be written.</param>
        /// <param name="value">The value of the controller message.</param>
        public ControllerVoiceMidiEvent(long deltaTime, byte channel, byte number, byte value) :
            base(deltaTime, CategoryId, channel)
        {
            m_number = number;
            Validate.SetIfInRange("value", ref m_value, value, 0, 127);
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return Enum.IsDefined(typeof(Controller), m_number) ?
                string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t0x{2:X2}", base.ToString(), (Controller)m_number, m_value) :
                string.Format(CultureInfo.InvariantCulture, "{0}\t0x{1:X2}\t0x{2:X2}", base.ToString(), m_number, m_value);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(m_number);
            outputStream.WriteByte(m_value);
        }

        /// <summary>Gets or sets type of controller message to be written (0x0 to 0x7F).</summary>
        public byte Number
        {
            get { return m_number; }
            set { m_number = value; }
        }

        /// <summary>Gets or sets the value of the controller message (0x0 to 0x7F).</summary>
        public byte Value
        {
            get { return m_value; }
            set { Validate.SetIfInRange("Value", ref m_value, value, 0, 127); }
        }

        /// <summary>The first parameter as sent in the MIDI message.</summary>
        internal override byte Parameter1 { get { return m_number; } }

        /// <summary>The second parameter as sent in the MIDI message.</summary>
        internal override byte Parameter2 { get { return m_value; } }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new ControllerVoiceMidiEvent(DeltaTime, Channel, Number, Value);
        }
    }
}
