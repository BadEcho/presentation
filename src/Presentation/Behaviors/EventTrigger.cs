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
using BadEcho.Extensions;

using BadEcho.Presentation.Properties;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides an event trigger that will execute a sequence of actions upon the firing of a routed event on the target element
/// this component is attached to.
/// </summary>
public sealed class EventTrigger : ActionSource<EventTrigger>
{
    /// <summary>
    /// Identifies the <see cref="EventName"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty EventNameProperty
        = DependencyProperty.Register(nameof(EventName),
                                      typeof(string),
                                      typeof(EventTrigger),
                                      new FrameworkPropertyMetadata(OnEventNameChanged));

    private RoutedEvent? _routedEvent;

    /// <summary>
    /// Gets or sets the name of the routed event to execute actions in response to.
    /// </summary>
    public string? EventName
    {
        get => (string) GetValue(EventNameProperty);
        set => SetValue(EventNameProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        UpdateEventSubscription();
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (TargetObject == null || _routedEvent == null)
            return;

        WritePreamble();
        TargetObject.RemoveHandler(_routedEvent, (RoutedEventHandler) OnEvent);
        _routedEvent = null;
        WritePostscript();
    }

    private static void OnEventNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        EventTrigger trigger = (EventTrigger) sender;

        trigger.UpdateEventSubscription();
    }

    private void UpdateEventSubscription()
    {
        if (TargetObject == null)
            return;

        WritePreamble();

        if (_routedEvent != null)
        {
            TargetObject.RemoveHandler(_routedEvent, (RoutedEventHandler) OnEvent);
            _routedEvent = null;
        }

        if (!string.IsNullOrEmpty(EventName))
        {
            _routedEvent = EventManager.GetRoutedEvents().FirstOrDefault(e => e.Name == EventName)
                           ?? throw new InvalidOperationException(Strings.UnknownRoutedEvent.InvariantFormat(EventName));

            TargetObject.AddHandler(_routedEvent, (RoutedEventHandler) OnEvent);
        }

        WritePostscript();
    }

    private void OnEvent(object sender, RoutedEventArgs e)
        => ExecuteActions(e);
}
