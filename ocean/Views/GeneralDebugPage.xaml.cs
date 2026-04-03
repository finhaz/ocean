using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors;
using ocean.Communication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data;
using System.Data.Common;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Newtonsoft.Json; // 必须添加，否则JsonConvert识别不到
using System.IO;
using ocean.ViewModels; 
using ocean.Mvvm;

namespace ocean.UI
{
    /// <summary>
    /// GeneralDebugPage.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class GeneralDebugPage
    {
        private AppViewModel _globalVM = AppViewModel.Instance;

        public GeneralDebugPage()
        {

            InitializeComponent();
            DataContext = _globalVM;
 
        }

        private void setsure_click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Collapsed;
            _globalVM.ModbusSet.setsurehander(sender,e);
        }

        private void setcancel_click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Collapsed;
        }


        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Visible;
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            // 多选删除：获取所有选中的ModbusDataItem
            var selectedItems = dataGrodx.SelectedItems.Cast<ModbusDataItem>().ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("请指定行！");
                return;
            }

            // 逐个移除（避免遍历集合时修改集合导致异常）
            foreach (var item in selectedItems)
            {
                _globalVM.ModbusSet.ModbusDataList.Remove(item);
            }
        }


        private void ShowText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            // 滚动到内容末尾（常用）
            textBox.ScrollToEnd();

            // 可选：滚动到顶部
            // textBox.ScrollToHome();

            // 可选：滚动到水平末尾
            // textBox.ScrollToRightEnd();
        }

        private void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            _globalVM.ModbusSet.ExportConfig();
        }

        private void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            _globalVM.ModbusSet.ImportConfig();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _globalVM.ModbusSet.Page_LoadedD(sender,e);

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _globalVM.ModbusSet.Page_UnLoadedD(sender,e);
        }

        // 释放资源
        public void Dispose()
        {
            Page_Unloaded(null, null);
        }

        private void cPro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem == null) return;

            string selectedProtocol = comboBox.SelectedItem.ToString();
            _globalVM.ModbusSet.InitProtocol(selectedProtocol);
        }
    }




}
