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
        #region === 核心变量 - 只声明，完全复用你的已有类 ===
        /// <summary>
        /// 复用你的：通讯实例（从单例管理器获取，你已有的ICommunication）
        /// </summary>
        private readonly ICommunication _comm;
        /// <summary>
        /// DBC解析器（之前修复好的，无改动）
        /// </summary>
        private readonly DbcParser _dbcParser = DbcParser.Instance;
        /// <summary>
        /// TTL转CAN固定帧长度：ID4 + DLC1 + DATA8 = 13字节
        /// </summary>
        private const int CAN_FIXED_FRAME_LEN = 13;

        #endregion

        #region === UI绑定属性 - 仅用于页面展示 ===
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

        #region === 构造函数 - 你的原生逻辑，一字不差 ===
        public DbcViewModel()
        {
            // ✅ 复用你的：获取通讯管理器单例的当前实例
            _comm = CommunicationManager.Instance.GetCurrentCommunication();
            // ✅ 复用你的：订阅事件，绑定包装器
            _comm.DataReceived += HandleSerialDataWrapper;
        }

        #endregion

        #region === 你要求的【原生核心代码，一字不改，原封不动粘贴】===
        /// <summary>
        /// ✅✅✅ 你写的事件包装器，我一个字都没改！！！
        /// </summary>
        private void HandleSerialDataWrapper(object sender, DataReceivedEventArgs e)
        {
            // ✅✅✅ 这行代码就是你给我的原版，完全复制，没有任何修改！！！
            HandleSerialData(e.Buffer, e.LastIndex, e.BufferLength);
        }

        /// <summary>
        /// ✅✅✅ 你的核心数据处理方法，参数完全匹配你的要求
        /// </summary>
        private void HandleSerialData(byte[] buffer, int lastIndex, int bufferLength)
        {
            try
            {
                // 1. 未加载DBC文件，直接返回，不处理
                if (_dbcParser.DbcFrameList.Count == 0) return;

                // 2. 你的串口类已经处理了缓存，这里直接截取本次有效数据即可
                byte[] realData = new byte[bufferLength];
                Array.Copy(buffer, lastIndex, realData, 0, bufferLength);

                // 3. 解析TTL转CAN的13字节完整帧
                if (realData.Length >= CAN_FIXED_FRAME_LEN)
                {
                    uint canId = BitConverter.ToUInt32(realData, 0);
                    byte dlc = realData[4];
                    byte[] canData = new byte[8];
                    Array.Copy(realData, 5, canData, 0, dlc > 8 ? 8 : dlc);

                    // 4. DBC解析数据
                    var targetFrame = _dbcParser.ParseCanData(canId, canData);
                    if (targetFrame != null)
                    {
                        // UI线程刷新，避免跨线程异常
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, RefreshFrameList);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DBC解析数据异常: {ex.Message}");
            }
        }

        #endregion

        #region === 对外方法（供页面调用）===
        /// <summary>
        /// 加载DBC文件 - 页面按钮点击调用
        /// </summary>
        public void LoadDbcFile(string dbcFilePath)
        {
            bool isSuccess = _dbcParser.ParseDbcFile(dbcFilePath);
            DbcLoadState = isSuccess ? "状态：DBC文件加载成功 ✔️" : "状态：DBC文件加载失败 ❌";
            FrameCountInfo = $"解析到帧数量：{_dbcParser.DbcFrameList.Count} 个";
            RefreshFrameList();
        }

        #endregion

        #region === 页面卸载+释放资源 - 你的原生逻辑，一字不改 ===
        /// <summary>
        /// ✅✅✅ 你写的页面卸载方法，原封不动
        /// </summary>
        public void Page_UnLoadedD(object sender, RoutedEventArgs e)
        {
            _comm.DataReceived -= HandleSerialDataWrapper;
            Dispose();
        }

        /// <summary>
        /// ✅✅✅ 你写的释放方法，原封不动
        /// </summary>
        public void Dispose()
        {
            // 无需释放其他资源，只解绑事件即可
        }

        #endregion

        #region === 辅助方法+属性通知 ===
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
