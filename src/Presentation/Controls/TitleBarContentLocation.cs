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

namespace BadEcho.Presentation.Controls;

/// <summary>
/// Specifies the location of a title bar's content relative to any navigation buttons.
/// </summary>
public enum TitleBarContentLocation
{
    /// <summary>
    /// The content will appear before any navigation buttons.
    /// </summary>
    BeforeNavigation,
    /// <summary>
    /// The content will appear after any navigation buttons.
    /// </summary>
    AfterNavigation
}
