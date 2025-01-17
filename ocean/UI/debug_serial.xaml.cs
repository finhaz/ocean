﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Windows.Threading;
using ocean.Communication;

namespace ocean.UI
{
    /// <summary>
    /// debug_serial.xaml 的交互逻辑
    /// </summary>
    public partial class debug_serial : Page
    {
        public Debugsera tcom { get; set; }



        public debug_serial()
        {
           

            InitializeComponent();

            tcom = new Debugsera();

        }



        private void btOpenCom_Click(object sender, RoutedEventArgs e)
        {

            if (CommonRes.mySerialPort.IsOpen)
            {
                CommonRes.mySerialPort.Close();
                cbBaudRate.IsEnabled = true;
                cbDataBits.IsEnabled = true;
                cbParity.IsEnabled = true;
                cbPortName.IsEnabled = true;
                cbStopBits.IsEnabled = true;
                btOpenCom.Content = "打开串口";
                tbComState.Text = cbPortName.Text + "已关闭";
                comState.Style = (Style)this.FindResource("EllipseStyleRed");
            }
            else
            {
                CommonRes.mySerialPort.PortName = cbPortName.Text;
                CommonRes.mySerialPort.BaudRate = Convert.ToInt32(cbBaudRate.Text);
                switch (Convert.ToString(cbParity.Text))
                {
                    case "无":
                        CommonRes.mySerialPort.Parity = Parity.None;
                        break;
                    case "奇校验":
                        CommonRes.mySerialPort.Parity = Parity.Odd;
                        break;
                    case "偶校验":
                        CommonRes.mySerialPort.Parity = Parity.Even;
                        break;
                }
                switch (Convert.ToInt32(cbStopBits.Text))
                {
                    case 0:
                        CommonRes.mySerialPort.StopBits = StopBits.None;
                        break;
                    case 1:
                        CommonRes.mySerialPort.StopBits = StopBits.One;
                        break;
                    case 2:
                        CommonRes.mySerialPort.StopBits = StopBits.Two;
                        break;
                }
                try
                {
                    CommonRes.mySerialPort.Open();
                }
                catch
                {
                    tbComState.Text = cbPortName.Text + "串口被占用！";
                    MessageBox.Show("串口被占用！");
                    return;
                }
                cbBaudRate.IsEnabled = false;
                cbDataBits.IsEnabled = false;
                cbParity.IsEnabled = false;
                cbPortName.IsEnabled = false;
                cbStopBits.IsEnabled = false;
                btOpenCom.Content = "关闭串口";
                tbComState.Text = cbPortName.Text + "," + cbBaudRate.Text + "," +
                    cbParity.Text + "," + cbDataBits.Text + "," + cbStopBits.Text;
                comState.Style = (Style)this.FindResource("EllipseStyleGreen");
            }

        }

        private void btSend_Click(object sender, RoutedEventArgs e)
        {
            tcom.btSend_Event(tbSend.Text, (bool)ck16Send.IsChecked);
        }



        private void btClearView_Click(object sender, RoutedEventArgs e)
        {
            tcom.tbReceive.Text = "";
            tcom.txtRecive.Text = "0";
        }

        private void ck16View_Click(object sender, RoutedEventArgs e)
        {
            tcom.ckHexState = (bool)ck16View.IsChecked;
        }



        private void ckAutoSend_Click(object sender, RoutedEventArgs e)
        {
            if (CommonRes.mySerialPort.IsOpen == false)
            {
                MessageBox.Show("串口未开！");
                ckAutoSend.IsChecked = false;
                return;
            }
            if (ckAutoSend.IsChecked == true)
            {

                tbkIntervalTime.Visibility = Visibility.Visible;
                tbIntervalTime.Visibility = Visibility.Visible;
                tcom.time1.Interval = TimeSpan.FromSeconds(Convert.ToDouble(tbIntervalTime.Text));
                if (Convert.ToDouble(tbIntervalTime.Text) == 0)
                {
                    return;
                }
                else
                {
                    tcom.time1.Start();
                }
            }
            else
            {
                tbkIntervalTime.Visibility = Visibility.Hidden;
                tbIntervalTime.Visibility = Visibility.Hidden;
                tcom.time1.Stop();
            }
            tbReceive.ScrollToEnd();
        }

        private void ck16Send_Click(object sender, RoutedEventArgs e)
        {
            //get16View((bool)ck16Send.IsChecked);
        }



        private void get16View(bool isHex)
        {
            if (isHex == true)
            {
                //将字符器转为Ascii码
                string hexString, hexStringView = "";
                hexString = tcom.StringToHexString(tbSend.Text);
                for (int i = 0; i < hexString.Length; i += 2)
                {
                    hexStringView += hexString.Substring(i, 2) + " ";
                }
                if (ckAdvantechCmd.IsChecked == true) { hexStringView += "0D"; }

                TextBox myText = grdSend.FindName("tb16View") as TextBox;
                //如果已有tb16View这个控件，则进行显示，如没有则创建并进行显示
                if (myText != null)
                {
                    myText.Text = hexStringView;
                }
                else
                {
                    #region//创建一个文本框来显示字符串的Acsii码
                    TextBox myTextBox = new TextBox();
                    myTextBox.VerticalAlignment = VerticalAlignment.Top;
                    myTextBox.Height = 22;
                    myTextBox.Margin = new Thickness(3, 0, 0, 0);
                    myTextBox.IsReadOnly = true;
                    myTextBox.IsEnabled = true;
                    tbSend.Margin = new Thickness(3, 22, 0, 0);
                    grdSend.Children.Add(myTextBox);
                    grdSend.RegisterName("tb16View", myTextBox);
                    myTextBox.SetValue(Grid.ColumnProperty, 1);
                    myTextBox.Text = hexStringView;
                    #endregion
                }
            }
            else
            {
                //移除tb16View控件
                TextBox myTextBox = grdSend.FindName("tb16View") as TextBox;
                if (myTextBox != null)
                {
                    grdSend.Children.Remove(myTextBox);//移除对应按钮控件   
                    grdSend.UnregisterName("tb16View");
                    tbSend.Margin = new Thickness(3, 0, 0, 0);
                }
            }
        }

        private void ckAdvantechCmd_Click(object sender, RoutedEventArgs e)
        {
            ck16Send_Click(sender, e);
            if (ckAsciiView.IsChecked == true)
            {
                get16View((bool)ckAsciiView.IsChecked);
            }
        }


        private void btExpend_Click(object sender, RoutedEventArgs e)
        {
            if (bdExpend.IsVisible == true)
            {
                bdExpend.Visibility = Visibility.Hidden;
                tbReceive.Margin = new Thickness(0, 1, 0, 0);
            }
            else
            {
                bdExpend.Visibility = Visibility.Visible;
                tbReceive.Margin = new Thickness(0, 1, bdExpend.Width + 5, 0);
            }
            CheckBox ckBox = gdExpend.FindName("ckExpend0") as CheckBox;
            if (ckBox == null)
            {
                bdExpend.Visibility = Visibility.Visible;
                for (int i = 0; i < 10; i++)
                {
                    CheckBox newCkBox = new CheckBox();
                    newCkBox.VerticalAlignment = VerticalAlignment.Center;
                    newCkBox.HorizontalAlignment = HorizontalAlignment.Center;
                    gdExpend.Children.Add(newCkBox);
                    newCkBox.SetValue(Grid.RowProperty, i + 1);
                    gdExpend.RegisterName("ckExpend" + i, newCkBox);

                    TextBox newTextBox = new TextBox();
                    gdExpend.Children.Add(newTextBox);
                    newTextBox.SetValue(Grid.RowProperty, i + 1);
                    newTextBox.SetValue(Grid.ColumnProperty, 1);
                    newTextBox.Margin = new Thickness(0, 3, 5, 3);
                    gdExpend.RegisterName("expendTextBox" + i, newTextBox);

                    Button newButton = new Button();
                    gdExpend.Children.Add(newButton);
                    newButton.SetValue(Grid.RowProperty, i + 1);
                    newButton.SetValue(Grid.ColumnProperty, 2);
                    newButton.Margin = new Thickness(0, 3, 0, 3);
                    newButton.Content = "发送" + i;
                    newButton.Click += new RoutedEventHandler(dynamicButton_Click);
                }
            }
        }

        private void dynamicButton_Click(object sender, RoutedEventArgs e)
        {
            Button bt1 = (Button)sender;
            string str = "";
            str = Convert.ToString(bt1.Content);
            TextBox tb1 = gdExpend.FindName("expendTextBox" + str.Substring(str.Length - 1, 1)) as TextBox;
            CheckBox ck1 = gdExpend.FindName("ckExpend" + str.Substring(str.Length - 1, 1)) as CheckBox;
            tcom.btSend_Event(tb1.Text, (bool)ck1.IsChecked);
            //MessageBox.Show("点了确定！" + str.Substring(str.Length - 1, 1));
        }

        private void tbSend_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ckAsciiView.IsChecked == true)
            {
                get16View((bool)ckAsciiView.IsChecked);
            }
        }

        private void ckAsciiView_Click(object sender, RoutedEventArgs e)
        {
            get16View((bool)ckAsciiView.IsChecked);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CommonRes.mySerialPort.DataReceived-= new SerialDataReceivedEventHandler(tcom.mySerialPort_DataReceived);
        }
    }
}
