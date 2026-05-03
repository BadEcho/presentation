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

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides a trigger that will execute a sequence of actions when bound data meets a specified condition.
/// </summary>
public sealed class DataTrigger : ActionSource<DataTrigger>
{
    /// <summary>
    /// Identifies the <see cref="Binding"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BindingProperty
        = DependencyProperty.Register(nameof(Binding),
                                      typeof(object),
                                      typeof(DataTrigger),
                                      new PropertyMetadata(OnBindingChanged));
    /// <summary>
    /// Identifies the <see cref="Value"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ValueProperty
        = DependencyProperty.Register(nameof(Value),
                                      typeof(object),
                                      typeof(DataTrigger),
                                      new PropertyMetadata(OnValueChanged));
    /// <summary>
    /// Gets or sets the binding that produces the property value of the data object.
    /// </summary>
    public object? Binding
    {
        get => GetValue(BindingProperty);
        set => SetValue(BindingProperty, value);
    }

    /// <summary>
    /// Gets or sets the value to be compared with the property value of the data object.
    /// </summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnBindingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        DataTrigger trigger = (DataTrigger)sender;

        trigger.EvaluateBinding();
    }

    private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        DataTrigger trigger = (DataTrigger)sender;

        trigger.EvaluateBinding();
    }

    private void EvaluateBinding()
    {
        if (XamlValueComparer.Evaluate(Binding, Value)) 
            ExecuteActions();
    }
}
