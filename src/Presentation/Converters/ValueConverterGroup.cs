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

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace BadEcho.Presentation.Converters;

/// <summary>
/// Provides a group of <see cref="IValueConverter"/> instances, allowing for the combining of multiple physical value converters into
/// a single logical value converter.
/// </summary>
[ContentProperty("Converters")]
public sealed class ValueConverterGroup : IValueConverter
{
    /// <summary>
    /// Gets the chained <see cref="IValueConverter"/> instances used during the conversion process.
    /// </summary>
    public ICollection<IValueConverter> Converters
    { get; } = [];

    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => Converters.Aggregate(value, (currentValue, c) => c.Convert(currentValue, targetType, parameter, culture));

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Converters.Aggregate(value, (currentValue, c) => c.ConvertBack(currentValue, targetType, parameter, culture));
}
