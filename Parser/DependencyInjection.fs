namespace Parser.DependencyInjection

open System
open System.Runtime.CompilerServices
open System.Globalization
open Microsoft.Extensions.DependencyInjection
open Parser.Infos
open Parser.Builder

type CommandHelper() =
    let mutable modules: ModuleInfo array = [||]
    member this.Modules
        with get() = modules
        and internal set value = modules <- value
        
    member val Culture = CultureInfo.InvariantCulture with get, set
        
[<Extension>]
type IServiceCollectionExtension =
    [<Extension>]
    static member RegisterModulesFromAssemblyContaining (serviceCollection: IServiceCollection) (moduleType: Type) =
        let modules = getModuleTypes moduleType |> Array.map createModuleInfo
        modules |> Array.iter (fun m -> serviceCollection.AddTransient m.Type |> ignore)
        
        let helper = CommandHelper()
        helper.Modules <- modules
        
        serviceCollection.AddSingleton helper