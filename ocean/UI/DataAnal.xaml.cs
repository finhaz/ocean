using ocean.Communication;
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
    public partial class DataAnal : Page
    {
        public CommonRes ucom { get; set; }

        public DataAnal()
        {
            ucom = new CommonRes();
            InitializeComponent();        
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
            int addr = Int32.Parse(ucom.rText);
            ucom.runstop_cotnrol(addr,true);
            textBox1.Text= "系统正在运行";
        }

        private void btSTOP_Click(object sender, RoutedEventArgs e)
        {
            int addr = Int32.Parse(ucom.rText);
            ucom.runstop_cotnrol(addr,false);
            textBox1.Text = "系统停止运行";
           
        }


        private void datashow_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            ucom.select_index = datashow.SelectedIndex;
        }


        private void datashow_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ucom.newValue = (e.EditingElement as TextBox).Text;
            ucom.DB_Com.runnum=ucom.dtrun.Rows.Count;
        }


        private void MButton2_Click(object sender, RoutedEventArgs e)
        {
            //读取选中行
            var x = dataset.SelectedIndex;
            ucom.mbutton_set("PARAMETER_SET", (int)x);
        }




        private void dataset_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ucom.newValue = (e.EditingElement as TextBox).Text;
            CommonRes.dt2 = ucom.dtset;
        }


        private void MButton3_Click(object sender, RoutedEventArgs e)
        {
            //读取选中行
            var x = datafactor.SelectedIndex; 
            ucom.mbutton_set("PARAMETER_FACTOR", (int)x);
        }

        private void datafactor_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ucom.newValue = (e.EditingElement as TextBox).Text;
            CommonRes.dt3 = ucom.dtfactor;
        }


        private void cbProcho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {   
            switch (cbProcho.SelectedIndex)
            {
                case 0: CommonRes.Protocol_num = 0; break;
                case 1: CommonRes.Protocol_num = 1; break;
                default: CommonRes.Protocol_num = 1; break;
            }                 
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CommonRes.mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(ucom.mySerialPort_DataReceived);
        }
    }
}
