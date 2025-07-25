﻿// -----------------------------------------------------------------------
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
/// Provides a behavior that will execute a method once its required parameter information is set on the target dependency
/// object which the behavior is attached to, as well as an optional cleanup method upon local value disassociation.
/// </summary>
/// <typeparam name="TTarget">The type of <see cref="DependencyObject"/> this behavior attaches to.</typeparam>
/// <typeparam name="TParameter">The type of parameter accepted by the method executed by this behavior.</typeparam>
public sealed class DelegateBehavior<TTarget,TParameter> : Behavior<TTarget, TParameter>
    where TTarget : DependencyObject
    where TParameter : class
{
    private readonly Action<TTarget, TParameter> _associationAction;
    private readonly Action<TTarget, TParameter>? _disassociationAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateBehavior{TTarget,TParameter}"/> class.
    /// </summary>
    /// <param name="action">
    /// The method to execute upon parameter information becoming associated with this behavior.
    /// </param>
    public DelegateBehavior(Action<TTarget, TParameter> action)
        : this(action, null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateBehavior{TTarget,TParameter}"/> class.
    /// </summary>
    /// <param name="associationAction">
    /// The method to execute upon parameter information becoming associated with this behavior.
    /// </param>
    /// <param name="disassociationAction">
    /// The method to execute upon parameter information becoming disassociated with this behavior.
    /// </param>
    public DelegateBehavior(Action<TTarget, TParameter> associationAction, Action<TTarget, TParameter>? disassociationAction)
    {
        Require.NotNull(associationAction, nameof(associationAction));

        _associationAction = associationAction;
        _disassociationAction = disassociationAction;
    }

    /// <inheritdoc/>
    protected override void OnValueAssociated(TTarget targetObject, TParameter newValue) 
        => _associationAction(targetObject, newValue);

    /// <inheritdoc/>
    protected override void OnValueDisassociated(TTarget targetObject, TParameter oldValue) 
        => _disassociationAction?.Invoke(targetObject, oldValue);

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore()
        => new DelegateBehavior<TTarget, TParameter>(_associationAction, _disassociationAction);
}