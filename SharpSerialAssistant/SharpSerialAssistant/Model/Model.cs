using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Threading;
using System.Threading;

namespace SharpSerialAssistant
{
    class Model : EventSender, IModel
    {
        public void SubscribeErrorEvent(EventDelegate listener)
        {

        }
        public string[] GetSerialPorts()
        {
            return SerialPort.GetPortNames();
        }

        public Encoding GetSerialPortEncoding()
        {
            return serialPort.Encoding;
        }

        public bool GetSerialPortOpen()
        {
            return serialPort.IsOpen;
        }

        public void AddErrorHandler(MessageDelegate handler)
        {
            this.errorHandler += handler;
        }

        public void AddInfoHandler(MessageDelegate handler)
        {
            this.infoHandler += handler;
        }

        public void AddRecvUIUpdateHandler(MessageDelegate handler)
        {
            this.recvUIUpdateHandler += handler;
        }
        public void AddRecvDataShowHandler(MessageDelegate handler)
        {
            this.recvDataShowHandler += handler;
        }
        public void InitSerialPort()
        {
            serialPort.DataReceived += SerialPort_DataReceived;
            InitCheckTimer();
        }
        public void ConfigPort(string name, int baudRate, Parity parity, int dataBits, StopBits stopBits, Encoding encoding)
        {
            serialPort.PortName = name;
            serialPort.BaudRate = baudRate;
            serialPort.Parity = parity;
            serialPort.DataBits = dataBits;
            serialPort.StopBits = stopBits;
            serialPort.Encoding = encoding;
        }

        public bool OpenPort(string name, int baudRate, Parity parity, int dataBits, StopBits stopBits, Encoding encoding)
        {
            bool flag = false;
            ConfigPort(name, baudRate, parity, dataBits, stopBits, encoding);

            try
            {
                serialPort.Open();
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                RaiseMessage(infoHandler, string.Format("成功打开端口{0}, 波特率{1}。", serialPort.PortName, serialPort.BaudRate.ToString()));
                flag = true;
            }
            catch(Exception ex)
            {
                RaiseMessage(errorHandler, ex.Message);
            }

            return flag;
        }

        public bool ClosePort()
        {
            bool flag = false;

            try
            {
                serialPort.Close();
                RaiseMessage(infoHandler, string.Format("成功关闭端口{0}。", serialPort.PortName));
                flag = true;
            }
            catch (Exception ex)
            {
                RaiseMessage(errorHandler, ex.Message);
            }

            return flag;
        }

        private bool SerialPortWrite(string data)
        {
            SerialPortWrite(data, false);
            return false;
        }
        private string appendContent = "\n";
        private bool SerialPortWrite(string data, bool reportEnable)
        {
            if (serialPort == null)
            {
                return false;
            }

            if (serialPort.IsOpen == false)
            {
                RaiseMessage(errorHandler, "串口未打开，无法发送数据。");
                return false;
            }

            try
            {
                if (sendMode == SendMode.Character)
                {
                    serialPort.Write(data + appendContent);
                }
                else if (sendMode == SendMode.Hex)
                {
                    string[] grp = data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    List<byte> list = new List<byte>();

                    foreach (var item in grp)
                    {
                        list.Add(Convert.ToByte(item, 16));
                    }

                    serialPort.Write(list.ToArray(), 0, list.Count);
                }

                if (reportEnable)
                {
                    RaiseMessage(infoHandler, string.Format("成功发送：{0}。", data));
                }
            }
            catch (Exception ex)
            {
                RaiseMessage(errorHandler, ex.Message);
                return false;
            }

            return true;
        }

        public bool SendData()
        {
            if (string.IsNullOrEmpty(sendTxt))
            {
                RaiseMessage(errorHandler, "要发送的内容不能为空！");
                return false;
            }

            if (isSendAuto)
            {
                return SerialPortWrite(sendTxt, false);
            }
            else
            {
                return SerialPortWrite(sendTxt);
            }
        }

        public void autoSendData()
        {
            bool ret = SendData();

            if (ret == false)
            {
                return;
            }

            // trigger automatic sending
            StartAutoSendDataTimer(this.autoSendInterval);

            RaiseMessage(infoHandler, "串口自动发送数据中...");
        }

        public void InitAutoSendDataTimer()
        {
            autoSendDataTimer.IsEnabled = false;
            autoSendDataTimer.Tick += AutoSendDataTimer_Tick;
        }

        public void SetRecvMode(ReceiveMode mode)
        {
            this.recvMode = mode;
        }

        private void StartAutoSendDataTimer(int interval)
        {
            autoSendDataTimer.IsEnabled = false;
            autoSendDataTimer.Interval = TimeSpan.FromMilliseconds(interval);
            autoSendDataTimer.Start();
        }

        private void StopAutoSendDataTimer()
        {
            autoSendDataTimer.IsEnabled = false;
            autoSendDataTimer.Stop();
        }

        public void SetAppendContent(string append)
        {
            this.appendContent = append;
        }

        public void SetSendMode(SendMode mode)
        {
            this.sendMode = mode;
        }

        public void SetAutoSend(bool isAuto)
        {
            this.isSendAuto = isAuto;
            if(!isAuto)
            {
                StopAutoSendDataTimer();
            }
        }

        public void SetSendData(string data)
        {
            this.sendTxt = data;
        }

        public void SetAutoSendInterval(int interval)
        {
            this.autoSendInterval = interval;
        }

        private void AutoSendDataTimer_Tick(object sender, EventArgs e)
        {
            bool ret = false;
            ret = SendData();
            if(ret == false)
            {
                StopAutoSendDataTimer();
            }
        }

        private string GetSaveRecvDataFilePath()
        {
            string path = @"data.txt";

            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Title = "select path to save...";
            sfd.FileName = string.Format("data{0}", DateTime.Now.ToString("yyyyMdHHMMss"));
            sfd.Filter = "text file|*.txt";

            if (sfd.ShowDialog() == true)
            {
                path = sfd.FileName;
            }

            return path;
        }
        public void SaveData(string str)
        {
            string path = GetSaveRecvDataFilePath();
            try
            {
                using (System.IO.StreamWriter sr = new StreamWriter(path))
                {
                    sr.Write(str);

                    RaiseMessage(infoHandler, string.Format("成功保存数据到{0}", path));
                }
            }
            catch (Exception ex)
            {
                RaiseMessage(errorHandler, ex.Message);
            }

        }

        ///////////////////////////////////////////////////////////////////////
        private const int THRESH_VALUE = 128;
        private bool shouldClear = true;

        /// <summary>
        /// update：utilize buffering, if the buffer is filled out, the data will be picked out and shown in the interface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = sender as System.IO.Ports.SerialPort;

            if (sp != null)
            {
                // temporary buffer to cache all data
                int bytesToRead = sp.BytesToRead;
                byte[] tempBuffer = new byte[bytesToRead];

                // read out all the bytes in the buffer
                sp.Read(tempBuffer, 0, bytesToRead);

                // at first, check if to clear out the global buffer
                if (shouldClear)
                {
                    recvBuffer.Clear();
                    shouldClear = false;
                }

                // coye temporary data into global buffer
                recvBuffer.AddRange(tempBuffer);

                if (recvBuffer.Count >= THRESH_VALUE)
                {
                    // using thread to handle received data
                    Thread dataHandler = new Thread(new ParameterizedThreadStart(ReceivedDataHandler));
                    dataHandler.Start(recvBuffer);
                }

                // using another thread to handle left bytes of the buffer
                StartCheckTimer();

                RaiseMessage(recvUIUpdateHandler, "");
            }
        }

        private void ReceivedDataHandler(object obj)
        {
            List<byte> recvBuffer = new List<byte>();
            recvBuffer.AddRange((List<byte>)obj);

            if (recvBuffer.Count == 0)
            {
                return;
            }

            string recvStr = Utilities.BytesToText(recvBuffer, recvMode, serialPort.Encoding);
            RaiseMessage(recvDataShowHandler, recvStr);

            // dump all bytes of the global buffer, then clear it out
            shouldClear = true;
        }

        // A timer is used to handle the left bytes of the buffer. Its purpose is to gurantee that all data can be processed
        private DispatcherTimer checkTimer = new DispatcherTimer();
        private const int TIMEOUT = 50;

        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            //prevent trigger again
            StopCheckTimer();
            if(recvBuffer.Count < THRESH_VALUE)
            {
                Thread dataHandler = new Thread(new ParameterizedThreadStart(ReceivedDataHandler));
                dataHandler.Start(recvBuffer);
            }
        }
        public void InitCheckTimer()
        {
            checkTimer.Interval = new TimeSpan(0, 0, 0, 0, TIMEOUT);
            checkTimer.IsEnabled = false;
            checkTimer.Tick += CheckTimer_Tick;
        }

        private void StartCheckTimer()
        {
            checkTimer.IsEnabled = true;
            checkTimer.Start();
        }

        private void StopCheckTimer()
        {
            checkTimer.IsEnabled = false;
            checkTimer.Stop();
        }

        private SerialPort serialPort = new SerialPort();
        MessageDelegate errorHandler;
        MessageDelegate infoHandler;
        MessageDelegate recvUIUpdateHandler;
        MessageDelegate recvDataShowHandler;

        private DispatcherTimer autoSendDataTimer = new DispatcherTimer();
        private SendMode sendMode = SendMode.Character;
        private bool isSendAuto = false;
        private int autoSendInterval = 1000;
        private string sendTxt = "";
        private ReceiveMode recvMode = ReceiveMode.Character;
        private List<byte> recvBuffer = new List<byte>();
        
    }
}
