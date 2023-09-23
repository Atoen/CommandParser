using Parser.DependencyInjection;
using Parser.Descriptors;

namespace ModuleDefinitions.Modules;

[Name("Test"), Summary("Testing module")]
public class TestModule : ModuleBase
{
    private readonly CommandHelper _helper;

    public TestModule(CommandHelper helper) => _helper = helper;

    [Command, Alias("Calc", "C")]
    public async Task Calculate(double num1, [Summary("Operator")] char op, double num2)
    {
        var result = op switch
        {
            '+' => num1 + num2,
            '-' => num1 - num2,
            '*' => num1 * num2,
            '/' => num1 / num2,
            '^' => Math.Pow(num1, num2),
            _ => throw new ArgumentOutOfRangeException(nameof(op))
        };

        Console.WriteLine(result.ToString(_helper.Culture));
    }

    [Command, Alias("H", "?"), Summary("Display help for specific command")]
    public async Task Help(string? name = null)
    {
        if (name is not null)
        {
            var success = _helper.TryFindCommand(name, out var commandAlias);
            
            if (!success)
            {
                Console.WriteLine($"Couldn't find command like {name}");
                return;
            }

            Console.WriteLine(commandAlias.Command);
            return;
        }

        foreach (var module in _helper.Modules)
        {
            Console.WriteLine(module);
        }
    }

    [Command, Alias("B")]
    public async Task Binary(long num)
    {
        var binary = Convert.ToString(num, 2);
        Console.WriteLine(binary);
    }
}