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

using BadEcho.Logging;
using BadEcho.Presentation.ViewModels;
using BadEcho.Presentation.Properties;

namespace BadEcho.Presentation.Navigation;

/// <summary>
/// Provides support for navigating between views.
/// </summary>
public sealed class NavigationService
{
    private readonly Stack<Type> _back = new();
    private readonly Stack<Type> _forward = new();

    private readonly INavigationContextSource _contextSource;
    private INavigationHost? _host;
    private Type? _currentType;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="contextSource">Source to use for retrieving navigation content data contexts.</param>
    public NavigationService(INavigationContextSource contextSource)
    {
        _contextSource = contextSource;
    }

    /// <summary>
    /// Occurs when navigation is requested.
    /// </summary>
    public event EventHandler<EventArgs<Type>>? Navigating;

    /// <summary>
    /// Gets a value indicating if there are entries in back navigation history that can be navigated to.
    /// </summary>
    public bool CanNavigateBack
        => _back.Count == 0;

    /// <summary>
    /// Gets a value indicating if there are entries in forward navigation history that can be navigated to.
    /// </summary>
    public bool CanNavigateForward
        => _forward.Count == 0;

    /// <summary>
    /// Sets the host that will facilitate the display of content being navigated to.
    /// </summary>
    /// <param name="host">The <see cref="INavigationHost"/> to receive navigation content.</param>
    public void SetHost(INavigationHost host)
    {
        _host = host;
        
        _back.Clear();
        _forward.Clear();
    }

    /// <summary>
    /// Navigates to content targeted by the specified view model type.
    /// </summary>
    /// <typeparam name="T">The type of view model targeting the content to navigate to.</typeparam>
    public void Navigate<T>()
        => Navigate(typeof(T));

    /// <summary>
    /// Navigates to content targeted by the specified view model type.
    /// </summary>
    /// <param name="viewModelType">The type of view model targeting the content to navigate to.</param>
    public void Navigate(Type viewModelType)
        => Navigate(viewModelType, true);

    /// <summary>
    /// Navigates to the most recent entry in back navigation history.
    /// </summary>
    public void NavigateBack()
    {
        if (CanNavigateBack)
            throw new InvalidOperationException(Strings.EmptyBackNavigationHistory);
        
        Type previousType = _back.Pop();

        if (_currentType != null)
            _forward.Push(_currentType);

        Navigate(previousType, false);
    }

    /// <summary>
    /// Navigates to the most recent entry in forward navigation history.
    /// </summary>
    public void NavigateForward()
    {
        if (CanNavigateForward)
            throw new InvalidOperationException(Strings.EmptyForwardNavigationHistory);
        
        Type nextType = _forward.Pop();

        Navigate(nextType, true);
    }
    
    private void Navigate(Type viewModelType, bool recordHistory)
    {
        if (_host == null)
        {
            Logger.Debug(Strings.NavigationServiceNoHost);
            return;
        }
        
        if (recordHistory && _currentType != null)
            _back.Push(_currentType);

        _currentType = viewModelType;

        IViewModel viewModel = _contextSource.GetViewModel(viewModelType);
        OnNavigating(viewModelType);

        _host.CurrentViewModel = viewModel;
    }

    private void OnNavigating(Type viewModelType)
        => Navigating?.Invoke(this, new EventArgs<Type>(viewModelType));
}

