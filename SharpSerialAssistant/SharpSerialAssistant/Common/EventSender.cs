using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSerialAssistant
{
    class EventSender
    {
        // It is usually safer to copy a delegate before raising it.
        // See discussions on this topic by searching for "c# delegate copy"
        protected void RaiseEvent(EventDelegate e)
        {
            EventDelegate copy = e;
            if(copy != null)
            {
                copy();
            }
        }

        protected void RaiseMessage(MessageDelegate m, string s)
        {
            MessageDelegate copy = m;
            if (copy != null)
            {
                copy(s);
            }
        }
    }
}
