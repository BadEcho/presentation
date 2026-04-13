
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
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using BadEcho.Interop;
using BadEcho.Presentation.Extensions;
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
    private static readonly Thickness _BorderHighContrast = new(8, 2, 8, 8);
    private static readonly Thickness _ResizeBorderHighContrast = new(8, 2, 8, 8);
    private static readonly Thickness _BorderNormal = new(0);
    private static readonly Thickness _BorderNormalPadded = new(3,0,0,0);
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

        var state = new NonClientAreaState(this, targetObject);

        WritePreamble();
        StateMap.Add(targetObject, state);
        WritePostscript();

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
        return window.ResizeMode is ResizeMode.NoResize
            ? new Thickness(0)
            : SystemParameters.HighContrast
                ? _ResizeBorderHighContrast
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
        state.RemoveWrapper();

        window.Content = content;

        state.NonClientControlAdded = false;
    }

    private static WindowChrome CreateChrome(Window window)
    {
        var chrome = new WindowChrome
                     {
                         CornerRadius = _CornerRadius,
                         GlassFrameThickness = new Thickness(-1),
                         ResizeBorderThickness = GetResizeBorderThickness(window),
                         UseAeroCaptionButtons = true
                     };

        WindowChrome.SetWindowChrome(window, chrome);

        return chrome;
    }

    private static void UpdateWindowVisuals(Window window, NonClientAreaState state)
    {
        WindowChrome chrome = WindowChrome.GetWindowChrome(window);

        if (SystemParameters.HighContrast)
        {   // High contrast themes have very noticeable, thick borders whose colors we need to manage when the window's
            // active state changes.
            state.FrameBorder.SetResourceReference(Control.BorderBrushProperty,
                                                   window.IsActive
                                                       ? SystemColors.ActiveCaptionBrushKey
                                                       : SystemColors.InactiveCaptionBrushKey);
            
            state.FrameBorder.BorderThickness = _BorderHighContrast;

            // The corner radius needs to be "massaged" a bit in the event we have switched from a non-HC theme to an HC theme,
            // see documentation for the NextCornerRadius property for more information.
            chrome.CornerRadius = state.NextCornerRadius;
            state.NextCornerRadius = _CornerRadius;
            
            chrome.ResizeBorderThickness = GetResizeBorderThickness(window);
            chrome.NonClientFrameEdges = NonClientFrameEdges.None;
        }
        else
        {
            state.FrameBorder.BorderBrush = Brushes.Transparent;
            // The normal frame border thickness sometimes needs to be padded in response to system theme related events,
            // see documentation for the ThemeSettingsChanged property for more information.
            state.FrameBorder.BorderThickness = state.ThemeSettingsChanged ? _BorderNormalPadded : _BorderNormal;

            chrome.CornerRadius = _CornerRadius;
            chrome.ResizeBorderThickness = GetResizeBorderThickness(window);
            chrome.NonClientFrameEdges = NonClientFrameEdges.Right | NonClientFrameEdges.Bottom | NonClientFrameEdges.Left;
        }
    }

    private NonClientAreaState GetState(ContentControl targetObject)
    {
        return !StateMap.TryGetValue(targetObject, out NonClientAreaState? state)
            ? throw new InvalidOperationException(Strings.NoNonClientAreaStateForTarget)
            : state;
    }

    private void HandleTargetInitialized(object? sender, EventArgs e)
    {
        if (sender == null)
            return;

        Window window = (Window) sender;
        NonClientAreaState state = GetState(window);

        CreateChrome(window);
        UpdateWindowVisuals(window, state);
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

        WindowChrome chrome = WindowChrome.GetWindowChrome(window) ?? CreateChrome(window);
        UpdateWindowVisuals(window, state);

        chrome.CaptionHeight = nonClientControl.ActualHeight;
    }

    private void HandleTargetUnloaded(object sender, EventArgs e)
    {
        ContentControl contentControl = (ContentControl)sender;

        Detach(contentControl);
    }

    private void HandleTargetActivated(object? sender, EventArgs e)
    {
        if (sender == null)
            return;

        Window window = (Window) sender;
        NonClientAreaState state = GetState(window);

        // The chrome will normally have been created before an Activated event has fired, except
        // in the case of the window having already been initialized prior to the attachment of this behavior.
        if (WindowChrome.GetWindowChrome(window) == null)
            CreateChrome(window);

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
        private readonly WindowWrapper? _wrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonClientAreaState"/> class.
        /// </summary>
        /// <param name="behavior">The behavior this provides state data for.</param>
        /// <param name="targetObject">The target object the behavior is attached to.</param>
        public NonClientAreaState(NonClientAreaBehavior behavior, ContentControl targetObject)
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

            if (targetObject is Window window)
            {
                _wrapper = window.GetWrapper();
                _wrapper.AddCallback(WindowProcedure);
            }

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
        /// Gets or sets a value indicating if a system theme related change occurred while the behavior has been active.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will be set when the user switches between (non-HC) themes in their Personalization settings,
        /// or edits the currently active theme. When this happens, the window's content will be slightly offset
        /// to the left (I'm assuming due to some WPF bug having to do with custom non-client areas).
        /// </para>
        /// <para>
        /// We set this property to true to indicate to the behavior to pad the frame border's thickness to correct this offset.
        /// This additional padding needs to remain in effect for the remainder of the window's lifetime.
        /// </para>
        /// </remarks>
        public bool ThemeSettingsChanged
        {
            get
            {
                _behavior.ReadPreamble();
                return field;
            }
            private set
            {
                _behavior.WritePreamble();
                field = value;
                _behavior.WritePostscript();
            }
        }

        /// <summary>
        /// Gets or sets the corner radius value that should be applied to the non-client area during the next visuals update.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will be set to a temporary value if we just switched from a non-HC theme to an HC theme. When that occurs,
        /// we need to set the corner radius to a different value during the first visuals update pass before setting 
        /// it to the actual value during the next pass.
        /// </para>
        /// <para>
        /// If we don't set it to a temporary value after the theme changes, the edges will remain straight and pointy,
        /// even if we constantly set the corner radius to the same smooth value each time this is called.
        /// </para>
        /// </remarks>
        public CornerRadius NextCornerRadius
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
        /// Stops the interception of WndProc messages being sent to the behavior's target object.
        /// </summary>
        public void RemoveWrapper()
        {
            _behavior.WritePreamble();
            _wrapper?.RemoveCallback(WindowProcedure);
            _behavior.WritePostscript();
        }

        private ProcedureResult WindowProcedure(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            var result = new ProcedureResult(IntPtr.Zero, false);
            var hwndSource = HwndSource.FromHwnd(hWnd);

            if (hwndSource is not { RootVisual: Window window })
                return result;

            if ((WindowMessage)msg == WindowMessage.SystemColorChange)
            {
                ThemeSettingsChanged = true;
                UpdateWindowVisuals(window, this);
            }

            if ((WindowMessage) msg == WindowMessage.ThemeChanged)
            {
                NextCornerRadius = new CornerRadius(0);
                UpdateWindowVisuals(window, this);
            }

            return new ProcedureResult(IntPtr.Zero, false);
        }
    }
}