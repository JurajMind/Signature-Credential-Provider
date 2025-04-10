﻿using System;
using System.Diagnostics;

namespace MicroLibrary
{
    /// <summary>
    ///     MicroStopwatch class
    /// </summary>
    public class MicroStopwatch : Stopwatch
    {
        private readonly double _microSecPerTick =
            1000000D/Frequency;

        public MicroStopwatch()
        {
            if (!IsHighResolution)
            {
                throw new Exception("On this system the high-resolution " +
                                    "performance counter is not available");
            }
        }

        public long ElapsedMicroseconds
        {
            get { return (long) (ElapsedTicks*_microSecPerTick); }
        }
    }
}