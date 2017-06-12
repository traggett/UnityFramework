//----------------------------------------------------------------------- 
// <copyright file="MidiTrack.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp.Events;
using MidiSharp.Events.Meta;
using MidiSharp.Events.Meta.Text;
using MidiSharp.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace MidiSharp
{
	/// <summary>Represents a single MIDI track in a MIDI file.</summary>
    [DebuggerDisplay("Name = {TrackName}, Events = {Events.Count}")]
	public sealed class MidiTrack : IEnumerable<MidiEvent>
    {
        /// <summary>Collection of MIDI event added to this track.</summary>
        private readonly MidiEventCollection m_events;
		/// <summary>Whether the track can be written without an end of track marker.</summary>
		private bool m_requireEndOfTrack;

		/// <summary>Initialize the track.</summary>
		public MidiTrack()
		{
			// Create the buffer to store all event information
            m_events = new MidiEventCollection();

			// We don't yet have an end of track marker, but we want one eventually.
			m_requireEndOfTrack = true;
		}

        /// <summary>Initializes the track with a copy of the data in another track.</summary>
        /// <returns>The track to copy.</returns>
        public MidiTrack(MidiTrack source) : this()
        {
            Validate.NonNull("source", source);
            m_requireEndOfTrack = source.RequireEndOfTrack;
            foreach (var e in source.Events) {
                m_events.Add(e.DeepClone());
            }
        }

		/// <summary>Gets whether an end of track event has been added.</summary>
		public bool HasEndOfTrack 
		{
			get
			{
				// Determine whether the last event is an end of track event
                return m_events.Count > 0 ?
                    m_events[m_events.Count-1] is EndOfTrackMetaMidiEvent :
                    false;
			}
		}

		/// <summary>Gets or sets whether an end of track marker is required for writing out the entire track.</summary>
		/// <remarks>
		/// MIDI files require an end of track marker at the end of every track.  
		/// Setting this to false could have negative consequences.
		/// </remarks>
        public bool RequireEndOfTrack { get { return m_requireEndOfTrack; } set { m_requireEndOfTrack = false; } }

		/// <summary>Gets the collection of MIDI events that are a part of this track.</summary>
		public MidiEventCollection Events { get { return m_events; } }

        /// <summary>Gets an enumerator for the tracks in the sequence.</summary>
        /// <returns>An enumerator for the tracks in the sequence.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return Events.GetEnumerator(); }

        /// <summary>Gets an enumerator for the tracks in the sequence.</summary>
        /// <returns>An enumerator for the tracks in the sequence.</returns>
        public IEnumerator<MidiEvent> GetEnumerator() { return Events.GetEnumerator(); }

		/// <summary>Write the track to the output stream.</summary>
		/// <param name="outputStream">The output stream to which the track should be written.</param>
		internal void Write(Stream outputStream)
		{
            Validate.NonNull("outputStream", outputStream);

			// Make sure we have an end of track marker if we need one
			if (!HasEndOfTrack && m_requireEndOfTrack) throw new InvalidOperationException("The track cannot be written until it has an end of track marker.");
			
            using (MemoryStream memStream = new MemoryStream()) {
                // Get the event data and write it out
                for (int i = 0; i < m_events.Count; i++) {
                    m_events[i].Write(memStream);
                }

                // Tack on the header and write the whole thing out to the main stream.
                MTrkChunkHeader header = new MTrkChunkHeader(memStream.ToArray());
                header.Write(outputStream);
            }
		}

		/// <summary>Writes the track to a string in human-readable form.</summary>
		/// <returns>A human-readable representation of the events in the track.</returns>
		public override string ToString()
		{
			// Create a writer, dump to it, return the string
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture)) {
                ToString(writer);
                return writer.ToString();
            }
		}

		/// <summary>Dumps the MIDI track to the writer in human-readable form.</summary>
		/// <param name="writer">The writer to which the track should be written.</param>
		public void ToString(TextWriter writer)
		{
            Validate.NonNull("writer", writer);
			foreach(MidiEvent ev in Events)
			{
				writer.WriteLine(ev.ToString());
			}
		}

        /// <summary>Gets the name of the track, based on finding the first track name event in the track, if one exists.</summary>
        internal string TrackName
        {
            get
            {
                foreach (MidiEvent ev in Events) {
                    var nameEvent = ev as SequenceTrackNameTextMetaMidiEvent;
                    if (nameEvent != null) return nameEvent.Text;
                }
                return null;
            }
        }
	}
}