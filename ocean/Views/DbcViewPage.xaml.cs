using Microsoft.Win32;
using ocean.Mvvm;
using ocean.ViewModels;
using System;
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

namespace ocean.Views
{
    /// <summary>
    /// DbcViewPage.xaml 的交互逻辑
    /// </summary>
    public partial class DbcViewPage : Page
    {
        // 获取ViewModel实例
        private DbcViewModel _viewModel => DataContext as DbcViewModel;

        public DbcViewPage()
        {
            InitializeComponent();
            // 绑定页面卸载事件，调用你的原生卸载方法
            this.Unloaded += _viewModel.Page_UnLoadedD;
        }

        private void BtnLoadDbc_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DBC文件 (*.dbc)|*.dbc|所有文件 (*.*)|*.*";
            ofd.Title = "选择DBC描述文件";
            if (ofd.ShowDialog() == true)
            {
                _viewModel.LoadDbcFile(ofd.FileName);
            }
        }

        // ✅ 只新增这一个方法，仅此一行代码
        private void BtnSendCanFrame_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as ViewModels.DbcViewModel;
            vm?.SendCanFrameByDbc();
        }

        // 修改后的【进入AT指令模式】按钮事件 - 核心：下发指令+标记等待，不立即弹窗
        private void BtnEnterATMode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var comm = ocean.Communication.CommunicationManager.Instance.GetCurrentCommunication();
                var vm = this.DataContext as ViewModels.DbcViewModel;

                if (comm != null && comm.IsOpen)
                {
                    // 1. 标记：开始等待模块返回OK\r\n
                    vm._isWaitingATOkResponse = true;
                    // 2. 绑定回调事件（收到OK后执行弹窗）
                    vm.ATModeSuccessCallback = () =>
                    {
                        MessageBox.Show("✅ AT指令模式进入成功！模块已返回 OK\\r\\n，CAN通讯可正常工作", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        vm._isWaitingATOkResponse = false;//重置标记
                    };
                    vm.ATModeFailCallback = () =>
                    {
                        MessageBox.Show("❌ 进入AT模式失败，未收到模块OK响应", "失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                        vm._isWaitingATOkResponse = false;//重置标记
                    };
                    // 3. 下发核心指令：AT+AT\r\n 给TTL转CAN模块
                    byte[] atCmd = System.Text.Encoding.ASCII.GetBytes("AT+AT\r\n");
                    comm.SendData(atCmd);
                }
                else
                {
                    MessageBox.Show("⚠️ 串口未打开！请先打开串口再执行该操作", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ 下发AT指令异常：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                var vm = this.DataContext as ViewModels.DbcViewModel;
                vm._isWaitingATOkResponse = false;
            }
        }

    }
}
