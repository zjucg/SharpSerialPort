using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SharpSerialAssistant
{
    public delegate void EventDelegate();
    public delegate void MessageDelegate(string msg);

    delegate void UpdateDelegate(object value, object userdata);
}
