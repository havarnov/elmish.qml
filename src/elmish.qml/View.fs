module Elmish.Qml.View

type Color =
    | Red
    | Blue
    | Black

let toString c =
    match c with
    | Red -> "red"
    | Blue -> "blue"
    | Black -> "black"


type QmlProp =
    | Width of int
    | Height of int
    | Color of Color
    | BorderWidth of int
    | BorderColor of Color
    | Radius of int

type QmlItem =
    | Rectangle

type QmlElement = {
    Item: QmlItem
    Attributes: QmlProp seq
    Children: QmlItem seq
}


let inline rectangle props children =
    {
        Item = Rectangle;
        Attributes = props;
        Children = children;
    }

let prosToQml props =
    props
    |> Seq.map
        (fun p ->
        match p with
            | Width w -> sprintf "width: %d" w
            | Height h -> sprintf "height: %d" h
            | Color c -> sprintf "color: %A" (toString c)
            | BorderWidth w -> sprintf "border.width: %d" w
            | BorderColor c -> sprintf "border.color: %A" (toString c)
            | Radius r -> sprintf "radius: %d" r)
    |> Seq.fold (fun s n -> sprintf "%s%s%s" s System.Environment.NewLine n) ""

let toQml dom =
    match dom.Item with
    | Rectangle ->
        (prosToQml dom.Attributes)
        |> sprintf
                """
                import QtQuick 2.7
                import QtQuick.Controls 2.0
                import QtQuick.Layouts 1.0

                Rectangle {
                    %s
                }
                """
