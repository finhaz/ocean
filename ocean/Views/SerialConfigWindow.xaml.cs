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
using System.Windows.Shapes;

namespace ocean.Views
{
    /// <summary>
    /// SerialConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SerialConfigWindow : Window
    {
        private readonly CanConfigViewModel _viewModel;
        public SerialConfigWindow(CanConfigViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }

        private void BtnRefreshPort_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RefreshPortNameList();
        }

        private void BtnOpenPort_Click(object sender, RoutedEventArgs e)
        {
            string res = _viewModel.OpenSerialPort();
            MessageBox.Show(res, "提示", MessageBoxButton.OK, res.Contains("成功") ? MessageBoxImage.Information : MessageBoxImage.Error);
        }

        private void BtnClosePort_Click(object sender, RoutedEventArgs e)
        {
            string res = _viewModel.CloseSerialPort();
            MessageBox.Show(res, "提示", MessageBoxButton.OK, res.Contains("成功") ? MessageBoxImage.Information : MessageBoxImage.Error);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
