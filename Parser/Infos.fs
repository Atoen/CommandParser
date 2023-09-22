module Parser.Infos

open System
open System.Reflection
open Parser.Definitions

type ParameterReadMode =
    | Parse
    | SpanParse
    | Convert

type ModuleInfo = {
    Name: string
    Summary: string
    Type: Type
    Commands: CommandInfo array
}

and CommandInfo = {
    Module: ModuleInfo
    Name: string
    Summary: string
    Method: MethodInfo
    RequiredParamCount: int
    ExtraArgsHandleMode: ExtraArgsHandleMode
    Parameters: ParameterInfo array
}

and ParameterInfo = {
    Command: CommandInfo
    Summary: string
    Name: string
    Type: Type
    DefaultValue: obj option
    Optional: bool
    ReadMode: ParameterReadMode
    Remainder: bool
}