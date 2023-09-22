namespace Parser.Descriptors

open System

[<AbstractClass>]
type ModuleBase() = class end

[<AllowNullLiteral>]
type DescriptorAttribute<'T>(value: 'T Option) =
    inherit Attribute()
    member this.Value with get() = value
    new () = DescriptorAttribute<'T> None

[<AllowNullLiteral>]
[<AttributeUsage(AttributeTargets.Method)>]
type CommandAttribute(value) =
    inherit DescriptorAttribute<string>(if value = String.Empty then None else Some value)
    new () = CommandAttribute String.Empty

[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Parameter)>]
type NameAttribute(value) = inherit DescriptorAttribute<string>(Some value)

[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Method ||| AttributeTargets.Parameter)>]
type SummaryAttribute(value) = inherit DescriptorAttribute<string>(Some value)

[<AttributeUsage(AttributeTargets.Method)>]
type AliasAttribute([<ParamArray>] value) = inherit DescriptorAttribute<string array>(Some value)

type ExtraArgsHandleMode =
    | Ignore = 0
    | Error = 1
    
[<AttributeUsage(AttributeTargets.Method)>]
type ExtraArgsAttribute(value) = inherit DescriptorAttribute<ExtraArgsHandleMode>(Some value)

type RemainderAttribute(value) =
    inherit DescriptorAttribute<bool>(Some value)
    new () = RemainderAttribute true