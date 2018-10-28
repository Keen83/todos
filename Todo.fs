namespace Todos

type Todo = 
    {
        Id: string
        Text: string
        Done: bool
        Cost: int
    }

type TodoCriteria = 
    | All

type TodoFind = TodoCriteria -> Todo[]

type TodoSave = Todo -> Todo

type TodoDelete = string -> bool