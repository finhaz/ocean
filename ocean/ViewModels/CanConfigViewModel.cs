using ocean.Communication;
using ocean.Interfaces;
using ocean.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ocean.ViewModels
{
    public class CanConfigViewModel : ObservableObject, IDisposable
    {
        #region ==== 核心字段(原有+新增串口配置字段，严格适配你的SerialConfig/SerialCommunication) ====
        private readonly ICommunication _comm;
        private readonly StringBuilder _recvBuffer = new();
        private const int AtCmdTimeout = 1200;
        private readonly Encoding _encode = Encoding.ASCII;
        private bool _isConfigMode = false;
        public bool _isWaitingATOk = false;

        public SerialPortConfig PortConfig { get; set; } = new SerialPortConfig();

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

        private int _selectedSerialBaud = 1;
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
            PortConfig.PortNames= new ObservableCollection<string>(SerialPort.GetPortNames());
            AtModuleStatus = PortConfig.PortNames.Count > 0 ? "已加载串口号，请选择配置" : "未检测到可用串口";
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
                    SelectedPortName = PortConfig.SelectedPortName,
                    SelectedBaudRate = PortConfig.SelectedBaudRate,
                    SelectedDataBits = PortConfig.SelectedDataBit,
                    SelectedParityName = PortConfig.SelectedParityName,
                    SelectedStopBit = PortConfig.SelectedStopBit
                };
                _comm.Open(config);
                //AtModuleStatus = $"✅ 串口打开成功：{SelectedPortName} {SelectedPortBaud}bps";
                AtModuleStatus = $"✅ 串口打开成功：{PortConfig.SelectedPortName} {PortConfig.SelectedBaudRate}bps";
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
        /// <summary>
        /// 读取模块当前所有配置【终极完善，解析所有三类查询指令，参数自动同步到界面】
        /// </summary>
        public async Task ReadCanConfigAsync()
        {
            if (!CheckSerialConn()) return;

            AtModuleStatus = "正在读取模块配置...";
            try
            {
                // 进入AT配置模式
                _isWaitingATOk = true;
                bool enterOk = await SendAtCmdAsync("AT+CG\r\n");
                if (!enterOk)
                {
                    AtModuleStatus = "❌ 读取失败：进入AT配置模式失败";
                    _isWaitingATOk = false;
                    return;
                }

                // ========== 1. 读取CAN工作模式 AT+CAN_MODE=? ==========
                string modeResult = await SendAtQueryCmdAsync("AT+CAN_MODE=?\r\n");
                if (!string.IsNullOrEmpty(modeResult) && modeResult != "ERROR")
                {
                    string modeVal = ParseAtResultValue(modeResult);
                    if (int.TryParse(modeVal, out int mode))
                    {
                        SelectedWorkMode = mode;
                    }
                }

                // ========== 2. 读取CAN波特率 AT+CAN_BAUD=? ==========
                string baudResult = await SendAtQueryCmdAsync("AT+CAN_BAUD=?\r\n");
                if (!string.IsNullOrEmpty(baudResult) && baudResult != "ERROR")
                {
                    string baudVal = ParseAtResultValue(baudResult);
                    if (int.TryParse(baudVal, out int baud))
                    {
                        var matchItem = CanBaudList.Find(x => x.Value == baud);
                        if (matchItem != null)
                        {
                            SelectedCanBaud = CanBaudList.IndexOf(matchItem);
                        }
                    }
                }

                // ========== 3. 读取串口参数 AT+USART_PARAM=? ✅核心解析✅ ==========
                string usartResult = await SendAtQueryCmdAsync("AT+USART_PARAM=?\r\n");
                if (!string.IsNullOrEmpty(usartResult) && usartResult != "ERROR")
                {
                    ParseUsartParam(usartResult);
                }

                // ========== 4. 读取CAN帧格式配置 AT+CAN_FRAMEFORMAT=? ✅核心解析✅ ==========
                string frameResult = await SendAtQueryCmdAsync("AT+CAN_FRAMEFORMAT=?\r\n");
                if (!string.IsNullOrEmpty(frameResult) && frameResult != "ERROR")
                {
                    ParseCanFrameFormat(frameResult);
                }

                // ========== 5. 读取CAN滤波器配置 AT+CAN_FILTER0=? ✅核心解析✅ ==========
                string filterResult = await SendAtQueryCmdAsync("AT+CAN_FILTER0=?\r\n");
                if (!string.IsNullOrEmpty(filterResult) && filterResult != "ERROR")
                {
                    ParseCanFilter(filterResult);
                }

                // 退出配置模式
                await SendAtCmdAsync("AT+ET\r\n");
                _isConfigMode = false;
                _isWaitingATOk = false;
                AtModuleStatus = "✅ 配置读取完成，所有参数已同步到界面";
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
                
                //int serialBaud = SerialBaudList[SelectedSerialBaud];
                int serialBaud = PortConfig.BaudRates[SelectedSerialBaud];
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
            SelectedSerialBaud = 1;
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
                    if (_recvBuffer.ToString().Contains("OK\r\n")) 
                        return true;
                    if (_recvBuffer.ToString().Contains("ERROR\r\n")) 
                        return false;
                }
            }
            return false;
        }


        /// <summary>
        /// 版本2 - 发送查询类AT指令，返回完整的模块响应内容【核心适配多行/多参数返回】
        /// 适用：所有带=?的查询指令，兼容单行/多行返回，自动过滤无效内容
        /// 返回：纯净的指令返回行 如 +CAN_MODE:0 、+USART_PARAM:115200,8,1,0
        /// </summary>
        private async Task<string> SendAtQueryCmdAsync(string cmd)
        {
            lock (_recvBuffer) { _recvBuffer.Clear(); }
            byte[] sendBytes = _encode.GetBytes(cmd);
            _comm.Send(sendBytes, 0, sendBytes.Length);

            int waitMs = 0;
            while (waitMs < AtCmdTimeout)
            {
                await Task.Delay(30);
                waitMs += 30;
                lock (_recvBuffer)
                {
                    string recvStr = _recvBuffer.ToString();
                    if (!string.IsNullOrEmpty(recvStr))
                    {
                        // 核心适配：过滤所有无效内容，只提取以 + 开头的有效指令返回行
                        foreach (var line in recvStr.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (line.StartsWith("+"))
                            {
                                return line.Trim();
                            }
                        }
                        // 如果匹配到ERROR，直接返回ERROR
                        if (recvStr.Contains("ERROR"))
                        {
                            return "ERROR";
                        }
                    }
                }
            }
            return string.Empty; //超时返回空
        }

        /// <summary>
        /// 通用解析：提取AT指令返回的参数部分 如 +XXX:内容 → 内容
        /// </summary>
        private string ParseAtResultValue(string atResult)
        {
            if (string.IsNullOrEmpty(atResult) || !atResult.Contains(":") || atResult == "ERROR")
                return string.Empty;
            return atResult.Split(new[] { ':' }, 2)[1].Trim();
        }

        #endregion

        #region ✅ 新增：3个专用解析方法【精准解析三类查询指令的多参数返回】✅
        /// <summary>
        /// 解析 AT+USART_PARAM=? 返回值 → +USART_PARAM:<Baud>,<DataBit>,<StopBit>,<ParityBit>
        /// 参数顺序：波特率,数据位,停止位,校验位
        /// </summary>
        private void ParseUsartParam(string usartResult)
        {
            string paramStr = ParseAtResultValue(usartResult);
            if (string.IsNullOrEmpty(paramStr)) return;

            string[] paramArr = paramStr.Split(',');
            if (paramArr.Length != 4) return;

            // 解析波特率 → 匹配下拉列表赋值
            if (int.TryParse(paramArr[0], out int baud) && PortConfig.BaudRates.Contains(baud))
            {
                SelectedSerialBaud = PortConfig.BaudRates.IndexOf(baud);
            }
            // 解析数据位 →直接赋值 0=8位 1=9位
            if (int.TryParse(paramArr[1], out int databit))
            {
                SelectedDataBit = databit;
            }
            // 解析停止位 → 直接赋值
            if (int.TryParse(paramArr[2], out int stopbit))
            {
                SelectedStopBit = stopbit;
            }
            // 解析校验位 → 直接赋值
            if (int.TryParse(paramArr[3], out int parity))
            {
                SelectedParity = parity;
            }
        }

        /// <summary>
        /// 解析 AT+CAN_FRAMEFORMAT=? 返回值 → +CAN_FRAMEFORMAT:<Enable>,<FrameFormat>,<StdID>,<ExtID>
        /// 参数顺序：透传使能,帧格式,标准ID,扩展ID
        /// </summary>
        private void ParseCanFrameFormat(string frameResult)
        {
            string paramStr = ParseAtResultValue(frameResult);
            if (string.IsNullOrEmpty(paramStr)) return;

            string[] paramArr = paramStr.Split(',');
            if (paramArr.Length != 4) return;

            // 解析透传使能 → bool值
            if (int.TryParse(paramArr[0], out int enable))
            {
                IsTransEnable = enable == 1;
            }
            // 解析帧格式 → 0=标准帧 1=扩展帧
            if (int.TryParse(paramArr[1], out int frameFormat))
            {
                SelectedFrameFormat = frameFormat;
            }
            // 解析标准ID
            StdId = paramArr[2].Trim();
            // 解析扩展ID
            ExtId = paramArr[3].Trim();
        }

        /// <summary>
        /// 解析 AT+CAN_FILTER0=? 返回值 → +CAN_FILTER0:<Enable>,<Mode>,<Id>,<MaskId>
        /// 参数顺序：滤波器使能,滤波模式,过滤ID,掩码ID
        /// </summary>
        private void ParseCanFilter(string filterResult)
        {
            string paramStr = ParseAtResultValue(filterResult);
            if (string.IsNullOrEmpty(paramStr)) return;

            string[] paramArr = paramStr.Split(',');
            if (paramArr.Length != 4) return;

            // 解析滤波器使能 → bool值
            if (int.TryParse(paramArr[0], out int enable))
            {
                IsFilterEnable = enable == 1;
            }
            // 解析过滤ID
            FilterId = paramArr[2].Trim();
            // 解析掩码ID
            MaskId = paramArr[3].Trim();
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

                    /*
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
                    */
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DBC解析数据异常: {ex.Message}");
            }
        }
        #endregion

        #region ==== 接口实现 (原有保留+解绑事件) ====

        public void Dispose()
        {
            _comm.DataReceived -= HandleSerialDataWrapper;
            //_comm?.Dispose();
        }
        #endregion
    }

    public class CanBaudItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
}