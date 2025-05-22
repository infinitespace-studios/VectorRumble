#region File Description
//-----------------------------------------------------------------------------
// WorldRules.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
#endregion

namespace VectorRumble
{
    public enum AsteroidDensity
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    /// <summary>
    /// Adjustable game settings.
    /// </summary>
    public static class WorldRules
    {
        public static AsteroidDensity AsteroidDensity = AsteroidDensity.Low;
        public static int BlurIntensity = 5;
        public static bool ControllersCanShootInAllDirections;
        public static string DefaultArena = Strings.Arena_Random;
        public static bool MotionBlur = true;
        public static bool NeonEffect = true;
        public static int ScoreLimit = 10;
    }
}
