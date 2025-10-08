using Experiments.OpenTelemetry.Communication.Commands;
using Experiments.OpenTelemetry.Communication.Responses;
using Functional;
using UserInterface.Console.Generic;
using static Functional.F;

namespace Experiments.OpenTelemetry.Configurator.Interaction;

internal sealed class PrintConfigurationParametersScenario : InteractionScenario
{
    public override async Task<Context> Execute(Context context)
        => (await context.UI
                .WriteMessage($"Sending {nameof(PrintConfigurationParametersCommand)}...")
                .Pipe(_ => TryAsync(() => Utility.SendCommandAsync(new PrintConfigurationParametersCommand())).RunAsync()).ConfigureAwait(false)
            )
            .Match(
                ex => context.UI.WriteEmpty().WriteMessage(ex.ToString()).Pipe(_ => context with { CurrentScenario = new EntryPointScenario() }),
                response => response is TextResponse txtResponse
                                ? context.UI.WriteEmpty()
                                            .WriteMessage(txtResponse.Value).Pipe(_ => context with { CurrentScenario = new EntryPointScenario() })
                                : context.UI.WriteEmpty()
                                        .WriteMessage($"Invalid response was received: {response}")
                                        .Pipe(_ => context with { CurrentScenario = new EntryPointScenario() })
            );
}
