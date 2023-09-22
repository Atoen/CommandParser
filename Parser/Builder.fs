module internal Parser.Builder

open System
open System.Reflection
open System.Threading.Tasks
open Parser.Definitions
open Parser.Infos

let (|ImplementsInterface|_|) name (paramType: Type) = if paramType.GetInterface(name) <> null then Some() else None

let getReadMode (parameterType: Type) =
    
    if parameterType = typeof<string> then Some Convert else
    match parameterType with
    | ImplementsInterface "ISpanParsable`1" -> Some SpanParse
    | ImplementsInterface "IParsable`1" -> Some Parse
    | ImplementsInterface "IConvertible" -> Some Convert
    | _ -> None

let getModuleTypes (baseType: Type) =
    baseType.Assembly.GetExportedTypes()
    |> Array.filter (fun t -> not t.IsAbstract && baseType.IsAssignableFrom t)
    
let getAttribute<'T, 'U when 'T :> DescriptorAttribute<'U>> (array: Attribute array) =
    match Array.tryFind (fun a -> a.GetType() = typeof<'T>) array with
    | Some (:? 'T as attribute) -> Some attribute
    | _ -> None

let getAttributeValue<'T, 'U when 'T :> DescriptorAttribute<'U>> (fallback: 'U) (array: Attribute array) =
    match getAttribute<'T, 'U> array with
    | Some attribute -> Option.defaultValue fallback attribute.Value
    | None -> fallback

let createParameterInfo (parameter: Reflection.ParameterInfo) (command: CommandInfo) (commandParamCount: int) =
    let attributes = Array.ofSeq (parameter.GetCustomAttributes())
    
    let remainder = attributes |> getAttributeValue<RemainderAttribute, _>  false
    let name = attributes |> getAttributeValue<NameAttribute, _> parameter.Name
    let summary = attributes |> getAttributeValue<SummaryAttribute, _> String.Empty
    
    if remainder &&
        parameter.ParameterType <> typeof<string> && parameter.ParameterType <> typeof<string Option>
        then failwithf $"Remainder attribute can only be used on string type parameters: \
                         Module %s{command.Module.Name}, Command %s{command.Name}, Parameter %s{name}"
        
    if remainder && parameter.Position <> commandParamCount - 1
        then failwithf $"Remainder attribute can only be used on the last parameter: \
                        Module %s{command.Module.Name}, Command %s{command.Name}, Parameter %s{name}"
    
    let defaultValue =
       match parameter.HasDefaultValue, parameter.DefaultValue with
       | true, defaultValue -> Some defaultValue
       | _ -> None
       
    let readMode =
        match getReadMode parameter.ParameterType with
        | None -> failwith "Parameter needs to implement IParsable or IConvertible interface"
        | Some mode -> mode
    
    {Name = name; Summary = summary; Command = command; Remainder = remainder; ReadMode = readMode 
     Optional = parameter.IsOptional; Type = parameter.ParameterType; DefaultValue = defaultValue }
                    
let createCommandInfo (method: MethodInfo) (parentModule: ModuleInfo) =
    let attributes = Array.ofSeq (method.GetCustomAttributes())
   
    let name = attributes |> getAttributeValue<CommandAttribute, _>  method.Name
    let summary = attributes |> getAttributeValue<SummaryAttribute, _>  String.Empty
    let extraArgsMode = attributes |> getAttributeValue<ExtraArgsAttribute, _> ExtraArgsHandleMode.Ignore
    
    if method.ReturnType <> typeof<Task> then failwithf $"Command return type must be %s{typeof<Task>.FullName}: \
                                                          Module %s{parentModule.Name}, Command %s{name}"
   
    let commandInfo = { Name = name; Summary = summary; Parameters = [||]; Method = method
                        Module = parentModule; RequiredParamCount = 0; ExtraArgsHandleMode = extraArgsMode }
    let parameters = method.GetParameters()
    let paramCount = parameters.Length
        
    {
        commandInfo with
            Parameters = parameters
            |> Array.map (fun p -> createParameterInfo p commandInfo paramCount)
            
            RequiredParamCount = parameters
            |> Array.filter (fun p -> not p.IsOptional)
            |> Array.length
    }
    
let createModuleInfo (moduleType: Type) =
    let attributes = Array.ofSeq (moduleType.GetCustomAttributes())
    
    let name = attributes |> getAttributeValue<NameAttribute, _> moduleType.Name
    let summary = attributes |> getAttributeValue<SummaryAttribute, _> String.Empty
    
    let moduleInfo = { Name = name; Summary = summary; Type = moduleType; Commands = [||] }
    
    {
        moduleInfo with
            Commands = moduleType.GetMethods()
            |> Array.filter (fun m -> m.GetCustomAttribute<CommandAttribute>() <> null)
            |> Array.map (fun m -> createCommandInfo m moduleInfo)
    }