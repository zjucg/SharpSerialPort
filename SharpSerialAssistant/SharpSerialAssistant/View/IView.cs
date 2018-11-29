using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpSerialAssistant
{
    interface IView
    {
        void Update(byte[] msg);

        string GetErrorMessage();

        void SetPorts(string[] ports);

        void setSerialPortOpen(bool isopen);

        void SetInfoColor();

        void Information(string msg);

        void Alert(string msg);

        void InitClockTimer();

        void UpdateTimeDate();

        ReceiveMode RecvModeChanged(string mode);

        void SendModeChanged(string mode, Encoding encoding);

        void SetIsShowRecvData(bool showData);

        void ShowRecvData(string data);

        void UpdateUIAfterRecv(string info);

        void SetOpenCloseBtn(string content);

        bool GetEnableAutoSend();

        void SetEnableAutoSend(bool isEnable);

        string GetAutoSendStr();

        int GetAutoSendDataInterval();

        SendMode GetSendMode();

        string GetRecvData();
    }
}
