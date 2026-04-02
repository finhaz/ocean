using ocean.Communication;
using ocean.Interfaces;
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
using static System.Data.Odbc.ODBC32;


namespace ocean.UI
{
    /// <summary>
    /// SerialConfigPage.xaml 的交互逻辑
    /// </summary>
    public partial class SerialConfigPage : Page
    {
        private AppViewModel _globalVM = AppViewModel.Instance;
        public DispatcherTimer time1 = new DispatcherTimer();

        delegate void HanderInterfaceUpdataDelegate(string mySendData);
        HanderInterfaceUpdataDelegate myUpdataHander;
        delegate void txtGotoEndDelegate();

        // 缓存当前串口实例（避免重复获取）
        private SerialCommunication _serialComm;
        //这里不用CommunicationManager单例特性，是因为本页面专门针对的是串口
        //配置的时候专门针对串口优化，等其他页面使用就选择_comm，利用公共属性
        //private ICommunication _comm;

        public SerialConfigPage()
        {
            InitializeComponent();
            DataContext = _globalVM;

            time1.Tick += new EventHandler(time1_Tick);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            _globalVM.SerialConfig.ScrollToEndRequested += () =>
            {
                // 这里可以直接访问 tbReceive
                tbReceive.ScrollToEnd();
            };

            try
            {
                // 获取串口实例
                _serialComm = (SerialCommunication)CommunicationManager.Instance.GetCurrentCommunication();
                // 设置串口编码
                _serialComm.Encoding = System.Text.Encoding.GetEncoding("GB2312");
            }
            catch (InvalidOperationException ex)
            {
                // 未选择串口时提示（可选）
                _globalVM.SerialConfig.TbComStateText = "未初始化串口：" + ex.Message;
            }

            bdExpend.Visibility = Visibility.Hidden;

            // 替换 CommonRes.mySerialPort.IsOpen 判断
            if (_serialComm != null && _serialComm.IsConnected)
            {
                btOpenCom.Content = "关闭串口";
                comState.Style = (Style)FindResource("EllipseStyleGreen");
            }
        }

        private void time1_Tick(object sender, EventArgs e)
        {
            // 替换 CommonRes.mySerialPort.IsOpen 判断
            if (_serialComm != null && _serialComm.IsConnected)
            {
                _globalVM.SerialConfig.btSend_Event(tbSend.Text, (bool)ck16Send.IsChecked);
            }
        }

        private void tbIntervalTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ckAutoSend.IsChecked == true)
            {
                if (string.IsNullOrEmpty(tbIntervalTime.Text) || Convert.ToDouble(tbIntervalTime.Text) == 0)
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

        /// <summary>
        /// 打开/关闭串口按钮（已改造）
        /// </summary>
        private void btOpenCom_Click(object sender, RoutedEventArgs e)
        {
            if (_serialComm.IsConnected)
            {
                // 关闭串口逻辑
                _serialComm.Close();
                _globalVM.SerialConfig.IsConfigEnabled = true;
                btOpenCom.Content = "打开串口";
                _globalVM.SerialConfig.TbComStateText = cbPortName.Text + "已关闭";
                comState.Style = (Style)FindResource("EllipseStyleRed");
            }
            else
            {
                try
                {
                    if (string.IsNullOrEmpty(_globalVM.SerialConfig.PortConfig.SelectedPortName))
                    {
                        MessageBox.Show("未选择串口！");
                        return;
                    }

                    var serialConfig = new SerialConfig
                    {
                        SelectedPortName = _globalVM.SerialConfig.PortConfig.SelectedPortName,
                        SelectedBaudRate = _globalVM.SerialConfig.PortConfig.SelectedBaudRate,
                        SelectedParityName = _globalVM.SerialConfig.PortConfig.SelectedParityName,
                        SelectedStopBit = _globalVM.SerialConfig.PortConfig.SelectedStopBit,
                        SelectedDataBits = int.Parse(cbDataBits.Text)
                    };

                    _serialComm.Open(serialConfig);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("占用"))
                    {
                        _globalVM.SerialConfig.TbComStateText = cbPortName.Text + "串口被占用！";
                        MessageBox.Show("串口被占用！");
                    }
                    else
                    {
                        _globalVM.SerialConfig.TbComStateText = "串口打开失败：" + ex.Message;
                        MessageBox.Show("串口打开失败：" + ex.Message);
                    }
                    return;
                }

                _globalVM.SerialConfig.IsConfigEnabled = false;
                btOpenCom.Content = "关闭串口";
                _globalVM.SerialConfig.TbComStateText = $"{cbPortName.Text},{cbBaudRate.Text}," +
                    $"{cbParity.Text},{cbDataBits.Text},{cbStopBits.Text}";
                comState.Style = (Style)this.FindResource("EllipseStyleGreen");
            }
        }

        private void btSend_Click(object sender, RoutedEventArgs e)
        {
            _globalVM.SerialConfig.btSend_Event(tbSend.Text, (bool)ck16Send.IsChecked);
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
                byte[] bytes = Encoding.Default.GetBytes(vm.TbReceiveText);
                vm.TbReceiveText = vm.ByteArrayToHexString(bytes);
            }
            else
            {
                byte[] bytes = vm.HexStringToByteArray(vm.TbReceiveText.Replace(" ", ""));
                vm.TbReceiveText = Encoding.Default.GetString(bytes);
            }
        }

        private void ckAutoSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _serialComm = (SerialCommunication)CommunicationManager.Instance.GetCurrentCommunication();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
                ckAutoSend.IsChecked = false;
                return;
            }

            if (!_serialComm.IsConnected)
            {
                MessageBox.Show("串口未开！");
                ckAutoSend.IsChecked = false;
                return;
            }

            if (ckAutoSend.IsChecked == true)
            {
                tbkIntervalTime.Visibility = Visibility.Visible;
                tbIntervalTime.Visibility = Visibility.Visible;

                if (string.IsNullOrEmpty(tbIntervalTime.Text) || Convert.ToDouble(tbIntervalTime.Text) == 0)
                {
                    return;
                }
                else
                {
                    time1.Interval = TimeSpan.FromSeconds(Convert.ToDouble(tbIntervalTime.Text));
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

        #region 简化后的get16View方法（仅触发ViewModel逻辑）
        private void get16View(bool isHex)
        {
            _globalVM.SerialConfig.Toggle16View(isHex);
        }

        private void ckAdvantechCmd_Click(object sender, RoutedEventArgs e)
        {
            ck16Send_Click(sender, e);
            if (ckAsciiView.IsChecked == true)
            {
                get16View((bool)ckAsciiView.IsChecked);
            }
        }
        #endregion

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
            string str = bt1.Content.ToString();
            TextBox tb1 = gdExpend.FindName("expendTextBox" + str.Substring(str.Length - 1, 1)) as TextBox;
            CheckBox ck1 = gdExpend.FindName("ckExpend" + str.Substring(str.Length - 1, 1)) as CheckBox;
            _globalVM.SerialConfig.btSend_Event(tb1.Text, (bool)ck1.IsChecked);
        }

        private void tbSend_TextChanged(object sender, TextChangedEventArgs e)
        {
            _globalVM.SerialConfig.tbSend_TextChanged();         
        }

        private void ckAsciiView_Click(object sender, RoutedEventArgs e)
        {
            get16View((bool)ckAsciiView.IsChecked);
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _globalVM.SerialConfig.Page_UnLoadedD(sender,e);
            time1.Stop();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_serialComm.IsConnected)
            {
                comState.Style = (Style)FindResource("EllipseStyleRed");
            }
            _globalVM.SerialConfig.Page_LoadedD(sender, e);

        }
    }
}
