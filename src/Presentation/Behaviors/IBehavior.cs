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
/// Defines an infrastructure for attached properties that, when attached to a target dependency object, directly influences
/// the state and functioning of said object.
/// </summary>
public interface IBehavior
{
    /// <summary>
    /// Gets the default property metadata to use when registering this behavior as an attached property.
    /// </summary>
    PropertyMetadata DefaultMetadata { get; }
}
