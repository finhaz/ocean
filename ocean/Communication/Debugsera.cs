using System;
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





        public TextBox tbSend { get; set; }


        public TextBlock tbkIntervalTime { get; set; }
        //public TextBox tbIntervalTime { get; set; }

        public Border bdExpend { get; set; }




        public Grid grdSend { get; set; }

        public bool ckHexState;

        

        //SerialPort mySerialPort = new SerialPort();
        //public DispatcherTimer time1 = new DispatcherTimer();


        delegate void HanderInterfaceUpdataDelegate(string mySendData);
        HanderInterfaceUpdataDelegate myUpdataHander;
        delegate void txtGotoEndDelegate();


        public Debugsera()
        {
            tbReceive=new TextBox();
            txtRecive=new TextBox();
            txtSend=new TextBox();
            tbComState=new TextBox();



            tbSend =new TextBox();


            tbkIntervalTime =new TextBlock();
            //tbIntervalTime=new TextBox();
            bdExpend=new Border();



            grdSend=new Grid();

            txtRecive.Text = "0";
            txtSend.Text = "0";
            tbComState.Text = "0";



        }




    }
}
