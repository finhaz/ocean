using ocean.Communication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports; // 必须引入：包含StopBits、Parity枚举
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;
using ocean.Mvvm;

namespace ocean.ViewModels
{
    /// <summary>
    /// 串口配置ViewModel（继承自自定义的ObservableObject）
    /// </summary>
    public class SerialConfigViewModel : ObservableObject
    {
        #region 端口名相关
        private ObservableCollection<string> _portNames = new ObservableCollection<string>();
        public ObservableCollection<string> PortNames
        {
            get => _portNames;
            set => SetProperty(ref _portNames, value);
        }

        private string _selectedPortName = string.Empty;
        public string SelectedPortName
        {
            get => _selectedPortName;
            set => SetProperty(ref _selectedPortName, value);
        }
        #endregion

        #region 波特率相关
        private ObservableCollection<int> _baudRates = new ObservableCollection<int> { 4800, 9600, 19200, 38400, 43000, 56000 };
        public ObservableCollection<int> BaudRates
        {
            get => _baudRates;
            set => SetProperty(ref _baudRates, value);
        }

        private int _selectedBaudRate = 9600;
        public int SelectedBaudRate
        {
            get => _selectedBaudRate;
            set => SetProperty(ref _selectedBaudRate, value);
        }
        #endregion

        #region 校验位相关
        private ObservableCollection<string> _parityNames = new ObservableCollection<string> { "无", "奇校验", "偶校验" };
        public ObservableCollection<string> ParityNames
        {
            get => _parityNames;
            set => SetProperty(ref _parityNames, value);
        }

        private string _selectedParityName = "无";
        public string SelectedParityName
        {
            get => _selectedParityName;
            set
            {
                if (SetProperty(ref _selectedParityName, value))
                {
                    // 同步更新Parity枚举：使用System.IO.Ports.Parity
                    SelectedParity = value switch
                    {
                        "无" => Parity.None,
                        "奇校验" => Parity.Odd,
                        "偶校验" => Parity.Even,
                        _ => Parity.None
                    };
                }
            }
        }

        private Parity _selectedParity = Parity.None;
        public Parity SelectedParity
        {
            get => _selectedParity;
            set => SetProperty(ref _selectedParity, value);
        }
        #endregion

        #region 数据位相关
        private ObservableCollection<int> _dataBits = new ObservableCollection<int> { 6, 7, 8 };
        public ObservableCollection<int> DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        private int _selectedDataBit = 8;
        public int SelectedDataBit
        {
            get => _selectedDataBit;
            set => SetProperty(ref _selectedDataBit, value);
        }
        #endregion

        #region 停止位相关（重点：重命名集合变量，避免与枚举重名）
        /// <summary>
        /// 停止位可选值集合（命名为StopBitValues，避免与StopBits枚举冲突）
        /// </summary>
        private ObservableCollection<int> _stopBitValues = new ObservableCollection<int> { 1, 2 };
        public ObservableCollection<int> StopBitValues
        {
            get => _stopBitValues;
            set => SetProperty(ref _stopBitValues, value);
        }

        /// <summary>
        /// 选中的停止位数值（1/2）
        /// </summary>
        private int _selectedStopBit = 1;
        public int SelectedStopBit
        {
            get => _selectedStopBit;
            set
            {
                if (SetProperty(ref _selectedStopBit, value))
                {
                    // 同步更新StopBits枚举：使用System.IO.Ports.StopBits
                    SelectedStopBitEnum = value == 1 ? StopBits.One : StopBits.Two;
                }
            }
        }

        /// <summary>
        /// 选中的停止位枚举（SerialPort的StopBits类型）
        /// </summary>
        private StopBits _selectedStopBitEnum = StopBits.One;
        public StopBits SelectedStopBitEnum
        {
            get => _selectedStopBitEnum;
            set => SetProperty(ref _selectedStopBitEnum, value);
        }
        #endregion

        #region 控件状态
        private bool _isConfigEnabled = true;
        public bool IsConfigEnabled
        {
            get => _isConfigEnabled;
            set => SetProperty(ref _isConfigEnabled, value);
        }
        #endregion

        #region 辅助方法
        public void InitPortNames()
        {
            PortNames.Clear();
            try
            {
                foreach (var port in SerialPort.GetPortNames())
                {
                    PortNames.Add(port);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取串口名失败：{ex.Message}");
            }
        }
        #endregion


        #region 新增：复选框相关属性
        /// <summary>
        /// 16进制发送（对应ck16Send的IsChecked）
        /// </summary>
        private bool _isHexSend;
        public bool IsHexSend
        {
            get => _isHexSend;
            set => SetProperty(ref _isHexSend, value);
        }

        /// <summary>
        /// 自动发送（对应ckAutoSend的IsChecked）
        /// </summary>
        private bool _isAutoSend;
        public bool IsAutoSend
        {
            get => _isAutoSend;
            set
            {
                if (SetProperty(ref _isAutoSend, value))
                {
                    // 自动发送状态变化时的额外逻辑（如启动/停止自动发送定时器）
                    OnAutoSendStateChanged(value);
                }
            }
        }
        #endregion

        #region 新增：复选框点击逻辑（替代Click事件）
        /// <summary>
        /// 16进制发送状态变化的处理逻辑
        /// </summary>


        /// <summary>
        /// 自动发送状态变化的处理逻辑
        /// </summary>
        /// <param name="isAutoSend">是否开启自动发送</param>
        private void OnAutoSendStateChanged(bool isAutoSend)
        {
            // 原ckAutoSend_Click中的逻辑可移至此处
            // 示例：启动/停止自动发送定时器
            if (isAutoSend)
            {
                // 启动自动发送定时器（需在ViewModel中定义定时器）
                // _autoSendTimer?.Start();
                Console.WriteLine("自动发送已开启");
            }
            else
            {
                // 停止自动发送定时器
                // _autoSendTimer?.Stop();
                Console.WriteLine("自动发送已关闭");
            }
        }
        #endregion

        #region 新增：16进制显示相关属性
        /// <summary>
        /// 16进制显示（对应ck16View的IsChecked）
        /// </summary>
        private bool _isHexView;
        public bool IsHexView
        {
            get => _isHexView;
            set => SetProperty(ref _isHexView, value);
        }
        #endregion

        #region 命令属性（统一处理点击逻辑）
        /// <summary>
        /// 16进制发送复选框点击命令
        /// </summary>
        public ICommand HexSendClickCommand { get; }

        /// <summary>
        /// 自动发送复选框点击命令
        /// </summary>
        public ICommand AutoSendClickCommand { get; }

        /// <summary>
        /// 16进制显示复选框点击命令（新增）
        /// </summary>
        public ICommand HexViewClickCommand { get; }
        #endregion

        #region 构造函数
        public SerialConfigViewModel()
        {
            InitPortNames();
            InitComStateStyle();

            // 初始化命令
            //HexSendClickCommand = new RelayCommand(OnHexSendClicked);
            //AutoSendClickCommand = new RelayCommand(OnAutoSendClicked);
            //HexViewClickCommand = new RelayCommand(OnHexViewClicked); // 新增命令初始化
        }
        #endregion

        #region 事件处理方法
        /// <summary>
        /// 16进制发送点击逻辑
        /// </summary>
        private void OnHexSendClicked()
        {
            // 原ck16Send_Click的逻辑
            Console.WriteLine($"16进制发送状态切换：{IsHexSend}");
        }

        /// <summary>
        /// 自动发送点击逻辑
        /// </summary>
        private void OnAutoSendClicked()
        {
            // 原ckAutoSend_Click的逻辑
            Console.WriteLine($"自动发送状态切换：{IsAutoSend}");
        }

        /// <summary>
        /// 16进制显示点击逻辑（新增：处理原ck16View_Click）
        /// </summary>
        private void OnHexViewClicked()
        {
            // 原ck16View_Click的业务逻辑移至此处
            // 示例：切换接收数据的显示格式（字符串/16进制）
            Console.WriteLine($"16进制显示状态切换：{IsHexView}");
            // 可在此处添加显示格式切换的核心逻辑，比如通知串口接收模块切换解析方式
        }
        #endregion


        #region //16进制处理
        public string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();
        }

        public byte[] HexStringToByteArray(string s)
        {
            //s=s.ToUpper();
            s = s.Replace(" ", "");
            if (s.Length % 2 != 0)
            {
                s = s.Substring(0, s.Length - 1) + "0" + s.Substring(s.Length - 1);
            }
            byte[] buffer = new byte[s.Length / 2];


            try
            {
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
                return buffer;
            }
            catch
            {
                string errorString = "E4";
                byte[] errorData = new byte[errorString.Length / 2];
                errorData[0] = Convert.ToByte(errorString, 16);
                return errorData;
            }
        }

        public string StringToHexString(string s)
        {
            //s = s.ToUpper();
            s = s.Replace(" ", "");

            string buffer = "";
            char[] myChar;
            myChar = s.ToCharArray();
            for (int i = 0; i < s.Length; i++)
            {
                //buffer = buffer + Convert.ToInt32(myChar[i]);
                buffer = buffer + Convert.ToString(myChar[i], 16);
                buffer = buffer.ToUpper();
            }
            return buffer;
        }

        #endregion



        #region 新增：仅添加两个核心属性（无其他多余代码）
        /// <summary>
        /// 串口状态椭圆的样式（对应comState.Style）
        /// </summary>
        private Style _comStateStyle;
        public Style ComStateStyle
        {
            get => _comStateStyle;
            set => SetProperty(ref _comStateStyle, value);
        }


        /// <summary>
        /// 打开串口按钮的内容（对应btOpenCom.Content）
        /// </summary>
        private object _openComButtonContent = "打开串口"; // 默认值
        public object OpenComButtonContent
        {
            get => _openComButtonContent;
            set => SetProperty(ref _openComButtonContent, value);
        }


        /// <summary>
        /// 初始化串口状态样式（从全局资源获取）
        /// </summary>
        private void InitComStateStyle()
        {
            // 安全获取：先判断资源是否存在，避免空引用
            if (Application.Current.Resources.Contains("EllipseStyleRed"))
            {
                _comStateStyle = Application.Current.Resources["EllipseStyleRed"] as Style;
            }
            else
            {
                // 兜底：若资源不存在，新建默认样式（防止空值）
                _comStateStyle = new Style(typeof(Ellipse));
                _comStateStyle.Setters.Add(new Setter(Shape.FillProperty, System.Windows.Media.Brushes.Red));
            }
        }

        #endregion


        #region 新增：文本控件绑定属性（仅字符串/Visibility类型，初始值按要求设置）
        /// <summary>
        /// 发送框文本（tbSend.Text），初始值"0"
        /// </summary>
        private string _tbSendText = "0";
        public string TbSendText
        {
            get => _tbSendText;
            set => SetProperty(ref _tbSendText, value);
        }

        /// <summary>
        /// 间隔时间输入框文本（tbIntervalTime.Text）
        /// </summary>
        private string _tbIntervalTimeText = string.Empty;
        public string TbIntervalTimeText
        {
            get => _tbIntervalTimeText;
            set => SetProperty(ref _tbIntervalTimeText, value);
        }

        /// <summary>
        /// 间隔时间标签可见性（tbkIntervalTime.Visibility）
        /// </summary>
        private Visibility _tbkIntervalTimeVisibility = Visibility.Visible;
        public Visibility TbkIntervalTimeVisibility
        {
            get => _tbkIntervalTimeVisibility;
            set => SetProperty(ref _tbkIntervalTimeVisibility, value);
        }

        /// <summary>
        /// 间隔时间输入框可见性（tbIntervalTime.Visibility）
        /// </summary>
        private Visibility _tbIntervalTimeVisibility = Visibility.Visible;
        public Visibility TbIntervalTimeVisibility
        {
            get => _tbIntervalTimeVisibility;
            set => SetProperty(ref _tbIntervalTimeVisibility, value);
        }

        /// <summary>
        /// 串口状态文本（tbComState.Text），初始值"0"
        /// </summary>
        private string _tbComStateText = "0";
        public string TbComStateText
        {
            get => _tbComStateText;
            set => SetProperty(ref _tbComStateText, value);
        }

        /// <summary>
        /// 接收统计文本（txtRecive.Text），初始值"0"
        /// </summary>
        private string _receiveCount = "0";
        public string ReceiveCount 
        { 
            get => _receiveCount; 
            set => SetProperty(ref _receiveCount, value); 
        }

        /// <summary>
        /// 发送统计文本（txtSend.Text），初始值"0"
        /// </summary>
        private string _sendCount = "0";
        public string SendCount
        {
            get => _sendCount;
            set => SetProperty(ref _sendCount, value);
        }
        #endregion

        #region 新增：接收文本框（tbReceive）绑定属性
        /// <summary>
        /// 串口接收内容文本（tbReceive.Text）
        /// </summary>
        private string _tbReceiveText = string.Empty; // 初始值为空，可按需修改
        public string TbReceiveText
        {
            get => _tbReceiveText;
            set => SetProperty(ref _tbReceiveText, value);
        }
        #endregion


        #region 新增：16进制预览相关属性（控制UI显示/内容）
        /// <summary>
        /// 是否显示16进制预览框（替代动态创建/删除控件）
        /// </summary>
        private Visibility _tb16ViewVisibility = Visibility.Collapsed;
        public Visibility Tb16ViewVisibility
        {
            get => _tb16ViewVisibility;
            set => SetProperty(ref _tb16ViewVisibility, value);
        }

        /// <summary>
        /// 16进制预览框的文本内容（替代直接赋值控件Text）
        /// </summary>
        private string _tb16ViewText = string.Empty;
        public string Tb16ViewText
        {
            get => _tb16ViewText;
            set => SetProperty(ref _tb16ViewText, value);
        }

        /// <summary>
        /// 是否勾选AdvantechCmd（绑定到ckAdvantechCmd的IsChecked）
        /// </summary>
        private bool _isAdvantechCmdChecked = false;
        public bool IsAdvantechCmdChecked
        {
            get => _isAdvantechCmdChecked;
            set => SetProperty(ref _isAdvantechCmdChecked, value);
        }

        #endregion

        #region 核心方法：更新16进制预览文本（原转换逻辑移至此处）
        /// <summary>
        /// 切换16进制预览的显示状态，并更新内容
        /// </summary>
        /// <param name="isHex">是否启用16进制预览</param>
        public void Toggle16View(bool isHex)
        {
            if (isHex)
            {
                // 显示16进制预览框
                Tb16ViewVisibility = Visibility.Visible;
                // 更新预览文本
                Update16ViewText();
            }
            else
            {
                // 隐藏16进制预览框
                Tb16ViewVisibility = Visibility.Collapsed;
                Tb16ViewText = string.Empty;
            }
        }

        /// <summary>
        /// 计算16进制预览文本（复用原转换逻辑）
        /// </summary>
        private void Update16ViewText()
        {
            string hexString = StringToHexString(TbSendText);
            string hexStringView = string.Empty;

            for (int i = 0; i < hexString.Length; i += 2)
            {
                hexStringView += hexString.Substring(i, 2) + " ";
            }

            // 若勾选AdvantechCmd，追加0D
            if (IsAdvantechCmdChecked)
            {
                hexStringView += "0D";
            }

            Tb16ViewText = hexStringView;
        }
        #endregion



    }
}