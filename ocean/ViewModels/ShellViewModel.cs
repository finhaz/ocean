using MahApps.Metro.IconPacks;
using ocean.Mvvm;
using ocean.UI;
using ocean.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ocean.ViewModels
{
    // 通讯类型枚举（用于MainPage选择）
    public enum CommunicationType
    {
        SerialPort,    // 串口
        Ethernet,       // 网络
        CAN,
        IIC
    }
    public class ShellViewModel : ObservableObject
    {
        public ObservableCollection<MenuItem> Menu { get; } = new();

        public ObservableCollection<MenuItem> OptionsMenu { get; } = new();

        private const string CommunicationMenuLabel = "通讯配置界面";
        // 通讯类型与页面的映射表（扩展时仅需修改这里）
        //TTL转CAN暂时用和串口同一界面，以后考虑加入AT指令
        private readonly Dictionary<CommunicationType, (string Uri, Type PageType)> _commTypeMap = new()
        {
            { CommunicationType.SerialPort, ("Views/SerialConfigPage.xaml", typeof(SerialConfigPage)) },
            { CommunicationType.Ethernet, ("Views/EthernetConfigPage.xaml", typeof(EthernetConfigPage)) },
            { CommunicationType.CAN, ("Views/CANConfigPage.xaml", typeof(CANConfigPage)) }           
        };


        public ShellViewModel()
        {
            BuildMenus();
        }

        private void BuildMenus()
        {
            // Build the menus
            //汉堡菜单上方的按钮
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.NetworkWiredSolid },
                Label = "选择通讯界面",
                NavigationType = typeof(MainPage),
                NavigationDestination = new Uri("Views/MainPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.ServerSolid },
                Label = "通讯配置界面",
                NavigationType = typeof(SerialConfigPage),
                NavigationDestination = new Uri("Views/SerialConfigPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.UserSolid },
                Label = "通用调试界面",
                NavigationType = typeof(GeneralDebugPage),
                NavigationDestination = new Uri("Views/GeneralDebugPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.PowerOffSolid },
                Label = "设备控制界面",
                NavigationType = typeof(DeviceControlPage),
                NavigationDestination = new Uri("Views/DeviceControlPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.CanadianMapleLeafBrands},
                Label = "DBC界面",
                NavigationType = typeof(DbcViewPage),
                NavigationDestination = new Uri("Views/DbcViewPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FileArrowDownSolid },
                Label = "通讯协议说明",
                NavigationType = typeof(Prointroduction),
                NavigationDestination = new Uri("Views/Prointroduction.xaml", UriKind.RelativeOrAbsolute)
            });

            //汉堡菜单下方的按钮
            this.OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.InfoSolid },
                Label = "About",
                NavigationType = typeof(AboutPage),
                NavigationDestination = new Uri("Views/AboutPage.xaml", UriKind.RelativeOrAbsolute)
            });
        }

        /// <summary>
        /// 更新通讯配置菜单Uri（仅改Uri/PageType，不改标签）
        /// </summary>
        public void UpdateCommunicationMenu(CommunicationType type)
        {
            if (!_commTypeMap.ContainsKey(type)) return;

            var configMenu = Menu.FirstOrDefault(m => m.Label == CommunicationMenuLabel);
            if (configMenu == null) return;

            var (newUri, newPageType) = _commTypeMap[type];
            configMenu.NavigationDestination = new Uri(newUri, UriKind.RelativeOrAbsolute);
            configMenu.NavigationType = newPageType;
        }

    }
}