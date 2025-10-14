using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: Guid("73c5ac3c-81ad-4959-a81f-b120c216fb7e")]

[assembly: SuppressMessage("Performance",
                           "CA1812",
                           Scope = "type",
                           Target = "~T:BadEcho.Presentation.Extensions.ApplicationHostedService",
                           Justification = "This is instantiated by the dependency injection container.")]

[assembly: SuppressMessage("Performance",
                           "CA1812",
                           Scope = "type",
                           Target = "~T:BadEcho.Presentation.Extensions.ServiceNavigationContextSource",
                           Justification = "This is instantiated by the dependency injection container.")]
