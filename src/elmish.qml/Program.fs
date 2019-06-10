module Elmish.Qml.Program

open Elmish
open Elmish.Qml.View

open Qml.Net

let main = """
import QtQuick 2.7
import QtQuick.Controls 2.0
import QtQuick.Layouts 1.0
import QmlElmish 1.0

ApplicationWindow {
    visible: true
    width: 640
    height: 480
    title: qsTr('Hello World')

    Item {
        property var dynamicObj
        id: container
        anchors.fill: parent
    }

    QmlElmishManager {
        id: mngr
        onQmlStrChanged: {
            if (container.dynamicObj)
            {
                container.dynamicObj.destroy();
            }
            container.dynamicObj = Qt.createQmlObject(mngr.qmlStr, container, "testing");
        }
    }
}
"""

let startElmishLoop
    (program: Program<unit, 'model, 'msg, QmlElement<'msg>>) =
    let setState model dispatch =
        let qmlEl = Program.view program model dispatch

        let (qml, msgs) = toQmlAndCmdMap qmlEl
        // printfn "%s" qml
        let callback m =
            match msgs |> Map.tryFind m with
            | Some msg -> dispatch msg
            | None -> ()

        if not (isNull Elmish.Qml.Csharp.QmlElmishManager.Instance)
        then
            Elmish.Qml.Csharp.QmlElmishManager.Instance.Callback <- (System.Action<string> callback)
            Elmish.Qml.Csharp.QmlElmishManager.Instance.ChangeBindableProperty(qml)
        ()

    program
    |> Program.withSetState setState
    |> Program.run

let runApp (app: QGuiApplication) program =
    use engine = new QQmlApplicationEngine()
    let _ = Qml.Net.Qml.RegisterType<Elmish.Qml.Csharp.QmlElmishManager>("QmlElmish", 1, 0);
    engine.LoadData main
    startElmishLoop program
    app.Exec()
