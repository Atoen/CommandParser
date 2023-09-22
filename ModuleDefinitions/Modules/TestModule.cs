using Parser.Definitions;

namespace ModuleDefinitions.Modules;

[Name("Test")]
public class TestModule : ModuleBase
{
    [Command, ExtraArgs(ExtraArgsHandleMode.Error)]
    public async Task O(string a, int b = 3)
    {
        Console.WriteLine($"a: {a}");

        // throw new Exception();
    }
    
    [Command, ExtraArgs(ExtraArgsHandleMode.Error)]
    public async Task A()
    {
        Console.WriteLine($"a: f");
    }
}