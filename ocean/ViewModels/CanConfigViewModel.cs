using ocean.Communication;
using ocean.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ocean.ViewModels
{
    public class CanConfigViewModel : INotifyPropertyChanged, IDisposable
    {
        #region ==== 核心字段(原有+新增串口配置字段，严格适配你的SerialConfig/SerialCommunication) ====
        private readonly ICommunication _comm;
        private readonly StringBuilder _recvBuffer = new();
        private const int AtCmdTimeout = 1200;
        private readonly Encoding _encode = Encoding.ASCII;
        private bool _isConfigMode = false;
        public bool _isWaitingATOk = false;

        //==== 新增：串口配置字段 (和弹窗绑定，完全适配你的SerialConfig) ====
        private string _selectedPortName = "COM3";
        private int _selectedPortBaud = 115200;
        private int _selectedPortDataBits = 8;
        private string _selectedPortParity = "无";
        private int _selectedPortStopBit = 1;
        private List<string> _portNameList = new();
        #endregion

        #region ==== 界面双向绑定属性 (原有全部保留) ====
        private int _selectedWorkMode = 0;
        public int SelectedWorkMode
        {
            get => _selectedWorkMode;
            set { _selectedWorkMode = value; OnPropertyChanged(); }
        }

        private int _selectedCanBaud = 3;
        public int SelectedCanBaud
        {
            get => _selectedCanBaud;
            set { _selectedCanBaud = value; OnPropertyChanged(); }
        }

        private int _selectedSerialBaud = 4;
        public int SelectedSerialBaud
        {
            get => _selectedSerialBaud;
            set { _selectedSerialBaud = value; OnPropertyChanged(); }
        }

        private int _selectedDataBit = 0;
        public int SelectedDataBit
        {
            get => _selectedDataBit;
            set { _selectedDataBit = value; OnPropertyChanged(); }
        }

        private int _selectedStopBit = 1;
        public int SelectedStopBit
        {
            get => _selectedStopBit;
            set { _selectedStopBit = value; OnPropertyChanged(); }
        }

        private int _selectedParity = 0;
        public int SelectedParity
        {
            get => _selectedParity;
            set { _selectedParity = value; OnPropertyChanged(); }
        }

        private bool _isTransEnable = true;
        public bool IsTransEnable
        {
            get => _isTransEnable;
            set { _isTransEnable = value; OnPropertyChanged(); }
        }

        private int _selectedFrameFormat = 0;
        public int SelectedFrameFormat
        {
            get => _selectedFrameFormat;
            set { _selectedFrameFormat = value; OnPropertyChanged(); }
        }

        private string _stdId = "136";
        public string StdId
        {
            get => _stdId;
            set { _stdId = value; OnPropertyChanged(); }
        }

        private string _extId = "0";
        public string ExtId
        {
            get => _extId;
            set { _extId = value; OnPropertyChanged(); }
        }

        private bool _isFilterEnable = false;
        public bool IsFilterEnable
        {
            get => _isFilterEnable;
            set { _isFilterEnable = value; OnPropertyChanged(); }
        }

        private string _filterId = "0";
        public string FilterId
        {
            get => _filterId;
            set { _filterId = value; OnPropertyChanged(); }
        }

        private string _maskId = "0";
        public string MaskId
        {
            get => _maskId;
            set { _maskId = value; OnPropertyChanged(); }
        }

        private string _atModuleStatus = "就绪，请先配置并打开串口";
        public string AtModuleStatus
        {
            get => _atModuleStatus;
            set { _atModuleStatus = value; OnPropertyChanged(); }
        }
        #endregion

        #region ==== 新增：串口配置弹窗绑定属性 (核心) ====
        public string SelectedPortName
        {
            get => _selectedPortName;
            set { _selectedPortName = value; OnPropertyChanged(); }
        }
        public int SelectedPortBaud
        {
            get => _selectedPortBaud;
            set { _selectedPortBaud = value; OnPropertyChanged(); }
        }
        public int SelectedPortDataBits
        {
            get => _selectedPortDataBits;
            set { _selectedPortDataBits = value; OnPropertyChanged(); }
        }
        public string SelectedPortParity
        {
            get => _selectedPortParity;
            set { _selectedPortParity = value; OnPropertyChanged(); }
        }
        public int SelectedPortStopBit
        {
            get => _selectedPortStopBit;
            set { _selectedPortStopBit = value; OnPropertyChanged(); }
        }
        public List<string> PortNameList
        {
            get => _portNameList;
            set { _portNameList = value; OnPropertyChanged(); }
        }
        public List<int> PortBaudList { get; } = new() { 4800, 9600, 38400, 57600, 115200, 230400, 460800 };
        public List<int> PortDataBitsList { get; } = new() { 7, 8, 9 };
        public List<string> PortParityList { get; } = new() { "无", "奇校验", "偶校验" };
        public List<int> PortStopBitList { get; } = new() { 0, 1, 2 };
        #endregion

        #region ==== 下拉数据源 (原有保留) ====
        public List<CanBaudItem> CanBaudList { get; } = new()
        {
            new CanBaudItem{Value=10000,Text="10Kbps"},
            new CanBaudItem{Value=20000,Text="20Kbps"},
            new CanBaudItem{Value=50000,Text="50Kbps"},
            new CanBaudItem{Value=100000,Text="100Kbps"},
            new CanBaudItem{Value=250000,Text="250Kbps"},
            new CanBaudItem{Value=500000,Text="500Kbps"},
            new CanBaudItem{Value=1000000,Text="1Mbps"}
        };

        public List<int> SerialBaudList { get; } = new() { 4800, 9600, 38400, 57600, 115200, 230400, 460800 };
        #endregion

        #region ==== 构造函数 (原有+初始化串口列表) ====
        public CanConfigViewModel(ICommunication communication)
        {
            _comm = communication;
            if (_comm is SerialCommunication serialComm)
            {
                serialComm.Encoding = _encode;
            }
            _comm.DataReceived += HandleSerialDataWrapper;
            RefreshPortNameList();//初始化加载本机所有串口号
        }
        #endregion

        #region ==== 新增：串口核心操作方法 (弹窗调用) ====
        /// <summary>
        /// 刷新本机所有可用串口号
        /// </summary>
        public void RefreshPortNameList()
        {
            PortNameList = new List<string>(SerialPort.GetPortNames());
            AtModuleStatus = PortNameList.Count > 0 ? "已加载串口号，请选择配置" : "未检测到可用串口";
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        public string OpenSerialPort()
        {
            try
            {
                if (_comm.IsConnected)
                {
                    return "串口已处于打开状态";
                }
                //实例化你的SerialConfig，赋值配置参数，严格适配你的类
                SerialConfig config = new SerialConfig()
                {
                    SelectedPortName = SelectedPortName,
                    SelectedBaudRate = SelectedPortBaud,
                    SelectedDataBits = SelectedPortDataBits,
                    SelectedParityName = SelectedPortParity,
                    SelectedStopBit = SelectedPortStopBit
                };
                _comm.Open(config);
                AtModuleStatus = $"✅ 串口打开成功：{SelectedPortName} {SelectedPortBaud}bps";
                return "打开成功";
            }
            catch (Exception ex)
            {
                string errMsg = $"串口打开失败：{ex.Message}";
                AtModuleStatus = $"❌ {errMsg}";
                return errMsg;
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public string CloseSerialPort()
        {
            try
            {
                if (!_comm.IsConnected)
                {
                    return "串口已处于关闭状态";
                }
                _comm.Close();
                AtModuleStatus = "✅ 串口已关闭，等待重新配置";
                return "关闭成功";
            }
            catch (Exception ex)
            {
                string errMsg = $"串口关闭失败：{ex.Message}";
                AtModuleStatus = $"❌ {errMsg}";
                return errMsg;
            }
        }
        #endregion

        #region ==== 对外核心方法-读取/保存/恢复 (原有保留+串口校验强化) ====
        public async Task ReadCanConfigAsync()
        {
            if (!CheckSerialConn()) return;

            AtModuleStatus = "正在读取模块配置...";
            try
            {
                _isWaitingATOk = true;
                bool enterOk = await SendAtCmdAsync("AT+CG\r\n");
                if (!enterOk)
                {
                    AtModuleStatus = "❌ 读取失败：进入AT配置模式失败";
                    _isWaitingATOk = false;
                    return;
                }

                await SendAtCmdAsync("AT+CAN_MODE=?\r\n");
                await SendAtCmdAsync("AT+CAN_BAUD=?\r\n");
                await SendAtCmdAsync("AT+USART_PARAM=?\r\n");
                await SendAtCmdAsync("AT+CAN_FRAMEFORMAT=?\r\n");
                await SendAtCmdAsync("AT+CAN_FILTER0=?\r\n");

                await SendAtCmdAsync("AT+ET\r\n");
                _isConfigMode = false;
                _isWaitingATOk = false;
                AtModuleStatus = "✅ 配置读取完成";
            }
            catch (Exception ex)
            {
                AtModuleStatus = $"❌ 读取异常：{ex.Message}";
                _isWaitingATOk = false;
            }
        }

        public async Task SaveCanConfigAsync()
        {
            if (!CheckSerialConn()) return;
            if (!CheckParamsLegal()) return;

            AtModuleStatus = "正在写入配置到模块...";
            try
            {
                _isWaitingATOk = true;
                bool enterOk = await SendAtCmdAsync("AT+CG\r\n");
                if (!enterOk)
                {
                    AtModuleStatus = "❌ 保存失败：进入AT配置模式失败";
                    _isWaitingATOk = false;
                    return;
                }

                await SendAtCmdAsync($"AT+CAN_MODE={SelectedWorkMode}\r\n");
                await SendAtCmdAsync($"AT+CAN_BAUD={CanBaudList[SelectedCanBaud].Value}\r\n");

                int serialBaud = SerialBaudList[SelectedSerialBaud];
                int dataBit = SelectedDataBit == 0 ? 8 : 9;
                await SendAtCmdAsync($"AT+USART_PARAM={serialBaud},{dataBit},{SelectedStopBit},{SelectedParity}\r\n");

                int transEn = IsTransEnable ? 1 : 0;
                await SendAtCmdAsync($"AT+CAN_FRAMEFORMAT={transEn},{SelectedFrameFormat},{StdId},{ExtId}\r\n");

                int filterEn = IsFilterEnable ? 1 : 0;
                await SendAtCmdAsync($"AT+CAN_FILTER0={filterEn},0,{FilterId},{MaskId}\r\n");

                bool saveOk = await SendAtCmdAsync("AT+SAVE\r\n");
                await SendAtCmdAsync("AT+ET\r\n");
                _isConfigMode = false;
                _isWaitingATOk = false;

                AtModuleStatus = saveOk ? "✅ 配置保存成功，重启模块生效" : "⚠️ 配置发送成功，保存到FLASH失败";
            }
            catch (Exception ex)
            {
                AtModuleStatus = $"❌ 保存异常：{ex.Message}";
                _isWaitingATOk = false;
            }
        }

        public async Task RestoreDefaultAsync()
        {
            if (!CheckSerialConn()) return;

            AtModuleStatus = "正在恢复出厂配置...";
            try
            {
                _isWaitingATOk = true;
                bool enterOk = await SendAtCmdAsync("AT+CG\r\n");
                if (!enterOk)
                {
                    AtModuleStatus = "❌ 恢复失败：进入AT配置模式失败";
                    _isWaitingATOk = false;
                    return;
                }

                bool defOk = await SendAtCmdAsync("AT+DEFAULT\r\n");
                await SendAtCmdAsync("AT+ET\r\n");
                _isConfigMode = false;
                _isWaitingATOk = false;

                if (defOk)
                {
                    ResetDefaultValue();
                    AtModuleStatus = "✅ 出厂配置恢复完成";
                }
                else
                {
                    AtModuleStatus = "❌ 恢复出厂配置失败";
                }
            }
            catch (Exception ex)
            {
                AtModuleStatus = $"❌ 恢复异常：{ex.Message}";
                _isWaitingATOk = false;
            }
        }
        #endregion

        #region ==== 私有辅助方法 (原有保留) ====
        private bool CheckSerialConn()
        {
            if (!_comm.IsConnected)
            {
                AtModuleStatus = "❌ 错误：串口未连接，请先配置并打开串口";
                return false;
            }
            return true;
        }

        private bool CheckParamsLegal()
        {
            if (!int.TryParse(StdId, out int std) || std < 0 || std > 2047)
            {
                AtModuleStatus = "❌ 标准帧ID必须是0-2047的整数";
                return false;
            }
            if (!int.TryParse(ExtId, out int ext) || ext < 0 || ext > 536870911)
            {
                AtModuleStatus = "❌ 扩展帧ID必须是0-536870911的整数";
                return false;
            }
            return true;
        }

        private void ResetDefaultValue()
        {
            SelectedWorkMode = 0;
            SelectedCanBaud = 3;
            SelectedSerialBaud = 4;
            SelectedDataBit = 0;
            SelectedStopBit = 1;
            SelectedParity = 0;
            IsTransEnable = true;
            SelectedFrameFormat = 0;
            StdId = "136";
            ExtId = "0";
            IsFilterEnable = false;
            FilterId = "0";
            MaskId = "0";
        }

        private async Task<bool> SendAtCmdAsync(string cmd)
        {
            lock (_recvBuffer) { _recvBuffer.Clear(); }
            byte[] sendBytes = _encode.GetBytes(cmd);
            _comm.Send(sendBytes, 0, sendBytes.Length);

            int waitMs = 0;
            while (waitMs < AtCmdTimeout)
            {
                await Task.Delay(50);
                waitMs += 50;
                lock (_recvBuffer)
                {
                    if (_recvBuffer.ToString().Contains("OK\r\n")) return true;
                    if (_recvBuffer.ToString().Contains("ERROR\r\n")) return false;
                }
            }
            return false;
        }
        #endregion

        #region ==== 串口接收逻辑【你的原版写法，一字未改】核心重点 ====
        private void HandleSerialDataWrapper(object sender, DataReceivedEventArgs e)
        {
            HandleSerialData(e.Buffer, e.LastIndex, e.BufferLength);
        }

        private void HandleSerialData(byte[] buffer, int lastIndex, int bufferLength)
        {
            try
            {
                if (_isWaitingATOk)
                {
                    string recvAscii = Encoding.ASCII.GetString(buffer, lastIndex, bufferLength);
                    lock (_recvBuffer) { _recvBuffer.Append(recvAscii); }

                    if (recvAscii.Contains("OK\r\n"))
                    {
                        _isWaitingATOk = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AtModuleStatus = "✅ AT指令模式进入成功，CAN可正常工作";
                            _isWaitingATOk = false;
                        });
                        return;
                    }
                    else if (recvAscii.Contains("ERROR\r\n"))
                    {
                        _isWaitingATOk = false;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AtModuleStatus = "❌ AT指令执行失败，返回错误";
                            _isWaitingATOk = false;
                        });
                        return;
                    }
                }

                // ================ 你的原有CAN解析逻辑 直接粘贴这里 =====================
                // if (_dbcParser.DbcFrameList.Count == 0) return;
                // byte[] realData = new byte[bufferLength];
                // Array.Copy(buffer, lastIndex, realData, 0, bufferLength);
                // CanAtFrameInfo parsedFrame = CanAtFrameBuilder.ParseFrame(realData);
                // if (realData.Length >= CAN_FIXED_FRAME_LEN)
                // {
                //     uint canId = parsedFrame.IdInfo.ExtendedFrameId;
                //     byte[] canData = parsedFrame.Data;
                //     var targetFrame = _dbcParser.ParseCanData(canId, canData);
                //     if (targetFrame == null) return;
                //     Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, RefreshFrameList);
                // }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DBC解析数据异常: {ex.Message}");
            }
        }
        #endregion

        #region ==== 接口实现 (原有保留+解绑事件) ====
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void Dispose()
        {
            _comm.DataReceived -= HandleSerialDataWrapper;
            _comm?.Dispose();
        }
        #endregion
    }

    public class CanBaudItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
}