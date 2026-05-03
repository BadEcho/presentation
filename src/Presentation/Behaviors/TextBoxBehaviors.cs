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
using System.Windows.Controls;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides behaviors that target and augment <see cref="TextBox"/> controls.
/// </summary>
public static class TextBoxBehaviors
{
    /// <summary>
    /// Identifies the attached property that gets or sets the autocompletion source of a <see cref="TextBox"/> instance.
    /// </summary>
    public static readonly DependencyProperty AutocompletionProperty
        = RegisterAutocompletionAttachment();

    /// <summary>
    /// Gets the value of the <see cref="AutocompletionProperty"/> attached property for a given <see cref="TextBox"/>.
    /// </summary>
    /// <param name="source">The text box from which the property value is read.</param>
    /// <returns>The autocompletion source attached to <c>source</c>.</returns>
    public static IAutocompletionSource GetAutocompletion(TextBox source)
    {
        Require.NotNull(source, nameof(source));

        return (IAutocompletionSource) source.GetValue(AutocompletionProperty);
    }

    /// <summary>
    /// Sets the value of the <see cref="AutocompletionProperty"/> attached property on a given <see cref="TextBox"/>.
    /// </summary>
    /// <param name="source">The text box to which the property value is written.</param>
    /// <param name="value">The autocompletion source to set.</param>
    public static void SetAutoCompletion(TextBox source, IAutocompletionSource value)
    {
        Require.NotNull(source, nameof(source));

        source.SetValue(AutocompletionProperty, value);
    }

    private static DependencyProperty RegisterAutocompletionAttachment()
    {
        var behavior = new AutocompletionBehavior();

        return DependencyProperty.RegisterAttached(
            NameOf.ReadDependencyPropertyName(() => AutocompletionProperty),
            typeof(IAutocompletionSource),
            typeof(TextBoxBehaviors),
            behavior.DefaultMetadata);
    }
}
