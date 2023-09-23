namespace Parser.Handler

open System
open System.Threading.Tasks
open Parser.DependencyInjection
open Parser.CommandParser

type CommandHandler(helper: CommandHelper, provider: IServiceProvider) =
    
    member this.HandleCommand (command: string) : Task<Result<unit, string>> =        
        let tokens = command.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        
        match tokens with
        | [||] -> Task.FromResult(Error "No arguments provided")
        | _ ->
            match helper.InvocableAliases |> Array.tryFind (fun c -> c.Alias = tokens[0]) with
            | None -> Task.FromResult(Error $"Couldn't find command like {tokens[0]}")
            | Some commandAlias -> 
                if tokens.Length > 1 then Array.tail tokens else Array.empty
                |> invokeCommand commandAlias.Command provider helper.Culture