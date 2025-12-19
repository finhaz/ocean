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
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace ocean.UI
{
    /// <summary>
    /// Modserial.xaml 的交互逻辑
    /// </summary>
    /// 
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }


    public class TextBoxAutoScrollBehavior : Behavior<TextBox>
    {
        // 可选：定义是否滚动到顶部的属性（默认滚动到顶部）
        public bool ScrollToTop
        {
            get { return (bool)GetValue(ScrollToTopProperty); }
            set { SetValue(ScrollToTopProperty, value); }
        }

        public static readonly DependencyProperty ScrollToTopProperty =
            DependencyProperty.Register("ScrollToTop", typeof(bool), typeof(TextBoxAutoScrollBehavior), new PropertyMetadata(true));

        // 附加到TextBox时
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += OnTextChanged;
        }

        // 从TextBox分离时
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextChanged -= OnTextChanged;
        }

        // 文本变化时滚动
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (AssociatedObject == null) return;

            if (ScrollToTop)
            {
                AssociatedObject.ScrollToHome(); // 滚动到顶部
            }
            else
            {
                AssociatedObject.ScrollToEnd(); // 滚动到底部
            }
        }
    }


    public partial class Modserial
    {

        public Modbusset mcom { get; set; }


        public ObservableCollection<string> Options { get; set; } = new ObservableCollection<string> { "线圈状态(RW)", "离散输入(RO)", "保持寄存器(RW)", "输入寄存器(RO)" };
        public ICommand ButtonCommand { get; }

        // 定义变量记录上一次点击的信息（避免多个TextBlock互相干扰）
        private DateTime _lastClickTime;
        private TextBlock _lastClickedTextBlock;

        public Modserial()
        {
            mcom = new Modbusset();

            InitializeComponent();
            this.DataContext = this;
            dataGrodx.DataContext = this;
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
                //ucom.runstop_cotnrol(addr, true);
                try
                {
                    int anum = Convert.ToInt32(rowView["Command"]);
                    mcom.Monitor_Set(addr, anum);
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
            int radd = Int32.Parse(mcom.Sadd);
            int rsnum = Int32.Parse(mcom.snum);
            int i = 0;
            for (i = 0; i < rsnum; i++) {
                // 创建新行并赋值
                DataRow newRow = mcom.dtm.NewRow();
                newRow["ID"] = mcom.dtm.Rows.Count + 1;
                newRow["Value"] = radd;
                newRow["SelectedOption"] = mcom.kind_num;
                newRow["Addr"] = radd;
                newRow["Number"] = 1;
                newRow["NOffSet"] = 0;
                newRow["NBit"] = 16;
                mcom.dtm.Rows.Add(newRow);
                radd = radd + 1;
            }
            mcom.Sadd = Convert.ToString(radd);
        }

        private void setcancel_click(object sender, RoutedEventArgs e)
        {
            Setadd.Visibility = Visibility.Collapsed;
        }

        private void cbProcho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbProcho.SelectedIndex)
            {
                
                case 0: mcom.kind_num = "线圈状态(RW)"; break;
                case 1: mcom.kind_num = "离散输入(RO)"; break;
                case 2: mcom.kind_num = "保持寄存器(RW)"; break;
                case 3: mcom.kind_num = "输入寄存器(RO)"; break;
                default: mcom.kind_num = "保持寄存器(RW)"; break;
                
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
                mcom.dtm.Rows.Remove(selectedRowView.Row);
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
                    mcom.Monitor_Get(addr, 1);
                    mcom.readpos = Convert.ToInt32(rowView["ID"]);

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


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CommonRes.mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(mcom.mySerialPort_DataReceived);
        }

    }
}
