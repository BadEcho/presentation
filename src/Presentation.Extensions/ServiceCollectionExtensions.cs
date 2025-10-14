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

using System.Windows;
using BadEcho.Presentation.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace BadEcho.Presentation.Extensions;

/// <summary>
/// Provides extensions methods for setting up services that integrate the Bad Echo Presentation framework and Windows
/// Presentation Foundation with a hosted application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds services that enable support for hosting the specified Windows Presentation Foundation application.
    /// </summary>
    /// <typeparam name="TApplication">The type of <see cref="Application"/> to host.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to add services to.</param>
    /// <returns>The current <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddApplication<TApplication>(this IServiceCollection services)
        where TApplication : Application
    {
        Require.NotNull(services, nameof(services));

        services.AddSingleton<Application, TApplication>();
        services.AddSingleton<UserInterfaceContext>(
            sp => UserInterface.RunApplication(sp.GetRequiredService<Application>));

        services.AddHostedService<ApplicationHostedService>();

        services.AddSingleton<INavigationContextSource, ServiceNavigationContextSource>();
        services.AddSingleton<NavigationService>();

        return services;
    }
}
