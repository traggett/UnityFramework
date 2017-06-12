//----------------------------------------------------------------------- 
// <copyright file="MidiEvent.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace MidiSharp.Events
{
	/// <summary>A MIDI event, serving as the base class for all types of MIDI events.</summary>
	public abstract class MidiEvent
	{
		/// <summary>The amount of time before this event.</summary>
		private long m_deltaTime;

		/// <summary>Initialize the event.</summary>
		/// <param name="deltaTime">The amount of time before this event.</param>
		internal MidiEvent(long deltaTime)
		{
			// Store the data
			DeltaTime = deltaTime;
		}

		/// <summary>Write the event to the output stream.</summary>
		/// <param name="outputStream">The stream to which the event should be written.</param>
		public virtual void Write(Stream outputStream)
		{
			// Write out the delta time
			WriteVariableLength(outputStream, m_deltaTime);
		}

		/// <summary>Combines two 7-bit values into a single 14-bit value.</summary>
		/// <param name="upper">The upper 7-bits.</param>
		/// <param name="lower">The lower 7-bits.</param>
		/// <returns>A 14-bit value stored in an integer.</returns>
		internal static int CombineBytesTo14Bits(byte upper, byte lower)
		{
			// Turn the two bytes into a 14 bit value
			int fourteenBits = upper;
			fourteenBits <<= 7;
			fourteenBits |= lower;
			return fourteenBits;
		}

		/// <summary>Splits a 14-bit value into two bytes each with 7 of the bits.</summary>
		/// <param name="bits">The value to be split.</param>
		/// <param name="upperBits">The upper 7 bits.</param>
		/// <param name="lowerBits">The lower 7 bits.</param>
		internal static void Split14BitsToBytes(int bits, out byte upperBits, out byte lowerBits)
		{
			lowerBits = (byte)(bits & 0x7F);
			bits >>= 7;
			upperBits = (byte)(bits & 0x7F);
		}

		/// <summary>Writes bytes for a long value in the special 7-bit form.</summary>
		/// <param name="outputStream">The stream to which the length should be written.</param>
		/// <param name="value">The value to be converted and written.</param>
		protected static void WriteVariableLength(Stream outputStream, long value)
		{
            Validate.NonNull("outputStream", outputStream);

			long buffer;

			// Parse the value into bytes containing each set of 7-bits and a 1-bit marker
			// for whether there are more bytes in the length
			buffer = value & 0x7F;
			while ((value >>= 7) > 0) 
			{
                buffer = (buffer << 8) | 0x80;
                buffer += value & 0x7F;
			}

			// Write all of the bytes in correct order
			while (true)
			{
				outputStream.WriteByte((byte)(buffer & 0xFF));
                if ((buffer & 0x80) == 0) {
                    break; // if the marker bit is not set, we're done
                }
                buffer >>= 8;
			}
		}

		/// <summary>Converts an array of bytes into human-readable form.</summary>
		/// <param name="data">The array to convert.</param>
		/// <returns>The string containing the bytes.</returns>
		protected static string DataToString(byte [] data)
		{
			if (data != null)
			{
				var sb = new StringBuilder(2 + (data.Length * 5));
                sb.Append('[');
				for(int i=0; i< data.Length; i++)
				{
					sb.AppendFormat(CultureInfo.InvariantCulture, "{0}0x{1:X2}", i > 0 ? "," : "", data[i]);
				}
				sb.Append(']');
				return sb.ToString();
			}
            return string.Empty;
		}

		/// <summary>Gets the name of a note given its numeric value.</summary>
		/// <param name="note">The numeric value of the note.</param>
		/// <returns>The name of the note.</returns>
		public static string GetNoteName(byte note)
		{
			// Get the octave and the pitch within the octave
			int octave = note / 12;
			int pitch = note % 12;

			// Translate the pitch into a note name
            string name = string.Empty;
			switch(pitch)
			{
				case 0: name = "C"; break;
				case 1: name = "C#"; break;
				case 2: name = "D"; break;
				case 3: name = "D#"; break;
				case 4: name = "E"; break;
				case 5: name = "F"; break;
				case 6: name = "F#"; break;
				case 7: name = "G"; break;
				case 8: name = "G#"; break;
				case 9: name = "A"; break;
				case 10: name = "A#"; break;
				case 11: name = "B"; break;
			}

			// Append the octave onto the name, e.g. C5
			return string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, octave);
		}

		/// <summary>Gets the note value for a string representing the name of a note.</summary>
		/// <param name="noteName">
		/// The name of the note, such as "C#4" or "Eb0".
		/// Valid names are in the range from "C0" (0) to "G10" (127).
		/// </param>
		/// <returns>The numeric value of the specified note.</returns>
		public static byte GetNoteValue(string noteName)
		{
            Validate.NonNull("noteName", noteName);
            Validate.InRange("noteName.Length", noteName.Length, 2, int.MaxValue);

			int noteValue = 0;

			// Get's a value for the note name
			switch(Char.ToLowerInvariant(noteName[0]))
			{
				case 'c': noteValue = 0; break;
				case 'd': noteValue = 2; break;
				case 'e': noteValue = 4; break;
				case 'f': noteValue = 5; break;
				case 'g': noteValue = 7; break;
				case 'a': noteValue = 9; break;
				case 'b': noteValue = 11; break;
				default: Validate.ThrowOutOfRange("noteName[0]", noteName[0], 'a', 'g'); break;
			}

            int curPos = 1;

			// Get a value for the # or b if one exists.  If we want to allow multiple
			// flats or sharps, just wrap this section of code in a loop.
			char nextChar = Char.ToLowerInvariant(noteName[curPos]);
			if (nextChar == 'b') 
			{
				noteValue--; curPos++;
			}
			else if (nextChar == '#')
			{
				noteValue++; curPos++;
			}

			// Make sure we're still have more to read
            Validate.InRange("noteName.Length", noteName.Length, curPos, int.MaxValue);

			// Get the value for the octave (0 through 10)
			int octave = noteName[curPos] - '0';
			if (octave == 1 && curPos < noteName.Length-1) // if the first digit is a 1 and if there are more digits
			{
				curPos++;
				octave = 10 + (noteName[curPos] - '0'); // the second digit better be 0, but just in case
			}
            Validate.InRange("octave", octave, 0, 10);
		
			// Combine octave and note value into final noteValue
			noteValue = (octave * 12) + noteValue;
            Validate.InRange("noteValue", noteValue, 0, 127);
			return (byte)noteValue;
		}
		
		/// <summary>Gets the note value for the specific percussion.</summary>
		/// <param name="percussion">The percussion for which we need the note value.</param>
		/// <returns>The numeric value of the specified percussion.</returns>
		public static byte GetNoteValue(GeneralMidiPercussion percussion)
		{
			// The GeneralMidiPercussion enumeration already has the correct note values
			// built into it, so just cast the value to a byte and return it.
			return (byte)percussion;
		}

		/// <summary>Generate a string representation of the event.</summary>
		/// <returns>A string representation of the event.</returns>
		public override string ToString()
		{
            return string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", GetType().Name, DeltaTime);
		}

		/// <summary>Gets or sets the amount of time before this event.</summary>
		public long DeltaTime 
		{ 
			get { return m_deltaTime; } 
			set { Validate.SetIfInRange("DeltaTime", ref m_deltaTime, value, 0, long.MaxValue); }
		}

		/// <summary>Creates a deep copy of the MIDI event.</summary>
		/// <returns>A deep clone of the MIDI event.</returns>
		public abstract MidiEvent DeepClone();

        /// <summary>Makes a copy of a byte array.</summary>
        /// <param name="data">The data to be copied.</param>
        /// <returns>A new byte array containin the same values as the original.</returns>
        internal static byte[] DeepClone(byte[] data)
        {
            return data != null ? (byte[])data.Clone() : null;
        }
    }
}