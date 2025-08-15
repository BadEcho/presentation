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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BadEcho.Presentation.Hosting.Tests;

public class Startup
{
    public void ConfigureHostApplicationBuilder(IHostApplicationBuilder builder) 
    {
        builder.Services.AddTransient<TestService>();
        builder.Services.AddApplication<App>();
    }
}
