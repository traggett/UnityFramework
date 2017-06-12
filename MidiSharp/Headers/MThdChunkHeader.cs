//----------------------------------------------------------------------- 
// <copyright file="MThdChunkHeader.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.IO;

namespace MidiSharp.Headers
{
    /// <summary>"MThd" header for writing out MIDI files.</summary>
    internal struct MThdChunkHeader
    {
        /// <summary>The id for an MThd header.</summary>
        private static readonly byte[] s_mThdID = new byte[] { 0x4d, 0x54, 0x68, 0x64 }; // 0x4d546864 = "MThd"
        /// <summary>Expected length of the MThdHeader. 2 bytes for each of the format, num tracks, and division.</summary>
        const int MThdHeaderLength = 6;

        /// <summary>Additional chunk header data.</summary>
        private readonly ChunkHeader m_header;
        /// <summary>The format for the MIDI file (0, 1, or 2).</summary>
        private readonly Format m_format;
        /// <summary>The number of tracks in the MIDI sequence.</summary>
        private readonly int m_numTracks;
        /// <summary>Specifies the meaning of the delta-times</summary>
        private readonly int m_division;

        /// <summary>Initialize the MThd chunk header.</summary>
        /// <param name="format">
        /// The format for the MIDI file (0, 1, or 2).
        /// 0 - a single multi-channel track
        /// 1 - one or more simultaneous tracks
        /// 2 - one or more sequentially independent single-track patterns
        /// </param>
        /// <param name="numTracks">The number of tracks in the MIDI file.</param>
        /// <param name="division">
        /// The meaning of the delta-times in the file.
        /// If the number is zero or positive, then bits 14 thru 0 represent the number of delta-time 
        /// ticks which make up a quarter-note. If number is negative, then bits 14 through 0 represent
        /// subdivisions of a second, in a way consistent with SMPTE and MIDI time code.
        /// </param>
        public MThdChunkHeader(Format format, int numTracks, int division)
        {
            // Verify the parameters
            Validate.InRange("format", (int)format, (int)MidiSharp.Format.Zero, (int)MidiSharp.Format.Two);
            Validate.InRange("numTracks", numTracks, 1, int.MaxValue);
            Validate.InRange("division", division, 1, int.MaxValue);

            m_header = new ChunkHeader(s_mThdID, 6);
            m_format = format;
            m_numTracks = numTracks;
            m_division = division;
        }

        /// <summary>Gets the format for the MIDI file (0, 1, or 2).</summary>
        public Format Format { get { return m_format; } }
        /// <summary>Gets the number of tracks in the MIDI sequence.</summary>
        public int NumberOfTracks { get { return m_numTracks; } }
        /// <summary>Gets the meaning of the delta-times</summary>
        public int Division { get { return m_division; } }

        /// <summary>Validates that a header is correct as an MThd header.</summary>
        /// <param name="header">The header to be validated.</param>
        private static void ValidateHeader(ChunkHeader header)
        {
            Validate.NonNull("header.ID", header.ID);

            for (int i = 0; i < 4; i++) {
                if (header.ID[i] != s_mThdID[i]) {
                    throw new InvalidOperationException("Invalid MThd header.");
                }
            }
            if (header.Length != MThdHeaderLength) {
                throw new InvalidOperationException("The length of the MThd header is incorrect.");
            }
        }

        /// <summary>Writes the MThd header out to the stream.</summary>
        /// <param name="outputStream">The stream to which the header should be written.</param>
        public void Write(Stream outputStream)
        {
            Validate.NonNull("outputStream", outputStream);

            // Write out the main header
            m_header.Write(outputStream);

            // Add format, numTracks, and division
            WriteTwoByteValue(outputStream, (int)m_format);
            WriteTwoByteValue(outputStream, m_numTracks);
            WriteTwoByteValue(outputStream, m_division);
        }

        /// <summary>Read in an MThd chunk from the stream.</summary>
        /// <param name="inputStream">The stream from which to read the MThd chunk.</param>
        /// <returns>The MThd chunk read.</returns>
        public static MThdChunkHeader Read(Stream inputStream)
        {
            Validate.NonNull("inputStream", inputStream);

            // Read in a header from the stream and validate it
            ValidateHeader(ChunkHeader.Read(inputStream));

            // Read in the format, number of tracks, and the division, each a two-byte value
            int format = ReadTwoByteValue(inputStream);
            int numTracks = ReadTwoByteValue(inputStream);
            int division = ReadTwoByteValue(inputStream);

            // Create a new MThd header and return it
            return new MThdChunkHeader((Format)format, numTracks, division);
        }

        private static int ReadTwoByteValue(Stream stream)
        {
            return (Validate.NonNegative(stream.ReadByte()) << 8) | Validate.NonNegative(stream.ReadByte());
        }

        private static void WriteTwoByteValue(Stream stream, int value)
        {
            stream.WriteByte((byte)((value & 0xFF00) >> 8));
            stream.WriteByte((byte)(value & 0x00FF));
        }
    }
}