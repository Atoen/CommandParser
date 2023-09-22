using Parser.Descriptors;

namespace ModuleDefinitions.Modules;

[Name("Test")]
public class TestModule : ModuleBase
{
    [Command("Calc"), ExtraArgs(ExtraArgsHandleMode.Error)]
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

        Console.WriteLine(result);
    }

    [Command, ExtraArgs(ExtraArgsHandleMode.Error)]
    public async Task A()
    {
        Console.WriteLine($"a: f");
    }
}