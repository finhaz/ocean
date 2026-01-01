using Microsoft.Win32;
using ocean.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ocean.ViewModels
{
    public class EthernetConfigViewModel : INotifyPropertyChanged
    {
        #region 字段和属性
        private string _selectedProtocol = "TCP";
        private string _tcpMode = "Server";
        private string _localIp = "127.0.0.1";
        private string _localPort = "8080";
        private string _remoteIp = "127.0.0.1";
        private string _remotePort = "8080";
        private string _dataLog = string.Empty;
        private string _sendData = string.Empty;
        private bool _isConnected;
        private TcpListener _tcpListener;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private UdpClient _udpClient;

        // 协议选择
        public string SelectedProtocol
        {
            get => _selectedProtocol;
            set
            {
                _selectedProtocol = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTcpSelected));
                OnPropertyChanged(nameof(IsRemoteConfigVisible));
            }
        }

        // TCP模式（Server/Client）
        public string TcpMode
        {
            get => _tcpMode;
            set
            {
                _tcpMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRemoteConfigVisible));
            }
        }

        // 本地IP和端口
        public string LocalIp
        {
            get => _localIp;
            set { _localIp = value; OnPropertyChanged(); }
        }

        public string LocalPort
        {
            get => _localPort;
            set { _localPort = value; OnPropertyChanged(); }
        }

        // 远程IP和端口
        public string RemoteIp
        {
            get => _remoteIp;
            set { _remoteIp = value; OnPropertyChanged(); }
        }

        public string RemotePort
        {
            get => _remotePort;
            set { _remotePort = value; OnPropertyChanged(); }
        }

        // 数据日志
        public string DataLog
        {
            get => _dataLog;
            set { _dataLog = value; OnPropertyChanged(); }
        }

        // 发送数据
        public string SendData
        {
            get => _sendData;
            set { _sendData = value; OnPropertyChanged(); }
        }

        // 连接状态
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConnectButtonText));
            }
        }

        // 连接按钮文本
        public string ConnectButtonText => IsConnected ? "断开" : "连接";

        // TCP是否选中
        public bool IsTcpSelected => SelectedProtocol == "TCP";

        // 是否显示远程配置（TCP客户端/UDP）
        public bool IsRemoteConfigVisible =>
            (SelectedProtocol == "TCP" && TcpMode == "Client") ||
            SelectedProtocol == "UDP";
        #endregion

        #region 命令
        public ICommand ConnectCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SendDataCommand { get; }
        public ICommand ClearLogCommand { get; }
        #endregion

        #region 构造函数
        public EthernetConfigViewModel()
        {
            // 初始化命令
            ConnectCommand = new DelegateCommand(ExecuteConnect);
            SaveCommand = new DelegateCommand(ExecuteSave);
            SendDataCommand = new DelegateCommand(ExecuteSendData);
            ClearLogCommand = new DelegateCommand(ExecuteClearLog);
        }
        #endregion

        #region 命令执行方法
        // 连接/断开
        private void ExecuteConnect()
        {
            try
            {
                if (IsConnected)
                {
                    // 断开连接
                    Disconnect();
                    AddLog("已断开连接");
                }
                else
                {
                    // 建立连接
                    if (SelectedProtocol == "TCP")
                    {
                        ConnectTcp();
                    }
                    else
                    {
                        ConnectUdp();
                    }
                    AddLog("连接成功");
                    IsConnected = true;
                }
            }
            catch (Exception ex)
            {
                AddLog($"连接失败：{ex.Message}");
                IsConnected = false;
            }
        }

        // 保存配置
        private void ExecuteSave()
        {
            try
            {
                // 这里可以实现配置保存逻辑（如保存到配置文件）
                AddLog("配置已保存");
                MessageBox.Show("配置保存成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                AddLog($"保存失败：{ex.Message}");
                MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 发送数据
        private void ExecuteSendData()
        {
            if (string.IsNullOrEmpty(SendData))
            {
                AddLog("发送失败：请输入要发送的数据");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(SendData);

                if (SelectedProtocol == "TCP" && IsConnected)
                {
                    _networkStream?.Write(data, 0, data.Length);
                    AddLog($"发送TCP数据：{SendData}");
                }
                else if (SelectedProtocol == "UDP" && _udpClient != null)
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(RemoteIp), int.Parse(RemotePort));
                    _udpClient.Send(data, data.Length, remoteEP);
                    AddLog($"发送UDP数据：{SendData}");
                }

                // 清空发送框
                SendData = string.Empty;
            }
            catch (Exception ex)
            {
                AddLog($"发送失败：{ex.Message}");
            }
        }

        // 清空日志
        private void ExecuteClearLog()
        {
            DataLog = string.Empty;
        }
        #endregion

        #region 网络操作方法
        // TCP连接
        private void ConnectTcp()
        {
            if (TcpMode == "Server")
            {
                // TCP服务器模式
                int port = int.Parse(LocalPort);
                _tcpListener = new TcpListener(IPAddress.Parse(LocalIp), port);
                _tcpListener.Start();

                // 异步监听客户端连接
                _tcpListener.BeginAcceptTcpClient(OnTcpClientConnected, null);
                AddLog($"TCP服务器已启动，监听 {LocalIp}:{LocalPort}");
            }
            else
            {
                
                // TCP客户端模式
                _tcpClient = new TcpClient();
                _tcpClient.Connect(RemoteIp, int.Parse(RemotePort));
                _networkStream = _tcpClient.GetStream();

                // 异步接收数据
                StartReceiveTcpData();
                AddLog($"已连接到TCP服务器 {RemoteIp}:{RemotePort}");
                
                /*
                // 第一步：创建TcpClient时，绑定指定的本地IP和端口
                IPAddress localIp = IPAddress.Parse(LocalIp); // 你的本地IP（127.0.0.1）
                int localPort = int.Parse(LocalPort); // 你的本地端口（8080）
                IPEndPoint localEP = new IPEndPoint(localIp, localPort);

                _tcpClient = new TcpClient(localEP); // 强制绑定本地IP和端口

                // 第二步：连接远程服务器
                _tcpClient.Connect(RemoteIp, int.Parse(RemotePort));
                _networkStream = _tcpClient.GetStream();

                // 异步接收数据
                StartReceiveTcpData();
                AddLog($"已连接到TCP服务器 {RemoteIp}:{RemotePort}，本地绑定：{localEP}");
                */
            }
        }

        // UDP连接
        private void ConnectUdp()
        {
            _udpClient = new UdpClient(int.Parse(LocalPort));

            // 异步接收数据
            StartReceiveUdpData();
            AddLog($"UDP客户端已启动，本地端口：{LocalPort}");
        }

        // 断开连接
        private void Disconnect()
        {
            if (_networkStream != null)
            {
                _networkStream.Close();
                _networkStream = null;
            }

            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient = null;
            }

            if (_tcpListener != null)
            {
                _tcpListener.Stop();
                _tcpListener = null;
            }

            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }

            IsConnected = false;
        }

        // TCP客户端连接回调
        private void OnTcpClientConnected(IAsyncResult ar)
        {
            try
            {
                if (_tcpListener == null) return;

                _tcpClient = _tcpListener.EndAcceptTcpClient(ar);
                _networkStream = _tcpClient.GetStream();

                // 继续监听新连接
                _tcpListener.BeginAcceptTcpClient(OnTcpClientConnected, null);

                // 开始接收数据
                StartReceiveTcpData();

                AddLog($"客户端已连接：{((IPEndPoint)_tcpClient.Client.RemoteEndPoint).ToString()}");
            }
            catch (Exception ex)
            {
                AddLog($"客户端连接失败：{ex.Message}");
            }
        }

        // 开始接收TCP数据
        private void StartReceiveTcpData()
        {
            // 第一步：先捕获当前的networkStream引用（避免后续被Disconnect置空）
            var currentStream = _networkStream;
            if (currentStream == null) return;

            byte[] buffer = new byte[1024];
            currentStream.BeginRead(buffer, 0, buffer.Length, (ar) =>
            {
                try
                {
                    // 第二步：再次校验当前流是否有效（双重保险）
                    if (currentStream == null || !currentStream.CanRead)
                    {
                        return;
                    }

                    int bytesRead = currentStream.EndRead(ar); // 改用捕获的currentStream，而非全局_networkStream
                    if (bytesRead > 0)
                    {
                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        AddLog($"接收TCP数据：{data}");

                        // 继续接收前，再次检查全局流是否有效（防止中途断开）
                        if (_networkStream != null)
                        {
                            StartReceiveTcpData();
                        }
                    }
                    else
                    {
                        AddLog("TCP连接已关闭");
                        // 断开前校验，避免重复调用Disconnect
                        if (IsConnected)
                        {
                            Disconnect();
                        }
                    }
                }
                catch (IOException ex)
                {
                    // 捕获连接断开类异常（如对方主动关闭）
                    AddLog($"TCP连接异常断开：{ex.Message}");
                    if (IsConnected)
                    {
                        Disconnect();
                    }
                }
                catch (NullReferenceException)
                {
                    // 兜底捕获空引用异常，避免程序崩溃
                    AddLog("TCP接收数据时流已释放");
                }
                catch (Exception ex)
                {
                    AddLog($"接收TCP数据失败：{ex.Message}");
                }
            }, null);
        }

        // 开始接收UDP数据
        private void StartReceiveUdpData()
        {
            if (_udpClient == null) return;

            _udpClient.BeginReceive((ar) =>
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = _udpClient.EndReceive(ar, ref remoteEP);

                    if (data.Length > 0)
                    {
                        string text = Encoding.UTF8.GetString(data);
                        AddLog($"接收UDP数据 [{remoteEP}]: {text}");
                    }

                    // 继续接收
                    StartReceiveUdpData();
                }
                catch (Exception ex)
                {
                    AddLog($"接收UDP数据失败：{ex.Message}");
                }
            }, null);
        }

        // 添加日志
        private void AddLog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DataLog += $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";

                // 自动滚动到最后一行
                // 如果需要更精确的滚动，可以在XAML中给TextBox命名并操作ScrollViewer
            });
        }
        #endregion

        #region INotifyPropertyChanged实现
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

}