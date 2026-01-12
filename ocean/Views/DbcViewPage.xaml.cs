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
    }
}
