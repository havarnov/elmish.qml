open Elmish
open Elmish.Qml
open Elmish.Qml.View
open Qml.Net
open Qml.Net.Runtimes

module App =
    type Model = {
        Name: string
        Age: int
    }

    type Msg =
        | UpdateName of string
        | IncrementAge

    let init () =
        {
            Name = "foobar"
            Age = 23
        },
        Cmd.ofMsg IncrementAge

    let update msg model =
        match msg with
        | UpdateName name -> { model with Name = name }, Cmd.none
        | IncrementAge ->
            { model with Age = model.Age + 1 }, Cmd.none

    let view model dispatch =
        rectangle [ Width 100; Height 100; Color Red; BorderColor Black; BorderWidth 5; Radius 10 ] []
        

[<EntryPoint>]
let main argv =
    RuntimeManager.DiscoverOrDownloadSuitableQtRuntime()
    use app = new QGuiApplication(argv)
    Program.mkProgram App.init App.update App.view
    |> Program.runApp app
