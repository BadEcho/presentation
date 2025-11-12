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

using System.Windows.Threading;
using BadEcho.Interop;
using BadEcho.Presentation.Extensions;
using BadEcho.Presentation.Windows;
using Xunit;
using Window = System.Windows.Window;

namespace BadEcho.Presentation.Tests;

public class PresentationWindowWrapperTests
{
    [Fact]
    public void SourceHook_PositionChanging_MessageIntercepted()
    {
        bool messageIntercepted = false;
        Window? window;

        UserInterface.RunUIFunction(
            () =>
            {
                window = new Window();
                window.Show();
                Assert.NotNull(window);
                
                var wrapper = new PresentationWindowWrapper(window.GetHandle());

                wrapper.AddCallback(Callback);
        
                window.Width = 200;

                Assert.True(messageIntercepted);

                Dispatcher.CurrentDispatcher.InvokeShutdown();
            },
            true);

        ProcedureResult Callback(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if ((WindowMessage) msg == WindowMessage.WindowPositionChanging)
            {
                messageIntercepted = true;
            }
            
            return new ProcedureResult(IntPtr.Zero, true);
        }
    }
}
