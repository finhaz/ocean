using ocean.Interfaces;
using ocean.Mvvm;
using SomeNameSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace ocean.Communication
{
    public class MCControl:ObservableObject
    {
        //自定义运行界面的地址

        private string raddText = "128";
        public string RaddText
        {
            get => raddText;
            set => SetProperty(ref raddText, value);
        }


        //控件绑定相关

        //泛型控件
        // 定义TextBlock<string>实例（供XAML绑定）
        public TextBlock<string> SerialTextBlock { get; set; }

        private string bshowcontet = "数据采集";
        public string BshowContent
        {
            get => bshowcontet;
            set => SetProperty(ref bshowcontet, value); // 一行搞定，无需重复逻辑
        }

        private string protocolNum = "Modbus协议";
        public string ProtocolNum
        {
            get => protocolNum;
            set => SetProperty(ref protocolNum, value);
        }


        public ICommand ButtonIncrease { get; }

        //数据处理
        public DataTable dtrun { get; set; }
        public DataTable dtset { get; set; }
        public DataTable dtfactor { get; set; }


        //数据库对接
        public int select_index;
        public string newValue;
        public bool brun = false;
        public bool bshow = false;
        //bool bmodify = false;
        //bool Initialized = true;//调试参数与修正系数核验标志位
        public bool flag_uncon = false;//上下参数不一致标志位
        //bool flag_get_runvalue = false;//读取运行数据标志位


        public bool flag_under_first = false;
        public bool flag_upper_first = false;

        public bool sopen = false;
        public float old_value;
        public float new_value;
        public int sn = 0;
        public int mrow;
        public int Num_time = 0;
        public int Num_DSP = 0;
        static int Set_Num_DSP = 2;//代表有机子数

        public Data_r[] data = new Data_r[200];
        public int runnum;
        public byte[] sendbf = new byte[128];

        //定时器
        private DispatcherTimer mDataTimer = null; //定时器
        private long timerExeCount = 0; //定时器执行次数
        DateTime s1;
        DateTime s2;

        // 核心：当前选中的协议实例（直接复用原有单例）
        private IProtocol _currentProtocol;

        public MCControl() 
        {

            SerialTextBlock = new TextBlock<string>();
            // 初始化命令：执行逻辑 + 可执行判断
            ButtonIncrease = new MyCommand(
                execAction: ButtonIncreaseAction,
                changeFunc: parameter => true     // 可选：可执行条件（始终返回true）
            );

            InitTimer();
            LoadDataFromDatabase();
            
        }

        // 协议切换（选择框事件调用）
        public void InitProtocol(string protocolNum)
        {
            // 直接赋值原有单例，无需新建对象
            _currentProtocol = protocolNum switch
            {
                "FE协议" => COMFE.Instance,       // COMFE实现了IProtocol
                "Modbus协议" => COMModbus.Instance, // COMModbus实现了IProtocol
                _ => throw new ArgumentException($"不支持的协议类型：{protocolNum}")
            };
        }


        /// <summary>
        /// 从数据库加载数据的核心方法（内聚初始化逻辑）
        /// </summary>
        public void LoadDataFromDatabase()
        {
            // 读取数据库表，赋值给对应属性
            dtrun = DB_SQLlite.GetDBTable("PARAMETER_RUN");
            dtset = DB_SQLlite.GetDBTable("PARAMETER_SET");
            dtfactor = DB_SQLlite.GetDBTable("PARAMETER_FACTOR");

            // 可选：处理空表情况（避免后续操作空引用）
            dtrun ??= new DataTable("PARAMETER_RUN");
            dtset ??= new DataTable("PARAMETER_SET");
            dtfactor ??= new DataTable("PARAMETER_FACTOR");
            runnum = dtrun.Rows.Count;
        }


        public void show_stop()
        {
            if (bshow == true)
            {
                int send_num = 0;
                if (sn == runnum)
                {
                    sn = 0;
                }
                send_num = _currentProtocol.MonitorGet(sendbf, sn, data, runnum);
                sn = sn + 1;
                CommonRes.mySerialPort.Write(sendbf, 0, send_num);

            }
        }


        // 核心：业务处理方法（适配CommonRes的委托）
        public void HandleSerialData(byte[] gbuffer, int gb_last, int buffer_len)
        {           

            byte[] buffer = new byte[200];        
            int n_dsp = 0;
            int check_result = 0;

            string str = "";
            str = SerialDataProcessor.Instance.FormatSerialDataToHexString(gbuffer, buffer_len, "RX:", gb_last, true);
            // 线程安全更新UI
            UiDispatcherHelper.ExecuteOnUiThread(() =>
            {
                SerialTextBlock.Text += str;
                //ShowTextBox.ScrollToEnd();
            });

            // 4. Protocol_num=0的业务处理（原核心逻辑）
            if (ProtocolNum == "FE协议")
            {
                // 数据区未接收完整，直接返回
                if (buffer_len < gbuffer[4] + 5)
                {
                    return;
                }

                // 校验数据
                check_result = COMFE.Instance.monitor_check(gbuffer);

                // 校验成功=1的处理
                if (check_result == 1)
                {
                    n_dsp = gbuffer[7];
                    // 字节拼接为浮点数（原逻辑）
                    for (int i_k = 0; i_k < 9; i_k++)
                    {
                        // PSO_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(gbuffer, 8 + 4 * i_k);
                        // IPSO_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(gbuffer, 8 + 4 * i_k);
                        // Fish_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(gbuffer, 8 + 4 * i_k);
                    }

                    // 达到设定DSP数量的处理
                    if (n_dsp == Set_Num_DSP)
                    {
                        // 原线程逻辑：建议用Task替代Thread，避免线程创建开销
                        // Task.Run(() => PSO_v.cale_pso());
                        // Task.Run(() => update_UI_PSO());
                    }
                }
                // 校验成功=2的处理
                else if (check_result == 2)
                {
                    float temp_val = BitConverter.ToSingle(gbuffer, 8);

                    // 处理数据库数据更新
                    if (data[gbuffer[5]].SN == gbuffer[5])
                    {
                        // 提取核心索引变量，减少重复取值
                        int bufferVal = gbuffer[5];
                        if (bufferVal < 44)
                        {
                            int dataIndex = bufferVal;
                            data[dataIndex].VALUE = temp_val;
                            // 抽取UI更新逻辑，减少冗余注释（注释可统一放方法/常量处）
                            UpdateUiForRunTable(dataIndex);
                        }
                        else
                        {
                            // 扁平化嵌套：合并vindex和ovla的赋值逻辑
                            (int vindex, float ovla, string dbTable) = bufferVal switch
                            {
                                < 90 => (data[bufferVal].SN - 44, Convert.ToSingle(dtset.Rows[data[bufferVal].SN - 44][5]), "PARAMETER_SET"),
                                _ => (data[bufferVal].SN - 90, Convert.ToSingle(dtfactor.Rows[data[bufferVal].SN - 90][2]), "PARAMETER_FACTOR")
                            };

                            if (temp_val != ovla)
                            {
                                if (!flag_under_first)
                                {
                                    flag_uncon = true; // 上下参数不一致
                                }
                                else
                                {
                                    // 抽取UI更新+数据库保存逻辑，避免重复判断bufferVal
                                    UpdateUiAndSaveDb(bufferVal, vindex, temp_val, dbTable);
                                }
                            }
                        }
                    }
                    data[gbuffer[5]].ACK = gbuffer[7];
                }
            }
        }


        /// --------------- 抽取的辅助方法（可放在当前类中） ---------------
        /// <summary>
        /// 更新运行表UI
        /// </summary>
        private void UpdateUiForRunTable(int dataIndex)
        {
            UiDispatcherHelper.ExecuteOnUiThread(() =>
            {
                dtrun.Rows[data[dataIndex].SN][5] = data[dataIndex].VALUE;
            });
        }

        /// <summary>
        /// 跨线程更新UI并保存数据库
        /// </summary>
        private void UpdateUiAndSaveDb(int bufferVal, int vindex, float tempVal, string dbTable)
        {
            UiDispatcherHelper.ExecuteOnUiThread(() =>
            {
                if (bufferVal < 90)
                {
                    dtset.Rows[vindex][5] = tempVal;
                }
                else
                {
                    dtfactor.Rows[vindex][2] = tempVal;
                }
                DB_SQLlite.Instance.DataBase_SET_Save(dbTable, tempVal, (byte)bufferVal);
            });
        }



        public void runstop_cotnrol(int addr, bool pbrun)
        {

            int send_num = 0;
            brun = pbrun;
            //textBox1.Text = "系统停止运行";

            if (_currentProtocol == null)
                throw new InvalidOperationException("请先选择协议！");

            send_num = _currentProtocol.MonitorRun(sendbf, brun, addr);

            CommonRes.mySerialPort.Write(sendbf, 0, send_num);
            string txt = "TX:";
            txt= SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num, "TX:", true);

            
            // 线程安全更新UI
            UiDispatcherHelper.ExecuteOnUiThread(() =>
            {
                SerialTextBlock.Text += txt;
                //txtSerialLog.ScrollToEnd();
            });
        }

        public void mbutton_set(string table, int x)
        {
            string y;
            int z;
            int tempsn;
            string val;
            float value;
            int send_num = 0;

            if (table == "PARAMETER_FACTOR")
            {
                y = dtfactor.Rows[0][0].ToString();
                z = Convert.ToInt32(y);
                tempsn = x + z;


                val = dtfactor.Rows[x][2].ToString();
                value = Convert.ToSingle(val);
            }
            else
            {
                y = dtset.Rows[0][0].ToString();
                z = Convert.ToInt32(y);
                tempsn = x + z;


                val = dtset.Rows[x][5].ToString();
                value = Convert.ToSingle(val);
            }



            DB_SQLlite.Instance.DataBase_SET_Save(table, value, (byte)tempsn);

            if (CommonRes.mySerialPort.IsOpen == true)
            {

                if (_currentProtocol == null)
                    throw new InvalidOperationException("请先选择协议！");

                send_num = _currentProtocol.MonitorSet(sendbf, tempsn, data, value);

                CommonRes.mySerialPort.Write(sendbf, 0, send_num);
            }
            else
            {
                MessageBox.Show("请打开串口！");
            }
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

            show_stop();
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


        private void ButtonIncreaseAction(object parameter)
        {
            MessageBox.Show("h!");
            if (CommonRes.mySerialPort.IsOpen == true)
            {
                bshow = !bshow;
                if (bshow)
                {
                    BshowContent = "停止采集";
                    StartTimer();
                }
                else
                {
                    BshowContent = "开始采集";
                    StopTimer();
                }
            }
            else
            {
                MessageBox.Show("打开串口！");
            }
        }
    }
}
