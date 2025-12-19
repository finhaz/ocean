using ocean.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ocean.Mvvm
{
    // 全局ViewModel
    public class AppViewModel : ObservableObject
    {
        // *************************
        // 单例核心实现（懒加载+线程安全）
        // *************************
        // 1. 私有静态只读的懒加载实例（Lazy<T> 是.NET内置的线程安全懒加载类）
        private static readonly Lazy<AppViewModel> _lazyInstance = new Lazy<AppViewModel>(() => new AppViewModel());

        // 2. 公共静态属性，提供全局访问点
        public static AppViewModel Instance => _lazyInstance.Value;

        // 3. 私有构造函数，防止外部通过 new 关键字创建实例
        private AppViewModel()
        {
            // 初始化Modbusset实例
            _modbusSet = new Modbusset();
        }

        // *************************
        // 业务属性
        // *************************
        private Modbusset _modbusSet;
        public Modbusset ModbusSet
        {
            get => _modbusSet;
            set => SetProperty(ref _modbusSet, value);
        }

        // 全局串口配置实例（所有页面共享）
        public SerialConfigViewModel SerialConfig { get; } = new SerialConfigViewModel();

        // 可添加其他全局共享属性（如之前的CommonRes、Debugsera等）
        // private CommonRes _ucom;
        // public CommonRes Ucom { get => _ucom; set => SetProperty(ref _ucom, value); }
    }
}
