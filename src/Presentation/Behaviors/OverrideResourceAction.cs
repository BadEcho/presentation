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
/// Locally overrides a resource on the framework-level element this action is attached to.
/// </summary>
/// <remarks>
/// This is useful when we want to customize parts of a control's template that are otherwise
/// uncustomizable (without duplicating the entire thing) by replacing the system/theme resources
/// it uses locally.
/// </remarks>
public sealed class OverrideResourceAction : BehaviorAction
{
    /// <summary>
    /// Gets or sets the name of the resource to override.
    /// </summary>
    public string Name
    { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overriding value of the resource. If null, the original value of the
    /// resource is restored.
    /// </summary>
    public object? Value
    { get; set; }

    /// <inheritdoc/>
    public override bool Execute(object? parameter)
    {
        if (TargetObject is not FrameworkElement element)
            return false;

        if (Value == null)
        {
            element.Resources.Remove(Name);
            return true;
        }

        element.Resources[Name] = Value;

        return true;
    }

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore()
        => new OverrideResourceAction();
}
