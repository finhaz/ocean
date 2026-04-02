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


            bdExpend.Visibility = Visibility.Hidden;

            if (_globalVM.SerialConfig._serialComm != null && _globalVM.SerialConfig._serialComm.IsConnected)
            {
                btOpenCom.Content = "关闭串口";
                comState.Style = (Style)FindResource("EllipseStyleGreen");
            }
        }

        private void time1_Tick(object sender, EventArgs e)
        {
            if (_globalVM.SerialConfig._serialComm != null && _globalVM.SerialConfig._serialComm.IsConnected)
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
            _globalVM.SerialConfig.btOpenCom_Click();

            if (_globalVM.SerialConfig._serialComm.IsConnected)
            {
                comState.Style = (Style)this.FindResource("EllipseStyleGreen");
                
            }
            else
            {
                comState.Style = (Style)FindResource("EllipseStyleRed");
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

            if (!_globalVM.SerialConfig._serialComm.IsConnected)
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
            _globalVM.SerialConfig.Toggle16View((bool)ck16Send.IsChecked);
        }


        private void ckAdvantechCmd_Click(object sender, RoutedEventArgs e)
        {
            ck16Send_Click(sender, e);
            if (ckAsciiView.IsChecked == true)
            {
                _globalVM.SerialConfig.Toggle16View((bool)ckAsciiView.IsChecked);
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
            _globalVM.SerialConfig.Toggle16View((bool)ckAsciiView.IsChecked);
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _globalVM.SerialConfig.Page_UnLoadedD(sender,e);
            time1.Stop();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _globalVM.SerialConfig.Page_LoadedD(sender, e);
            if (!_globalVM.SerialConfig._serialComm.IsConnected)
            {
                comState.Style = (Style)FindResource("EllipseStyleRed");
            }
        }
    }
}
