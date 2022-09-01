﻿//-----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2022 Bad Echo LLC. All rights reserved.
//
//		Bad Echo Technologies are licensed under the
//		GNU Affero General Public License v3.0.
//
//		See accompanying file LICENSE.md or a copy at:
//		https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
//-----------------------------------------------------------------------

namespace BadEcho.Presentation.Configuration;

/// <summary>
/// Provides configuration settings for a Bad Echo Presentation framework application.
/// </summary>
public sealed class PresentationConfiguration
{
    /// <summary>
    /// Get or sets the index of the monitor that the main window of the Bad Echo Presentation framework application should be
    /// initially launched on.
    /// </summary>
    /// <remarks>
    /// A monitor's index corresponds to where the monitor is in the arrangement defined in the user's display settings, with
    /// the lowest index being the leftmost monitor and the highest index being the rightmost.
    /// </remarks>
    public int LaunchDisplay
    { get; init; }
}