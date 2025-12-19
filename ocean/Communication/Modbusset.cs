using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ocean.Communication

{
    public class Modbusset : INotifyPropertyChanged
    {
        // 1. 声明属性变更事件
        public event PropertyChangedEventHandler PropertyChanged;
        //针对数据协议：
        byte[] gbuffer = new byte[4096];
        int gb_index = 0;//缓冲区注入位置
        int get_index = 0;// 缓冲区捕捉位置

        

        // 2. 触发事件的方法（供属性的set调用）
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 3. 将sadd从**字段**改为**私有字段+公共属性**（带通知）
        private string _sadd = "0"; // 初始值可根据你的业务调整
        public string Sadd // 推荐帕斯卡命名法（首字母大写），XAML绑定可兼容小写（但建议统一）
        {
            get => _sadd;
            set
            {
                if (_sadd != value)
                {
                    _sadd = value;
                    OnPropertyChanged(); // 关键：值变化时触发通知，UI会同步更新
                }
            }
        }
        private string _boxStr = string.Empty;
        public string BoxStr
        {
            get => _boxStr;
            set
            {
                if (_boxStr != value)
                {
                    _boxStr = value;
                    // 触发事件，通知UI更新
                    OnPropertyChanged(nameof(BoxStr));
                }
            }
        }


        public string kind_num ;
        public string snum { get; set; }
        public string dSelectedOption { get; set; }
        public static DataTable dt1 = new DataTable();
        public DataTable dtm { get; set; }

        public int readpos = 0;

        public Message_modbus zcom { get; set; }
        static Modbusset()
        {
            dt1.Columns.Add("ID", typeof(int));
            dt1.Columns.Add("Name", typeof(string));
            dt1.Columns.Add("Value", typeof(double));
            dt1.Columns.Add("Command", typeof(double));
            dt1.Columns.Add("IsButtonClicked", typeof(bool));
            dt1.Columns.Add("Unit", typeof(string));
            dt1.Columns.Add("Rangle", typeof(string));
            dt1.Columns.Add("SelectedOption", typeof(string));
            dt1.Columns.Add("Addr", typeof(int));
            dt1.Columns.Add("Number", typeof(int));
            dt1.Columns.Add("NOffSet", typeof(int));
            dt1.Columns.Add("NBit", typeof(int));

            
        }

        public Modbusset() 
        {

            dtm = dt1;
            kind_num = "保持寄存器(RW)";
            Sadd = "0";
            snum = "1";
            dSelectedOption = "保持寄存器(RW)";
            zcom = new Message_modbus();

            CommonRes.mySerialPort.DataReceived += new SerialDataReceivedEventHandler(mySerialPort_DataReceived);
        }


        public void mySerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] buffer = new byte[200];
            int i = 0;
            int buffer_len = 0;

            string str = "RX:";
            int n_dsp = 0;
            int check_result = 0;
            int gb_last = gb_index;//记录上次的位置

            int temp_Value;

            try
            {
                if (CommonRes.Protocol_num == 0)
                {
                    buffer_len = CommonRes.mySerialPort.Read(gbuffer, 0, gbuffer.Length);
                }
                else if (CommonRes.Protocol_num == 1)
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
            //output(str);
            BoxStr += str;

            if(CommonRes.Protocol_num==1)
            {
                Array.Copy(gbuffer, gb_last, buffer, 0, buffer_len);
                temp_Value =Monitor_Solve(buffer);
                if (buffer[1] == 3)
                {
                    if (dtm.Rows.Count > 0)
                    {
                        dtm.Rows[readpos - 1]["Value"] = temp_Value;
                    }
                }
            }
            else
            {

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

    }
}
