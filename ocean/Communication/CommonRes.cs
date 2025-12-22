using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Data;
using System.Threading;
using System.Windows.Documents;
using SomeNameSpace;
using System.Windows.Threading;
using ocean.UI;
using System.Windows.Controls;
using ocean.Mvvm;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;

namespace ocean
{
    public class CommonRes : ObservableObject
    {
        //静态字段
        public static SerialPort mySerialPort = new SerialPort();

        // 注意：委托参数需包含业务处理所需的所有数据（如读取的字节、gb_last、buffer_len等）
        public delegate void SerialDataReceivedHandler(byte[] gbuffer, int gb_last, int buffer_len);
        //针对数据协议：
        public static byte[] gbuffer = new byte[4096];
        public static int gb_index = 0;//缓冲区注入位置
        public static int get_index = 0;// 缓冲区捕捉位置
        public static int Protocol_num { get; set; }

        // 当前活跃Page的处理方法引用（初始为null）
        public static SerialDataReceivedHandler CurrentDataHandler { get; set; }

        // 静态构造函数：只绑定一次串口的DataReceived事件（核心）
        static CommonRes()
        {
            // 串口核心接收逻辑：只负责读取数据，不处理业务
            mySerialPort.DataReceived += MySerialPort_DataReceived;
        }



        // 串口核心接收方法：读取数据后，分发给当前活跃的Page
        // 串口核心接收方法：统一读取数据，分发到业务处理委托

        private static void MySerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int buffer_len = 0;
            int gb_last = gb_index; // 记录当前的缓冲区位置（原Modbusset的gb_last）

            try
            {
                // 1. 统一读取串口数据（原Modbusset中的读取逻辑移到这里）
                buffer_len = mySerialPort.Read(gbuffer, gb_index, gbuffer.Length - gb_index);
                gb_index += buffer_len;
                if (gb_index >= gbuffer.Length)
                {
                    gb_index -= gbuffer.Length;
                }
            }
            catch
            {
                return;
            }

            // 2. 若有活跃的业务处理委托，调用它（传递所需参数）
            CurrentDataHandler?.Invoke(gbuffer, gb_last, buffer_len);
        }






        public static DataTable dt1 = new DataTable();
        public static DataTable dt2 = new DataTable();
        public static DataTable dt3 = new DataTable();










        public CommonRes()
        {


        }


    }
}
