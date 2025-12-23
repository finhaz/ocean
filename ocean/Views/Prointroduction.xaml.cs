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
using ocean.Mvvm;
using ocean.ViewModels;

namespace ocean.UI
{
    /// <summary>
    /// Prointroduction.xaml 的交互逻辑
    /// </summary>
    public partial class Prointroduction : Page
    {
        private AppViewModel _globalVM = AppViewModel.Instance;

        public Prointroduction()
        {
            InitializeComponent();
            DataContext = _globalVM;
        }

    }
}
