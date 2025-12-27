using ocean.database;
using ocean.Interfaces;
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ocean.Communication

{
    public class GeneralDebugPageViewModel: ObservableObject
    {
        public byte[] sendbf = new byte[128];
        //针对数据协议：
        byte[] gbuffer = new byte[4096];
        int gb_index = 0;//缓冲区注入位置
        int get_index = 0;// 缓冲区捕捉位置

        public DataTable dtm { get; set; }


        // 控件命令（供XAML绑定）
        public ICommand DataGridDoubleClickCommand { get; }
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

        // 核心：当前选中的协议实例（直接复用原有单例）
        private IProtocol _currentProtocol;





        public GeneralDebugPageViewModel()
        {
            dtm = new DataTable();
            AddDataTableColumns(dtm);
            ButtonCommand = new RelayCommand(OnButtonClick);
            DataGridDoubleClickCommand = new RelayCommand<DataGrid>(ExecuteDataGridDoubleClick);
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
            DataR temp_Value = new DataR();
            int checkresult = 0;

            string str = "";
            str = SerialDataProcessor.Instance.FormatSerialDataToHexString(gbuffer, buffer_len, "RX:", gb_last, true);
            // 线程安全更新UI
            UiDispatcherHelper.ExecuteOnUiThread(() =>
            {
                BoxStr += str;               
            });


            Array.Copy(gbuffer, gb_last, buffer, 0, buffer_len);

            checkresult=_currentProtocol.MonitorCheck(buffer, buffer.Length);

            if (checkresult == 1)
            {
                if (buffer[1] == 3 && dtm.Rows.Count > 0)
                {
                    temp_Value = _currentProtocol.MonitorSolve(buffer,Readpos-1);
                    // 跨线程更新DataTable（避免UI线程异常）
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        dtm.Rows[temp_Value.SN]["Value"] = temp_Value.VALUE;
                    });
                }
            }
            else
            {
                UiDispatcherHelper.ExecuteOnUiThread(() =>
                {
                    BoxStr += "RX:Wrong";
                });
            }
            
        }

        public int Monitor_Solve(byte[] buffer)
        {

            int Value = 0;

            if (buffer[1] == 3)
            {
                byte[] typeBytes = new byte[buffer[2]];
                Array.Copy(buffer, 3, typeBytes, 0, buffer[2]);
                Array.Reverse(typeBytes);
                Value = BitConverter.ToInt16(typeBytes, 0);
            }
            else if (buffer[1] == 6)
            {

            }

            return Value;
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
                    int value = Convert.ToInt32(rowView["Command"]);
                    int send_num = _currentProtocol.MonitorSet(sendbf, addr, (float)value);
                    CommonRes.mySerialPort.Write(sendbf, 0, send_num);
                    string txt = "";
                    txt = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num, "TX:", true);
                    BoxStr += txt;
                }
                catch
                {
                    MessageBox.Show("输入命令不能为空！");
                }

            }

        }



        // 双击事件核心逻辑（参数为DataGrid，适配你的RelayCommand<T>）
        private void ExecuteDataGridDoubleClick(DataGrid dataGrid)
        {
            if (dataGrid == null) return;

            // 捕获当前鼠标位置的元素（替代原e.OriginalSource）
            var mousePosition = Mouse.GetPosition(dataGrid);
            var hitTestResult = VisualTreeHelper.HitTest(dataGrid, mousePosition);
            if (hitTestResult == null) return;

            // 1. 查找点击的单元格（复用FindVisualParent）
            var cell = FindVisualParent<DataGridCell>(hitTestResult.VisualHit as DependencyObject);
            if (cell == null) return;

            // 2. 判断是否是数值列
            if (cell.Column.Header.ToString() == "数值")
            {
                // 3. 提取数值、地址、ID等信息
                var rowView = cell.DataContext as DataRowView;
                if (rowView != null)
                {
                    // 提取Value列值
                    object value = rowView["Value"];
                    value = value == DBNull.Value ? "空值" : value;
                    MessageBox.Show($"当前数值：{value}", "数值详情");

                    // 提取Addr并调用ReadButtonHander（增加类型安全判断）
                    if (int.TryParse(rowView["Addr"].ToString(), out int addr))
                    {
                        // 执行读取逻辑
                        int send_num = _currentProtocol.MonitorGet(sendbf, addr, 1);
                        CommonRes.mySerialPort.Write(sendbf, 0, send_num);

                        string txt = "";
                        txt = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num, "TX:", true);
                        BoxStr += txt;

                    }

                    // 提取ID并赋值给Readpos
                    if (int.TryParse(rowView["ID"].ToString(), out int readpos))
                    {
                        Readpos = readpos; // 绑定属性，外部可通过ModbusSet.Readpos访问
                    }
                }
            }
        }

        // 辅助方法：向上查找可视化树中的指定类型元素（必须有这个方法！）
        private T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T target)
                {
                    return target;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }





        public void Dispose()
        {
            // 若有其他资源需释放，在此处理
        }

    }
}
