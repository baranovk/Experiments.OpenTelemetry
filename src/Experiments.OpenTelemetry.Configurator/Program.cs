using Experiments.OpenTelemetry.Configurator.Interaction;
using UserInterface.Console.Generic;

namespace Experiments.OpenTelemetry.Configurator;

internal sealed class Program
{
    static async Task Main() => await UserInterfaceRunner.RunAsync(new EntryPointScenario()).ConfigureAwait(false);
}
