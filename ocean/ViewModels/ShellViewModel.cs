using MahApps.Metro.IconPacks;
using ocean.Mvvm;
using ocean.UI;
using ocean.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ocean.ViewModels
{
    // 通讯类型枚举（用于MainPage选择）
    public enum CommunicationType
    {
        SerialPort,    // 串口
        Ethernet       // 以太网
    }
    public class ShellViewModel : BindableBase
    {
        public ObservableCollection<MenuItem> Menu { get; } = new();

        public ObservableCollection<MenuItem> OptionsMenu { get; } = new();

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
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.ServerSolid },
                Label = "选择通讯界面",
                NavigationType = typeof(MainPage),
                NavigationDestination = new Uri("Views/MainPage.xaml", UriKind.RelativeOrAbsolute)
            });
            this.Menu.Add(new MenuItem()
            {
                Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.ServerSolid },
                Label = "串口配置界面",
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
        /// 根据选择的通讯类型，更新配置菜单的跳转目标
        /// </summary>
        /// <param name="type">通讯类型（串口/以太网）</param>
        public void UpdateCommunicationMenu(CommunicationType type)
        {
            // 找到"通讯配置"菜单
            var configMenu = Menu.FirstOrDefault(m => m.Label == "通讯配置");
            if (configMenu == null) return;

            switch (type)
            {
                case CommunicationType.SerialPort:
                    configMenu.NavigationDestination = new Uri("Views/SerialConfigPage.xaml", UriKind.RelativeOrAbsolute);
                    configMenu.NavigationType = typeof(SerialConfig);
                    configMenu.Label = "串口配置界面"; // 可选：修改菜单显示名称
                    configMenu.Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.ServerSolid };
                    break;

                case CommunicationType.Ethernet:
                    configMenu.NavigationDestination = new Uri("Views/UserPage.xaml", UriKind.RelativeOrAbsolute);
                    configMenu.NavigationType = typeof(UserPage);
                    configMenu.Label = "以太网配置界面"; // 可选：修改菜单显示名称
                    configMenu.Icon = new PackIconFontAwesome() { Kind = PackIconFontAwesomeKind.SectionSolid};
                    break;
            }
        }

    }
}