using ocean.Communication;
using ocean.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ocean.Views
{
    public partial class MainPage : Page
    {
        private ShellViewModel _shellViewModel;


        public MainPage()
        {
            InitializeComponent();
        }

        private void CmbCommunicationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (CmbCommunicationType.SelectedValue)
            {
                case CommunicationType.SerialPort:
                    // 选择串口：创建串口实例（仅创建，不打开/不绑定事件）
                    CommunicationManager.Instance.CreateSerialInstance();
                    break;
                case CommunicationType.Ethernet:
                    // 选择以太网：暂不处理（预留）
                    break;
            }
        }
        
    }
}
