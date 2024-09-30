using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Data;
using System.Threading;
using System.Windows.Documents;
using SomeNameSpace;
using System.Windows.Threading;
using ocean.UI;
using System.Windows.Controls;
using ocean.Mvvm;
using System.Windows;

namespace ocean
{
    public class CommonRes
    {
        public static SerialPort mySerialPort = new SerialPort();

        public TextBox textmain { get; set; }


        public static DataTable dt1 = new DataTable();
        public static DataTable dt2 = new DataTable();
        public static DataTable dt3 = new DataTable();


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
        public int Protocol_num = 1;
        static int Set_Num_DSP = 2;//代表有机子数


        //通讯协议
        public Message NYS_com = new Message();
        public Message_modbus FCOM2 = new Message_modbus();


        //定时器
        private DispatcherTimer mDataTimer = null; //定时器
        private long timerExeCount = 0; //定时器执行次数
        DateTime s1;
        DateTime s2;


        //针对数据协议：
        byte[] gbuffer = new byte[4096];
        int gb_index = 0;//缓冲区注入位置
        int get_index = 0;// 缓冲区捕捉位置

        public CommonRes()
        {
            textmain = new TextBox();

            dtrun = CommonRes.dt1;

            dtset = CommonRes.dt2;

            dtfactor = CommonRes.dt3;

            //CommonRes.mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(mySerialPort_DataReceived);

            CommonRes.mySerialPort.DataReceived += new SerialDataReceivedEventHandler(mySerialPort_DataReceived);

            DB_Com.runnum = dtrun.Rows.Count;

            InitTimer();

        }


        public void show_stop()
        {
            if (bshow == true)
            {
                if (Protocol_num == 0)
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
                else if (Protocol_num == 1)
                {
                    //DB_Com.DataBase_RUN_Save();

                    FCOM2.Monitor_Get_03(0, DB_Com.runnum);

                    CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, 8);
                }

            }
        }


        public void mySerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (Protocol_num == 0)
            {
                Thread.Sleep(80);//照顾当前粒子群的非环形缓冲读取法
            }

            byte[] buffer = new byte[200];
            int i = 0;
            int buffer_len = 0;

            string str = "RX:";
            int n_dsp = 0;
            int check_result = 0;
            int gb_last = gb_index;//记录上次的位置


            try
            {
                if (Protocol_num == 0)
                {
                    buffer_len = CommonRes.mySerialPort.Read(gbuffer, 0, gbuffer.Length);
                }
                else if (Protocol_num == 1)
                {
                    buffer_len = CommonRes.mySerialPort.Read(gbuffer, gb_index, (gbuffer.Length - gb_index));
                    gb_index = gb_index + buffer_len;
                    if (gb_index >= gbuffer.Length)
                        gb_index = gb_index - gbuffer.Length;
                }
            }
            catch
            {
                return;
            }


            for (i = 0; i < buffer_len; i++)
            {
                str += Convert.ToString(gbuffer[(gb_last + i) % gbuffer.Length], 16) + ' ';
            }
            str += '\r';
            //richTextBox1.Text += str;
            output(str);

            if (Protocol_num == 0)
            {
                if (buffer_len < gbuffer[4] + 5) //数据区尚未接收完整
                {
                    return;
                }

                check_result = NYS_com.monitor_check(gbuffer);

                if (check_result == 1)
                {
                    n_dsp = gbuffer[7];
                    for (int i_k = 0; i_k < 9; i_k++)//字节重新拼接为浮点数
                    {
                        //PSO_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(gbuffer, 8 + 4 * i_k);

                        //IPSO_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(gbuffer, 8 + 4 * i_k);

                        //Fish_v.u_dsp[n_dsp - 1, i_k] = BitConverter.ToSingle(gbuffer, 8 + 4 * i_k);


                    }
                    if (n_dsp == Set_Num_DSP)
                    {

                        //Thread th = new Thread(new ThreadStart(PSO_v.cale_pso)); //创建PSO线程
                        //th.Start(); //启动线程                          
                        //Thread th1 = new Thread(new ThreadStart(update_UI_PSO)); //创建UI线程
                        //th1.Start(); //启动线程


                        /*
                        Thread th = new Thread(new ThreadStart(IPSO_v.cale_pso)); //创建IPSO线程
                        th.Start(); //启动线程                          
                        Thread th1 = new Thread(new ThreadStart(update_UI_IPSO)); //创建UI线程
                        th1.Start(); //启动线程
                        */


                        /*
                        Thread th = new Thread(new ThreadStart(Fish_v.cale_fish)); //创建FISH线程
                        th.Start(); //启动线程                          
                        Thread th1 = new Thread(new ThreadStart(update_UI_FishAI)); //创建UI线程
                        th1.Start(); //启动线程
                        */

                    }
                }
                else if (check_result == 2)
                {
                    float temp_val;
                    temp_val = BitConverter.ToSingle(gbuffer, 8);

                    if (DB_Com.data[gbuffer[5]].SN == gbuffer[5])
                    {
                        if (gbuffer[5] < 44)
                        {
                            DB_Com.data[gbuffer[5]].VALUE = temp_val;
                            dtrun.Rows[DB_Com.data[gbuffer[5]].SN][5] = DB_Com.data[gbuffer[5]].VALUE;
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
                                if (flag_under_first == false)
                                {
                                    flag_uncon = true;//说明出现上下参数不一致
                                }
                                else
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

        public void runstop_cotnrol(bool pbrun)
        {
            //bool brun;
            int send_num = 0;
            brun = pbrun;
            //textBox1.Text = "系统停止运行";
            if (Protocol_num == 0)//FE协议
            {
                NYS_com.Monitor_Run(brun);
                send_num = NYS_com.sendbf[4] + 5;
                CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, send_num);
            }
            else if (Protocol_num == 1)//modbus
            {
                //1号机1通道
                FCOM2.Monitor_Run(1, 128, brun);
                send_num = 8;
                CommonRes.mySerialPort.Write(FCOM2.sendbf, 0, send_num);
            }

            string txt = "TX:";
            for (int i = 0; i < send_num; i++)
            {
                if (Protocol_num == 0)
                {
                    txt += Convert.ToString(NYS_com.sendbf[i], 16);
                }
                else if (Protocol_num == 1)
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

        public void mbutton_set(string table,int x)
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
                if (Protocol_num == 0)
                {
                    NYS_com.Monitor_Set((byte)tempsn, (byte)(DB_Com.data[tempsn].COMMAND), value);
                    CommonRes.mySerialPort.Write(NYS_com.sendbf, 0, NYS_com.sendbf[4] + 5);
                }
                else if (Protocol_num == 1)
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





    }
}
