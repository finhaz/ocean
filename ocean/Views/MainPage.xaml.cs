using ocean.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ocean.Views
{
    public partial class MainPage : Page
    {
        private ShellViewModel _shellViewModel;

        public MainPage()
        {
            InitializeComponent();

            // 获取ShellViewModel（需保证Shell是主窗口）
            var shellWindow = Application.Current.MainWindow as MainWindow;
            if (shellWindow != null)
            {
                _shellViewModel = shellWindow.DataContext as ShellViewModel;
            }
        }

        // RadioButton选择事件
        private void RdoCommunication_Checked(object sender, RoutedEventArgs e)
        {
            if (_shellViewModel == null) return;

            var radio = sender as RadioButton;
            if (radio == RdoSerial)
            {
                // 切换为串口配置
                _shellViewModel.UpdateCommunicationMenu(CommunicationType.SerialPort);
            }
            else if (radio == RdoEthernet)
            {
                // 切换为以太网配置
                _shellViewModel.UpdateCommunicationMenu(CommunicationType.Ethernet);
            }
        }
    }
}
