module Parser.Infos

open System
open System.Reflection
open Parser.Descriptors

type ParameterReadMode =
    | Parse
    | Convert

type ModuleInfo = {
    Name: string
    Summary: string
    Type: Type
    Commands: CommandInfo array
}
with
    override this.ToString() =
        let summaryInfo = if this.Summary <> String.Empty then $" - {this.Summary}:" else ":"
        let commandsInfo = 
            if this.Commands.Length > 0 then
                let commands = this.Commands |> Array.map (fun c -> c.Name) |> String.concat ", "
                $" {commands}"
            else
                ""
        $"Module {this.Name}{summaryInfo}{commandsInfo}"
        
and CommandInfo = {
    Module: ModuleInfo
    Name: string
    Aliases: string array
    InvokeNames: string array
    Summary: string
    Method: MethodInfo
    RequiredParamCount: int
    ExtraArgsHandleMode: ExtraArgsHandleMode
    Parameters: ParameterInfo array
}
with
    override this.ToString() =
        let summaryInfo = if this.Summary <> String.Empty then $" - {this.Summary}." else ""
        let aliasInfo =
            if this.Aliases.Length > 0 then
                let aliases = this.Aliases |> Array.map id |> String.concat ", "
                $" (aliases: {aliases})"
            else ""
        let paramsInfo =
            if this.Parameters.Length > 0 then
                let parameters = this.Parameters |> Array.map (fun p -> p.ToString()) |> String.concat ", "
                $" Parameters: ({parameters})"
            else ""
            
        $"Command {this.Name}{aliasInfo}{summaryInfo}{paramsInfo}"

and ParameterInfo = {
    Command: CommandInfo
    Summary: string
    Name: string
    Type: Type
    DefaultValue: obj option
    ReadMode: ParameterReadMode
    Remainder: bool
}
with
    override this.ToString() =
        let summaryInfo = if this.Summary <> String.Empty then $" - {this.Summary}" else ""
        let defaultValueInfo = 
            match this.DefaultValue with
            | None -> ""
            | Some null -> " = null"
            | Some value -> value.ToString()
        let remainderInfo = if this.Remainder then " [remainder]" else ""
        $"{this.Name}:{this.Type.Name}{summaryInfo}{defaultValueInfo}{remainderInfo}"
        
