//----------------------------------------------------------------------- 
// <copyright file="MidiTrackCollection.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MidiSharp
{
    /// <summary>Provides a validated collection of MidiTracks.</summary>
    public sealed class MidiTrackCollection : Collection<MidiTrack>
    {
        /// <summary>The sequence with which this collection is associated.</summary>
        private readonly MidiSequence m_owningSequence;

        /// <summary>Initializes the collection.</summary>
        /// <param name="sequence">The sequence with which this collection is associated.</param>
        internal MidiTrackCollection(MidiSequence sequence)
        {
            Validate.NonNull("sequence", sequence);
            m_owningSequence = sequence;
        }

        /// <summary>Adds a new track to the track collection.</summary>
        /// <returns>The newly added track.</returns>
        public MidiTrack AddNewTrack()
        {
            MidiTrack track = new MidiTrack();
            Add(track);
            return track;
        }

        /// <summary>Adds a collection of tracks to this collection.</summary>
        /// <param name="tracks">The tracks to add.</param>
        public void AddRange(IEnumerable<MidiTrack> tracks)
        {
            Validate.NonNull("tracks", tracks);
            foreach (MidiTrack track in tracks) {
                Add(track);
            }
        }

        /// <summary>Inserts an element into the collection at the specified index.</summary>
        /// <param name="index">The index at which to insert the element..</param>
        /// <param name="item">The item to insert.</param>
        protected override void InsertItem(int index, MidiTrack item)
        {
            // Make sure the track is valid and that is hasn't already been added.
            Validate.NonNull("item", item);
            if (Contains(item)) {
                throw new ArgumentException("This track is already part of the sequence.");
            }

            // If this is format 0, we can only have 1 track
            if (m_owningSequence.Format == Format.Zero && Count >= 1) {
                throw new InvalidOperationException("Format 0 MIDI files can only have 1 track.");
            }

            base.InsertItem(index, item);
        }
    }
}