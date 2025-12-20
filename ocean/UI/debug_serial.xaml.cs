using ocean.Communication;
using ocean.ViewModels;
using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Threading;

namespace ocean.UI
{
    /// <summary>
    /// debug_serial.xaml 的交互逻辑
    /// </summary>
    public partial class debug_serial : Page
    {
        private AppViewModel _globalVM = AppViewModel.Instance;

        public DispatcherTimer time1 = new DispatcherTimer();

        delegate void HanderInterfaceUpdataDelegate(string mySendData);
        HanderInterfaceUpdataDelegate myUpdataHander;
        delegate void txtGotoEndDelegate();

        public debug_serial()
        {
            InitializeComponent();
            // 将Page的DataContext绑定到全局ViewModel
            DataContext = _globalVM;

            time1.Tick += new EventHandler(time1_Tick);
            //CommonRes.mySerialPort.DataReceived += new SerialDataReceivedEventHandler(mySerialPort_DataReceived);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            CommonRes.mySerialPort.Encoding = System.Text.Encoding.GetEncoding("GB2312");

            if (CommonRes.mySerialPort.IsOpen)
            {

                btOpenCom.Content = "关闭串口";
                comState.Style = (Style)FindResource("EllipseStyleGreen");
            }

        }

        private void time1_Tick(object sender, EventArgs e)
        {
            if (CommonRes.mySerialPort.IsOpen)
            {
                btSend_Event(tbSend.Text, (bool)ck16Send.IsChecked);
            }
        }

        private void tbIntervalTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ckAutoSend.IsChecked == true)
            {
                if (Convert.ToDouble(tbIntervalTime.Text) == 0)
                {
                    time1.Stop();
                }
                else
                {
                    time1.Interval = TimeSpan.FromSeconds(Convert.ToDouble(tbIntervalTime.Text));
                    time1.Start();
                }
            }
        }


        private void btOpenCom_Click(object sender, RoutedEventArgs e)
        {

            if (CommonRes.mySerialPort.IsOpen)
            {
                CommonRes.mySerialPort.Close();
                _globalVM.SerialConfig.IsConfigEnabled = true;
                btOpenCom.Content = "打开串口";
                _globalVM.SerialConfig.TbComStateText = cbPortName.Text + "已关闭";
                comState.Style = (Style)FindResource("EllipseStyleRed");
            }
            else
            {
                try
                {
                    CommonRes.mySerialPort.PortName = _globalVM.SerialConfig.SelectedPortName;
                }
                catch 
                {
                    MessageBox.Show("未选择串口！");
                    return;
                }
                CommonRes.mySerialPort.BaudRate = _globalVM.SerialConfig.SelectedBaudRate;
                switch (_globalVM.SerialConfig.SelectedParityName)
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
                switch (_globalVM.SerialConfig.SelectedStopBit )
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
                    _globalVM.SerialConfig.TbComStateText = cbPortName.Text + "串口被占用！";
                    MessageBox.Show("串口被占用！");
                    return;
                }
                _globalVM.SerialConfig.IsConfigEnabled = false;
                btOpenCom.Content = "关闭串口";
                _globalVM.SerialConfig.TbComStateText = cbPortName.Text + "," + cbBaudRate.Text + "," +
                    cbParity.Text + "," + cbDataBits.Text + "," + cbStopBits.Text;
                comState.Style = (Style)this.FindResource("EllipseStyleGreen");
            }

        }

        private void btSend_Click(object sender, RoutedEventArgs e)
        {
            btSend_Event(tbSend.Text, (bool)ck16Send.IsChecked);
        }


        public void btSend_Event(string strSend, bool hexState)
        {
            if (CommonRes.mySerialPort.IsOpen)
            {
                if (hexState == false)
                {
                    //if (ckAdvantechCmd.IsChecked == true) { strSend = strSend.ToUpper(); }
                    byte[] sendData = System.Text.Encoding.Default.GetBytes(strSend);
                    CommonRes.mySerialPort.Write(sendData, 0, sendData.Length);
                    txtSend.Text = Convert.ToString(Convert.ToInt32(txtSend.Text) + Convert.ToInt32(sendData.Length));
                    if (ckAdvantechCmd.IsChecked == true)
                    {
                        byte[] sendAdvCmd = _globalVM.SerialConfig.HexStringToByteArray("0D");
                        CommonRes.mySerialPort.Write(sendAdvCmd, 0, 1);
                        txtSend.Text = Convert.ToString(Convert.ToInt32(txtSend.Text) + Convert.ToInt32(sendData.Length));
                    }
                }
                else
                {
                    byte[] sendHexData = _globalVM.SerialConfig.HexStringToByteArray(strSend);
                    CommonRes.mySerialPort.Write(sendHexData, 0, sendHexData.Length);
                }
            }
            else
            {
                _globalVM.SerialConfig.TbComStateText = "串口未开";
                MessageBox.Show("串口没有打开，请检查！");
            }
        }



        private void btClearView_Click(object sender, RoutedEventArgs e)
        {

            _globalVM.SerialConfig.ReceiveCount = "0";
            _globalVM.SerialConfig.TbReceiveText = "";
        }

        private void ck16View_Click(object sender, RoutedEventArgs e)
        {

            var vm = _globalVM.SerialConfig;


            if (vm.IsHexView)
            {               
                // 从字符串切到16进制：需先将字符串转为字节数组
                byte[] bytes = Encoding.Default.GetBytes(vm.TbReceiveText);
                vm.TbReceiveText = vm.ByteArrayToHexString(bytes);               
            }
            else
            {
                // 从16进制切回字符串：需先将当前十六进制字符串转回字节数组
                byte[] bytes = vm.HexStringToByteArray(vm.TbReceiveText.Replace(" ", ""));
                vm.TbReceiveText = Encoding.Default.GetString(bytes);
            }
            
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
                time1.Interval = TimeSpan.FromSeconds(Convert.ToDouble(tbIntervalTime.Text));
                if (Convert.ToDouble(tbIntervalTime.Text) == 0)
                {
                    return;
                }
                else
                {
                    time1.Start();
                }
            }
            else
            {
                tbkIntervalTime.Visibility = Visibility.Hidden;
                tbIntervalTime.Visibility = Visibility.Hidden;
                time1.Stop();
            }
            tbReceive.ScrollToEnd();
        }

        private void ck16Send_Click(object sender, RoutedEventArgs e)
        {
            get16View((bool)ck16Send.IsChecked);           
        }



        private void get16View(bool isHex)
        {
            if (isHex == true)
            {
                //将字符器转为Ascii码
                string hexString, hexStringView = "";
                hexString = _globalVM.SerialConfig.StringToHexString(tbSend.Text);
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
            btSend_Event(tb1.Text, (bool)ck1.IsChecked);
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



        private void getData(string sendData)
        {
            tbReceive.Text += sendData;
        }

        private void txtGotoEnd()
        {
            tbReceive.ScrollToEnd();
        }

        private void txtReciveEvent(string byteNum)
        {
            txtRecive.Text = Convert.ToString(Convert.ToInt32(txtRecive.Text) + Convert.ToInt32(byteNum));
        }


        #region 核心：适配CommonRes委托的串口数据处理方法
        /// <summary>
        /// 适配CommonRes.SerialDataReceivedHandler的处理方法
        /// </summary>
        /// <param name="gbuffer">全局缓冲区</param>
        /// <param name="gb_last">缓冲区初始位置</param>
        /// <param name="buffer_len">读取的字节数</param>
        /// <param name="protocolNum">协议号</param>
        private void DebugSerialDataHandler(byte[] gbuffer, int gb_last, int buffer_len, int protocolNum)
        {

            // 1. 从全局缓冲区中提取本次读取的数据（替代原有直接读取串口的逻辑）
            byte[] buf = new byte[buffer_len];
            Array.Copy(gbuffer, gb_last, buf, 0, buffer_len); // 从gbuffer的gb_last位置复制buffer_len个字节



            // 2. 复用原有数据处理逻辑（几乎无改动）
            myUpdataHander = new HanderInterfaceUpdataDelegate(getData);
            txtGotoEndDelegate myGotoend = txtGotoEnd;
            HanderInterfaceUpdataDelegate myUpdata1 = new HanderInterfaceUpdataDelegate(txtReciveEvent);

            string abc, abc1;
            if (_globalVM.SerialConfig.IsHexView == true)
            {
                abc = _globalVM.SerialConfig.ByteArrayToHexString(buf);
                string hexStringView = "";
                for (int i = 0; i < abc.Length; i += 2)
                {
                    hexStringView += abc.Substring(i, 2) + " ";
                }
                abc = hexStringView;
                abc1 = abc.Replace(" ", "");
                if (abc1.Length >= 2 && abc1.Substring(abc1.Length - 2, 2) == "0D")
                {
                    abc = abc + "\n";
                }
            }
            else
            {
                abc = System.Text.Encoding.Default.GetString(buf);
            }

            // 3. 保留原有Dispatcher调度更新UI
            Dispatcher.Invoke(myUpdataHander, new string[] { abc });
            Dispatcher.Invoke(myGotoend);
            Dispatcher.Invoke(myUpdata1, new string[] { buffer_len.ToString() });
        }

        #endregion



        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // 页面卸载时，清空全局CurrentDataHandler（避免冲突）

            if (CommonRes.CurrentDataHandler == DebugSerialDataHandler)
            {
                CommonRes.CurrentDataHandler = null;
            }
            time1.Stop(); // 可选：停止定时器
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CommonRes.CurrentDataHandler = DebugSerialDataHandler;
        }
    }
}
