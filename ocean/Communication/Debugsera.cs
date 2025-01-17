﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO.Ports;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Shapes;


namespace ocean.Communication
{
    public class Debugsera : Page
    {
        //控件绑定相关
        public TextBox tbReceive { get; set; }
        public TextBox txtRecive { get; set; }

        

        public TextBox txtSend { get; set; }

        public TextBox tbComState { get; set; }

        public CheckBox ckAdvantechCmd { get; set; }

        public ComboBox cbBaudRate { get; set; }
        public ComboBox cbDataBits { get; set; }

        public ComboBox cbParity { get; set; }
        public ComboBox cbPortName{ get; set; }
        public ComboBox cbStopBits{ get; set; }
        
        public CheckBox ck16Send { get; set; }
        public CheckBox ck16View { get; set; }

        public TextBox tbSend { get; set; }


        public TextBlock tbkIntervalTime { get; set; }
        public TextBox tbIntervalTime { get; set; }

        public Border bdExpend { get; set; }

        public Button btOpenCom { get; set; }

        public Ellipse comState { get; set; }

        public CheckBox ckAutoSend { get; set; }


        public Grid grdSend { get; set; }

        public bool ckHexState;

        

        //SerialPort mySerialPort = new SerialPort();
        public DispatcherTimer time1 = new DispatcherTimer();


        delegate void HanderInterfaceUpdataDelegate(string mySendData);
        HanderInterfaceUpdataDelegate myUpdataHander;
        delegate void txtGotoEndDelegate();


        public Debugsera()
        {
            tbReceive=new TextBox();
            txtRecive=new TextBox();
            txtSend=new TextBox();
            tbComState=new TextBox();
            ckAdvantechCmd=new CheckBox();

            cbBaudRate=new ComboBox();
            cbDataBits=new ComboBox();
            cbParity=new ComboBox();
            cbPortName=new ComboBox();
            cbStopBits=new ComboBox();

            ck16Send =new CheckBox();
            ck16View=new CheckBox();
            tbSend =new TextBox();


            tbkIntervalTime =new TextBlock();
            tbIntervalTime=new TextBox();
            bdExpend=new Border();

            btOpenCom=new Button();

            comState=new Ellipse();

            ckAutoSend=new CheckBox();
            grdSend=new Grid();


            txtRecive.Text = "0";
            txtSend.Text = "0";
            tbComState.Text = "0";
            tbIntervalTime.Text= "0";
            btOpenCom.Content = "打开串口";
            //comState.Style = (Style)FindResource("EllipseStyleRed");


            time1.Tick += new EventHandler(time1_Tick);

            tbIntervalTime.TextChanged += new System.Windows.Controls.TextChangedEventHandler(tbIntervalTime_TextChanged);


            //mySerialPort.Encoding = System.Text.Encoding.GetEncoding("UTF8");
            ckHexState = (bool)ck16View.IsChecked;


            CommonRes.mySerialPort.DataReceived += new SerialDataReceivedEventHandler(mySerialPort_DataReceived);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            CommonRes.mySerialPort.Encoding = System.Text.Encoding.GetEncoding("GB2312");


            some_intial();

            tbkIntervalTime.Visibility = Visibility.Hidden;
            tbIntervalTime.Visibility = Visibility.Hidden;
            bdExpend.Visibility = Visibility.Hidden;

            if (CommonRes.mySerialPort.IsOpen)
            {
                cbBaudRate.IsEnabled = false;
                cbDataBits.IsEnabled = false;
                cbParity.IsEnabled = false;
                cbPortName.IsEnabled = false;
                cbStopBits.IsEnabled = false;

                btOpenCom.Content = "关闭串口";
                comState.Style = (Style)FindResource("EllipseStyleGreen");
            }

        }


        public void some_intial()
        {
            #region//串口设置初始化
            //串口cbComName.Text
            try
            {
                string[] portsName = SerialPort.GetPortNames();
                Array.Sort(portsName);
                cbPortName.ItemsSource = portsName;
                cbPortName.Text = Convert.ToString(cbPortName.Items[0]);
            }
            catch
            {
                cbPortName.Text = "暂无串口";
            }

            //波特率cbBaudRate.Text
            int[] baudRateData = { 4800, 9600, 19200, 38400, 43000, 56000 };
            cbBaudRate.ItemsSource = baudRateData;
            cbBaudRate.Text = Convert.ToString(cbBaudRate.Items[1]);
            //检验位cbParity.Text
            string[] parityBit = { "无", "奇校验", "偶校验" };
            cbParity.ItemsSource = parityBit;
            cbParity.Text = Convert.ToString(cbParity.Items[0]);
            //数据位cbDataBits.Text
            int[] dataBits = { 6, 7, 8 };
            cbDataBits.ItemsSource = dataBits;
            cbDataBits.Text = Convert.ToString(cbDataBits.Items[2]);
            //停止位cbStopBits.Text
            int[] stopBits = { 1, 2 };
            cbStopBits.ItemsSource = stopBits;
            cbStopBits.Text = Convert.ToString(cbStopBits.Items[0]);
            #endregion
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



        public void btSend_Event(string strSend, bool hexState)
        {
            if (CommonRes.mySerialPort.IsOpen)
            {
                if (hexState == false)
                {
                    //if (ckAdvantechCmd.IsChecked == true) { strSend = strSend.ToUpper(); }
                    byte[] sendData = System.Text.Encoding.Default.GetBytes(strSend);
                    CommonRes.mySerialPort.Write(sendData, 0, sendData.Length);
                    txtSend.Text = Convert.ToString(Convert.ToInt32(txtSend.Text) + Convert.ToInt32(sendData.Length));
                    if (ckAdvantechCmd.IsChecked == true)
                    {
                        byte[] sendAdvCmd = HexStringToByteArray("0D");
                        CommonRes.mySerialPort.Write(sendAdvCmd, 0, 1);
                        txtSend.Text = Convert.ToString(Convert.ToInt32(txtSend.Text) + Convert.ToInt32(sendData.Length));
                    }
                }
                else
                {
                    byte[] sendHexData = HexStringToByteArray(strSend);
                    CommonRes.mySerialPort.Write(sendHexData, 0, sendHexData.Length);
                }
            }
            else
            {
                tbComState.Text = "串口未开";
                MessageBox.Show("串口没有打开，请检查！");
            }
        }

        private void time1_Tick(object sender, EventArgs e)
        {
            if (CommonRes.mySerialPort.IsOpen)
            {
                btSend_Event(tbSend.Text, (bool)ck16Send.IsChecked);
            }
        }


        private void tbIntervalTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ckAutoSend.IsChecked == true)
            {
                if (Convert.ToDouble(tbIntervalTime.Text) == 0)
                {
                    time1.Stop();
                }
                else
                {
                    time1.Interval = TimeSpan.FromSeconds(Convert.ToDouble(tbIntervalTime.Text));
                    time1.Start();
                }
            }
        }







    }
}
