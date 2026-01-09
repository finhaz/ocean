using ocean.database;
using ocean.Interfaces;
using ocean.Mvvm;
using ocean.ViewModels;
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

        // 核心：替换原DataTable为ObservableCollection
        private ObservableCollection<ModbusDataItem> _modbusDataList;
        public ObservableCollection<ModbusDataItem> ModbusDataList
        {
            get => _modbusDataList;
            set => SetProperty(ref _modbusDataList, value);
        }


        // 控件命令（供XAML绑定）
        public ICommand DataGridDoubleClickCommand { get; }
        public ICommand ButtonCommand { get; }


        // 从**字段**改为**私有字段+公共属性**（带通知）
        private string _sadd = "1"; // 初始值可根据你的业务调整
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

        private string _dSelectedTransferType = "无符号整数";
        public string DSelectedTransferType
        {
            get => _dSelectedTransferType;
            set => SetProperty(ref _dSelectedTransferType, value);
        }

        private string _dSelectedDisplayType = "浮点数";
        public string DSelectedDisplayType
        {
            get => _dSelectedDisplayType;
            set => SetProperty(ref _dSelectedDisplayType, value);
        }

        

        private string _proSelectedOption = "Modbus RTU协议";
        public string ProSelectedOption
        {
            get => _proSelectedOption;
            set => SetProperty(ref _proSelectedOption, value);
        }

        // 控制高级列显示/隐藏的布尔属性
        private bool _isAdvancedColumnsVisible=false;
        public bool IsAdvancedColumnsVisible
        {
            get => _isAdvancedColumnsVisible;
            set => SetProperty(ref _isAdvancedColumnsVisible, value);
        }



        private string _intoSelectedOption = "Modbus RTU协议";
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
        { "Modbus RTU协议", "FE协议","Modbus TCP协议" };

        public ObservableCollection<string> TransferTypeOptions { get; set; } = new ObservableCollection<string>
        { "有符号整数", "无符号整数", "浮点数", "字节流","位数据" };

        public ObservableCollection<string> DisplayTypeOptions { get; set; } = new ObservableCollection<string>
        { "浮点数", "位数据", "十进制整数", "十六进制整数","字节流","字符串" };


        // 协议信息集合
        public ObservableCollection<ProtocolInfo> ProtocolList { get; set; } = new ObservableCollection<ProtocolInfo>
        {
            new ProtocolInfo
            {
                Name = "Modbus RTU协议",
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


        // 新增：全局通讯实例（ViewModel内唯一，与SerialConfigView共用同一个实例）
        // 利用CommunicationManager单例特性，保证和SerialConfigView的串口实例完全一致
        private ICommunication _comm;
        // 切换列显示/隐藏的命令
        public ICommand ToggleAdvancedColumnsCommand { get; }

        public GeneralDebugPageViewModel()
        {
            //dtm = new DataTable();
            //AddDataTableColumns(dtm);
            // 初始化数据（替代原AddDataTableColumns和添加行的逻辑）
            ModbusDataList = new ObservableCollection<ModbusDataItem>();
            AddModbusDataItem();
            ButtonCommand = new RelayCommand(OnButtonClick);
            DataGridDoubleClickCommand = new RelayCommand<DataGrid>(ExecuteDataGridDoubleClick);

            // 初始化命令（使用Microsoft.Xaml.Behaviors的DelegateCommand，也可自定义ICommand）
            ToggleAdvancedColumnsCommand = new DelegateCommand(() =>
            {
                // 切换布尔值
                IsAdvancedColumnsVisible = !IsAdvancedColumnsVisible;
            });

        }




        // 协议切换（选择框事件调用）
        public void InitProtocol(string protocolNum)
        {
            // 若需要运行时切换通讯方式，可定时/按需刷新实例（示例）
            _comm = CommunicationManager.Instance.GetCurrentCommunication();
            // 直接赋值原有单例，无需新建对象
            _currentProtocol = protocolNum switch
            {
                "FE协议" => COMFE.Instance,       // COMFE实现了IProtocol
                "Modbus RTU协议" => COMModbus.Instance, // COMModbus实现了IProtocol
                "Modbus TCP协议"=>TCPModbus.Instance,//TCPModbus实现了IProtocol
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
            dt.Columns.Add("Coefficient", typeof(int));
            dt.Columns.Add("Offset",typeof(int));
            dt.Columns.Add("DecimalPlaces", typeof(int));
            dt.Columns.Add("TransferType", typeof(string));
            dt.Columns.Add("DisplayType", typeof(string));
            dt.Columns.Add("ByteOrder", typeof(int));
            dt.Columns.Add("WordOrder", typeof(int));
            dt.Columns.Add("IsDrawCurve", typeof(bool));
            dt.Columns.Add("IntervalTime", typeof(int));
        }

        /// <summary>
        /// 添加新行（替代原DataTable的AddRow逻辑）
        /// </summary>
        public void AddModbusDataItem()
        {
            ModbusDataList.Add(new ModbusDataItem
            {
                ID = ModbusDataList.Count + 1,
                Name = "默认名称",
                Value = 0.0, // 默认浮点数
                Command = 0.0,
                IsButtonClicked = false,
                Unit = "",
                Rangle = "",
                SelectedOption = DSelectedOption,
                Addr = 0,
                Number = 1,
                NOffSet = 0,
                NBit = 16,
                Coefficient = 1,
                Offset = 0,
                DecimalPlaces = 2,
                TransferType = DSelectedTransferType,
                DisplayType = DSelectedDisplayType, // 默认显示类型
                ByteOrder = 0,
                WordOrder = 0,
                IsDrawCurve = false,
                IntervalTime = 1000
            });
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
            checkresult = _currentProtocol.MonitorCheck(buffer, buffer_len);

            if (checkresult == 1)
            {
                //if (dtm.Rows.Count > 0)
                if(ModbusDataList.Count > 0) 
                {
                    temp_Value = _currentProtocol.MonitorSolve(buffer, Readpos - 1);
                    if (temp_Value.RWX==1)
                    {
                        // 跨线程更新DataTable（避免UI线程异常）
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            //dtm.Rows[temp_Value.SN]["Value"] = temp_Value.VALUE;
                            var targetItem = ModbusDataList.FirstOrDefault(item => item.ID == (temp_Value.SN + 1));

                            if (targetItem != null)
                            {
                                // 2. 核心赋值：直接给实体类的Value属性赋值
                                // 同时根据当前行的DisplayType自动转换类型（保持和选择的呈现类型一致）
                                switch (targetItem.DisplayType)
                                {
                                    case "十进制整数":
                                        targetItem.RValue = Convert.ToInt32(temp_Value.VALUE);
                                        // 转为int类型赋值
                                        targetItem.Value = Convert.ToInt32(temp_Value.VALUE) * targetItem.Coefficient;
                                        break;
                                    case "浮点数":
                                        targetItem.RValue = Convert.ToDouble(temp_Value.VALUE);
                                        // 转为double类型赋值（浮点数）
                                        targetItem.Value = Convert.ToDouble(temp_Value.VALUE) * targetItem.Coefficient;
                                        break;
                                    default:
                                        targetItem.RValue = Convert.ToInt32(temp_Value.VALUE);
                                        // 转为int类型赋值
                                        targetItem.Value = Convert.ToInt32(temp_Value.VALUE) * targetItem.Coefficient;
                                        break;
                                }
                            }
                        });
                    }
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


        /*
        private void OnButtonClick(object parameter)
        {

            if (parameter is DataRowView rowView)
            {
                int addr = Convert.ToInt32(rowView["Addr"]);

                // 直接使用全局_comm，无需重复获取（与SerialConfigView实例一致）
                if (!_comm.IsConnected)
                {
                    MessageBox.Show("请打开串口！");
                    return;
                }
                try
                {
                    int value = Convert.ToInt32(rowView["Command"]);
                    int send_num = _currentProtocol.MonitorSet(sendbf, addr, (float)value);

                    // 共用同一个通讯实例发送
                    _comm.Send(sendbf, 0, send_num);

                    string txt = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num, "TX:", true);
                    BoxStr += txt;
                }
                catch
                {
                    MessageBox.Show("输入命令不能为空！");
                }
            }
        }
        */

        private void OnButtonClick(object parameter)
        {
            // 核心修改：参数类型从DataRowView改为ModbusDataItem
            if (parameter is ModbusDataItem item)
            {
                int addr = item.Addr; // 直接取实体类的属性，无需Convert
                int numr = item.Number;

                // 原有串口判断逻辑保留
                if (!_comm.IsConnected)
                {
                    MessageBox.Show("请打开串口！");
                    return;
                }

                try
                {
                    // 核心修改：从实体类的Command属性取值（原rowView["Command"]）
                    // 注意：Command在实体类中是double类型，转int保持原有逻辑
                    int value = Convert.ToInt32(item.Command);
                    int send_num = _currentProtocol.MonitorSet(sendbf, addr, (float)value,item.SelectedOption );

                    // 原有发送逻辑保留
                    _comm.Send(sendbf, 0, send_num);

                    string txt = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num, "TX:", true);
                    BoxStr += txt;
                }
                catch (Exception ex)
                {
                    // 优化异常提示：区分空值和其他异常
                    if (ex is FormatException || ex is InvalidCastException)
                    {
                        MessageBox.Show("输入命令必须是有效的数字！");
                    }
                    else if (ex is NullReferenceException)
                    {
                        MessageBox.Show("输入命令不能为空！");
                    }
                    else
                    {
                        MessageBox.Show($"发送失败：{ex.Message}");
                    }
                }
            }
            else
            {
                // 兼容旧数据或参数异常的情况
                MessageBox.Show("无法识别当前行数据！");
            }
        }


        /*
        // 双击事件核心逻辑
        private void ExecuteDataGridDoubleClick(DataGrid dataGrid)
        {
            if (dataGrid == null) return;

            var mousePosition = Mouse.GetPosition(dataGrid);
            var hitTestResult = VisualTreeHelper.HitTest(dataGrid, mousePosition);
            if (hitTestResult == null) return;

            var cell = FindVisualParent<DataGridCell>(hitTestResult.VisualHit as DependencyObject);
            if (cell == null) return;

            if (cell.Column.Header.ToString() == "数值")
            {
                var rowView = cell.DataContext as DataRowView;
                if (rowView != null)
                {
                    object value = rowView["Value"];
                    value = value == DBNull.Value ? "空值" : value;
                    MessageBox.Show($"当前数值：{value}", "数值详情");

                    if (int.TryParse(rowView["Addr"].ToString(), out int addr))
                    {
                        int send_num = _currentProtocol.MonitorGet(sendbf, addr, 1);

                        // 直接使用全局_comm发送（与SerialConfigView实例一致）
                        _comm.Send(sendbf, 0, send_num);

                        string txt = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num, "TX:", true);
                        BoxStr += txt;
                    }

                    if (int.TryParse(rowView["ID"].ToString(), out int readpos))
                    {
                        Readpos = readpos;
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
        */

        // 双击事件核心逻辑（适配ModbusDataItem实体类）
        private void ExecuteDataGridDoubleClick(DataGrid dataGrid)
        {
            if (dataGrid == null) return;

            var mousePosition = Mouse.GetPosition(dataGrid);
            var hitTestResult = VisualTreeHelper.HitTest(dataGrid, mousePosition);
            if (hitTestResult == null) return;

            var cell = FindVisualParent<DataGridCell>(hitTestResult.VisualHit as DependencyObject);
            if (cell == null) return;

            // 仅处理“数值”列的双击（原有逻辑保留）
            if (cell.Column.Header.ToString() == "数值")
            {
                // 核心修改：DataContext从DataRowView改为ModbusDataItem
                var item = cell.DataContext as ModbusDataItem;
                if (item != null)
                {
                    // 核心修改：直接取实体类的Value属性，替换原rowView["Value"]
                    object value = item.Value ?? "空值"; // 实体类用null，替代原DBNull.Value
                    MessageBox.Show($"当前数值：{value}", "数值详情");

                    // 核心修改：直接取实体类的Addr属性（本身就是int，无需TryParse）
                    int addr = item.Addr;
                    int numr=item.Number;
                    int send_num = _currentProtocol.MonitorGet(sendbf, addr, numr, item.SelectedOption);

                    // 原有发送逻辑保留
                    _comm.Send(sendbf, 0, send_num);
                    string txt = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num, "TX:", true);
                    BoxStr += txt;

                    // 核心修改：直接取实体类的ID属性（本身就是int，无需TryParse）
                    Readpos = item.ID;
                }
            }
        }

        // 辅助方法：向上查找可视化树中的指定类型元素（完全保留，无需修改）
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

        public void Page_LoadedD(object sender, RoutedEventArgs e)
        {
            /*
            // 仅对串口类型绑定数据接收事件（预留以太网扩展）
            if (_comm is SerialCommunication serialComm)
            {
                // 绑定串口数据接收事件（替代 CommonRes.CurrentDataHandler = HandleSerialData）
                serialComm.DataReceived += HandleSerialDataWrapper;
            }        
            */
            _comm.DataReceived += HandleSerialDataWrapper;
        }

        public void Page_UnLoadedD(object sender, RoutedEventArgs e)
        {
            /*
            // 仅对串口类型解绑数据接收事件
            if (_comm is SerialCommunication serialComm)
            {
                // 解绑事件（替代 CommonRes.CurrentDataHandler 清空）
                serialComm.DataReceived -= HandleSerialDataWrapper;
            }
            */

            _comm.DataReceived -= HandleSerialDataWrapper;
            // 释放ViewModel资源（原有逻辑保留）
            Dispose();
        }

        // 事件包装器（适配 DataReceivedEventArgs 到原有 HandleSerialData 参数）
        // 核心：无需修改原有 HandleSerialData 逻辑，仅做参数映射
        private void HandleSerialDataWrapper(object sender, DataReceivedEventArgs e)
        {
            // 直接调用原有 HandleSerialData 方法（参数与原 CommonRes 委托一致）
            HandleSerialData(e.Buffer, e.LastIndex, e.BufferLength);
        }



        public void Dispose()
        {
            // 若有其他资源需释放，在此处理
        }

    }
}
