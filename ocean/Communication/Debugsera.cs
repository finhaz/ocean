﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO.Ports;
using System.Windows.Threading;


namespace ocean.Communication
{
    public class Debugsera : Page
    {
        //控件绑定相关
        public TextBox tbReceive { get; set; }
        public TextBox txtRecive { get; set; }

        public bool ckHexState;


        delegate void HanderInterfaceUpdataDelegate(string mySendData);
        HanderInterfaceUpdataDelegate myUpdataHander;
        delegate void txtGotoEndDelegate();


        public Debugsera()
        {
            tbReceive=new TextBox();
            txtRecive=new TextBox();
            txtRecive.Text = "0";

        }


        public string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();
        }

        public byte[] HexStringToByteArray(string s)
        {
            //s=s.ToUpper();
            s = s.Replace(" ", "");
            if (s.Length % 2 != 0)
            {
                s = s.Substring(0, s.Length - 1) + "0" + s.Substring(s.Length - 1);
            }
            byte[] buffer = new byte[s.Length / 2];


            try
            {
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                return buffer;
            }
            catch
            {
                string errorString = "E4";
                byte[] errorData = new byte[errorString.Length / 2];
                errorData[0] = (byte)Convert.ToByte(errorString, 16);
                return errorData;
            }
        }

        public string StringToHexString(string s)
        {
            //s = s.ToUpper();
            s = s.Replace(" ", "");

            string buffer = "";
            char[] myChar;
            myChar = s.ToCharArray();
            for (int i = 0; i < s.Length; i++)
            {
                //buffer = buffer + Convert.ToInt32(myChar[i]);
                buffer = buffer + Convert.ToString(myChar[i], 16);
                buffer = buffer.ToUpper();
            }
            return buffer;
        }


        private void getControlState()
        {

        }
        private void getData(string sendData)
        {
            tbReceive.Text += sendData;
        }

        private void txtGotoEnd()
        {
            tbReceive.ScrollToEnd();
        }

        private void txtReciveEvent(string byteNum)
        {
            txtRecive.Text = Convert.ToString(Convert.ToInt32(txtRecive.Text) + Convert.ToInt32(byteNum));
        }



        public void mySerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

            int n = CommonRes.mySerialPort.BytesToRead;
            byte[] buf = new byte[n];
            CommonRes.mySerialPort.Read(buf, 0, n);
            myUpdataHander = new HanderInterfaceUpdataDelegate(getData);
            txtGotoEndDelegate myGotoend = txtGotoEnd;
            HanderInterfaceUpdataDelegate myUpdata1 = new HanderInterfaceUpdataDelegate(txtReciveEvent);
            string abc, abc1;
            if (ckHexState == true)
            {
                abc = ByteArrayToHexString(buf);
                string hexStringView = "";
                for (int i = 0; i < abc.Length; i += 2)
                {
                    hexStringView += abc.Substring(i, 2) + " ";
                }
                abc = hexStringView;
                abc1 = abc.Replace(" ", "");
                if (abc1.Substring(abc1.Length - 2, 2) == "0D")
                {
                    abc = abc + "\n";
                }

            }
            else
            {
                abc = System.Text.Encoding.Default.GetString(buf);
            }
            Dispatcher.Invoke(myUpdataHander, new string[] { abc });
            Dispatcher.Invoke(myGotoend);
            Dispatcher.Invoke(myUpdata1, new string[] { n.ToString() });
        }



    }
}
