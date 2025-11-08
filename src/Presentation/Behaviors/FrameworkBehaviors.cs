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
public static class FrameworkBehaviors
{
    /// <summary>
    /// Identifies the attached property that gets or sets a <see cref="FrameworkElement"/> instance's collection of
    /// event triggers.
    /// </summary>
    public static readonly DependencyProperty EventTriggersProperty
        = BehaviorFactory.Create<EventTriggersBehavior>(
            NameOf.ReadAccessorEnabledDependencyPropertyName(() => EventTriggersProperty),
            typeof(EventTriggerCollection),
            typeof(FrameworkBehaviors));

    /// <summary>
    /// Identifies the attached property that gets or sets a <see cref="FrameworkElement"/> instance's collection of
    /// property triggers.
    /// </summary>
    public static readonly DependencyProperty TriggersProperty
        = BehaviorFactory.Create<TriggersBehavior>(
            NameOf.ReadAccessorEnabledDependencyPropertyName(() => TriggersProperty),
            typeof(TriggerCollection),
            typeof(FrameworkBehaviors));

    /// <summary>
    /// Gets the value of the <see cref="EventTriggersProperty"/> attached property for a given <see cref="FrameworkElement"/>.
    /// </summary>
    /// <param name="source">The framework-level element from which the property value is read.</param>
    /// <returns>The collection of event triggers attached to <c>source</c>.</returns>
    public static EventTriggerCollection GetEventTriggers(FrameworkElement source) 
        => EventTriggersBehavior.GetAttachment(source);

    /// <summary>
    /// Gets the value of the <see cref="TriggersProperty"/> attached property for a given <see cref="FrameworkElement"/>.
    /// </summary>
    /// <param name="source">The framework-level element from which the property value is read.</param>
    /// <returns>The collection of property triggers attached to <c>source</c>.</returns>
    public static TriggerCollection GetTriggers(FrameworkElement source)
        => TriggersBehavior.GetAttachment(source);
    
    /// <summary>
    /// Provides a compound behavior that hosts a collection of event triggers attached to a target framework-level element.
    /// </summary>
    private sealed class EventTriggersBehavior : CompoundBehavior<FrameworkElement, EventTriggerCollection>
    {
        /// <summary>
        /// Gets the value of the <see cref="FrameworkBehaviors.EventTriggersProperty"/> attached property for a given
        /// <see cref="FrameworkElement"/>.
        /// </summary>
        /// <param name="source">The framework-level element from which the property value is read.</param>
        /// <returns>The collection of event triggers attached to <c>source</c>.</returns>
        public static EventTriggerCollection GetAttachment(FrameworkElement source)
            => GetAttachment(source, EventTriggersProperty);

        /// <inheritdoc/>
        protected override Freezable CreateInstanceCore() 
            => new EventTriggersBehavior();
    }

    /// <summary>
    /// Provides a compound behavior that hosts a collection of property triggers attached to a target framework-level element.
    /// </summary>
    private sealed class TriggersBehavior : CompoundBehavior<FrameworkElement, TriggerCollection>
    {
        /// <summary>
        /// Gets the value of the <see cref="FrameworkBehaviors.TriggersProperty"/> attached property for a given
        /// <see cref="FrameworkElement"/>.
        /// </summary>
        /// <param name="source">The framework-level element from which the property value is read.</param>
        /// <returns>The collection of property triggers attached to <c>source</c>.</returns>
        public static TriggerCollection GetAttachment(FrameworkElement source)
            => GetAttachment(source, TriggersProperty);

        /// <inheritdoc/>
        protected override Freezable CreateInstanceCore() 
            => new TriggersBehavior();
    }
}