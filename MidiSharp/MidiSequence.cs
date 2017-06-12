//----------------------------------------------------------------------- 
// <copyright file="MidiSequence.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace MidiSharp
{
	/// <summary>Represents a MIDI sequence containing tracks of MIDI data.</summary>
    [DebuggerDisplay("Format = {Format}, Division = {Division}, Tracks = {Tracks.Count}")]
	public sealed class MidiSequence : IEnumerable<MidiTrack>
	{
		/// <summary>The format of the MIDI file (0, 1, or 2).</summary>
		private Format m_format;
		/// <summary>The meaning of delta-times.</summary>
		private int m_division;
		/// <summary>The tracks in the MIDI sequence.</summary>
		private readonly MidiTrackCollection m_tracks;

        /// <summary>Initialize the MIDI sequence with default values for format and division.</summary>
        public MidiSequence() : this(Format.Zero, 480)
        {
        }

		/// <summary>Initialize the MIDI sequence.</summary>
		/// <param name="format">
		/// The format for the MIDI file (0, 1, or 2).
		/// 0 - a single multi-channel track
		/// 1 - one or more simultaneous tracks
		/// 2 - one or more sequentially independent single-track patterns
		/// </param>
		/// <param name="division">The meaning of the delta-times in the file.</param>
		public MidiSequence(Format format, int division)
		{
			Format = format;
			Division = division;
            m_tracks = new MidiTrackCollection(this);
		}

        /// <summary>Initialize the MIDI sequence with a copy of the data from another sequence.</summary>
        /// <param name="source">The source sequence from which to copy.</param>
        public MidiSequence(MidiSequence source)
        {
            Validate.NonNull("source", source);
            
            Format = source.Format;
            Division = source.Division;
            m_tracks = new MidiTrackCollection(this);

            foreach (MidiTrack t in source) {
                m_tracks.Add(new MidiTrack(t));
            }
        }

		/// <summary>Gets the format of the sequence.</summary>
		public Format Format 
        { 
            get { return m_format; }
            private set 
            { 
                Validate.InRange("Format", (int)value, (int)Format.Zero, (int)Format.Two);
                m_format = value;
            }
        }

		/// <summary>
        /// Gets or sets the division for the sequence. This is the time division used to
        /// decode the track event delta times into "real" time, and it represents either ticks 
        /// per beat or frames per second.
        /// </summary>
		public int Division 
		{ 
			get { return m_division; } 
			private set { Validate.SetIfInRange("Division", ref m_division, value, 0, int.MaxValue); }
		}

        /// <summary>Gets the division type of the sequence.</summary>
        public DivisionType DivisionType
        {
            get { return (Division & 0x8000) != 0 ? DivisionType.TicksPerBeat : DivisionType.FramesPerSecond; }
        }

        /// <summary>
        /// Gets the ticks per beat or frame of the division. Ticks per beat translate to the 
        /// number of clock ticks or track delta positions in every quarter note of music or
        /// in every frame.
        /// </summary>
        public int TicksPerBeatOrFrame
        {
            get { return DivisionType == DivisionType.TicksPerBeat ? Division & 0x7FFF : Division & 0x00FF; }
        }

        /// <summary>Gets the number of SMPTE frames per second.</summary>
        public int FramesPerSecond
        {
            get
            {
                if (DivisionType != DivisionType.FramesPerSecond) throw new InvalidOperationException("The division type does not support this value.");
                return (Division & 0x7F00) >> 8;
            }
        }

        /// <summary>Gets the collection of tracks in this sequence.</summary>
        public MidiTrackCollection Tracks { get { return m_tracks; } }

		/// <summary>Gets an enumerator for the tracks in the sequence.</summary>
		/// <returns>An enumerator for the tracks in the sequence.</returns>
		IEnumerator IEnumerable.GetEnumerator() { return m_tracks.GetEnumerator(); }

        /// <summary>Gets an enumerator for the tracks in the sequence.</summary>
        /// <returns>An enumerator for the tracks in the sequence.</returns>
        public IEnumerator<MidiTrack> GetEnumerator() { return m_tracks.GetEnumerator(); }

		/// <summary>Writes the sequence to a string in human-readable form.</summary>
		/// <returns>A human-readable representation of the tracks and events in the sequence.</returns>
		public override string ToString()
		{
			// Create a writer, dump to it, return the string
            using (var writer = new StringWriter(CultureInfo.InvariantCulture)) {
                ToString(writer);
                return writer.ToString();
            }
		}

		/// <summary>Dumps the MIDI sequence to the writer in human-readable form.</summary>
		/// <param name="writer">The writer to which the sequence should be written.</param>
		public void ToString(TextWriter writer)
		{
            Validate.NonNull("writer", writer);
		
			// Info on sequence
			writer.WriteLine("MIDI Sequence");
			writer.WriteLine("Format: " + m_format);
			writer.WriteLine("Tracks: " + m_tracks.Count);
			writer.WriteLine("Division: " + m_division);
			writer.WriteLine("");

			// Print out each track
            foreach (MidiTrack track in this) {
                track.ToString(writer);
            }
		}

		/// <summary>Writes the MIDI sequence to the output stream.</summary>
		/// <param name="outputStream">The stream to which the MIDI sequence should be written.</param>
		public void Save(Stream outputStream)
		{
			// Check parameters
            Validate.NonNull("outputStream", outputStream);

			// Check valid state (as best we can check)
			if (m_tracks.Count < 1) throw new InvalidOperationException("No tracks have been added.");

			// Write out the main header for the sequence
			WriteHeader(outputStream, m_tracks.Count);

			// Write out each track in the order it was added to the sequence
			for(int i=0; i<m_tracks.Count; i++)
			{
				// Write out the track to the stream
				m_tracks[i].Write(outputStream);
			}
		}

		/// <summary>Writes a MIDI file header out to the stream.</summary>
		/// <param name="outputStream">The output stream to which the header should be written.</param>
		/// <param name="numTracks">The number of tracks that will be a part of this sequence.</param>
		/// <remarks>This functionality is automatically performed during a Save.</remarks>
		private void WriteHeader(Stream outputStream, int numTracks)
		{
			// Check parameters
            Validate.NonNull("outputStream", outputStream);
            Validate.InRange("numTracks", numTracks, 1, int.MaxValue);

			// Write out the main header for the sequence
			MThdChunkHeader mainHeader = new MThdChunkHeader(Format, numTracks, m_division);
			mainHeader.Write(outputStream);
		}

		/// <summary>Reads a MIDI stream into a new MidiSequence.</summary>
		/// <param name="inputStream">The stream containing the MIDI data.</param>
		/// <returns>A MidiSequence containing the parsed MIDI data.</returns>
		public static MidiSequence Open(Stream inputStream)
		{
            Validate.NonNull("inputStream", inputStream);
		
			// Read in the main MIDI header
			MThdChunkHeader mainHeader = MThdChunkHeader.Read(inputStream);

			// Read in all of the tracks
			MTrkChunkHeader [] trackChunks = new MTrkChunkHeader[mainHeader.NumberOfTracks];
			for(int i=0; i<mainHeader.NumberOfTracks; i++)
			{
				trackChunks[i] = MTrkChunkHeader.Read(inputStream);
			}

			// Create the MIDI sequence
			MidiSequence sequence = new MidiSequence(mainHeader.Format, mainHeader.Division);
			for(int i=0; i<mainHeader.NumberOfTracks; i++)
			{
				sequence.Tracks.Add(MidiParser.ParseToTrack(trackChunks[i].Data));
			}
			return sequence;
		}
    }
}