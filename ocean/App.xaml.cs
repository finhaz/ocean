using ocean.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ocean
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 初始化全局AppViewModel
            AppViewModel.Instance.Initialize();

            // 主窗口绑定全局AppViewModel
            var mainWindow = new MainWindow();
            mainWindow.DataContext = AppViewModel.Instance;
            //mainWindow.Show();
        }
    }
}
