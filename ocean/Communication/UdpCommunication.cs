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
    // UDP通讯类（实现ICommunication）
    public class UdpCommunication : ICommunication
    {
        private UdpClient _udpClient;
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
            _udpClient = new UdpClient(_config.LocalPort);
            _isConnected = true;
            StartReceiveUdpData();
        }

        public void Close()
        {
            _udpClient?.Close();
            _isConnected = false;
        }

        public void Send(byte[] data, int offset, int length)
        {
            if (!_isConnected)
                throw new InvalidOperationException("UDP未启动");

            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(_config.RemoteIp), _config.RemotePort);
            _udpClient.Send(data, length, remoteEP);
        }

        public string FormatSendData(byte[] data, int length)
        {
            // 从数组索引0开始，取length长度的字节格式化
            return $"TX(UDP): {BitConverter.ToString(data, 0, length).Replace("-", " ")}";
        }

        private void StartReceiveUdpData()
        {
            _udpClient.BeginReceive((ar) =>
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = _udpClient.EndReceive(ar, ref remoteEP);
                    DataReceived?.Invoke(this, new DataReceivedEventArgs(data, 0, data.Length));
                    StartReceiveUdpData();
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
