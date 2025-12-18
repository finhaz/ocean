using ocean.Communication;
using ocean.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO.Ports;
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
    public partial class Modserial
    {

        public Modbusset mcom { get; set; }
        public CommonRes ucom {  get; set; }
        public ObservableCollection<string> Options { get; set; } = new ObservableCollection<string> { "线圈状态(RW)", "离散输入(RO)", "保持寄存器(RW)", "输入寄存器(RO)" };
        public ICommand ButtonCommand { get; }


        public Modserial()
        {
            mcom = new Modbusset();
            ucom = new CommonRes();
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
                ucom.runstop_cotnrol(addr, true);
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

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CommonRes.mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(ucom.mySerialPort_DataReceived);
        }

    }
}
