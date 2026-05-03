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

using System.IO;

namespace BadEcho.Presentation;

/// <summary>
/// Provides a source of autocompletion suggestions based on file system paths.
/// </summary>
public sealed class FileAutocompletionSource : IAutocompletionSource
{
    /// <inheritdoc/>
    public IEnumerable<string> SuggestCompletions(string text)
    {
        Require.NotNull(text, nameof(text));

        List<string> suggestions = [];

        string? directory = text.EndsWith(Path.DirectorySeparatorChar) ? text: Path.GetDirectoryName(text);

        if (!Directory.Exists(directory))
            return suggestions;

        IEnumerable<string> directories = Directory.GetDirectories(directory)
                                                   .Select(n => $"{n}{Path.DirectorySeparatorChar}");

        List<string> directoryItems = [..directories, ..Directory.GetFiles(directory)];
        suggestions.AddRange(directoryItems.Where(i => i.StartsWith(text, StringComparison.CurrentCultureIgnoreCase)));

        return suggestions;
    }
}
