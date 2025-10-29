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

using System.ComponentModel;
using System.Windows;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides a trigger that will execute a sequence of actions when a dependency property's value matches a specified value.
/// </summary>
public sealed class Trigger : ActionSource<Trigger>
{
    private DependencyPropertyDescriptor? _targetDescriptor;
    private DependencyProperty? _property;
    
    /// <summary>
    /// Gets or sets the property that returns the value that is compared with <see cref="Value"/>.
    /// </summary>
    public DependencyProperty? Property
    {
        get => _property;
        set
        {
            _property = value;
            
            UpdatePropertyBinding();
        }
    }

    /// <summary>
    /// Get or sets the value to be compared with the value of <see cref="Property"/>.
    /// </summary>
    public object? Value 
    { get; set; }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        UpdatePropertyBinding();
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        base.OnDetaching(); 

        if (TargetObject == null || _targetDescriptor == null)
            return;

        _targetDescriptor.RemoveValueChanged(TargetObject, OnValueChanged);
        _targetDescriptor = null;
    }

    private void UpdatePropertyBinding()
    {
        if (TargetObject == null)
            return;

        if (Property == null)
            return;

        if (_targetDescriptor != null)
        {
            _targetDescriptor.RemoveValueChanged(TargetObject, OnValueChanged);
            _targetDescriptor = null;
        }

        _targetDescriptor = DependencyPropertyDescriptor.FromProperty(Property, TargetObject.GetType());
        _targetDescriptor.AddValueChanged(TargetObject, OnValueChanged);
    }

    private void OnValueChanged(object? sender, EventArgs e)
    {
        if (TargetObject == null || Property == null)
            return;
        
        var targetValue = TargetObject.GetValue(Property);
        
        // The trigger's Value property, set via an attribute in XAML, will always have an underlying type of string,
        // since the XAML processor doesn't have any information about its actual type (it's declared as an object to allow all possible types).
        // Using a converter from TypeDescriptor will handle all possible primitive types, as well as WPF - specific types such as Thickness.
        var triggerValue = Value == null
            ? null
            : TypeDescriptor.GetConverter(Property.PropertyType).ConvertFrom(Value);

        if (targetValue == null && triggerValue != null)
            return;

        if (targetValue != null && !targetValue.Equals(triggerValue))
            return;

        ExecuteActions();
    }
}
