//-----------------------------------------------------------------------
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
//-----------------------------------------------------------------------

using System.Windows.Interop;
using BadEcho.Presentation.Properties;
using BadEcho.Interop;
using BadEcho.Presentation.Extensions;

namespace BadEcho.Presentation.Windows;

/// <summary>
/// Provides a wrapper around an <c>HWND</c> of a window created by WPF.
/// </summary>
/// <suppressions>
/// ReSharper disable RedundantAssignment
/// </suppressions>
public sealed class PresentationWindowWrapper : WindowWrapper
{
    private readonly HwndSource _source;

    private bool _sourceHooked;

    /// <summary>
    /// Initializes a new instance of the <see cref="PresentationWindowWrapper"/> class.
    /// </summary>
    /// <param name="handle">A handle for a window created by WPF.</param>
    public PresentationWindowWrapper(IntPtr handle)
        : base(WindowHandle.InvalidHandle)
    {
        var source = HwndSource.FromHwnd(handle);
        
        _source = source
            ?? throw new ArgumentException(Strings.WindowNotPresentation, nameof(handle));

        Handle = source.GetSafeHandle();
    }

    /// <inheritdoc/>
    protected override void OnCallbackAdded(WindowProcedure addedCallback)
    {
        base.OnCallbackAdded(addedCallback);

        if (_sourceHooked)
            return;

        _source.AddHook(SourceHook);

        _sourceHooked = true;
    }

    /// <inheritdoc/>
    protected override void OnDestroyingWindow()
    {
        base.OnDestroyingWindow();

        if (!_sourceHooked)
            return;
        
        _source.RemoveHook(SourceHook);
        _sourceHooked = false;
    }

    private IntPtr SourceHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {   
        ProcedureResult result = WindowProcedure(hWnd, (uint)msg, wParam, lParam);
        handled = result.Handled;

        return result.LResult;
    }
}