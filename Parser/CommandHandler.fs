namespace Parser.Handler

open System
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Parser.Definitions
open Parser.DependencyInjection
open Parser.Infos

type CommandHandler(helper: CommandHelper, provider: IServiceProvider) =
    let commands = helper.Modules |> Array.collect (fun m -> m.Commands)
    
    let readArg (arg: string) (parameterInfo: ParameterInfo) : Result<obj, string> =
        try
            match parameterInfo.ReadMode with
            | Parse -> failwith "todo"
            | SpanParse -> failwith "todo"
            | Convert -> Ok (Convert.ChangeType(arg, parameterInfo.Type, helper.Culture))
        with
        | e -> Error $"Couldn't parse {parameterInfo.Name}: expected type {parameterInfo.Type} (input: '{arg}')"
    
    let validateArgCount (command: CommandInfo) (args: string array) =
        if command.Parameters.Length = 0 then
            match command.ExtraArgsHandleMode with
            | ExtraArgsHandleMode.Error -> Error "Command was invoked with too many parameters."
            | _ -> Ok ()
        
        elif args.Length < command.RequiredParamCount then Error "Command was invoked with too few parameters."
        
        elif command.ExtraArgsHandleMode = ExtraArgsHandleMode.Error && not (command.Parameters |> Array.last).Remainder &&
             args.Length > command.Parameters.Length
            then Error "Command was invoked with too many parameters."
            
        else Ok ()
    
    let parseArgs (command: CommandInfo) (args: string array) : Result<obj array, string> =
        match args |> validateArgCount command with
        | Error e -> Error e
        | Ok _ ->
            let parameterCount = command.Parameters.Length
            let parsedArgs = Array.create parameterCount (obj())
            
            let rec setDefaultParams i array: obj array =
                if i >= parameterCount then array
                else
                    match command.Parameters[i].DefaultValue with
                    | None -> failwith "todo"
                    | Some value ->
                        array[i] <- value
                        setDefaultParams (i + 1) array
            
            let rec tryParse i array : Result<obj array, string> =
                if i >= args.Length || i >= parameterCount then Ok array
                else
                    let parameter = command.Parameters[i]
                    let toRead =
                        match parameter.Remainder with
                        | true -> String.concat " " (Array.skip i args)
                        | false -> args[i]

                    match parameter |> readArg toRead with
                    | Error e -> Error e
                    | Ok parsed -> 
                        array[i] <- parsed
                        tryParse (i + 1) array

            match tryParse 0 parsedArgs with
            | Error e -> Error e
            | Ok parsed -> Ok (parsed |> setDefaultParams args.Length)
    
    let invokeCommand (command: CommandInfo) (args: string array) : Task<Result<unit, string>> =
        match args |> parseArgs command with
        | Error e -> Task.FromResult(Error e)
        | Ok args -> 
            async {
                try
                    let instance = ActivatorUtilities.CreateInstance(provider, command.Module.Type)
                    let! result = command.Method.Invoke(instance, args) :?> Task |> Async.AwaitTask
                    return Ok result
                with
                | e -> return Error $"{e.GetType()}: {e.Message}"
            } |> Async.StartAsTask
        
    member this.HandleCommand (command: string) : Task<Result<unit, string>> =
        let tokens = command.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        
        match tokens with
        | [||] -> Task.FromResult(Error "No arguments provided")
        | _ ->
            match commands |> Array.tryFind (fun c -> c.Name = tokens[0]) with
            | None -> Task.FromResult(Error $"Couldn't find command like {tokens[0]}")
            | Some commandInfo -> 
                let args = if tokens.Length > 1 then Array.tail tokens else Array.empty
                invokeCommand commandInfo args