using System;
using Qml.Net;

namespace Elmish.Qml.Csharp
{
    public class QmlElmishManager
    {
        public QmlElmishManager()
        {
            Instance = this;
        }

        public static string _qmlStr = "";

        public void ChangeBindableProperty(string newQmlStr)
        {
            QmlStr = newQmlStr;
        }


        [NotifySignal]
        public string QmlStr
        {
            get
            {
                return _qmlStr;
            }
            set
            {
                _qmlStr = value;
                this.ActivateSignal("qmlStrChanged");
            }
        }

        public static QmlElmishManager Instance { get; private set; }
    }
}
