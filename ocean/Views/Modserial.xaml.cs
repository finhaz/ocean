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
    /// Modserial.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class Modserial
    {
        private AppViewModel _globalVM = AppViewModel.Instance;

        public Modserial()
        {

            InitializeComponent();
            DataContext = _globalVM;
 
        }

        private void setsure_click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Collapsed;
            int radd = Int32.Parse(_globalVM.ModbusSet.Sadd);
            int rsnum = Int32.Parse(_globalVM.ModbusSet.Snum);
            int i = 0;
            for (i = 0; i < rsnum; i++) {
                // 创建新行并赋值
                DataRow newRow = _globalVM.ModbusSet.dtm.NewRow();
                newRow["ID"] = _globalVM.ModbusSet.dtm.Rows.Count + 1;
                newRow["Value"] = radd;
                newRow["SelectedOption"] = _globalVM.ModbusSet.DSelectedOption;
                newRow["Addr"] = radd;
                newRow["Number"] = 1;
                newRow["NOffSet"] = 0;
                newRow["NBit"] = 16;
                _globalVM.ModbusSet.dtm.Rows.Add(newRow);
                radd = radd + 1;
            }
            _globalVM.ModbusSet.Sadd = Convert.ToString(radd);
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
            if (dataGrodx.SelectedItem is DataRowView selectedRowView)
            {
                _globalVM.ModbusSet.dtm.Rows.Remove(selectedRowView.Row);
            }
            else
            {
                MessageBox.Show("请指定行！");
            }
        }



        private void DataGrodx_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 1. 查找点击的单元格（核心：通过可视化树找DataGridCell）
            var cell = FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
            if (cell == null) return;

            // 2. 判断是否是数值列
            if (cell.Column.Header.ToString() == "数值")
            {
                // 3. 提取数值
                var rowView = cell.DataContext as DataRowView;
                if (rowView != null)
                {
                    object value = rowView["Value"];
                    value = value == DBNull.Value ? "空值" : value;
                    MessageBox.Show($"当前数值：{value}", "数值详情");
                    int addr = Convert.ToInt32(rowView["Addr"]);
                    _globalVM.ModbusSet.ReadButtonHander(addr, 1);
                    _globalVM.ModbusSet.Readpos = Convert.ToInt32(rowView["ID"]);

                }
            }
        }

        // 辅助方法：向上查找可视化树中的指定类型元素（必须有这个方法！）
        private T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T target)
                {
                    return target;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
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
            // 打开保存文件对话框
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "导出Modbus配置",
                FileName = $"ModbusConfig_{DateTime.Now:yyyyMMddHHmmss}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    //// 方式1：如果是ObservableCollection<ModbusConfigItem>
                    //var configList = _globalVM.ModbusSet.dtm.ToList();
                    //string json = JsonConvert.SerializeObject(configList, Formatting.Indented);

                    // 方式2：如果是DataTable
                    string json = JsonConvert.SerializeObject(_globalVM.ModbusSet.dtm, Newtonsoft.Json.Formatting.Indented);

                    // 写入文件
                    File.WriteAllText(saveFileDialog.FileName, json);
                    MessageBox.Show("导出成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            // 打开文件选择对话框
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON配置文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Title = "导入Modbus配置"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(openFileDialog.FileName);

                    //// 方式1：如果是ObservableCollection<ModbusConfigItem>
                    //var configList = JsonConvert.DeserializeObject<List<ModbusConfigItem>>(json);
                    //_globalVM.ModbusSet.dtm.Clear(); // 清空原有数据
                    //foreach (var item in configList)
                    //{
                    //    _globalVM.ModbusSet.dtm.Add(item);
                    //}

                    //方式2：如果是DataTable
                    DataTable importedDt = JsonConvert.DeserializeObject<DataTable>(json);
                    _globalVM.ModbusSet.dtm.Clear();
                    foreach (DataRow row in importedDt.Rows)
                    {
                         _globalVM.ModbusSet.dtm.ImportRow(row);
                    }

                    MessageBox.Show("导入成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 将Modbusset的HandleSerialData方法注册为当前活跃处理者
            CommonRes.CurrentDataHandler = _globalVM.ModbusSet.HandleSerialData;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // 仅当当前处理者是本Page的Modbusset时，才取消
            if (CommonRes.CurrentDataHandler == _globalVM.ModbusSet.HandleSerialData)
            {
                CommonRes.CurrentDataHandler = null;
            }
            // 释放Modbusset资源
            _globalVM.ModbusSet.Dispose();
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
