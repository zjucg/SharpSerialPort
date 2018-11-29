using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace SharpSerialAssistant
{
    interface IModel
    {
        void AddErrorHandler(MessageDelegate handler);
        void AddInfoHandler(MessageDelegate handler);

        void AddRecvUIUpdateHandler(MessageDelegate handler);
        void AddRecvDataShowHandler(MessageDelegate handler);

        string[] GetSerialPorts();

        Encoding GetSerialPortEncoding();

        bool GetSerialPortOpen();

        void InitSerialPort();
        bool OpenPort(string name, int baudRate, Parity parity, int dataBits, StopBits stopBits, Encoding encoding);
        bool ClosePort();
        bool SendData();

        void InitAutoSendDataTimer();
        void SetRecvMode(ReceiveMode mode);
        void SetAppendContent(string append);
        void SetSendMode(SendMode mode);

        void SetAutoSendInterval(int interval);

        void SetAutoSend(bool isAuto);
        void SetSendData(string data);

        void autoSendData();

        void InitCheckTimer();

        void SaveData(string str);
    }
}
