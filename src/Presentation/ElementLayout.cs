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

namespace BadEcho.Presentation;

/// <summary>
/// Provides inheritable layout properties for UI elements.
/// </summary>
public static class ElementLayout
{
    /// <summary>
    /// Identifies the attached property which indicates whether a UI element (and its children, if any) should be displayed
    /// in compact form.
    /// </summary>
    public static readonly DependencyProperty IsCompactProperty
        = DependencyProperty.RegisterAttached(NameOf.ReadDependencyPropertyName(() => IsCompactProperty),
                                              typeof(bool),
                                              typeof(ElementLayout),
                                              new FrameworkPropertyMetadata(false,
                                                                            FrameworkPropertyMetadataOptions.Inherits));
    /// <summary>
    /// Gets the value of the <see cref="IsCompactProperty"/> attached property for a given <see cref="UIElement"/>.
    /// </summary>
    /// <param name="source">The element from which the property value is read.</param>
    /// <returns>Value indicating if <c>source</c> should be displayed in compact form.</returns>
    public static bool GetIsCompact(UIElement source)
    {
        Require.NotNull(source, nameof(source));
        
        return (bool) source.GetValue(IsCompactProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="IsCompactProperty"/> attached property on a given <see cref="UIElement"/>.
    /// </summary>
    /// <param name="source">The element to which the attached property is written.</param>
    /// <param name="value">The value indicating whether <c>source</c> should be displayed in compact form.</param>
    public static void SetIsCompact(UIElement source, bool value)
    {
        Require.NotNull(source, nameof(source));

        source.SetValue(IsCompactProperty, value);
    }
}
