using ocean.Communication;
using ocean.ViewModels;
using SomeNameSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;


namespace ocean.UI
{
    /// <summary>
    /// DataAnal.xaml 的交互逻辑
    /// </summary>
    public partial class MCcontrolpage : Page
    {

        private AppViewModel _globalVM = AppViewModel.Instance;

        public MCcontrolpage()
        {
            InitializeComponent();
            DataContext = _globalVM;
        }



        private void btRUN_Click(object sender, RoutedEventArgs e)
        {
            //bool brun;
            //Thread th = new Thread(new ThreadStart(test)); //创建线程
            //th.Start(); //启动线程

            //PSO_v.pso_init = false;
            //IPSO_v.pso_init = false;

            if (!CommonRes.mySerialPort.IsOpen)
            {
                MessageBox.Show("请打开串口！");
                return;
            }
            int addr = Int32.Parse(_globalVM.McController.RaddText);
            _globalVM.McController.runstop_cotnrol(addr,true);
            textBox1.Text= "系统正在运行";
        }

        private void btSTOP_Click(object sender, RoutedEventArgs e)
        {
            int addr = Int32.Parse(_globalVM.McController.RaddText);
            _globalVM.McController.runstop_cotnrol(addr,false);
            textBox1.Text = "系统停止运行";
           
        }


        private void datashow_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            _globalVM.McController.select_index = datashow.SelectedIndex;
        }


        private void datashow_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _globalVM.McController.newValue = (e.EditingElement as TextBox).Text;
            _globalVM.McController.runnum= _globalVM.McController.dtrun.Rows.Count;
        }


        private void MButton2_Click(object sender, RoutedEventArgs e)
        {
            //读取选中行
            var x = dataset.SelectedIndex;
            _globalVM.McController.mbutton_set("PARAMETER_SET", (int)x);
        }




        private void dataset_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _globalVM.McController.newValue = (e.EditingElement as TextBox).Text;
            
        }


        private void MButton3_Click(object sender, RoutedEventArgs e)
        {
            //读取选中行
            var x = datafactor.SelectedIndex;
            _globalVM.McController.mbutton_set("PARAMETER_FACTOR", (int)x);
        }

        private void datafactor_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _globalVM.McController.newValue = (e.EditingElement as TextBox).Text;
            
        }


        private void Page_Unloaded(object sender, RoutedEventArgs e)
        { 
             // 仅当当前处理者是本Page的MKlll时，才取消
            if (CommonRes.CurrentDataHandler == _globalVM.McController.HandleSerialData)
            {
                CommonRes.CurrentDataHandler = null;
            }
            // 释放MKlll资源
            //_globalVM.MKlll.Dispose();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CommonRes.CurrentDataHandler = _globalVM.McController.HandleSerialData;
        }

        public void Dispose()
        {
            Page_Unloaded(null, null);
        }

        private void cbProcho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem == null) return;

            string selectedProtocol = comboBox.SelectedItem.ToString();
            _globalVM.McController.InitProtocol(selectedProtocol);
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
    }
}
