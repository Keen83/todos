open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.FileProviders
open MongoDB.Driver
open Giraffe

open Todos
open Todos.Http
// open Todos.TodoInMemory
open Todos.TodoInMongo


let getFilePath fileName = 
    Path.Combine(Directory.GetCurrentDirectory(), @"./client/dist/todos-client/" + fileName)

let routes =
    choose [
        TodoHttp.handlers
        GET >=> route "/"       >=> htmlFile "./client/dist/todos-client/index.html"
        ]

let configureApp (app: IApplicationBuilder) =
    app.UseDefaultFiles() |> ignore
    app.UseStaticFiles( 
             StaticFileOptions(
                 FileProvider = 
                     new PhysicalFileProvider(
                             Path.Combine(Directory.GetCurrentDirectory(), @"client/dist/todos-client")),
                 RequestPath = PathString("")
         )
        ) |> ignore
    app.UseGiraffe routes

let configureAppConfiguration (hostingContext: WebHostBuilderContext) (config: IConfigurationBuilder) =
    let envName = hostingContext.HostingEnvironment.EnvironmentName

    config
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile(sprintf "appsettings.%s.json" envName)
        .AddEnvironmentVariables()
        |> ignore

let configureServices (services: IServiceCollection) =
    let mongo = MongoClient ("mongodb://localhost:27017/")
    let db = mongo.GetDatabase "todos"
    services.AddGiraffe() |> ignore
    //services.AddTodoInMemory(Hashtable()) |> ignore
    services.AddTodoInMongo(db.GetCollection<Todo>("todos")) |> ignore

let configureLogging (hostingContext: WebHostBuilderContext) (builder: ILoggingBuilder) =
    builder
        .AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
        .AddConsole()
        .AddDebug()
    |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .ConfigureAppConfiguration(configureAppConfiguration)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0 // return an integer exit code
