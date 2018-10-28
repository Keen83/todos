namespace Todos.Http

open System
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2


open Giraffe

open Todos

module TodoHttp = 
    let handlers : HttpFunc -> HttpContext -> HttpFuncResult = 
        choose [
            POST >=> route "/todos" >=> 
                fun next context ->
                    task {
                        let logger = context.GetLogger<Todo>()
                        logger.LogCritical ("New todo is saving!")
                        
                        let save = context.GetService<TodoSave>()
                        let! todo = context.BindJsonAsync<Todo>()
                        let id = ShortGuid.fromGuid(Guid.NewGuid())
                        let todo = { todo with Id = id }
                        return! json (save todo) next context
                    }

            GET >=> route "/todos" >=>
                fun next context ->
                    let find = context.GetService<TodoFind>()
                    let todos = find TodoCriteria.All
                    json todos next context

            PUT >=> routef "/todos/%s" (fun id ->
                fun next context -> 
                    task {
                        let save = context.GetService<TodoSave>()
                        let! todo = context.BindJsonAsync<Todo>()
                        let todo = { todo with Id = id }
                        return! json (save todo) next context
                    })

            DELETE >=> routef "/todos/%s" (fun id -> 
                fun next context ->
                    let delete = context.GetService<TodoDelete>()
                    json (delete id) next context)
        ]