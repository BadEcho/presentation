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

using System.Windows;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides a collection of data triggers for a controlling behavior.
/// </summary>
public sealed class DataTriggerCollection : AttachableComponentCollection<UIElement, DataTrigger>
{
    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore()
        => new DataTriggerCollection();
}
