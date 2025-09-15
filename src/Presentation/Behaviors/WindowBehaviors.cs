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
/// Provides behaviors that target and augment <see cref="Window"/> controls.
/// </summary>
public static class WindowBehaviors
{
    /// <summary>
    /// Identifies the attached property that gets or sets the content of a <see cref="Window"/> instance's non-client
    /// area.
    /// </summary>
    public static readonly DependencyProperty NonClientAreaProperty
        = RegisterNonClientAttachment();

    /// <summary>
    /// Gets the value of the <see cref="NonClientAreaProperty"/> attached property for a given <see cref="Window"/>.
    /// </summary>
    /// <param name="source">The window from which the property value is read.</param>
    /// <returns>The non-client area content attached to <c>source</c>.</returns>
    public static FrameworkElement GetNonClientArea(Window source)
    {
        Require.NotNull(source, nameof(source));
        
        return (FrameworkElement) source.GetValue(NonClientAreaProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="NonClientAreaProperty"/> attached property on a given <see cref="Window"/>.
    /// </summary>
    /// <param name="source">The window to which the property value is written.</param>
    /// <param name="value">The content to set.</param>
    public static void SetNonClientArea(Window source, FrameworkElement value)
    {
        Require.NotNull(source, nameof(source));

        source.SetValue(NonClientAreaProperty, value);
    }

    private static DependencyProperty RegisterNonClientAttachment()
    {
        var behavior = new NonClientAreaBehavior();

        return DependencyProperty.RegisterAttached(
            NameOf.ReadDependencyPropertyName(() => NonClientAreaProperty),
            typeof(FrameworkElement),
            typeof(WindowBehaviors),
            behavior.DefaultMetadata);
    }
}