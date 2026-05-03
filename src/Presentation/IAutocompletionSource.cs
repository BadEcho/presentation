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

namespace BadEcho.Presentation;

/// <summary>
/// Provides a source of suggestions to help complete an editing control's text.
/// </summary>
public interface IAutocompletionSource
{
    /// <summary>
    /// Gives suggestions to complete the provided text.
    /// </summary>
    /// <param name="text">The text to provide suggestion completions for.</param>
    /// <returns>Suggested completions for <c>text</c>.</returns>
    IEnumerable<string> SuggestCompletions(string text);
}
