﻿using System;

namespace AdventOfCode.Utilities
{
    public interface ILocation
    {
        bool IsPositive { get; }
        bool IsNonNegative { get; }
        bool IsCenter { get; }
        bool IsNonPositive { get; }
        bool IsNegative { get; }

        int ValueSum { get; }
        int ValueProduct { get; }
        int ManhattanDistanceFromCenter { get; }
    }

    public interface ILocation<T> : ILocation, IEquatable<T>
    {
        T Absolute { get; }
        T Invert { get; }

        int ManhattanDistance(T other);
        T SignedDifferenceFrom(T other);
    }
}
