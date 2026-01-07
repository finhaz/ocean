using Microsoft.Win32;
using ocean.database;
using ocean.Interfaces;
using ocean.Mvvm;
using SomeNameSpace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace ocean.Communication
{
    public class DeviceControlPageViewModel: ObservableObject
    {
        //自定义运行界面的地址

        private string raddText = "128";
        public string RaddText
        {
            get => raddText;
            set => SetProperty(ref raddText, value);
        }


        //控件绑定相关

        //泛型控件
        // 定义TextBlock<string>实例（供XAML绑定）
        public TextBlock<string> SerialTextBlock { get; set; }

        private string bshowcontet = "数据采集";
        public string BshowContent
        {
            get => bshowcontet;
            set => SetProperty(ref bshowcontet, value); // 一行搞定，无需重复逻辑
        }

        private string protocolNum = "Modbus RTU协议";
        public string ProtocolNum
        {
            get => protocolNum;
            set => SetProperty(ref protocolNum, value);
        }


        public ICommand ButtonIncrease { get; }


        //数据库对接
        public int select_index;
        public string newValue;
        public bool brun = false;
        public bool bshow = false;
        //bool bmodify = false;
        //bool Initialized = true;//调试参数与修正系数核验标志位
        public bool flag_uncon = false;//上下参数不一致标志位
        //bool flag_get_runvalue = false;//读取运行数据标志位


        public bool flag_under_first = false;
        public bool flag_upper_first = false;

        public bool sopen = false;
        public float old_value;
        public float new_value;
        public int sn = 0;
        public int mrow;
        public int Num_time = 0;
        public int Num_DSP = 0;
        static int Set_Num_DSP = 2;//代表有机子数

        public int runnum;
        public byte[] sendbf = new byte[128];

        //定时器
        private DispatcherTimer mDataTimer = null; //定时器
        private long timerExeCount = 0; //定时器执行次数
        DateTime s1;
        DateTime s2;

        // 核心：当前选中的协议实例（直接复用原有单例）
        private IProtocol _currentProtocol;

        // 核心：当前选中的数据库实例（直接复用原有单例）
        //private IDatabaseOperation _dbOperation=new DB_SQLlite();
        // 新增：全局通讯实例（ViewModel内唯一，与SerialConfigView共用同一个实例）
        // 利用CommunicationManager单例特性，保证和SerialConfigView的串口实例完全一致
        private ICommunication _comm;

        public DeviceControlPageViewModel() 
        {

            SerialTextBlock = new TextBlock<string>();
            // 初始化命令：执行逻辑 + 可执行判断
            ButtonIncrease = new MyCommand(
                execAction: ButtonIncreaseAction,
                changeFunc: parameter => true     // 可选：可执行条件（始终返回true）
            );

            InitTimer();
            
        }

        // 协议切换（选择框事件调用）
        public void InitProtocol(string protocolNum)
        {
            // 若需要运行时切换通讯方式，可定时/按需刷新实例（示例）
            _comm = CommunicationManager.Instance.GetCurrentCommunication();
            // 直接赋值原有单例，无需新建对象
            _currentProtocol = protocolNum switch
            {
                "FE协议" => COMFE.Instance,       // COMFE实现了IProtocol
                "Modbus RTU协议" => COMModbus.Instance, // COMModbus实现了IProtocol
                "Modbus TCP协议" => TCPModbus.Instance,//TCPModbus实现了IProtocol
                _ => throw new ArgumentException($"不支持的协议类型：{protocolNum}")
            };
        }



        public void show_stop()
        {
            if (bshow == true)
            {
                int send_num = 0;
                if (sn == runnum)
                {
                    sn = 0;
                }
                send_num = _currentProtocol.MonitorGet(sendbf, sn,1);
                sn = sn + 1;
                // 共用同一个通讯实例发送
                _comm.Send(sendbf, 0, send_num);


                string str = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num, "TX:", 0, true);
                // 线程安全更新UI
                UiDispatcherHelper.ExecuteOnUiThread(() =>
                {
                    SerialTextBlock.Text += str;
                    //ShowTextBox.ScrollToEnd();
                });

            }
        }


        // 核心：业务处理方法（适配CommonRes的委托）
        public void HandleSerialData(byte[] gbuffer, int gb_last, int buffer_len)
        {           

            byte[] buffer = new byte[200];        
            int n_dsp = 0;
            int check_result = 0;

            string str = "";
            str = SerialDataProcessor.Instance.FormatSerialDataToHexString(gbuffer, buffer_len, "RX:", gb_last, true);
            // 线程安全更新UI
            UiDispatcherHelper.ExecuteOnUiThread(() =>
            {
                SerialTextBlock.Text += str;
                //ShowTextBox.ScrollToEnd();
            });

            Array.Copy(gbuffer, gb_last, buffer, 0, buffer_len);
            check_result = _currentProtocol.MonitorCheck(buffer, buffer_len);


            // 校验成功=1的处理
            if (check_result == 1)
            {
                DataR datax= new DataR();
                datax = _currentProtocol.MonitorSolve(buffer, sn - 1);
                if (datax.RWX == 1)
                {                   
                    // 抽取UI更新逻辑，减少冗余注释（注释可统一放方法/常量处）
                    UiDispatcherHelper.ExecuteOnUiThread(() =>
                    {
                        Dtrun.Rows[datax.SN][5] = datax.VALUE;
                    });                   
                }
            }
            else
            {
                UiDispatcherHelper.ExecuteOnUiThread(() =>
                {
                    SerialTextBlock.Text += "RX:Wrong";
                });
            }

        }




        public void runstop_cotnrol(int addr, bool pbrun)
        {

            int send_num = 0;
            brun = pbrun;
            //textBox1.Text = "系统停止运行";

            if (_currentProtocol == null)
                throw new InvalidOperationException("请先选择协议！");

            send_num = _currentProtocol.MonitorRun(sendbf, brun, addr);
            sn=addr;

            // 共用同一个通讯实例发送
            _comm.Send(sendbf, 0, send_num);
            string txt = SerialDataProcessor.Instance.FormatSerialDataToHexString(sendbf, send_num, "TX:", true);

            
            // 线程安全更新UI
            UiDispatcherHelper.ExecuteOnUiThread(() =>
            {
                SerialTextBlock.Text += txt;
                //txtSerialLog.ScrollToEnd();
            });
        }

        public void mbutton_set(string table, int x)
        {
            string y;
            int z;
            int tempsn;
            string val;
            float value;
            int send_num = 0;

            if (table == "PARAMETER_FACTOR")
            {
                y = Dtfactor.Rows[0][0].ToString();
                z = Convert.ToInt32(y);
                tempsn = x + z;


                val = Dtfactor.Rows[x][2].ToString();
                value = Convert.ToSingle(val);
            }
            else
            {
                y = Dtset.Rows[0][0].ToString();
                z = Convert.ToInt32(y);
                tempsn = x + z;


                val = Dtset.Rows[x][5].ToString();
                value = Convert.ToSingle(val);
            }



            _dbOperation.DataBase_SET_Save(table, value, (byte)tempsn);

            if (_comm.IsConnected)
            {

                if (_currentProtocol == null)
                    throw new InvalidOperationException("请先选择协议！");

                send_num = _currentProtocol.MonitorSet(sendbf, tempsn,value);

                _comm.Send(sendbf, 0, send_num);
            }
            else
            {
                MessageBox.Show("请打开串口！");
            }
        }




        private void InitTimer()
        {
            if (mDataTimer == null)
            {
                mDataTimer = new DispatcherTimer();
                mDataTimer.Tick += new EventHandler(DataTimer_Tick);
                mDataTimer.Interval = TimeSpan.FromSeconds(10);
            }
        }
        private void DataTimer_Tick(object sender, EventArgs e)
        {
            s2 = DateTime.Now;
            s1 = DateTime.Now;
            ++timerExeCount;

            show_stop();
        }

        public void StartTimer()
        {
            if (mDataTimer != null && mDataTimer.IsEnabled == false)
            {
                mDataTimer.Start();
                s1 = DateTime.Now;
            }
        }
        public void StopTimer()
        {
            if (mDataTimer != null && mDataTimer.IsEnabled == true)
            {
                mDataTimer.Stop();
            }
        }


        private void ButtonIncreaseAction(object parameter)
        {
            MessageBox.Show("h!");
            if (_comm.IsConnected)
            {
                bshow = !bshow;
                if (bshow)
                {
                    BshowContent = "停止采集";
                    StartTimer();
                }
                else
                {
                    BshowContent = "开始采集";
                    StopTimer();
                }
            }
            else
            {
                MessageBox.Show("打开串口！");
            }
        }

        #region 1. 绑定属性（手动实现SetProperty）
        // 选中的数据库文件路径
        private string _dbFilePath = string.Empty;
        public string DbFilePath
        {
            get => _dbFilePath;
            set => SetProperty(ref _dbFilePath, value);
        }

        // 读取的三张数据表（用于UI绑定）
        private DataTable _dtrun = new DataTable();
        public DataTable Dtrun
        {
            get => _dtrun;
            set => SetProperty(ref _dtrun, value);
        }

        private DataTable _dtset = new DataTable();
        public DataTable Dtset
        {
            get => _dtset;
            set => SetProperty(ref _dtset, value);
        }

        private DataTable _dtfactor = new DataTable();
        public DataTable Dtfactor
        {
            get => _dtfactor;
            set => SetProperty(ref _dtfactor, value);
        }

        // 接口变量（统一操作数据库）
        private IDatabaseOperation _dbOperation = null!;
        #endregion

        // ViewModels/GeneralDebugWindowViewModel.cs
        #region 2. 命令定义（复用现有RelayCommand<T>）
        // 文件选择命令（无参数，泛型指定为object，参数传null）
        private ICommand? _selectDatabaseFileCommand;
        public ICommand SelectDatabaseFileCommand
        {
            get
            {
                if (_selectDatabaseFileCommand == null)
                {
                    // 泛型指定为object，execute忽略参数，canExecute设为null
                    _selectDatabaseFileCommand = new RelayCommand<object>(
                        parameter => SelectDatabaseFile(), // 无参数逻辑
                        parameter => true // 始终可执行
                    );
                }
                return _selectDatabaseFileCommand;
            }
        }

        // 读取数据表命令（同理）
        private ICommand? _loadDatabaseTablesCommand;
        public ICommand LoadDatabaseTablesCommand
        {
            get
            {
                if (_loadDatabaseTablesCommand == null)
                {
                    _loadDatabaseTablesCommand = new RelayCommand<object>(
                        parameter => LoadDatabaseTables(),
                        parameter => true
                    );
                }
                return _loadDatabaseTablesCommand;
            }
        }
        #endregion

        #region 3. 命令执行逻辑（与原逻辑一致）
        /// <summary>
        /// 选择数据库文件的逻辑
        /// </summary>
        private void SelectDatabaseFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "数据库文件|*.db;*.mdb;*.accdb|SQLite文件(*.db)|*.db|Access文件(*.mdb;*.accdb)|*.mdb;*.accdb",
                Title = "选择数据库文件",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // 赋值给绑定属性（自动触发UI更新）
                DbFilePath = openFileDialog.FileName;
                // 初始化数据库操作实例
                InitDbOperation(DbFilePath);
            }
        }

        /// <summary>
        /// 读取数据表的逻辑
        /// </summary>
        private void LoadDatabaseTables()
        {
            if (string.IsNullOrEmpty(DbFilePath) || _dbOperation == null)
            {
                MessageBox.Show("请先选择有效的数据库文件！");
                return;
            }

            try
            {
                // 先检查表是否存在
                if (!_dbOperation.IsTableExist("PARAMETER_RUN"))
                {
                    MessageBox.Show("表PARAMETER_RUN不存在！");
                    return;
                }

                // 调用接口读取表数据（统一调用，无需区分数据库类型）
                Dtrun = _dbOperation.GetDBTable("PARAMETER_RUN");
                Dtset = _dbOperation.GetDBTable("PARAMETER_SET");
                Dtfactor = _dbOperation.GetDBTable("PARAMETER_FACTOR");
                runnum = Dtrun.Rows.Count;
                MessageBox.Show("数据表读取成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取失败：{ex.Message}");
            }
        }
        #endregion

        #region 4. 私有方法：初始化数据库实例（自动识别类型）
        private void InitDbOperation(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath).ToLower();
            string connStr = string.Empty;

            switch (ext)
            {
                case ".db":
                    connStr = $"Data Source={filePath};Version=3;";
                    _dbOperation = new DB_SQLlite(connStr);
                    break;
                case ".mdb":
                    // 适配.mdb的Jet驱动
                    //connStr = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={filePath};";
                    connStr = $"Driver={{Microsoft Access Driver (*.mdb, *.accdb)}};DBQ={filePath};";
                    _dbOperation = new DB_Access(connStr);
                    break;
                case ".accdb":
                    // 适配.accdb的ACE驱动
                    connStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};";
                    _dbOperation = new DB_Access(connStr);
                    break;
                default:
                    throw new NotSupportedException($"不支持的数据库类型：{ext}");
            }
        }
        #endregion

        #region 保存数据命令（无参数RelayCommand）
        private ICommand? _saveDataToDbCommand;
        public ICommand SaveDataToDbCommand
        {
            get
            {
                if (_saveDataToDbCommand == null)
                {
                    _saveDataToDbCommand = new RelayCommand<object>(
                        parameter => SaveDataToDb(),
                        parameter => true
                    ); 
                }
                return _saveDataToDbCommand;
            }
        }

        /// <summary>
        /// 保存所有表数据到数据库
        /// </summary>
        private void SaveDataToDb()
        {
            // 校验：未选择数据库/无数据时提示
            if (string.IsNullOrEmpty(DbFilePath) || _dbOperation == null)
            {
                MessageBox.Show("请先选择有效的数据库文件！");
                return;
            }

            try
            {
                // 保存三张表（按顺序）
                if (Dtrun != null && Dtrun.Rows.Count > 0)
                {
                    _dbOperation.UpdateDBTable(Dtrun, "PARAMETER_RUN");
                }
                if (Dtset != null && Dtset.Rows.Count > 0)
                {
                    _dbOperation.UpdateDBTable(Dtset, "PARAMETER_SET");
                }
                if (Dtfactor != null && Dtfactor.Rows.Count > 0)
                {
                    _dbOperation.UpdateDBTable(Dtfactor, "PARAMETER_FACTOR");
                }

                MessageBox.Show("所有数据已成功保存到数据库！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}");
            }
        }
        #endregion



        public void Page_LoadedD(object sender, RoutedEventArgs e)
        {
            /*
            // 仅对串口类型绑定数据接收事件（预留以太网扩展）
            if (_comm is SerialCommunication serialComm)
            {
                // 绑定串口数据接收事件（替代 CommonRes.CurrentDataHandler = HandleSerialData）
                serialComm.DataReceived += HandleSerialDataWrapper;
            }
            */
            _comm.DataReceived += HandleSerialDataWrapper;
        }


        public void Page_UnloadedD(object sender, RoutedEventArgs e)
        {
            /*
            // 仅对串口类型解绑数据接收事件
            if (_comm is SerialCommunication serialComm)
            {
                // 解绑事件（替代 CommonRes.CurrentDataHandler 清空）
                serialComm.DataReceived -= HandleSerialDataWrapper;
            }
            */
            _comm.DataReceived -= HandleSerialDataWrapper;
            // 释放ViewModel资源（原有逻辑保留）
            Dispose();

        }

        // 事件包装器（适配 DataReceivedEventArgs 到原有 HandleSerialData 参数）
        // 核心：无需修改原有 HandleSerialData 逻辑，仅做参数映射
        private void HandleSerialDataWrapper(object sender, DataReceivedEventArgs e)
        {
            // 直接调用原有 HandleSerialData 方法（参数与原 CommonRes 委托一致）
            HandleSerialData(e.Buffer, e.LastIndex, e.BufferLength);
        }

        public void Dispose()
        {
            // 若有其他资源需释放，在此处理
        }


    }
}
