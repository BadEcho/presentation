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
using BadEcho.Extensions;

using BadEcho.Presentation.Properties;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides an event trigger that will execute a sequence of actions upon the firing of a routed event on the target object
/// this component is attached to.
/// </summary>
[ContentProperty("Actions")]
public sealed class EventTrigger : AttachableComponent<FrameworkElement>
{
    /// <summary>
    /// Identifies the <see cref="Actions"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ActionsProperty
        = DependencyProperty.Register(nameof(Actions),
                                      typeof(BehaviorActionCollection),
                                      typeof(EventTrigger));
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
    /// Initializes a new instance of the <see cref="EventTrigger"/> class.
    /// </summary>
    public EventTrigger()
    {
        var actions = new BehaviorActionCollection();
        
        SetValue(ActionsProperty, actions);
    }

    /// <summary>
    /// Gets the collection of actions executed upon the firing of the specified routed event.
    /// </summary>
    public BehaviorActionCollection Actions
    {
        get => (BehaviorActionCollection) GetValue(ActionsProperty);
    }

    /// <summary>
    /// Gets or sets the name of the routed event to execute actions in response to.
    /// </summary>
    public string? EventName
    {
        get => (string)GetValue(EventNameProperty);
        set => SetValue(EventNameProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        UpdateEventSubscription();

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

        if (TargetObject == null || _routedEvent == null)
            return;

        TargetObject.RemoveHandler(_routedEvent, (RoutedEventHandler) OnEvent);
    }

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore() 
        => new EventTrigger();

    private static void OnEventNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        EventTrigger trigger = (EventTrigger) d;

        trigger.UpdateEventSubscription();
    }

    private void UpdateEventSubscription()
    {
        if (TargetObject == null)
            return;

        if (_routedEvent != null)
        {
            TargetObject.RemoveHandler(_routedEvent, (RoutedEventHandler) OnEvent);
            _routedEvent = null;
        }

        if (string.IsNullOrEmpty(EventName))
            return;

        _routedEvent = EventManager.GetRoutedEvents().FirstOrDefault(e => e.Name == EventName);
        
        if (_routedEvent == null)
            throw new InvalidOperationException(Strings.UnknownRoutedEvent.InvariantFormat(EventName));

        TargetObject.AddHandler(_routedEvent, (RoutedEventHandler)OnEvent);
    }

    private void OnEvent(object sender, RoutedEventArgs e)
    {
        BehaviorActionCollection actions = Actions;

        foreach (BehaviorAction action in actions)
        {
            if (!action.Execute())
                return;
        }
    }
}
