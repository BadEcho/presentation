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

using System.ComponentModel;
using System.Windows;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides a trigger that will execute a sequence of actions when a dependency property's value matches a specified value.
/// </summary>
public sealed class Trigger : ActionSource<Trigger>
{
    private DependencyPropertyDescriptor? _targetDescriptor;
    
    /// <summary>
    /// Gets or sets the property that returns the value that is compared with <see cref="Value"/>.
    /// </summary>
    public DependencyProperty? Property
    {
        get
        {
            ReadPreamble();
            return field;
        }
        set
        {
            WritePreamble();
            field = value;
            UpdatePropertyBinding();
            WritePostscript();
        }
    }

    /// <summary>
    /// Get or sets the value to be compared with the value of <see cref="Property"/>.
    /// </summary>
    public object? Value
    {
        get
        {
            ReadPreamble();
            return field;
        }
        set
        {
            WritePreamble();
            field = value;
            WritePostscript();
        }
    }

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

        WritePreamble();
        _targetDescriptor.RemoveValueChanged(TargetObject, OnValueChanged);
        _targetDescriptor = null;
        WritePostscript();
    }

    private void UpdatePropertyBinding()
    {
        if (TargetObject == null)
            return;

        if (Property == null)
            return;

        _targetDescriptor?.RemoveValueChanged(TargetObject, OnValueChanged);
        // The property descriptor is cached so that we can remove a previously added value-changed handler, even if the Property changes.
        _targetDescriptor = DependencyPropertyDescriptor.FromProperty(Property, TargetObject.GetType());
        _targetDescriptor.AddValueChanged(TargetObject, OnValueChanged);
    }

    private void OnValueChanged(object? sender, EventArgs e)
    {
        if (TargetObject == null || Property == null)
            return;

        var targetValue = TargetObject.GetValue(Property);
        
        if (XamlValueComparer.Evaluate(targetValue, Value))
            ExecuteActions();
    }
}
