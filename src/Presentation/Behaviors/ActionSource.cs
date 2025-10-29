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
using System.Windows.Markup;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides a base class for a source of attached actions.
/// </summary>
/// <typeparam name="T">The type deriving from this class. Used for dependency property registration purposes.</typeparam>
[ContentProperty("Actions")]
public abstract class ActionSource<T> : AttachableComponent<UIElement>
    where T : ActionSource<T>, new()
{
    /// <summary>
    /// Identifies the <see cref="Actions"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ActionsProperty
        = DependencyProperty.Register(nameof(Actions),
                                      typeof(BehaviorActionCollection),
                                      typeof(T));

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionSource{T}"/> class.
    /// </summary>
    protected ActionSource()
    {
        var actions = new BehaviorActionCollection();

        SetValue(ActionsProperty, actions);
    }

    /// <summary>
    /// Gets the collection of attached actions.
    /// </summary>
    public BehaviorActionCollection Actions
    {
        get => (BehaviorActionCollection) GetValue(ActionsProperty);
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        if (TargetObject == null)
            return;

        foreach (BehaviorAction action in Actions)
        {
            action.Attach(TargetObject);
        }
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (TargetObject == null)
            return;

        foreach (BehaviorAction action in Actions)
        {
            action.Detach(TargetObject);
        }
    }

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore()
        => new T();

    /// <summary>
    /// Executes the attached actions.
    /// </summary>
    protected void ExecuteActions()
    {
        foreach (BehaviorAction action in Actions)
        {
            if (!action.Execute())
                return;
        }
    }
}
