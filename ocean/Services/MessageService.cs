using ocean.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ocean.Services
{
    public class MessageService : IMessageService
    {
        public void Show(string message, string title = "提示",
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.Information)
        {
            // 真正弹窗口（只在这里出现一次）
            MessageBox.Show(message, title, button, icon);
        }
    }
}
