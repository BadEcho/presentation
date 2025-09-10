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
/// Provides general-purpose behaviors that target and augment framework-level elements.
/// </summary>
public static class Behaviors
{
    /// <summary>
    /// Identifies the attached property that gets or sets a <see cref="FrameworkElement"/> instance's collection of
    /// event triggers.
    /// </summary>
    public static readonly DependencyProperty EventTriggersProperty
        = RegisterEventTriggersAttachment();

    /// <summary>
    /// Gets the value of the <see cref="EventTriggersProperty"/> attached property for a given <see cref="FrameworkElement"/>.
    /// </summary>
    /// <param name="source">The framework-level element from which the property value is read.</param>
    /// <returns>The collection of event triggers attached to <c>source</c>.</returns>
    public static EventTriggerCollection GetEventTriggers(FrameworkElement source) 
        => EventTriggersBehavior.GetTriggers(source);
    
    private static DependencyProperty RegisterEventTriggersAttachment()
    {
        var behavior = new EventTriggersBehavior();

        return DependencyProperty.RegisterAttached(
            NameOf.ReadAccessorEnabledDependencyPropertyName(() => EventTriggersProperty),
            typeof(EventTriggerCollection),
            typeof(Behaviors),
            behavior.DefaultMetadata);
    }

    /// <summary>
    /// Provides a compound behavior that hosts a collection of event triggers attached to a target framework-level element.
    /// </summary>
    private sealed class EventTriggersBehavior : CompoundBehavior<FrameworkElement, EventTriggerCollection>
    {
        /// <summary>
        /// Gets the value of the <see cref="Behaviors.EventTriggersProperty"/> attached property for a given
        /// <see cref="FrameworkElement"/>.
        /// </summary>
        /// <param name="source">The framework-level element from which the property value is read.</param>
        /// <returns>The collection of event triggers attached to <c>source</c>.</returns>
        public static EventTriggerCollection GetTriggers(FrameworkElement source)
            => GetAttachment(source, EventTriggersProperty);

        /// <inheritdoc/>
        protected override Freezable CreateInstanceCore()
            => new EventTriggersBehavior();
    }
}