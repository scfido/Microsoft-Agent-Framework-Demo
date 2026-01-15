using Microsoft.Agents.AI.Workflows;

namespace MafDemo.Judge;

/// <summary>
/// 通过人机交互进行数字猜测游戏的工作流。
/// 演示了如何使用请求端口与外部世界进行交互。
/// </summary>
internal class JudgeWorkflow
{
    /// <summary>
    /// Get a workflow that plays a number guessing game with human-in-the-loop interaction.
    /// An input port allows the external world to provide inputs to the workflow upon requests.
    /// </summary>
    private static Workflow Build()
    {
        // Create the executors
        RequestPort numberRequestPort = RequestPort.Create<NumberSignal, int>("GuessNumber");
        JudgeExecutor judgeExecutor = new(42);

        // Build the workflow by connecting executors in a loop
        return new WorkflowBuilder(numberRequestPort)
            .AddEdge(numberRequestPort, judgeExecutor)
            .AddEdge(judgeExecutor, numberRequestPort)
            .WithOutputFrom(judgeExecutor)
            .Build();
    }

    internal static async Task RunAsync()
    {
        // Create the workflow
        var workflow = Build();

        // Execute the workflow
        await using StreamingRun handle = await InProcessExecution.StreamAsync(workflow, NumberSignal.Init);
        await foreach (WorkflowEvent evt in handle.WatchStreamAsync())
        {
            switch (evt)
            {
                case RequestInfoEvent requestInputEvt:
                    // Handle `RequestInfoEvent` from the workflow
                    ExternalResponse response = HandleExternalRequest(requestInputEvt.Request);
                    await handle.SendResponseAsync(response);
                    break;

                case WorkflowOutputEvent outputEvt:
                    // The workflow has yielded output
                    Console.WriteLine($"Workflow completed with result: {outputEvt.Data}");
                    return;
            }
        }
    }

    private static ExternalResponse HandleExternalRequest(ExternalRequest request)
    {
        if (request.DataIs<NumberSignal>())
        {
            switch (request.DataAs<NumberSignal>())
            {
                case NumberSignal.Init:
                    int initialGuess = ReadIntegerFromConsole("Please provide your initial guess: ");
                    return request.CreateResponse(initialGuess);
                case NumberSignal.Above:
                    int lowerGuess = ReadIntegerFromConsole("You previously guessed too large. Please provide a new guess: ");
                    return request.CreateResponse(lowerGuess);
                case NumberSignal.Below:
                    int higherGuess = ReadIntegerFromConsole("You previously guessed too small. Please provide a new guess: ");
                    return request.CreateResponse(higherGuess);
            }
        }

        throw new NotSupportedException($"Request {request.PortInfo.RequestType} is not supported");
    }

    private static int ReadIntegerFromConsole(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int value))
            {
                return value;
            }
            Console.WriteLine("Invalid input. Please enter a valid integer.");
        }
    }
}
