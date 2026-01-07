using ocean.database;
using ocean.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//RTU格式
//地址 功能码	数据	CRC校验
//1 byte	1 byte	N bytes	2 bytes

namespace ocean
{
    //计划设计modbus协议版本
    public class COMModbus:IProtocol
    {
        //public byte[] sendbf = new byte[128];
        byte[] revbuffer = new byte[256];

        // 单例模式（保持不变）
        private COMModbus() { }
        private static readonly Lazy<COMModbus> _instance = new Lazy<COMModbus>(() => new COMModbus());
        public static COMModbus Instance => _instance.Value;

        public UInt16 CRC16ccitt(byte[] data, int len,UInt16 StartIndex)
        {
            UInt16 ccitt16 = 0xA001;
            UInt16 crc = 0xFFFF;

            for (int j= StartIndex; j<len ; j++)
            {
                crc ^= data[j];
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001)==1)
                    {
                        crc >>= 1;
                        crc ^= ccitt16;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }


        // 新增：实现IProtocol接口的MonitorRun（适配统一调用）
        public int MonitorRun(byte[] sendbf, bool brun, int addr = 0)
        {
            float order = 0;
            if (brun)
                order = 0xaa;
            else
                order = 0x55;

            MonitorSet(sendbf, addr, order);
            // 返回Modbus固定发送长度
            return 8;
        }

        // 新增：实现IProtocol接口的MonitorSet（适配统一调用）
        public int MonitorSet(byte[] sendbf, int tempsn, object value = null,object regtype = null)
        {
            byte functionCode = 0;
            switch (regtype)
            {
                case "线圈状态(RW)":
                    functionCode = 0x05;
                    break;
                case "离散输入(RO)":
                    functionCode = 0x02;
                    break;
                case "保持寄存器(RW)":
                    functionCode = 0x06;
                    break;
                case "输入寄存器(RO)":
                    functionCode = 0x04;
                    break;
                default:
                    functionCode = 0x06;
                    break;
            }

            // 校验功能码合法性
            if (functionCode != 0x05 && functionCode != 0x06)
            {
                throw new ArgumentException("仅支持05(强制单线圈)和06(预置单寄存器)功能码");
            }

            int crc = 0;
            float send_value=(float)value;
            Int16 svalue = (short)send_value; // 转换为16位短整型
            Array.Clear(sendbf, 0, sendbf.Length);

            // 1. 从站地址
            sendbf[0] = 0x01;
            // 2. 功能码（动态传入）
            sendbf[1] = functionCode;

            // 3. 操作地址（线圈/寄存器地址，高位在前）
            byte[] temp_i = BitConverter.GetBytes(tempsn);
            sendbf[2] = temp_i[1]; // 地址高位
            sendbf[3] = temp_i[0]; // 地址低位

            // 4. 写入数值（适配05/06的差异）
            
            if (functionCode == 0x05)
            {
                // 05功能码：线圈值固定为0xFF00(置1)或0x0000(置0)
                //svalue = svalue != 0 ? unchecked((short)0xFF00) : (short)0x0000;
                svalue = svalue != 0 ? (short)-256 : (short)0x0000;
            }
            
            temp_i = BitConverter.GetBytes(svalue);
            sendbf[4] = temp_i[1]; // 数值高位
            sendbf[5] = temp_i[0]; // 数值低位

            // 5. 计算CRC16校验（前6字节）
            crc = CRC16ccitt(sendbf, 6, 0);

            // 6. 填充CRC校验值（低位在前）
            temp_i = BitConverter.GetBytes(crc);
            sendbf[6] = temp_i[0]; // CRC低位
            sendbf[7] = temp_i[1]; // CRC高位



            // 返回Modbus固定发送长度
            return 8;
        }

        public int MonitorGet(byte[] sendbf, int tempsn, object num=null, object regtype = null)
        {
            byte functionCode=0;
            switch (regtype)
            {
                case "线圈状态(RW)":
                    functionCode = 0x01;
                    break;
                case "离散输入(RO)":
                    functionCode = 0x02;
                    break;
                case "保持寄存器(RW)":
                    functionCode = 0x03;
                    break;
                case "输入寄存器(RO)":
                    functionCode = 0x04;
                    break;
                default:
                    functionCode = 0x03;
                    break;
            }

            // 校验功能码合法性（支持01/02/03/04）
            if (functionCode != 0x01 && functionCode != 0x02 &&
                functionCode != 0x03 && functionCode != 0x04)
            {
                throw new ArgumentException("仅支持01(线圈)/02(离散输入)/03(保持寄存器)/04(输入寄存器)功能码");
            }

            int crc;
            // 清空发送缓冲区
            Array.Clear(sendbf, 0, sendbf.Length);

            // 1. 从站地址（默认0x01，可根据实际设备调整）
            sendbf[0] = 0x01;
            // 2. 功能码（通过形参传入，适配01/03）
            sendbf[1] = functionCode;

            // 3. 起始地址（2字节，高位在前）
            byte[] temp_i = BitConverter.GetBytes(tempsn);
            sendbf[2] = temp_i[1]; // 地址高位
            sendbf[3] = temp_i[0]; // 地址低位

            // 4. 读取数量（2字节，高位在前）
            temp_i = BitConverter.GetBytes((int)num);
            sendbf[4] = temp_i[1]; // 数量高位
            sendbf[5] = temp_i[0]; // 数量低位

            // 5. 计算CRC16校验（前6字节）
            crc = CRC16ccitt(sendbf, 6, 0);

            // 6. 填充CRC校验值（低位在前，高位在后）
            temp_i = BitConverter.GetBytes(crc);
            sendbf[6] = temp_i[0]; // CRC低位
            sendbf[7] = temp_i[1]; // CRC高位


            return 8;
        }


        public int MonitorCheck(byte[] buffer, object len = null)
        {

            int crc;
            int crc_g;
            int index = (int)len - 2;
            crc = CRC16ccitt(buffer, ((int)len - 2), 0);
            if (index > 2)
            {
                crc_g = BitConverter.ToUInt16(buffer, index);
                if (crc_g == crc)
                    return 0X01;
            }
            return 0X03;
        }

        public DataR MonitorSolve(byte[] buffer, object Readpos = null)
        {
            DataR data = new DataR();
            data.COMMAND = buffer[1];
            data.SN = (int)Readpos;
            byte[] typeBytes = new byte[buffer[2]];
            switch (data.COMMAND)
            {
                case 0x01:
                    data.VALUE = buffer[3];
                    data.RWX = 1;
                    break;
                case 0x02:
                    data.VALUE = buffer[3];
                    data.RWX = 1;
                    break;
                case 0x03:
                    Array.Copy(buffer, 3, typeBytes, 0, buffer[2]);
                    Array.Reverse(typeBytes);
                    data.VALUE = BitConverter.ToInt16(typeBytes, 0);
                    data.RWX = 1;
                    break;
                case 0x04:
                    Array.Copy(buffer, 3, typeBytes, 0, buffer[2]);
                    Array.Reverse(typeBytes);
                    data.VALUE = BitConverter.ToInt16(typeBytes, 0);
                    data.RWX = 1;
                    break;
                default:
                    data.SN = buffer[2] << 8 + buffer[3];
                    data.RWX = 0;
                    break;
            }

            return data;
        }
    }
}
