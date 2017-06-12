//----------------------------------------------------------------------- 
// <copyright file="BaseTextMetaMidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System.Globalization;
using System.IO;
using System.Text;

namespace MidiSharp.Events.Meta.Text
{
    /// <summary>Represents a text meta event message.</summary>
    public abstract class BaseTextMetaMidiEvent : MetaMidiEvent
    {
        /// <summary>The text associated with the event.</summary>
        private string m_text;

        /// <summary>Intializes the meta MIDI event.</summary>
        /// <param name="deltaTime">The amount of time before this event.</param>
        /// <param name="metaEventID">The ID of the meta event.</param>
        /// <param name="text">The text associated with the event.</param>
        internal BaseTextMetaMidiEvent(long deltaTime, byte metaEventID, string text) :
            base(deltaTime, metaEventID)
        {
            Text = text;
        }

        /// <summary>Generate a string representation of the event.</summary>
        /// <returns>A string representation of the event.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", base.ToString(), Text.Trim());
        }

        /// <summary>Write the event to the output stream.</summary>
        /// <param name="outputStream">The stream to which the event should be written.</param>
        public override void Write(Stream outputStream)
        {
            base.Write(outputStream);
            byte[] asciiBytes = Encoding.UTF8.GetBytes(m_text);
            WriteVariableLength(outputStream, asciiBytes.Length);
            outputStream.Write(asciiBytes, 0, asciiBytes.Length);
        }

        /// <summary>Gets or sets the text associated with this event.</summary>
        public string Text
        {
            get { return m_text; }
            set { Validate.SetIfNonNull("Text", ref m_text, value); }
        }

        /// <summary>Creates a deep copy of the MIDI event.</summary>
        /// <returns>A deep clone of the MIDI event.</returns>
        public sealed override MidiEvent DeepClone()
        {
            // Note: this is only valid because a shallow clone of this event is also a deep clone.
            // If this event ever gets a rfield where a shallow copy != deep copy, this implementation 
            // will need to be updated.
            return (MidiEvent)MemberwiseClone();
        }
    }
}
