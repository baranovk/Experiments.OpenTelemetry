using Experiments.OpenTelemetry.Communication.Commands;
using Experiments.OpenTelemetry.Communication.Responses;
using UserInterface.Console.Generic;

namespace Experiments.OpenTelemetry.Configurator.Interaction;

internal sealed class PrintConfigurationParametersScenario : InteractionScenario
{
    public override async Task<Context> Execute(Context context)
    {
        context.UI.WriteMessage($"Sending {nameof(PrintConfigurationParametersCommand)}...");
        var response = (await Utility.SendCommandAsync(new PrintConfigurationParametersCommand()).ConfigureAwait(false));
        context.UI.WriteMessage("OK");

        context.UI
            .WriteMessage((response as TextResponse)?.Value
                ?? $"Invalid response: {response.GetType().Name}. Expected {typeof(TextResponse).Name} response.");

        return context with { CurrentScenario = new EntryPointScenario() };
    }
}
