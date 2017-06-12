//----------------------------------------------------------------------- 
// <copyright file="PitchWheelSteps.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp
{
    /// <summary>Half and whole value steps for the pitch wheel.</summary>
    public enum PitchWheelStep
    {
        /// <summary>A complete whole step down.</summary>
        WholeStepDown = 0x0,
        /// <summary>3/4 steps down.</summary>
        ThreeQuarterStepDown = 0x500,
        /// <summary>1/2 step down.</summary>
        HalfStepDown = 0x1000,
        /// <summary>1/4 step down.</summary>
        QuarterStepDown = 0x1500,
        /// <summary>No movement.</summary>
        NoStep = 0x2000,
        /// <summary>1/4 step up.</summary>
        QuarterStepUp = 0x2500,
        /// <summary>1/2 step up.</summary>
        HalfStepUp = 0x3000,
        /// <summary>3/4 steps up.</summary>
        ThreeQuarterStepUp = 0x3500,
        /// <summary>A complete whole step up.</summary>
        WholeStepUp = 0x3FFF
    }
}
