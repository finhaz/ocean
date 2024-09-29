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
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Page
    {
        public TextBox<string> ChangeTextBox { get; set; }

        public Settings()
        {
            InitializeComponent();

            ChangeTextBox = new TextBox<string>();
            ChangeTextBox.TextChangeCallBack = (text) => { MessageBox.Show(text); };//声明TextChange
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
         
            int i = 1;
        }
    }
}
