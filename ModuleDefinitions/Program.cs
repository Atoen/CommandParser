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
    
    var result = await handler.HandleCommand(input);
    if (result.IsOk)
    {
        WriteResult("Ok", ConsoleColor.Green);
    }
    else
    {
        WriteResult(result.ErrorValue, ConsoleColor.Red);
    }
}

void WriteResult(string result, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(result);
    Console.ResetColor();
}