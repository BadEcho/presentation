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

using BadEcho.Presentation.Extensions.Properties;
using BadEcho.Presentation.Navigation;
using BadEcho.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace BadEcho.Presentation.Extensions;

/// <summary>
/// Provides a mechanism for retrieving data contexts for navigation content from an <see cref="IServiceProvider"/>.
/// </summary>
internal sealed class ServiceNavigationContextSource : INavigationContextSource
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceNavigationContextSource(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IViewModel GetViewModel(Type viewModelType)
    {
        Require.NotNull(viewModelType, nameof(viewModelType));

        if (_serviceProvider.GetRequiredService(viewModelType) is not IViewModel viewModel)
            throw new ArgumentException(Strings.NavigationTypeNotViewModel, nameof(viewModelType));

        return viewModel;
    }
}
