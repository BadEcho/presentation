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

using BadEcho.Presentation.ViewModels;

namespace BadEcho.Presentation.Navigation;

/// <summary>
/// Defines a view abstraction that supports navigation.
/// </summary>
public interface INavigationHost
{
    /// <summary>
    /// Gets or sets the view model for the content currently navigated to.
    /// </summary>
    IViewModel? CurrentViewModel { get; set; }
}
