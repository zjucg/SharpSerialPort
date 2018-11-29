using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SharpSerialAssistant
{
    public partial class MainWindow : Window
    {
        private Stack<Visibility> panelVisibilityStack = new Stack<Visibility>(3);
        private bool IsCompactViewMode()
        {
            if (autoSendConfigPanel.Visibility == Visibility.Collapsed &&
                serialCommunicationConfigPanel.Visibility == Visibility.Collapsed &&
                autoSendConfigPanel.Visibility == Visibility.Collapsed)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void EnterCompactViewMode()
        {
            // first to keep all visible
            panelVisibilityStack.Push(serialPortConfigPanel.Visibility);
            panelVisibilityStack.Push(autoSendConfigPanel.Visibility);
            panelVisibilityStack.Push(serialCommunicationConfigPanel.Visibility);

            // enter simple mode
            serialPortConfigPanel.Visibility = Visibility.Collapsed;
            autoSendConfigPanel.Visibility = Visibility.Collapsed;
            serialCommunicationConfigPanel.Visibility = Visibility.Collapsed;

            // change corresonding menuitem, deselect
            serialSettingViewMenuItem.IsChecked = false;
            autoSendDataSettingViewMenuItem.IsChecked = false;
            serialCommunicationSettingViewMenuItem.IsChecked = false;

            // enable all to view mode
            serialSettingViewMenuItem.IsEnabled = false;
            autoSendDataSettingViewMenuItem.IsEnabled = false;
            serialCommunicationSettingViewMenuItem.IsEnabled = false;

            // change to simple mode
            compactViewMenuItem.IsChecked = true;

            // 
            statusInfoTextBlock.Text = "进入简洁视图模式。";
        }
        private void RestoreViewMode()
        {
            // restore
            serialCommunicationConfigPanel.Visibility = panelVisibilityStack.Pop();
            autoSendConfigPanel.Visibility = panelVisibilityStack.Pop();
            serialPortConfigPanel.Visibility = panelVisibilityStack.Pop();

            // check menuitems
            if (serialPortConfigPanel.Visibility == Visibility.Visible)
            {
                serialSettingViewMenuItem.IsChecked = true;
            }

            if (autoSendConfigPanel.Visibility == Visibility.Visible)
            {
                autoSendDataSettingViewMenuItem.IsChecked = true;
            }

            if (serialCommunicationConfigPanel.Visibility == Visibility.Visible)
            {
                serialCommunicationSettingViewMenuItem.IsChecked = true;
            }

            serialSettingViewMenuItem.IsEnabled = true;
            autoSendDataSettingViewMenuItem.IsEnabled = true;
            serialCommunicationSettingViewMenuItem.IsEnabled = true;

            compactViewMenuItem.IsChecked = false;

            // 
            statusInfoTextBlock.Text = "恢复原来的视图模式。";
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void appendRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            string appendContent = "\n";
            if (rb != null)
            {
                switch (rb.Tag.ToString())
                {
                    case "none":
                        appendContent = "";
                        break;
                    case "return":
                        appendContent = "\r";
                        break;
                    case "newline":
                        appendContent = "\n";
                        break;
                    case "retnewline":
                        appendContent = "\r\n";
                        break;
                    default:
                        break;
                }
                this.presenter.setAppendContent(appendContent);
                statusInfoTextBlock.Text = "发送追加：" + rb.Content.ToString();
            }
        }

        private void serialSettingViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool state = serialSettingViewMenuItem.IsChecked;

            if (state == false)
            {
                serialPortConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                serialPortConfigPanel.Visibility = Visibility.Collapsed;
                if (IsCompactViewMode())
                {
                    serialPortConfigPanel.Visibility = Visibility.Visible;
                    EnterCompactViewMode();
                }
            }

            serialSettingViewMenuItem.IsChecked = !state;
        }

        private void autoSendDataSettingViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool state = autoSendDataSettingViewMenuItem.IsChecked;

            if (state == false)
            {
                autoSendConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                autoSendConfigPanel.Visibility = Visibility.Collapsed;
                if (IsCompactViewMode())
                {
                    autoSendConfigPanel.Visibility = Visibility.Visible;
                    EnterCompactViewMode();
                }
            }

            autoSendDataSettingViewMenuItem.IsChecked = !state;
        }

        private void serialCommunicationSettingViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool state = serialCommunicationSettingViewMenuItem.IsChecked;

            if (state == false)
            {
                serialCommunicationConfigPanel.Visibility = Visibility.Visible;
            }
            else
            {
                serialCommunicationConfigPanel.Visibility = Visibility.Collapsed;

                if (IsCompactViewMode())
                {
                    serialCommunicationConfigPanel.Visibility = Visibility.Visible;
                    EnterCompactViewMode();
                }
            }

            serialCommunicationSettingViewMenuItem.IsChecked = !state;
        }

        private void compactViewMenuItem_Click(object sender, RoutedEventArgs e)
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

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SharpSerialAssistant.About about = new About();
            about.ShowDialog();
        }

        private void helpMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
