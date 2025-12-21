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

namespace BadEcho.Presentation.Converters;

/// <summary>
/// Provides a multi-value converter that converts provided Boolean values to the result of an
/// aggregated operation.
/// </summary>
public sealed class MultiBooleanConverter : MultiValueConverter<bool,bool>
{
    /// <summary>
    /// Gets or sets a <see cref="BooleanOperation"/> value that specifies the aggregated operation
    /// performs on the input values.
    /// </summary>
    public BooleanOperation Operation
    { get; set; }

    /// <inheritdoc/>
    protected override bool Convert(IEnumerable<bool> values, object? parameter, CultureInfo culture)
        => Operation switch
        {
            BooleanOperation.And => values.All(x => x),
            _ => values.Any(x => x)
        };
}
