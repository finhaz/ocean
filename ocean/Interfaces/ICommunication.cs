using ocean.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocean.Interfaces
{
    /// <summary>
    /// 通用通讯接口
    /// </summary>
    // 通用通讯接口（暂仅实现串口）
    public interface ICommunication : IDisposable
    {
        bool IsConnected { get; }
        event EventHandler<DataReceivedEventArgs> DataReceived;
        void Open(CommunicationConfig config);
        void Close();
        void Send(byte[] data, int offset, int length);
        string FormatSendData(byte[] data, int length);
    }

    // 数据接收事件参数
    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Buffer { get; }
        public int LastIndex { get; }
        public int BufferLength { get; }
        public DataReceivedEventArgs(byte[] buffer, int lastIndex, int bufferLength)
        {
            Buffer = buffer;
            LastIndex = lastIndex;
            BufferLength = bufferLength;
        }
    }


    // 通讯配置基类
    public abstract class CommunicationConfig
    {
        public CommunicationType Type { get; set; } = CommunicationType.Serial;
    }

    // 串口配置类（核心）
    public class SerialConfig : CommunicationConfig
    {
        // 绑定UI的配置项（与原有_globalVM.SerialConfig字段对应）
        public string SelectedPortName { get; set; }
        public int SelectedBaudRate { get; set; }
        public string SelectedParityName { get; set; }
        public int SelectedStopBit { get; set; }
        public int SelectedDataBits { get; set; } = 8; // 默认8位数据位

        // UI状态属性
        public bool IsConfigEnabled { get; set; } = true;
        public string TbComStateText { get; set; } = "串口未打开";
    }

    // 通讯类型枚举（预留以太网）
    public enum CommunicationType
    {
        Serial,
        Tcp
    }
}
