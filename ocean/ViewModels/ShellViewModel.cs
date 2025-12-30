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
        Ethernet,       // 以太网
        TcpClient,
        Udp
    }
    public class ShellViewModel : BindableBase
    {
        public ObservableCollection<MenuItem> Menu { get; } = new();

        public ObservableCollection<MenuItem> OptionsMenu { get; } = new();

        private const string CommunicationMenuLabel = "通讯配置界面";
        // 通讯类型与页面的映射表（扩展时仅需修改这里）
        private readonly Dictionary<CommunicationType, (string Uri, Type PageType)> _commTypeMap = new()
        {
            { CommunicationType.SerialPort, ("Views/SerialConfig.xaml", typeof(SerialConfig)) },
            { CommunicationType.Ethernet, ("Views/UserPage.xaml", typeof(UserPage)) },
            { CommunicationType.TcpClient, ("Views/UserPage.xaml", typeof(UserPage)) },
            { CommunicationType.Udp, ("Views/UserPage.xaml", typeof(UserPage)) }
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
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.SellsyBrands },
                Label = "选择通讯界面",
                NavigationType = typeof(MainPage),
                NavigationDestination = new Uri("Views/MainPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.ServerSolid },
                Label = "通讯配置界面",
                NavigationType = typeof(BugsPage),
                NavigationDestination = new Uri("Views/SerialConfig.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.UserSolid },
                Label = "通用调试界面",
                NavigationType = typeof(UserPage),
                NavigationDestination = new Uri("Views/GeneralDebugPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.UserDoctorSolid },
                Label = "控制界面",
                NavigationType = typeof(DeviceControlPage),
                NavigationDestination = new Uri("Views/DeviceControlPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.FontAwesomeBrands },
                Label = "协议说明",
                NavigationType = typeof(Prointroduction),
                NavigationDestination = new Uri("Views/Prointroduction.xaml", UriKind.RelativeOrAbsolute)
            });

            //汉堡菜单下方的按钮
            this.OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.AnchorSolid },
                Label = "Settings",
                NavigationType = typeof(SettingsPage),
                NavigationDestination = new Uri("Views/SettingsPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.OptionsMenu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.AndroidBrands },
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