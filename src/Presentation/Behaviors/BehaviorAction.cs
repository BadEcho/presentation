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

using System.Windows;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides a base action executed as the result of a behavior's influence on the target object the behavior is attached to.
/// </summary>
public abstract class BehaviorAction : AttachableComponent<DependencyObject>
{
    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="parameter">An optional parameter provided to the action.</param>
    /// <returns>Value indicating the success of the action's execution.</returns>
    public abstract bool Execute(object? parameter);
}