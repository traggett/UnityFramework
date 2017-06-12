//----------------------------------------------------------------------- 
// <copyright file="MTrkChunkHeader.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.IO;

namespace MidiSharp.Headers
{
	/// <summary>"MTrk" header for writing out tracks.</summary>
	/// <remarks>"MTrkChunkHeader" is a bit of a misnomer as it includes all of the data for the track, as well, in byte form.</remarks>
	internal struct MTrkChunkHeader
    {
        /// <summary>The MTrk header id.</summary>
        private static readonly byte[] MTrkID = new byte[] { 0x4d, 0x54, 0x72, 0x6b }; // 0x4d54726b = "MTrk"
		/// <summary>Additional chunk header data.</summary>
		private readonly ChunkHeader m_header;
		/// <summary>Data for which this is a header.</summary>
		private readonly byte[] m_data;

		/// <summary>Initialize the MTrk chunk header.</summary>
		/// <param name="data">The track data for which this is a header.</param>
		public MTrkChunkHeader(byte [] data)
		{
			m_header = new ChunkHeader(MTrkID, data != null ? data.Length : 0);
			m_data = data;
		}

		/// <summary>Gets the data for which this is a header.</summary>
		public byte [] Data { get { return m_data; } }

		/// <summary>Validates that a header is correct as an MThd header.</summary>
		/// <param name="header">The header to be validated.</param>
        private static void ValidateHeader(ChunkHeader header)
        {
            byte[] validHeader = MTrkID;
            for (int i = 0; i < 4; i++) {
                if (header.ID[i] != validHeader[i]) {
                    throw new InvalidOperationException("Invalid MThd header.");
                }
            }
            if (header.Length <= 0) {
                throw new InvalidOperationException("The length of the MThd header is incorrect.");
            }
        }

		/// <summary>Writes the track header out to the stream.</summary>
		/// <param name="outputStream">The stream to which the header should be written.</param>
		public void Write(Stream outputStream)
		{
            Validate.NonNull("outputStream", outputStream);
			m_header.Write(outputStream);
			if (m_data != null) outputStream.Write(m_data, 0, m_data.Length);
		}

		/// <summary>Read in an MTrk chunk from the stream.</summary>
		/// <param name="inputStream">The stream from which to read the MTrk chunk.</param>
		/// <returns>The MTrk chunk read.</returns>
		public static MTrkChunkHeader Read(Stream inputStream)
		{
            Validate.NonNull("inputStream", inputStream);
		
			// Read in a header from the stream and validate it
			ChunkHeader header = ChunkHeader.Read(inputStream);
			ValidateHeader(header);

			// Read in the amount of data specified in the chunk header
			byte[] data = new byte[header.Length];
            int bytesRead;
            for (int offset = 0; offset < data.Length; offset += bytesRead) {
                if ((bytesRead = inputStream.Read(data, offset, data.Length - offset)) < 0) {
                    throw new InvalidOperationException("Not enough data in stream to fill MTrk chunk.");
                }
            }

			// Return the new chunk
			return new MTrkChunkHeader(data);
		}
	}
}