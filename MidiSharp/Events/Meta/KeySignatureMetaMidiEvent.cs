//----------------------------------------------------------------------- 
// <copyright file="KeySignatureMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;

namespace MidiSharp.Events.Meta
{
    /// <summary>A key signature meta event message.</summary>
    public sealed class KeySignatureMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The meta id for this event.</summary>
        internal const byte MetaId = 0x59;
        /// <summary>Number of sharps or flats in the signature.</summary>
        private Key m_key;
        /// <summary>Tonality of the signature.</summary>
        private Tonality m_tonality;

        /// <summary>Intializes the meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="key">Key of the signature.</param>
        /// <param name="tonality">Tonality of the signature.</param>
        public KeySignatureMetaMidiEvent(long deltaTime, Key key, Tonality tonality) :
            base(deltaTime, MetaId)
        {
            Key = key;
            Tonality = tonality;
        }

        /// <summary>Intializes the key signature meta event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="key">Key of the signature.</param>
        /// <param name="tonality">Tonality of the signature.</param>
        public KeySignatureMetaMidiEvent(long deltaTime, byte key, byte tonality) :
            this(deltaTime, (Key)key, (Tonality)tonality)
        {
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}", base.ToString(), m_key, m_tonality);
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            outputStream.WriteByte(0x02);
            outputStream.WriteByte((byte)m_key);
            outputStream.WriteByte((byte)m_tonality);
        }

        /// <summary>Gets or sets the numerator for the event.</summary>
        public Key Key
        {
            get { return m_key; }
            set
            {
                Validate.InRange("Key", (sbyte)value, (sbyte)MidiSharp.Key.Flat7, (sbyte)MidiSharp.Key.Sharp7);
                m_key = value;
            }
        }

        /// <summary>Gets or sets the denominator for the event.</summary>
        public Tonality Tonality
        {
            get { return m_tonality; }
            set
            {
                Validate.InRange("Tonality", (byte)value, (byte)MidiSharp.Tonality.Major, (byte)MidiSharp.Tonality.Minor);
                m_tonality = value;
            }
        }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public override MidiEvent DeepClone()
        {
            return new KeySignatureMetaMidiEvent(DeltaTime, this.Key, this.Tonality);
        }
    }
}
