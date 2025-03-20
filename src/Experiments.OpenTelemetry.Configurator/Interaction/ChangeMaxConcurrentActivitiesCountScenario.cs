using Experiments.OpenTelemetry.Communication.Commands;

namespace Experiments.OpenTelemetry.Configurator.Interaction;

internal sealed class ChangeMaxConcurrentActivitiesCountScenario
    : ChangeIntegerConfigurationParameterScenario<ChangeMaxConcurrentActivitiesCountCommand>
{
    public ChangeMaxConcurrentActivitiesCountScenario()
        : base(maxConcurrentActivities => new ChangeMaxConcurrentActivitiesCountCommand(maxConcurrentActivities),
            "Enter max concurrent executing activities number or press 'Q' to cancel",
            "Invalid input. Please enter number or 'Q'", () => new EntryPointScenario())
    {
    }
}
