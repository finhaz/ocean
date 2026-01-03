using Microsoft.Win32;
using ocean.Communication;
using ocean.Interfaces;
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
        // 原有字段完全保留
        private string _selectedProtocol = "TCP";
        private string _tcpMode = "Server";
        private string _localIp = "127.0.0.1";
        private string _localPort = "8080";
        private string _remoteIp = "127.0.0.1";
        private string _remotePort = "8080";
        private string _dataLog = string.Empty;
        private string _sendData = string.Empty;

        // 当前以太网实例（TCP/UDP）
        private ICommunication _ethernetComm;

        // 原有属性完全保留
        public string SelectedProtocol
        {
            get => _selectedProtocol;
            set
            {
                _selectedProtocol = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTcpSelected));
                OnPropertyChanged(nameof(IsRemoteConfigVisible));
                // 切换协议时重新创建实例
                CreateEthernetProtocolInstance();
            }
        }

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

        public string DataLog
        {
            get => _dataLog;
            set { _dataLog = value; OnPropertyChanged(); }
        }

        public string SendData
        {
            get => _sendData;
            set { _sendData = value; OnPropertyChanged(); }
        }

        public bool IsConnected
        {
            get => _ethernetComm?.IsConnected ?? false;
            set
            {
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConnectButtonText));
            }
        }

        public string ConnectButtonText => IsConnected ? "断开" : "连接";
        public bool IsTcpSelected => SelectedProtocol == "TCP";
        public bool IsRemoteConfigVisible =>
            (SelectedProtocol == "TCP" && TcpMode == "Client") ||
            SelectedProtocol == "UDP";

        // 原有命令保留
        public ICommand ConnectCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SendDataCommand { get; }
        public ICommand ClearLogCommand { get; }

        public EthernetConfigViewModel()
        {
            // 初始化时创建默认协议实例（TCP）
            CreateEthernetProtocolInstance();

            // 原有命令初始化保留
            ConnectCommand = new DelegateCommand(ExecuteConnect);
            SaveCommand = new DelegateCommand(ExecuteSave);
            SendDataCommand = new DelegateCommand(ExecuteSendData);
            ClearLogCommand = new DelegateCommand(ExecuteClearLog);
        }

        // 根据SelectedProtocol创建TCP/UDP实例
        private void CreateEthernetProtocolInstance()
        {
            try
            {
                // 释放原有实例
                _ethernetComm?.Dispose();

                // 根据选择的协议创建实例
                if (SelectedProtocol == "TCP")
                {
                    _ethernetComm = new TcpCommunication();
                }
                else
                {
                    _ethernetComm = new UdpCommunication();
                }

                // 替换CommunicationManager中的实例
                CommunicationManager.Instance.ReplaceEthernetInstance(_ethernetComm);

                // 绑定数据接收事件
                _ethernetComm.DataReceived += OnEthernetDataReceived;
                AddLog($"已切换到{SelectedProtocol}协议");
            }
            catch (Exception ex)
            {
                AddLog($"切换协议失败：{ex.Message}");
            }
        }

        // 连接/断开逻辑（原有逻辑适配）
        private void ExecuteConnect()
        {
            try
            {
                if (IsConnected)
                {
                    _ethernetComm.Close();
                    AddLog($"已断开{SelectedProtocol}连接");
                }
                else
                {
                    // 构建配置
                    var config = new EthernetConfig
                    {
                        Type = CommunicationType.Ethernet,
                        SelectedProtocol = SelectedProtocol,
                        TcpMode = TcpMode,
                        LocalIp = LocalIp,
                        LocalPort = int.Parse(LocalPort),
                        RemoteIp = RemoteIp,
                        RemotePort = int.Parse(RemotePort)
                    };

                    // 打开连接
                    _ethernetComm.Open(config);
                    AddLog($"{SelectedProtocol}({TcpMode})连接成功");
                }
                IsConnected = _ethernetComm.IsConnected;
            }
            catch (Exception ex)
            {
                AddLog($"{SelectedProtocol}连接失败：{ex.Message}");
                IsConnected = false;
            }
        }

        // 发送数据逻辑（原有逻辑适配）
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
                _ethernetComm.Send(data, 0, data.Length);
                AddLog($"发送{SelectedProtocol}数据：{SendData}");
                SendData = string.Empty;
            }
            catch (Exception ex)
            {
                AddLog($"发送失败：{ex.Message}");
            }
        }

        // 数据接收处理
        private void OnEthernetDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Buffer, e.LastIndex, e.BufferLength);
            AddLog($"接收{SelectedProtocol}数据：{data}");
        }

        // 原有方法完全保留
        private void ExecuteSave()
        {
            try
            {
                AddLog("以太网配置已保存");
                MessageBox.Show("配置保存成功！", "提示");
            }
            catch (Exception ex)
            {
                AddLog($"保存失败：{ex.Message}");
            }
        }

        private void ExecuteClearLog()
        {
            DataLog = string.Empty;
        }

        private void AddLog(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DataLog += $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            });
        }

        // INotifyPropertyChanged实现保留
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // TCP通讯实现（独立类）
    public class TcpCommunication : ICommunication
    {
        private TcpListener _tcpListener;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private EthernetConfig _config;

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public bool IsConnected { get; private set; }
        public CommunicationConfig Config
        {
            get => _config;
            set => _config = (EthernetConfig)value;
        }

        public void Open(CommunicationConfig config)
        {
            _config = (EthernetConfig)config;
            IsConnected = false;

            if (_config.TcpMode == "Server")
            {
                _tcpListener = new TcpListener(IPAddress.Parse(_config.LocalIp), _config.LocalPort);
                _tcpListener.Start();
                _tcpListener.BeginAcceptTcpClient(OnTcpClientConnected, null);
                IsConnected = true;
            }
            else
            {
                _tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse(_config.LocalIp), _config.LocalPort));
                _tcpClient.Connect(_config.RemoteIp, _config.RemotePort);
                _networkStream = _tcpClient.GetStream();
                IsConnected = true;
                StartReceiveData();
            }
        }

        public void Close()
        {
            _networkStream?.Close();
            _tcpClient?.Close();
            _tcpListener?.Stop();
            IsConnected = false;
        }

        public void Send(byte[] data, int offset, int length)
        {
            if (!IsConnected || _networkStream == null)
                throw new InvalidOperationException("TCP未连接");
            _networkStream.Write(data, offset, length);
        }

        public string FormatSendData(byte[] data, int length)
        {
            return $"TX(TCP): {BitConverter.ToString(data).Replace("-", " ")}";
        }

        private void OnTcpClientConnected(IAsyncResult ar)
        {
            try
            {
                _tcpClient = _tcpListener.EndAcceptTcpClient(ar);
                _networkStream = _tcpClient.GetStream();
                _tcpListener.BeginAcceptTcpClient(OnTcpClientConnected, null);
                StartReceiveData();
            }
            catch { }
        }

        private void StartReceiveData()
        {
            byte[] buffer = new byte[1024];
            _networkStream?.BeginRead(buffer, 0, buffer.Length, (ar) =>
            {
                try
                {
                    int bytesRead = _networkStream.EndRead(ar);
                    if (bytesRead > 0)
                    {
                        DataReceived?.Invoke(this, new DataReceivedEventArgs(buffer, 0, bytesRead));
                        StartReceiveData();
                    }
                }
                catch { }
            }, null);
        }

        public void Dispose() => Close();
    }

    // UDP通讯实现（独立类）
    public class UdpCommunication : ICommunication
    {
        private UdpClient _udpClient;
        private EthernetConfig _config;

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public bool IsConnected { get; private set; }
        public CommunicationConfig Config
        {
            get => _config;
            set => _config = (EthernetConfig)value;
        }

        public void Open(CommunicationConfig config)
        {
            _config = (EthernetConfig)config;
            _udpClient = new UdpClient(_config.LocalPort);
            IsConnected = true;
            StartReceiveData();
        }

        public void Close()
        {
            _udpClient?.Close();
            IsConnected = false;
        }

        public void Send(byte[] data, int offset, int length)
        {
            if (!IsConnected)
                throw new InvalidOperationException("UDP未启动");
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(_config.RemoteIp), _config.RemotePort);
            _udpClient.Send(data, length, remoteEP);
        }

        public string FormatSendData(byte[] data, int length)
        {
            return $"TX(UDP): {BitConverter.ToString(data).Replace("-", " ")}";
        }

        private void StartReceiveData()
        {
            _udpClient.BeginReceive((ar) =>
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = _udpClient.EndReceive(ar, ref remoteEP);
                    DataReceived?.Invoke(this, new DataReceivedEventArgs(data, 0, data.Length));
                    StartReceiveData();
                }
                catch { }
            }, null);
        }

        public void Dispose() => Close();
    }

}