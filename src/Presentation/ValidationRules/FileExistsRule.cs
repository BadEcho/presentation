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
using System.IO;
using System.Windows.Controls;
using BadEcho.Presentation.Properties;

namespace BadEcho.Presentation.ValidationRules;

/// <summary>
/// Provides a validation rule that checks if a path points to an existing file.
/// </summary>
public sealed class FileExistsRule : ValidationRule
{
    /// <inheritdoc/>
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        string? path = (string?) value;

        return !File.Exists(path)
            ? new ValidationResult(false, Strings.ValidationFileDoesNotExist)
            : ValidationResult.ValidResult;
    }
}
