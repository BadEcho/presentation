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
    /// <remarks>
    /// This method will configure any running application host so that its lifetime is shared with the WPF application.
    /// When the last window is closed, the application host will stop.
    /// </remarks>
    public static IServiceCollection AddApplication<TApplication>(this IServiceCollection services)
        where TApplication : Application
    {
        return services.AddApplication<TApplication>(true);
    }

    /// <summary>
    /// Adds services that enable support for hosting the specified Windows Presentation Foundation application.
    /// </summary>
    /// <typeparam name="TApplication">The type of <see cref="Application"/> to host.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to add services to.</param>
    /// <param name="useWpfLifetime">
    /// Value indicating if the lifetime of the host is tied to the lifetime of the WPF application.
    /// </param>
    /// <returns>The current <see cref="IServiceCollection"/> instance so that additional calls can be chained.</returns>
    /// <remarks>
    /// If <paramref name="useWpfLifetime"/> is <c>true</c>, the WPF application will shut down when the last window
    /// is closed, and any running application host will stop.
    /// </remarks>
    public static IServiceCollection AddApplication<TApplication>(this IServiceCollection services, bool useWpfLifetime)
        where TApplication : Application
    {
        Require.NotNull(services, nameof(services));

        var shutdownMode = useWpfLifetime ? ShutdownMode.OnLastWindowClose : ShutdownMode.OnExplicitShutdown;

        services.AddSingleton<Application, TApplication>();
        services.AddSingleton(CreateContext);

        services.AddHostedService<ApplicationHostedService>();

        services.AddSingleton<INavigationContextSource, ServiceNavigationContextSource>();
        services.AddSingleton<NavigationService>();

        return services;

        UserInterfaceContext CreateContext(IServiceProvider serviceProvider)
        {
            return UserInterface.RunApplication(serviceProvider.GetRequiredService<Application>,
                                                app => app.ShutdownMode = shutdownMode);
        }
    }
}
