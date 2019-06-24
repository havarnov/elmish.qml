module Elmish.Qml.Program

open Elmish
open Elmish.Qml.View

open Qml.Net
open Elmish.Qml.Csharp

let main = """
import QtQuick 2.7
import QtQuick.Controls 2.0
import QtQuick.Layouts 1.0

ApplicationWindow {
    visible: true
    width: 640
    height: 480
    title: qsTr('Hello World')

    Item {
        property var dynamicObj
        id: container
        anchors.fill: parent

        Connections {
            target: mngr
            onQmlStrChanged: {
                if (container.dynamicObj)
                {
                    container.dynamicObj.destroy();
                }
                container.dynamicObj = Qt.createQmlObject(mngr.qmlStr, container, "testing");
            }
        }
    }
}
"""

let startElmishLoop
    (program: Program<unit, 'model, 'msg, QmlElement<'msg>>)
    (manager: QmlElmishManager)
    (engine: QQmlApplicationEngine) =
    let setState model dispatch =
        let qmlEl = Program.view program model dispatch

        // let (qml, msgs) = toQmlAndCmdMap qmlEl
        // // printfn "%s" qml
        // let callback m =
        //     match msgs |> Map.tryFind m with
        //     | Some msg -> dispatch msg
        //     | None -> ()

        // manager.Callback <- (System.Action<string> callback)
        // manager.QmlStr <- qml

        let qmls =
            toQmlAndCmdMap2 qmlEl

        let rec addToContext (q: Qmls<'msg>) =
            for c in q.Children |> Seq.toList do
                addToContext c
            printfn "%s" q.Id
            engine.SetContextProperty(q.Id, q.Manager)
        
        addToContext qmls

        let callback m =
            match qmls.CmdMap |> Map.tryFind m with
            | Some msg -> dispatch msg
            | None -> ()

        manager.Callback <- (System.Action<string> callback)
        printfn "%s" qmls.QmlStr
        manager.QmlStr <- qmls.QmlStr

        let rec i (inner: Qmls<'msg>) =
            let callback m =
                match inner.CmdMap |> Map.tryFind m with
                | Some msg -> dispatch msg
                | None -> ()

            inner.Manager.Callback <- (System.Action<string> callback)
            printfn "%s" inner.QmlStr
            inner.Manager.QmlStr <- inner.QmlStr

            for c in inner.Children |> Seq.toList do
                i c

        for c in qmls.Children |> Seq.toList do
            i c

        ()

    program
    |> Program.withSetState setState
    |> Program.run

let runApp (app: QGuiApplication) program =
    use engine = new QQmlApplicationEngine()
    let mngr = QmlElmishManager()
    engine.SetContextProperty("mngr", mngr)
    engine.LoadData main
    startElmishLoop program mngr engine
    app.Exec()
