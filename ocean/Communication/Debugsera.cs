using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ocean.Communication
{
    public class Debugsera
    {
        //控件绑定相关
        public TextBox tbReceive { get; set; }




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
    }
}
