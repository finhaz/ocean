using ocean.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ocean.Communication
{
    // TCP通讯类（实现ICommunication）
    public class TcpCommunication : ICommunication
    {
        private TcpListener _tcpListener;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private EthernetConfig _config;
        private bool _isConnected;

        public event EventHandler<DataReceivedEventArgs> DataReceived;
        public bool IsConnected => _isConnected;
        public CommunicationConfig Config
        {
            get => _config;
            set => _config = (EthernetConfig)value;
        }

        public void Open(CommunicationConfig config)
        {
            _config = (EthernetConfig)config;
            _isConnected = false;

            if (_config.TcpMode == "Server")
            {
                // TCP服务器模式
                _tcpListener = new TcpListener(IPAddress.Parse(_config.LocalIp), _config.LocalPort);
                _tcpListener.Start();
                _tcpListener.BeginAcceptTcpClient(OnTcpClientConnected, null);
                _isConnected = true;
            }
            else
            {
                // TCP客户端模式
                _tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse(_config.LocalIp), _config.LocalPort));
                _tcpClient.Connect(_config.RemoteIp, _config.RemotePort);
                _networkStream = _tcpClient.GetStream();
                _isConnected = true;
                StartReceiveTcpData();
            }
        }

        public void Close()
        {
            _networkStream?.Close();
            _tcpClient?.Close();
            _tcpListener?.Stop();
            _isConnected = false;
        }

        public void Send(byte[] data, int offset, int length)
        {
            if (!_isConnected || _networkStream == null)
                throw new InvalidOperationException("TCP未连接");

            _networkStream.Write(data, offset, length);
        }

        public string FormatSendData(byte[] data, int length)
        {
            // 从数组索引0开始，取length长度的字节格式化
            return $"TX(TCP): {BitConverter.ToString(data, 0, length).Replace("-", " ")}";
        }

        private void OnTcpClientConnected(IAsyncResult ar)
        {
            try
            {
                _tcpClient = _tcpListener.EndAcceptTcpClient(ar);
                _networkStream = _tcpClient.GetStream();
                _tcpListener.BeginAcceptTcpClient(OnTcpClientConnected, null);
                StartReceiveTcpData();
            }
            catch { }
        }

        private void StartReceiveTcpData()
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
                        StartReceiveTcpData();
                    }
                }
                catch { }
            }, null);
        }

        public void Dispose()
        {
            Close();
        }
    }

}
