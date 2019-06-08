module Elmish.Qml.View

open System

type Color =
    | Red
    | Blue
    | Black

let toString c =
    match c with
    | Red -> "red"
    | Blue -> "blue"
    | Black -> "black"


type QmlProp<'msg> =
    | Width of int
    | Height of int
    | Color of Color
    | BorderWidth of int
    | BorderColor of Color
    | Radius of int
    | OnClicked of 'msg

type QmlItem =
    | Rectangle

type QmlElement<'msg> = {
    Item: QmlItem
    Attributes: QmlProp<'msg> seq
    Children: QmlElement<'msg> seq
}

let msgToString id msg =
    sprintf
        """
        MouseArea {
            anchors.fill: parent
            onClicked: {
                mngr.msgReceived('%s')
           }
        }
        """
        id

let inline rectangle props children =
    {
        Item = Rectangle;
        Attributes = props;
        Children = children;
    }

let prosToQml props =
    let empty s = (s, None)
    let pToStr = (fun p ->
        match p with
            | Width w -> sprintf "width: %d" w |> empty
            | Height h -> sprintf "height: %d" h |> empty
            | Color c -> sprintf "color: %A" (toString c) |> empty
            | BorderWidth w -> sprintf "border.width: %d" w |> empty
            | BorderColor c -> sprintf "border.color: %A" (toString c) |> empty
            | Radius r -> sprintf "radius: %d" r |> empty
            | OnClicked msg ->
                let id = Guid.NewGuid().ToString()
                (msgToString id msg, Some (id, msg)))

    let (strs, msgs) =
        props
        |> Seq.fold
            (fun s n ->
                let (strList, msgList) = s
                let (propStr, msg) = pToStr n
                ((Seq.append strList (Seq.singleton propStr)), (match msg with | Some (id, mm) -> Map.add id mm msgList | None -> msgList)))
            (Seq.empty, Map.empty)
    
    let propStr = strs |> Seq.fold (fun s n -> sprintf "%s%s%s" s System.Environment.NewLine n) String.Empty
    (propStr, msgs)

let rec toQmlInner dom: (string * Map<string, 'a>) =
    printfn "%A" dom.Children
    match dom.Item with
    | Rectangle ->
        let (props, msgs) = (prosToQml dom.Attributes)
        let children = Seq.toList (Seq.map toQmlInner dom.Children)
        let xx =
            children
            |> Seq.map (snd >> Map.toSeq)
            |> Seq.collect id
        let msgs2 = Seq.append (Map.toSeq msgs) xx |> Map.ofSeq
        let childQml =
            children
            |> Seq.map fst
            |> Seq.fold (fun s n -> sprintf "%s%s%s" s System.Environment.NewLine n) String.Empty

        (
            sprintf
                """
                Rectangle {
                    %s
                    %s
                }
                """
                props
                childQml,
            msgs2)

let toQml dom =
    let (el, msgs) = toQmlInner dom
    (
        sprintf
            """
            import QtQuick 2.7
            import QtQuick.Controls 2.0
            import QtQuick.Layouts 1.0
            %s
            """
            el,
        msgs
    )