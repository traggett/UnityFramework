//----------------------------------------------------------------------- 
// <copyright file="MidiParser.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Meta.Text;
using MidiSharp.Events.Voice;
using MidiSharp.Events.Voice.Note;
using System;
using System.Text;

namespace MidiSharp
{
    /// <summary>MIDI track parser.</summary>
    internal static class MidiParser
    {
        /// <summary>Parses a byte array into a track's worth of events.</summary>
        /// <param name="data">The data to be parsed.</param>
        /// <returns>The track containing the parsed events.</returns>
        public static MidiTrack ParseToTrack(byte[] data)
        {
            long pos = 0; // current position in data
            bool running = false; // whether we're in running status
            int status = 0; // the current status byte
            bool sysExContinue = false; // whether we're in a multi-segment system exclusive message
            byte[] sysExData = null; // system exclusive data up to this point from a multi-segment message

            // Create the new track
            MidiTrack track = new MidiTrack();

            try {
                // Process all bytes, turning them into events
                while (pos < data.Length) {
                    // Read in the delta time
                    long deltaTime = ReadVariableLength(data, ref pos);

                    // Get the next character
                    byte nextValue = data[pos];

                    // Are we continuing a sys ex?  If so, the next value better be 0x7F
                    if (sysExContinue && (nextValue != 0x7F)) {
                        var mpe = new MidiParserException("Expected to find a system exclusive continue byte.", pos);
                        mpe.Data["nextValue"] = nextValue;
                        throw mpe;
                    }

                    // Are we in running status?  Determine whether we're running and
                    // what the current status byte is.
                    if ((nextValue & 0x80) == 0) {
                        // We're now in running status... if the last status was 0, uh oh!
                        if (status == 0) {
                            throw new MidiParserException("Status byte required for running status.", pos);
                        }

                        // Keep the last iteration's status byte, and now we're in running mode
                        running = true;
                    }
                    else {
                        // Not running, so store the current status byte and mark running as false
                        status = nextValue;
                        running = false;
                    }

                    // Grab the 4-bit identifier
                    byte messageType = (byte)((status >> 4) & 0xF);

                    MidiEvent tempEvent = null;

                    // Handle voice events
                    if (messageType >= 0x8 && messageType <= 0xE) {
                        if (!running) pos++; // if we're running, we don't advance; if we're not running, we do
                        byte channel = (byte)(status & 0xF); // grab the channel from the status byte
                        tempEvent = ParseVoiceEvent(deltaTime, messageType, channel, data, ref pos);
                    }
                    // Handle meta events
                    else if (status == 0xFF) {
                        pos++;
                        byte eventType = data[pos];
                        pos++;
                        tempEvent = ParseMetaEvent(deltaTime, eventType, data, ref pos);
                    }
                    // Handle system exclusive events
                    else if (status == 0xF0) {
                        pos++;
                        long length = ReadVariableLength(data, ref pos); // figure out how much data to read

                        // If this is single-segment message, process the whole thing
                        if (data[pos + length - 1] == 0xF7) {
                            sysExData = new byte[length - 1];
                            Array.Copy(data, (int)pos, sysExData, 0, (int)length - 1);
                            tempEvent = new SystemExclusiveMidiEvent(deltaTime, sysExData);
                        }
                        // It's multi-segment, so add the new data to the previously aquired data
                        else {
                            // Add to previously aquired sys ex data
                            int oldLength = (sysExData == null ? 0 : sysExData.Length);
                            byte[] newSysExData = new byte[oldLength + length];
                            if (sysExData != null) sysExData.CopyTo(newSysExData, 0);
                            Array.Copy(data, (int)pos, newSysExData, oldLength, (int)length);
                            sysExData = newSysExData;
                            sysExContinue = true;
                        }
                        pos += length;
                    }
                    // Handle system exclusive continuations
                    else if (status == 0xF7) {
                        if (!sysExContinue) sysExData = null;

                        // Figure out how much data there is
                        pos++;
                        long length = ReadVariableLength(data, ref pos);

                        // Add to previously aquired sys ex data
                        int oldLength = (sysExData == null ? 0 : sysExData.Length);
                        byte[] newSysExData = new byte[oldLength + length];
                        if (sysExData != null) sysExData.CopyTo(newSysExData, 0);
                        Array.Copy(data, (int)pos, newSysExData, oldLength, (int)length);
                        sysExData = newSysExData;

                        // Make it a system message if necessary (i.e. if we find an end marker)
                        if (data[pos + length - 1] == 0xF7) {
                            tempEvent = new SystemExclusiveMidiEvent(deltaTime, sysExData);
                            sysExData = null;
                            sysExContinue = false;
                        }
                    }
                    // Nothing we know about
                    else {
                        var e = new MidiParserException("Invalid status byte found.", pos);
                        e.Data["status"] = status;
                        throw e;
                    }

                    // Add the newly parsed event if we got one
                    if (tempEvent != null) track.Events.Add(tempEvent);
                }

                // Return the newly populated track
                return track;
            }
            // Let MidiParserExceptions through
            catch (MidiParserException) { throw; }
            // Wrap all other exceptions in MidiParserExceptions
            catch (Exception exc) { throw new MidiParserException("Failed to parse MIDI file.", exc, pos); }
        }

        /// <summary>Parse a meta MIDI event from the data stream.</summary>
        /// <param name="deltaTime">The previously parsed delta-time for this event.</param>
        /// <param name="eventType">The previously parsed type of message we're expecting to find.</param>
        /// <param name="data">The data stream from which to read the event information.</param>
        /// <param name="pos">The position of the start of the event information.</param>
        /// <returns>The parsed meta MIDI event.</returns>
        private static MidiEvent ParseMetaEvent(long deltaTime, byte eventType, byte[] data, ref long pos)
        {
            try {
                MidiEvent tempEvent = null;

                // Create the correct meta event based on its meta event id/type
                switch (eventType) {
                    // Sequence number
                    case SequenceNumberMetaMidiEvent.MetaId:
                        pos++; // skip past the 0x02
                        int number = ((data[pos] << 8) | data[pos + 1]);
                        tempEvent = new SequenceNumberMetaMidiEvent(deltaTime, number);
                        pos += 2; // skip read values
                        break;

                    // Text events (copyright, lyrics, etc)
                    case TextMetaMidiEvent.MetaId: 
                        tempEvent = new TextMetaMidiEvent(deltaTime, ReadASCIIText(data, ref pos)); 
                        break;
                    case CopyrightTextMetaMidiEvent.MetaId: 
                        tempEvent = new CopyrightTextMetaMidiEvent(deltaTime, ReadASCIIText(data, ref pos)); 
                        break;
                    case SequenceTrackNameTextMetaMidiEvent.MetaId: 
                        tempEvent = new SequenceTrackNameTextMetaMidiEvent(deltaTime, ReadASCIIText(data, ref pos)); 
                        break;
                    case InstrumentTextMetaMidiEvent.MetaId: 
                        tempEvent = new InstrumentTextMetaMidiEvent(deltaTime, ReadASCIIText(data, ref pos)); 
                        break;
                    case LyricTextMetaMidiEvent.MetaId: 
                        tempEvent = new LyricTextMetaMidiEvent(deltaTime, ReadASCIIText(data, ref pos)); 
                        break;
                    case MarkerTextMetaMidiEvent.MetaId: 
                        tempEvent = new MarkerTextMetaMidiEvent(deltaTime, ReadASCIIText(data, ref pos)); 
                        break;
                    case CuePointTextMetaMidiEvent.MetaId: 
                        tempEvent = new CuePointTextMetaMidiEvent(deltaTime, ReadASCIIText(data, ref pos)); 
                        break;
                    case ProgramNameTextMetaMidiEvent.MetaId: 
                        tempEvent = new ProgramNameTextMetaMidiEvent(deltaTime, ReadASCIIText(data, ref pos)); 
                        break;
                    case DeviceNameTextMidiEvent.MetaId: 
                        tempEvent = new DeviceNameTextMidiEvent(deltaTime, ReadASCIIText(data, ref pos)); 
                        break;

                    // Channel prefix
                    case ChannelPrefixMetaMidiEvent.MetaId:
                        pos++; // skip 0x1
                        tempEvent = new ChannelPrefixMetaMidiEvent(deltaTime, data[pos]);
                        pos++; // skip read value
                        break;

                    // Port number
                    case MidiPortMetaMidiEvent.MetaId:
                        pos++; // skip 0x1
                        tempEvent = new MidiPortMetaMidiEvent(deltaTime, data[pos]);
                        pos++; // skip read value
                        break;

                    // End of track
                    case EndOfTrackMetaMidiEvent.MetaId:
                        pos++; // skip 0x0
                        tempEvent = new EndOfTrackMetaMidiEvent(deltaTime);
                        break;

                    // Tempo
                    case TempoMetaMidiEvent.MetaId:
                        pos++; // skip 0x3
                        int tempo = ((data[pos] << 16) | data[pos + 1] << 8 | data[pos + 2]);
                        tempEvent = new TempoMetaMidiEvent(deltaTime, tempo);
                        pos += 3;
                        break;

                    // SMPTE offset
                    case SMPTEOffsetMetaMidiEvent.MetaId:
                        pos++; // skip 0x5
                        tempEvent = new SMPTEOffsetMetaMidiEvent(deltaTime,
                            data[pos], data[pos + 1], data[pos + 2], data[pos + 3], data[pos + 4]);
                        pos += 5;
                        break;

                    // Time signature
                    case TimeSignatureMetaMidiEvent.MetaId:
                        pos++; // skip past 0x4
                        tempEvent = new TimeSignatureMetaMidiEvent(deltaTime,
                            data[pos], data[pos + 1], data[pos + 2], data[pos + 3]);
                        pos += 4;
                        break;

                    // Key signature
                    case KeySignatureMetaMidiEvent.MetaId:
                        pos++; // skip past 0x2
                        tempEvent = new KeySignatureMetaMidiEvent(deltaTime, (Key)data[pos], (Tonality)data[pos + 1]);
                        pos += 2;
                        break;

                    // Proprietary
                    case ProprietaryMetaMidiEvent.MetaId:
                        // Read in the variable length and that much data, then store it
                        long length = ReadVariableLength(data, ref pos);
                        byte[] propData = new byte[length];
                        Array.Copy(data, (int)pos, propData, 0, (int)length);
                        tempEvent = new ProprietaryMetaMidiEvent(deltaTime, propData);
                        pos += length;
                        break;

                    // An unknown meta event!
                    default:
                        // Read in the variable length and that much data, then store it
                        length = ReadVariableLength(data, ref pos);
                        byte[] unknownData = new byte[length];
                        Array.Copy(data, (int)pos, unknownData, 0, (int)length);
                        tempEvent = new UnknownMetaMidiEvent(deltaTime, eventType, unknownData);
                        pos += length;
                        break;
                }
                return tempEvent;
            }
            // Something bad happened; wrap it in a parser exception
            catch (Exception exc) { throw new MidiParserException("Unable to parse meta MIDI event.", exc, pos); }
        }

        /// <summary>Parse a voice event from the data stream.</summary>
        /// <param name="deltaTime">The previously parsed delta-time for this event.</param>
        /// <param name="messageType">The previously parsed type of message we're expecting to find.</param>
        /// <param name="channel">The previously parsed channel for this message.</param>
        /// <param name="data">The data stream from which to read the event information.</param>
        /// <param name="pos">The position of the start of the event information.</param>
        /// <returns>The parsed voice MIDI event.</returns>
        private static MidiEvent ParseVoiceEvent(long deltaTime, byte messageType, byte channel, byte[] data, ref long pos)
        {
            try {
                MidiEvent tempEvent = null;

                // Create the correct voice event based on its message id/type
                switch (messageType) {
                    // NOTE OFF
                    case OffNoteVoiceMidiEvent.CategoryId:
                        tempEvent = new OffNoteVoiceMidiEvent(deltaTime, channel, data[pos], data[pos + 1]);
                        pos += 2;
                        break;

                    // NOTE ON
                    case OnNoteVoiceMidiEvent.CategoryId:
                        tempEvent = new OnNoteVoiceMidiEvent(deltaTime, channel, data[pos], data[pos + 1]);
                        pos += 2;
                        break;

                    // AFTERTOUCH
                    case AftertouchNoteVoiceMidiEvent.CategoryId:
                        tempEvent = new AftertouchNoteVoiceMidiEvent(deltaTime, channel, data[pos], data[pos + 1]);
                        pos += 2;
                        break;

                    // CONTROLLER
                    case ControllerVoiceMidiEvent.CategoryId:
                        tempEvent = new ControllerVoiceMidiEvent(deltaTime, channel, data[pos], data[pos + 1]);
                        pos += 2;
                        break;

                    // PROGRAM CHANGE
                    case ProgramChangeVoiceMidiEvent.CategoryId:
                        tempEvent = new ProgramChangeVoiceMidiEvent(deltaTime, channel, data[pos]);
                        pos += 1;
                        break;

                    // CHANNEL PRESSURE
                    case ChannelPressureVoiceMidiEvent.CategoryId:
                        tempEvent = new ChannelPressureVoiceMidiEvent(deltaTime, channel, data[pos]);
                        pos += 1;
                        break;

                    // PITCH WHEEL
                    case PitchWheelVoiceMidiEvent.CategoryId:
                        int position = ((data[pos] << 8) | data[pos + 1]);
                        byte upper, lower;
                        MidiEvent.Split14BitsToBytes(position, out upper, out lower);
                        tempEvent = new PitchWheelVoiceMidiEvent(deltaTime, channel, upper, lower);
                        pos += 2;
                        break;

                    // UH OH!
                    default:
                        Validate.ThrowOutOfRange("messageType", messageType, 0x8, 0xE);
                        break;
                }

                // Return the newly parsed event
                return tempEvent;
            }
            // Something bad happened; wrap it in a parser exception
            catch (Exception exc) { throw new MidiParserException("Unable to parse voice MIDI event.", exc, pos); }
        }

        /// <summary>Reads an ASCII text sequence from the data stream.</summary>
        /// <param name="data">The data stream from which to read the text.</param>
        /// <param name="pos">The position of the start of the sequence.</param>
        /// <returns>The text as a string.</returns>
        private static string ReadASCIIText(byte[] data, ref long pos)
        {
            // Read the length of the string, grab the following data as ASCII text, move ahead
            long length = ReadVariableLength(data, ref pos);
            string text = Encoding.UTF8.GetString(data, (int)pos, (int)length);
            pos += length;
            return text;
        }

        /// <summary>Reads a variable-length value from the data stream.</summary>
        /// <param name="data">The data to process.</param>
        /// <param name="pos">The position at which to start processing.</param>
        /// <returns>The value read; pos is updated to reflect the new position.</returns>
        private static long ReadVariableLength(byte[] data, ref long pos)
        {
            // Start with the first byte
            long length = data[pos];

            // If the special "there's more data" marker isn't set, we're done
            if ((data[pos] & 0x80) != 0) {
                // Remove the special marker
                length &= 0x7F;
                do {
                    // Continually get all bytes, removing the marker, until no marker is found
                    pos++;
                    length = (length << 7) + (data[pos] & 0x7F);
                }
                while (pos < data.Length && ((data[pos] & 0x80) != 0));
            }

            // Advance past the last used byte and return the length
            pos++;
            return length;
        }

        /// <summary>Exception thrown when an error is encountered during the parsing of a MIDI file.</summary>
        internal sealed class MidiParserException : Exception
        {
            /// <summary>Position in the data stream that caused the exception.</summary>
            private readonly long m_position;

            /// <summary>Initialize the exception.</summary>
            /// <param name="message">The message for the exception.</param>
            /// <param name="position">Position in the data stream that caused the exception.</param>
            public MidiParserException(string message, long position) :
                this(message, null, position)
            {
            }

            /// <summary>Initialize the exception.</summary>
            /// <param name="message">The message for the exception.</param>
            /// <param name="innerException">The exception that caused this exception.</param>
            /// <param name="position">Position in the data stream that caused the exception.</param>
            public MidiParserException(string message, Exception innerException, long position) :
                base(message, innerException)
            {
                m_position = position;
            }

            /// <summary>Gets or sets the byte position that caused the exception.</summary>
            public long Position { get { return m_position; } }
        }
    }
}