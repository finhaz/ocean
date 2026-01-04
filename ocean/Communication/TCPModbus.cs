using ocean.database;
using ocean.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocean.Communication
{
    /// <summary>
    /// Modbus TCP实现类（完全仿照COMModbus的函数名/结构）
    /// </summary>
    public class TCPModbus : IProtocol
    {
        byte[] revbuffer = new byte[256];

        // 单例模式（和COMModbus完全一致）
        private TCPModbus() { }
        private static readonly Lazy<TCPModbus> _instance = new Lazy<TCPModbus>(() => new TCPModbus());
        public static TCPModbus Instance => _instance.Value;

        /// <summary>
        /// 构建Modbus TCP 03功能码（读取保持寄存器）请求报文
        /// </summary>
        /// <param name="sendbf">发送缓冲区</param>
        /// <param name="sn">寄存器起始地址</param>
        /// <param name="num">读取寄存器数量</param>
        public void Monitor_Get_03(byte[] sendbf, int sn, int num)
        {
            Array.Clear(sendbf, 0, sendbf.Length);

            // --------------------- MBAP头（6字节）---------------------
            sendbf[0] = 0x00;        // Transaction ID高字节（固定0x0001，自定义）
            sendbf[1] = 0x01;        // Transaction ID低字节
            sendbf[2] = 0x00;        // Protocol ID高字节（固定0x0000）
            sendbf[3] = 0x00;        // Protocol ID低字节
            sendbf[4] = 0x00;        // Length高字节（后续字节数：UnitID(1)+功能码数据(5)=6 → 0x0006）
            sendbf[5] = 0x06;        // Length低字节
            sendbf[6] = 0x01;        // Unit ID（从站地址，对应RTU的slaveId）

            // --------------------- 03功能码数据（5字节）---------------------
            sendbf[7] = 0x03;        // 功能码03
                                     // 寄存器地址（大端序）
            byte[] temp_i = BitConverter.GetBytes(sn);
            sendbf[8] = temp_i[1];   // 高字节
            sendbf[9] = temp_i[0];   // 低字节
                                     // 读取寄存器数量（大端序）
            temp_i = BitConverter.GetBytes(num);
            sendbf[10] = temp_i[1];  // 高字节
            sendbf[11] = temp_i[0];  // 低字节

            // TCP无CRC，无需计算CRC
        }

        /// <summary>
        /// 构建Modbus TCP 06功能码（写入单个寄存器）请求报文
        /// </summary>
        /// <param name="sendbf">发送缓冲区</param>
        /// <param name="sn">寄存器地址</param>
        /// <param name="send_value">写入值（浮点转短整型）</param>
        public void Monitor_Set_06(byte[] sendbf, int sn, float send_value)
        {
            Array.Clear(sendbf, 0, sendbf.Length);
            Int16 svalue = (short)send_value;

            // --------------------- MBAP头（6字节）---------------------
            sendbf[0] = 0x00;        // Transaction ID高字节
            sendbf[1] = 0x01;        // Transaction ID低字节
            sendbf[2] = 0x00;        // Protocol ID高字节
            sendbf[3] = 0x00;        // Protocol ID低字节
            sendbf[4] = 0x00;        // Length高字节（UnitID(1)+功能码数据(5)=6 → 0x0006）
            sendbf[5] = 0x06;        // Length低字节
            sendbf[6] = 0x01;        // Unit ID

            // --------------------- 06功能码数据（5字节）---------------------
            sendbf[7] = 0x06;        // 功能码06
                                     // 寄存器地址（大端序）
            byte[] temp_i = BitConverter.GetBytes(sn);
            sendbf[8] = temp_i[1];   // 高字节
            sendbf[9] = temp_i[0];   // 低字节
                                     // 写入值（大端序）
            temp_i = BitConverter.GetBytes(svalue);
            sendbf[10] = temp_i[1];  // 高字节
            sendbf[11] = temp_i[0];  // 低字节

            // TCP无CRC，无需计算CRC
        }

        /// <summary>
        /// 构建Modbus TCP 06功能码（运行/停止控制）请求报文
        /// </summary>
        /// <param name="sendbf">发送缓冲区</param>
        /// <param name="machine">从站地址</param>
        /// <param name="adr">寄存器地址</param>
        /// <param name="brun">运行/停止</param>
        public void Monitor_Run(byte[] sendbf, byte machine, int adr, bool brun)
        {
            Array.Clear(sendbf, 0, sendbf.Length);

            // --------------------- MBAP头（6字节）---------------------
            sendbf[0] = 0x00;        // Transaction ID高字节
            sendbf[1] = 0x01;        // Transaction ID低字节
            sendbf[2] = 0x00;        // Protocol ID高字节
            sendbf[3] = 0x00;        // Protocol ID低字节
            sendbf[4] = 0x00;        // Length高字节（UnitID(1)+功能码数据(5)=6 → 0x0006）
            sendbf[5] = 0x06;        // Length低字节
            sendbf[6] = machine;     // Unit ID（传入的从站地址）

            // --------------------- 06功能码数据（5字节）---------------------
            sendbf[7] = 0x06;        // 功能码06
                                     // 寄存器地址（大端序）
            byte[] temp_i = BitConverter.GetBytes(adr);
            sendbf[8] = temp_i[1];   // 高字节
            sendbf[9] = temp_i[0];   // 低字节
                                     // 控制值（0xaa/0x55，大端序）
            sendbf[10] = 0x00;       // 高字节固定0x00
            sendbf[11] = brun ? (byte)0xaa : (byte)0x55; // 低字节

            // TCP无CRC，无需计算CRC
        }

        /// <summary>
        /// Modbus TCP报文校验（替代RTU的CRC校验）
        /// 校验逻辑：MBAP头合法 + 长度匹配 + 功能码合法
        /// </summary>
        /// <param name="buffer">接收缓冲区</param>
        /// <param name="len">接收长度</param>
        /// <returns>0x01=校验通过，0x03=校验失败</returns>
        public int Monitor_check(byte[] buffer, int len)
        {
            // 1. 最小长度校验：MBAP头(6) + 功能码(1) + 至少1字节数据 = 8字节
            if (len < 8) return 0x03;

            // 2. 校验Protocol ID（必须为0x0000）
            if (buffer[2] != 0x00 || buffer[3] != 0x00) return 0x03;

            // 3. 校验Length字段（Length = 总长度 - 6，因为Length是MBAP头后的数据长度）
            int lengthField = (buffer[4] << 8) | buffer[5];
            if (lengthField != len - 6) return 0x03;

            // 4. 校验功能码（仅支持03/06，可扩展）
            byte funcCode = buffer[7];
            if (funcCode != 0x03 && funcCode != 0x06) return 0x03;

            // 所有校验通过
            return 0x01;
        }

        /// <summary>
        /// Modbus TCP响应报文解析（跳过MBAP头，解析功能码和数据）
        /// </summary>
        /// <param name="buffer">接收缓冲区</param>
        /// <param name="Readpos">寄存器起始地址</param>
        /// <returns>解析后的数据</returns>
        public DataR Monitor_Solve(byte[] buffer, int Readpos)
        {
            DataR data = new DataR();
            data.COMMAND = buffer[7];  // 跳过MBAP头(6字节)，功能码在第7位（索引7）
            data.SN = Readpos;

            // 解析03功能码响应（读取保持寄存器）
            if (data.COMMAND == 0x03)
            {
                // 03响应结构：MBAP(6) + 功能码(1) + 字节数(1) + 寄存器数据(N*2)
                byte byteCount = buffer[8];  // 数据字节数
                byte[] typeBytes = new byte[byteCount];
                Array.Copy(buffer, 9, typeBytes, 0, byteCount); // 从第9位开始拷贝数据
                Array.Reverse(typeBytes);                       // 大端序转小端序
                data.VALUE = BitConverter.ToInt16(typeBytes, 0);
            }
            // 06功能码响应无需解析数据（仅确认写入成功）
            else if (data.COMMAND == 0x06)
            {
                // 可扩展：解析06响应的寄存器地址和写入值
            }

            return data;
        }

        // --------------------- 以下为IProtocol接口实现（和COMModbus完全一致）---------------------
        public int MonitorRun(byte[] sendbf, bool brun, int addr = 0)
        {
            this.Monitor_Run(sendbf, 1, addr, brun);
            return 12; // TCP 06功能码报文长度为12字节（MBAP6 + 数据6）
        }

        public int MonitorSet(byte[] sendbf, int tempsn, object value = null)
        {
            this.Monitor_Set_06(sendbf, tempsn, (float)value);
            return 12; // TCP 06功能码报文长度为12字节
        }

        public int MonitorGet(byte[] sendbf, int tempsn, object num = null)
        {
            this.Monitor_Get_03(sendbf, tempsn, 1);
            return 12; // TCP 03功能码报文长度为12字节
        }

        public int MonitorCheck(byte[] buffer, object len = null)
        {
            return this.Monitor_check(buffer, (int)len);
        }

        public DataR MonitorSolve(byte[] buffer, object Readpos = null)
        {
            return this.Monitor_Solve(buffer, (int)Readpos);
        }

        // 原RTU的CRC方法保留（空实现，避免调用报错）
        public UInt16 crc16_ccitt(byte[] data, int len, UInt16 StartIndex)
        {
            return 0; // TCP无需CRC，返回0即可
        }
    }
}
