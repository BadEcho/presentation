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
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using BadEcho.Presentation.Properties;

namespace BadEcho.Presentation;

/// <summary>
/// Provides a converter of <see cref="Rect"/> objects to and from JSON.
/// </summary>
public sealed class JsonRectConverter : JsonConverter<Rect>
{
    /// <inheritdoc/>
    public override Rect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException(Strings.JsonRectInvalidValue);

        string rect = reader.GetString() ?? string.Empty;

        string[] sides = rect.Split(CultureInfo.InvariantCulture.TextInfo.ListSeparator);

        if (sides.Length == 0)
            return Rect.Empty;
        
        if (sides.Length != 4)
            throw new JsonException(Strings.JsonRectInvalidValue);
        
        return new Rect(double.Parse(sides[0], CultureInfo.InvariantCulture),
                        double.Parse(sides[1], CultureInfo.InvariantCulture),
                        double.Parse(sides[2], CultureInfo.InvariantCulture),
                        double.Parse(sides[3], CultureInfo.InvariantCulture));
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Rect value, JsonSerializerOptions options)
    {
        Require.NotNull(writer, nameof(writer));

        string separator = CultureInfo.InvariantCulture.TextInfo.ListSeparator;
        string rect = $"{value.X}{separator}{value.Y}{separator}{value.Width}{separator}{value.Height}";

        writer.WriteStringValue(rect);
    }
}
