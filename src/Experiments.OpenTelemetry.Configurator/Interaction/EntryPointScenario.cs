using UserInterface.Console.Generic;

namespace Experiments.OpenTelemetry.Configurator.Interaction;

internal sealed class EntryPointScenario : InteractionScenario
{
    public EntryPointScenario()
        : base([
            new UserInterface.Console.Generic.Interaction("1", "Print current configuration parameters", new PrintConfigurationParametersScenario()),
            new UserInterface.Console.Generic.Interaction("2", "Set max concurrent executing activities", new ChangeMaxConcurrentActivitiesCountScenario()),
        ])
    {
    }
}
