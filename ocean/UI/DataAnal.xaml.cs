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
using SomeNameSpace;
using static System.Net.Mime.MediaTypeNames;


namespace ocean.UI
{
    /// <summary>
    /// DataAnal.xaml 的交互逻辑
    /// </summary>
    public partial class DataAnal : Page
    {
        //定时器
        private DispatcherTimer mDataTimer = null; //定时器
        private long timerExeCount = 0; //定时器执行次数
        DateTime s1;
        DateTime s2;

        public CommonRes ucom { get; set; }



        public DataAnal()
        {
            InitializeComponent();
         
        }


        private void InitTimer()
        {
            if (mDataTimer == null)
            {
                mDataTimer = new DispatcherTimer();
                mDataTimer.Tick += new EventHandler(DataTimer_Tick);
                mDataTimer.Interval = TimeSpan.FromSeconds(10);
            }
        }
        private void DataTimer_Tick(object sender, EventArgs e)
        {
            s2 = DateTime.Now;
            s1 = DateTime.Now;
            ++timerExeCount;

            ucom.show_stop();
        }

        public void StartTimer()
        {
            if (mDataTimer != null && mDataTimer.IsEnabled == false)
            {
                mDataTimer.Start();
                s1 = DateTime.Now;
            }
        }
        public void StopTimer()
        {
            if (mDataTimer != null && mDataTimer.IsEnabled == true)
            {
                mDataTimer.Stop();
            }
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

            ucom.runstop_cotnrol(true);
            textBox1.Text= "系统正在运行";
        }

        private void btSTOP_Click(object sender, RoutedEventArgs e)
        {
            ucom.runstop_cotnrol(false);
            textBox1.Text = "系统停止运行";
           
        }

        private void Page_Loaded(object sender, EventArgs e)
        {
            ucom = new CommonRes();

            ucom.dtrun = CommonRes.dt1;

            ucom.dtset = CommonRes.dt2;

            ucom.dtfactor = CommonRes.dt3;

            InitTimer();
            
            CommonRes.mySerialPort.DataReceived += new SerialDataReceivedEventHandler(ucom.mySerialPort_DataReceived);
            ucom.DB_Com.runnum = ucom.dtrun.Rows.Count;
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

        private void dataset_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            //读取选中行
            var x = dataset.SelectedIndex;
        }


        private void MButton2_Click(object sender, RoutedEventArgs e)
        {
            //读取选中行
            var x = dataset.SelectedIndex;
            string y = ucom.dtset.Rows[0][0].ToString();
            int z = Convert.ToInt32(y);
            int tempsn = x + z;
            string val =  ucom.dtset.Rows[x][5].ToString();
            float value=Convert.ToSingle(val);
            ucom.DB_Com.DataBase_SET_Save("PARAMETER_SET", value, (byte)tempsn);
            ucom.mbutton_set(tempsn, value);
        }




        private void dataset_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ucom.newValue = (e.EditingElement as TextBox).Text;
            CommonRes.dt2 = ucom.dtset;
        }


        private void datafactor_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {            
            var x = datafactor.SelectedIndex;
        }


        private void MButton3_Click(object sender, RoutedEventArgs e)
        {
            //读取选中行
            var x = datafactor.SelectedIndex;           
            string y = ucom.dtfactor.Rows[0][0].ToString();
            int z = Convert.ToInt32(y);
            int tempsn = x + z;
            string val =  ucom.dtfactor.Rows[x][2].ToString();
            float value = Convert.ToSingle(val);
            ucom.DB_Com.DataBase_SET_Save("PARAMETER_FACTOR", value, (byte)tempsn);
            ucom.mbutton_set(tempsn, value);
        }

        private void datafactor_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ucom.newValue = (e.EditingElement as TextBox).Text;
            CommonRes.dt3 = ucom.dtfactor;
        }

        private void btShow_Click(object sender, RoutedEventArgs e)
        {
            if (CommonRes.mySerialPort.IsOpen == true)
            {
                ucom.bshow = !ucom.bshow;
                if (ucom.bshow)
                {
                    btShow.Content = "停止采集";
                    StartTimer();
                }
                else
                {
                    btShow.Content = "开始采集";
                    StopTimer();
                }
            }
            else
            {
                MessageBox.Show("打开串口！");
            }

        }


        private void cbProcho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {   
            if(ucom!=null)
            {
                switch (cbProcho.SelectedIndex)
                {
                    case 0: ucom.Protocol_num = 0; break;
                    case 1: ucom.Protocol_num = 1; break;
                    default: ucom.Protocol_num = 0; break;
                }
            }                  
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CommonRes.mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(ucom.mySerialPort_DataReceived);
        }
    }
}
