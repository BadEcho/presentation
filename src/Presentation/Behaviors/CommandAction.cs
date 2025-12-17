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
using System.Windows.Input;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides an action that, when executed, will execute a bound <see cref="ICommand"/> instance if possible.
/// </summary>
public sealed class CommandAction : BehaviorAction
{
    /// <summary>
    /// Identifies the <see cref="Command"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CommandProperty
        = DependencyProperty.Register(nameof(Command),
                                      typeof(ICommand),
                                      typeof(CommandAction));
    /// <summary>
    /// Identifies the <see cref="Parameter"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ParameterProperty
        = DependencyProperty.Register(nameof(Parameter),
                                      typeof(object),
                                      typeof(CommandAction));

    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> instance that will be executed when this action is executed.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?) GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the data used by <see cref="Command"/>.
    /// </summary>
    public object? Parameter
    {
        get => (object?) GetValue(ParameterProperty);
        set => SetValue(ParameterProperty, value);
    }

    /// <inheritdoc/>
    public override bool Execute(object? parameter)
    {
        if (Command == null)
            return false;

        if (Parameter != null)
            parameter = Parameter;

        if (!Command.CanExecute(parameter))
            return false;

        Command.Execute(parameter);

        return true;
    }

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore() 
        => new CommandAction();
}