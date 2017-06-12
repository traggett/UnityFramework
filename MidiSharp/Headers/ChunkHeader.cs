//----------------------------------------------------------------------- 
// <copyright file="ChunkHeader.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.IO;

namespace MidiSharp.Headers
{
    /// <summary>Chunk header to store base MIDI chunk information.</summary>
    internal struct ChunkHeader
    {
        /// <summary>The id representing this chunk header.</summary>
        private readonly byte[] m_id;
        /// <summary>The amount of data following the header.</summary>
        private readonly long m_length;

        /// <summary>Initialize the ChunkHeader.</summary>
        /// <param name="id">The 4-byte header identifier.</param>
        /// <param name="length">The amount of data to be stored under this header.</param>
        public ChunkHeader(byte[] id, long length)
        {
            // Verify the parameters
            Validate.NonNull("id", id);
            Validate.InRange("id.Length", id.Length, 4, 4);
            Validate.InRange("length", length, 0, long.MaxValue);

            // Store the data
            m_id = id;
            m_length = length;
        }

        /// <summary>Gets the id representing this chunk header.</summary>
        public byte[] ID { get { return m_id; } }
        /// <summary>Gets the amount of data following the header.</summary>
        public long Length { get { return m_length; } }

        /// <summary>Writes the chunk header out to the stream.</summary>
        /// <param name="outputStream">The stream to which the header should be written.</param>
        public void Write(Stream outputStream)
        {
            Validate.NonNull("outputStream", outputStream);

            // Write out the id
            outputStream.WriteByte(m_id[0]);
            outputStream.WriteByte(m_id[1]);
            outputStream.WriteByte(m_id[2]);
            outputStream.WriteByte(m_id[3]);

            // Write out the length in big-endian
            outputStream.WriteByte((byte)((m_length & 0xFF000000) >> 24));
            outputStream.WriteByte((byte)((m_length & 0x00FF0000) >> 16));
            outputStream.WriteByte((byte)((m_length & 0x0000FF00) >> 8));
            outputStream.WriteByte((byte)(m_length & 0x000000FF));
        }

        /// <summary>Reads a chunk header from the input stream.</summary>
        /// <param name="inputStream">The stream from which to read.</param>
        /// <returns>The chunk header read from the stream.</returns>
        public static ChunkHeader Read(Stream inputStream)
        {
            Validate.NonNull("inputStream", inputStream);

            // Read the id
            byte[] id = new byte[4];
            if (inputStream.Read(id, 0, id.Length) != id.Length) {
                throw new InvalidOperationException("The stream is invalid.");
            }

            // Read the length
            long length = 0;
            for (int i = 0; i < 4; i++) {
                length = (length << 8) | (byte)Validate.NonNegative(inputStream.ReadByte());
            }

            // Create a new header with the read data
            return new ChunkHeader(id, length);
        }
    }
}
