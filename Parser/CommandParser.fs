namespace Parser

open System
open System.Collections.Concurrent
open System.Globalization
open System.Reflection
open System.Threading.Tasks
open Microsoft.Extensions.DependencyInjection
open Parser.Descriptors
open Parser.Infos  

module internal CommandParser =

    let parseMethods = ConcurrentDictionary<Type, MethodInfo>()
    
    let getParseMethod (parameter: ParameterInfo) =
        let paramType = parameter.Type
        match paramType |> parseMethods.TryGetValue with
        | true, method -> method
        | _ ->
            let method = paramType.GetMethod("Parse", [|typeof<string>; typeof<IFormatProvider>|])
            parseMethods[paramType] <- method
            method
    
    let readArg (parameterInfo: ParameterInfo) (culture: CultureInfo) (arg: string)  =
        try
            match parameterInfo.ReadMode with
            | Parse -> Ok ((getParseMethod parameterInfo).Invoke(null, [|arg; culture|]))
            | Convert -> Ok (Convert.ChangeType(arg, parameterInfo.Type, culture))
        with
        | e -> Error $"Couldn't parse {parameterInfo.Name}: expected type {parameterInfo.Type} (input: '{arg}')."
        
    let (|LessThan|_|) x value = if value < x then Some () else None
    let (|MoreThan|_|) x value = if value > x then Some () else None
    
    let validateArgCount (command: CommandInfo) (args: string array) =
        match command.Parameters.Length, args.Length, command.ExtraArgsHandleMode with
        | 0, MoreThan 0, ExtraArgsHandleMode.Error -> Error "Command was invoked with too many parameters."
        | _, LessThan command.RequiredParamCount, _ -> Error "Command was invoked with too few parameters."
        | _, MoreThan command.Parameters.Length, ExtraArgsHandleMode.Error
            when not (command.Parameters |> Array.last).Remainder -> Error "Command was invoked with too many parameters."
        | _ -> Ok ()
    
    let parseArgs (command: CommandInfo)  (culture: CultureInfo) (args: string array) : Result<obj array, string> =
        match args |> validateArgCount command with
        | Error e -> Error e
        | Ok _ ->
            let parameterCount = command.Parameters.Length
            let parsedArgs = Array.zeroCreate parameterCount
            
            let rec tryParse i array : Result<obj array, string> =
                if i >= args.Length || i >= parameterCount then Ok array
                else
                    let parameter = command.Parameters[i]
                    let toRead =
                        match parameter.Remainder with
                        | true -> args |> Array.skip i |> String.concat " "
                        | false -> args[i]
                    
                    match toRead |> readArg parameter culture with
                    | Error e -> Error e
                    | Ok parsed -> 
                        array[i] <- parsed
                        array |> tryParse (i + 1)
                        
            let rec setDefaultParams i array: obj array =
                if i >= parameterCount then array
                else
                    let parameter = command.Parameters[i]
                    match parameter.DefaultValue with
                    | None -> failwith $"Failed to invoke %s{command.Name}: parameter %s{parameter.Name} \
                                         is missing it's default value."
                    | Some value ->
                        array[i] <- value
                        array |> setDefaultParams (i + 1)
            
            match parsedArgs |> tryParse 0 with
            | Error e -> Error e
            | Ok parsed -> Ok (parsed |> setDefaultParams args.Length)
    
    let invokeCommand command provider culture args : Task<Result<unit, string>> =
        match args |> parseArgs command culture with
        | Error e -> Task.FromResult(Error e)
        | Ok args ->
            async {
                try
                    let instance = ActivatorUtilities.CreateInstance(provider, command.Module.Type)
                    do! command.Method.Invoke(instance, args) :?> Task |> Async.AwaitTask
                    return Ok ()
                with
                | e ->
                    return Error (
                        match e.InnerException with
                        | null -> $"{e.GetType()}: {e.Message}"
                        | inner -> $"{inner.GetType()}: {inner.Message}")
            } |> Async.StartAsTask