using ocean.Mvvm;
using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel;
using System.Data;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ocean.Communication

{
    public class Modbusset : ObservableObject
    {
        public byte[] sendbf = new byte[128];

        public Modbusset()
        {
            dtm = new DataTable();
            AddDataTableColumns(dtm);
            ButtonCommand = new RelayCommand(OnButtonClick);

        }

        public ICommand ButtonCommand { get; }


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



        private string _intoSelectedOption = "Modbus协议";
        public string IntoSelectedOption
        {
            get => _intoSelectedOption;
            set
            {
                // 调用基类的SetProperty赋值，若值变化则触发通知
                if (SetProperty(ref _intoSelectedOption, value))
                {
                    // 关键：当选中的协议名称变更时，主动通知SelectedProtocol属性变更
                    OnPropertyChanged(nameof(SelectedProtocol));
                }
            }
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


        // 协议信息集合
        public ObservableCollection<ProtocolInfo> ProtocolList { get; set; } = new ObservableCollection<ProtocolInfo>
        {
            new ProtocolInfo
            {
                Name = "Modbus协议",
                FrameStructure = "地址域(1字节) + 功能码(1字节) + 数据域(N字节) + CRC校验(2字节)",
                Transport = "RTU/ASCII/TCP三种，RTU为二进制格式，效率更高",
                Features = "常用功能码：03（读保持寄存器）、06（写单个寄存器）"
            },
            new ProtocolInfo
            {
                Name = "FE协议",
                FrameStructure = "帧头(2字节: 0xFE 0x01) + 设备地址(1字节) + 数据长度(2字节) + 数据域(N字节) + 校验和(1字节)",
                Transport = "基于串口的私有协议，波特率默认9600，8N1格式",
                Features = "支持广播帧，数据域采用小端序存储"
            }
        };

        // 新增：筛选后的当前协议（核心属性）
        public ProtocolInfo SelectedProtocol
        {
            get
            {
                // 根据选中的协议名称，从ProtocolList中筛选对应对象
                return ProtocolList.FirstOrDefault(p => p.Name == IntoSelectedOption);
            }
        }

        //针对数据协议：
        byte[] gbuffer = new byte[4096];
        int gb_index = 0;//缓冲区注入位置
        int get_index = 0;// 缓冲区捕捉位置

        public DataTable dtm { get; set; }


        //public Message_modbus zcom { get; set; }
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
        public void HandleSerialData(byte[] gbuffer, int gb_last, int buffer_len)
        {
            byte[] buffer = new byte[200];
            int i = 0;
            int temp_Value = 0;

            string str = "";
            str = SerialDataProcessor.Instance.FormatSerialDataToHexString(gbuffer, buffer_len, "RX:", gb_last, true);
            // 线程安全更新UI
            UiDispatcherHelper.ExecuteOnUiThread(() =>
            {
                BoxStr += str;               
            });

            // 3. Modbus协议1的业务处理（原逻辑）
            if (ProSelectedOption == "Modbus协议")
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
            COMModbus.Instance.Monitor_Get_03(sendbf, addr, value);
            CommonRes.mySerialPort.Write(sendbf, 0, send_num);

            string txt = "";
            txt = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num,"TX:",true);
            BoxStr += txt;
        }


        public void Monitor_Set(int addr,int value)
        {
            int send_num = 8;
            COMModbus.Instance.Monitor_Set_06(sendbf, addr, value);
            CommonRes.mySerialPort.Write(sendbf, 0, send_num);

            string txt = "";
            txt = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num,"TX:",true);
            BoxStr += txt;
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
                try
                {
                    int anum = Convert.ToInt32(rowView["Command"]);
                    Monitor_Set(addr, anum);
                }
                catch
                {
                    MessageBox.Show("输入命令不能为空！");
                }

            }

        }



        public void Dispose()
        {
            // 若有其他资源需释放，在此处理
        }

    }
}
