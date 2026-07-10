// <copyright file="IsExternalInit.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>
//
// This declaration is required to use init-only properties and record types
// when targeting netstandard2.0 or .NET Framework. The type exists in the
// .NET 5+ runtime but must be provided manually for older targets.
// See: https://developercommunity.visualstudio.com/t/error-cs0518-isexternalinit/1244809

namespace System.Runtime.CompilerServices;

// This file lives outside any single project and is linked into every
// project that needs it (see each project's .csproj MSBuild file for the
// linked Compile item), so each assembly gets its own internal copy of the
// type without introducing a project reference between layers that
// otherwise have none.

/// <summary>
/// Reserved for use by the compiler.
/// </summary>
/// <remarks>
/// This type is required by the C# compiler to support init-only properties
/// and record types on targets older than .NET 5. It is not intended for
/// direct use in application code.
/// </remarks>
internal static class IsExternalInit
{
}
