//----------------------------------------------------------------------- 
// <copyright file="MidiEventCollection.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using MidiSharp.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MidiSharp
{
    /// <summary>Provides a collection of MidiEvents.</summary>
    public sealed class MidiEventCollection : Collection<MidiEvent>
    {
        /// <summary>Initializes the collection.</summary>
        internal MidiEventCollection() { } // Prevent external instantiation

        /// <summary>Adds a collection of events to this collection.</summary>
        /// <param name="events">The events to add.</param>
        public void AddRange(IEnumerable<MidiEvent> events)
        {
            Validate.NonNull("events", events);
            foreach (MidiEvent ev in events) {
                Add(ev);
            }
        }

        /// <summary>Sorts the events in the collection.</summary>
        /// <param name="comparison">The comparison operation to use for sorting.</param>
        internal void Sort(Comparison<MidiEvent> comparison)
        {
            ((List<MidiEvent>)Items).Sort(comparison);
        }

        /// <summary>Converts the delta times on all events to from delta times to total times.</summary>
        internal void ConvertDeltasToTotals()
        {
            long total = this[0].DeltaTime;
            for (int i = 1; i < Count; i++) {
                total += this[i].DeltaTime;
                this[i].DeltaTime = total;
            }
        }

        /// <summary>Converts the delta times on all events from total times back to delta times.</summary>
        internal void ConvertTotalsToDeltas()
        {
            long lastValue = 0;
            for (int i = 0; i < Count; i++) {
                long tempTime = this[i].DeltaTime;
                this[i].DeltaTime -= lastValue;
                lastValue = tempTime;
            }
        }
    }
}