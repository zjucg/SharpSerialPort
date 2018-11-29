using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Documents;

namespace SharpSerialAssistant
{
    public enum ReceiveMode
    {
        Character,  //char
        Hex,        //hex
        Decimal,    //decimal
        Octal,      //octal
        Binary      //binary
    }

    public enum SendMode
    {
        Character,  //char
        Hex         //hex
    }

    class View : EventSender, IView
    {
        public View(MainWindow mainwindow)
        {
            this.mainWindow = mainwindow;
        }
        public string GetErrorMessage()
        {
            throw new NotImplementedException();
        }

        public void Update(byte[] msg)
        {
            throw new NotImplementedException();
        }

        public void SetPorts(string[] ports)
        {
            mainWindow.portsComboBox.ItemsSource = ports;
            if(mainWindow.portsComboBox.Items.Count > 0)
            {
                mainWindow.portsComboBox.SelectedIndex = 0;
                mainWindow.portsComboBox.IsEnabled = true;
                Information(string.Format("查找到可以使用的端口{0}个。", mainWindow.portsComboBox.Items.Count.ToString()));
            }
            else
            {
                mainWindow.portsComboBox.IsEnabled = false;
                Alert("Oops，没有查找到可用端口；您可以点击“查找”按钮手动查找。");
            }
        }

        public void SetInfoColor()
        {
            if(isSerialPortOpen)
            {
                // #FFCA5100
                mainWindow.statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xCA, 0x51, 0x00));
            }
            else
            {
                // #FF007ACC
                mainWindow.statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC));
            }
        }
        public void Information(string msg)
        {
            mainWindow.statusInfoTextBlock.Text = msg;
        }

        public void Alert(string msg)
        {
            // #FF68217A
            mainWindow.statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x21, 0x2A));
            mainWindow.statusInfoTextBlock.Text = msg;
        }

        public void setSerialPortOpen(bool isopen)
        {
            this.isSerialPortOpen = isopen;
            SetInfoColor();
        }

        public void UpdateTimeDate()
        {
            string timeDateString = "";
            DateTime now = DateTime.Now;
            timeDateString = string.Format("{0}年{1}月{2}日 {3}:{4}:{5}",
                now.Year,
                now.Month.ToString("00"),
                now.Day.ToString("00"),
                now.Hour.ToString("00"),
                now.Minute.ToString("00"),
                now.Second.ToString("00"));

            mainWindow.timeDateTextBlock.Text = timeDateString;
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimeDate();
        }

        public void InitClockTimer()
        {
            clockTimer.Interval = new TimeSpan(0, 0, 1);
            clockTimer.IsEnabled = true;
            clockTimer.Tick += ClockTimer_Tick;
            clockTimer.Start();
        }

        public ReceiveMode RecvModeChanged(string mode)
        {
            //
            // TO-DO:
            // convert text to the specified format
            //
            mainWindow.recvDataRichTextBox.Document.Blocks.Clear();
            switch (mode)
            {
                case "char":
                    recvMode = ReceiveMode.Character;
                    Information("提示：字符显示模式。");
                    break;
                case "hex":
                    recvMode = ReceiveMode.Hex;
                    Information("提示：十六进制显示模式。");
                    break;
                case "dec":
                    recvMode = ReceiveMode.Decimal;
                    Information("提示：十进制显示模式。");
                    break;
                case "oct":
                    recvMode = ReceiveMode.Octal;
                    Information("提示：八进制显示模式。");
                    break;
                case "bin":
                    recvMode = ReceiveMode.Binary;
                    Information("提示：二进制显示模式。");
                    break;
                default:
                    break;
            }

            return recvMode;
        }

        public void SendModeChanged(string mode, Encoding encoding)
        {
            string sendData = mainWindow.sendDataTextBox.Text;
            switch (mode)
            {
                case "char":
                    sendMode = SendMode.Character;
                    Information("提示：发送字符文本。");
                    // to char
                    mainWindow.sendDataTextBox.Text = Utilities.ToSpecifiedText(sendData, SendMode.Character, encoding);
                    break;
                case "hex":
                    // to hex
                    sendMode = SendMode.Hex;
                    Information("提示：发送十六进制。输入十六进制数据之间用空格隔开，如：1D 2A 38。");
                    mainWindow.sendDataTextBox.Text = Utilities.ToSpecifiedText(sendData, SendMode.Hex, encoding);
                    break;
                default:
                    break;
            }
        }

        public void SetIsShowRecvData(bool showData)
        {
            this.showReceiveData = showData;
        }

        public void ShowRecvData(string data)
        {
            this.mainWindow.handleRecvData(data);
        }

        public void UpdateUIAfterRecv(string info)
        {
            this.mainWindow.updateUIAfterRecvData(info);
        }

        public void SetOpenCloseBtn(string content)
        {
            mainWindow.openClosePortButton.Content = content;
        }

        public bool GetEnableAutoSend()
        {
            return mainWindow.autoSendEnableCheckBox.IsChecked == true;
        }

        public void SetEnableAutoSend(bool isEnable)
        {
            mainWindow.autoSendEnableCheckBox.IsChecked = isEnable;
        }

        public string GetAutoSendStr()
        {
            return string.Format("{0} {1}。", mainWindow.autoSendIntervalTextBox.Text, mainWindow.timeUnitComboBox.Text.Trim());
        }

        public int GetAutoSendDataInterval()
        {
            int interval = 1000;
            if (int.TryParse(mainWindow.autoSendIntervalTextBox.Text.Trim(), out interval) == true)
            {
                string select = mainWindow.timeUnitComboBox.Text.Trim();
                switch (select)
                {
                    case "毫秒":
                        break;
                    case "秒钟":
                        interval *= 1000;
                        break;
                    case "分钟":
                        interval = interval * 60 * 1000;
                        break;
                    default:
                        break;
                }
            }

            return interval;
        }

        public SendMode GetSendMode()
        {
            return sendMode;
        }

        public string GetRecvData()
        {
            return (new TextRange(mainWindow.recvDataRichTextBox.Document.ContentStart, mainWindow.recvDataRichTextBox.Document.ContentEnd)).Text;
        }

        private MainWindow mainWindow = null;
        private bool isSerialPortOpen = false;
        private DispatcherTimer clockTimer = new DispatcherTimer();

        private ReceiveMode recvMode = ReceiveMode.Character;
        private SendMode sendMode = SendMode.Character;
        private bool showReceiveData = true;
    }
}
