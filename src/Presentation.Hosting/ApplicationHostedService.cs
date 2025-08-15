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
using Microsoft.Extensions.Hosting;

namespace BadEcho.Presentation.Hosting;

/// <summary>
/// Provides a hosted service that runs a WPF application.
/// </summary>
internal sealed class ApplicationHostedService : IHostedService
{
    private readonly UserInterfaceContext _context;
    private readonly IHostApplicationLifetime _lifetime;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationHostedService"/> class.
    /// </summary>
    public ApplicationHostedService(UserInterfaceContext context, IHostApplicationLifetime lifetime)
    {
        _context = context;
        _lifetime = lifetime;
        
        _context.Completed += HandleContextCompleted;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _context.Start();

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_context.IsExecuting)
            return;

        await _context.Dispatcher.InvokeAsync(() =>
        {
            Application.Current?.Shutdown();
        });
    }

    private void HandleContextCompleted(object? sender, EventArgs e)
    {
        _lifetime.StopApplication();
    }
}
