using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.IO.Ports;
using System.Windows.Input;

namespace SharpSerialAssistant
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 窗口关闭前拦截
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.Key == Key.S)
            {
                //SaveData(GetSaveDataPath());
            }

            // Ctrl+Enter 进入/退出简洁视图模式
            else if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.Key == Key.Enter)
            {
                if (IsCompactViewMode())
                {
                    RestoreViewMode();
                }
                else
                {
                    EnterCompactViewMode();
                }
            }

            // Enter发送数据
            else if (e.Key == Key.Enter)
            {
                string textToSend = sendDataTextBox.Text;
                if (autoSendEnableCheckBox.IsChecked == true)
                {
                    this.presenter.sendData(true, textToSend);
                }
                else
                {
                    this.presenter.sendData(false, textToSend);
                }
            }
        }

        private void openClosePortButton_Click(object sender, RoutedEventArgs e)
        {
            string name = GetSelectedPortName();
            int baudRate = GetSelectedBaudRate();
            Parity parity = GetSelectedParity();
            int dataBits = GetSelectedDataBits();
            StopBits stopBits = GetSelectedStopBits();
            Encoding encoding = GetSelectedEncoding();

            this.presenter.openClosePort(name, baudRate, parity, dataBits, stopBits, encoding);
        }
        private string GetSelectedPortName()
        {
            return portsComboBox.Text;
        }

        private int GetSelectedBaudRate()
        {
            int baudRate = 9600;
            int.TryParse(baudRateComboBox.Text, out baudRate);
            return baudRate;
        }

        private Parity GetSelectedParity()
        {
            string select = parityComboBox.Text;

            Parity p = Parity.None;
            if (select.Contains("Odd"))
            {
                p = Parity.Odd;
            }
            else if (select.Contains("Even"))
            {
                p = Parity.Even;
            }
            else if (select.Contains("Space"))
            {
                p = Parity.Space;
            }
            else if (select.Contains("Mark"))
            {
                p = Parity.Mark;
            }

            return p;
        }

        private int GetSelectedDataBits()
        {
            int dataBits = 8;
            int.TryParse(dataBitsComboBox.Text, out dataBits);

            return dataBits;
        }

        private StopBits GetSelectedStopBits()
        {
            StopBits stopBits = StopBits.None;
            string select = stopBitsComboBox.Text.Trim();

            if (select.Equals("1"))
            {
                stopBits = StopBits.One;
            }
            else if (select.Equals("1.5"))
            {
                stopBits = StopBits.OnePointFive;
            }
            else if (select.Equals("2"))
            {
                stopBits = StopBits.Two;
            }

            return stopBits;
        }

        private Encoding GetSelectedEncoding()
        {
            string select = encodingComboBox.Text;
            Encoding enc = Encoding.Default;

            if (select.Contains("UTF-8"))
            {
                enc = Encoding.UTF8;
            }
            else if (select.Contains("ASCII"))
            {
                enc = Encoding.ASCII;
            }
            else if (select.Contains("Unicode"))
            {
                enc = Encoding.Unicode;
            }

            return enc;
        }

        private void findPortButton_Click(object sender, RoutedEventArgs e)
        {
            this.presenter.findPorts();
        }

        private void showRecvDataCheckBox_Click(object sender, RoutedEventArgs e)
        {
            showReceiveData = (bool)showRecvDataCheckBox.IsChecked;
            this.presenter.isShowRecvData(showReceiveData);
        }

        private void saveRecvDataButton_Click(object sender, RoutedEventArgs e)
        {
            this.presenter.saveRecvData2File();
        }

        private void clearRecvDataButton_Click(object sender, RoutedEventArgs e)
        {
            recvDataRichTextBox.Document.Blocks.Clear();
        }

        private void sendDataModeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if(rb != null && presenter != null)
            {
                string mode = rb.Tag.ToString();
                this.presenter.changeSendMode(mode);
            }
        }

        private void recvModeButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            
            if(recvDataRichTextBox == null)
            {
                return;
            }

            if (rb != null && presenter != null)
            {
                string mode = rb.Tag.ToString();
                this.presenter.changeRecvMode(mode);
            }
        }

        private void manualInputRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void loadFileRadioButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void clearSendDataTextBox_Click(object sender, RoutedEventArgs e)
        {
            sendDataTextBox.Clear();
        }

        private void autoSendEnableCheckBox_Click(object sender, RoutedEventArgs e)
        {
            this.presenter.enableAutoSend();
        }

        private void sendDataButton_Click(object sender, RoutedEventArgs e)
        {
            string textToSend = sendDataTextBox.Text;
            if (autoSendEnableCheckBox.IsChecked == true)
            {
                this.presenter.sendData(true, textToSend);
                progressBar.Visibility = Visibility.Visible;
            }
            else
            {
                this.presenter.sendData(false, textToSend);
            }
        }

        public void updateUIAfterRecvData(string msg)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (autoSendEnableCheckBox.IsChecked == false)
                {
                    statusInfoTextBlock.Text = msg;
                }
                dataRecvStatusBarItem.Visibility = Visibility.Visible;
            }));
        }
        public void handleRecvData(string recv)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (showReceiveData)
                {
                    var start = recvDataRichTextBox.Document.ContentStart;
                    var end = recvDataRichTextBox.Document.ContentEnd;
                    int diff = start.GetOffsetToPosition(end);
                    if(diff == 0) // wired, RichTextBox might has a bug for the first line. It seems have no break line
                    {
                        recvDataRichTextBox.AppendText("\n");
                    }
                    // 根据显示模式显示接收到的字节.
                    recvDataRichTextBox.AppendText(recv);
                    recvDataRichTextBox.ScrollToEnd();
                }

                dataRecvStatusBarItem.Visibility = Visibility.Collapsed;
            }));
        }

        private bool showReceiveData = true;
    }
}
