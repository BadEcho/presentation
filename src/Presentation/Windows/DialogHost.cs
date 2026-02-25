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

using BadEcho.Interop;
using BadEcho.Presentation.Extensions;
using BadEcho.Presentation.Selectors;
using BadEcho.Presentation.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;


namespace BadEcho.Presentation.Windows;

/// <summary>
/// Provides a host a window that acts as a modern modal dialog on top of a parent, owning window.
/// </summary>
/// <remarks>
/// This will spawn a window that fits snuggly within the confines of a parent window. Both dialog and parent windows can be resized
/// as a single entity while the dialog is open. Clicking outside the dialog window onto the parent window (other than the toolbar)
/// will cause the dialog to close.
/// </remarks>
public sealed class DialogHost
{
    private readonly System.Windows.Window _owner;
    private readonly System.Windows.Window _dialog;

    private PresentationWindowWrapper? _ownerWrapper;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogHost"/> class.
    /// </summary>
    /// <param name="owner">The parent window.</param>
    public DialogHost(System.Windows.Window owner)
    {
        _owner = owner;
        var content = new UserControl
                   {
                       ContentTemplateSelector = new ViewContextTemplateSelector()
                   };

        // Bind to the source itself.
        var contentBinding = new Binding();
        BindingOperations.SetBinding(content, ContentControl.ContentProperty, contentBinding);

        _dialog = new Window
        {
            ResizeMode = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = _owner,
            Content = content,
            ShowInTaskbar = false,
            // Remove the icon from the top corner of the dialog.
            Icon = new DrawingImage()
        };
        
        _dialog.LocationChanged += HandleDialogLocationChanged;
        _dialog.Closed += HandleDialogClosed;

        _owner.SizeChanged += HandleOwnerSizeChanged;
        _owner.LocationChanged += HandleOwnerLocationChanged;
        
        if (_owner.IsInitialized)
            HookWindow();
        else
            _owner.SourceInitialized += HandleOwnerSourceInitialized;
    }

    /// <summary>
    /// Occurs when the dialog window has closed.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Gets or sets the amount of padding between the parent and child window.
    /// </summary>
    public double Padding
    {
        get;
        set
        {
            field = value;

            _dialog.Width = _owner.Width - field;
            _dialog.Height = _owner.Height - field;
        }
    }

    /// <summary>
    /// Displays the dialog using the provided view model as its data context.
    /// </summary>
    /// <param name="viewModel">The view model to use as the data context.</param>
    public void Show(IViewModel viewModel)
    {
        _owner.Effect = new BlurEffect { Radius = 10.0 };
        _owner.IsHitTestVisible = false;
        
        _dialog.Show();

        // This instance will be kept alive by the dialog's HwndSource until its closure.
        _ = new NativeWindow(_dialog.GetWrapper())
            {
                DisableContextMenu = true
            };

        _dialog.DataContext = viewModel;
    }

    private void HookWindow()
    {
        _ownerWrapper = _owner.GetWrapper();

        _ownerWrapper.AddCallback(OwnerWindowProcedure);
    }

    private void HandleDialogClosed(object? sender, EventArgs e)
    {
        _owner.SizeChanged -= HandleOwnerSizeChanged;
        _owner.LocationChanged -= HandleOwnerLocationChanged;
        _owner.SourceInitialized -= HandleOwnerSourceInitialized;

        _ownerWrapper?.RemoveCallback(OwnerWindowProcedure);

        _owner.Effect = null;
        _owner.IsHitTestVisible = true;

        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void HandleOwnerSourceInitialized(object? sender, EventArgs e) 
        => HookWindow();

    private void HandleOwnerSizeChanged(object sender, SizeChangedEventArgs e)
    {   // If the size of the parent changes, we'll change along with it.
        var heightDiff = e.NewSize.Height - e.PreviousSize.Height;
        var widthDiff = e.NewSize.Width - e.PreviousSize.Width;

        _dialog.Height += heightDiff;
        _dialog.Width += widthDiff;
    }

    private void HandleOwnerLocationChanged(object? sender, EventArgs e)
    {   // If the dialog is active, then the dialog is being dragged; let its own event handler propagate the changes.
        if (_dialog.IsActive)
            return;

        _dialog.Left = _owner.Left + Padding / 2;
        _dialog.Top = _owner.Top + Padding / 2;
    }

    private void HandleDialogLocationChanged(object? sender, EventArgs e)
    {   // If the dialog isn't active, then the owner is being dragged; let its own event handler propagate the changes.
        if (!_dialog.IsActive)
            return;

        _owner.Left = _dialog.Left - Padding / 2;
        _owner.Top = _dialog.Top - Padding / 2;
    }

    private ProcedureResult OwnerWindowProcedure(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        WindowMessage message = (WindowMessage) msg;

        switch (message)
        {   
            case WindowMessage.ExitSizeMove:
                // Occurs when the user releases the mouse button after resizing the parent window.
                _dialog.Activate();
                break;

            case WindowMessage.LeftButtonUp:
                // Clicking anywhere on the parent's client area will close this dialog.
                // Hit testing on the parent window is disabled (i.e., it cannot be clicked on) while the dialog is visible,
                // so we have to intercept mouse click messages at this level, as the normal WPF mouse events will not fire.
                _dialog.Close();
                break;
        }

        return new ProcedureResult(IntPtr.Zero, false);
    }
}
