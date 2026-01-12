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

                if (realData.Length >= CAN_FIXED_FRAME_LEN)
                {
                    uint canId = BitConverter.ToUInt32(realData, 0);
                    byte[] canData = new byte[8];
                    Array.Copy(realData, 5, canData, 0, Math.Min(realData[4], (byte)8));

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
