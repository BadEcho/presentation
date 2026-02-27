
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

using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using BadEcho.Presentation.Properties;
using Window = System.Windows.Window;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides a behavior that, when attached to a target window, allows us to add a control to its non-client area.
/// </summary>
/// <remarks>
/// <para>
/// We are unable to restrict the attached properties' target object types to <see cref="Window"/> if we want to still
/// support their use at design-time. This is because the XAML designer does not use the <see cref="Window"/> type for
/// windows, but rather the internal <c>Microsoft.VisualStudio.XSurface.Wpf.Window</c> type, which does not derive from
/// <see cref="Window"/>.
/// </para>
/// <para>
/// In order to support both types, we allow this behavior to target their lowest common ancestor, which is
/// <see cref="ContentControl"/>.
/// </para>
/// </remarks>
public sealed class NonClientAreaBehavior : Behavior<ContentControl, Control>, IHandlerBypassable
{
    private const int GENERAL_CHANGES_REQUIRED = 3;

    private static readonly Thickness _BorderHighContrast = new(8, 2, 8, 8);
    private static readonly Thickness _ResizeBorderHighContrast = new(8, 2, 8, 8);
    private static readonly Thickness _BorderNormal = new(4, 0, 4, 4);
    private static readonly Thickness _ResizeBorderNormal = new(4);
    private static readonly CornerRadius _CornerRadius = new(12);

    /// <summary>
    /// Gets a mapping between target dependency objects and their non-client area configuration.
    /// </summary>
    private ConditionalWeakTable<ContentControl, NonClientAreaState> StateMap
    {
        get
        {
            ReadPreamble();

            return field;
        }
    } = [];

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// This will subscribe to a number of events on the target object, in preparation for adjust the object's content to include
    /// the non-client control. If there is already content on the window when this method is called, then the target object has
    /// already been initialized and loaded, in which case this method will immediately adjust the target object's content to
    /// include the non-client control.
    /// </para>
    /// <para>
    /// This will normally never be the case at run-time, as the window will normally not have its content set yet, assuming this
    /// behavior's attachment is due to its declaration in XAML. At design-time, however, this will often be the case, such as when
    /// the XAML for this behavior is added after the window has already been loaded with content defined. 
    /// </para>
    /// <para>
    /// This method also subscribes to changes to <see cref="ContentControl.ContentProperty"/> via its
    /// <see cref="DependencyPropertyDescriptor"/>, again mainly for supporting design-time-specific scenarios, but will also help
    /// cover any code that overwrites the content at run-time. This subscription, along with all other event subscriptions, are
    /// guaranteed to be removed when either the target object is unloading or this behavior is detached.
    /// </para>
    /// </remarks>
    protected override void OnValueAssociated(ContentControl targetObject, Control newValue)
    {
        Require.NotNull(targetObject, nameof(targetObject));
        Require.NotNull(newValue, nameof(newValue));

        var state = new NonClientAreaState(this);

        WritePreamble();
        StateMap.Add(targetObject, state);
        WritePostscript();

        SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
        targetObject.Loaded += HandleTargetLoaded;
        targetObject.Unloaded += HandleTargetUnloaded;
        
        if (targetObject.Content != null)
            AddNonClientControl(targetObject, newValue, state);

        DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, targetObject.GetType())
                                    .AddValueChanged(targetObject, OnContentChanged);

        if (targetObject is not Window window)
            return;

        window.Initialized += HandleTargetInitialized;
        window.Activated += HandleTargetActivated;
        window.Deactivated += HandleTargetDeactivated;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// This will unsubscribe from all events previously subscribed to, as well as remove the non-client control from
    /// the window, restoring its normal content. This is guaranteed to run when the target object unloads, if manual
    /// detachment has not occurred.
    /// </remarks>
    protected override void OnValueDisassociated(ContentControl targetObject, Control oldValue)
    {
        Require.NotNull(targetObject, nameof(targetObject));

        SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
        targetObject.Loaded -= HandleTargetLoaded;
        targetObject.Unloaded -= HandleTargetUnloaded;

        DependencyPropertyDescriptor.FromProperty(ContentControl.ContentProperty, targetObject.GetType())
                                    .RemoveValueChanged(targetObject, OnContentChanged);

        var state = GetState(targetObject);

        if (state.NonClientControlAdded)
            RemoveNonClientControl(targetObject, state);

        WritePreamble();
        StateMap.Remove(targetObject);
        WritePostscript();

        if (targetObject is not Window window)
            return;

        window.Initialized -= HandleTargetInitialized;
        window.Activated -= HandleTargetActivated;
        window.Deactivated -= HandleTargetDeactivated;
    }

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore()
        => new NonClientAreaBehavior();

    private static Thickness GetResizeBorderThickness(Window window)
    {
        if (SystemParameters.HighContrast)
        {
            return window.ResizeMode is ResizeMode.CanMinimize or ResizeMode.NoResize
                ? new Thickness(0)
                : _ResizeBorderHighContrast;
        }

        return window.ResizeMode is ResizeMode.CanMinimize or ResizeMode.NoResize
            ? new Thickness(0)
            : _ResizeBorderNormal;
    }

    private static void AddNonClientControl(ContentControl window, Control nonClientControl, NonClientAreaState state)
    {
        var content = (UIElement) window.Content;

        // Detach the current content from the window so we can attach it to our grid.
        window.Content = null;

        // We package the non-client control along with the existing content into a grid where they are separated.
        // The first row is the non-client area, the second row is the client area.
        if (!state.NonClientControlAdded)
        {
            state.AreasGrid.Children.Add(nonClientControl);
            Grid.SetRow(nonClientControl, 0);

            state.NonClientControlAdded = true;
        }
        else
        {
            if (state.AreasGrid.Children.Count > 1)
                state.AreasGrid.Children.RemoveAt(1);
        }

        if (null != content)
        {
            state.AreasGrid.Children.Add(content);
            Grid.SetRow(content, 1);
        }

        window.Content = state.FrameBorder;
    }

    private static void RemoveNonClientControl(ContentControl window, NonClientAreaState state)
    {
        UIElement? content = null;

        if (state.AreasGrid.Children.Count > 1)
            content = state.AreasGrid.Children[1];

        state.AreasGrid.Children.Clear();

        window.Content = content;

        state.NonClientControlAdded = false;
    }

    private static WindowChrome CreateChrome(Window window, NonClientAreaState state)
    {
        var chrome = new WindowChrome
                     {
                         CornerRadius = _CornerRadius,
                         GlassFrameThickness = new Thickness(-1),
                         ResizeBorderThickness = GetResizeBorderThickness(window),
                         UseAeroCaptionButtons = true
                     };

        WindowChrome.SetWindowChrome(window, chrome);

        UpdateWindowVisuals(window, state);

        return chrome;
    }

    private static void UpdateWindowVisuals(Window window, NonClientAreaState state)
    {   // There is a very small chance our chrome is null here, in the event we somehow have a user preference changed
        // system event before our window has become initialized.
        WindowChrome chrome = WindowChrome.GetWindowChrome(window);

        if (SystemParameters.HighContrast)
        {   // High contrast themes have very noticeable, thick borders whose colors we need to manage when the window's
            // active state changes.
            state.FrameBorder.SetResourceReference(Control.BorderBrushProperty,
                                                   window.IsActive
                                                       ? SystemColors.ActiveCaptionBrushKey
                                                       : SystemColors.InactiveCaptionBrushKey);
            if (!state.DelayBorderAdjustment)
                state.FrameBorder.BorderThickness = _BorderHighContrast;

            if (chrome != null)
            {
                chrome.CornerRadius = _CornerRadius;
                chrome.ResizeBorderThickness = GetResizeBorderThickness(window);
                chrome.NonClientFrameEdges = NonClientFrameEdges.None;
            }
        }
        else
        {
            state.FrameBorder.BorderBrush = Brushes.Transparent;

            if (!state.DelayBorderAdjustment)
                state.FrameBorder.BorderThickness = _BorderNormal;
            
            if (chrome != null)
            {
                chrome.CornerRadius = _CornerRadius;
                chrome.ResizeBorderThickness = GetResizeBorderThickness(window);
                chrome.NonClientFrameEdges = NonClientFrameEdges.Right | NonClientFrameEdges.Bottom | NonClientFrameEdges.Left;
            }
        }
    }

    private NonClientAreaState GetState(ContentControl targetObject)
    {
        return !StateMap.TryGetValue(targetObject, out NonClientAreaState? state)
            ? throw new InvalidOperationException(Strings.NoNonClientAreaStateForTarget)
            : state;
    }

    private void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {   // This event will be raised by a running message pump. So, most of the time we will already be running
        // on the dispatcher's thread. However, we'll still want to manually invoke via the dispatcher,
        // in the event that we have additional message pumps running (which is the case for several of my apps). 
        Dispatcher.Invoke(() =>
        {
            foreach (var (targetObject, state) in StateMap)
            {
                if (targetObject is not Window window)
                    return;

                var chrome = WindowChrome.GetWindowChrome(window);

                // In the event we are switching to a high contrast theme, we need to force the corner radius to
                // a different value before setting it back to the desired amount. Otherwise, the corner radius can
                // sometimes be lost during the transition, leaving us with pointy edges.
                chrome?.CornerRadius = new CornerRadius(0);

                /*
                This is a bit of a hack, but it's the only way I've been able to reliably get the content to be properly
                painted following a change to and from a high contrast theme.

                The thickness of the frame border needs to be adjusted to properly offset the content from the changing
                frame border sizes; however, if we do this while the frame is being adjusted, the content can end up not
                being properly aligned.

                The problem is that it's very difficult to tell when the window's frame is done being adjusted following
                the enabling/ disabling of a high contrast theme. This event handler will be called many times after such
                a change, and the only safe time to adjust the frame border's thickness is after the final invocation.

                From my testing, this event handler's final invocation (during a high contrast theme switch) is the third
                invocation using the "General" category in its event arguments. This resolves all issues very reliably
                on my system, however I suspect the behavior just described may differ between systems. More testing is required.
                */
                if (e.Category == UserPreferenceCategory.General)
                    state.GeneralChangesRemaining--;

                UpdateWindowVisuals(window, state);

                if (state.GeneralChangesRemaining == 0)
                    state.GeneralChangesRemaining = GENERAL_CHANGES_REQUIRED;
            }
        }, DispatcherPriority.Send);
    }

    private void HandleTargetLoaded(object sender, RoutedEventArgs e)
    {
        ContentControl contentControl = (ContentControl)sender;
        Control nonClientControl = GetAssociatedValue(contentControl);

        // We aren't working with an actual 'Window' during design-time, so, in that case, we bail out here.
        if (DesignerProperties.GetIsInDesignMode(contentControl))
            return;

        // Now that the non-client control's layout has been finalized, we can adjust the non-client area to the appropriate size.
        Window window = (Window) contentControl;
        NonClientAreaState state = GetState(window);

        WindowChrome chrome = WindowChrome.GetWindowChrome(window) ?? CreateChrome(window, state);

        chrome.CaptionHeight = nonClientControl.ActualHeight;
    }

    private void HandleTargetUnloaded(object sender, EventArgs e)
    {
        ContentControl contentControl = (ContentControl)sender;

        Detach(contentControl);
    }

    private void HandleTargetInitialized(object? sender, EventArgs e)
    {
        if (sender == null)
            return;

        Window window = (Window) sender;
        NonClientAreaState state = GetState(window);

        CreateChrome(window, state);
    }

    private void HandleTargetActivated(object? sender, EventArgs e)
    {
        if (sender == null)
            return;

        Window window = (Window) sender;
        NonClientAreaState state = GetState(window);

        UpdateWindowVisuals(window, state);
    }

    private void HandleTargetDeactivated(object? sender, EventArgs e)
    {
        if (sender == null)
            return;

        Window window = (Window) sender;
        NonClientAreaState state = GetState(window);

        UpdateWindowVisuals(window, state);
    }

    private void OnContentChanged(object? sender, EventArgs e)
    {
        if (sender == null || this.IsHandlingBypassed())
            return;

        ContentControl contentControl = (ContentControl)sender;
        NonClientAreaState state = GetState(contentControl);
        Control nonClientControl = GetAssociatedValue(contentControl);

        this.BypassHandlers(() => AddNonClientControl(contentControl, nonClientControl, state));
    }

    /// <summary>
    /// Provides state data for a target object a <see cref="NonClientAreaBehavior"/> instance is attached to.
    /// </summary>
    private sealed class NonClientAreaState
    {
        private readonly NonClientAreaBehavior _behavior;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonClientAreaState"/> class.
        /// </summary>
        /// <param name="behavior">The behavior this provides state data for.</param>
        public NonClientAreaState(NonClientAreaBehavior behavior)
        {
            _behavior = behavior;
            AreasGrid = new Grid();
            AreasGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.0, GridUnitType.Auto) });
            AreasGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.0, GridUnitType.Star) });

            FrameBorder = new Border
            {
                BorderBrush = Brushes.Transparent,
                Child = AreasGrid,
            };

            // We need to account for the resize border edges at run-time, since we own most edges of the frame.
            // We want to ignore this at design-time, however, as it just adds an empty and unnecessary space at our edges.
            if (!DesignerProperties.GetIsInDesignMode(FrameBorder))
                FrameBorder.BorderThickness = SystemParameters.HighContrast ? _BorderHighContrast : _BorderNormal;
        }

        /// <summary>
        /// Gets the outermost border that frames the entire target object.
        /// </summary>
        public Border FrameBorder
        {
            get
            {
                _behavior.ReadPreamble();
                return field;
            }
        }

        /// <summary>
        /// Gets the grid that contains the non-client and client areas of the target object.
        /// </summary>
        public Grid AreasGrid
        {
            get
            {
                _behavior.ReadPreamble();
                return field;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the control associated with the owning behavior has been added
        /// to the non-client area of the target object.
        /// </summary>
        public bool NonClientControlAdded
        {
            get
            {
                _behavior.ReadPreamble();
                return field;
            }
            set
            {
                _behavior.WritePreamble();
                field = value;
                _behavior.WritePostscript();
            }
        }

        /// <summary>
        /// Gets or sets the number of <see cref="UserPreferenceCategory.General"/> changes following a theme change
        /// remaining before the border thickness of <see cref="FrameBorder"/> can be adjusted.
        /// </summary>
        public int GeneralChangesRemaining
        {
            get
            {
                _behavior.ReadPreamble();
                return field;
            }
            set
            {
                _behavior.WritePreamble();
                field = value;
                DelayBorderAdjustment = field != 0;
                _behavior.WritePostscript();
            }
        } = GENERAL_CHANGES_REQUIRED;

        /// <summary>
        /// Gets or sets a value indicating if adjustments to the border thickness of <see cref="FrameBorder"/> should be delayed.
        /// </summary>
        public bool DelayBorderAdjustment
        {
            get
            {
                _behavior.ReadPreamble();
                return field;
            }
            // This should only be called within a WritePreamble/Postscript block.
            private set;
        }
    }
}