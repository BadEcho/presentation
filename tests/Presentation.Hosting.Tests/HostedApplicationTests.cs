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

namespace BadEcho.Presentation.Hosting.Tests;

public class HostedApplicationTests
{
    private readonly Application _application;

    public HostedApplicationTests(Application application)
    {
        _application = application;
    }

    [Fact]
    public void ApplicationCurrent_ComparedToInjected_IsSame()
    {
        Assert.Equal(Application.Current, _application);
    }

    [Fact]
    public void ApplicationProperties_TestService_ReturnsInitialized()
    {
        Assert.True(_application.Properties.Contains(nameof(TestService)));
        Assert.NotNull(_application.Properties[nameof(TestService)]);
    }
}
