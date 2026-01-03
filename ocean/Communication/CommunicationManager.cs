using ocean.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ocean.ViewModels;

namespace ocean.Communication
{
    /// <summary>
    /// 通讯管理器（单例，管理全局串口实例）
    /// </summary>
    public class CommunicationManager
    {
        private static readonly Lazy<CommunicationManager> _instance =
            new Lazy<CommunicationManager>(() => new CommunicationManager());
        public static CommunicationManager Instance => _instance.Value;

        // 当前通讯实例（串口/以太网）
        private ICommunication _currentComm;
        // 当前通讯类型
        private CommunicationType _currentType;
        // 串口实例缓存（专门供SerialConfigPage调用）
        private SerialCommunication _serialInstance;

        // 1. 创建串口实例
        public void CreateSerialInstance()
        {
            // 释放原有串口实例
            _serialInstance?.Dispose();
            // 创建新串口实例并缓存
            _serialInstance = new SerialCommunication();
            // 设置为当前通讯实例
            _currentComm = _serialInstance;
            _currentType = CommunicationType.SerialPort;
        }

        // 2. 恢复 GetSerialInstance 方法（SerialConfigPage 专用）
        public SerialCommunication GetSerialInstance()
        {
            if (_serialInstance == null)
                throw new InvalidOperationException("请先在MainPage选择“串口”创建实例！");
            return _serialInstance;
        }

        // 2. 创建以太网实例（区分TCP/UDP）
        public void CreateEthernetInstance()
        {
            DisposeCurrentInstance();
            // 先创建空的以太网实例（后续由EthernetConfigViewModel替换为TCP/UDP）
            _currentComm = new EmptyEthernetCommunication();
            _currentType = CommunicationType.Ethernet;
        }


        // 3. 供EthernetConfigViewModel替换具体协议实例（TCP/UDP）
        public void ReplaceEthernetInstance(ICommunication ethernetComm)
        {
            if (_currentType != CommunicationType.Ethernet)
                throw new InvalidOperationException("当前非以太网类型，无法替换");

            DisposeCurrentInstance();
            _currentComm = ethernetComm;
        }

        // 3. 获取当前通讯实例
        public ICommunication GetCurrentCommunication()
        {
            if (_currentComm == null)
                throw new InvalidOperationException("请先在MainPage选择通讯方式！");
            return _currentComm;
        }

        // 4. 获取当前通讯类型
        public CommunicationType GetCurrentType() => _currentType;

        // 5. 释放当前实例
        public void DisposeCurrentInstance()
        {
            _currentComm?.Dispose();
            _currentComm = null;
        }

        // 空以太网实例（占位用）
        // 空以太网实例（占位用）
        private class EmptyEthernetCommunication : ICommunication
        {
            public bool IsConnected => false;
            public event EventHandler<DataReceivedEventArgs> DataReceived;
            public CommunicationConfig Config { get; set; }

            public void Open(CommunicationConfig config) { }
            public void Close() { }
            public void Send(byte[] data, int offset, int length) { }
            public string FormatSendData(byte[] data, int length) => string.Empty;
            public void Dispose() { }
        }

    }
}
