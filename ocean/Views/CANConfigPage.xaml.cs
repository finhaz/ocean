using ocean.Communication;
using ocean.Interfaces;
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
    public partial class CANConfigPage : Page, IDisposable
    {
        private readonly CanConfigViewModel _viewModel;
        ICommunication comm;

        public CANConfigPage()
        {
            InitializeComponent();
            
            comm = CommunicationManager.Instance.GetCurrentCommunication();
            _viewModel = new CanConfigViewModel(comm);
            this.DataContext = _viewModel;
        }

        // 新增：打开串口配置弹窗
        private void BtnSerialConfig_Click(object sender, RoutedEventArgs e)
        {
            SerialConfigWindow window = new SerialConfigWindow(_viewModel);
            window.ShowDialog();


        }

        private async void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.ReadCanConfigAsync();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveCanConfigAsync();
        }

        private async void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.RestoreDefaultAsync();
        }

        private void CANConfigPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _viewModel.Dispose();
        }

        public void Dispose()
        {
            _viewModel?.Dispose();
        }
    }
}
