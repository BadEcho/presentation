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
/// Defines a mechanism for retrieving the data context for navigation content.
/// </summary>
public interface INavigationContextSource
{
    /// <summary>
    /// Gets a view model of the specified type targeting content being navigated to.
    /// </summary>
    /// <param name="viewModelType">The type of view model targeting the content to navigate to.</param>
    /// <returns>A <see cref="IViewModel"/> instance for <c>viewModelType</c>.</returns>
    IViewModel GetViewModel(Type viewModelType);
}
