// <copyright file="CollectionEquality.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace HL7Parser.Domain.Common;

/// <summary>
/// Provides shared equality and hash code utilities
/// for collection-typed domain members.
/// </summary>
internal static class CollectionEquality
{
    /// <summary>
    /// Determines whether two sequences are equal by comparing their elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequences.</typeparam>
    /// <param name="first">The first sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <returns>
    /// <see langword="true"/> if the sequences are equal;
    /// otherwise <see langword="false"/>.
    /// </returns>
    internal static bool SequenceEqual<T>(IEnumerable<T> first, IEnumerable<T> second)
        => first.SequenceEqual(second);

    internal static int GetHashCode<T>(IEnumerable<T> items)
    {
        var hash = 17;
        foreach (T? item in items)
        {
            hash = (hash * 31) + (item?.GetHashCode() ?? 0);
        }

        return hash;
    }
}
