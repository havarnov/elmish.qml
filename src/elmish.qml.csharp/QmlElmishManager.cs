﻿using System;
using System.Collections.Generic;
using Qml.Net;

namespace Elmish.Qml.Csharp
{
    public class QmlElmishManager
    {
        public QmlElmishManager()
        {
        }

        public static string _qmlStr = "";


        public Action<string> Callback { get; set; }

        public void MsgReceived(string msg)
        {
            if (Callback != null)
            {
                Callback(msg);
            }
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
    }
}
