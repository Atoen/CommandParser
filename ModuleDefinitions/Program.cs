using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ModuleDefinitions.Modules;
using Parser.Handler;
using Parser.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<CommandHandler>();
services.RegisterModulesFromAssemblyContaining(typeof(TestModule));

var provider = services.BuildServiceProvider();
var handler = provider.GetRequiredService<CommandHandler>();

Console.WriteLine("Ready");

while (true)
{
    var input = Console.ReadLine();
    if (input is null) return;

    var start = Stopwatch.GetTimestamp();
    
    var result = await handler.HandleCommand(input);
    var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
    if (result.IsOk)
    {
        WriteResult($"Ok ({elapsed:F2}ms)", ConsoleColor.Green);
    }
    else
    {
        WriteResult($"{result.ErrorValue} ({elapsed:F2}ms)", ConsoleColor.Red);
    }
}

void WriteResult(string result, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(result);
    Console.ResetColor();
}