//----------------------------------------------------------------------- 
// <copyright file="Controller.cs" company="Stephen Toub"> 
//     Copyright (c) Stephen Toub. All rights reserved. 
// </copyright> 
//----------------------------------------------------------------------- 

namespace MidiSharp
{
    /// <summary>List of defined controllers.  All descriptions come from MidiRef4.</summary>
    public enum Controller : byte
    {
        /// <summary>Switches between groups of sounds when more than 128 programs are in use.</summary>
        BankSelectCourse = 0,
        /// <summary>Sets the modulation wheel to a particular value.</summary>
        ModulationWheelCourse = 1,
        /// <summary>Often used to control aftertouch.</summary>
        BreathControllerCourse = 2,
        /// <summary>Often used to control aftertouch.</summary>
        FootPedalCourse = 4,
        /// <summary>The rate at which portamento slides the pitch between two notes.</summary>
        PortamentoTimeCourse = 5,
        /// <summary>Various.</summary>
        DataEntryCourse = 6,
        /// <summary>Volume level for a given channel.</summary>
        VolumeCourse = 7,
        /// <summary>Controls stereo-balance.</summary>
        BalanceCourse = 8,
        /// <summary>Where the stereo sound should be placed within the sound field.</summary>
        PanPositionCourse = 10,
        /// <summary>Percentage of volume.</summary>
        ExpressionCourse = 11,
        /// <summary>Various.</summary>
        EffectControl1Course = 12,
        /// <summary>Various.</summary>
        EffectControl2Course = 13,
        /// <summary>Various.</summary>
        GeneralPurposeSlider1 = 16,
        /// <summary>Various.</summary>
        GeneralPurposeSlider2 = 17,
        /// <summary>Various.</summary>
        GeneralPurposeSlider3 = 18,
        /// <summary>Various.</summary>
        GeneralPurposeSlider4 = 19,
        /// <summary>Switches between groups of sounds when more than 128 programs are in use.</summary>
        BankSelectFine = 32,
        /// <summary>Sets the modulation wheel to a particular value.</summary>
        ModulationWheelFine = 33,
        /// <summary>Often used to control aftertouch.</summary>
        BreathControllerFine = 34,
        /// <summary>Often used to control aftertouch.</summary>
        FootPedalFine = 36,
        /// <summary>The rate at which portamento slides the pitch between two notes.</summary>
        PortamentoTimeFine = 37,
        /// <summary>Various.</summary>
        DataEntryFine = 38,
        /// <summary>Volume level for a given channel.</summary>
        VolumeFine = 39,
        /// <summary>Controls stereo-balance.</summary>
        BalanceFine = 40,
        /// <summary>Where the stereo sound should be placed within the sound field.</summary>
        PanPositionFine = 42,
        /// <summary>Percentage of volume.</summary>
        ExpressionFine = 43,
        /// <summary>Various.</summary>
        EffectControl1Fine = 44,
        /// <summary>Various.</summary>
        EffectControl2Fine = 45,
        /// <summary>Lengthens release time of playing notes.</summary>
        HoldPedalOnOff = 64,
        /// <summary>The rate at which portamento slides the pitch between two notes.</summary>
        PortamentoOnOff = 65,
        /// <summary>Sustains notes that are already on.</summary>
        SustenutoPedalOnOff = 66,
        /// <summary>Softens volume of any notes played.</summary>
        SoftPedalOnOff = 67,
        /// <summary>Legato effect between notes.</summary>
        LegatoPedalOnOff = 68,
        /// <summary>Lengthens release time of playing notes.</summary>
        Hold2PedalOnOff = 69,
        /// <summary>Various.</summary>
        SoundVariation = 70,
        /// <summary>Controls envelope levels.</summary>
        SoundTimbre = 71,
        /// <summary>Controls envelope release times.</summary>
        SoundReleaseTime = 72,
        /// <summary>Controls envelope attack time.</summary>
        SoundAttackTime = 73,
        /// <summary>Controls filter's cutoff frequency.</summary>
        SoundBrightness = 74,
        /// <summary>Various.</summary>
        SoundControl6 = 75,
        /// <summary>Various.</summary>
        SoundControl7 = 76,
        /// <summary>Various.</summary>
        SoundControl8 = 77,
        /// <summary>Various.</summary>
        SoundControl9 = 78,
        /// <summary>Various.</summary>
        SoundControl10 = 79,
        /// <summary>Various.</summary>
        GeneralPurposeButton1OnOff = 80,
        /// <summary>Various.</summary>
        GeneralPurposeButton2OnOff = 81,
        /// <summary>Various.</summary>
        GeneralPurposeButton3OnOff = 82,
        /// <summary>Various.</summary>
        GeneralPurposeButton4OnOff = 83,
        /// <summary>Controls level of effects.</summary>
        EffectsLevel = 91,
        /// <summary>Controls level of tremulo.</summary>
        TremuloLevel = 92,
        /// <summary>Controls level of chorus.</summary>
        ChorusLevel = 93,
        /// <summary>Detune amount for device.</summary>
        CelesteLevel = 94,
        /// <summary>Level of phaser effect.</summary>
        PhaserLevel = 95,
        /// <summary>Causes data button's value to increment.</summary>
        DataButtonIncrement = 96,
        /// <summary>Causes data button's value to decrement.</summary>
        DataButtonDecrement = 97,
        /// <summary>Controls which parameter the button and data entry controls affect.</summary>
        NonRegisteredParameterFine = 98,
        /// <summary>Controls which parameter the button and data entry controls affect.</summary>
        NonRegisteredParameterCourse = 99,
        /// <summary>Controls which parameter the button and data entry controls affect.</summary>
        RegisteredParameterFine = 100,
        /// <summary>Controls which parameter the button and data entry controls affect.</summary>
        RegisteredParameterCourse = 101,
        /// <summary>Mutes all sounding notes.</summary>
        AllSoundOff = 120,
        /// <summary>Resets controllers to default states.</summary>
        AllControllersOff = 121,
        /// <summary>Turns on or off local keyboard.</summary>
        LocalKeyboardOnOff = 122,
        /// <summary>Mutes all sounding notes.</summary>
        AllNotesOff = 123,
        /// <summary>Turns Omni off.</summary>
        OmniModeOff = 124,
        /// <summary>Turns Omni on.</summary>
        OmniModeOn = 125,
        /// <summary>Enables Monophonic operation.</summary>
        MonoOperation = 126,
        /// <summary>Enables Polyphonic operation.</summary>
        PolyOperation = 127
    }
}
