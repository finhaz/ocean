using ocean.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // 当前串口实例（替代原CommonRes.mySerialPort）
        private SerialCommunication _serialCommunication;

        private CommunicationManager() { }

        /// <summary>
        /// 创建串口实例（由MainPage初始化）
        /// </summary>
        public void CreateSerialInstance()
        {
            // 释放原有实例
            _serialCommunication?.Dispose();
            // 创建新串口实例
            _serialCommunication = new SerialCommunication();
        }

        /// <summary>
        /// 获取当前串口实例
        /// </summary>
        public SerialCommunication GetSerialInstance()
        {
            if (_serialCommunication == null)
                throw new InvalidOperationException("串口实例未初始化，请先在MainPage创建");

            return _serialCommunication;
        }

        /// <summary>
        /// 获取通用通讯接口（预留以太网扩展）
        /// </summary>
        public ICommunication GetCurrentCommunication()
        {
            return GetSerialInstance();
        }

        /// <summary>
        /// 释放串口实例
        /// </summary>
        public void DisposeSerialInstance()
        {
            _serialCommunication?.Dispose();
            _serialCommunication = null;
        }
    }
}
