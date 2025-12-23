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
using System.Numerics;
using System.Windows;
using System.Windows.Data;

namespace BadEcho.Presentation.Converters;

/// <summary>
/// Provides a value converter that converts a provided value to its inverse.
/// </summary>
public sealed class InverseValueConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return DependencyProperty.UnsetValue;
        
        return value switch
        {
            bool uValue => !uValue,
            char uValue => -uValue,
            sbyte uValue => -uValue,
            byte uValue => -uValue,
            short uValue => -uValue,
            ushort uValue => -uValue,
            int uValue => -uValue,
            long uValue => -uValue,
            // For some reason, a unary negation operator w/ an unsigned long results in an ambiguous invocation, even though it implements IUnaryNegationOperator...
            // ...so, something going on behind the scenes here. Passing it to a method with the unary negation generic constraint resolves this.
            ulong uValue => Invert(uValue),
            float uValue => -uValue,
            double uValue => -uValue,
            decimal uValue => -uValue,
            _ => DependencyProperty.UnsetValue
        };
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Convert(value, targetType, parameter, culture);

    private static TSelf Invert<TSelf>(TSelf value) where TSelf: IUnaryNegationOperators<TSelf,TSelf> 
        => -value;
}
