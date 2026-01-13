using ocean.Communication;
using ocean.Interfaces;
using ocean.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ocean.ViewModels
{
    public class DbcViewModel : INotifyPropertyChanged, IDisposable
    {
        #region === 核心变量 ===
        private readonly ICommunication _comm;
        private readonly DbcParser _dbcParser = DbcParser.Instance;
        private const int CAN_FIXED_FRAME_LEN = 13;

        #endregion

        #region === UI绑定属性 ===
        private string _dbcLoadState = "状态：未加载DBC文件";
        private string _frameCountInfo = "解析到帧数量：0 个";
        private ObservableCollection<CanFrameDefine> _dbcFrameList = new ObservableCollection<CanFrameDefine>();

        public string DbcLoadState
        {
            get => _dbcLoadState;
            set { _dbcLoadState = value; OnPropertyChanged(); }
        }
        public string FrameCountInfo
        {
            get => _frameCountInfo;
            set { _frameCountInfo = value; OnPropertyChanged(); }
        }
        public ObservableCollection<CanFrameDefine> DbcFrameList
        {
            get => _dbcFrameList;
            set { _dbcFrameList = value; OnPropertyChanged(); }
        }

        // ====== 新增：发送功能绑定属性 ======
        private CanFrameDefine _selectedSendFrame;
        /// <summary>
        /// 选中要发送的CAN帧
        /// </summary>
        public CanFrameDefine SelectedSendFrame
        {
            get => _selectedSendFrame;
            set { _selectedSendFrame = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 绑定UI的发送状态提示
        /// </summary>
        private string _sendStatus = "发送状态：就绪";
        public string SendStatus
        {
            get => _sendStatus;
            set { _sendStatus = value; OnPropertyChanged(); }
        }


        #endregion

        #region === 构造函数 ===
        public DbcViewModel()
        {
            _comm = CommunicationManager.Instance.GetCurrentCommunication();
            _comm.DataReceived += HandleSerialDataWrapper;
        }

        #endregion

        #region === 原生事件处理 ===
        private void HandleSerialDataWrapper(object sender, DataReceivedEventArgs e)
        {
            HandleSerialData(e.Buffer, e.LastIndex, e.BufferLength);
        }

        private void HandleSerialData(byte[] buffer, int lastIndex, int bufferLength)
        {
            try
            {
                if (_dbcParser.DbcFrameList.Count == 0) return;

                byte[] realData = new byte[bufferLength];
                Array.Copy(buffer, lastIndex, realData, 0, bufferLength);

                // 解析AT指令
                CanAtFrameInfo parsedFrame = CanAtFrameBuilder.ParseFrame(realData);
                //parsedFrame.

                if (realData.Length >= CAN_FIXED_FRAME_LEN)
                {
                    //uint canId = BitConverter.ToUInt32(realData, 0);
                    uint canId=parsedFrame.IdInfo.ExtendedFrameId;
                    //byte[] canData = new byte[8];
                    //Array.Copy(realData, 5, canData, 0, Math.Min(realData[4], (byte)8));

                    byte[] canData=parsedFrame.Data;

                    var targetFrame = _dbcParser.ParseCanData(canId, canData);
                    if (targetFrame == null) return;

                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, RefreshFrameList);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DBC解析数据异常: {ex.Message}");
            }
        }

        #endregion

        #region === 对外方法 ===
        public void LoadDbcFile(string dbcFilePath)
        {
            bool isSuccess = _dbcParser.ParseDbcFile(dbcFilePath);
            DbcLoadState = isSuccess ? "状态：DBC文件加载成功 ✔️" : "状态：DBC文件加载失败 ❌";
            FrameCountInfo = $"解析到帧数量：{_dbcParser.DbcFrameList.Count} 个";
            RefreshFrameList();
        }

        public void Page_UnLoadedD(object sender, RoutedEventArgs e)
        {
            _comm.DataReceived -= HandleSerialDataWrapper;
            Dispose();
        }

        public void Dispose()
        {
        }

        #endregion

        #region
        // ====== 新增：发送功能核心方法 ======
        /// <summary>
        /// 对外暴露的发送按钮点击事件 - UI按钮直接绑定这个方法
        /// </summary>
        public void SendCanFrameByDbc()
        {
            try
            {
                if (_selectedSendFrame == null)
                {
                    SendStatus = "发送状态：请先选择要发送的帧！";
                    return;
                }
                if (_comm == null || !_comm.IsConnected)
                {
                    SendStatus = "发送状态：串口未打开！";
                    return;
                }

                // 1. 核心：生成要发送的13字节裸数据 (4字节ID+1字节DLC+8字节Data)
                byte[] sendData = PackSendCanData(_selectedSendFrame);
                // 2. 调用串口发送
                _comm.Send(sendData,0,sendData.Length);
                // 3. 更新发送状态
                //SendStatus = isSendOk ? $"发送状态：发送成功 ✔️ 帧名：{_selectedSendFrame.FrameName}" : "发送状态：发送失败 ❌";
                SendStatus = $"发送状态:发送";
            }
            catch (Exception ex)
            {
                SendStatus = $"发送状态：异常 - {ex.Message}";
            }
        }

        /// <summary>
        /// 核心打包方法：按规则生成 13字节发送数据
        /// 格式：【4字节小端FrameId】 + 【1字节DataLength】 + 【8字节信号数据】
        /// </summary>
        private byte[] PackSendCanData(CanFrameDefine frame)
        {
            byte[] sendBuffer = new byte[13];//固定13字节
            byte[] dataBuffer = new byte[8];//固定8字节CAN数据域

            
            // 步骤1：填充4字节小端FrameId (和你接收的格式完全一致，必须小端！)
            byte[] frameIdBytes = BitConverter.GetBytes(frame.FrameId);
           // Array.Copy(frameIdBytes, 0, sendBuffer, 0, 4);

            // 步骤2：填充1字节DataLength (你的实体类里的DataLength，就是DLC，0-8)
            sendBuffer[4] = frame.DataLength;

            // 步骤3：填充8字节信号数据域 - 把帧内所有信号的RawValue按位打包到8字节数组
            if (frame.Signals != null && frame.Signals.Count > 0)
            {
                foreach (var signal in frame.Signals)
                {
                    PackSignalToDataBuffer(signal, dataBuffer);
                }
            }

            // 步骤4：把8字节数据域填充到13字节的5-12位
            //Array.Copy(dataBuffer, 0, sendBuffer, 5, 8);
            

            byte[] framed = CanAtFrameBuilder.BuildFrame(frame.FrameId, true, false, dataBuffer);


            return framed;
        }

        /// <summary>
        /// 信号值按位打包到8字节数组 - 核心位运算，和你解析的位规则完全一致
        /// 把Signal的RawValue，按StartBit和BitLength写入对应位，完美适配你的DBC定义
        /// </summary>
        private void PackSignalToDataBuffer(CanSignalDefine signal, byte[] dataBuffer)
        {
            long rawValue = signal.RawValue;
            int startBit = signal.StartBit;
            int bitLength = signal.BitLength;

            for (int i = 0; i < bitLength; i++)
            {
                int bitPos = startBit + i;
                int byteIndex = bitPos / 8;
                int bitIndex = bitPos % 8;

                if (byteIndex >= 8) continue;//超出8字节范围，忽略

                // 把原始值的第i位，写入数据缓冲区的对应位
                if ((rawValue & (1L << i)) != 0)
                {
                    dataBuffer[byteIndex] |= (byte)(1 << bitIndex);
                }
                else
                {
                    dataBuffer[byteIndex] &= (byte)~(1 << bitIndex);
                }
            }
        }

        #endregion




        #region === 辅助方法 ===
        private void RefreshFrameList()
        {
            DbcFrameList.Clear();
            foreach (var frame in _dbcParser.DbcFrameList)
            {
                DbcFrameList.Add(frame);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
