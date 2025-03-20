using Experiments.OpenTelemetry.Communication.Commands;
using UserInterface.Console.Generic;
using UserInterface.Console.Generic.Scenarios;
using Functional;
using static Functional.F;
using Experiments.OpenTelemetry.Communication.Responses;

namespace Experiments.OpenTelemetry.Configurator.Interaction;

internal class ChangeIntegerConfigurationParameterScenario<TChangeCommand> : HandleIntegerInputInteractionScenario
{
    public ChangeIntegerConfigurationParameterScenario(
        Func<int, TChangeCommand> generateChangeCommand,
        string prompt,
        string invalidInputMessage,
        Func<InteractionScenario> onCancelScenario,
        string? cancelKey = null) : base(prompt, invalidInputMessage, onCancelScenario,
            async (intInput, ctx) => await HandleInput(intInput, ctx, generateChangeCommand).ConfigureAwait(false), cancelKey)
    {
    }

    private static async Task<Context> HandleInput(IntegerInput maxConcurrentActivities, Context ctx,
        Func<int, TChangeCommand> generateChangeCommand)
        => (await ctx.UI.WriteEmpty()
                  .WriteMessage($"Sending {typeof(TChangeCommand).Name}...")
                  .Pipe(_ => TryAsync(
                                () => Utility.SendCommandAsync(generateChangeCommand(maxConcurrentActivities.Value)!)
                            ).RunAsync()
                  ).ConfigureAwait(false)
            )
            .Match(
                ex => ctx.UI.WriteEmpty().WriteMessage(ex.ToString()).Pipe(_ => ctx with { CurrentScenario = new EntryPointScenario() }),
                response => response is AckResponse
                                ? ctx.UI.WriteMessage("OK").Pipe(_ => ctx with { CurrentScenario = new EntryPointScenario() })
                                : ctx.UI.WriteEmpty()
                                        .WriteMessage($"Received response: {response}")
                                        .Pipe(_ => ctx with { CurrentScenario = new EntryPointScenario() })
            );
}
