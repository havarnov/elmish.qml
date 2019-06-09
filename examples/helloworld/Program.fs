open Elmish
open Elmish.Qml
open Elmish.Qml.View
open Qml.Net
open Qml.Net.Runtimes

module App =
    type Model = {
        Color: Color
        InnerColor: Color
        Names: string seq
    }

    type Msg =
        | SetColor of Color
        | SetInnerColor of Color
        | AddName of string
        | RemoveName of string

    let init () =
        {
            Color = Red
            InnerColor = Black
            Names = ["Håvar"; "Hilde"; "Agnes"]
        },
        Cmd.none

    let update msg model =
        match msg with
        | SetColor c ->
            { model with Color = c }, Cmd.none
        | SetInnerColor c ->
            { model with InnerColor = c }, Cmd.none
        | AddName name ->
            { model with Names = Seq.append model.Names [name] }, Cmd.none
        | RemoveName name ->
            { model with Names = Seq.filter (fun n -> name <> n ) model.Names }, Cmd.none

    let nameView name =
        text
            [
                QmlProp.Text name;
                OnClicked (RemoveName name);
                HoverColor (Black, Green);
            ]

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
            [ Width 300; Height 500; Color model.Color; BorderColor Black; BorderWidth 5; Radius 10; OnClicked (SetColor newColor) ]
            [
                column
                    []
                    [
                        scroll [ Height 350; Width 300] (column [] (model.Names |> Seq.map nameView));
                        rectangle
                            [ Width 50; Height 30; Color model.InnerColor; BorderColor Black; BorderWidth 5; Radius 10; OnClicked (AddName (System.Guid.NewGuid().ToString())); ]
                            [ text [ QmlProp.Text "AddNew" ] ];
                    ]
            ]
        

[<EntryPoint>]
let main argv =
    RuntimeManager.DiscoverOrDownloadSuitableQtRuntime()
    use app = new QGuiApplication(argv)
    Program.mkProgram App.init App.update App.view
    |> Program.runApp app
