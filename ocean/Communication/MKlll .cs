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

namespace ocean.Communication
{
    public class MKlll
    {



        public string rText { get; set; }

        //控件绑定相关
        public TextBox textmain { get; set; }

        //按键
        public Button btShow { get; set; }

        bool isCanExec = true;
        public ICommand ButtonIncrease => new MyCommand(ButtonIncreaseAction, MyCanExec);

        public event PropertyChangedEventHandler PropertyChanged;
        //数据处理
        public DataTable dtrun { get; set; }
        public DataTable dtset { get; set; }
        public DataTable dtfactor { get; set; }


        //数据库对接
        public int select_index;
        //DataBase_Interface DB_Com = new DataBase_Interface();
        public DB_sqlite DB_Com = new DB_sqlite();
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


        //通讯协议
        public Message NYS_com = new Message();
        public Message_modbus FCOM2 = new Message_modbus();


        //定时器
        private DispatcherTimer mDataTimer = null; //定时器
        private long timerExeCount = 0; //定时器执行次数
        DateTime s1;
        DateTime s2;



        public MKlll() 
        {
            textmain = new TextBox();

            btShow = new Button();

            btShow.Content = "数据采集";

            dtrun = CommonRes.dt1;

            dtset = CommonRes.dt2;

            dtfactor = CommonRes.dt3;

            CommonRes.Protocol_num = 1;

            rText = "128";

            //CommonRes.mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(mySerialPort_DataReceived);

            //CommonRes.mySerialPort.DataReceived += new SerialDataReceivedEventHandler(mySerialPort_DataReceived);

            DB_Com.runnum = dtrun.Rows.Count;

            InitTimer();
        }


            public void show_stop()
            {
                if (bshow == true)
                {
                    if (CommonRes.Protocol_num == 0)
                    {
                        if (sn == DB_Com.runnum)
                        {
                            sn = 0;
                            //    DB_Com.DataBase_RUN_Save();
                        }

                        NYS_com.Monitor_Get((byte)sn, (byte)DB_Com.data[sn].COMMAND);

                        CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);

                        sn = sn + 1;

                    }
                    else if (CommonRes.Protocol_num == 1)
                    {
                        //DB_Com.DataBase_RUN_Save();

                        FCOM2.Monitor_Get_03(0, DB_Com.runnum);

                        CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, 8);
                    }

                }
            }


        // 核心：业务处理方法（适配CommonRes的委托）
        public void HandleSerialData(byte[] gbuffer, int gb_last, int buffer_len, int protocolNum)
        {
            // 1. 处理Protocol_num=0时的延迟（原逻辑）
            if (protocolNum == 0)
            {
                Thread.Sleep(80); // 照顾粒子群的非环形缓冲读取法
            }

            byte[] buffer = new byte[200];
            int i = 0;
            string str = "RX:";
            int n_dsp = 0;
            int check_result = 0;

            // 2. 拼接串口数据字符串（原逻辑）
            for (i = 0; i < buffer_len; i++)
            {
                str += Convert.ToString(gbuffer[(gb_last + i) % gbuffer.Length], 16) + ' ';
            }
            str += '\r';

            // 3. 调用output方法（注意跨线程：若output更新UI，需用Dispatcher）
            Application.Current.Dispatcher.Invoke(() =>
            {
                output(str);
            });

            // 4. Protocol_num=0的业务处理（原核心逻辑）
            if (protocolNum == 0)
            {
                // 数据区未接收完整，直接返回
                if (buffer_len < gbuffer[4] + 5)
                {
                    return;
                }

                // 校验数据
                check_result = NYS_com.monitor_check(gbuffer);

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
                    if (DB_Com.data[gbuffer[5]].SN == gbuffer[5])
                    {
                        if (gbuffer[5] < 44)
                        {
                            DB_Com.data[gbuffer[5]].VALUE = temp_val;
                            // 跨线程更新DataTable（避免UI线程异常）
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                dtrun.Rows[DB_Com.data[gbuffer[5]].SN][5] = DB_Com.data[gbuffer[5]].VALUE;
                            });
                        }
                        else
                        {
                            float ovla;
                            int vindex;

                            if (gbuffer[5] < 90)
                            {
                                vindex = DB_Com.data[gbuffer[5]].SN - 44;
                                ovla = Convert.ToSingle(dtset.Rows[vindex][5]);
                            }
                            else
                            {
                                vindex = DB_Com.data[gbuffer[5]].SN - 90;
                                ovla = Convert.ToSingle(dtfactor.Rows[vindex][2]);
                            }

                            if (temp_val != ovla)
                            {
                                if (!flag_under_first)
                                {
                                    flag_uncon = true; // 上下参数不一致
                                }
                                else
                                {
                                    // 跨线程更新DataTable和数据库
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        if (gbuffer[5] < 90)
                                        {
                                            dtset.Rows[vindex][5] = temp_val;
                                            DB_Com.DataBase_SET_Save("PARAMETER_SET", temp_val, (byte)gbuffer[5]);
                                        }
                                        else
                                        {
                                            dtfactor.Rows[vindex][2] = temp_val;
                                            DB_Com.DataBase_SET_Save("PARAMETER_FACTOR", temp_val, (byte)gbuffer[5]);
                                        }
                                    });
                                }
                            }
                        }
                    }
                    DB_Com.data[gbuffer[5]].ACK = gbuffer[7];
                }
            }
        }



            private delegate void outputDelegate(string para);
        private void output(string para)
        {
            textmain.Dispatcher.Invoke(new outputDelegate(outputAction), para);
        }
        private void outputAction(string para)
        {
            textmain.Text += para;
        }

        public void runstop_cotnrol(int addr, bool pbrun)
        {
            //bool brun;
            int send_num = 0;
            brun = pbrun;
            //textBox1.Text = "系统停止运行";
            if (CommonRes.Protocol_num == 0)//FE协议
            {
                NYS_com.Monitor_Run(brun);
                send_num = NYS_com.sendbf[4] + 5;
                CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, send_num);
            }
            else if (CommonRes.Protocol_num == 1)//modbus
            {
                //1号机1通道
                FCOM2.Monitor_Run(1, addr, brun);
                send_num = 8;
                CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, send_num);
            }

            string txt = "TX:";
            for (int i = 0; i < send_num; i++)
            {
                if (CommonRes.Protocol_num == 0)
                {
                    txt += Convert.ToString(NYS_com.sendbf[i], 16);
                }
                else if (CommonRes.Protocol_num == 1)
                {
                    txt += Convert.ToString(FCOM2.sendbf[i], 16);
                }
                txt += ' ';
            }
            txt += '\r';
            txt += '\n';
            //show_text.Text+=txt;
            textmain.Text += txt;
        }

        public void mbutton_set(string table, int x)
        {
            string y;
            int z;
            int tempsn;
            string val;
            float value;

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



            DB_Com.DataBase_SET_Save(table, value, (byte)tempsn);

            if (CommonRes.mySerialPort.IsOpen == true)
            {
                if (CommonRes.Protocol_num == 0)
                {
                    NYS_com.Monitor_Set((byte)tempsn, (byte)(DB_Com.data[tempsn].COMMAND), value);
                    CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);
                }
                else if (CommonRes.Protocol_num == 1)
                {
                    FCOM2.Monitor_Set_06(tempsn, value);
                    CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, 8);
                }
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

        private bool MyCanExec(object parameter)
        {
            return isCanExec;
        }

        private void ButtonIncreaseAction(object parameter)
        {
            //MessageBox.Show("h!");
            if (CommonRes.mySerialPort.IsOpen == true)
            {
                bshow = !bshow;
                if (bshow)
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
    }
}
