// -----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2026 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace BadEcho.Presentation; 

/// <summary>
/// Provides methods for comparing values originating from XAML declarations or through data binding.
/// </summary>
internal static class XamlValueComparer
{
    /// <summary>
    /// Determines if a value sourced from an object equals a XAML-declared value.
    /// </summary>
    /// <param name="source">The source value to compare. This will be either the value of a dependency property or a binding.</param>
    /// <param name="value">The XAML-declared value to compare.</param>
    /// <returns>True if <c>source</c> equals <c>value</c>; otherwise, false.</returns>
    public static bool Evaluate(object? source, object? value)
    {
        object? convertedValue = value;

        if (source != null && value != null)
        {   // The built-in TypeConverters should cover primitives and WPF-specific types.
            TypeConverter converter = TypeDescriptor.GetConverter(source.GetType());

            if (converter.CanConvertFrom(value.GetType()))
                convertedValue = converter.ConvertFrom(value);
        }
        
        return Equals(source, convertedValue);
    }
}
