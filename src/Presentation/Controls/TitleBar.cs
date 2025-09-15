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
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using BadEcho.Interop;
using BadEcho.Presentation.Properties;
using BadEcho.Presentation.Windows;
using Microsoft.Win32;
using Point = System.Windows.Point;
using Window = System.Windows.Window;

namespace BadEcho.Presentation.Controls;

/// <summary>
/// Provides a custom window title bar that features navigation controls usable in high contrast themes and can host content.
/// </summary>
/// <remarks>
/// This control also provides standard window title bar buttons when a high contrast theme is active. When such a theme is active,
/// at least while using WPF's Fluent "theme" within a custom frame, there are normally no title bar buttons present.
/// </remarks>
[TemplatePart(Name = MAIN_PANEL_NAME, Type = typeof(DockPanel))]
[TemplatePart(Name = NAVIGATION_PANEL_NAME, Type = typeof(StackPanel))]
[TemplatePart(Name = CONTENT_PRESENTER_NAME, Type = typeof(ContentPresenter))]
[TemplatePart(Name = HIGH_CONTRAST_PANEL_NAME, Type = typeof(StackPanel))]
[TemplatePart(Name = MINIMIZE_BUTTON_NAME, Type = typeof(Button))]
[TemplatePart(Name = MAXIMIZE_BUTTON_NAME, Type = typeof(Button))]
[TemplatePart(Name = MAXIMIZE_BUTTON_ICON_NAME, Type = typeof(TextBlock))]
[TemplatePart(Name = CLOSE_BUTTON_NAME, Type = typeof(Button))]
public sealed class TitleBar : ContentControl
{
    private const string MAIN_PANEL_NAME = "PART_MainPanel";
    private const string NAVIGATION_PANEL_NAME = "PART_NavigationPanel";
    private const string CONTENT_PRESENTER_NAME = "PART_ContentPresenter";
    private const string HIGH_CONTRAST_PANEL_NAME = "PART_HighContrastPanel";
    private const string MINIMIZE_BUTTON_NAME = "PART_MinimizeButton";
    private const string MAXIMIZE_BUTTON_NAME = "PART_MaximizeButton";
    private const string MAXIMIZE_BUTTON_ICON_NAME = "PART_MaximizeButtonIcon";
    private const string CLOSE_BUTTON_NAME = "PART_CloseButton";

    /// <summary>
    /// Identifies the <see cref="BackCommand"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty BackCommandProperty
        = DependencyProperty.Register(nameof(BackCommand),
                                      typeof(ICommand),
                                      typeof(TitleBar));
    /// <summary>
    /// Identifies the <see cref="CanGoBack"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CanGoBackProperty
        = DependencyProperty.Register(nameof(CanGoBack),
                                      typeof(bool),
                                      typeof(TitleBar));
    /// <summary>
    /// Identifies the <see cref="IsNavigationVisible"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsNavigationVisibleProperty
        = DependencyProperty.Register(nameof(IsNavigationVisible),
                                      typeof(bool),
                                      typeof(TitleBar),
                                      new FrameworkPropertyMetadata(true));
    /// <summary>
    /// Identifies the <see cref="ContentLocation"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ContentLocationProperty
        = DependencyProperty.Register(nameof(ContentLocation),
                                      typeof(TitleBarContentLocation),
                                      typeof(TitleBar),
                                      new FrameworkPropertyMetadata(TitleBarContentLocation.BeforeNavigation,
                                                                    OnContentLocationChanged),
                                      IsContentLocationValid);
    private NativeWindow? _native;

    private DockPanel? _mainPanel;
    private StackPanel? _navigationPanel;
    private ContentPresenter? _contentPresenter;

    private StackPanel? _highContrastPanel;
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private TextBlock? _maximizeButtonIcon;
    private Button? _closeButton;

    /// <summary>
    /// Initializes the <see cref="TitleBar"/> class.
    /// </summary>
    static TitleBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TitleBar), new FrameworkPropertyMetadata(typeof(TitleBar)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TitleBar"/> class.
    /// </summary>
    public TitleBar()
    {
        Loaded += HandleTitleBarLoaded;
        Unloaded += HandleTitleBarUnloaded;
    }

    /// <summary>
    /// Gets or sets the <see cref="ICommand"/> executed when the back button is clicked.
    /// </summary>
    public ICommand? BackCommand
    {
        get => (ICommand?) GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating if the back button is enabled.
    /// </summary>
    public bool CanGoBack
    {
        get => (bool) GetValue(CanGoBackProperty);
        set => SetValue(CanGoBackProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating if the navigation panel and its buttons are visible.
    /// </summary>
    public bool IsNavigationVisible
    {
        get => (bool) GetValue(IsNavigationVisibleProperty);
        set => SetValue(IsNavigationVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the location of this title bar's content relative to any navigation buttons.
    /// </summary>
    public TitleBarContentLocation ContentLocation
    {
        get => (TitleBarContentLocation) GetValue(ContentLocationProperty);
        set => SetValue(ContentLocationProperty, value);
    }

    /// <inheritdoc/>
    public override void OnApplyTemplate()
    {
        if (_minimizeButton != null)
            _minimizeButton.Click -= HandleMinimizeClick;

        if (_maximizeButton != null)
            _maximizeButton.Click -= HandleMaximizeClick;

        if (_closeButton != null)
            _closeButton.Click -= HandleCloseClick;

        base.OnApplyTemplate();

        _mainPanel = GetTemplateChild(MAIN_PANEL_NAME) as DockPanel;
        _navigationPanel = GetTemplateChild(NAVIGATION_PANEL_NAME) as StackPanel;
        _contentPresenter = GetTemplateChild(CONTENT_PRESENTER_NAME) as ContentPresenter;

        UpdateContentLocation();

        _highContrastPanel = GetTemplateChild(HIGH_CONTRAST_PANEL_NAME) as StackPanel;
        _minimizeButton = GetTemplateChild(MINIMIZE_BUTTON_NAME) as Button;

        if (_minimizeButton != null)
            _minimizeButton.Click += HandleMinimizeClick;

        _maximizeButton = GetTemplateChild(MAXIMIZE_BUTTON_NAME) as Button;

        if (_maximizeButton != null)
            _maximizeButton.Click += HandleMaximizeClick;

        _maximizeButtonIcon = GetTemplateChild(MAXIMIZE_BUTTON_ICON_NAME) as TextBlock;

        _closeButton = GetTemplateChild(CLOSE_BUTTON_NAME) as Button;

        if (_closeButton != null)
            _closeButton.Click += HandleCloseClick;
    }

    private static bool IsContentLocationValid(object value)
    {
        TitleBarContentLocation contentLocation = (TitleBarContentLocation) value;

        return contentLocation
            is TitleBarContentLocation.BeforeNavigation or TitleBarContentLocation.AfterNavigation;
    }
    
    private static void OnContentLocationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        TitleBar titleBar = (TitleBar) sender;

        titleBar.UpdateContentLocation();
    }   

    private void UpdateContentLocation()
    {
        Dock contentPresenterDock = ContentLocation == TitleBarContentLocation.BeforeNavigation
            ? Dock.Left
            : Dock.Right;

        Dock navigationPanelDock = ContentLocation == TitleBarContentLocation.BeforeNavigation
            ? Dock.Right
            : Dock.Left;

        if (_contentPresenter != null)
            DockPanel.SetDock(_contentPresenter, contentPresenterDock);

        if (_navigationPanel != null)
            DockPanel.SetDock(_navigationPanel, navigationPanelDock);
    }

    private void UpdateHost(Window host)
    {
        if (_mainPanel != null) 
            _mainPanel.Margin = host.WindowState == WindowState.Maximized ? new Thickness(6, 6, 6, 0) : default;

        if (_highContrastPanel != null) 
            _highContrastPanel.Visibility = SystemParameters.HighContrast ? Visibility.Visible : Visibility.Collapsed;

        SetContentWidth();
    }

    private void SetContentWidth()
    {
        if (_mainPanel == null)
            return;

        if (SystemParameters.HighContrast)
        {
            if (_highContrastPanel == null)
                return;

            Point highContrastPanelLocation = _highContrastPanel.TransformToAncestor(this)
                                                                .Transform(new Point(0, 0));

            _mainPanel.Width = highContrastPanelLocation.X - _mainPanel.Margin.Left - _mainPanel.Margin.Right;
        }
        else
        {
            if (_native == null)
                return;

            Display display = Display.FromWindow(_native.Handle);

            Rectangle captionButtonBounds = _native.CaptionButtonBounds;
            _mainPanel.Width = (captionButtonBounds.Left / display.ScaleFactor) - _mainPanel.Margin.Left - _mainPanel.Margin.Right;
        }
    }

    private void HandleTitleBarLoaded(object sender, RoutedEventArgs e)
    {   // The high contrast panel is always enabled during design-time. Additionally, all controls will be rendered as being in the client
        // area, so we just use the high contrast panel's size to size the main panel.
        if (DesignerProperties.GetIsInDesignMode(this) && _mainPanel != null && _highContrastPanel != null)
            _mainPanel.Width = ActualWidth - _highContrastPanel.ActualWidth;

        Window? host = Window.GetWindow(this);

        if (host == null)
            return;

        SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;

        IntPtr handle = new WindowInteropHelper(host).Handle;

        _native = new NativeWindow(new PresentationWindowWrapper(handle));

        host.Activated += HandleHostActivated;
        host.SizeChanged += HandleHostSizeChanged;

        UpdateHost(host);
    }

    private void HandleTitleBarUnloaded(object sender, RoutedEventArgs e)
    {
        Window? host = Window.GetWindow(this);

        if (host == null)
            return;

        SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
        host.Activated -= HandleHostActivated;
        host.SizeChanged -= HandleHostSizeChanged;
    }

    private void HandleHostActivated(object? sender, EventArgs e)
    {
        Window? host = (Window?)sender;

        if (host == null)
            throw new InvalidOperationException(Strings.WindowEventNoEventSender);

        UpdateHost(host);
    }

    private void HandleHostSizeChanged(object sender, SizeChangedEventArgs e)
        => UpdateHost((Window) sender);

    private void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {   // This event will be raised by a running message pump. So, most of the time we will already be running
        // on the dispatcher's thread. However, we'll still want to manually invoke via the dispatcher,
        // in the event that we have additional message pumps running (which is the case for several of my apps). 
        Dispatcher.Invoke(() =>
        {
            Window? host = Window.GetWindow(this);

            if (host == null)
                return;

            UpdateHost(host);
        });
    }

    private void HandleMinimizeClick(object sender, RoutedEventArgs e)
    {
        Window? host = Window.GetWindow(this);

        if (host != null)
            host.WindowState = WindowState.Minimized;
    }

    private void HandleMaximizeClick(object sender, RoutedEventArgs e)
    {
        Window? host = Window.GetWindow(this);

        if (host == null)
            return;

        if (host.WindowState == WindowState.Maximized)
        {
            host.WindowState = WindowState.Normal;

            if (_mainPanel != null)
                _mainPanel.Margin = default;

            if (_maximizeButtonIcon != null)
                _maximizeButtonIcon.Text = "\uE922";
        }
        else
        {
            host.WindowState = WindowState.Maximized;

            if (_mainPanel != null)
                _mainPanel.Margin = new Thickness(6,6,6,0);

            if (_maximizeButtonIcon != null)
                _maximizeButtonIcon.Text = "\uE923";
        }
    }

    private void HandleCloseClick(object sender, RoutedEventArgs e)
    {
        Window? host = Window.GetWindow(this);

        host?.Close();
    }
}
