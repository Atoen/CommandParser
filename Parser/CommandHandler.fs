namespace Parser.Handler

open System
open System.Threading.Tasks
open Parser.DependencyInjection
open Parser.Infos
open Parser.CommandParser

    
type CommandHandler(helper: CommandHelper, provider: IServiceProvider) =
   
    let commandsWithAliases =
        helper.Modules
        |> Array.collect (fun m -> m.Commands)
        |> Array.map (fun c -> c.InvokeNames |> Array.map (fun a -> { Command = c; Alias = a }))
        |> Array.concat        
           
    member this.HandleCommand (command: string) : Task<Result<unit, string>> =        
        let tokens = command.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        
        match tokens with
        | [||] -> Task.FromResult(Error "No arguments provided")
        | _ ->
            match commandsWithAliases |> Array.tryFind (fun c -> c.Alias = tokens[0]) with
            | None -> Task.FromResult(Error $"Couldn't find command like {tokens[0]}")
            | Some commandAlias -> 
                if tokens.Length > 1 then Array.tail tokens else Array.empty
                |> invokeCommand commandAlias.Command provider helper.Culture