//----------------------------------------------------------------------- 
// <copyright file="Validate.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

using System;
using System.Globalization;

namespace MidiSharp
{
    internal static class Validate
    {
        #region Throw*
        internal static void ThrowOutOfRange<T>(string parameterName, T input, T minInclusive, T maxInclusive)
        {
            throw new ArgumentOutOfRangeException(parameterName,
                string.Format(CultureInfo.CurrentUICulture, "{0} is outside of range [{1},{2}]", input, minInclusive, maxInclusive));
        }

        internal static void ThrowNull(string parameterName)
        {
            throw new ArgumentNullException(parameterName);
        }

        internal static void ThrowNonNegativeInvalidData()
        {
            throw new InvalidOperationException("A zero or positive value was expected.");
        }
        #endregion

        #region InRange
        internal static void InRange(string parameterName, byte input, byte minInclusive, byte maxInclusive)
        {
            if (input < minInclusive || input > maxInclusive) ThrowOutOfRange(parameterName, input, minInclusive, maxInclusive);
        }

        internal static void InRange(string parameterName, sbyte input, sbyte minInclusive, sbyte maxInclusive)
        {
            if (input < minInclusive || input > maxInclusive) ThrowOutOfRange(parameterName, input, minInclusive, maxInclusive);
        }

        internal static void InRange(string parameterName, int input, int minInclusive, int maxInclusive)
        {
            if (input < minInclusive || input > maxInclusive) ThrowOutOfRange(parameterName, input, minInclusive, maxInclusive);
        }

        internal static void InRange(string parameterName, long input, long minInclusive, long maxInclusive)
        {
            if (input < minInclusive || input > maxInclusive) ThrowOutOfRange(parameterName, input, minInclusive, maxInclusive);
        }
        #endregion

        #region SetIfInRange
        internal static void SetIfInRange(string parameterName, ref byte target, byte input, byte minInclusive, byte maxInclusive)
        {
            InRange(parameterName, input, minInclusive, maxInclusive);
            target = input;
        }

        internal static void SetIfInRange(string parameterName, ref int target, int input, int minInclusive, int maxInclusive)
        {
            InRange(parameterName, input, minInclusive, maxInclusive);
            target = input;
        }

        internal static void SetIfInRange(string parameterName, ref long target, long input, long minInclusive, long maxInclusive)
        {
            InRange(parameterName, input, minInclusive, maxInclusive);
            target = input;
        }
        #endregion

        #region *NonNull
        internal static void NonNull<T>(string parameterName, T input) where T : class
        {
            if (input == null) ThrowNull(parameterName);
        }

        internal static void SetIfNonNull<T>(string parameterName, ref T target, T input) where T : class
        {
            NonNull(parameterName, input);
            target = input;
        }
        #endregion

        #region NonNegative
        public static int NonNegative(int value)
        {
            if (value < 0) {
                ThrowNonNegativeInvalidData();
            }
            return value;
        }
        #endregion
    }
}