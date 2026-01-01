using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ocean.ViewModels
{
    // 极简ViewModel
    public class EthernetConfigViewModel : INotifyPropertyChanged
    {
        // 协议选择（TCP/UDP）
        private string _selectedProtocol = "TCP";
        public string SelectedProtocol
        {
            get => _selectedProtocol;
            set { _selectedProtocol = value; OnPropertyChanged(); }
        }

        // 本地IP
        private string _localIp = "192.168.1.100";
        public string LocalIp
        {
            get => _localIp;
            set { _localIp = value; OnPropertyChanged(); }
        }

        // 本地端口
        private int _localPort = 8080;
        public int LocalPort
        {
            get => _localPort;
            set { _localPort = value; OnPropertyChanged(); }
        }

        // 远程IP
        private string _remoteIp = "192.168.1.1";
        public string RemoteIp
        {
            get => _remoteIp;
            set { _remoteIp = value; OnPropertyChanged(); }
        }

        // 远程端口
        private int _remotePort = 8080;
        public int RemotePort
        {
            get => _remotePort;
            set { _remotePort = value; OnPropertyChanged(); }
        }

        // 连接状态
        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConnectButtonText));
            }
        }

        // 连接按钮文本
        public string ConnectButtonText => IsConnected ? "断开" : "连接";

        // 命令
        public ICommand ConnectCommand => new RelayCommand(Connect);
        public ICommand SaveCommand => new RelayCommand(Save);

        // 连接/断开逻辑
        private void Connect()
        {
            IsConnected = !IsConnected;
            MessageBox.Show($"{SelectedProtocol} {ConnectButtonText} 成功");
        }

        // 保存逻辑
        private void Save()
        {
            MessageBox.Show("配置已保存");
        }

        // 属性变更通知
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    // 极简RelayCommand
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged;
    }
}
