open Elmish
open Elmish.Qml
open Elmish.Qml.View
open Qml.Net
open Qml.Net.Runtimes

module App =
    type Model = {
        Color: Color
        InnerColor: Color
    }

    type Msg =
        | SetColor of Color
        | SetInnerColor of Color

    let init () =
        {
            Color = Red
            InnerColor = Black
        },
        Cmd.none

    let update msg model =
        match msg with
        | SetColor c ->
            { model with Color = c }, Cmd.none
        | SetInnerColor c ->
            { model with InnerColor = c }, Cmd.none

    let view model dispatch =
        let newColor =
            match model.Color with
            | Red -> Blue
            | _ -> Red
        let newInnerColor =
            match model.InnerColor with
            | Black -> Red
            | _ -> Black
        rectangle
            [ Width 100; Height 100; Color model.Color; BorderColor Black; BorderWidth 5; Radius 10; OnClicked (SetColor newColor) ]
            [
                rectangle [ Width 50; Height 50; Color model.InnerColor; BorderColor Black; BorderWidth 5; Radius 10; OnClicked (SetInnerColor newInnerColor); ] []
            ]
        

[<EntryPoint>]
let main argv =
    RuntimeManager.DiscoverOrDownloadSuitableQtRuntime()
    use app = new QGuiApplication(argv)
    Program.mkProgram App.init App.update App.view
    |> Program.runApp app
