module Elmish.Qml.View

open System
open Elmish.Qml.Csharp

type Color =
    | Red
    | Blue
    | Black
    | Green

let toString c =
    match c with
    | Red -> "red"
    | Blue -> "blue"
    | Black -> "black"
    | Green -> "green"

type QmlElement<'msg> = {
    Item: QmlItem
    Attributes: QmlProp<'msg> seq
    Children: QmlElement<'msg> seq
}
and QmlProp<'msg> =
    | Width of int
    | Height of int
    | Color of Color
    | BorderWidth of int
    | BorderColor of Color
    | Radius of int
    | OnClicked of 'msg
    | Text of string
    | HoverColor of Color * Color
and QmlItem =
    | Rectangle
    | Column
    | Text
    | ScrollView

type Qmls<'a> = {
    Id: string;
    Manager: QmlElmishManager;
    QmlStr: string;
    CmdMap: Map<string, 'a>
    Children: Qmls<'a> list;
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

let inline element itemType props children =
    {
        Item = itemType;
        Attributes = props;
        Children = children;
    }

let inline rectangle props children =
    element Rectangle props children

let inline column props children =
    element Column props children

let inline text props =
    element Text props []

let inline scroll props child =
    element ScrollView props (Seq.singleton child)

let concatNewLine s n =
    sprintf "%s%s%s" s System.Environment.NewLine n

module Seq =
    let concatNewLine s = Seq.fold concatNewLine String.Empty s

let empty s = (s, None)

let rec pToStr p: string * (string * 'a) seq option =
    match p with
        | Width w -> sprintf "width: %d" w |> empty
        | Height h -> sprintf "height: %d" h |> empty
        | Color c -> sprintf "color: %A" (toString c) |> empty
        | BorderWidth w -> sprintf "border.width: %d" w |> empty
        | BorderColor c -> sprintf "border.color: %A" (toString c) |> empty
        | Radius r -> sprintf "radius: %d" r |> empty
        | OnClicked msg ->
            let id = Guid.NewGuid().ToString()
            (msgToString id msg, Some (Seq.ofList [(id, msg)]))
        | QmlProp.Text t -> sprintf "text: '%s'" t |> empty
        | HoverColor (d, h) ->
            let i = "foo" + System.Random().Next().ToString()
            sprintf
                """
                color: %s.containsMouse ? %A : %A
                MouseArea {
                    id: %s
                    z: -1
                    anchors.fill: parent
                    hoverEnabled: true
                }
                """
                <| i
                <| toString h
                <| toString d
                <| i
                |> empty

let prosToQml props =
    props
    |> Seq.fold
        (fun s n ->
            let (strList, msgList) = s
            let (propStr, msg) = pToStr n
            (
                (Seq.append strList (Seq.singleton propStr)),
                (
                    match msg with
                    | Some (s) -> Map.ofSeq (Seq.append (Map.toSeq msgList) s)
                    | None -> msgList)))
        (Seq.empty, Map.empty)
    |> (fun (s, m) -> (s |> Seq.concatNewLine), m)

let rec toQmlInner dom: (string * Map<string, 'a>) =

    let (props, msgs) = (prosToQml dom.Attributes)

    let childrenQml =
        dom.Children
        |> Seq.map toQmlInner
        |> Seq.toList

    let msgs =
        childrenQml
        |> Seq.map (snd >> Map.toSeq)
        |> Seq.collect id
        |> Seq.append (Map.toSeq msgs)
        |> Map.ofSeq

    let childrenQmlCombined =
        childrenQml
        |> Seq.map fst
        |> Seq.concatNewLine

    (
        sprintf
            """
            %A {
                %s
                %s
            }
            """
            dom.Item
            props
            childrenQmlCombined,
        msgs)

let toQmlAndCmdMap dom =
    let (el, msgs) = toQmlInner dom
    (
        sprintf
            """
            import QtQuick 2.7
            import QtQuick.Controls 2.0
            import QtQuick.Controls 1.4
            import QtQuick.Layouts 1.0
            %s
            """
            el,
        msgs
    )

let i id =
    sprintf
        """
        Item {
            property var dynamicObj
            id: container%s
            anchors.fill: parent

            Connections {
                target: %s
                onQmlStrChanged: {
                    if (container%s.dynamicObj)
                    {
                        container%s.dynamicObj.destroy();
                    }
                    container%s.dynamicObj = Qt.createQmlObject(%s.qmlStr, container%s, "container%s");
                }
            }
        }
        """
        id
        id
        id
        id
        id
        id
        id
        id

let rec toQmlInner2 (dom: QmlElement<'a>): Qmls<'a> =

    let children = dom.Children |> Seq.toList |> List.map toQmlInner2

    let (props, msgs) = prosToQml dom.Attributes

    let qmlStr =
        sprintf
            """
            import QtQuick 2.7
            import QtQuick.Controls 2.0
            import QtQuick.Controls 1.4
            import QtQuick.Layouts 1.0

            %A {
                %s
                %s
            }
            """
            dom.Item
            props
            (children
                |> Seq.map (fun c -> c.Id)
                |> Seq.map i
                |> Seq.concatNewLine)

    {
        Id = sprintf "mngr%d" (Random().Next())
        Manager = QmlElmishManager();
        QmlStr = qmlStr;
        CmdMap = msgs;
        Children = children;
    }

let toQmlAndCmdMap2 (dom: QmlElement<'a>): Qmls<'a> =
    toQmlInner2 dom