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
using BadEcho.Logging;
using BadEcho.Presentation.Properties;

namespace BadEcho.Presentation;

/// <summary>
/// Provides a context for hosting UI components.
/// </summary>
public sealed class UserInterfaceContext : IDisposable
{
    private const int HRESULT_DISPATCHER_SHUTDOWN = unchecked((int)0x80131509);

    private readonly ManualResetEventSlim _start = new();
    private readonly Action _uiFunction;
    private readonly Thread _uiThread;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInterfaceContext"/> class.
    /// </summary>
    /// <param name="uiFunction">The function to run in a UI appropriate context.</param>
    public UserInterfaceContext(Action uiFunction)
    {
        Require.NotNull(uiFunction, nameof(uiFunction));

        _uiFunction = uiFunction;

        _uiThread = new Thread(UIFunctionRunner)
                    {
                        IsBackground = true
                    };

        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.Start();
    }

    /// <summary>
    /// Starts the execution of UI-related functionality.
    /// </summary>
    public void Start()
        => _start.Set();

    /// <summary>
    /// Blocks the calling thread until the UI thread represented by this context exits.
    /// </summary>
    public void Join()
        => _uiThread.Join();

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _start.Dispose();

        _disposed = true;
    }

    private void UIFunctionRunner()
    {
        try
        {
            var context = new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher);

            SynchronizationContext.SetSynchronizationContext(context);

            _start.Wait();

            _uiFunction();
        }
        catch (InvalidOperationException invalidEx)
        {
            if (invalidEx.HResult != HRESULT_DISPATCHER_SHUTDOWN)
                throw;

            Logger.Debug(Strings.BadEchoDispatcherManuallyShutdown);
        }
        catch (EngineException engineEx)
        {
            if (!engineEx.IsProcessed)
                Logger.Critical(Strings.BadEchoDispatcherError, engineEx.InnerException ?? engineEx);
        }
    }
}
