using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace SharpSerialAssistant
{
    class Presenter
    {
        public Presenter(MainWindow mainwindow)
        {
            this.mainWindow = mainwindow;
            this.model = new Model();
            this.view = new View(mainwindow);
            this.view.InitClockTimer();
            this.model.AddInfoHandler(this.view.Information);
            this.model.AddErrorHandler(this.view.Alert);
            this.model.AddRecvDataShowHandler(this.view.ShowRecvData);
            this.model.AddRecvUIUpdateHandler(this.view.UpdateUIAfterRecv);
            this.model.InitAutoSendDataTimer();
            this.model.InitSerialPort();
        }

        public void findPorts()
        {
            string[] ports = this.model.GetSerialPorts();
            bool isOpen = this.model.GetSerialPortOpen();
            this.view.setSerialPortOpen(isOpen);
            this.view.SetPorts(ports);
        }

        public void changeRecvMode(string mode)
        {
            ReceiveMode rcvmode = this.view.RecvModeChanged(mode);
            this.model.SetRecvMode(rcvmode);   
        }

        public void changeSendMode(string mode)
        {
            Encoding encoding = this.model.GetSerialPortEncoding();
            this.view.SendModeChanged(mode, encoding);
        }

        public void isShowRecvData(bool showData)
        {
            this.view.SetIsShowRecvData(showData);
        }

        public void openClosePort(string name, int baudRate, Parity parity, int dataBits, StopBits stopBits, Encoding encoding)
        {
            bool isopen = this.model.GetSerialPortOpen();
            if (isopen)
            {
                if(this.model.ClosePort())
                {
                    mainWindow.progressBar.Visibility = System.Windows.Visibility.Collapsed;
                    this.view.SetOpenCloseBtn("打开");
                    
                }
            }
            else
            {
                if(this.model.OpenPort(name, baudRate, parity, dataBits, stopBits, encoding))
                {
                    this.view.SetOpenCloseBtn("关闭");
                }
            }
            isopen = this.model.GetSerialPortOpen();
            this.view.setSerialPortOpen(isopen);
        }

        public bool enableAutoSend()
        {
            bool ischecked = this.view.GetEnableAutoSend();
            if(ischecked)            
            {
                this.view.Information(string.Format("使能串口自动发送功能，发送间隔：{0}。", this.view.GetAutoSendStr()));
            }
            else
            {
                this.view.Information("禁用串口自动发送功能。");
                this.model.SetAutoSend(false);
                mainWindow.progressBar.Visibility = System.Windows.Visibility.Collapsed;
            }

            return ischecked;
        }

        public void sendData(bool isAuto, string data)
        {
            this.model.SetAutoSend(isAuto);
            this.model.SetSendData(data);
            this.model.SetSendMode(this.view.GetSendMode());
            if (isAuto)
            {
                this.model.SetAutoSendInterval(this.view.GetAutoSendDataInterval());
                this.model.autoSendData();
            }
            else
            {
                this.model.SendData();                
            }
        }

        public void setAppendContent(string appendContent)
        {
            this.model.SetAppendContent(appendContent);
        }

        public void saveRecvData2File()
        {
            string data = this.view.GetRecvData();
            this.model.SaveData(data);
        }


        void handleUpdate()
        {

        }

        MainWindow mainWindow = null;
        IModel model = null;
        IView view = null;
    }
}
