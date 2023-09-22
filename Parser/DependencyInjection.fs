namespace Parser.DependencyInjection

open System
open System.Runtime.CompilerServices
open System.Globalization
open Microsoft.Extensions.DependencyInjection
open Parser.Infos
open Parser.Builder

type CommandAlias = {
    Command: CommandInfo
    Alias: string
}

type CommandHelper(modules) =
    
    let invocableAliases =
        modules
        |> Array.collect (fun m -> m.Commands)
        |> Array.map (fun c -> c.InvokeNames |> Array.map (fun a -> { Command = c; Alias = a }))
        |> Array.concat
           
    do
        invocableAliases
        |> Array.groupBy (fun ca -> ca.Alias)
        |> Array.filter (fun (_, group) -> group.Length > 1)
        |> Array.iter (fun (alias, group) ->
            Console.ForegroundColor <- ConsoleColor.Yellow 
            eprintfn $"Warning: Duplicate alias '%s{alias}' found:"
            group |> Array.iter (fun c ->
                let command = c.Command
                eprintfn $"\tmodule: '%s{command.Module.Name}' command: '%s{command.Name}'"))
        
        Console.ResetColor()

            
    member this.InvocableAliases
        with get() = invocableAliases
        
    member this.Modules
        with get() = modules
        
    member val Culture = CultureInfo.InvariantCulture with get, set
        
[<Extension>]
type IServiceCollectionExtension =
    [<Extension>]
    static member RegisterModulesFromAssemblyContaining (serviceCollection: IServiceCollection) (moduleType: Type) =
        let modules = getModuleTypes moduleType |> Array.map createModuleInfo
        modules |> Array.iter (fun m -> serviceCollection.AddTransient m.Type |> ignore)
        
        serviceCollection.AddSingleton(CommandHelper modules)