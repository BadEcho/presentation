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

using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using BadEcho.Interop;

namespace BadEcho.Presentation.Extensions;

/// <summary>
/// Provides a set of static methods intended to simplify the use of windows and dialogs.
/// </summary>
public static class WindowExtensions
{
    /// <summary>
    /// This represents the number of WPF's device-independent units corresponding to one inch of pixels on a physical
    /// display.
    /// </summary>
    private const double PIXEL_PER_INCH = 96.0;

    /// <summary>
    /// Moves this window to the primary display device.
    /// </summary>
    /// <param name="window">The current window to move.</param>
    public static void MoveToPrimaryDisplay(this Window window)
        => window.MoveToDisplay(Display.Primary);

    /// <summary>
    /// Moves this window to the provided display device.
    /// </summary>
    /// <param name="window">The current window to move.</param>
    /// <param name="targetDisplay">The display device to move the window to.</param>
    public static void MoveToDisplay(this Window window, Display targetDisplay)
    {
        Require.NotNull(window, nameof(window));
        Require.NotNull(targetDisplay, nameof(targetDisplay));
        
        var width = (int)window.Width;
        var height = (int)window.Height;

        // If the window state is Maximized, it must be set to normal before any changes are made to size and position.
        if (window.WindowState == WindowState.Maximized)
        {
            window.WindowState = WindowState.Normal;
            // We will want to set the state back to maximized after we're done changing the size, unless the window is meant
            // to allow for transparency. In that case, WPF happens to handle the Maximized state poorly, often assigning an
            // incorrect size and position for the window, causing it to overflow onto the next monitor.
            if (!window.AllowsTransparency)
            {   // We must wait for the state to completely change to normal before switching it back to Maximized, as switching it
                // back to Maximized after changing the position and size will result in our changes being canceled.
                window.StateChanged += HandleStateChangedToNormal;
            }

            width = targetDisplay.WorkingArea.Width;
            height = targetDisplay.WorkingArea.Height;
        }

        Rectangle targetArea = targetDisplay.WorkingArea with { Width = width, Height = height };
        Rectangle scaledTargetArea = NonDeviceArea(targetDisplay, targetArea);
        
        window.Left = scaledTargetArea.Left;
        window.Top = scaledTargetArea.Top;
        window.Width = scaledTargetArea.Width;
        window.Height = scaledTargetArea.Height;
    }

    /// <summary>
    /// Recenters this window on the monitor it's currently being displayed on.
    /// </summary>
    /// <param name="window">The current window to recenter.</param>
    public static void Recenter(this Window window)
    {
        Require.NotNull(window, nameof(window));

        Rectangle displayArea = window.FindDisplayArea();

        window.Left = displayArea.X + (displayArea.Width - window.ActualWidth) / 2;
        window.Top = displayArea.Y + (displayArea.Height - window.ActualHeight) / 2;
    }

    /// <summary>
    /// Finds the scaled working area for the display this window is currently being displayed on.
    /// </summary>
    /// <param name="window">The current window to find the display area for.</param>
    /// <returns>The scaled working area for the display <c>window</c> is currently being displayed on.</returns>
    public static Rectangle FindDisplayArea(this Window window)
    {
        Require.NotNull(window, nameof(window));

        Display windowDisplay = Display.FromWindow(window.GetSafeHandle());

        return NonDeviceArea(windowDisplay, windowDisplay.WorkingArea);
    }

    /// <summary>
    /// Gets the window handle for this window.
    /// </summary>
    /// <param name="window">The current window to get the handle for.</param>
    /// <returns>The window handle for <c>window</c>.</returns>
    public static IntPtr GetHandle(this Window window)
    {
        Require.NotNull(window, nameof(window));

        return new WindowInteropHelper(window).Handle;
    }

    /// <summary>
    /// Gets a non-owning <see cref="SafeHandle"/> instance for this window's handle.
    /// </summary>
    /// <param name="window">The current window to create a non-owning safe handle for.</param>
    /// <returns>A non-owning <see cref="WindowHandle"/> for the window handle of <c>window</c>.</returns>
    public static WindowHandle GetSafeHandle(this Window window)
    {
        Require.NotNull(window, nameof(window));

        IntPtr handle = window.GetHandle();

        return new WindowHandle(handle, false);
    }

    private static Rectangle NonDeviceArea(Display display, Rectangle referenceArea)
        => Display.IsDpiPerMonitor
            ? NonDeviceAreaUsingPerMonitorDpi(display, referenceArea)
            : NonDeviceAreaUsingSystemDpi(referenceArea);

    private static Rectangle NonDeviceAreaUsingPerMonitorDpi(Display display, Rectangle referenceArea)
    {
        double pixelCoefficient = PIXEL_PER_INCH / display.MonitorDpi;

        return ApplyPixelCoefficient(pixelCoefficient, referenceArea);
    }

    private static Rectangle NonDeviceAreaUsingSystemDpi(Rectangle referenceArea)
    {
        double pixelCoefficient = PIXEL_PER_INCH / Display.SystemDpi;

        return ApplyPixelCoefficient(pixelCoefficient, referenceArea);
    }

    private static Rectangle ApplyPixelCoefficient(double pixelCoefficient, Rectangle referenceArea)
    {
        var x = (int)(referenceArea.X * pixelCoefficient);
        var y = (int)(referenceArea.Y * pixelCoefficient);
        var width = (int)(referenceArea.Width * pixelCoefficient);
        var height = (int)(referenceArea.Height * pixelCoefficient);

        return new Rectangle(x, y, width, height);
    }

    private static void HandleStateChangedToNormal(object? sender, EventArgs e)
    {
        if (sender is not Window window)
            return;

        window.StateChanged -= HandleStateChangedToNormal;
        window.WindowState = WindowState.Maximized;
    }
}