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
/// Provides a base behavior that influences the state and functioning of a target framework-level element by controlling an
/// auxiliary component that's attached to it.
/// </summary>
/// <typeparam name="TTarget">The type of <see cref="FrameworkElement"/> the auxiliary component attaches to.</typeparam>
/// <typeparam name="TAttachableComponent">The type of attachable component controlled by this behavior.</typeparam>
public abstract class CompoundBehavior<TTarget, TAttachableComponent> : Behavior<TTarget, TAttachableComponent>
    where TTarget : FrameworkElement
    where TAttachableComponent : class, IAttachableComponent<TTarget>, new()
{
    /// <summary>
    /// Acts as an accessor to the auxiliary component attached to the target framework-level element.
    /// </summary>
    /// <param name="source">The target framework-level element the component is attached to.</param>
    /// <param name="attachedProperty">
    /// The identifier of the dependency property the attachment is set to on the target framework-level element.
    /// </param>
    /// <returns>
    /// The <typeparamref name="TAttachableComponent"/> instance of the behavior's auxiliary component attached to <c>source</c>
    /// as the <c>attachedProperty</c> property.
    /// </returns>
    protected static TAttachableComponent GetAttachment(TTarget source, DependencyProperty attachedProperty)
    {
        Require.NotNull(source, nameof(source));

        TAttachableComponent? attachment = (TAttachableComponent?) source.GetValue(attachedProperty);
            
        if (attachment == null)
        {
            attachment = new TAttachableComponent();
            source.SetValue(attachedProperty, attachment);
        }

        return attachment;
    }

    /// <inheritdoc/>
    protected override void OnValueAssociated(TTarget targetObject, TAttachableComponent newValue)
    {
        Require.NotNull(targetObject, nameof(targetObject));
        Require.NotNull(newValue, nameof(newValue));
        
        newValue.Attach(targetObject);

        targetObject.Unloaded += HandleTargetUnloaded;
    }

    /// <inheritdoc/>
    protected override void OnValueDisassociated(TTarget targetObject, TAttachableComponent oldValue)
    {
        Require.NotNull(targetObject, nameof(targetObject));
        Require.NotNull(oldValue, nameof(oldValue));
        
        oldValue.Detach(targetObject);

        targetObject.Unloaded -= HandleTargetUnloaded;
    }
    
    private void HandleTargetUnloaded(object sender, RoutedEventArgs e)
    {
        TTarget targetObject = (TTarget) sender;

        Detach(targetObject);
    }
}