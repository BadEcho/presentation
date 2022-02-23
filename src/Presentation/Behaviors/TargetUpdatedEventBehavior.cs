﻿//-----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2022 Bad Echo LLC. All rights reserved.
//
//		Bad Echo Technologies are licensed under a
//		Creative Commons Attribution-NonCommercial 4.0 International License.
//
//		See accompanying file LICENSE.md or a copy at:
//		http://creativecommons.org/licenses/by-nc/4.0/
// </copyright>
//-----------------------------------------------------------------------

using System.Windows;
using System.Windows.Data;

namespace BadEcho.Fenestra.Behaviors;

/// <summary>
/// Provides a compound behavior that executes a sequence of actions attached to a target dependency object upon
/// the firing of the <see cref="Binding.TargetUpdatedEvent"/> routed event.
/// </summary>
public sealed class TargetUpdatedEventBehavior : EventBehavior
{
    /// <summary>
    /// Identifies the attached property that gets or sets a <see cref="DependencyObject"/> instance's collection
    /// of actions executed by an attached instance of this behavior.
    /// </summary>
    public static readonly DependencyProperty ActionsProperty 
        = RegisterAttachment();

    /// <summary>
    /// Initializes a new instance of the <see cref="TargetUpdatedEventBehavior"/> class.
    /// </summary>
    public TargetUpdatedEventBehavior()
        : base(Binding.TargetUpdatedEvent)
    { }
        
    /// <inheritdoc/>
    protected override DependencyProperty HostedActionsProperty
        => ActionsProperty;

    /// <summary>
    /// Gets the value of the <see cref="ActionsProperty"/> attached property for a given <see cref="DependencyObject"/>.
    /// </summary>
    /// <param name="source">The dependency object from which the property value is read.</param>
    /// <returns>The collection of actions executed by the instance of this behavior attached to <c>source</c>.</returns>
    public static BehaviorActionCollection<DependencyObject> GetActions(DependencyObject source)
        => GetAttachment(source, ActionsProperty);

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore() 
        => new TargetUpdatedEventBehavior();

    private static DependencyProperty RegisterAttachment()
    {
        TargetUpdatedEventBehavior behavior = new();

        return RegisterAttachment(NameOf.ReadAccessorEnabledDependencyPropertyName(() => ActionsProperty),
                                  typeof(TargetUpdatedEventBehavior),
                                  behavior.DefaultMetadata);
    }
}