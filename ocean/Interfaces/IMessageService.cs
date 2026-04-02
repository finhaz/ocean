using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ocean.Interfaces
{
    public interface IMessageService
    {
        // 一个方法支持所有弹窗格式
        void Show(string message, string title = "提示",
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.Information);
    }
}
