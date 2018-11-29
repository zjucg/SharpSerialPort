using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace SharpSerialAssistant
{
    public partial class MainWindow : Window
    {
        private void Window_Initialized(object sender, EventArgs e)
        {
            InitInterface();
            presenter = new Presenter(this);
            presenter.findPorts();
        }
        private void InitInterface()
        {
            portsComboBox.IsEnabled = false;
            baudRateComboBox.SelectedIndex = 3;
            parityComboBox.SelectedIndex = 0;
            dataBitsComboBox.SelectedIndex = 0;
            stopBitsComboBox.SelectedIndex = 0;
            encodingComboBox.SelectedIndex = 0;
            recvCharacterRadioButton.IsChecked = true;
            showRecvDataCheckBox.IsChecked = true;
            sendCharacterRadioButton.IsChecked = true;
            manualInputRadioButton.IsChecked = true;
            loadFileRadioButton.IsEnabled = false;
            timeUnitComboBox.SelectedIndex = 0;
            autoSendIntervalTextBox.Text = "1000";
            string sendData = "Hello, World!";
            sendDataTextBox.Text = sendData;
            statusInfoTextBlock.Text = "欢迎使用串口助手！";
        }

        private Presenter presenter = null;

    }
}
