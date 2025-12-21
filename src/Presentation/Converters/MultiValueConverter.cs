// -----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2025 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using System.Globalization;
using System.Windows.Data;

namespace BadEcho.Presentation.Converters;

/// <summary>
/// Provides a base <see cref="IMultiValueConverter"/> implementation which performs validation of the values provided
/// by the associated source bindings in addition to making them available in their expectedly strongly typed forms.
/// </summary>
/// <typeparam name="TInput">The type of value produced by the associated source bindings.</typeparam>
/// <typeparam name="TOutput">The type of value produced by the value converter.</typeparam>
/// <remarks>This multi-value converter base is only valid for source bindings that all provide the same type.</remarks>
public abstract class MultiValueConverter<TInput,TOutput> : IMultiValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture) 
        => Convert(values.OfType<TInput>(), parameter, culture);

    /// <inheritdoc/>
    public virtual object?[]? ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {   // You want to return null instead of UnsetValue if a multi-value converter doesn't support backwards conversion.
        // We default to null here because of how rare it is for it to even be possible to convert back to source values.
        return null;
    }

    /// <summary>
    /// Converts the provided input values to a value for the binding target.
    /// </summary>
    /// <inheritdoc cref="IMultiValueConverter.Convert"/>
    /// <returns><c>values</c> in their <typeparamref name="TOutput"/> typed form.</returns>
    protected abstract TOutput Convert(IEnumerable<TInput> values, object? parameter, CultureInfo culture);
}
