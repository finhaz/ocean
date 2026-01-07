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

        public void Monitor_Get_03(byte[] sendbf,int sn,int num)
        {
            int crc;
            Array.Clear(sendbf, 0, sendbf.Length);
            sendbf[0] = 0x01;
            sendbf[1] = 0x03;
            //寄存器地址
            byte[] temp_i = BitConverter.GetBytes(sn);            
            sendbf[2] = temp_i[1];
            sendbf[3] = temp_i[0];
            //读取数据个数
            temp_i = BitConverter.GetBytes(num);
            sendbf[4] = temp_i[1]; 
            sendbf[5] = temp_i[0];

            crc = crc16_ccitt(sendbf, 6,0);

            temp_i = BitConverter.GetBytes(crc);
            sendbf[6] = temp_i[0];
            sendbf[7] = temp_i[1];
        }

        public void Monitor_Set_06(byte[] sendbf, int sn, float send_value)
        {
            int crc = 0;
            Int16 svalue = (short)send_value;
            Array.Clear(sendbf, 0, sendbf.Length);
            sendbf[0] = 0x01;
            sendbf[1] = 0x06;
            //寄存器地址
            byte[] temp_i = BitConverter.GetBytes(sn);
            sendbf[2] = temp_i[1];
            sendbf[3] = temp_i[0];

            temp_i = BitConverter.GetBytes(svalue);
            sendbf[4] = temp_i[1];
            sendbf[5] = temp_i[0];
            crc = crc16_ccitt(sendbf, 6,0);

            temp_i = BitConverter.GetBytes(crc);
            sendbf[6] = temp_i[0];
            sendbf[7] = temp_i[1];
        }

        public void Monitor_Run(byte[] sendbf,byte machine,int adr, bool brun)
        {
            int crc;
            Array.Clear(sendbf, 0, sendbf.Length);
            sendbf[0] = machine;
            sendbf[1] = 0x06;
            byte[] temp_i = BitConverter.GetBytes(adr);

            sendbf[2] = temp_i[1];
            sendbf[3] = temp_i[0];

            sendbf[4] = 0x00;
            if (brun)
                sendbf[5] = 0xaa;
            else
                sendbf[5] = 0x55;

            crc = crc16_ccitt(sendbf,6,0);

            temp_i=BitConverter.GetBytes(crc);
            sendbf[6] = temp_i[0];
            sendbf[7] = temp_i[1];
        }


        public int Monitor_check(byte[] buffer,int len)
        {
            int crc;
            int crc_g;
            int index = len - 2;
            crc = crc16_ccitt(buffer, (len-2), 0);
            if (index > 2)
            {
                crc_g = BitConverter.ToUInt16(buffer, index);
                if (crc_g == crc)
                    return 0X01;               
            }
            return 0X03;     
        }



        public DataR Monitor_Solve(byte[] buffer, int Readpos)
        {
            DataR data=new DataR();
            data.COMMAND = buffer[1];
            data.SN = Readpos;

            if (data.COMMAND == 3)
            {
                byte[] typeBytes = new byte[buffer[2]];
                Array.Copy(buffer, 3, typeBytes, 0, buffer[2]);
                Array.Reverse(typeBytes);
                data.VALUE = BitConverter.ToInt16(typeBytes, 0);
            }
            else if (data.COMMAND == 6)
            {
                data.SN = buffer[2]<<8+buffer[3];
            }

            return data;
        }


        public UInt16 crc16_ccitt(byte[] data, int len,UInt16 StartIndex)
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
            // 直接调用原有方法（param1固定为1）
            this.Monitor_Run(sendbf, 1, addr, brun);
            // 返回Modbus固定发送长度
            return 8;
        }

        // 新增：实现IProtocol接口的MonitorSet（适配统一调用）
        public int MonitorSet(byte[] sendbf, int tempsn, object value = null)
        {
            // 直接调用原有方法
            this.Monitor_Set_06(sendbf, tempsn, (float)value);
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
                case "保持寄存器(RW)":
                    //this.Monitor_Get_03(sendbf, tempsn, 1);
                    functionCode = 0x03;
                    break;
            }

            // 校验功能码合法性（可选，避免传入错误功能码）
            if (functionCode != 0x01 && functionCode != 0x03)
            {
                throw new ArgumentException("仅支持01（读取线圈）和03（读取保持寄存器）功能码");
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
            temp_i = BitConverter.GetBytes((bool)num);
            sendbf[4] = temp_i[1]; // 数量高位
            sendbf[5] = temp_i[0]; // 数量低位

            // 5. 计算CRC16校验（前6字节）
            crc = crc16_ccitt(sendbf, 6, 0);

            // 6. 填充CRC校验值（低位在前，高位在后）
            temp_i = BitConverter.GetBytes(crc);
            sendbf[6] = temp_i[0]; // CRC低位
            sendbf[7] = temp_i[1]; // CRC高位


            return 8;
        }


        public int MonitorCheck(byte[] buffer, object len = null)
        {
            int CheckResult = 0;
            CheckResult = this.Monitor_check(buffer,(int)len);
            return CheckResult;
        }

        public DataR MonitorSolve(byte[] buffer, object Readpos = null)
        {
            DataR data = new DataR();
            data = this.Monitor_Solve(buffer, (int)Readpos);
            return data;
        }
    }
}
