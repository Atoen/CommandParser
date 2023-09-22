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
    Console.WriteLine(result.IsError ? result.ErrorValue : "Ok");
}