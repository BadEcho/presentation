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
public sealed class UserInterfaceContext
{
    private const int HRESULT_DISPATCHER_SHUTDOWN = unchecked((int)0x80131509);

    private readonly Action _uiFunction;
    private readonly Thread _uiThread;

    private Dispatcher? _dispatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInterfaceContext"/> class.
    /// </summary>
    /// <param name="uiFunction">The function to run in a UI-appropriate context.</param>
    public UserInterfaceContext(Action uiFunction)
    {
        Require.NotNull(uiFunction, nameof(uiFunction));

        _uiFunction = uiFunction;
        
        _uiThread = new Thread(UIFunctionRunner)
                    {
                        IsBackground = true
                    };
    
        _uiThread.SetApartmentState(ApartmentState.STA);
    }

    /// <summary>
    /// Occurs when this context's UI-related functionality has finished executing.
    /// </summary>
    public event EventHandler<EventArgs>? Completed;

    /// <summary>
    /// Gets a value indicating if this context's UI-related functionality is executing.
    /// </summary>
    public bool IsExecuting
    { get; private set; }

    /// <summary>
    /// Gets the <see cref="System.Windows.Threading.Dispatcher"/> instance running within this context.
    /// </summary>
    public Dispatcher Dispatcher
        => _dispatcher ?? throw new InvalidOperationException(Strings.ContextHasNoDispatcher);

    /// <summary>
    /// Starts the execution of UI-related functionality.
    /// </summary>
    public void Start()
        => _uiThread.Start();

    /// <summary>
    /// Blocks the calling thread until the UI thread represented by this context exits.
    /// </summary>
    public void Join()
        => _uiThread.Join();

    private void UIFunctionRunner()
    {
        try
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            
            var context = new DispatcherSynchronizationContext(_dispatcher);
            SynchronizationContext.SetSynchronizationContext(context);
            
            IsExecuting = true;

            _uiFunction();

            IsExecuting = false;

            Completed?.Invoke(this, EventArgs.Empty);
        }
        catch (InvalidOperationException invalidEx)
        {
            if (invalidEx.HResult != HRESULT_DISPATCHER_SHUTDOWN)
                throw;

            Logger.Debug(Strings.ContextDispatcherManuallyShutdown);
        }
        catch (EngineException engineEx)
        {
            if (!engineEx.IsProcessed)
                Logger.Critical(Strings.ContextDispatcherError, engineEx.InnerException ?? engineEx);
        }
    }
}
