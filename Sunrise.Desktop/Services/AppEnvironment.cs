using System;
using Sunrise.Services;

namespace Sunrise.Desktop.Services;

internal sealed class AppEnvironment : IAppEnvironment
{
    public string MachineName => Environment.MachineName;
}
