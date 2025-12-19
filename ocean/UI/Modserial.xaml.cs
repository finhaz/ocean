using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors;
using ocean.Communication;
using ocean.Mvvm;
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
using System.IO; // 操作文件的核心命名空间

namespace ocean.UI
{
    /// <summary>
    /// Modserial.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class Modserial
    {
        private AppViewModel _globalVM = AppViewModel.Instance;
        public ICommand ButtonCommand { get; }

        public Modserial()
        {

            InitializeComponent();
            DataContext = _globalVM;
            // 初始化命令
            ButtonCommand = new RelayCommand(OnButtonClick);
 
        }

        private void OnButtonClick(object parameter)
        {

            if (parameter is DataRowView rowView)
            {
                // 处理按钮点击逻辑（例如弹出对话框）
                //MessageBox.Show($"按钮被点击，行ID：{rowView["ID"]}");
                int addr = Convert.ToInt32(rowView["Addr"]);

                if (!CommonRes.mySerialPort.IsOpen)
                {
                    MessageBox.Show("请打开串口！");
                    return;
                }
                try
                {
                    int anum = Convert.ToInt32(rowView["Command"]);
                    AppViewModel.Instance.ModbusSet.Monitor_Set(addr, anum);
                }
                catch
                {
                    MessageBox.Show("输入命令不能为空！");
                }

            }

        }


        private void setsure_click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Collapsed;
            int radd = Int32.Parse(AppViewModel.Instance.ModbusSet.Sadd);
            int rsnum = Int32.Parse(AppViewModel.Instance.ModbusSet.Snum);
            int i = 0;
            for (i = 0; i < rsnum; i++) {
                // 创建新行并赋值
                DataRow newRow = AppViewModel.Instance.ModbusSet.dtm.NewRow();
                newRow["ID"] = AppViewModel.Instance.ModbusSet.dtm.Rows.Count + 1;
                newRow["Value"] = radd;
                newRow["SelectedOption"] = AppViewModel.Instance.ModbusSet.KindNum;
                newRow["Addr"] = radd;
                newRow["Number"] = 1;
                newRow["NOffSet"] = 0;
                newRow["NBit"] = 16;
                AppViewModel.Instance.ModbusSet.dtm.Rows.Add(newRow);
                radd = radd + 1;
            }
            AppViewModel.Instance.ModbusSet.Sadd = Convert.ToString(radd);
        }

        private void setcancel_click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Collapsed;
        }

        private void cbProcho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbProcho.SelectedIndex)
            {
                
                case 0: AppViewModel.Instance.ModbusSet.KindNum = "线圈状态(RW)"; break;
                case 1: AppViewModel.Instance.ModbusSet.KindNum = "离散输入(RO)"; break;
                case 2: AppViewModel.Instance.ModbusSet.KindNum = "保持寄存器(RW)"; break;
                case 3: AppViewModel.Instance.ModbusSet.KindNum = "输入寄存器(RO)"; break;
                default: AppViewModel.Instance.ModbusSet.KindNum = "保持寄存器(RW)"; break;
                
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Visible;
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrodx.SelectedItem is DataRowView selectedRowView)
            {
                AppViewModel.Instance.ModbusSet.dtm.Rows.Remove(selectedRowView.Row);
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
                    AppViewModel.Instance.ModbusSet.Monitor_Get(addr, 1);
                    AppViewModel.Instance.ModbusSet.Readpos = Convert.ToInt32(rowView["ID"]);

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
                    //var configList = AppViewModel.Instance.ModbusSet.dtm.ToList();
                    //string json = JsonConvert.SerializeObject(configList, Formatting.Indented);

                    // 方式2：如果是DataTable
                    string json = JsonConvert.SerializeObject(AppViewModel.Instance.ModbusSet.dtm, Newtonsoft.Json.Formatting.Indented);

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
                    //AppViewModel.Instance.ModbusSet.dtm.Clear(); // 清空原有数据
                    //foreach (var item in configList)
                    //{
                    //    AppViewModel.Instance.ModbusSet.dtm.Add(item);
                    //}

                    //方式2：如果是DataTable
                    DataTable importedDt = JsonConvert.DeserializeObject<DataTable>(json);
                    AppViewModel.Instance.ModbusSet.dtm.Clear();
                    foreach (DataRow row in importedDt.Rows)
                    {
                         AppViewModel.Instance.ModbusSet.dtm.ImportRow(row);
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
    }




}
