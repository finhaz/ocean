using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace ocean.Mvvm
{
    public class ObservableObject : INotifyPropertyChanged
    {
        // 属性变更事件
        public event PropertyChangedEventHandler? PropertyChanged;

        // 触发事件的基础方法
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 通用属性赋值方法（核心：泛型+引用传递，统一处理赋值和通知）
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            // 若值未变化，直接返回false，避免不必要的通知
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            // 赋值并触发通知
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
