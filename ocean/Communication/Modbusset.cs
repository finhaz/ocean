using ocean.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Data;
using System.Windows;

namespace ocean.Communication

{
    public class Modbusset : ObservableObject
    {
  
        public Modbusset()
        {
            dtm = new DataTable();
            AddDataTableColumns(dtm);

            zcom = new Message_modbus();
            //CommonRes.mySerialPort.DataReceived += new SerialDataReceivedEventHandler(mySerialPort_DataReceived);
        }

        // 从**字段**改为**私有字段+公共属性**（带通知）
        private string _sadd = "0"; // 初始值可根据你的业务调整
        public string Sadd // 推荐帕斯卡命名法（首字母大写），XAML绑定可兼容小写（但建议统一）
        {
            get => _sadd;
            set => SetProperty(ref _sadd, value); // 一行搞定，无需重复逻辑
        }

        private string _boxStr = string.Empty;
        public string BoxStr
        {
            get => _boxStr;
            set => SetProperty(ref _boxStr, value); // 一行搞定，无需重复逻辑
        }

        private string _kindNum = "保持寄存器(RW)";
        public string KindNum
        {
            get => _kindNum;
            set => SetProperty(ref _kindNum, value); // 一行搞定，无需重复逻辑
        }

        private string _snum = "1";
        public string Snum
        {
            get => _snum;
            set => SetProperty(ref _snum, value);
        }

        private string _dSelectedOption = "保持寄存器(RW)";
        public string DSelectedOption
        {
            get => _dSelectedOption;
            set => SetProperty(ref _dSelectedOption, value);
        }

        private string _proSelectedOption = "Modbus协议";
        public string ProSelectedOption
        {
            get => _proSelectedOption;
            set => SetProperty(ref _proSelectedOption, value);
        }

        private int _readpos;
        public int Readpos
        {
            get => _readpos;
            set => SetProperty(ref _readpos, value);
        }

        public ObservableCollection<string> Options { get; set; } = new ObservableCollection<string>
        { "线圈状态(RW)", "离散输入(RO)", "保持寄存器(RW)", "输入寄存器(RO)" };

        public ObservableCollection<string> ProOptions { get; set; } = new ObservableCollection<string>
        { "Modbus协议", "FE协议" };
        

        //针对数据协议：
        byte[] gbuffer = new byte[4096];
        int gb_index = 0;//缓冲区注入位置
        int get_index = 0;// 缓冲区捕捉位置

        public DataTable dtm { get; set; }


        public Message_modbus zcom { get; set; }
        // 提取列初始化的通用方法（便于复用，可选）
        private void AddDataTableColumns(DataTable dt)
        {
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Value", typeof(double));
            dt.Columns.Add("Command", typeof(double));
            dt.Columns.Add("IsButtonClicked", typeof(bool));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Rangle", typeof(string));
            dt.Columns.Add("SelectedOption", typeof(string));
            dt.Columns.Add("Addr", typeof(int));
            dt.Columns.Add("Number", typeof(int));
            dt.Columns.Add("NOffSet", typeof(int));
            dt.Columns.Add("NBit", typeof(int));        
        }


        // 核心：业务处理方法（适配CommonRes的委托）
        public void HandleSerialData(byte[] gbuffer, int gb_last, int buffer_len, int protocolNum)
        {
            byte[] buffer = new byte[200];
            int i = 0;
            string str = "RX:";
            int temp_Value = 0;

            // 1. 拼接串口数据字符串（原逻辑）
            for (i = 0; i < buffer_len; i++)
            {
                str += Convert.ToString(gbuffer[(gb_last + i) % gbuffer.Length], 16) + ' ';
            }
            str += '\r';

            // 2. 跨线程更新UI属性（BoxStr）
            Application.Current.Dispatcher.Invoke(() =>
            {
                BoxStr += str;
            });

            // 3. Modbus协议1的业务处理（原逻辑）
            if (protocolNum == 1)
            {
                Array.Copy(gbuffer, gb_last, buffer, 0, buffer_len);
                temp_Value = Monitor_Solve(buffer);

                if (buffer[1] == 3 && dtm.Rows.Count > 0)
                {
                    // 跨线程更新DataTable（避免UI线程异常）
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        dtm.Rows[Readpos - 1]["Value"] = temp_Value;
                    });
                }
            }
        }

        public int Monitor_Solve(byte[] buffer)
        {
            int Value = 0;
            if (buffer[1]==3)
            {
                byte[] typeBytes = new byte[buffer[2]];
                Array.Copy(buffer, 3, typeBytes, 0, buffer[2]);
                Array.Reverse(typeBytes);
                Value = BitConverter.ToInt16(typeBytes, 0);
            }
            else if (buffer[1]==6)
            {

            }
            return Value;
        }


        public void Monitor_Get(int addr,int value)
        {
            int send_num = 8;
            zcom.Monitor_Get_03(addr, value);
            CommonRes.mySerialPort.Write(zcom.sendbf, 0, send_num);

            string txt = "";
            txt = zcom.TX_showstr(zcom.sendbf, send_num);
            BoxStr += txt;
        }


        public void Monitor_Set(int addr,int value)
        {
            int send_num = 8;
            zcom.Monitor_Set_06(addr, value);
            CommonRes.mySerialPort.Write(zcom.sendbf, 0, send_num);

            string txt = "";
            txt = zcom.TX_showstr(zcom.sendbf, send_num);
            BoxStr += txt;
        }

        public void Dispose()
        {
            // 若有其他资源需释放，在此处理
        }

    }
}
