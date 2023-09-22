using Parser;
using Parser.DependencyInjection;
using Parser.Descriptors;

namespace ModuleDefinitions.Modules;

[Name("Test")]
public class TestModule : ModuleBase
{
    private readonly CommandHelper _helper;

    public TestModule(CommandHelper helper) => _helper = helper;

    [Command, Alias("Calc", "C", "C")]
    public async Task Calculate(double num1, char op, double num2)
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

    [Command, Alias("H", "?"), ExtraArgs(ExtraArgsHandleMode.Error)]
    public async Task Help()
    {
        foreach (var commandInfo in _helper.Modules.SelectMany(x => x.Commands))
        {
            Console.WriteLine(commandInfo.Name);
        }
    }
}